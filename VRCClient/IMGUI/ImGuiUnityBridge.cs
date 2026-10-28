using System;
using System.Collections.Concurrent;
using UnityEngine;
using VRClient.Modules;

namespace VRClient.IMGUI
{
    /// <summary>
    /// Zentrale Instanz, um beliebige Aktionen im Unity-Hauptthread auszuführen.
    /// </summary>
    public static class MainThreadDispatcher
    {
        private static readonly ConcurrentQueue<Action> _executionQueue = new ConcurrentQueue<Action>();
        private static bool _initialized = false;
        private static readonly object _initLock = new object();

        private static void EnsureInitialized()
        {
            if (_initialized) return;
            lock (_initLock)
            {
                if (_initialized) return;
                EventManager.OnAnyUpdate += () =>
                {
                    while (_executionQueue.TryDequeue(out var action))
                    {
                        try { action?.Invoke(); }
                        catch (Exception ex) { Debug.LogError($"MainThreadDispatcher error: {ex}"); }
                    }
                };
                _initialized = true;
            }
        }

        public static void Run(Action action)
        {
            if (action == null) return;
            EnsureInitialized();
            _executionQueue.Enqueue(action);
        }

        public static void Run<T>(Action<T> action, T param) => Run(() => action(param));
    }
}