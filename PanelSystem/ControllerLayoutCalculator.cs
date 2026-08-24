using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace SideKicker.PanelSystem
{
    internal class ControllerLayoutCalculator
    {
        private const int FrameThickness = 4;
        private const int LeftMargin = 8;
        private const int ContextPanelIconLength = 64;

        public Point GetSidePanelSize(int contextPanelCount)
        {
            return new Point(LeftMargin + ContextPanelIconLength + FrameThickness, contextPanelCount * (FrameThickness + ContextPanelIconLength) + FrameThickness);
        }

        public Rectangle GetContextPanelIconBounds(int index)
        {
            return new Rectangle(LeftMargin, FrameThickness + index * (ContextPanelIconLength + FrameThickness), ContextPanelIconLength, ContextPanelIconLength);
        }
    }
}
