using HarmonyLib;
using UnityEngine;
using VRC.SDKBase;

namespace VRClient.SDK.Patching
{
    [HarmonyPatch(typeof(VRC.SDKBase.Networking), nameof(VRC.SDKBase.Networking.GetOwner))]
    internal class NetworkingOwnerPatch
    {
        [HarmonyPostfix]
        public static void Postfix(GameObject obj, ref VRCPlayerApi __result)
        {
            if (obj != null)
            {
                __result = Networking.LocalPlayer;
            }
        }
    }
}
