using Cpp2IL.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VRClient.SDK.VRCs
{
    internal static class GObject
    {
        public static GameObject Find(string name) => GameObject.Find(name);

        public static void Destroy(GameObject obj, float t = 0) => GameObject.Destroy(obj, t);

        public static void SetActive(GameObject obj, bool state) => obj?.SetActive(state);

        internal static void DestroyMeLocal(this UnityEngine.Object obj, bool Silent = false)
        {
            if (obj != null)
            {
                string objname = obj.name;
                string typename = obj.GetType().ToString();

                if (obj is GameObject o)
                {
                    if (o != null)
                    {
                        UnityEngine.Object.Destroy(o);
                    }
                }
                else if (obj is Transform item)
                {
                    if (item != null)
                    {
                        UnityEngine.Object.Destroy(item.gameObject);
                    }
                }
                else
                {
                    if (obj != null)
                    {
                        UnityEngine.Object.Destroy(obj);
                    }
                }
            }
        }
    }
}
