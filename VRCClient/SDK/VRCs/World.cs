// =========================================================================
// World.cs
// =========================================================================
using System;
using System.Collections.Generic;
using VRC.Core;

namespace VRClient.SDK.VRCs
{
    internal class World
    {
        public static List<World> InstanceHistory { get; } = new List<World>();
        public static World? CurrentWorld { get; private set; }

        public string WorldName { get; set; } = "";
        public string InstanceName { get; set; } = "";
        public string InstanceId { get; set; } = "";
        public string WorldId { get; set; } = "";
        public DateTime VisitedAt { get; set; }

        public World(ApiWorldInstance instance)
        {
            if (instance == null) return;

            CurrentWorld = this;
            WorldName = instance.world?.name ?? "Unknown World";
            InstanceName = instance.name ?? "Unknown Instance";
            InstanceId = instance.instanceId ?? "0";
            WorldId = instance.worldId ?? "0";
            VisitedAt = DateTime.Now;
        }

        public World() { }
    }
}