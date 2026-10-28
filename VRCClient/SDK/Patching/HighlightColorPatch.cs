using HarmonyLib;
using UnityEngine;

namespace VRClient.SDK.Patching
{
    internal class HighlightColorPatch
    {
        [HarmonyPatch(typeof(HighlightsFXStandalone), nameof(HighlightsFXStandalone.Awake))]
        [System.Obsolete]
        class HighlightsFXStandalonePatch
        {
            static void Postfix(HighlightsFXStandalone __instance)
            {
                __instance.field_Protected_Color_0 = Color.white;
            }
        }
    }
}
