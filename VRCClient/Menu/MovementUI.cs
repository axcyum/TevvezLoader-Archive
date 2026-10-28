using System;
using System.Collections.Generic;
using UnityEngine;
using VRC.Core;
using VRC.SDKBase;
using VRClient.IMGUI;
using VRClient.Modules;
using VRClient.SDK;
using VRClient.SDK.VRCs;

namespace VRClient.Menu
{
    internal class MovementUI : UiModule.Page
    {
        public MovementUI() : base("Movement") { }

        public override void Render()
        {
            GUILayout.BeginVertical();

            // Fly
            bool fly = Cfg.Fly;
            if (GUILayout.Toggle(fly, "Fly") != fly)
            {
                Cfg.Fly = !fly;
                MainThreadDispatcher.Run(() => MovementModule.ToggleFly(Cfg.Fly));
            }

            GUILayout.Label($"Fly Basic Speed: {Cfg.FlyBasicSpeed:F1}");
            Cfg.FlyBasicSpeed = GUILayout.HorizontalSlider(Cfg.FlyBasicSpeed, 0.01f, 20f);

            GUILayout.Label($"Fly Shift Speed: {Cfg.FlyShiftSpeed:F1}");
            Cfg.FlyShiftSpeed = GUILayout.HorizontalSlider(Cfg.FlyShiftSpeed, 0.01f, 20f);

            // Click TP
            bool clickTp = Cfg.ClickTP;
            if (GUILayout.Toggle(clickTp, "Click TP") != clickTp)
            {
                Cfg.ClickTP = !clickTp;
                UpdateWatermark("ClickTP", Cfg.ClickTP);
            }

            // Bunny Hop
            bool bunny = Cfg.BunnyHop;
            if (GUILayout.Toggle(bunny, "Bunny Hop") != bunny)
            {
                Cfg.BunnyHop = !bunny;
                UpdateWatermark("BunnyHop", Cfg.BunnyHop);
            }

            // Inf Jump
            bool infJump = Cfg.InfJump;
            if (GUILayout.Toggle(infJump, "Inf Jump") != infJump)
            {
                Cfg.InfJump = !infJump;
                UpdateWatermark("Inf Jump", Cfg.InfJump);
            }

            // Step High
            GUILayout.Label($"Step High: {Cfg.StepValue:F2}");
            Cfg.StepValue = GUILayout.HorizontalSlider(Cfg.StepValue, 1f, 111f);

            // Auto Step
            bool step = Cfg.StepHigh;
            if (GUILayout.Toggle(step, "Auto Step") != step)
            {
                Cfg.StepHigh = !step;
                if (MovementModule.origionalheight == 0f)
                    MovementModule.origionalheight = Player.localplayer.playerObject.gameObject.GetComponent<VRCPlayerStepHeight>().maxStepHeight;
                if (!Cfg.StepHigh)
                    Player.localplayer.playerObject.gameObject.GetComponent<VRCPlayerStepHeight>().maxStepHeight = MovementModule.origionalheight;
                UpdateWatermark("Auto Step", Cfg.StepHigh);
            }

            // Speed Hack
            GUILayout.Label($"Set Speed: {Cfg.SpeedHack:F1}");
            float newSpeed = GUILayout.HorizontalSlider(Cfg.SpeedHack, 0f, 20f);
            if (Mathf.Abs(newSpeed - Cfg.SpeedHack) > 0.001f)
            {
                Cfg.SpeedHack = newSpeed;
                var playerApi = Player.localplayer?.playerapi;
                if (playerApi != null)
                {
                    playerApi.SetWalkSpeed(Cfg.SpeedHack);
                    playerApi.SetRunSpeed(Cfg.SpeedHack * 2.5f);
                    playerApi.SetStrafeSpeed(Cfg.SpeedHack);
                }
            }

            // Teleport Random
            GUILayout.Label($"E12 Random Teleport: {Cfg.TeleportRandom:F1}");
            Cfg.TeleportRandom = GUILayout.HorizontalSlider(Cfg.TeleportRandom, 1000f, 50000f);

            GUILayout.EndVertical();
        }
    }

    internal class MovementModule : BaseModule
    {
        private static Vector3 originalGravity;
        private float lastTeleportTime = 0f;
        private readonly float teleportCooldown = 0.2f;
        public static float origionalheight;

        private static readonly List<GameObject> AllClones = new();

        private Camera _cachedCamera;
        private float _lastCameraCheck;
        private float lastGroundedSet = 0f;
        private bool didMidJump = false;
        private float lastJumpPress = 0f;

