using UnityEngine;
using VRC.Localization;

namespace VRClient.SDK.VRCs
{
    public static class UIUtils
    {
        private static readonly string resourcePath = Path.Combine(EntryPoint.PluginPath, "Assets", "Images");

        public static Transform GhostModeHud;
        public static Sprite Ghost = LoadSprite("GhostIcon"), Logo = LoadSprite("LogoIcon");

        public static Sprite LoadSprite(string sprite)
        {
            if (!Directory.Exists(resourcePath))
            {
                Directory.CreateDirectory(resourcePath);
            }

            var file = File.ReadAllBytes(Path.Combine(resourcePath, sprite + ".png"));
            if (file != null || file.Length != 0)
            {
                Logging.Log(Path.Combine(resourcePath, sprite + ".png"), Logging.LType.Success);
            }
            return CreateSprite(file);
        }

        public static Sprite CreateSprite(byte[] data)
        {
            if (data == null || data.Length == 0) return null;
            var tex = new Texture2D(2, 2);
            if (!tex.LoadImage(data)) return null;
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0, 0), 100000f, 1000u, SpriteMeshType.FullRect, Vector4.zero, false);
            sprite.hideFlags |= HideFlags.DontUnloadUnusedAsset;
            return sprite;
        }
    }
}