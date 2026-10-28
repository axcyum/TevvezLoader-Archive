using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using VRC.Core;
using VRC.SDKBase;
using VRClient.IMGUI;
using VRClient.Menu;
using VRClient.SDK;
using VRClient.SDK.VRCs;
using static VRClient.SDK.Logging;

namespace VRClient.Modules
{
    // =========================================================================
    // BaseModule (unverändert)
    // =========================================================================
    public abstract class BaseModule : MonoBehaviour
    {
        public virtual bool NeedsUpdate => true;

        public virtual void Awake() => EventManager.RegisterModule(this);
        public virtual void OnUpdate() { }
        public virtual void OnPlayerJoin(VRC.Player player) { }
        public virtual void OnPlayerLeft(VRC.Player player) { }
        public virtual void OnSceneUnLoaded(int scene, string mode) { }
        public virtual void OnSceneLoaded(int scene, string mode) { }
        public virtual void OnInstanceLoaded(ApiWorld world, ApiWorldInstance instance) { }
        public virtual void OnDestroy() => EventManager.UnregisterModule(this);

        public static void UpdateWatermark(string name, bool enabled, bool showHud = false)
        {
            try
            {
                if (Cfg.Hud)
                {
                    if (enabled)
                        UiModule.WatermarkRenderer.AddMod(name);
                    else
                        UiModule.WatermarkRenderer.RemoveMod(name);
                }

                if (showHud)
                    MainThreadDispatcher.Run(() => PopupUtils.HudMessage("Tevvez Cheats", $"{name} : {enabled}", 5f));
            }
            catch (Exception ex)
            {
                Logging.Log($"[ERR_UIM_001] UpdateWatermark failed for {name}: {ex.Message}", Logging.LType.Error);
            }
        }
    }

    // =========================================================================
    // EventManager (unverändert)
    // =========================================================================
    internal class EventManager : MonoBehaviour
    {
        public static EventManager Instance { get; private set; }

        private static readonly List<BaseModule> modules = new List<BaseModule>();
        private static readonly object _lock = new object();

        public static event Action OnAnyUpdate;

        public static void RegisterModule(BaseModule m)
        {
            lock (_lock) modules.Add(m);
        }

        public static void UnregisterModule(BaseModule m)
        {
            lock (_lock) modules.Remove(m);
        }

        public static IEnumerable<BaseModule> GetModules()
        {
            lock (_lock) return modules.ToArray();
        }

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            try { OnAnyUpdate?.Invoke(); }
            catch (Exception ex)
            {
                Logging.Log($"[ERR_UIM_002] OnAnyUpdate event error: {ex.Message}", Logging.LType.Error);
            }

            BaseModule[] snapshot;
            lock (_lock) snapshot = modules.ToArray();

