using System;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using PopUpUI.Toolkit;

namespace SideKicker.Theming
{
    internal static class FontService
    {
        public static FontProvider Regular { get; } = new FontProvider(new Dictionary<int, SpriteFont>
        {
            { 12, ContentAssistant.FromLoaded<SpriteFont>("Font_Consolas_12") },
            { 16, ContentAssistant.FromLoaded<SpriteFont>("Font_Consolas_16") },
            { 24, ContentAssistant.FromLoaded<SpriteFont>("Font_Consolas_24") },
        });

        public static event Action FontChanged;
    }
}
