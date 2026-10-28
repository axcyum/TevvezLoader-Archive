using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

namespace VRClient.SDK
{
    public static class InputHelper
    {
        // --- SteamVR Actions (null-gesichert) ---
        private static SteamVR_Action_Boolean _jump;
        private static SteamVR_Action_Boolean _openQM;
        private static SteamVR_Action_Boolean _micToggle;
        private static SteamVR_Action_Boolean _grab;
        private static SteamVR_Action_Single _trigger;
        private static SteamVR_Action_Vector2 _moveStick;
        private static SteamVR_Action_Vector2 _rotStick;

        // --- Double‑Tap‑Interna ---
        private static readonly Dictionary<string, float> _lastTapTime = new Dictionary<string, float>();
        private static readonly Dictionary<string, SteamVR_Action_Boolean> _actionCache = new Dictionary<string, SteamVR_Action_Boolean>();
        private static bool _triggerReady = true;

        // --- Öffentliche Zustände (VR + Fallback) ---
        public static bool jump => _jump?.state ?? false || Input.GetKey(KeyCode.Space);
        public static bool openQM => _openQM?.state ?? false || Input.GetKey(KeyCode.Escape);
        public static bool micToggle => _micToggle?.state ?? false || Input.GetKey(KeyCode.V);
        public static bool grab => _grab?.state ?? false || Input.GetKey(KeyCode.Mouse0);

        public static float trigger => _trigger?.axis ?? 0f;
        public static Vector2 moveStick
        {
            get
            {
                if (_moveStick == null) return Vector2.zero;
                return _moveStick.axis;
            }
        }
        public static Vector2 rotStick
        {
            get
            {
                if (_rotStick == null) return Vector2.zero;
                return _rotStick.axis;
            }
        }

        public static Vector2 controller_moveStick => new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        public static Vector2 controller_rotStick => new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        static InputHelper()
        {

        }

        public static void Setup()
        {
            try
            {
                _jump = SteamVR_Input.GetBooleanAction("jump");
                _openQM = SteamVR_Input.GetBooleanAction("Menu");
                _micToggle = SteamVR_Input.GetBooleanAction("Toggle Microphone");
                _grab = SteamVR_Input.GetBooleanAction("Grab");
                _trigger = SteamVR_Input.GetSingleAction("gesture_trigger_axis");
                _moveStick = SteamVR_Input.GetVector2Action("Move");
                _rotStick = SteamVR_Input.GetVector2Action("Rotate");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[InputHelper] SteamVR initialization failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Liefert true, wenn die SteamVR Boolean-Action (aus dem Cache) ein Double‑Tap war.
        /// </summary>
        public static bool HasDoubleClicked(string actionName, float threshold = 0.3f)
        {
            if (!_actionCache.TryGetValue(actionName, out var action))
            {
                action = SteamVR_Input.GetBooleanAction(actionName);
                if (action == null) return false;
                _actionCache[actionName] = action;
            }

            if (!action.stateDown)
                return false;

            return CheckDoubleTap(actionName, threshold);
        }

        /// <summary>
        /// Double‑Tap für den Trigger, mit benutzerdefinierbaren Schwellwerten.
        /// </summary>
        public static bool HasDoubleClickedTrigger(float timeThreshold = 0.3f, float triggerThreshold = 0.2f)
        {
            if (_trigger == null)
                return false;

            float axis = _trigger.axis;
            if (axis <= triggerThreshold)
            {
                _triggerReady = true;
                return false;
            }

            if (!_triggerReady)
                return false;

            _triggerReady = false;
            return CheckDoubleTap("trigger", timeThreshold);
        }

        // --- Interne Hilfsmethode ---
        private static bool CheckDoubleTap(string key, float threshold)
        {
            float prevTime = _lastTapTime.TryGetValue(key, out var value) ? value : 0f;
            float now = Time.time;

            if (now - prevTime <= threshold)
            {
                _lastTapTime[key] = 0f;  // Reset, damit kein endloser Double‑Tap
                return true;
            }

            _lastTapTime[key] = now;
            return false;
        }
    }
}