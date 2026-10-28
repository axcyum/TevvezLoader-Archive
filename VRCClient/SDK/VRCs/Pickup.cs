using System.Collections.Generic;
using VRC.SDKBase;

namespace VRClient.SDK.VRCs
{
    internal class Pickup
    {
        // Dictionary mit Lock für Threadsicherheit (optional, aber sicher)
        private static readonly object _lock = new object();
        internal static Dictionary<VRC_Pickup, Pickup> pickups = new Dictionary<VRC_Pickup, Pickup>();

        public Pickup(VRC_Pickup vrcPickup)
        {
            // Threadsicher zum Dictionary hinzufügen
            lock (_lock)
            {
                pickups[vrcPickup] = this;
            }

            // Modifikationen am Original (ESP / Cheat-Funktionalität)
            vrcPickup.allowManipulationWhenEquipped = true;
            vrcPickup.pickupable = true;
            vrcPickup.proximity = 9999999f;
            vrcPickup.DisallowTheft = false;
        }

        // Hilfsmethode zum sicheren Entfernen aus dem Dictionary
        public static bool Remove(VRC_Pickup key)
        {
            lock (_lock)
            {
                return pickups.Remove(key);
            }
        }
    }
}