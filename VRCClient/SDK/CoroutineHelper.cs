using BepInEx.Unity.IL2CPP.Utils.Collections;
using Il2CppInterop.Runtime.Injection;
using System.Collections;
using UnityEngine;

namespace VRClient.SDK
{
    internal class CoroutineHelper
    {
        private static CoroutineRunner _runner;

        private static void EnsureRunner()
        {
            if (_runner != null) return;

            // Typ nur registrieren, wenn noch nicht geschehen
            if (!ClassInjector.IsTypeRegisteredInIl2Cpp<CoroutineRunner>())
                ClassInjector.RegisterTypeInIl2Cpp<CoroutineRunner>();

            var go = new GameObject("CoroutineHelper");
            UnityEngine.Object.DontDestroyOnLoad(go);
            go.hideFlags = HideFlags.HideAndDontSave;
            _runner = go.AddComponent<CoroutineRunner>();
        }

        public static Coroutine Start(IEnumerator routine)
        {
            if (routine == null) return null;
            EnsureRunner();
            return _runner.StartCoroutine(routine.WrapToIl2Cpp());
        }

        public static void Stop(Coroutine coroutine)
        {
            if (_runner != null && coroutine != null)
                _runner.StopCoroutine(coroutine);
        }

        internal static void WaitForObj(string obj, Action action)
        {
            Start(WaitForObjFunc(obj, action));
        }

        internal static void Delay(float del, Action action)
        {
            Start(DelayFunc(del, action));
        }

        private static IEnumerator DelayFunc(float del, Action action)
        {
            yield return new WaitForSeconds(del);
            action.Invoke();
            yield break;
        }

        private static IEnumerator WaitForObjFunc(string obj, Action action)
        {
            while (GameObject.Find(obj) == null)
            {
                yield return new WaitForFixedUpdate();
            }
            yield return new WaitForFixedUpdate();
            action.Invoke();
            yield break;
        }
    }

    // Einfacher MonoBehaviour – kein BaseModule nötig
    internal class CoroutineRunner : MonoBehaviour { }
}