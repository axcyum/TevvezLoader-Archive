using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using VRC.Core;
using VRC.SDKBase;
using VRClient.IMGUI;
using VRClient.Modules;
using VRClient.SDK;
using VRClient.SDK.Patching;
using VRClient.SDK.VRCs;

namespace VRClient.Menu
{
    // =========================================================================
    // PlayerListUI – reine Darstellung
    // =========================================================================
    internal class PlayerListUI : UiModule.Page
    {
        public PlayerListUI() : base("PlayerList") { }

        // -----------------------------------------------------------------
        // Datenmodell (unverändert, aber jetzt ohne unnötige statische Felder)
        // -----------------------------------------------------------------
        public class PlayerDisplayData
        {
            public string Id;
            public string Username;
            public string Note;          // aktuell aus PlayerListModule._playerNotes
            public bool IsFriend;
            public (string text, Color color) Platform;
            public List<(string text, Color color)> Tags;
            public Color NameColor;
            public ApiAvatar avatar;
        }

        // -----------------------------------------------------------------
        // UI‑Zustand (Instanzvariablen, da Page Singleton)
        // -----------------------------------------------------------------
        private static string _expandedPlayerId;

        private static Vector2 _playerListScrollPos = Vector2.zero;

        // Filter‑Cache
        private static List<PlayerDisplayData> _filteredPlayers = new List<PlayerDisplayData>(200);
        private static int _lastCacheVersion = -1;

        // Body‑Part‑Dropdown
        private static bool _showBodyPartSelection;
        private static string _selectionPlayerId;

        // -----------------------------------------------------------------
        // Render
        // -----------------------------------------------------------------
        public override void Render()
        {
            // Cache‑Snapshot (direkt aus dem Modul, kein redundanter Local‑Cache)
            var cache = PlayerListModule._cachedPlayerData;
            int currentVersion = PlayerListModule._cacheVersion;

            if (_lastCacheVersion != currentVersion)
            {
                _filteredPlayers.Clear();
                lock (PlayerListModule._dataLock)
                    _filteredPlayers.AddRange(cache.Values);
                _filteredPlayers.Sort((a, b) =>
                    string.Compare(a.Username, b.Username, StringComparison.OrdinalIgnoreCase));
                _lastCacheVersion = currentVersion;
            }

            _playerListScrollPos = GUILayout.BeginScrollView(_playerListScrollPos, GUILayout.ExpandHeight(true));

            // Kopfzeile
            GUILayout.BeginHorizontal();
            GUILayout.Label("Player", GUILayout.ExpandWidth(true));
            GUILayout.Label("Actions", GUILayout.Width(60));
            GUILayout.EndHorizontal();
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));

            foreach (var data in _filteredPlayers)
            {
                string id = data.Id;

                // Body‑Part / Offset direkt aus den zentralen Dictionaries des Moduls
                string bodyPart = PlayerListModule.GetBodyPartForPlayer(id);
                Vector3 offset = PlayerListModule.GetOffsetForPlayer(id);

                GUILayout.BeginHorizontal();

                // Linke Spalte: Name + Tags
                GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
                GUILayout.BeginHorizontal();
                foreach (var (text, color) in data.Tags)
                {
                    GUI.color = color;
                    GUILayout.Label($"[{text}] ", GUILayout.ExpandWidth(false));
                }
                GUI.color = Color.white;
                GUI.color = data.NameColor;
                GUILayout.Label(data.Username, GUILayout.ExpandWidth(false));
                GUI.color = Color.white;
                if (!string.IsNullOrEmpty(data.Note))
                {
                    GUI.color = new Color(0.6f, 0.6f, 0.6f);
                    GUILayout.Label($" ({data.Note})", GUILayout.ExpandWidth(false));
                    GUI.color = Color.white;
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();

                // Rechte Spalte: Aufklapp‑Button
                bool isExpanded = (_expandedPlayerId == id);
                if (GUILayout.Button(isExpanded ? "▼" : "►", GUILayout.Width(30)))
                    _expandedPlayerId = isExpanded ? null : id;

                GUILayout.EndHorizontal();

                // Erweiterter Bereich
                if (isExpanded)
                {
                    GUILayout.BeginVertical(GUI.skin.box);
                    RenderPopupContent(id, data, bodyPart, offset);
                    GUILayout.EndVertical();
                }
            }

            GUILayout.EndScrollView();
        }

