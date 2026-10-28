using System;
using UnityEngine;
using VRClient.Modules;
using VRClient.SDK;
using VRClient.SDK.VRCs;

namespace VRClient.Menu
{
	internal class InstanceUI : UiModule.Page
	{
		public InstanceUI() : base("Main") { }

		public override void Render()
		{
			GUILayout.BeginVertical();

			bool val;

			val = Cfg.UdonLogger;
			if (GUILayout.Toggle(val, "Udon Logger") != val)
			{
				Cfg.UdonLogger = !val;
				UpdateWatermark("Udon Logger", Cfg.UdonLogger);
			}

			val = Cfg.UdonBlocker;
			if (GUILayout.Toggle(val, "Udon Blocker") != val)
			{
				Cfg.UdonBlocker = !val;
				UpdateWatermark("Udon Blocker", Cfg.UdonBlocker);
			}

			val = Cfg.blockSync;
			if (GUILayout.Toggle(val, "Udon Sync Blocker") != val)
			{
				Cfg.blockSync = !val;
				UpdateWatermark("Udon Sync Blocker", Cfg.blockSync);
			}

			val = Cfg.OwnerSpoof;
			if (GUILayout.Toggle(val, "World Spoof") != val)
			{
				Cfg.OwnerSpoof = !val;
				UpdateWatermark("World Spoof", Cfg.OwnerSpoof);
			}

			val = Cfg.MasterSpoof;
			if (GUILayout.Toggle(val, "Master Spoof") != val)
			{
				Cfg.MasterSpoof = !val;
				UpdateWatermark("Master Spoof", Cfg.MasterSpoof);
			}

			val = Cfg.EventLog;
			if (GUILayout.Toggle(val, "Event Logger") != val)
			{
				Cfg.EventLog = !val;
				UpdateWatermark("Event Logger", Cfg.EventLog);
			}

			GUILayout.EndVertical();
		}
	}
}