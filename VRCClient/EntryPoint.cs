using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using BepInEx.Unity.IL2CPP.Utils;
using HarmonyLib;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRC.Core;
using VRC.UI.Elements;
using VRClient.IMGUI;
using VRClient.Menu;
using VRClient.Modules;
using VRClient.SDK;
using VRClient.SDK.Patching;
using VRClient.SDK.VRCs;
using WorldAPI;
using WorldAPI.ButtonAPI;
using WorldAPI.ButtonAPI.Buttons;
using WorldAPI.ButtonAPI.Groups;

namespace VRClient
{
    [BepInPlugin("com.v.VRCClientMod", "https://t.me/+Ba98xQhu8MIzMjBi", "1.0.0")]
    public class EntryPoint : BasePlugin
    {
        internal static ManualLogSource _Log { get; private set; }
        internal static Harmony _Harmony { get; } = new Harmony("TevvezCheats");
        internal static string CurrentModVersion = "0.0.0.2";

        internal static Process GameProc { get; set; }
        internal static string GameName { get; set; }

        private GameObject _managerGameObject;
        private EventManager _eventManager;
        private List<Type> _moduleTypes;

        internal static IntPtr handle;

        // HttpClient als static readonly – korrekt für langlebige Anwendungen
        private static readonly HttpClient _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        internal static readonly string PluginPath = Path.Combine("BepInEx", "plugins");

        // ------------------------------------------------------------------------
        // Haupt-Einstiegspunkt (von TevvezPlugin)
        // ------------------------------------------------------------------------
        public override void Load()
        {
            _Log = Log;
            Logging.Log("[ERR_ENT_000] TevvezCheats: Load() started", Logging.LType.Login);

            // 2. CoreLimiterPatch (falls vorhanden)
            try
            {
                CoreLimiterPatch.Start();
            }
            catch (Exception ex)
            {
                Logging.Log($"[ERR_ENT_002] CoreLimiterPatch.Start failed: {ex.Message}", Logging.LType.Error);
            }

            // 3. Modul-Typen sammeln
            _moduleTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(BaseModule)))
                .ToList();
            Logging.Log($"[ERR_ENT_003] Found {_moduleTypes.Count} module types", Logging.LType.Info);

            // 4. Manager-GameObject erstellen (DontDestroyOnLoad)
            _managerGameObject = new GameObject("TevvezCheats_Manager");
            UnityEngine.Object.DontDestroyOnLoad(_managerGameObject);
            _managerGameObject.hideFlags |= HideFlags.HideAndDontSave;

            // 5. IL2CPP-Komponenten registrieren
            try
            {
                ClassInjector.RegisterTypeInIl2Cpp<EventManager>();
                ClassInjector.RegisterTypeInIl2Cpp<SINameplate>();
                _eventManager = _managerGameObject.AddComponent<EventManager>();
            }
            catch (Exception ex)
            {
                Logging.Log($"[ERR_ENT_004] Failed to register EventManager/SINameplate: {ex.Message}", Logging.LType.Error);
            }

            // 6. Alle Module in IL2CPP registrieren und als Komponente hinzufügen
            foreach (var type in _moduleTypes)
            {
                try
                {
                    ClassInjector.RegisterTypeInIl2Cpp(type);
                    Logging.Log($"[ERR_ENT_012] Registered module type: {type.Name}", Logging.LType.Info);

                    var component = _managerGameObject.AddComponent(Il2CppType.From(type));
                    Logging.Log($"[ERR_ENT_056] Module {type.Name} added as component.", Logging.LType.Info);
                }
                catch (Exception ex)
                {
                    Logging.Log($"[ERR_ENT_013] Failed to register or add {type.Name}: {ex.Message}", Logging.LType.Error);
                }
            }

            // 7. Harmony-Patches anwenden
            try
            {
                HistoryPersistence.LoadHistory();
                _Harmony.PatchAll();
                Logging.Log("[ERR_ENT_005] Harmony patches applied", Logging.LType.Info);
            }
            catch (Exception ex)
            {
                Logging.Log($"[ERR_ENT_006] Harmony patching failed: {ex.Message}", Logging.LType.Error);
            }

            // 8. Verzögerte Initialisierung starten (Coroutine)
            _eventManager.StartCoroutine(DelayedInitializationCoroutine());

