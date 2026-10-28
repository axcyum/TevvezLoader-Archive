using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRC.Localization;

namespace VRClient.SDK.VRCs
{
    internal static class PopupUtils
    {
        public static void HudMessage(string content, string? description = null, float duration = 5f)
        {
            LocalizableString title = LocalizableStringExtensions.Localize(content, null, null, null);
            LocalizableString desc = description != null ? LocalizableStringExtensions.Localize(description, null, null, null) : null;

            VRCUiManager.field_Private_Static_VRCUiManager_0.field_Private_HudController_0.notification
                .Method_Public_Void_Sprite_LocalizableString_LocalizableString_Single_ReactiveProperty_1_Boolean_0(
                    UIUtils.Logo, title, desc, duration, null
                );
        }
    }
}
