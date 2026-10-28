using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRClient.Modules;
using VRClient.SDK;
using VRClient.SDK.VRCs;

namespace VRClient.Menu
{
    // ------------------------------------------------------------
    // Datenstrukturen (unverändert)
    // ------------------------------------------------------------
    public struct Player3DBoxData
    {
        public Vector2 C0, C1, C2, C3, C4, C5, C6, C7;
        public bool V0, V1, V2, V3, V4, V5, V6, V7;
        public Color Color;
    }

    public class UnifiedEspLabel
    {
        public string Text;
        public Vector2 ScreenPos;
        public Color Color;
    }

    public class PlayerESPPoint
    {
        public string Name;
        public Vector2 ScreenPos;
        public Color Color;
    }

    public class ItemNamePoint
    {
        public string Name;
        public Vector2 ScreenPos;
        public Color Color;
    }

    public class SkeletonLine
    {
        public Vector2 Start;
        public Vector2 End;
        public Color Color;
    }

    public struct Player2DBoxData
    {
        public Vector2 Min;
        public Vector2 Max;
        public Color Color;
    }

    // =========================================================================
    // EspUI – Einstellungen und Zeichnen (Unity IMGUI)
    // =========================================================================
    internal class EspUI : UiModule.Page
    {
        public EspUI() : base("ESP") { }

        // Hilfsvariable für Collapsible States
        private static bool _playerSection = true;
        private static bool _boxSection = true;
        private static bool _capsuleSection;
        private static bool _itemSection;
        private static bool _interactableSection;

        public override void Render()
        {
            GUILayout.BeginVertical();

            // ---- Sektion: Spieler ----
            _playerSection = GUILayout.Toggle(_playerSection, "Spieler", GUI.skin.label);
            if (_playerSection)
            {
                GUILayout.BeginVertical(GUI.skin.box);

                // Name ESP
                bool nameEsp = Cfg.NameESP;
                if (GUILayout.Toggle(nameEsp, "Name ESP") != nameEsp)
                {
                    Cfg.NameESP = !nameEsp;
                    UpdateWatermark("Name ESP", Cfg.NameESP);
                }
                if (Cfg.NameESP && !Cfg.UseTrustColor)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Color", GUILayout.Width(50));
                    Cfg.CustomColor = ColorSlider4(Cfg.CustomColor);
                    GUILayout.EndHorizontal();
                }

                // Player Distance
                SliderWithLabel("Player Distance", ref Cfg.PlayerESPDistance, 10f, 200f, "F0");

                GUILayout.EndVertical();
            }

            // ---- Sektion: Boxen ----
            _boxSection = GUILayout.Toggle(_boxSection, "Boxen", GUI.skin.label);
            if (_boxSection)
            {
                GUILayout.BeginVertical(GUI.skin.box);

                bool box3 = Cfg.Box3ESP;
                if (GUILayout.Toggle(box3, "3D Box ESP") != box3)
                {
                    Cfg.Box3ESP = !box3;
                    UpdateWatermark("3D Box ESP", Cfg.Box3ESP);
                }
                bool box2 = Cfg.Box2ESP;
                if (GUILayout.Toggle(box2, "2D Box ESP") != box2)
                {
                    Cfg.Box2ESP = !box2;
                    UpdateWatermark("2D Box ESP", Cfg.Box2ESP);
                }
                if ((Cfg.Box3ESP || Cfg.Box2ESP) && !Cfg.UseTrustColor)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Color", GUILayout.Width(50));
                    Cfg.BoxColor = ColorSlider4(Cfg.BoxColor);
                    GUILayout.EndHorizontal();
                }
                SliderWithLabel("Thickness", ref Cfg.BoxThickness, 0.1f, 5f, "F1");

                // Skeleton
                bool skel = Cfg.SkeletonESP;
                if (GUILayout.Toggle(skel, "Skeleton ESP") != skel)
                {
                    Cfg.SkeletonESP = !skel;
                    UpdateWatermark("Skeleton ESP", Cfg.SkeletonESP);
                }
                SliderWithLabel("Skeleton Thickness", ref Cfg.SkeletonThickness, 0.1f, 5f, "F1");

                // Zombie Box
                bool zombie = ESPModule.ZombieBoxESP;
                if (GUILayout.Toggle(zombie, "Zombie Box ESP") != zombie)
                {
                    ESPModule.ZombieBoxESP = !zombie;
                    UpdateWatermark("Zombie Box ESP", ESPModule.ZombieBoxESP);
                }
                if (ESPModule.ZombieBoxESP)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Color", GUILayout.Width(50));
                    ESPModule.ZombieBoxColor = ColorSlider4(ESPModule.ZombieBoxColor);
                    GUILayout.EndHorizontal();
                    SliderWithLabel("Zombie Distance", ref Cfg.ZombieESPDistance, 10f, 200f, "F0");
                }

