using HarmonyLib;

namespace VRClient.SDK.Patching
{
    [HarmonyPatch(typeof(VRCPlusStatus))]
    internal class VRCPlusStatusPatch
    {
        [HarmonyPatch("prop_ReactiveProperty_1_Boolean_0", MethodType.Getter)]
        [HarmonyPostfix]
        private static void VRCSpoof(ref ReactiveProperty<bool> __result)
        {
            if (__result != null)
            {
                __result.field_Protected_T_0 = true;
            }
        }
    }
}