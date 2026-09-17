using System;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using PopUpUI.Toolkit;

namespace SideKicker.Rendering
{
    internal static class FontService
    {
        public static FontProvider Regular { get; } = new FontProvider(new Dictionary<int, SpriteFont>
        {
            { 12, ContentAssistant.FromLoaded<SpriteFont>("Font_SegoeUI_12") },
            { 16, ContentAssistant.FromLoaded<SpriteFont>("Font_SegoeUI_16") },
            { 24, ContentAssistant.FromLoaded<SpriteFont>("Font_SegoeUI_24") },
            { 20, ContentAssistant.FromLoaded<SpriteFont>("Font_SegoeUI_20") }
        });

        public static event Action FontChanged;
    }
}
