using System;
using UnityEngine;

namespace VRClient.Modules
{
    internal class ThirdPerson
    {
        // ----- Einstellungen (können vor Init angepasst werden) -----
        public static float FOV = 90f;
        public static float NearClipPlane = 0.01f;

        // ----- Tasten (bei Bedarf änderbar) -----
        public static KeyCode ToggleKey1 = KeyCode.Mouse2;   // Umschalten Normal ↔ Behind ↔ InFront
        public static KeyCode ToggleKey2 = KeyCode.F5;   // Umschalten Normal ↔ Behind ↔ InFront
        public static KeyCode EscapeKey = KeyCode.Escape;   // Sofort zurück zu Normal
        public static KeyCode MoveLeftKey = KeyCode.Q;      // Schulter nach links (nur im Behind‑Modus)
        public static KeyCode MoveRightKey = KeyCode.E;     // Schulter nach rechts (nur im Behind‑Modus)

        // ----- Interne Variablen -----
        private static Camera _thirdPersonCamera;
        private static Camera _vrcCamera;
        private static CameraMode _currentMode = CameraMode.Normal;
        private static CameraBehindMode _cameraBehindMode = CameraBehindMode.Center;
        private static bool _initialized = false;

        // ----- Einmalige Initialisierung (rufst du beim Start auf) -----
        public static void Initialize()
        {
            if (_initialized) return;

            // VRC‑Kamera suchen (Pfad ggf. an deine Umgebung anpassen)
            _vrcCamera = GameObject.Find("TrackingVolume/TrackingSteam2(Clone)/SteamCamera/[CameraRig]/Neck/Camera")?.GetComponent<Camera>();
            if (_vrcCamera == null)
            {
                _vrcCamera = Camera.main;
                if (_vrcCamera == null)
                {
                    Debug.LogError("[ThirdPerson] Keine Kamera gefunden!");
                    return;
                }
            }

            // Dritte‑Person‑Kamera erstellen
            _thirdPersonCamera = new GameObject("ThirdPerson Camera").AddComponent<Camera>();
            _thirdPersonCamera.fieldOfView = FOV;
            _thirdPersonCamera.nearClipPlane = NearClipPlane;
            _thirdPersonCamera.enabled = false;
            _thirdPersonCamera.transform.parent = _vrcCamera.transform.parent;

            _initialized = true;
            Debug.Log("[ThirdPerson] Initialisiert. Drücke die mittlere Maustaste zum Umschalten.");
        }

        // ----- Wird jede Frame aufgerufen (z.B. in einer MonoBehaviour‑Update) -----
        public static void Update()
        {
            if (!_initialized || _thirdPersonCamera == null || _vrcCamera == null) return;

            // Mausrad‑Zoom (optional)
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.001f)
            {
                _thirdPersonCamera.transform.position += _thirdPersonCamera.transform.forward * scroll * 2f;
            }

            // Toggle mit mittlerer Maustaste: Normal → Behind → InFront → Normal …
            if (Input.GetKeyDown(ToggleKey1) || Input.GetKeyDown(ToggleKey2))
            {
                _currentMode++;
                if (_currentMode > CameraMode.InFront) _currentMode = CameraMode.Normal;

                if (_currentMode != CameraMode.Normal)
                {
                    RepositionCamera(_currentMode == CameraMode.Behind, _cameraBehindMode);
                    _thirdPersonCamera.enabled = true;
                }
                else
                {
                    _thirdPersonCamera.enabled = false;
                }
            }

            // Escape → sofort Normal
            if (Input.GetKeyDown(EscapeKey) && _currentMode != CameraMode.Normal)
            {
                _currentMode = CameraMode.Normal;
                _thirdPersonCamera.enabled = false;
            }

            // Nur im Behind‑Modus: Schulterwechsel mit Q / E
            if (_currentMode == CameraMode.Behind)
            {
                if (Input.GetKeyDown(MoveLeftKey))
                {
                    _cameraBehindMode--;
                    if (_cameraBehindMode < CameraBehindMode.LeftShoulder)
                        _cameraBehindMode = CameraBehindMode.LeftShoulder;
                    RepositionCamera(true, _cameraBehindMode);
                }
                if (Input.GetKeyDown(MoveRightKey))
                {
                    _cameraBehindMode++;
                    if (_cameraBehindMode > CameraBehindMode.RightShoulder)
                        _cameraBehindMode = CameraBehindMode.RightShoulder;
                    RepositionCamera(true, _cameraBehindMode);
                }
            }
        }

        // ----- Kamera positionieren -----
        private static void RepositionCamera(bool isBehind, CameraBehindMode shoulder = CameraBehindMode.Center)
        {
            var vrc = _vrcCamera.transform;
            var tp = _thirdPersonCamera.transform;
            tp.parent = vrc;
            tp.position = vrc.position + (isBehind ? -vrc.forward : vrc.forward);
            tp.LookAt(vrc);

            if (isBehind)
            {
                if (shoulder == CameraBehindMode.RightShoulder)
                    tp.position += vrc.right * 0.5f;
                else if (shoulder == CameraBehindMode.LeftShoulder)
                    tp.position -= vrc.right * 0.5f;
            }
            tp.position += tp.forward * 0.25f; // Abstand
        }

        // ----- Einstellungen nachträglich anwenden (optional) -----
        public static void ApplySettings()
        {
            if (_thirdPersonCamera != null)
            {
                _thirdPersonCamera.fieldOfView = FOV;
                _thirdPersonCamera.nearClipPlane = NearClipPlane;
            }
        }
    }

    public enum CameraMode
    {
        Normal = 0,
        Behind = 1,
        InFront = 2,
    }

    public enum CameraBehindMode
    {
        LeftShoulder = 0,
        Center = 1,
        RightShoulder = 2
    }
}