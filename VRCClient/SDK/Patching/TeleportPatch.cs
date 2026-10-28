using HarmonyLib;
using UnityEngine;
using VRC.SDKBase;
using VRClient.Menu;
using VRClient.Modules;

namespace VRClient.SDK.Patching
{
    [HarmonyPatch(typeof(VRCPlayerApi), nameof(VRCPlayerApi.TeleportTo), new Type[] { typeof(Vector3), typeof(Quaternion) })]
    internal class TeleportPatch1
    {
        [HarmonyPrefix]
        public static bool Prefix()
        {
            return !Cfg.AntiTeleport;
        }
    }

    [HarmonyPatch(typeof(VRCPlayer))]
    internal class TeleportPatch2
    {
        [HarmonyPatch("Method_Public_Void_Vector3_Quaternion_0")]
        [HarmonyPrefix]
        public static bool Prefix()
        {
            return !Cfg.AntiTeleport;
        }
    }
}
