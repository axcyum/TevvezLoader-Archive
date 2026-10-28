using Il2CppInterop.Runtime.InteropTypes;
using Photon.Client;
using Photon.Pun;
using UnityEngine;
using VRC.Udon;
using VRClient.Menu;

using Il2CppDictionary = Il2CppSystem.Collections.Generic.Dictionary<byte, Il2CppSystem.Object>;

namespace VRClient.SDK.VRCs
{
    internal static class PhotonHandler
    {
        internal static bool UdonBlocker = false;
        internal static bool blockSync = false;
        internal static bool E12Invis = false;
        internal static bool Deserialize = false;
        internal static bool UdonLogger = false;

        private const byte EventUdon = 18;
        private const byte EventCustom11 = 11;

        private enum HandlerResult
        {
            NoAction = 0,
            Continue = 1,
            Error = 2,
            StopProcessing = -1
        }

        private static readonly System.Random _random = new System.Random();

        internal static int HandleSend(ref byte code, ref Il2CppSystem.Object rawObject, ref RaiseEventOptions options, ref SendOptions sendOptions)
        {
            try
            {
                return code switch
                {
                    EventUdon => (int)HandleUdonSend(rawObject),
                    EventCustom11 => (int)HandlerResult.NoAction,
                    _ => (int)HandlerResult.NoAction
                };
            }
            catch (Exception ex)
            {
                Logging.Log(ex.Message, Logging.LType.Photon);
                return (int)HandlerResult.Error;
            }
        }

        private static HandlerResult HandleUdonSend(Il2CppSystem.Object rawObject)
        {
            var dict = ((Il2CppObjectBase)rawObject).Cast<Il2CppDictionary>();
            if (dict == null || !dict.TryGetValue(2, out var viewIdObj) || !dict.TryGetValue(1, out var hashObj))
                return HandlerResult.Error;

            int viewId = viewIdObj.Unbox<int>();
            int hash = hashObj.Unbox<int>();

            PhotonView photonView = PhotonView.Method_Public_Static_PhotonView_Int32_0(viewId);
            if (photonView == null)
                return HandlerResult.Error;

            UdonBehaviour udonBehaviour = photonView.GetComponent<UdonBehaviour>();
            if (udonBehaviour == null)
                return HandlerResult.Error;

            if (!udonBehaviour.TryGetEntrypointNameFromHash((uint)hash, out string eventName))
                return HandlerResult.Error;

            if (UdonLogger)
            {
                Logging.Log($"Sent udon event {eventName} to {udonBehaviour.gameObject.name}", Logging.LType.Photon);
            }

            if (UdonBlocker)
                return HandlerResult.StopProcessing;

            return HandlerResult.Continue;
        }
    }
}