        private void RenderPopupContent(string playerId, PlayerDisplayData data, string currentBodyPart, Vector3 currentOffset)
        {
            // Aktionen
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Teleport", GUILayout.Width(80)))
                MainThreadDispatcher.Run(() => PlayerListModule.TeleportToPlayer(playerId));
            if (GUILayout.Button("Copy Name", GUILayout.Width(80)))
                MainThreadDispatcher.Run(() => GUIUtility.systemCopyBuffer = data.Username);
            if (GUILayout.Button("Copy ID", GUILayout.Width(80)))
                MainThreadDispatcher.Run(() => GUIUtility.systemCopyBuffer = playerId);
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            // Sit‑Optionen
            GUILayout.Label("Sit Options", GUILayout.ExpandWidth(false));

            // Body‑Part Auswahl (Dropdown‑Simulation)
            string[] allParts = PlayerListModule.GetAllBodyParts();
            int partIdx = Array.IndexOf(allParts, currentBodyPart);
            if (partIdx < 0) partIdx = 0;

            GUILayout.BeginHorizontal();
            GUILayout.Label("Body Part", GUILayout.Width(70));
            GUILayout.Label(currentBodyPart, GUILayout.Width(100));
            if (GUILayout.Button("Change", GUILayout.Width(60)))
            {
                _showBodyPartSelection = !_showBodyPartSelection;
                _selectionPlayerId = playerId;
            }
            GUILayout.EndHorizontal();

            if (_showBodyPartSelection && _selectionPlayerId == playerId)
            {
                GUILayout.BeginVertical(GUI.skin.box);
                foreach (string part in allParts)
                {
                    if (GUILayout.Button(part))
                    {
                        PlayerListModule.SetBodyPartForPlayer(playerId, part);
                        _showBodyPartSelection = false;
                    }
                }
                GUILayout.EndVertical();
            }

            // Offset‑Slider (XY‑Layout aus Platzgründen getrennt)
            Vector3 offset = currentOffset;
            bool offsetChanged = false;

            GUILayout.BeginHorizontal();
            GUILayout.Label("Offset", GUILayout.Width(50));
            // X
            float newX = GUILayout.HorizontalSlider(offset.x, -3f, 3f, GUILayout.Width(80));
            if (Math.Abs(newX - offset.x) > 0.001f) { offset.x = newX; offsetChanged = true; }
            GUILayout.Label($"X:{offset.x:F2}", GUILayout.Width(50));
            // Y
            float newY = GUILayout.HorizontalSlider(offset.y, -3f, 3f, GUILayout.Width(80));
            if (Math.Abs(newY - offset.y) > 0.001f) { offset.y = newY; offsetChanged = true; }
            GUILayout.Label($"Y:{offset.y:F2}", GUILayout.Width(50));
            // Z
            float newZ = GUILayout.HorizontalSlider(offset.z, -3f, 3f, GUILayout.Width(80));
            if (Math.Abs(newZ - offset.z) > 0.001f) { offset.z = newZ; offsetChanged = true; }
            GUILayout.Label($"Z:{offset.z:F2}", GUILayout.Width(50));
            GUILayout.EndHorizontal();

            if (offsetChanged)
                PlayerListModule.SetOffsetForPlayer(playerId, offset);

            if (GUILayout.Button("Apply Sit", GUILayout.Width(100)))
                MainThreadDispatcher.Run(() => PlayerListModule.ToggleSitOnPlayer(playerId));
        }
    }

    // =========================================================================
    // PlayerListModule – Backend mit Persistenz und aufgeräumten Zuständen
    // =========================================================================
    internal class PlayerListModule : BaseModule
    {
        public static readonly object _dataLock = new object();
        public static Dictionary<string, PlayerListUI.PlayerDisplayData> _cachedPlayerData = new();
        public static int _cacheVersion;

        private static Dictionary<string, string> _playerBodyPartSelection = new();
        private static Dictionary<string, Vector3> _playerOffsets = new();
        private static Dictionary<string, string> _playerNotes = new();

        private static string _notesFilePath;
        private static bool _notesLoaded;

        private static string[] _allBodyParts;
        private static Dictionary<string, HumanBodyBones> _bodyPartToBone;

        private static Coroutine _currentSitCoroutine;
        private static bool _isSitting;

        private static bool _isUpdating;
        private static float _lastCacheUpdate;
        private const float CacheUpdateInterval = 0.5f;

        // -----------------------------------------------------------------
        // Initialisierung
        // -----------------------------------------------------------------
        static PlayerListModule()
        {
            try
            {
                var bones = Enum.GetValues(typeof(HumanBodyBones))
                    .Cast<HumanBodyBones>()
                    .Where(b => b != HumanBodyBones.LastBone)
                    .ToArray();
                _bodyPartToBone = new Dictionary<string, HumanBodyBones>();
                var readableNames = new List<string>();
                foreach (var bone in bones)
                {
                    string name = bone.ToString();
                    string readable = Regex.Replace(name, "(?<=[a-z])([A-Z])", " $1");
                    readableNames.Add(readable);
                    _bodyPartToBone[readable] = bone;
                }
                _allBodyParts = readableNames.ToArray();
            }
            catch
            {
                _allBodyParts = Array.Empty<string>();
                _bodyPartToBone = new Dictionary<string, HumanBodyBones>();
            }
        }

        public override void Awake()
        {
            base.Awake();
            _notesFilePath = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".",
                "PlayerNotes.json");
            CoroutineHelper.Start(CleanupCoroutine());
            CoroutineHelper.Start(DelayedCacheStart());
        }

        public override void OnUpdate()
        {
            if (!_isUpdating && Time.realtimeSinceStartup - _lastCacheUpdate >= CacheUpdateInterval)
            {
                _isUpdating = true;
                _lastCacheUpdate = Time.realtimeSinceStartup;
                CoroutineHelper.Start(UpdateCacheCoroutine());
            }
        }

        [Serializable]
        private class SerializableNotes
        {
            public Dictionary<string, string> entries = new();
        }

        // -----------------------------------------------------------------
        // Cache‑Koroutinen
        // -----------------------------------------------------------------
        private IEnumerator DelayedCacheStart()
        {
            while (Player.localplayer?.vrcplayer == null)
                yield return new WaitForSeconds(0.5f);
            yield return new WaitForSeconds(1f);
            CoroutineHelper.Start(UpdateCacheCoroutine());
        }

        private IEnumerator UpdateCacheCoroutine()
        {
            var local = Player.localplayer;
            if (local?.vrcplayer == null) { _isUpdating = false; yield break; }

            List<Player> snapshot;
            lock (Player.players) snapshot = new List<Player>(Player.players);

            bool changed = false;

            foreach (var p in snapshot)
            {
                if (p?.id == null) continue;
                if (!_cachedPlayerData.TryGetValue(p.id, out var data))
                {
                    data = CreateDisplayData(p);
                    _cachedPlayerData[p.id] = data;
                    changed = true;
                }
                else if (data.Username != p.username || data.IsFriend != p.isfriend)
                {
                    UpdateDisplayData(data, p);
                    changed = true;
                }

                // Note immer aktuell halten
                string note = _playerNotes.GetValueOrDefault(p.id, "");
                if (data.Note != note) { data.Note = note; changed = true; }
            }

            // Entfernen
            var activeIds = new HashSet<string>(snapshot.Select(p => p.id));
            var toRemove = _cachedPlayerData.Keys.Where(id => !activeIds.Contains(id)).ToList();
            foreach (var id in toRemove) _cachedPlayerData.Remove(id);
            if (toRemove.Count > 0) changed = true;

            if (changed) _cacheVersion++;

            _isUpdating = false;
        }

        private IEnumerator CleanupCoroutine()
        {
            var wait = new WaitForSeconds(5f);
            while (true)
            {
                yield return wait;
                try
                {
                    HashSet<string> activeIds;
                    lock (Player.players)
                        activeIds = new HashSet<string>(Player.players.Where(p => p?.id != null).Select(p => p.id));

                    foreach (var id in _playerBodyPartSelection.Keys.Where(id => !activeIds.Contains(id)).ToList())
                        _playerBodyPartSelection.Remove(id);
                    foreach (var id in _playerOffsets.Keys.Where(id => !activeIds.Contains(id)).ToList())
                        _playerOffsets.Remove(id);
                }
                catch { }
            }
        }

        // -----------------------------------------------------------------
        // Datenaufbereitung
        // -----------------------------------------------------------------
        private static PlayerListUI.PlayerDisplayData CreateDisplayData(Player p) => new()
        {
            Id = p.id,
            Username = p.username,
            Note = _playerNotes.GetValueOrDefault(p.id, ""),
            IsFriend = p.isfriend,
            Platform = GetPlayerPlatform(p),
            Tags = GetPlayerTags(p),
            NameColor = GetPlayerNameColor(p),
            avatar = p.avatar
        };

        private static void UpdateDisplayData(PlayerListUI.PlayerDisplayData data, Player p)
        {
            data.Username = p.username;
            data.IsFriend = p.isfriend;
            data.Platform = GetPlayerPlatform(p);
            data.Tags = GetPlayerTags(p);
            data.NameColor = GetPlayerNameColor(p);
            data.avatar = p.avatar;
        }

        private static (string text, Color color) GetPlayerPlatform(Player player)
        {
            try
            {
                if (player?.playerapi == null) return ("?", new Color(0.5f, 0.5f, 0.5f));
                if (player.isquestie) return ("Quest", Color.green);
                if (player.isvr) return ("PC-VR", new Color(0.75f, 0f, 0f));
                if (player.islocal) return ("LocalPlayer", Color.green);
                return ("Desktop", new Color(0.565f, 0.565f, 0.565f));
            }
            catch { return ("?", new Color(0.5f, 0.5f, 0.5f)); }
        }

        private static Color GetPlayerNameColor(Player player)
        {
            try { return player?.isfriend == true ? Color.yellow : Color.white; }
            catch { return Color.white; }
        }

        private static List<(string text, Color color)> GetPlayerTags(Player player)
        {
            var tags = new List<(string, Color)>();
            try
            {
                tags.Add(GetPlayerPlatform(player));
                if (player?.isfriend == true) tags.Add(("Friend", Color.yellow));
                if (player?.tag == "[Developer]") tags.Add(("Developer", new Color(1f, 0f, 1f)));
                if (player?.tag == "[Manager]") tags.Add(("Manager", new Color(1f, 0f, 1f)));
                if (player?.tag == "[User]") tags.Add(("User", Color.red));
                if (player.playerapi.isMaster) tags.Add(("Master", Color.red));
                if (player.apiuser.isSupporter) tags.Add(("V+", Color.red));
            }
            catch { }
            return tags;
        }

        // -----------------------------------------------------------------
        // Öffentliche Methoden für die UI
        // -----------------------------------------------------------------
        public static void TeleportToPlayer(string playerId)
        {
            Player target;
            lock (Player.players) target = Player.players.FirstOrDefault(p => p?.id == playerId);
            if (target?.vrcplayer == null) return;
            var local = Player.localplayer?.vrcplayer;
            if (local == null) return;
            var controller = local.GetComponent<CharacterController>();
            bool wasEnabled = controller != null && controller.enabled;
            if (controller != null) controller.enabled = false;
            local.transform.position = target.vrcplayer.transform.position;
            if (controller != null && wasEnabled) controller.enabled = true;
        }

        public static string GetBodyPartForPlayer(string playerId)
            => _playerBodyPartSelection.GetValueOrDefault(playerId, "Head");

        public static void SetBodyPartForPlayer(string playerId, string part)
            => _playerBodyPartSelection[playerId] = part;

        public static Vector3 GetOffsetForPlayer(string playerId)
            => _playerOffsets.GetValueOrDefault(playerId, Vector3.zero);

        public static void SetOffsetForPlayer(string playerId, Vector3 offset)
            => _playerOffsets[playerId] = offset;

        public static void ToggleSitOnPlayer(string playerId)
        {
            Player target;
            lock (Player.players) target = Player.players.FirstOrDefault(p => p?.id == playerId);
            if (target?.vrcplayer == null) return;

            string part = GetBodyPartForPlayer(playerId);
            Vector3 offset = GetOffsetForPlayer(playerId);
            if (!_bodyPartToBone.TryGetValue(part, out var bone)) return;

            ToggleSitCoroutine(target.VRCPlayer, bone, offset);
        }

        private static void ToggleSitCoroutine(VRC.Player target, HumanBodyBones bone, Vector3 offset)
        {
            if (_isSitting) { StopSitting(); return; }
            _currentSitCoroutine = CoroutineHelper.Start(SitCoroutine(target, bone, offset));
            UiModule.WatermarkRenderer.AddMod("Sit On");
        }

        private static void StopSitting()
        {
            if (!_isSitting) return;
            _isSitting = false;
            if (_currentSitCoroutine != null)
            {
                CoroutineHelper.Stop(_currentSitCoroutine);
                _currentSitCoroutine = null;
            }
            var localPlayer = VRCPlayer.field_Internal_Static_VRCPlayer_0;
            if (localPlayer != null)
            {
                var controller = localPlayer.gameObject.GetComponent<CharacterController>();
                if (controller != null && !controller.enabled) controller.enabled = true;
            }
            UiModule.WatermarkRenderer.RemoveMod("Sit On");
        }

        private static IEnumerator SitCoroutine(VRC.Player target, HumanBodyBones bone, Vector3 offset)
        {
            VRCPlayer localVRCPlayer = VRCPlayer.field_Internal_Static_VRCPlayer_0;
            VRCPlayerApi targetApi = target?.prop_VRCPlayerApi_0;
            if (localVRCPlayer == null || targetApi == null) yield break;

            var localController = localVRCPlayer.gameObject.GetComponent<CharacterController>();
            bool wasControllerEnabled = localController != null && localController.enabled;
            if (localController != null) localController.enabled = false;

            _isSitting = true;

            while (_isSitting)
            {
                if (target == null || targetApi == null || localVRCPlayer == null) break;
                if (Input.GetKeyDown(KeyCode.X))
                {
                    StopSitting();
                    break;
                }

                Vector3 bonePos = targetApi.GetBonePosition(bone);
                localVRCPlayer.transform.position = bonePos + offset;
                yield return null;
            }

            if (localController != null && wasControllerEnabled && !localController.enabled)
                localController.enabled = true;

            _isSitting = false;
            _currentSitCoroutine = null;
        }

        public static string[] GetAllBodyParts() => _allBodyParts;
    }
}