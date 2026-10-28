using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using VRC.SDKBase;
using VRClient.Modules;
using VRClient.SDK.VRCs;
using static VRClient.Modules.UiModule;

namespace VRClient.SDK.Patching
{
    [HarmonyPatch(typeof(NetworkManager))]
    internal class NetworkManagerPatch
    {
        private static readonly Dictionary<VRC.Player, Player> _playerCache = new();

        public static void ClearCache()
        {
            lock (_playerCache) _playerCache.Clear();
        }

        [HarmonyPatch("Method_Public_Void_Player_PDM_1")]
        [HarmonyPrefix]
        private static void OnPlayerJoin(VRC.Player __0)
        {
            if (__0 == null) return;

            var playerWrapper = new Player(__0);
            lock (_playerCache) _playerCache[__0] = playerWrapper;

            if (__0.prop_VRCPlayerApi_0.isLocal)
                Player.localplayer = playerWrapper;

            Logging.Log("[+] " + playerWrapper.username + $" ({playerWrapper.id})", Logging.LType.Join, true);

            // Namens‑Spoofing (unverändert, aber mit Kommentar)
            bool isLocal = __0.prop_VRCPlayerApi_0.isLocal;
            bool isMaster = __0.prop_VRCPlayerApi_0.isMaster;

            if (isLocal)
            {
                if (Cfg.OwnerSpoof && !Cfg.MasterSpoof)
                {
                    var world = RoomManager.field_Internal_Static_ApiWorld_0;
                    if (world != null)
                    {
                        Player.localplayer?.playerapi.displayName = world.authorName;
                        Networking.LocalPlayer.displayName = world.authorName;
                        Player.localplayer?.apiuser.displayName = world.authorName;
                    }
                }
            }

            foreach (var module in EventManager.GetModules().Where(m => m != null))
            {
                try { module.OnPlayerJoin(__0); }
                catch (Exception ex) { Logging.Log($"Error in {module.GetType().Name}.OnPlayerJoin: {ex}", Logging.LType.Error); }
            }
        }

        [HarmonyPatch("Method_Public_Void_Player_0")]
        [HarmonyPrefix]
        private static void OnPlayerLeave(VRC.Player __0)
        {
            if (__0 == null) return;

            Player leavingPlayer = null;
            lock (_playerCache)
            {
                if (_playerCache.TryGetValue(__0, out leavingPlayer))
                    _playerCache.Remove(__0);
            }

            if (leavingPlayer != null)
            {
                Logging.Log("[-] " + leavingPlayer.username + $" ({leavingPlayer.id})", Logging.LType.Leave, true);
                Player.RemovePlayer(leavingPlayer);
            }

            foreach (var module in EventManager.GetModules().Where(m => m != null))
            {
                try { module.OnPlayerLeft(__0); }
                catch (Exception ex) { Logging.Log($"Error in {module.GetType().Name}.OnPlayerLeft: {ex}", Logging.LType.Error); }
            }
        }
    }
}