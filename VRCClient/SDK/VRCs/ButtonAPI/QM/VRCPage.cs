using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRC.UI.Core.Styles;
using VRC.UI.Elements;
using WorldAPI.ButtonAPI.Extras;
using WorldAPI.ButtonAPI.QM.Controls;
using UnityEngine.Events;
using VRC.Localization;
using Object = UnityEngine.Object;

namespace WorldAPI.ButtonAPI;

public class VRCPage : WorldPage
{
    public bool IsRoot { get; set; } // This should be fine to edit
    public static Stack<int> pageStack = new();
    public static VRCPage lastOpenedPage { get; private set; }

    public Action BackButtonPress;
    public TextMeshProUGUI pageTitleText;
    public RectMask2D menuMask;

    internal GameObject extButtonGameObject;

    public VRCPage(string pageTitle, bool root = false, bool backButton = true, bool expandButton = false, Action expandButtonAction = null, string expandButtonTooltip = "", Sprite expandButtonSprite = null, bool preserveColor = false, string parentMenuName = "QuickMenuDashboard")
    {
        if (!APIBase.IsReady()) throw new Exception();
        if (APIBase.MenuPage == null)
        {
            //Logs.Error("Fatal Error: ButtonAPI.menuPageBase Is Null!");
            return;
        }

        var region = 0;
        MenuName = $"WorldMenu_{pageTitle}_{Guid.NewGuid()}";
        IsRoot = root;

        try
        {


            var gameObject = Object.Instantiate(APIBase.MenuPage, APIBase.MenuPage.transform.parent);
            gameObject.name = MenuName;
            gameObject.transform.SetSiblingIndex(9); //Changed from 6 to be put 2 game objects after the camera menu (unnecessary but im doing it anyways)
            gameObject.gameObject.active = false;

            region++;
            //Object.DestroyImmediate(gameObject.GetOrAddComponent<Il2CppVRC.UI.Elements.Menus.MainMenuContent>()); //changed to remove the Camera menu stuff instead of the launchpad menu stuff
            Object.DestroyImmediate(gameObject.GetOrAddComponent<MonoBehaviour1PublicBuExTaBuToEnIcBuEnGaUnique>());
            (Page = gameObject.gameObject.AddComponent<UIPage>()).field_Public_String_0 = MenuName;
            region++;

            Page.field_Private_Boolean_1 = true;
            //Page.field_Protected_MenuStateController_0 = QMUtils.GetMenuStateControllerInstance;
            //Page.field_Private_List_1_UIPage_0 = new Il2CppSystem.Collections.Generic.List<UIPage>();
            //Page.field_Private_List_1_UIPage_0.Add(Page);

            region++;
            //QMUtils.GetMenuStateControllerInstance.field_Private_Dictionary_2_String_UIPage_0.Add(MenuName, Page);
            
            QMUtils.GetMenuStateControllerInstance.field_Private_IUIPageFactory_0.Cast<UIPageFactory>()?.field_Private_Dictionary_2_String_IUIPage_0.Add(MenuName, Page.Cast<IUIPage>());
            QMUtils.GetMenuStateControllerInstance.field_Private_IUIPageFactory_0.Cast<UIPageFactory>()?.field_Private_List_1_IUIPage_0.Add(Page.TryCast<IUIPage>());
            /*if (root)
            {
                var list = QMUtils.GetMenuStateControllerInstance.field_Public_Il2CppReferenceArray_1_UIPage_0.ToList();
                list.Add(Page);
                QMUtils.GetMenuStateControllerInstance.field_Public_Il2CppReferenceArray_1_UIPage_0 = list.ToArray();
            }*/
            if (root)
            {
                //QMUtils.GetMenuStateControllerInstance.field_Public_Il2CppReferenceArray_1_UIPage_0.ToList();
                //list.Add(Page);
                //QMUtils.GetMenuStateControllerInstance.field_Public_Il2CppReferenceArray_1_UIPage_0 = list.ToArray();
            }
            var list = QMUtils.GetMenuStateControllerInstance.field_Public_Il2CppReferenceArray_1_UIPage_0.ToList();
            list.Add(Page);
            QMUtils.GetMenuStateControllerInstance.field_Public_Il2CppReferenceArray_1_UIPage_0 = list.ToArray();
            region++;

            (MenuContents = gameObject.transform.Find("Scrollrect/Viewport/VerticalLayoutGroup")).GetComponent<HorizontalOrVerticalLayoutGroup>().childControlHeight = true;
            MenuContents.DestroyChildren();

            region++;
            (pageTitleText = gameObject.Find("Header_Camera/LeftItemContainer/Text_Title").GetComponent<TextMeshProUGUI>()).fontSize = 54.7f;
            pageTitleText.text = pageTitle;
            pageTitleText.richText = true;
            region++;

            /*var backButtonGameObject = gameObject.transform.GetChild(0).Find("LeftItemContainer/Button_Back").gameObject;
            backButtonGameObject.SetActive(backButton);
            (backButtonGameObject.GetComponentInChildren<Button>().onClick = new Button.ButtonClickedEvent()).AddListener(new Action(() =>
            {
                if (IsRoot)
                    //QMUtils.GetMenuStateControllerInstance?.Method_Public_Void_String_UIContext_Boolean_EnumPublicSealedvaNoLeRiBoIn6vUnique_0("QuickMenuDashboard", null, false, EnumPublicSealedvaNoLeRiBoIn6vUnique.Right);
                    QMUtils.GetMenuStateControllerInstance?.Method_Public_Void_String_UIContext_Boolean_TransitionType_0("QuickMenuDashboard", null, false, TransitionType.Right);
                else
                    QMUtils.GetMenuStateControllerInstance?.Method_Public_Void_String_UIContext_Boolean_TransitionType_0(parentMenuName, null, false, TransitionType.Right); // stupid workaround
                BackButtonPress?.Invoke();
            }));*/
            var backButtonGameObject = gameObject.transform.GetChild(0).Find("LeftItemContainer/Button_Back").gameObject;
            backButtonGameObject.SetActive(backButton);
            (backButtonGameObject.GetComponentInChildren<Button>().onClick = new Button.ButtonClickedEvent()).AddListener(new Action(() =>
            {
                if (pageStack.Count > 1)
                {
                    pageStack.Pop();
                    QMUtils.GetMenuStateControllerInstance.Method_Private_Void_Int32_UIContext_Boolean_Boolean_0(pageStack.Peek(), null, true);
                }
                else
                    QMUtils.GetMenuStateControllerInstance.Method_Private_Void_Int32_UIContext_Boolean_Boolean_0(0, null, true);
                BackButtonPress?.Invoke();
            }));

            region++;
            (extButtonGameObject = gameObject.transform.GetChild(0).Find("RightItemContainer/Button_QM_Expand").gameObject).SetActive(expandButton);
            extButtonGameObject.GetComponentInChildren<Button>().onClick = new Button.ButtonClickedEvent();
            if (expandButtonAction != null)
                extButtonGameObject.GetComponentInChildren<Button>().onClick.AddListener(expandButtonAction);
            for (int i = 0; i < gameObject.transform.GetChild(0).Find("RightItemContainer").childCount; i++)
            {
                var ch = gameObject.transform.GetChild(0).Find("RightItemContainer").GetChild(i);
                if (ch.name != "Button_QM_Expand")
                    ch.gameObject.SetActive(false);
            }

            if (expandButtonSprite != null)
            {
                extButtonGameObject.GetComponentInChildren<Image>().sprite = expandButtonSprite;
                extButtonGameObject.GetComponentInChildren<Image>().overrideSprite = expandButtonSprite;
                if (preserveColor)
                {
                    extButtonGameObject.GetComponentInChildren<Image>().color = Color.white;
                    extButtonGameObject.GetComponentInChildren<StyleElement>(true).enabled = false;
                }
            }
            region++;

            (menuMask = MenuContents.parent.gameObject.GetOrAddComponent<VRCRectMask2D>()).enabled = true;
            gameObject.transform.Find("Scrollrect").GetOrAddComponent<VRCScrollRect>().enabled = true;
            gameObject.transform.Find("Scrollrect").GetOrAddComponent<ScrollRect>().verticalScrollbar = gameObject.transform.Find("Scrollrect/Scrollbar").GetOrAddComponent<Scrollbar>();
            gameObject.transform.Find("Scrollrect").GetOrAddComponent<ScrollRect>().verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
            gameObject.DestroyChildren(where => where.name != "Scrollrect" && where.name != "Header_Camera");
            region++;

            gameObject.transform.Find("Scrollrect/Viewport").GetComponent<VRCRectMask2D>().prop_Boolean_0 = true; // Fixes the items falling off of the QM
            gameObject.transform.Find("Scrollrect").GetComponent<VRCScrollRect>().field_Public_Boolean_0 = true; // Fixes the items falling off of the QM

            region++;
            Page.GetComponent<Canvas>().enabled = true; // Fix for Late Menu Creation
            Page.GetComponent<CanvasGroup>().enabled = true; // Fix for Late Menu Creation
            Page.GetComponent<UIPage>().enabled = true; // Fix for Late Menu Creation
            Page.GetComponent<GraphicRaycaster>().enabled = true; // Fix for Late Menu Creation
        }
        catch (System.Exception ex)
        {
            throw new Exception("Exception Caught When Making Page At Region: " + region + "\n\n" + ex);
        }
    }

