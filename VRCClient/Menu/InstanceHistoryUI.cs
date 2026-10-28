using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VRC.SDKBase;
using VRClient.IMGUI;
using VRClient.Modules;
using VRClient.SDK;
using VRClient.SDK.VRCs;

namespace VRClient.Menu
{
	internal class InstanceHistoryUI : UiModule.Page
	{
		public InstanceHistoryUI() : base("History") { }

		public override void Render()
		{
			var history = World.InstanceHistory;
			for (int i = history.Count - 1; i >= 0; i--)
			{
				World instance = history[i];
				string timeStr = instance.VisitedAt.ToString("HH:mm dd.MM.");
				string buttonText = $"{instance.WorldName} ({instance.InstanceName}) - {timeStr}";

				// Button mit voller Breite und Höhe 30
				if (GUILayout.Button(buttonText, GUILayout.Height(30), GUILayout.ExpandWidth(true)))
				{
					MainThreadDispatcher.Run(() => Networking.GoToRoom(instance.WorldId + ":" + instance.InstanceId));
				}
			}
		}
	}

	internal static class HistoryPersistence
	{
		private static readonly string HistoryPath = Path.Combine(EntryPoint.PluginPath, "Instance", "instance_history.json");
		private const int MaxHistorySize = 50;

		public static void LoadHistory()
		{
			string directory = Path.GetDirectoryName(HistoryPath);
			if (!string.IsNullOrEmpty(directory))
				Directory.CreateDirectory(directory);

			var history = World.InstanceHistory;
			if (!File.Exists(HistoryPath))
			{
				history.Clear();
				return;
			}

			try
			{
				string json = File.ReadAllText(HistoryPath);
				var loaded = JsonConvert.DeserializeObject<List<World>>(json);
				history.Clear();
				if (loaded != null)
					history.AddRange(loaded);
			}
			catch (Exception e)
			{
				Logging.Log($"Fehler beim Laden der History: {e.Message}", Logging.LType.Error);
				history.Clear();
			}
		}

		public static void AddOrUpdateHistory(World world)
		{
			var history = World.InstanceHistory;
			string instanceId = world.InstanceId;

			int existingIndex = -1;
			for (int i = 0; i < history.Count; i++)
			{
				if (history[i].InstanceId == instanceId)
				{
					existingIndex = i;
					break;
				}
			}

			if (existingIndex >= 0)
				history.RemoveAt(existingIndex);

			history.Add(world);
			SaveHistory();
		}

		public static void SaveHistory()
		{
			try
			{
				var history = World.InstanceHistory;
				while (history.Count > MaxHistorySize)
					history.RemoveAt(0);

				string json = JsonConvert.SerializeObject(history, Formatting.None);
				string directory = Path.GetDirectoryName(HistoryPath);
				if (!string.IsNullOrEmpty(directory))
					Directory.CreateDirectory(directory);

				File.WriteAllText(HistoryPath, json);
			}
			catch (Exception e)
			{
				Logging.Log($"Fehler beim Speichern der History: {e.Message}", Logging.LType.Error);
			}
		}
	}
}