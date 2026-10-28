using System;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using UnityEngine;

namespace VRClient.Modules
{
	public class ConfigStaticConverter : JsonConverter<Cfg>
	{
		public override Cfg Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			using var doc = JsonDocument.ParseValue(ref reader);
			var root = doc.RootElement;
			var fields = typeof(Cfg).GetFields(BindingFlags.Public | BindingFlags.Static);

			foreach (var field in fields)
			{
				var jsonName = field.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? field.Name;
				if (root.TryGetProperty(jsonName, out var value))
				{
					object? converted = JsonSerializer.Deserialize(value.GetRawText(), field.FieldType, options);
					field.SetValue(null, converted);
				}
			}
			return new Cfg();
		}

		public override void Write(Utf8JsonWriter writer, Cfg value, JsonSerializerOptions options)
		{
			writer.WriteStartObject();
			var fields = typeof(Cfg).GetFields(BindingFlags.Public | BindingFlags.Static);
			foreach (var field in fields)
			{
				var jsonName = field.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? field.Name;
				object? fieldValue = field.GetValue(null);
				writer.WritePropertyName(jsonName);
				JsonSerializer.Serialize(writer, fieldValue, field.FieldType, options);
			}
			writer.WriteEndObject();
		}
	}

	internal class ConfigModule : UiModule.Page
	{
		private static string ConfigPath = Path.Combine(EntryPoint.PluginPath, "Settings.json");
		private static JsonSerializerOptions JsonOptions = new JsonSerializerOptions
		{
			WriteIndented = true,
			IncludeFields = true
		};

		public ConfigModule() : base("Config") { }

		public override void Render()
		{
			// Verwende GUILayout für einfaches Layout
			GUILayout.BeginVertical();

			GUILayout.Label("Color UI", GUI.skin.label);
			GUILayout.Space(5);

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Save Config", GUILayout.Width(120), GUILayout.Height(25)))
				Save();
			if (GUILayout.Button("Delete Config", GUILayout.Width(120), GUILayout.Height(25)))
			{
				if (File.Exists(ConfigPath))
					File.Delete(ConfigPath);
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(10);
			bool Hud = Cfg.Hud;
			if (GUILayout.Toggle(Hud, "Hud Toggle") != Hud)
			{
				Cfg.Hud = !Hud;
			}

			GUILayout.Space(15);
			GUILayout.Label("Hotkey", GUI.skin.label);

			GUILayout.EndVertical();
		}

		public static void Save()
		{
			try
			{
				var dto = new ConfigDto
				{
					VRChat = Config.VRChat
				};

				string json = JsonSerializer.Serialize(dto, JsonOptions);
				File.WriteAllText(ConfigPath, json);
			}
			catch { /* Logging */ }
		}

		public static void Load()
		{
			try
			{
				if (!File.Exists(ConfigPath)) return;
				string json = File.ReadAllText(ConfigPath);
				var dto = JsonSerializer.Deserialize<ConfigDto>(json, JsonOptions);
				if (dto != null)
				{
					Config.VRChat = dto.VRChat;
				}
			}
			catch { /* Logging */ }
		}
	}
}