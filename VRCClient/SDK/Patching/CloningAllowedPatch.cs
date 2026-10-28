using HarmonyLib;
using VRC.Core;
using VRC.SDK3.Props;

namespace VRClient.SDK.Patching
{
    [HarmonyPatch(typeof(APIUser))]
    internal class CloningAllowedPatch
    {
        [HarmonyPatch("allowAvatarCopying", MethodType.Setter)]
        [HarmonyPrefix]
        public static void hk(ref bool __0)
        {
            __0 = true;
        }
    }
}