using HarmonyLib;
using System;
using System.Linq;
using VRC.Core;
using VRClient.Menu;
using VRClient.Modules;
using VRClient.SDK.VRCs;

namespace VRClient.SDK.Patching
{
    [HarmonyPatch(typeof(RoomManager))]
    internal class RoomManagerPatch
    {
        [HarmonyPatch("Method_Public_Static_Boolean_ApiWorld_ApiWorldInstance_String_Int32_0")]
        [HarmonyPostfix]
        private static void OnInstanceLoaded(ApiWorld __0, ApiWorldInstance __1)
        {
            if (__0 == null || __1 == null)
                return;

            try
            {
                var newWorld = new World(__1);
                HistoryPersistence.AddOrUpdateHistory(newWorld);
                Logging.Log($"Instance Joined: {newWorld.InstanceId}", Logging.LType.Info);
            }
            catch (Exception ex)
            {
                Logging.Log($"Error adding world to history: {ex}", Logging.LType.Error);
            }

            // Module benachrichtigen – über GetModules() für thread-sichere Enumeration
            foreach (var module in EventManager.GetModules().Where(m => m != null))
            {
                try
                {
                    module.OnInstanceLoaded(__0, __1);
                }
                catch (Exception ex)
                {
                    Logging.Log($"Error in {module.GetType().Name}.OnInstanceLoaded: {ex}", Logging.LType.Error);
                }
            }
        }
    }
}