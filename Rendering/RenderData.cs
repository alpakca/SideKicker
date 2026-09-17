using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Toolkit;
using System;
using System.Collections.Generic;
using System.Text;

namespace SideKicker.Rendering
{
    internal static class RenderData
    {
        private static readonly Dictionary<IconType, TextureInfo> _icons = new Dictionary<IconType, TextureInfo>
        {
            { IconType.EditButton, new TextureInfo(null) }, { IconType.CloseButton, new TextureInfo(null) },
            { IconType.CheckedToggleButton, new TextureInfo(null) }
        };

        public static Color Border => new Color(145, 132, 238, 255);
        public static Color BorderAccent => new Color(14, 14, 14, 255);
        public static Color Background => new Color(27, 27, 27, 255);
        public static Color Surface => new Color(44, 44, 44, 255);

        public static Color HoverOverlay => new Color(104, 104, 104, 72);
        public static Color HeldOverlay => new Color(72, 72, 72, 72);
        public static Color SelectionBox => new Color(59, 122, 186, 72);

        public static Color Text => new Color(255, 255, 255, 255);

        public static Color VerticalBar => new Color(235, 235, 235, 255);

        public static Texture2D GetIcon(IconType type) => LoadIfNotPresent(type);

        private static Texture2D LoadIfNotPresent(IconType type)
        {
            TextureInfo info = _icons[type];
            info.LoadedTexture ??= Texture2D.FromFile(DrawHelper.Device, info.LoadPath);

            return info.LoadedTexture;
        }

        private class TextureInfo
        {
            public string LoadPath { get; }
            public Texture2D? LoadedTexture { get; set; }

            public TextureInfo(string path)
            {
                LoadPath = path;
            }
        }
    }
}
