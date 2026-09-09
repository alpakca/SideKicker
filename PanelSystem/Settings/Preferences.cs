using Microsoft.Xna.Framework;
using System.Text.Json.Serialization;

namespace SideKicker.PanelSystem.Settings
{
    internal class Preferences
    {
        public static Preferences Default { get; } = new Preferences
        {
            OriginX = 1920,
            OriginY = 920,
            ShownPanelIndex = 0,
            ThemeId = "dark"
        };

        public int ShownPanelIndex { get; set; }
        public int OriginX { get; set; }
        public int OriginY { get; set; }
        public string ThemeId { get; set; }

        [JsonIgnore]
        public Point Origin => new Point(OriginX, OriginY);
    }
}
