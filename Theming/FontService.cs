using System;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace SideKicker.Theming
{
    internal static class FontService
    {
        public static FontProvider Regular { get; } = new FontProvider(new Dictionary<int, SpriteFont>
        {

        });

        public static FontProvider Bold { get; } = new FontProvider(new Dictionary<int, SpriteFont>
        {

        });

        public static Action FontChanged;
    }
}
