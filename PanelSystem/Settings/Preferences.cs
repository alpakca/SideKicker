using Microsoft.Xna.Framework;
using SideKicker.Panels;
using System;
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
            PanelOrder = Enum.GetValues<ContextPanelType>(),
            ThemeId = "dark"
        };

        public int ShownPanelIndex { get; set; }
        public int OriginX { get; set; }
        public int OriginY { get; set; }
        public ContextPanelType[] PanelOrder { get; set; }
        public string ThemeId { get; set; }

        [JsonIgnore]
        public Point Origin => new Point(OriginX, OriginY);

        public event Action<Point> OriginUpdated;
        public event Action<int, int> PanelsReordered;

        public void ChangeOrigin(Point value)
        {
            OriginX = value.X;
            OriginY = value.Y;

            OriginUpdated?.Invoke(value);
        }

        public void ReorderPanels(int firstPanelIndex, int secondPanelIndex)
        {
            (PanelOrder[firstPanelIndex], PanelOrder[secondPanelIndex]) = (PanelOrder[secondPanelIndex], PanelOrder[firstPanelIndex]);

            PanelsReordered?.Invoke(firstPanelIndex, secondPanelIndex);
        }
    }
}
