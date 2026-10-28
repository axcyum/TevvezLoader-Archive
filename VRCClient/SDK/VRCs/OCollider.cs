using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VRClient.SDK.VRCs
{
    public static class OCollider
    {
        internal static void RemoveColliders(this GameObject obj, bool Silent = false)
        {
            if (obj != null)
            {
                var colliders = obj.GetComponents<Collider>();

                foreach (var c in colliders)
                {
                    UnityEngine.Object.DestroyImmediate(c);
                }
            }
        }

        internal static void RemoveAllColliders(this GameObject obj, bool Silent = true)
        {
            if (obj != null)
            {
                var colliders = obj.GetComponentsInChildren<Collider>(true);
                if (!Silent)
                {

                }

                foreach (var c in colliders)
                {
                    UnityEngine.Object.DestroyImmediate(c);
                }
            }
        }

        internal static void Disablecollider(this GameObject obj)
        {
            if (obj != null)
            {
                var colliders = obj.GetComponents<Collider>();

                foreach (var c in colliders)
                {
                    c.enabled = false;
                }
            }
        }

        internal static void Set_Colliders_isTrigger(this GameObject obj, bool isTrigger, bool Quiet = true)
        {
            if (obj != null)
            {
                var colliders = obj.GetComponents<Collider>();
                foreach (var c in colliders)
                {
                    c.isTrigger = isTrigger;
                }
            }
        }

        internal static void DisableAllColliders(this GameObject obj)
        {
            if (obj != null)
            {
                var colliders = obj.GetComponentsInChildren<Collider>(true);
                foreach (var c in colliders)
                {
                    c.enabled = false;
                }
            }
        }

        internal static void EnableColliders(this GameObject obj)
        {
            if (obj != null)
            {
                var colliders = obj.GetComponentsInChildren<Collider>(true);
                foreach (var c in colliders)
                {
                    c.enabled = true;
                }

                var meshcolliders = obj.GetComponentsInChildren<MeshCollider>(true);
                foreach (var c in meshcolliders)
                {
                    c.enabled = true;
                    c.convex = true;
                }
            }
        }

        internal static void SetMesh_IsConvex(this GameObject obj, bool isConvex)
        {
            foreach (var col in obj.GetComponents<MeshCollider>())
            {
                col.convex = isConvex;
            }
        }

        internal static void IgnoreLocalPlayerCollision(this Collider obj, bool ignore = true)
        {
            var localcollider = Player.localplayer?.vrcplayer.gameObject.GetComponent<Collider>();
            if (localcollider != null)
            {
                Physics.IgnoreCollision(obj, localcollider, ignore);
            }

        }
        internal static void IgnoreLocalPlayerCollision(this Transform obj, bool ignore = true)
        {
            var localcollider = Player.localplayer?.vrcplayer.gameObject.GetComponent<Collider>();
            if (localcollider != null)
            {

                var colliders = obj.GetComponents<Collider>();
                foreach (var c in colliders)
                {
                    Physics.IgnoreCollision(c, localcollider, ignore);
                }
            }

        }
        internal static void IgnoreObjectCollision(this GameObject obj, Transform item, bool ignore = true)
        {
            var colliders = obj.GetComponents<Collider>();
            var colliders2 = item.GetComponents<Collider>();
            foreach (var c in colliders)
            {
                foreach (var c2 in colliders2)
                {
                    Physics.IgnoreCollision(c, c2, ignore);
                }
            }

        }

        internal static void IgnoreLocalPlayerCollision(this GameObject obj, bool ignore = true, bool IncludeTriggers = true, bool Quiet = true)
        {
            if (obj == null) return;
            var localcollider = Player.localplayer?.vrcplayer.gameObject.GetComponent<Collider>();
            if (localcollider != null)
            {

                var colliders = obj.GetComponents<Collider>();
                foreach (var c in colliders)
                {
                    if (c.isTrigger)
                    {
                        if (IncludeTriggers)
                        {
                            Physics.IgnoreCollision(c, localcollider, ignore);
                        }
                    }
                    else
                    {
                        Physics.IgnoreCollision(c, localcollider, ignore);
                    }
                }
            }

        }
    }
}