                GUILayout.EndVertical();
            }

            // ---- Sektion: Capsule ----
            _capsuleSection = GUILayout.Toggle(_capsuleSection, "Capsule", GUI.skin.label);
            if (_capsuleSection)
            {
                GUILayout.BeginVertical(GUI.skin.box);
                bool cap = Cfg.CapsuleESP;
                if (GUILayout.Toggle(cap, "Capsule ESP") != cap)
                {
                    Cfg.CapsuleESP = !cap;
                    UpdateWatermark("Capsule ESP", Cfg.CapsuleESP);
                }
                if (Cfg.CapsuleESP)
                {
                    if (!Cfg.UseTrustColor)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Color", GUILayout.Width(50));
                        Cfg.CapsuleColor = ColorSlider4(Cfg.CapsuleColor);
                        GUILayout.EndHorizontal();
                    }
                    SliderWithLabel("Blur Iterations", ref Cfg.CapsuleBlurIterations, 0, 3, "");
                    SliderWithLabel("Blur Size", ref Cfg.CapsuleBlurSize, 0.1f, 2f, "F1");
                    SliderWithLabel("Capsule Distance", ref Cfg.CapsuleESPDistance, 10f, 200f, "F0");
                }
                GUILayout.EndVertical();
            }

            // ---- Sektion: Items ----
            _itemSection = GUILayout.Toggle(_itemSection, "Items", GUI.skin.label);
            if (_itemSection)
            {
                GUILayout.BeginVertical(GUI.skin.box);
                bool itemBox = Cfg.ItemBoxESP;
                if (GUILayout.Toggle(itemBox, "Item Box ESP") != itemBox)
                {
                    Cfg.ItemBoxESP = !itemBox;
                    UpdateWatermark("Item Box ESP", Cfg.ItemBoxESP);
                }
                bool itemName = Cfg.ItemNameESP;
                if (GUILayout.Toggle(itemName, "Item Name ESP") != itemName)
                {
                    Cfg.ItemNameESP = !itemName;
                    UpdateWatermark("Item Name ESP", Cfg.ItemNameESP);
                }
                if (Cfg.ItemBoxESP || Cfg.ItemNameESP)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Color", GUILayout.Width(50));
                    Cfg.ItemBoxColor = ColorSlider4(Cfg.ItemBoxColor);
                    GUILayout.EndHorizontal();
                    SliderWithLabel("Item Distance", ref Cfg.ItemESPDistance, 10f, 200f, "F0");
                }
                GUILayout.EndVertical();
            }

            // ---- Sektion: Interactables ----
            _interactableSection = GUILayout.Toggle(_interactableSection, "Interactables", GUI.skin.label);
            if (_interactableSection)
            {
                GUILayout.BeginVertical(GUI.skin.box);
                bool worldBox = Cfg.InteractableBoxESP;
                if (GUILayout.Toggle(worldBox, "World Box ESP") != worldBox)
                {
                    Cfg.InteractableBoxESP = !worldBox;
                    UpdateWatermark("World Box ESP", Cfg.InteractableBoxESP);
                }
                bool worldName = Cfg.InteractableNameESP;
                if (GUILayout.Toggle(worldName, "World Name ESP") != worldName)
                {
                    Cfg.InteractableNameESP = !worldName;
                    UpdateWatermark("World Name ESP", Cfg.InteractableNameESP);
                }
                if (Cfg.InteractableBoxESP || Cfg.InteractableNameESP)
                {
                    // Filter
                    GUILayout.Label("Filter", GUILayout.Width(50));
                    FilterToggle("Udon", ref Cfg.UdonESP);
                    FilterToggle("Triggers", ref Cfg.TriggersESP);
                    FilterToggle("Station", ref Cfg.StationESP);
                    FilterToggle("Interactable", ref Cfg.InteractableESP);
                    if (GUILayout.Button("Refresh Filters", GUILayout.Width(120), GUILayout.Height(25)))
                        Cfg.InteractableRefresh = true;

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Color", GUILayout.Width(50));
                    Cfg.InteractableBoxColor = ColorSlider4(Cfg.InteractableBoxColor);
                    GUILayout.EndHorizontal();
                    SliderWithLabel("Interactable Distance", ref Cfg.InteractableESPDistance, 10f, 200f, "F0");
                }
                GUILayout.EndVertical();
            }

            // ---- Use Trust Color ----
            bool trust = Cfg.UseTrustColor;
            if (GUILayout.Toggle(trust, "Use Trust Color") != trust)
            {
                Cfg.UseTrustColor = !trust;
                UpdateWatermark("Use Trust Color", Cfg.UseTrustColor);
            }

            GUILayout.EndVertical();
        }

        // Einfache Slider+Label Kombination
        private static void SliderWithLabel(string label, ref float value, float min, float max, string format)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{label}: {value.ToString(format)}", GUILayout.Width(180));
            value = GUILayout.HorizontalSlider(value, min, max, GUILayout.Width(150));
            GUILayout.EndHorizontal();
        }

        private static void FilterToggle(string label, ref bool value)
        {
            GUILayout.BeginHorizontal();
            bool newVal = GUILayout.Toggle(value, label);
            if (newVal != value) { value = newVal; UpdateWatermark($"Filter {label}", value); }
            GUILayout.EndHorizontal();
        }

        // ------------------------------------------------------------
        // Überarbeitete ColorSlider4 (Preview als Box)
        // ------------------------------------------------------------
        private static Color ColorSlider4(Color color)
        {
            float r = color.r, g = color.g, b = color.b, a = color.a;
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.Label("R", GUILayout.Width(12)); r = GUILayout.HorizontalSlider(r, 0f, 1f);
            GUILayout.Label("G", GUILayout.Width(12)); g = GUILayout.HorizontalSlider(g, 0f, 1f);
            GUILayout.Label("B", GUILayout.Width(12)); b = GUILayout.HorizontalSlider(b, 0f, 1f);
            GUILayout.Label("A", GUILayout.Width(12)); a = GUILayout.HorizontalSlider(a, 0f, 1f);
            GUILayout.EndHorizontal();

            Color newColor = new Color(r, g, b, a);
            // Farbvorschau als Button (nicht interaktiv, aber sichtbar)
            var oldColor = GUI.color;
            GUI.color = newColor;
            GUILayout.Box("", GUILayout.Height(20));
            GUI.color = oldColor;

            GUILayout.EndVertical();
            return newColor;
        }

        // ---------- Zeichenmethoden (unverändert) ----------
        public static void Draw()
        {
            if (!_initialized) return;

            _allLabels.Clear();
            lock (_cacheLock)
            {
                if (Cfg.Box3ESP)
                    foreach (var box in _boxCache) Draw3DBox(box, Cfg.BoxThickness);

                if (Cfg.ItemBoxESP)
                    foreach (var box in _itemBoxCache) Draw3DBox(box, Cfg.BoxThickness);

                if (Cfg.InteractableBoxESP)
                    foreach (var box in _interactableBoxCache) Draw3DBox(box, Cfg.BoxThickness);

                if (Cfg.SkeletonESP)
                    foreach (var line in _skeletonLines) DrawLine(line.Start, line.End, line.Color, Cfg.SkeletonThickness);

                if (Cfg.Box2ESP)
                {
                    foreach (var box in _boxCache)
                        if (Get2DRectFrom3DBox(box, out var min, out var max))
                            DrawRect(min, max, box.Color, Cfg.BoxThickness);
                }

                if (ESPModule.ZombieBoxESP)
                    foreach (var box in ESPModule._zombieBoxCache)
                        Draw3DBox(box, 2f);

                if (Cfg.NameESP)
                    foreach (var p in _espPoints)
                        _allLabels.Add(new UnifiedEspLabel { Text = p.Name, ScreenPos = p.ScreenPos, Color = p.Color });

                if (Cfg.ItemNameESP)
                    foreach (var i in _itemNamePoints)
                        _allLabels.Add(new UnifiedEspLabel { Text = i.Name, ScreenPos = i.ScreenPos, Color = i.Color });

                if (Cfg.InteractableNameESP)
                    foreach (var i in _interactableNamePoints)
                        _allLabels.Add(new UnifiedEspLabel { Text = i.Name, ScreenPos = i.ScreenPos, Color = i.Color });
            }

            if (_allLabels.Count == 0) return;
            var groups = GroupLabelsByExactPosition(_allLabels, 0.1f);
            float lineHeight = GUI.skin.label.CalcSize(new GUIContent("A")).y + 2f;

            foreach (var group in groups)
            {
                if (group.Count == 1)
                    DrawText(group[0].Text, group[0].ScreenPos, group[0].Color);
                else
                {
                    Vector2 center = group[0].ScreenPos;
                    float totalHeight = group.Count * lineHeight;
                    float startY = center.y - totalHeight / 2f;
                    for (int i = 0; i < group.Count; i++)
                    {
                        Vector2 pos = new Vector2(center.x, startY + i * lineHeight);
                        DrawText(group[i].Text, pos, group[i].Color);
                    }
                }
            }
        }

        private static void DrawLine(Vector2 start, Vector2 end, Color color, float thickness)
        {
            if (thickness <= 0 || color.a == 0) return;
            Vector2 dir = end - start;
            float length = dir.magnitude;
            if (length < 0.001f) return;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            Rect rect = new Rect(start.x, start.y - thickness / 2f, length, thickness);
            Vector2 pivot = start;
            Matrix4x4 matrix = GUI.matrix;
            GUIUtility.RotateAroundPivot(angle, pivot);
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.matrix = matrix;
        }

        private static void DrawRect(Vector2 min, Vector2 max, Color color, float thickness)
        {
            if (thickness <= 0 || color.a == 0) return;
            DrawLine(new Vector2(min.x, min.y), new Vector2(max.x, min.y), color, thickness);
            DrawLine(new Vector2(max.x, min.y), new Vector2(max.x, max.y), color, thickness);
            DrawLine(new Vector2(max.x, max.y), new Vector2(min.x, max.y), color, thickness);
            DrawLine(new Vector2(min.x, max.y), new Vector2(min.x, min.y), color, thickness);
        }

        private static void Draw3DBox(Player3DBoxData box, float thickness)
        {
            Color col = box.Color;
            if (box.V0 && box.V1) DrawLine(box.C0, box.C1, col, thickness);
            if (box.V1 && box.V3) DrawLine(box.C1, box.C3, col, thickness);
            if (box.V3 && box.V2) DrawLine(box.C3, box.C2, col, thickness);
            if (box.V2 && box.V0) DrawLine(box.C2, box.C0, col, thickness);
            if (box.V4 && box.V5) DrawLine(box.C4, box.C5, col, thickness);
            if (box.V5 && box.V7) DrawLine(box.C5, box.C7, col, thickness);
            if (box.V7 && box.V6) DrawLine(box.C7, box.C6, col, thickness);
            if (box.V6 && box.V4) DrawLine(box.C6, box.C4, col, thickness);
            if (box.V0 && box.V4) DrawLine(box.C0, box.C4, col, thickness);
            if (box.V1 && box.V5) DrawLine(box.C1, box.C5, col, thickness);
            if (box.V2 && box.V6) DrawLine(box.C2, box.C6, col, thickness);
            if (box.V3 && box.V7) DrawLine(box.C3, box.C7, col, thickness);
        }

        private static void DrawText(string text, Vector2 pos, Color color)
        {
            GUIContent content = new GUIContent(text);
            Vector2 size = GUI.skin.label.CalcSize(content);
            Rect rect = new Rect(pos.x - size.x / 2f, pos.y - size.y / 2f, size.x, size.y);
            GUI.color = color;
            GUI.Label(rect, content);
            GUI.color = Color.white;
        }

        private static bool Get2DRectFrom3DBox(Player3DBoxData box, out Vector2 min, out Vector2 max)
        {
            min = default; max = default;
            bool any = false;
            var corners = new[] { box.C0, box.C1, box.C2, box.C3, box.C4, box.C5, box.C6, box.C7 };
            var valid = new[] { box.V0, box.V1, box.V2, box.V3, box.V4, box.V5, box.V6, box.V7 };
            for (int i = 0; i < 8; i++)
            {
                if (!valid[i]) continue;
                if (!any) { min = max = corners[i]; any = true; }
                else
                {
                    if (corners[i].x < min.x) min.x = corners[i].x;
                    if (corners[i].y < min.y) min.y = corners[i].y;
                    if (corners[i].x > max.x) max.x = corners[i].x;
                    if (corners[i].y > max.y) max.y = corners[i].y;
                }
            }
            return any;
        }

        private static List<List<UnifiedEspLabel>> GroupLabelsByExactPosition(List<UnifiedEspLabel> labels, float tolerance)
        {
            var groups = new List<List<UnifiedEspLabel>>();
            bool[] used = new bool[labels.Count];
            for (int i = 0; i < labels.Count; i++)
            {
                if (used[i]) continue;
                var group = new List<UnifiedEspLabel> { labels[i] };
                used[i] = true;
                Vector2 keyPos = RoundVector(labels[i].ScreenPos, tolerance);
                for (int j = i + 1; j < labels.Count; j++)
                {
                    if (used[j]) continue;
                    if (RoundVector(labels[j].ScreenPos, tolerance) == keyPos)
                    {
                        group.Add(labels[j]);
                        used[j] = true;
                    }
                }
                groups.Add(group);
            }
            return groups;
        }

        private static Vector2 RoundVector(Vector2 v, float tolerance)
        {
            float factor = 1f / tolerance;
            return new Vector2(Mathf.Round(v.x * factor) / factor, Mathf.Round(v.y * factor) / factor);
        }

        public static bool _initialized = false;
        public static List<UnifiedEspLabel> _allLabels = new List<UnifiedEspLabel>();
        public static object _cacheLock = new object();
        public static List<SkeletonLine> _skeletonLines = new List<SkeletonLine>();
        public static List<PlayerESPPoint> _espPoints = new List<PlayerESPPoint>();
        public static List<Player3DBoxData> _boxCache = new List<Player3DBoxData>();
        public static List<Player3DBoxData> _itemBoxCache = new List<Player3DBoxData>();
        public static List<ItemNamePoint> _itemNamePoints = new List<ItemNamePoint>();
        public static List<Player3DBoxData> _interactableBoxCache = new List<Player3DBoxData>();
        public static List<ItemNamePoint> _interactableNamePoints = new List<ItemNamePoint>();
    }

    // =========================================================================
    // EspUtils (unverändert)
    // =========================================================================
    public static class EspUtils
    {
        private static Vector3[] _worldCornersBuffer = new Vector3[8];
        private static Vector2[] _tmpCorners = new Vector2[8];
        private static bool[] _tmpValid = new bool[8];

        public static bool WorldToScreenPoint(Vector3 worldPos, Camera cam, int screenHeight, out Vector2 screenPos)
        {
            Vector3 sp = cam.WorldToScreenPoint(worldPos);
            if (sp.z > 0f)
            {
                screenPos = new Vector2(sp.x, screenHeight - sp.y);
                return true;
            }
            screenPos = default;
            return false;
        }

        public static bool WorldToScreenBounds(Bounds b, Camera cam, int screenHeight,
            out Vector2[] corners, out bool[] valid)
        {
            GetWorldCorners(b, _worldCornersBuffer);
            corners = _tmpCorners;
            valid = _tmpValid;
            bool any = false;
            for (int i = 0; i < 8; i++)
            {
                Vector3 sp = cam.WorldToScreenPoint(_worldCornersBuffer[i]);
                if (sp.z > 0f)
                {
                    any = true;
                    valid[i] = true;
                    corners[i] = new Vector2(sp.x, screenHeight - sp.y);
                }
                else valid[i] = false;
            }
            return any;
        }

        public static Player3DBoxData? TryBuildBoxData(Bounds bounds, Camera cam, int screenHeight, Color color)
        {
            return WorldToScreenBounds(bounds, cam, screenHeight, out var corners, out var valid)
                ? BuildBoxData(corners, valid, color)
                : null;
        }

        public static Player3DBoxData BuildBoxData(Vector2[] c, bool[] v, Color color) => new()
        {
            C0 = c[0],
            C1 = c[1],
            C2 = c[2],
            C3 = c[3],
            C4 = c[4],
            C5 = c[5],
            C6 = c[6],
            C7 = c[7],
            V0 = v[0],
            V1 = v[1],
            V2 = v[2],
            V3 = v[3],
            V4 = v[4],
            V5 = v[5],
            V6 = v[6],
            V7 = v[7],
            Color = color
        };

        public static void GetWorldCorners(Bounds b, Vector3[] buffer)
        {
            buffer[0] = new Vector3(b.min.x, b.min.y, b.min.z);
            buffer[1] = new Vector3(b.max.x, b.min.y, b.min.z);
            buffer[2] = new Vector3(b.min.x, b.max.y, b.min.z);
            buffer[3] = new Vector3(b.max.x, b.max.y, b.min.z);
            buffer[4] = new Vector3(b.min.x, b.min.y, b.max.z);
            buffer[5] = new Vector3(b.max.x, b.min.y, b.max.z);
            buffer[6] = new Vector3(b.min.x, b.max.y, b.max.z);
            buffer[7] = new Vector3(b.max.x, b.max.y, b.max.z);
        }

        public static Vector3 GetWorldCorner(Bounds b, int idx) => idx switch
        {
            0 => new Vector3(b.min.x, b.min.y, b.min.z),
            1 => new Vector3(b.max.x, b.min.y, b.min.z),
            2 => new Vector3(b.min.x, b.max.y, b.min.z),
            3 => new Vector3(b.max.x, b.max.y, b.min.z),
            4 => new Vector3(b.min.x, b.min.y, b.max.z),
            5 => new Vector3(b.max.x, b.min.y, b.max.z),
            6 => new Vector3(b.min.x, b.max.y, b.max.z),
            7 => new Vector3(b.max.x, b.max.y, b.max.z),
            _ => Vector3.zero
        };

        public static Bounds GetPickupBounds(GameObject obj)
        {
            var c = obj.GetComponent<Collider>();
            if (c) return c.bounds;
            var r = obj.GetComponentsInChildren<Renderer>();
            if (r.Length == 0) return new Bounds(obj.transform.position, Vector3.one * 0.5f);
            var b = r[0].bounds;
            for (int i = 1; i < r.Length; i++) b.Encapsulate(r[i].bounds);
            return b;
        }

        public static Bounds GetInteractableBounds(GameObject obj)
        {
            var cols = obj.GetComponentsInChildren<Collider>();
            if (cols.Length > 0)
            {
                var b = cols[0].bounds;
                for (int i = 1; i < cols.Length; i++) b.Encapsulate(cols[i].bounds);
                return b;
            }
            return GetPickupBounds(obj);
        }

        public static Bounds GetAvatarBounds(GameObject playerObj, Dictionary<GameObject, Transform> selectRegionCache)
        {
            if (!selectRegionCache.TryGetValue(playerObj, out var sr))
            {
                sr = playerObj.transform.Find("SelectRegion");
                selectRegionCache[playerObj] = sr;
            }
            var col = sr?.GetComponent<Collider>();
            if (col) return col.bounds;
            var r = sr?.GetComponent<Renderer>();
            if (r) return r.bounds;
            var cap = playerObj.GetComponent<CapsuleCollider>();
            return cap ? cap.bounds : new Bounds(playerObj.transform.position + Vector3.up, new Vector3(0.8f, 2f, 0.8f));
        }
    }

    internal class ESPModule : BaseModule
    {
        private const float ESP_INTERVAL = 0.023f;
        private const float INTERACTABLE_UPDATE_INTERVAL = 0.3f;
        private const float ZOMBIE_FIND_INTERVAL = 0.5f;

        private static readonly string[] Blacklist = {
            "vrcchair", "mirror", "spawn", "portal", "pickup", "notification", "cube", "mesh",
            "erase", "tracker", "parent", "hand", "achivement", "udon", "follow", "tracking",
            "head", "player", "hud", "breath", "video", "audio", "light", "particle", "respawn",
            "door", "button", "lever", "switch", "panel"
        };

        private static readonly (HumanBodyBones from, HumanBodyBones to)[] SkeletonConnections = {
            (HumanBodyBones.Head, HumanBodyBones.Neck),
            (HumanBodyBones.Neck, HumanBodyBones.UpperChest),
            (HumanBodyBones.UpperChest, HumanBodyBones.RightShoulder),
            (HumanBodyBones.RightShoulder, HumanBodyBones.RightUpperArm),
            (HumanBodyBones.RightUpperArm, HumanBodyBones.RightLowerArm),
            (HumanBodyBones.RightLowerArm, HumanBodyBones.RightHand),
            (HumanBodyBones.UpperChest, HumanBodyBones.LeftShoulder),
            (HumanBodyBones.LeftShoulder, HumanBodyBones.LeftUpperArm),
            (HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm),
            (HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftHand),
            (HumanBodyBones.UpperChest, HumanBodyBones.Chest),
            (HumanBodyBones.Chest, HumanBodyBones.Hips),
            (HumanBodyBones.Hips, HumanBodyBones.RightUpperLeg),
            (HumanBodyBones.RightUpperLeg, HumanBodyBones.RightLowerLeg),
            (HumanBodyBones.RightLowerLeg, HumanBodyBones.RightFoot),
            (HumanBodyBones.Hips, HumanBodyBones.LeftUpperLeg),
            (HumanBodyBones.LeftUpperLeg, HumanBodyBones.LeftLowerLeg),
            (HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftFoot),
        };

        public Camera _cachedCamera;
        public int _cachedScreenHeight;
        public float _lastCameraRefresh;
        public float _espTimer;
        private float _interactableTimer;
        private float _lastZombieFind;

        private readonly List<GameObject> _zombieObjects = new();
        public static Color ZombieBoxColor = Color.red;
        public static bool ZombieBoxESP;
        public static List<Player3DBoxData> _zombieBoxCache = new();

        private readonly List<PlayerESPPoint> _tempPoints = new(80);
        private readonly List<Player3DBoxData> _tempBoxes = new(80);
        private readonly List<Player3DBoxData> _tempItemBoxes = new(80);
        private readonly List<ItemNamePoint> _tempItemNames = new(80);
        private readonly List<Player3DBoxData> _tempInteractableBoxes = new(40);
        private readonly List<ItemNamePoint> _tempInteractableNames = new(40);
        private readonly List<Player> _tempPlayers = new(80);

        private readonly Dictionary<Player, HighlightsFXStandalone> _capsuleHighlighters = new();
        private readonly Dictionary<GameObject, Transform> _selectRegionCache = new();
        private readonly List<GameObject> _interactableObjects = new(200);
        private readonly Dictionary<GameObject, Bounds> _interactableBoundsCache = new();

        private int _lastFilterHash = -1;

        public override void Awake()
        {
            base.Awake();
            EspUI._initialized = true;
            RefreshPickups();
            RefreshInteractablesList();
            _lastFilterHash = BerechneFilterHash();
        }

        public override void OnUpdate()
        {
            _espTimer += Time.unscaledDeltaTime;
            if (_espTimer < ESP_INTERVAL) return;
            _espTimer -= ESP_INTERVAL;

            if (_cachedCamera == null || Time.unscaledTime - _lastCameraRefresh > 0.2f)
            {
                _cachedCamera = Camera.main;
                _lastCameraRefresh = Time.unscaledTime;
            }
            _cachedScreenHeight = Screen.height;
            if (_cachedCamera == null) return;

            bool doName = Cfg.NameESP, doBox = Cfg.Box3ESP || Cfg.Box2ESP,
                 doItemBox = Cfg.ItemBoxESP, doItemName = Cfg.ItemNameESP,
                 doInteractable = Cfg.InteractableBoxESP, doInteractableName = Cfg.InteractableNameESP,
                 doCapsule = Cfg.CapsuleESP, doSkeleton = Cfg.SkeletonESP;

            if (!doName && !doBox && !doItemBox && !doItemName &&
                !doInteractable && !doInteractableName && !doCapsule && !doSkeleton && !ZombieBoxESP)
                return;

            int currentFilterHash = BerechneFilterHash();
            if (currentFilterHash != _lastFilterHash || Cfg.InteractableRefresh)
            {
                _lastFilterHash = currentFilterHash;
                RefreshInteractablesList();
                _interactableBoundsCache.Clear();
                Cfg.InteractableRefresh = false;
            }

            Vector3 localPos = Player.localplayer?.playerObject?.transform?.position ?? Vector3.zero;

            // Zombies (eigener Timer)
            if (ZombieBoxESP && Time.unscaledTime - _lastZombieFind > ZOMBIE_FIND_INTERVAL)
            {
                FindZombies();
                _lastZombieFind = Time.unscaledTime;
            }

            _tempPoints.Clear(); _tempBoxes.Clear(); _tempItemBoxes.Clear();
            _tempItemNames.Clear(); _tempInteractableBoxes.Clear(); _tempInteractableNames.Clear();

            float playerDist = Cfg.PlayerESPDistance;
            lock (Player.players) { _tempPlayers.Clear(); _tempPlayers.AddRange(Player.players); }
            var localPlayer = Player.localplayer;

            foreach (var p in _tempPlayers)
            {
                if (p?.playerObject == null || p.playerapi == null || p == localPlayer) continue;
                Vector3 pPos = p.playerObject.transform.position;
                if (Vector3.Distance(localPos, pPos) > playerDist) continue;

                if (doName)
                {
                    if (EspUtils.WorldToScreenPoint(pPos - Vector3.up * 0.3f, _cachedCamera, _cachedScreenHeight, out var sp))
                    {
                        var col = Cfg.UseTrustColor ? TrustColorToColor(p.trustColorUint) : Cfg.CustomColor;
                        _tempPoints.Add(new PlayerESPPoint { Name = string.IsNullOrEmpty(p.tag) ? p.username : $"{p.tag} {p.username}", ScreenPos = sp, Color = col });
                    }
                }
                if (doBox)
                {
                    var box = EspUtils.TryBuildBoxData(EspUtils.GetAvatarBounds(p.playerObject, _selectRegionCache), _cachedCamera, _cachedScreenHeight, Cfg.BoxColor);
                    if (box.HasValue) _tempBoxes.Add(box.Value);
                }
            }

            UpdateSkeletonESP(playerDist);

            if (doItemBox || doItemName)
                ProcessPickups(doItemBox, doItemName, localPos, Cfg.ItemESPDistance);

            if (doInteractable || doInteractableName)
            {
                _interactableTimer += Time.unscaledDeltaTime;
                if (_interactableTimer >= INTERACTABLE_UPDATE_INTERVAL)
                {
                    _interactableTimer = 0f;
                    ProcessInteractables(doInteractable, doInteractableName, localPos, Cfg.InteractableESPDistance);
                }
            }

            UpdateCapsuleESP(localPos, Cfg.CapsuleESPDistance);

            var newZombieBoxes = new List<Player3DBoxData>();
            if (ZombieBoxESP)
            {
                float zombieDist = Cfg.ZombieESPDistance;
                foreach (var zombie in _zombieObjects)
                {
                    if (zombie == null) continue;
                    Vector3 zPos = zombie.transform.position;
                    if (Vector3.Distance(localPos, zPos) > zombieDist) continue;
                    var bounds = GetZombieBounds(zombie);
                    var box = EspUtils.TryBuildBoxData(bounds, _cachedCamera, _cachedScreenHeight, ZombieBoxColor);
                    if (box.HasValue) newZombieBoxes.Add(box.Value);
                }
            }

            lock (EspUI._cacheLock)
            {
                EspUI._espPoints = Swap(EspUI._espPoints, _tempPoints);
                EspUI._boxCache = Swap(EspUI._boxCache, _tempBoxes);
                EspUI._itemBoxCache = Swap(EspUI._itemBoxCache, _tempItemBoxes);
                EspUI._itemNamePoints = Swap(EspUI._itemNamePoints, _tempItemNames);
                EspUI._interactableBoxCache = Swap(EspUI._interactableBoxCache, _tempInteractableBoxes);
                EspUI._interactableNamePoints = Swap(EspUI._interactableNamePoints, _tempInteractableNames);
                _zombieBoxCache = newZombieBoxes;
            }
        }

        private static List<T> Swap<T>(List<T> current, List<T> temp)
        {
            current.Clear();
            current.AddRange(temp);
            return current;
        }

        private static Color TrustColorToColor(uint trustColor)
        {
            float r = (trustColor & 0xFF) / 255f;
            float g = ((trustColor >> 8) & 0xFF) / 255f;
            float b = ((trustColor >> 16) & 0xFF) / 255f;
            float a = ((trustColor >> 24) & 0xFF) / 255f;
            return new Color(r, g, b, a);
        }

        private int BerechneFilterHash() =>
            (Cfg.UdonESP ? 1 : 0) | (Cfg.TriggersESP ? 2 : 0) |
            (Cfg.StationESP ? 4 : 0) | (Cfg.InteractableESP ? 8 : 0);

        public static Bounds GetZombieBounds(GameObject zombieObj)
        {
            var rend = zombieObj.GetComponent<Renderer>();
            if (rend && rend.enabled) return rend.bounds;
            var col = zombieObj.GetComponentInChildren<Collider>();
            if (col && col.enabled) return col.bounds;
            return new Bounds(zombieObj.transform.position, Vector3.one * 0.8f);
        }

        private void FindZombies()
        {
            _zombieObjects.Clear();
            foreach (var rend in UnityEngine.Object.FindObjectsOfType<Renderer>())
            {
                if (!rend || !rend.enabled) continue;
                string name = rend.gameObject.name.ToLowerInvariant();
                if (name.Contains("zombie") || name.Contains("npc") || name.Contains("entity") || name.Contains("attack"))
                    _zombieObjects.Add(rend.gameObject);
            }
        }

        private void ProcessPickups(bool doBox, bool doName, Vector3 localPos, float maxDist)
        {
            foreach (var kvp in Pickup.pickups)
            {
                var obj = kvp.Key?.gameObject;
                if (!obj) continue;
                Vector3 objPos = obj.transform.position;
                if (Vector3.Distance(localPos, objPos) > maxDist) continue;
                var bounds = EspUtils.GetPickupBounds(obj);
                if (doBox)
                {
                    var box = EspUtils.TryBuildBoxData(bounds, _cachedCamera, _cachedScreenHeight, Cfg.ItemBoxColor);
                    if (box.HasValue) _tempItemBoxes.Add(box.Value);
                }
                if (doName)
                {
                    Vector3 top = (EspUtils.GetWorldCorner(bounds, 2) + EspUtils.GetWorldCorner(bounds, 3)) * 0.5f;
                    if (EspUtils.WorldToScreenPoint(top, _cachedCamera, _cachedScreenHeight, out var sp))
                        _tempItemNames.Add(new ItemNamePoint { Name = obj.name, ScreenPos = sp, Color = Cfg.ItemBoxColor });
                }
            }
        }

        private void ProcessInteractables(bool doBox, bool doName, Vector3 localPos, float maxDist)
        {
            var planes = GeometryUtility.CalculateFrustumPlanes(_cachedCamera);
            foreach (var obj in _interactableObjects)
            {
                if (!obj) continue;
                if (!_interactableBoundsCache.TryGetValue(obj, out var bounds))
                {
                    bounds = EspUtils.GetInteractableBounds(obj);
                    _interactableBoundsCache[obj] = bounds;
                }
                if (Vector3.Distance(localPos, bounds.center) > maxDist) continue;
                if (!GeometryUtility.TestPlanesAABB(planes, bounds)) continue;
                if (doBox)
                {
                    var box = EspUtils.TryBuildBoxData(bounds, _cachedCamera, _cachedScreenHeight, Cfg.InteractableBoxColor);
                    if (box.HasValue) _tempInteractableBoxes.Add(box.Value);
                }
                if (doName)
                {
                    Vector3 top = (EspUtils.GetWorldCorner(bounds, 2) + EspUtils.GetWorldCorner(bounds, 3)) * 0.5f;
                    if (EspUtils.WorldToScreenPoint(top, _cachedCamera, _cachedScreenHeight, out var sp))
                        _tempInteractableNames.Add(new ItemNamePoint { Name = obj.name, ScreenPos = sp, Color = Cfg.InteractableBoxColor });
                }
            }
        }

        private void UpdateSkeletonESP(float maxDist)
        {
            var lines = new List<SkeletonLine>();
            if (!Cfg.SkeletonESP) return;
            var local = Player.localplayer;
            var localPos = local?.playerObject?.transform?.position ?? Vector3.zero;
            foreach (var p in Player.players)
            {
                if (p?.playerObject == null || p.playerapi == null || p == local) continue;
                Vector3 pPos = p.playerObject.transform.position;
                if (Vector3.Distance(localPos, pPos) > maxDist) continue;
                Color color = Cfg.UseTrustColor ? TrustColorToColor(p.trustColorUint) : Cfg.CustomColor;
                var api = p.playerapi;
                bool hasUpperChest = api.GetBoneTransform(HumanBodyBones.UpperChest) != null;
                foreach (var conn in SkeletonConnections)
                {
                    var fromBone = conn.from == HumanBodyBones.UpperChest && !hasUpperChest ? HumanBodyBones.Chest : conn.from;
                    var toBone = conn.to == HumanBodyBones.UpperChest && !hasUpperChest ? HumanBodyBones.Chest : conn.to;
                    var tFrom = api.GetBoneTransform(fromBone);
                    var tTo = api.GetBoneTransform(toBone);
                    if (!tFrom || !tTo) continue;
                    if (EspUtils.WorldToScreenPoint(tFrom.position, _cachedCamera, _cachedScreenHeight, out var sf) &&
                        EspUtils.WorldToScreenPoint(tTo.position, _cachedCamera, _cachedScreenHeight, out var st))
                        lines.Add(new SkeletonLine { Start = sf, End = st, Color = color });
                }
            }
            lock (EspUI._cacheLock) EspUI._skeletonLines = lines;
        }

        private void UpdateCapsuleESP(Vector3 localPos, float maxDist)
        {
            if (!Cfg.CapsuleESP)
            {
                foreach (var hl in _capsuleHighlighters.Values) if (hl) UnityEngine.Object.Destroy(hl);
                _capsuleHighlighters.Clear();
                return;
            }

            var players = Player.players;
            var activeSet = new HashSet<Player>(players);
            foreach (var p in players)
            {
                if (p?.playerObject == null) continue;
                Vector3 pPos = p.playerObject.transform.position;
                bool inRange = Vector3.Distance(localPos, pPos) <= maxDist;

                if (!_capsuleHighlighters.TryGetValue(p, out var hl) || !hl)
                {
                    if (!inRange) continue;
                    hl = _cachedCamera.gameObject.AddComponent<HighlightsFXStandalone>();
                    _capsuleHighlighters[p] = hl;
                }

                if (inRange)
                {
                    if (!_selectRegionCache.TryGetValue(p.playerObject, out var sr))
                    {
                        sr = p.playerObject.transform.Find("SelectRegion");
                        _selectRegionCache[p.playerObject] = sr;
                    }
                    var mf = sr?.GetComponent<MeshFilter>();
                    if (mf)
                    {
                        Color col = Cfg.UseTrustColor ? TrustColorToColor(p.trustColorUint) : Cfg.CapsuleColor;
                        hl.Method_Public_Void_MeshFilter_Color_0(mf, col);
                        hl.blurIterations = (int)Cfg.CapsuleBlurIterations;
                        hl.blurSize = Cfg.CapsuleBlurSize;
                        hl.renderTextureWidth = 2048;
                        hl.renderTextureHeight = 1424;
                        hl.enabled = true;
                    }
                    else hl.enabled = false;
                }
                else
                {
                    hl.enabled = false; // außer Reichweite: Highlighter deaktivieren, nicht zerstören
                }
            }

            // Highlighter von nicht mehr existierenden Spielern entfernen
            foreach (var kvp in _capsuleHighlighters.Where(kvp => !activeSet.Contains(kvp.Key)).ToList())
            {
                if (kvp.Value) UnityEngine.Object.Destroy(kvp.Value);
                _capsuleHighlighters.Remove(kvp.Key);
            }
        }

        private void RefreshPickups()
        {
            try
            {
                foreach (var pickup in UnityEngine.Object.FindObjectsOfType<VRC_Pickup>(true))
                    if (!Pickup.pickups.ContainsKey(pickup)) new Pickup(pickup);
                var dead = Pickup.pickups.Keys.Where(k => !k || !k.gameObject).ToList();
                foreach (var k in dead) Pickup.pickups.Remove(k);
            }
            catch (Exception ex) { VRClient.EntryPoint._Log?.LogWarning($"RefreshPickups error: {ex.Message}"); }
        }

        private void RefreshInteractablesList()
        {
            var unique = new HashSet<GameObject>();
            void Add<T>(bool flag) where T : Component
            {
                if (!flag) return;
                foreach (var c in UnityEngine.Object.FindObjectsOfType<T>(true)) unique.Add(c.gameObject);
            }
            Add<UdonBehaviour>(Cfg.UdonESP);
            Add<VRC_Trigger>(Cfg.TriggersESP);
            Add<VRCStation>(Cfg.StationESP);
            Add<VRC_Interactable>(Cfg.InteractableESP);

            _interactableObjects.Clear();
            foreach (var obj in unique)
            {
                if (!obj) continue;
                string lower = obj.name.ToLowerInvariant();
                if (Array.Exists(Blacklist, b => lower.Contains(b))) continue;
                _interactableObjects.Add(obj);
            }
            _interactableBoundsCache.Clear();
        }

        public override void OnSceneLoaded(int scene, string mode)
        {
            if (scene == -1)
            {
                _selectRegionCache.Clear();
                _interactableBoundsCache.Clear();
                RefreshPickups();
                RefreshInteractablesList();
                _lastFilterHash = BerechneFilterHash();
            }
        }

        public override void OnSceneUnLoaded(int scene, string mode)
        {
            lock (EspUI._cacheLock)
            {
                EspUI._espPoints.Clear(); EspUI._boxCache.Clear(); EspUI._itemBoxCache.Clear();
                EspUI._itemNamePoints.Clear(); EspUI._interactableBoxCache.Clear();
                EspUI._interactableNamePoints.Clear(); EspUI._skeletonLines.Clear();
            }
            Pickup.pickups.Clear();
            _interactableObjects.Clear();
            _interactableBoundsCache.Clear();
            _selectRegionCache.Clear();
            foreach (var hl in _capsuleHighlighters.Values) if (hl) UnityEngine.Object.Destroy(hl);
            _capsuleHighlighters.Clear();
        }
    }
}