            for (int i = snapshot.Length - 1; i >= 0; i--)
            {
                var m = snapshot[i];
                if (m != null && m.NeedsUpdate)
                {
                    try { m.OnUpdate(); }
                    catch (Exception ex)
                    {
                        Logging.Log($"[ERR_UIM_003] Module {m.GetType().Name}.OnUpdate error: {ex.Message}", Logging.LType.Error);
                    }
                }
            }
        }
    }

    // =========================================================================
    // UiModule – überarbeitet: Speicherlecks behoben, Styles gecached
    // =========================================================================
    public class UiModule : BaseModule
    {
        // ----- Seiten-System -----
        public abstract class Page
        {
            public string Title { get; }
            protected Page(string title) => Title = title;
            public abstract void Render();   // Wird innerhalb der ScrollView aufgerufen (GUILayout)
            public static void UpdateWatermark(string name, bool enabled)
                => BaseModule.UpdateWatermark(name, enabled, showHud: true);
        }

        // ----- Wasserzeichen / Mod-Liste (Speicher‑sicher) -----
        public class WatermarkRenderer
        {
            private static readonly float RainbowSpeed = 0.15f;
            private static readonly Stopwatch _stopwatch = Stopwatch.StartNew();
            private static readonly HashSet<string> _modSet = new HashSet<string>();
            private static readonly List<string> _modList = new List<string>();
            private static readonly object _lock = new object();
            private static string _cachedWatermarkText = "";
            private static Vector2 _cachedWatermarkSize;
            private static bool _watermarkSizeCached;

            // Einmalige Texturen für den Rahmen (Farbe konstant)
            private static Texture2D _borderTexture;
            private static readonly Color BorderColor = new Color(153f / 255f, 76f / 255f, 204f / 255f, 1f);

            private static Texture2D GetBorderTexture()
            {
                if (_borderTexture == null)
                {
                    _borderTexture = new Texture2D(1, 1);
                    _borderTexture.SetPixel(0, 0, BorderColor);
                    _borderTexture.Apply();
                }
                return _borderTexture;
            }

            public static void DestroyResources()
            {
                if (_borderTexture != null)
                {
                    UnityEngine.Object.Destroy(_borderTexture);
                    _borderTexture = null;
                }
            }

            private static Color GetRainbowColor(float time)
            {
                float hue = (time * RainbowSpeed) % 1f;
                return Color.HSVToRGB(hue, 1f, 1f);
            }

            public static void AddMod(string modName)
            {
                lock (_lock)
                {
                    if (_modSet.Add(modName))
                    {
                        _modList.Clear();
                        _modList.AddRange(_modSet);
                        _watermarkSizeCached = false;
                    }
                }
            }

            public static void RemoveMod(string modName)
            {
                lock (_lock)
                {
                    if (_modSet.Remove(modName))
                    {
                        _modList.Clear();
                        _modList.AddRange(_modSet);
                        _watermarkSizeCached = false;
                    }
                }
            }

            public static void ClearMods()
            {
                lock (_lock)
                {
                    _modSet.Clear();
                    _modList.Clear();
                    _watermarkSizeCached = false;
                }
            }

            private static string[] GetModsSnapshot()
            {
                lock (_lock) return _modList.ToArray();
            }

            private static Vector2 CalcTextSize(string text)
            {
                return GUI.skin.label.CalcSize(new GUIContent(text));
            }

            public void Render(string windowTitle)
            {
                try
                {
                    if (Screen.width <= 0 || Screen.height <= 0)
                        return;

                    float framerate = Time.frameCount / Time.time;
                    string watermarkText = string.Format(
                        "YourName",
                        windowTitle, (int)framerate
                    );

                    if (watermarkText != _cachedWatermarkText)
                    {
                        _cachedWatermarkText = watermarkText;
                        _watermarkSizeCached = false;
                    }

                    if (!_watermarkSizeCached)
                    {
                        _cachedWatermarkSize = CalcTextSize(watermarkText);
                        _watermarkSizeCached = true;
                    }

                    const float padding = 5f;
                    const float spacing = 5f;
                    Vector2 pos = new Vector2(30, 20);

                    Color rainbow = GetRainbowColor((float)_stopwatch.Elapsed.TotalSeconds);
                    Color textColor = rainbow;

                    // Einmalig erstellte Border-Textur verwenden
                    Texture2D borderTex = GetBorderTexture();

                    // Wasserzeichen-Hintergrund
                    Rect rect = new Rect(pos.x, pos.y, _cachedWatermarkSize.x + padding * 2, _cachedWatermarkSize.y + padding * 2);
                    GUI.Box(rect, GUIContent.none);
                    GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, 2f), borderTex);
                    GUI.DrawTexture(new Rect(rect.x, rect.y + rect.height - 2f, rect.width, 2f), borderTex);

                    GUI.color = textColor;
                    GUI.Label(new Rect(pos.x + padding, pos.y + padding, _cachedWatermarkSize.x, _cachedWatermarkSize.y), watermarkText);
                    GUI.color = Color.white;

                    float currentY = rect.y + rect.height + spacing;
                    foreach (string mod in GetModsSnapshot())
                    {
                        Vector2 modSize = CalcTextSize(mod);
                        Rect modRect = new Rect(pos.x, currentY, modSize.x + padding * 2, modSize.y + padding * 2);
                        GUI.Box(modRect, GUIContent.none);
                        GUI.DrawTexture(new Rect(modRect.x, modRect.y, modRect.width, 2f), borderTex);
                        GUI.DrawTexture(new Rect(modRect.x, modRect.y + modRect.height - 2f, modRect.width, 2f), borderTex);
                        GUI.color = textColor;
                        GUI.Label(new Rect(modRect.x + padding, modRect.y + padding, modSize.x, modSize.y), mod);
                        GUI.color = Color.white;
                        currentY += modRect.height + spacing;
                    }
                }
                catch (Exception ex)
                {
                    Logging.Log($"[ERR_UIM_004] WatermarkRenderer.Render error: {ex.Message}", Logging.LType.Error);
                }
            }
        }

        // ----- Log-System (unverändert, aber Styles gecached) -----
        private static readonly List<(LType Type, string Message)> _logEntries = new List<(LType, string)>();
        private static readonly object _logLock = new object();
        public const int MaxLogEntries = 20;

        public static void AddLogEntry(string message, LType type = LType.Info)
        {
            lock (_logLock)
            {
                _logEntries.Add((type, message));
                if (_logEntries.Count > MaxLogEntries)
                    _logEntries.RemoveAt(0);
            }
        }

        private static (LType, string)[] GetLogSnapshot()
        {
            lock (_logLock) return _logEntries.ToArray();
        }

        // Gecachte GUIStyles für Log-Einträge
        private static readonly Dictionary<LType, GUIStyle> _logStyles = new Dictionary<LType, GUIStyle>();
        private static GUIStyle GetLogStyle(LType type)
        {
            if (!_logStyles.TryGetValue(type, out var style))
            {
                style = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleRight,
                    normal = { textColor = GetLogColor(type) }
                };
                _logStyles[type] = style;
            }
            return style;
        }

        private static Color GetLogColor(LType type)
        {
            switch (type)
            {
                case LType.Info: return Color.white;
                case LType.Warning: return Color.yellow;
                case LType.Error: return Color.red;
                case LType.Success: return Color.green;
                case LType.Debug: return Color.cyan;
                case LType.Join: return new Color(0f, 1f, 0.5f);
                case LType.Leave: return Color.gray;
                case LType.World: return new Color(0.5f, 0.5f, 1f);
                case LType.Crash: return Color.magenta;
                case LType.JavaScript: return new Color(1f, 0.5f, 0f);
                case LType.UdonDump: return new Color(1f, 0.8f, 0.4f);
                case LType.CEAddresse: return new Color(0.8f, 0.2f, 0.8f);
                case LType.Avatar: return new Color(0.2f, 0.8f, 1f);
                case LType.Udon: return new Color(0.6f, 1f, 0.6f);
                case LType.Heap: return new Color(1f, 0.6f, 0.6f);
                case LType.Login: return new Color(0.3f, 0.8f, 0.3f);
                case LType.Photon: return new Color(0.2f, 0.6f, 1f);
                case LType.Assets: return new Color(1f, 0.7f, 0.2f);
                case LType.Event: return new Color(0.9f, 0.9f, 0.1f);
                default: return Color.white;
            }
        }

        private static void DrawLogs()
        {
            try
            {
                var logs = GetLogSnapshot();
                if (logs.Length == 0) return;

                float rightX = Screen.width - 20;
                float topY = 20;
                float lineHeight = 20;
                float maxWidth = 1500;

                for (int i = logs.Length - 1; i >= 0; i--)
                {
                    var (type, message) = logs[i];
                    string display = $"[{type}] {message}";
                    GUIStyle style = GetLogStyle(type);
                    GUI.color = style.normal.textColor;
                    GUI.Label(new Rect(rightX - maxWidth, topY, maxWidth, lineHeight), display, style);
                    topY += lineHeight;
                }
                GUI.color = Color.white;
            }
            catch (Exception ex)
            {
                Logging.Log($"[ERR_UIM_LOG] DrawLogs error: {ex.Message}", Logging.LType.Error);
            }
        }

        // ----- Menü-Zustand -----
        public static bool DrawMenu = true;
        private static readonly WatermarkRenderer _watermark = new WatermarkRenderer();
        private static readonly Dictionary<string, List<Page>> _pageCategories = new Dictionary<string, List<Page>>();
        private static string _activeCategory = "";
        private static int _activePageIndex;
        private static Rect _windowRect = new Rect(100, 100, 900, 600);
        private static bool _windowVisible = false;
        private static bool _isResizing = false;
        private static Vector2 _resizeStartMouse;
        private static Rect _resizeStartRect;

        // Scroll-Positionen pro Seite (global)
        private static readonly Dictionary<string, Vector2> _scrollPositions = new Dictionary<string, Vector2>();

        // Styles (einmal initialisiert)
        private static GUIStyle _categoryStyle;
        private static GUIStyle _tabButtonStyle;
        private static GUIStyle _tabButtonActiveStyle;
        private static Texture2D _activeTabBackground;

        private static void InitStyles()
        {
            if (_categoryStyle == null)
            {
                _categoryStyle = new GUIStyle(GUI.skin.label)
                {
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = Color.white },
                    padding = new RectOffset(5, 5, 5, 5)
                };

                _tabButtonStyle = new GUIStyle(GUI.skin.button)
                {
                    alignment = TextAnchor.MiddleLeft,
                    padding = new RectOffset(20, 5, 5, 5),
                    normal = { textColor = new Color(132f / 255f, 136f / 255f, 139f / 255f, 1f) },
                    hover = { textColor = Color.white },
                    active = { textColor = Color.white },
                    onNormal = { textColor = Color.white },
                    onHover = { textColor = Color.white },
                    onActive = { textColor = Color.white },
                    margin = new RectOffset(0, 0, 2, 2)
                };

                _activeTabBackground = MakeColorTexture(new Color(0.2f, 0.2f, 0.2f, 0.5f));

                _tabButtonActiveStyle = new GUIStyle(_tabButtonStyle)
                {
                    normal = { textColor = Color.white, background = _activeTabBackground },
                    onNormal = { textColor = Color.white, background = _activeTabBackground }
                };
            }
        }

        private static Texture2D MakeColorTexture(Color color)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, color);
            tex.Apply();
            return tex;
        }

        public static void AddOrUpdatePage(string category, Page page)
        {
            try
            {
                if (!_pageCategories.TryGetValue(category, out var list))
                {
                    list = new List<Page>();
                    _pageCategories[category] = list;
                }

                int existingIndex = list.FindIndex(p => p.Title == page.Title);
                if (existingIndex != -1)
                    list[existingIndex] = page;
                else
                    list.Add(page);

                if (string.IsNullOrEmpty(_activeCategory) || !_pageCategories.ContainsKey(_activeCategory))
                {
                    _activeCategory = category;
                    _activePageIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Logging.Log($"[ERR_UIM_005] AddOrUpdatePage failed: {ex.Message}", Logging.LType.Error);
            }
        }

        public bool RemovePage(string category, Page page)
        {
            try
            {
                if (!_pageCategories.TryGetValue(category, out var list))
                    return false;

                int index = list.IndexOf(page);
                if (index == -1) return false;

                list.RemoveAt(index);
                if (list.Count == 0)
                {
                    _pageCategories.Remove(category);
                    if (_activeCategory == category)
                    {
                        _activeCategory = _pageCategories.Keys.FirstOrDefault() ?? "";
                        _activePageIndex = 0;
                    }
                }
                else if (_activeCategory == category && _activePageIndex >= list.Count)
                {
                    _activePageIndex = list.Count - 1;
                }
                return true;
            }
            catch (Exception ex)
            {
                Logging.Log($"[ERR_UIM_006] RemovePage failed: {ex.Message}", Logging.LType.Error);
                return false;
            }
        }

        // ========================================================================
        // OnGUI – mit Resize-Handling
        // ========================================================================
        private void OnGUI()
        {
            // Watermark immer zeichnen
            try { _watermark.Render("VRChat"); }
            catch (Exception ex) { Logging.Log($"[ERR_UIM_009] Watermark render error: {ex.Message}", Logging.LType.Error); }

            DrawLogs();

            // ESP
            if (Cfg.NameESP || Cfg.Box3ESP || Cfg.Box2ESP || Cfg.SkeletonESP || Cfg.CapsuleESP ||
                Cfg.ItemBoxESP || Cfg.ItemNameESP || Cfg.InteractableBoxESP || Cfg.InteractableNameESP ||
                ESPModule.ZombieBoxESP)
            {
                try { EspUI.Draw(); }
                catch (Exception ex) { Logging.Log($"[ERR_UIM_ESP] ESP draw error: {ex.Message}", Logging.LType.Error); }
            }

            if (!DrawMenu) return;

            InitStyles();

            // Resize-Handle-Logik (vor dem Fenster)
            if (_windowVisible)
            {
                const int handleSize = 24;
                Rect resizeHandleRect = new Rect(
                    _windowRect.x + _windowRect.width - handleSize,
                    _windowRect.y + _windowRect.height - handleSize,
                    handleSize,
                    handleSize
                );

                Event e = Event.current;
                Vector2 mousePos = e.mousePosition;

                if (e.type == EventType.MouseDown && resizeHandleRect.Contains(mousePos))
                {
                    _isResizing = true;
                    _resizeStartRect = _windowRect;
                    _resizeStartMouse = mousePos;
                    e.Use();
                }
                else if (e.type == EventType.MouseDrag && _isResizing)
                {
                    Vector2 delta = mousePos - _resizeStartMouse;
                    _windowRect.width = Mathf.Max(400f, _resizeStartRect.width + delta.x);
                    _windowRect.height = Mathf.Max(300f, _resizeStartRect.height + delta.y);
                    e.Use();
                }
                else if (e.type == EventType.MouseUp && _isResizing)
                {
                    _isResizing = false;
                    e.Use();
                }
            }

            // Fenster zeichnen
            if (_windowVisible)
            {
                _windowRect = GUI.Window(12345, _windowRect, (GUI.WindowFunction)DrawMenuWindow, "", GUI.skin.box);
            }
            else
            {
                if (DrawMenu)
                {
                    _windowVisible = true;
                    CenterWindow();
                }
            }
        }

        private void CenterWindow()
        {
            _windowRect.x = (Screen.width - _windowRect.width) / 2f;
            _windowRect.y = (Screen.height - _windowRect.height) / 2f;
        }

        // ========================================================================
        // DrawMenuWindow – Fensterinhalt
        // ========================================================================
        private void DrawMenuWindow(int windowID)
        {
            GUI.DragWindow(new Rect(0, 0, _windowRect.width, 25));

            // Titel
            string title = "Title Name from Menu";
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 16,
                normal = { textColor = Color.white }
            };
            GUI.Label(new Rect(0, 5, _windowRect.width, 20), title, titleStyle);

            // Schließen-Button
            if (GUI.Button(new Rect(_windowRect.width - 25, 5, 20, 20), "X"))
            {
                DrawMenu = false;
                _windowVisible = false;
                GUI.FocusControl(null);
                return;
            }

            float tabWidth = 160f;
            float padding = 10f;
            float contentLeft = tabWidth + padding * 2;

            // Linke Spalte – Kategorien/Tabs
            GUILayout.BeginArea(new Rect(padding, 30, tabWidth, _windowRect.height - 50));
            foreach (var kvp in _pageCategories)
            {
                string category = kvp.Key;
                var pages = kvp.Value;
                if (pages.Count == 0) continue;

                GUILayout.Label("- " + category, _categoryStyle);
                foreach (var page in pages)
                {
                    bool isActive = (_activeCategory == category && _activePageIndex == pages.IndexOf(page));
                    GUIStyle style = isActive ? _tabButtonActiveStyle : _tabButtonStyle;
                    if (GUILayout.Button(page.Title, style, GUILayout.Width(tabWidth - 10)))
                    {
                        _activeCategory = category;
                        _activePageIndex = pages.IndexOf(page);
                    }
                }
                GUILayout.Space(5);
            }
            GUILayout.EndArea();

            // Rechter Bereich mit ScrollView
            Rect contentRect = new Rect(contentLeft, 30, _windowRect.width - contentLeft - padding, _windowRect.height - 50);

            Page currentPage = null;
            if (_pageCategories.TryGetValue(_activeCategory, out var activePages) && activePages.Count > _activePageIndex)
                currentPage = activePages[_activePageIndex];

            string scrollKey = currentPage?.Title ?? "default";
            if (!_scrollPositions.ContainsKey(scrollKey))
                _scrollPositions[scrollKey] = Vector2.zero;

            GUILayout.BeginArea(contentRect);
            Vector2 scrollPos = _scrollPositions[scrollKey];
            scrollPos = GUILayout.BeginScrollView(scrollPos, false, true, GUILayout.Width(contentRect.width), GUILayout.Height(contentRect.height));

            currentPage?.Render();

            GUILayout.EndScrollView();
            GUILayout.EndArea();
            _scrollPositions[scrollKey] = scrollPos;

            // Visueller Resize-Griff
            const int handleSize = 24;
            Rect resizeHandleRect = new Rect(
                _windowRect.width - handleSize,
                _windowRect.height - handleSize,
                handleSize,
                handleSize
            );
            GUI.Box(resizeHandleRect, "◢");
        }

        // ----- Unity Lifecycle -----
        public override void Awake()
        {
            base.Awake();

            _windowVisible = DrawMenu;
            UpdateCursorState();

            try { ConfigModule.Load(); }
            catch (Exception e) { Logging.Log($"[ERR_UIM_011] ConfigModule.Load failed: {e.Message}", Logging.LType.Error); }

            Color unityColor = Cfg.ActionMenuColor;
            MainThreadDispatcher.Run(() => EntryPoint.ApplyActionMenuStyling(unityColor));

            AddOrUpdatePage("Player", new MovementUI());
            AddOrUpdatePage("Player", new ExploitUI());
            AddOrUpdatePage("Visual", new CamUI());
            AddOrUpdatePage("Visual", new EspUI());
            AddOrUpdatePage("Instance", new InstanceHistoryUI());
            AddOrUpdatePage("Instance", new InstanceUI());
            AddOrUpdatePage("Misc", new PlayerListUI());
            AddOrUpdatePage("Menu", new ConfigModule());

            if (_windowVisible)
                CenterWindow();
        }

        private void Update()
        {
            // Cursor-Zustand nur bei Änderung aktualisieren
            if (DrawMenu && _windowVisible)
            {
                if (Cursor.lockState != CursorLockMode.None || !Cursor.visible)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            }
            else
            {
                if (Cursor.lockState != CursorLockMode.Locked || Cursor.visible)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }

            // Toggle mit Insert
            if (Input.GetKeyDown(KeyCode.Insert))
            {
                DrawMenu = !DrawMenu;
                _windowVisible = DrawMenu;
                Logging.Log($"Menu toggled via Insert: {DrawMenu}", Logging.LType.Info);
                if (_windowVisible)
                    CenterWindow();
            }
        }

        private void UpdateCursorState()
        {
            if (_windowVisible)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void OnDestroy()
        {
            // Gecachte Texturen freigeben
            WatermarkRenderer.DestroyResources();
            if (_activeTabBackground != null)
            {
                Destroy(_activeTabBackground);
                _activeTabBackground = null;
            }
        }
    }
}