    public void AddExtButton(UnityAction onClick, string tooltip, Sprite icon)
    {
        var obj = UnityEngine.Object.Instantiate(extButtonGameObject, extButtonGameObject.transform.parent);
        obj.transform.SetSiblingIndex(0);
        obj.SetActive(true);
        obj.GetComponentInChildren<Button>().onClick = new Button.ButtonClickedEvent();
        obj.GetComponentInChildren<Button>().onClick.AddListener(onClick);
        obj.GetComponent<ToolTip>()._localizableString = tooltip.Localize();
        obj.GetComponentInChildren<Image>().sprite = icon;
        obj.GetComponentInChildren<Image>().overrideSprite = icon;
    }
    public void OpenMenu()
    {
        int temp = 0;
        try
        {
            /*temp++;
            Page.gameObject.active = true;
            temp++;
            //QMUtils.GetMenuStateControllerInstance.Method_Public_Void_String_UIContext_Boolean_TransitionType_0(Page.field_Public_String_0, null, false, UIPage.TransitionType.Right);


            QMUtils.GetMenuStateControllerInstance?.Method_Public_Void_String_UIContext_Boolean_TransitionType_0(Page.field_Public_String_0, null, false, TransitionType.Right);
            QMUtils.GetMenuStateControllerInstance?.Method_Public_Void_String_UIContext_Boolean_Boolean_0(Page.field_Public_String_0, null, true, false);
            QMUtils.GetMenuStateControllerInstance?.Method_Public_Void_IUIPage_UIContext_Boolean_TransitionType_0(Page.Cast<IUIPage>());
            Page.gameObject.GetComponent<Canvas>().enabled = true;
            Page.gameObject.GetComponent<CanvasGroup>().enabled = true;
            Page.gameObject.GetComponent<GraphicRaycaster>().enabled = true; //thanks fuckass
            temp++;
            OnMenuOpen?.Invoke();
            temp++;
            lastOpenedPage = this;
            temp++;*/
            Page.gameObject.active = true;
            var pageList = QMUtils.GetMenuStateControllerInstance.field_Public_Il2CppReferenceArray_1_UIPage_0;
            int index = 0;
            for (int i = 0; i < pageList.Count; i++)            
                if (pageList[i].field_Public_String_0.Equals(Page.field_Public_String_0))
                    index = i;

            pageStack.Push(index);
            QMUtils.GetMenuStateControllerInstance.Method_Private_Void_Int32_UIContext_Boolean_Boolean_0(index, null, true);
            OnMenuOpen?.Invoke();
            lastOpenedPage = this;
        }
        catch (System.Exception ex)
        {
            //throw new Exception("Exception Caught When Opening Page\n\n" + ex + "   " + temp);
        }
    }

