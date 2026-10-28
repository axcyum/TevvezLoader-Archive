using System;
//using Il2Cpp;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VRC.UI.Core.Styles;
using VRC.UI.Elements.Controls;
using WorldAPI.ButtonAPI.Controls;
using WorldAPI.ButtonAPI.Extras;

namespace WorldAPI.ButtonAPI;

public class Tab : ExtentedControl {
    public GameObject badgeGameObject { get; private set; }
    public TextMeshProUGUI badgeText { get; private set; }
    public Image tabIcon { get; private set; }
    public Button Button { get; private set; }

    public VRCPage Menu { get; private set; }

    private StyleElement element;

    public Tab(VRCPage menu, string tooltip, Sprite icon = null, Transform parent = null, bool openMenu = true)
    {
        if (!APIBase.IsReady()) throw new Exception();

        if (parent == null)
            parent = APIBase.Tab.parent;
        Menu = menu;

        (gameObject = UnityEngine.Object.Instantiate(APIBase.Tab.gameObject, parent)).name = menu.MenuName + "_Tab";
        (tabIcon = gameObject.transform.Find("Icon").GetComponent<Image>()).overrideSprite = icon;
        (element = gameObject.GetComponent<StyleElement>()).field_Private_Selectable_0 = (Button = gameObject.GetComponent<Button>());
        (Button.onClick = new()).AddListener((Action)delegate {
            gameObject.GetComponent<StyleElement>().field_Private_Selectable_0 = Button;
            if (openMenu) Menu?.OpenMenu();
            onClickAction?.Invoke();
        });
        var MenuTab = Button.GetComponent<Button1PublicObSt_c_sSt_cBoStStStUnique>();
        //Button.GetComponent<Button1PublicObSt_c_sSt_cBoStStStUnique>()._controlName = menu.MenuName; //disabled 4 naow sowwy
        MenuTab._controlName = "QuickMenuDevTools";
        //MenuTab._controlName = menu.MenuName;
        MenuTab._sendAnalytics = false;
        MenuTab.field_Private_MenuStateController_0 = QMUtils.GetMainMenuStateControllerInstance;

        badgeGameObject = gameObject.transform.GetChild(0).gameObject;
        badgeText = badgeGameObject.GetComponentInChildren<TextMeshProUGUI>();

        SetToolTip(tooltip);

        gameObject.active = true;
    }

}
