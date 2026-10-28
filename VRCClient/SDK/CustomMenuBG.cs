using Il2CppSystem.Resources;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VRC.Core;
using VRC.UI;

namespace VRClient.SDK
{
    internal class CustomMenuBG
    {
        public static bool IsLoaded = false;
        public static IEnumerator Init()
        {
            while (GameObject.Find("MenuContent/Backdrop/Backdrop/Background") == null)
            {
                yield return null;
            }
            GameObject.Find("MenuContent/Backdrop/Backdrop/Background").GetComponent<Image>().overrideSprite = VRCs.UIUtils.LoadSprite("MMBG.png");
            GameObject.Find("MenuContent/Backdrop/Backdrop/Background").GetComponent<Image>().color = Color.white;
            while (GameObject.Find("Canvas_QuickMenu(Clone)") == null)
            {
                yield return null;
            }
            GameObject.Find("Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/QMParent/BackgroundLayer01/Target").GetComponent<ImageEx>().overrideSprite = VRCs.UIUtils.LoadSprite("QMBG.png");
            GameObject.Find("Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/Wing_Left/Container/InnerContainer/Background").GetComponent<ImageEx>().overrideSprite = VRCs.UIUtils.LoadSprite("RightQMWingBG.png");
            GameObject.Find("Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/Wing_Right/Container/InnerContainer/Background").GetComponent<ImageEx>().overrideSprite = VRCs.UIUtils.LoadSprite("LeftQMWingBG.png");
            while (GameObject.Find("Canvas_MainMenu(Clone)") == null)
            {
                yield return null;
            }
            GameObject.Find("Canvas_MainMenu(Clone)/Container/MMParent/BackgroundLayer01/Target").GetComponent<ImageEx>().overrideSprite = VRCs.UIUtils.LoadSprite("MMBG.png");
            IsLoaded = true;
        }
        public static void ApplyColors()
        {
            if (IsLoaded)
            {
                GameObject.Find("Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/QMParent/BackgroundLayer01/Target").GetComponent<ImageEx>().color = Color.white;
                GameObject.Find("Canvas_MainMenu(Clone)/Container/MMParent/BackgroundLayer01/Target").GetComponent<ImageEx>().color = Color.white;
                GameObject.Find("Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/Wing_Right/Container/InnerContainer/Background").GetComponent<ImageEx>().color = Color.white;
                GameObject.Find("Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/Wing_Left/Container/InnerContainer/Background").GetComponent<ImageEx>().color = Color.white;
                if (GameObject.Find("UnscaledUI/ActionMenu/Container/MenuR/ActionMenu/Main/Background"))
                {
                    GameObject.Find("UnscaledUI/ActionMenu/Container/MenuR/ActionMenu/Main/Background").GetComponent<PedalGraphic>().color = Color.black;
                }
                if (GameObject.Find("UnscaledUI/ActionMenu/Container/MenuR/ActionMenu/Main/Center"))
                {
                    GameObject.Find("UnscaledUI/ActionMenu/Container/MenuR/ActionMenu/Main/Center").GetComponent<PedalGraphic>().color = Color.black;
                }
                IsLoaded = true;
            }
        }
    }
}
