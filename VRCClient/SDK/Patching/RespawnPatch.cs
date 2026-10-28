using HarmonyLib;
using VRC.SDKBase;
using VRClient.Menu;
using VRClient.Modules;

namespace VRClient.SDK.Patching
{
    [HarmonyPatch(typeof(VRCPlayerApi), nameof(VRCPlayerApi.Respawn))]
    internal class RespawnPatch
    {
        [HarmonyPatch(new Type[] { })]
        [HarmonyPrefix]
        public static bool Prefix()
        {
            return Cfg.AntiRespawn;
        }
    }
}