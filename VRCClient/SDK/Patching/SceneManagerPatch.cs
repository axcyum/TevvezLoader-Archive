using HarmonyLib;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRClient.Modules;
using VRClient.SDK.VRCs;

namespace VRClient.SDK.Patching
{
    [HarmonyPatch(typeof(SceneManager))]
    internal class SceneManagerPatch
    {
        [HarmonyPatch("Internal_SceneLoaded")]
        [HarmonyPrefix]
        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            foreach (var module in EventManager.GetModules().Where(m => m != null))
            {
                try { module.OnSceneLoaded(scene.buildIndex, scene.name); }
                catch (Exception ex) { Logging.Log($"Error in {module.GetType().Name}.OnSceneLoaded: {ex}", Logging.LType.Error); }
            }
        }

        [HarmonyPatch("Internal_SceneUnloaded")]
        [HarmonyPrefix]
        private static void OnSceneUnLoaded(Scene scene)
        {
            // Alle Spieler‑Wrapper sauber entfernen
            foreach (var player in Player.players.ToList())
                Player.RemovePlayer(player);

            // NetworkManager‑Cache leeren
            NetworkManagerPatch.ClearCache();

            foreach (var module in EventManager.GetModules().Where(m => m != null))
            {
                try { module.OnSceneUnLoaded(scene.buildIndex, scene.name); }
                catch (Exception ex) { Logging.Log($"Error in {module.GetType().Name}.OnSceneUnLoaded: {ex}", Logging.LType.Error); }
            }
        }
    }
}