    public void SetTitle(string text) => pageTitleText.text = text;
    public void CloseMenu() => Page.Method_Protected_Virtual_New_Void_0();
    public void SetExpandButtonActive(bool active) => extButtonGameObject?.SetActive(active);

    public void SetExpandButtonAction(UnityAction action)
    {
        if (extButtonGameObject != null && extButtonGameObject.GetComponentInChildren<Button>() != null)
        {
            extButtonGameObject.GetComponentInChildren<Button>().onClick = new Button.ButtonClickedEvent();
            extButtonGameObject.GetComponentInChildren<Button>().onClick.AddListener(action);
        }
    }

    public void SetExpandButtonTooltip(string tooltip)
    {
        if (extButtonGameObject != null && extButtonGameObject.GetComponentInChildren<ToolTip>() != null)
            extButtonGameObject.GetComponentInChildren<ToolTip>()._localizableString = tooltip.Localize();
    }

    public void SetExpandButtonSprite(Sprite sprite)
    {
        if (extButtonGameObject != null && extButtonGameObject.GetComponentInChildren<Image>() != null)
        {
            extButtonGameObject.GetComponentInChildren<Image>().sprite = sprite;
            extButtonGameObject.GetComponentInChildren<Image>().overrideSprite = sprite;
        }

    }
}



