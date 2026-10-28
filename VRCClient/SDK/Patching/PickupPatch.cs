using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Wrapper.Modules;
using VRClient.SDK.VRCs;

namespace VRClient.SDK.Patching
{
    [HarmonyPatch]
    public static class PickupsPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(VRC_Pickup), "Awake")]
        public static void pfAwake(VRC_Pickup __instance)
        {
            new Pickup(__instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(VRC_Pickup), "OnDestroy")]
        public static void pfDestroy(VRC_Pickup __instance)
        {
            Pickup.Remove(__instance);
        }
    }
}
