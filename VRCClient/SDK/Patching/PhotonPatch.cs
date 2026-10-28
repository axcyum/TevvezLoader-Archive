using HarmonyLib;
using Photon.Client;
using VRClient.SDK.VRCs;

namespace VRClient.SDK.Patching
{
    [HarmonyPatch]
    internal class PhotonPatch
    {
        private static bool Ready;

        internal static int USpeakCopyAc = -1;
        internal static int MovmentCopyAc = -1;

        public static bool wasKickedFromPrevious = false;

        internal static List<string> E1Flagged = new List<string>();
        internal static List<string> flaggedPlayers = new List<string>();
        internal static List<int> E1Blocked = new List<int>();
        internal static List<int> allEventBlock = new List<int>();

        [HarmonyPatch(typeof(LoadBalancingClient), nameof(LoadBalancingClient.Method_Public_Virtual_New_Boolean_Byte_Object_RaiseEventOptions_SendOptions_0))]
        [HarmonyPrefix]
        private static bool Prefix(ref byte param_1, ref Il2CppSystem.Object param_2, ref RaiseEventOptions param_3, ref SendOptions param_4)
        {
            return PhotonHandler.HandleSend(ref param_1, ref param_2, ref param_3, ref param_4) >= 0;
        }
    }
}