            Logging.Log("[ERR_ENT_007] Load() finished – waiting for game to be ready", Logging.LType.Info);
        }

        // ------------------------------------------------------------------------
        // Coroutine: Wartet auf Benutzer & Menü, startet dann UI-Init
        // ------------------------------------------------------------------------
        private IEnumerator DelayedInitializationCoroutine()
        {
            yield return null;

            // Warten auf APIUser.CurrentUser
            int waitCycles = 0;
            while (APIUser.CurrentUser == null)
            {
                if (waitCycles++ > 600)
                {
                    Logging.Log("[ERR_ENT_008] Timeout waiting for APIUser.CurrentUser", Logging.LType.Warning);
                    break;
                }
                yield return new WaitForEndOfFrame();
            }

            if (APIUser.CurrentUser == null)
                Logging.Log("[ERR_ENT_009] APIUser.CurrentUser is still null – proceeding with limited functionality", Logging.LType.Warning);
            else
                Logging.Log($"[ERR_ENT_010] User {APIUser.CurrentUser.displayName} logged in", Logging.LType.Info);

            // Warten auf QuickMenu und Kamera
            while (Camera.main == null || GameObject.Find("Canvas_QuickMenu(Clone)") == null)
                yield return new WaitForEndOfFrame();

            Logging.Log("[ERR_ENT_011] QuickMenu found, starting UI initialization...", Logging.LType.Info);

            _eventManager.StartCoroutine(UiInitializationCoroutine());
        }

        // ------------------------------------------------------------------------
        // Coroutine: UI-Erstellung, Input-Setup, Hintergrund-Task starten
        // ------------------------------------------------------------------------
        private IEnumerator UiInitializationCoroutine()
        {
            // Warten bis MainMenu existiert
            while (GameObject.Find("Canvas_MainMenu(Clone)") == null)
                yield return new WaitForEndOfFrame();

            CreateGhostModeHudIcon();

            // Cheats-Menü bauen
            try
            {
                InitUI();
                Logging.Log("[ERR_ENT_028] Cheats menu created", Logging.LType.Info);
            }
            catch (Exception ex)
            {
                Logging.Log($"[ERR_ENT_029] CreateCheatsMenu failed: {ex.Message}", Logging.LType.Error);
            }

            // InputHelper initialisieren
            try
            {
                InputHelper.Setup();
                Logging.Log("[ERR_ENT_030] InputHelper setup complete", Logging.LType.Info);
            }
            catch (Exception e)
            {
                Logging.Log($"[ERR_ENT_031] InputHelper setup failed: {e.Message}", Logging.LType.Error);
            }

            Logging.Log("[ERR_ENT_032] UI initialization complete.", Logging.LType.Info);

            ThirdPerson.Initialize();
            RamCleaner.StartMyCleaner();
        }

        // ------------------------------------------------------------------------
        // UI-Erstellung (unverändert, nur Methodenname angepasst)
        // ------------------------------------------------------------------------
        public VRCPage Main;
        public ButtonGroup RootGrp;
        public Transform QMDashContent;
        public Transform VRCUserSelMenu;
        public ButtonGroup UserSelGrp;

        public void InitUI()
        {
            SetQuickMenuTitle("TevvezCheats");

            var page = Main = new VRCPage("TevvezCheats");

            new Tab(page, "Tevvez", UIUtils.Logo);

            var generalPage = new VRCPage("General");
            var qualityOfLifePage = new VRCPage("Quality of Life");
            var movementPage = new VRCPage("Movement");
            var visualPage = new VRCPage("Visual");
            var networkPage = new VRCPage("Network");
            var gamesPage = new VRCPage("Games");

            var mainGroup = RootGrp = new ButtonGroup(page, "Main Menu", true);
            new VRCButton(mainGroup, "General", "Open general settings", generalPage.OpenMenu);
            new VRCButton(mainGroup, "Quality of Life", "Open QoL settings", qualityOfLifePage.OpenMenu);
            new VRCButton(mainGroup, "Movement", "Open movement modifiers", movementPage.OpenMenu);
            new VRCButton(mainGroup, "Visual", "Open ESP and visual tweaks", visualPage.OpenMenu);
            new VRCButton(mainGroup, "Network", "Open network and Udon settings", networkPage.OpenMenu);
            new VRCButton(mainGroup, "Games", "Open game-specific mods", gamesPage.OpenMenu);

            var grpNetwork = new ButtonGroup(networkPage, "Network/Udon", true);
        }

        // ------------------------------------------------------------------------
        // QuickMenu-Titel setzen
        // ------------------------------------------------------------------------
        private void SetQuickMenuTitle(string title)
        {
            try
            {
                var titleObj = GameObject.Find("Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/QMParent/Menu_QM_Launchpad/Header_H1/LeftItemContainer/Text_Title");
                if (titleObj != null)
                {
                    var text = titleObj.GetComponent<TextMeshProUGUIEx>();
                    if (text != null)
                    {
                        text.prop_String_0 = title;
                        text.enableAutoSizing = true;
                        Logging.Log($"[ERR_ENT_033] QuickMenu title set to '{title}'", Logging.LType.Info);
                    }
                    else
                        Logging.Log("[ERR_ENT_034] TextMeshProUGUIEx component missing on title object", Logging.LType.Warning);
                }
                else
                    Logging.Log("[ERR_ENT_035] Title object not found", Logging.LType.Warning);
            }
            catch (Exception e)
            {
                Logging.Log($"[ERR_ENT_036] SetQuickMenuTitle error: {e.Message}", Logging.LType.Error);
            }
        }

        // ------------------------------------------------------------------------
        // ActionMenu-Styling
        // ------------------------------------------------------------------------
        public static void ApplyActionMenuStyling(Color color)
        {
            try
            {
                var bg = GameObject.Find("UnscaledUI/ActionMenu/Container/MenuR/ActionMenu/Main/Background");
                if (bg != null)
                {
                    var pg = bg.GetComponent<PedalGraphic>();
                    if (pg != null) pg.color = color;
                }

                var center = GameObject.Find("UnscaledUI/ActionMenu/Container/MenuR/ActionMenu/Main/Center");
                if (center != null)
                {
                    var pg = center.GetComponent<PedalGraphic>();
                    if (pg != null)
                    {
                        pg.field_Public_Single_3 = 360f;
                        pg.color = color;
                    }
                }
            }
            catch (Exception e)
            {
                Logging.Log($"[ERR_ENT_037] ApplyActionMenuStyling error: {e.Message}", Logging.LType.Error);
            }
        }

        // ------------------------------------------------------------------------
        // GhostMode-HUD-Icon erstellen
        // ------------------------------------------------------------------------
        private void CreateGhostModeHudIcon()
        {
            try
            {
                var uiManager = VRCUiManager.field_Private_Static_VRCUiManager_0;
                if (uiManager == null)
                {
                    Logging.Log("[ERR_ENT_038] VRCUiManager instance is null – cannot create GhostModeHudIcon", Logging.LType.Warning);
                    return;
                }

                var original = uiManager.transform.Find(
                    "UnscaledUI/HudContent/HUD_UI 2(Clone)/VR Canvas/Container/Left/Icons/Dynamic Icons/Generic & Notification Icons/Notification Icons/Friend Request");
                if (original == null)
                {
                    Logging.Log("[ERR_ENT_039] Friend Request icon not found – cannot create Ghost icon", Logging.LType.Warning);
                    return;
                }

                var newIcon = UnityEngine.Object.Instantiate(original, original.parent);
                newIcon.name = $"icon_{Guid.NewGuid()}";
                UnityEngine.Object.Destroy(newIcon.GetComponent<HudIcon_Notifications>());
                newIcon.localPosition = new Vector3(-73.7875f, 90.9029f, -44.1711f);

                var child = newIcon.GetChild(0);
                if (child != null)
                {
                    var img = child.GetComponent<Image>();
                    if (img != null)
                    {
                        img.sprite = UIUtils.Ghost;
                        UIUtils.GhostModeHud = child;
                        Logging.Log("[ERR_ENT_040] GhostModeHudIcon created", Logging.LType.Info);
                    }
                    else
                        Logging.Log("[ERR_ENT_041] Image component missing on Ghost icon child", Logging.LType.Warning);
                }
                else
                    Logging.Log("[ERR_ENT_042] Ghost icon has no child object", Logging.LType.Warning);
            }
            catch (Exception e)
            {
                Logging.Log($"[ERR_ENT_043] CreateGhostModeHudIcon error: {e.Message}", Logging.LType.Error);
            }
        }

        // ======================================================================
        // Anwendung hart beenden (Validierungsfehler)
        // ======================================================================
        private void TerminateApplication()
        {
            Logging.Log("[ERR_ENT_052] Terminating due to validation failure.", Logging.LType.Error);
            Application.Quit();
            Environment.Exit(-1);
        }
    }
}