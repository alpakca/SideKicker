using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace SideKicker.Panels
{
    internal class TextPlaque
    {
        public string Text { get; set; }
        public int TextSize { get; set; }
        public bool IsBold { get; set; }

        public Vector2 Position { get; set; }
    }
}