        public override void OnUpdate()
        {
            var local = Player.localplayer;
            if (local?.playerapi == null) return;
            var api = local.playerapi;

            // Controller-Erkennung
            bool controllerConnected = Input.GetJoystickNames().Length > 0 &&
                                       !string.IsNullOrEmpty(Input.GetJoystickNames()[0]);
            if (Player.localplayer != null)
                Player.localplayer.iscontroller = controllerConnected;

            if (Cfg.Fly)
            {
                if (Time.time - lastGroundedSet >= 0.1f)
                {
                    var animator = local.playerObject.GetComponentInChildren<Animator>();
                    if (animator != null)
                        animator.SetBool("Grounded", true);
                    lastGroundedSet = Time.time;
                }

                UpdateFly(local, api);
            }

            if (Cfg.BunnyHop && api.IsPlayerGrounded() && IsJumping())
            {
                var vel = api.GetVelocity();
                vel.y = api.GetJumpImpulse();
                api.SetVelocity(vel);
            }
            if (Cfg.InfJump && !api.IsPlayerGrounded())
            {
                if (IsJumping())
                {
                    if (!didMidJump)
                    {
                        var vel = api.GetVelocity();
                        vel.y = api.GetJumpImpulse();
                        api.SetVelocity(vel);
                        didMidJump = true;
                    }
                }
                else
                {
                    didMidJump = false;
                }
            }

            if (Cfg.StepHigh)
            {
                Player.localplayer.playerObject.gameObject.GetComponent<VRCPlayerStepHeight>().maxStepHeight = Cfg.StepValue;
            }

            if (!local.iscontroller && !local.isvr)
            {
                if (Input.GetKey(KeyCode.LeftControl))
                {
                    if (Input.GetKeyDown(KeyCode.F))
                    {
                        Cfg.Fly = !Cfg.Fly;
                        ToggleFly(Cfg.Fly);
                    }
                    else if (Input.GetKeyDown(KeyCode.G))
                    {
                        Cfg.GhostMode = !Cfg.GhostMode;
                        ToggleGhostMode(Cfg.GhostMode);
                    }
                    else if (Input.GetKeyDown(KeyCode.I))
                    {
                        Cfg.Invisibility = !Cfg.Invisibility;
                        ToggleInvisibility(Cfg.Invisibility);
                    }
                }

                if (Cfg.ClickTP && Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Mouse0))
                {
                    if (Time.time - lastTeleportTime >= teleportCooldown)
                    {
                        Camera cam = GetCamera();
                        if (cam != null && Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit))
                        {
                            Vector3 pos = hit.point;
                            pos.y += 0.05f;

                            if (local.playerObject != null)
                            {
                                Rigidbody rb = local.playerObject.GetComponent<Rigidbody>();

                                if (rb != null)
                                    rb.MovePosition(pos);
                                else
                                    local.playerObject.transform.position = pos;

                                if (rb != null && !rb.isKinematic)
                                {
                                    rb.velocity = Vector3.zero;
                                    rb.angularVelocity = Vector3.zero;
                                }

                                lastTeleportTime = Time.time;
                            }
                        }
                    }
                }
            }

            if (local.iscontroller)
            {
                if (Input.GetKeyDown(KeyCode.JoystickButton1))
                {
                    if (Time.time - lastJumpPress < 0.2f)
                    {
                        Cfg.Fly = !Cfg.Fly;
                        ToggleFly(Cfg.Fly);
                    }
                    lastJumpPress = Time.time;
                }

                if (Input.GetKey(KeyCode.JoystickButton4) && Input.GetKeyDown(KeyCode.JoystickButton0))
                {
                    if (Time.time - lastTeleportTime >= teleportCooldown)
                    {
                        var cam = GetCamera();
                        if (cam != null && Physics.Raycast(cam.transform.position, cam.transform.forward, out var hit))
                        {
                            if (local.playerObject != null)
                                local.playerObject.transform.position = hit.point;
                            lastTeleportTime = Time.time;
                        }
                    }
                }
            }

