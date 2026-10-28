using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRClient.IMGUI;
using VRClient.Menu;

namespace VRClient.SDK.VRCs
{
    internal class Menu
    {
        private static void SetTitle(string title)
        {
            var report = GameObject.Find($"Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/QMParent/Menu_QM_ReportHere/QMHeader_H1/LeftItemContainer/Text_Title");
            if (report == null) return;
            var headerText = report.transform.GetComponent<TextMeshProUGUI>();
            if (headerText != null) headerText.text = title;
        }

        private static void SetIcon(Sprite icon)
        {
            var report = GameObject.Find($"Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/QMParent/Menu_QM_Launchpad/Header_H1/RightItemContainer/Button_QM_Report/Icon");
            if (report == null) return;
            var headerText = report.transform.GetComponent<Image>();
            if (headerText != null) headerText.sprite = icon;
        }

        // --------------------------------------------------------------------

        public static GameObject ModifyButton(string initialText, GameObject parent, Action<TextMeshProUGUI> action, string buttonName)
        {
            string path = $"Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/QMParent/Menu_QM_ReportHere/ButtonSection/VerticalLayoutGroup/Button_{buttonName}";
            GameObject buttonObj = GameObject.Find(path);
            if (buttonObj == null) { /* Fehler-Log */ return null; }

            // Den Button_Text&Icon finden
            Transform child = buttonObj.transform.Find("Button_Text&Icon");
            if (child == null) { /* Fehler-Log */ return null; }

            Transform icon = child.Find("Icon");
            if (icon != null) icon.gameObject.SetActive(false);

            var special = child.GetComponent<ButtonPublicIMoveHandlerIEventSystemHandlerIPointerDownHandlerIPointerUpHandlerIPointerEnterHandlerIPointerExitHandlerISelectHandlerIDeselectHandlerOb_tIm_i_bImStOb_t_sUnique>();
            if (special == null) { /* Fehler-Log */ return null; }

            TextMeshProUGUI textComp = child.Find("Text_ButtonName")?.GetComponent<TextMeshProUGUI>();
            if (textComp != null) textComp.text = initialText;

            special.interactable = true;
            var ev = new UnityEngine.UI.Button.ButtonClickedEvent();

            ev.AddListener((UnityEngine.Events.UnityAction)(() => action(textComp)));

            special.onClick = ev;
            return buttonObj;
        }

        public static void CreateCheatsMenu()
        {
            var player = Networking.LocalPlayer;
            GameObject leftScroll = GameObject.Find("Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/QMParent/Menu_QM_ReportHere/ButtonSection");
            if (leftScroll == null) return;

            GameObject disable_btn = GameObject.Find("Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/QMParent/Menu_QM_ReportHere/ButtonSection/Button_ViewReports");
            if (leftScroll == null) return;
            GameObject.Destroy(disable_btn);

            SetIcon(UIUtils.Logo);
            SetTitle("Tevvez Cheats | Simple UI");

            ModifyButton($"Flight : {(Cfg.Fly ? "ON" : "OFF")}", leftScroll, (textComp) => {
                Cfg.Fly = !Cfg.Fly;
                MovementModule.ToggleFly(Cfg.Fly);
                if (textComp != null) textComp.text = $"Flight : {(Cfg.Fly ? "ON" : "OFF")}";
            }, "ReportUser");

            ModifyButton($"ESP : {(Cfg.CapsuleESP ? "ON" : "OFF")}", leftScroll, (textComp) => {
                Cfg.CapsuleESP = !Cfg.CapsuleESP;
                if (textComp != null) textComp.text = $"ESP : {(Cfg.CapsuleESP ? "ON" : "OFF")}";
                MainThreadDispatcher.Run(() => PopupUtils.HudMessage("Tevvez Cheats", $"ESP : {(Cfg.CapsuleESP ? "ON" : "OFF")}", 5f));
            }, "ReportAvatar");

            ModifyButton($"BunnyHop : {(Cfg.BunnyHop ? "ON" : "OFF")}", leftScroll, (textComp) => {
                Cfg.BunnyHop = !Cfg.BunnyHop;
                if (textComp != null) textComp.text = $"BunnyHop : {(Cfg.BunnyHop ? "ON" : "OFF")}";
                MainThreadDispatcher.Run(() => PopupUtils.HudMessage("Tevvez Cheats", $"Bunny Hop : {(Cfg.BunnyHop ? "ON" : "OFF")}", 5f));
            }, "ReportWorld");

            ModifyButton($"Auto Step : {(Cfg.StepHigh ? "ON" : "OFF")}", leftScroll, (textComp) => {
                Cfg.StepHigh = !Cfg.StepHigh;
                if (textComp != null) textComp.text = $"Auto Step : {(Cfg.StepHigh ? "ON" : "OFF")}";
                MainThreadDispatcher.Run(() => PopupUtils.HudMessage("Tevvez Cheats", $"Auto Step : {(Cfg.StepHigh ? "ON" : "OFF")}", 5f));
            }, "ReportGroup");

            ModifyButton($"Test", leftScroll, (textComp) => {
                if (textComp != null) textComp.text = $"Test";
                MainThreadDispatcher.Run(() => PopupUtils.HudMessage("Tevvez Cheats", $"Test", 5f));
            }, "ReportInstance");

            ModifyButton($"Spoof Master : {(Cfg.MasterSpoof ? "ON" : "OFF")}", leftScroll, (textComp) => {
                Cfg.MasterSpoof = !Cfg.MasterSpoof;
                if (textComp != null) textComp.text = $"Spoof Master : {(Cfg.MasterSpoof ? "ON" : "OFF")}";
                MainThreadDispatcher.Run(() => PopupUtils.HudMessage("Tevvez Cheats", $"Spoof Master : {(Cfg.MasterSpoof ? "ON" : "OFF")}", 5f));
            }, "ReportSticker");

            ModifyButton($"Spoof World : {(Cfg.OwnerSpoof ? "ON" : "OFF")}", leftScroll, (textComp) => {
                Cfg.OwnerSpoof = !Cfg.OwnerSpoof;
                if (textComp != null) textComp.text = $"Spoof World : {(Cfg.OwnerSpoof ? "ON" : "OFF")}";
                MainThreadDispatcher.Run(() => PopupUtils.HudMessage("Tevvez Cheats", $"Spoof World : {(Cfg.OwnerSpoof ? "ON" : "OFF")}", 5f));
            }, "ReportEmoji");

            ModifyButton($"Udon Nuke", leftScroll, (textComp) => {
                CoroutineHelper.Start(Modules.Exploits.UdonModule.UdonNuke());
                MainThreadDispatcher.Run(() => PopupUtils.HudMessage("Tevvez Cheats", $"Udon Nuked", 5f));
            }, "ReportPrint");
        }
    }
}