/*
namespace WorldAPI.ButtonAPI;

public class VRCPage : WorldPage {
    public bool IsRoot { get; set; } // This should be fine to edit
    public static VRCPage lastOpenedPage { get; private set; }

    public Action BackButtonPress;
    public TextMeshProUGUI pageTitleText;
    public RectMask2D menuMask;

    private GameObject extButtonGameObject;

    public VRCPage(string pageTitle, bool root = false, bool backButton = true, bool expandButton = false, Action expandButtonAction = null, string expandButtonTooltip = "", Sprite expandButtonSprite = null, bool preserveColor = false, bool fix = true)
    {
        if (!APIBase.IsReady()) throw new Exception();
        if (APIBase.MenuPage == null) {
            Logs.Error("Fatal Error: ButtonAPI.menuPageBase Is Null!");
            return;
        }

        var region = 0;
        MenuName = "WorldMenu_" + pageTitle + Guid.NewGuid();
        IsRoot = root;

        try {
            var gameObject = Object.Instantiate(APIBase.MenuPage, APIBase.MenuPage.transform.parent);
            gameObject.name = MenuName;
            gameObject.transform.SetSiblingIndex(5);
            gameObject.gameObject.active = false;

            region++;
            //Object.DestroyImmediate(gameObject.GetOrAddComponent<VRC.UI.Elements.Menus.MainMenuContent>());
            Page = gameObject.gameObject.AddComponent<UIPage>();
            region++;

            Page.field_Public_String_0 = MenuName;
            Page.field_Private_Boolean_1 = true;
            //Page.field_Protected_MenuStateController_0 = QMUtils.GetMenuStateControllerInstance;
            //Page.field_Private_List_1_UIPage_0 = new Il2CppSystem.Collections.Generic.List<UIPage>();
            //Page.field_Private_List_1_UIPage_0.Add(Page);
            QMUtils.GetMenuStateControllerInstance?.Method_Public_Void_String_UIContext_Boolean_EnumPublicSealedvaNoLeRiBoIn6vUnique_0(Page.field_Public_String_0, null, false, EnumPublicSealedvaNoLeRiBoIn6vUnique.Right);

            region++;
            //QMUtils.GetMenuStateControllerInstance.field_Private_Dictionary_2_String_UIPage_0.Add(MenuName, Page);
            if (root) {
                var list = QMUtils.GetMenuStateControllerInstance.field_Public_Il2CppReferenceArray_1_UIPage_0.ToList();
                list.Add(Page);
                QMUtils.GetMenuStateControllerInstance.field_Public_Il2CppReferenceArray_1_UIPage_0 = list.ToArray();
            }
            region++;

            MenuContents = gameObject.transform.Find("ScrollRect/Viewport/VerticalLayoutGroup");
            MenuContents.GetComponent<VerticalLayoutGroup>().childControlHeight = true;
            MenuContents.DestroyChildren();

            region++;
            pageTitleText = gameObject.GetComponentInChildren<TextMeshProUGUI>(true);
            pageTitleText.fontSize = 54.7f;
            pageTitleText.text = pageTitle;
            pageTitleText.richText = true;
            region++;

            var backButtonGameObject = gameObject.transform.GetChild(0).Find("LeftItemContainer/Button_Back").gameObject;
            backButtonGameObject.SetActive(backButton);
            (backButtonGameObject.GetComponentInChildren<Button>().onClick = new Button.ButtonClickedEvent()).AddListener(new Action(() => {
                if (IsRoot) QMUtils.GetMenuStateControllerInstance.Method_Public_Void_String_UIContext_Boolean_EnumPublicSealedvaNoLeRiBoIn6vUnique_0("QuickMenuDashboard", null, false);
                else Page.Method_Protected_Virtual_New_Void_0();
                BackButtonPress?.Invoke();
            }));

            region++;
            extButtonGameObject = gameObject.transform.GetChild(0).Find("RightItemContainer/Button_QM_Expand").gameObject;
            extButtonGameObject.SetActive(expandButton);
            extButtonGameObject.GetComponentInChildren<Button>().onClick = new Button.ButtonClickedEvent();
            if (expandButtonAction != null)
                extButtonGameObject.GetComponentInChildren<Button>().onClick.AddListener(expandButtonAction);

            if (expandButtonSprite != null) {
                extButtonGameObject.GetComponentInChildren<Image>().sprite = expandButtonSprite;
                extButtonGameObject.GetComponentInChildren<Image>().overrideSprite = expandButtonSprite;
                if (preserveColor) {
                    extButtonGameObject.GetComponentInChildren<Image>().color = Color.white;
                    extButtonGameObject.GetComponentInChildren<StyleElement>(true).enabled = false;
                }
            }
            region++;

            menuMask = MenuContents.parent.gameObject.GetOrAddComponent<RectMask2D>();
            menuMask.enabled = true;
            gameObject.transform.Find("ScrollRect").GetOrAddComponent<ScrollRect>().enabled = true;
            gameObject.transform.Find("ScrollRect").GetOrAddComponent<ScrollRect>().verticalScrollbar = gameObject.transform.Find("ScrollRect/Scrollbar").GetOrAddComponent<Scrollbar>();
            gameObject.transform.Find("ScrollRect").GetOrAddComponent<ScrollRect>().verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
            gameObject.DestroyChildren(where => where.name != "ScrollRect" && where.name != "Header_H1");
            region++;

            gameObject.transform.Find("ScrollRect/Viewport").GetComponent<VRCRectMask2D>().prop_Boolean_0 = true; // Fixes the items falling off of the QM
            gameObject.transform.Find("ScrollRect").GetComponent<Il2Cpp.VRCScrollRect>().field_Public_Boolean_0 = true; // Fixes the items falling off of the QM

            region++;
            Page.GetComponent<Canvas>().enabled = true; // Fix for Late Menu Creation
            Page.GetComponent<CanvasGroup>().enabled = true; // Fix for Late Menu Creation
            Page.GetComponent<UIPage>().enabled = true; // Fix for Late Menu Creation
            Page.GetComponent<GraphicRaycaster>().enabled = true; // Fix for Late Menu Creation
        }
        catch (Exception ex) {
            throw new Exception("Exception Caught When Making Page At Region: " + region + "\n\n" + ex);
        }
    }

    public void AddExtButton(Action onClick, string tooltip, Sprite icon)
    {
        var obj = Object.Instantiate(extButtonGameObject, extButtonGameObject.transform.parent);
        obj.SetActive(true);
        obj.GetComponentInChildren<Button>().onClick = new Button.ButtonClickedEvent();
        obj.GetComponentInChildren<Button>().onClick.AddListener(onClick);
        obj.GetComponentInChildren<Image>().sprite = icon;
        obj.GetComponentInChildren<Image>().overrideSprite = icon;
    }


    public void OpenMenu()
    {
        Page.gameObject.active = true;
        //QMUtils.GetMenuStateControllerInstance.Method_Public_Void_String_UIContext_Boolean_TransitionType_0(Page.field_Public_String_0, null, false, UIPage.TransitionType.Right);
        //QMUtils.GetMenuStateControllerInstance.Method_Public_Void_String_UIContext_Boolean_EnumPublicSealedvaNoLeRiBoIn6vUnique_0(Page.field_Public_String_0, null, false);
        QMUtils.GetMenuStateControllerInstance?.Method_Public_Void_String_UIContext_Boolean_EnumPublicSealedvaNoLeRiBoIn6vUnique_0(Page.field_Public_String_0, null, false, EnumPublicSealedvaNoLeRiBoIn6vUnique.Right);
        Page.gameObject.GetComponent<Canvas>().gameObject.SetActive(true);
        Page.gameObject.GetComponent<CanvasGroup>().gameObject.SetActive(true);
        Page.gameObject.GetComponent<GraphicRaycaster>().gameObject.SetActive(true);
        OnMenuOpen?.Invoke();
        lastOpenedPage = this;
        MelonCoroutines.Start(IWillKrillSomeone());
    }

    IEnumerator IWillKrillSomeone() {
        yield return new WaitForSeconds(.3f);
        Page.transform.Find("Header_H1").gameObject.active = false;
        yield return new WaitForSeconds(.1f);
        Page.transform.Find("Header_H1").gameObject.active = true;
    }

    public void SetTitle(string text) => pageTitleText.text = text;
    public void CloseMenu() => Page.Method_Protected_Virtual_New_Void_0();

}
*/