            // VR‑Unterstützung
            if (local.isvr)
            {
                if (InputHelper.HasDoubleClicked("jump", 0.2f))
                {
                    Cfg.Fly = !Cfg.Fly;
                    ToggleFly(Cfg.Fly);
                }
                if (InputHelper.HasDoubleClickedTrigger(0.3f, 0.5f))
                {
                    if (InputHelper.rotStick.y < -0.5f)
                    {
                        Cfg.GhostMode = !Cfg.GhostMode;
                        ToggleGhostMode(Cfg.GhostMode);
                    }
                    else if (InputHelper.rotStick.y > 0.5f)
                    {
                        Cfg.Invisibility = !Cfg.Invisibility;
                        ToggleInvisibility(Cfg.Invisibility);
                    }
                }
            }
        }

        private Camera GetCamera()
        {
            if (_cachedCamera == null || Time.time - _lastCameraCheck > 0.5f)
            {
                _cachedCamera = Camera.main;
                _lastCameraCheck = Time.time;
            }
            return _cachedCamera;
        }

        public static void ToggleFly(bool enable)
        {
            var local = Player.localplayer;
            if (local?.playerapi == null || local.playerObject == null) return;

            var controller = local.playerObject.GetComponent<CharacterController>();
            if (controller != null)
                controller.enabled = !enable;

            if (enable)
            {
                originalGravity = Physics.gravity;
                Physics.gravity = Vector3.zero;

                UpdateWatermark("Fly", true, true);
            }
            else
            {
                Physics.gravity = originalGravity;
                UpdateWatermark("Fly", false, true);
            }
        }

        public static void ToggleGhostMode(bool toggle)
        {
            PhotonHandler.Deserialize = toggle;

            if (toggle)
            {
                ClonePlayer(Player.localplayer?.vrcplayer);
                if (UIUtils.GhostModeHud?.gameObject != null)
                    UIUtils.GhostModeHud.gameObject.SetActive(true);
                UpdateWatermark("Ghost Mode", true, true);
                PopupUtils.HudMessage("Ghost Mode", "On", 2);
            }
            else
            {
                CloneDestroy();
                if (UIUtils.GhostModeHud?.gameObject != null)
                    UIUtils.GhostModeHud.gameObject.SetActive(false);
                UpdateWatermark("Ghost Mode", false, true);
                PopupUtils.HudMessage("Ghost Mode", "Off", 2);
            }
        }

        public static void ToggleInvisibility(bool toggle)
        {
            PhotonHandler.E12Invis = toggle;

            if (toggle)
            {
                if (UIUtils.GhostModeHud?.gameObject != null)
                    UIUtils.GhostModeHud.gameObject.SetActive(true);
                UpdateWatermark("Invisibility", true, true);
                PopupUtils.HudMessage("Invisibility", "On", 2);
            }
            else
            {
                if (UIUtils.GhostModeHud?.gameObject != null)
                    UIUtils.GhostModeHud.gameObject.SetActive(false);
                UpdateWatermark("Invisibility", false, true);
                PopupUtils.HudMessage("Invisibility", "Off", 2);
            }
        }

        private static void ClonePlayer(VRCPlayer player)
        {
            if (player?.field_Private_VRCAvatarManager_0?.field_Private_GameObject_0 == null) return;
            GameObject clone = UnityEngine.Object.Instantiate(player.field_Private_VRCAvatarManager_0.field_Private_GameObject_0);
            if (clone == null) return;

            var anim = clone.GetComponent<Animator>();
            if (anim != null && anim.isHuman)
            {
                var head = anim.GetBoneTransform(HumanBodyBones.Head);
                if (head != null) head.localScale = Vector3.one;
                anim.enabled = false;
            }

            clone.name = $"{player.prop_Player_0?.field_Private_APIUser_0?.displayName ?? "Unknown"}'s Clone";
            clone.transform.position = player.transform.position;
            clone.transform.rotation = player.transform.rotation;
            AllClones.Add(clone);
        }

        private static void CloneDestroy()
        {
            foreach (var clone in AllClones)
                if (clone != null) UnityEngine.Object.Destroy(clone);
            AllClones.Clear();
        }

        private void UpdateFly(Player local, VRCPlayerApi api)
        {
            float speed = Cfg.FlyBasicSpeed;
            if (Input.GetKey(KeyCode.LeftShift))
                speed += Cfg.FlyShiftSpeed;

            var cam = GetCamera();
            if (cam == null) return;
            Transform camT = cam.transform;

            Vector3 move = Vector3.zero;

            if (!local.iscontroller && !local.isvr)
            {
                if (Input.GetKey(KeyCode.W)) move += camT.forward;
                if (Input.GetKey(KeyCode.S)) move -= camT.forward;
                if (Input.GetKey(KeyCode.A)) move -= camT.right;
                if (Input.GetKey(KeyCode.D)) move += camT.right;
                if (Input.GetKey(KeyCode.E)) move += camT.up;
                if (Input.GetKey(KeyCode.Q)) move -= camT.up;
            }

            if (local.isvr)
            {
                if (InputHelper.rotStick.y != 0f)
                    api.gameObject.transform.position += api.gameObject.transform.up * (speed * InputHelper.rotStick.y);
                if (InputHelper.moveStick.y != 0f)
                    api.gameObject.transform.position += camT.forward * (speed * InputHelper.moveStick.y);
                if (InputHelper.moveStick.x != 0f)
                    api.gameObject.transform.position += camT.right * (speed * InputHelper.moveStick.x);
            }
            if (local.iscontroller)
            {
                if (InputHelper.controller_rotStick.y != 0f)
                    api.gameObject.transform.position += api.gameObject.transform.up * (speed * InputHelper.controller_rotStick.y);
                if (InputHelper.controller_moveStick.y != 0f)
                    api.gameObject.transform.position += camT.forward * (speed * InputHelper.controller_moveStick.y);
                if (InputHelper.controller_moveStick.x != 0f)
                    api.gameObject.transform.position += camT.right * (speed * InputHelper.controller_moveStick.x);
            }

            var cc = local.playerObject.GetComponent<CharacterController>();
            if (cc != null && cc.enabled)
                cc.Move(move.normalized * speed);
            else
                local.playerObject.transform.position += move.normalized * speed;

            api.SetVelocity(Vector3.zero);
        }

        private static bool IsJumping()
        {
            var jumpInput = VRCInputManager.Method_Public_Static_VRCInput_String_0("Jump");
            return jumpInput != null && jumpInput.field_Public_Single_0 != 0;
        }

        public override void OnSceneLoaded(int scene, string name)
        {
            if (scene == -1)
            {
                if (Cfg.Fly)
                {
                    ToggleFly(false);
                    Cfg.Fly = false;
                    UpdateWatermark("Fly", false, false);
                }
            }
        }
    }
}