using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace SideKicker.Theming
{
    internal class ResolvedTheme
    {
        public string Id { get; }
        private readonly Dictionary<string, Color> _colors;
        private readonly Dictionary<string, Texture2D> _icons;

        public ResolvedTheme(string id, Dictionary<string, Color> colors, Dictionary<string, Texture2D> icons)
        {
            Id = id;
            _colors = colors;
            _icons = icons;
        }

        public Color GetColor(string id)
        {
            return _colors[id];
        }

        public Texture2D GetIcon(string id)
        {
            return _icons[id];
        }
    }
}
