using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace SideKicker.PanelSystem
{
    internal static class ControllerLayoutCalculator
    {
        private const int FrameThickness = 6;
        private const int LeftMargin = 12;
        private const int ContextPanelIconLength = 32;

        private const int OpenPanelVerticalBarMargin = 8;
        private const int OpenPanelVerticalBarWidth = 2;
        private const int OpenPanelVerticalBarHeight = 24;

        public static Point GetSidePanelSize(int contextPanelCount)
        {
            return new Point(LeftMargin + ContextPanelIconLength + FrameThickness, contextPanelCount * (FrameThickness + ContextPanelIconLength) + FrameThickness);
        }

        public static Rectangle GetContextPanelIconBounds(int index)
        {
            return new Rectangle(LeftMargin, FrameThickness + index * (ContextPanelIconLength + FrameThickness), ContextPanelIconLength, ContextPanelIconLength);
        }

        public static Rectangle GetOpenPanelBarBounds(int index)
        {
            int yOffset = (ContextPanelIconLength - OpenPanelVerticalBarHeight) / 2;
            return new Rectangle(OpenPanelVerticalBarMargin, FrameThickness + index * (ContextPanelIconLength + FrameThickness) + yOffset,
                                 OpenPanelVerticalBarWidth, OpenPanelVerticalBarHeight);
        }
    }
}
