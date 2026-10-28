using HarmonyLib;
using UnityEngine;
using VRC.SDKBase;

namespace VRClient.SDK.Patching
{
    [HarmonyPatch(typeof(VRCPlayerApi), nameof(VRCPlayerApi.IsOwner))]
    internal class OwnerPatch
    {
        // Wir nutzen Postfix, um das echte Ergebnis zu überschreiben
        [HarmonyPostfix]
        public static void Postfix(VRCPlayerApi __instance, GameObject obj, ref bool __result)
        {
            if (__instance == null || obj == null) return;

            if (__instance.isLocal && (obj.name == "PermissionManager" || obj.name == "PermissionManager (UnityEngine.GameObject)"))
            {
                __result = true;
            }
        }
    }
}