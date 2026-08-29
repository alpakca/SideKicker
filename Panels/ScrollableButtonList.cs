using Microsoft.Xna.Framework;
using PopUpUI.Elements;
using PopUpUI.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SideKicker.Panels
{
    internal class ScrollableButtonList
    {
        private int scrolledLength;

        public int ButtonMargin { get; set; }
        public int ButtonHeight { get; set; }

        public Rectangle Boundaries { get; set; }
        public int ButtonCount { get; set; }

        public int HoveredButtonIndex { get; private set; }
        public bool IsHoveredButtonHeld { get; private set; }
        public Point RelativeMouseToHovered { get; private set; }

        public event Action<int> ButtonPressed;
        public event Action<int> ButtonHeld;

        public Rectangle GetButtonBounds(int index)
        {
            return new Rectangle(ButtonMargin, ButtonMargin + index * (ButtonHeight + ButtonMargin) - scrolledLength,
                                 Boundaries.Width - 2 * ButtonMargin, ButtonHeight);
        }

        public int GetButtonOn(Point relativePosition)
        {
            for (int i = 0; i < ButtonCount; i++)
            {
                if (GetButtonBounds(i).Contains(relativePosition))
                    return i;
            }

            return -1;
        }

        public void Scroll(PopWindowMouseEventArgs e, int amount)
        {
            int maxScroll = ButtonMargin + ButtonCount * (ButtonHeight + ButtonMargin) - Boundaries.Height;
            if (maxScroll <= 0)
                return;

            scrolledLength = Math.Clamp(scrolledLength + amount, 0, maxScroll);
            MousePositionChanged(e); // update hover
        }

        public void UpdateScrolledLength()
        {
            int maxScroll = ButtonMargin + ButtonCount * (ButtonHeight + ButtonMargin) - Boundaries.Height;
            if (maxScroll <= 0)
            {
                scrolledLength = 0;
                return;
            }

            scrolledLength = Math.Clamp(scrolledLength, 0, maxScroll);
        }

        public void MouseStateChangeUpdate(PopWindowMouseEventArgs e)
        {
            HandleHolding(e);
        }

        public void MousePositionChanged(PopWindowMouseEventArgs e)
        {
            HandleHovering(e);
            HandleHolding(e);
        }

        private void HandleHolding(PopWindowMouseEventArgs e)
        {
            if (e.FirstTimeButtons.HasLeft && HoveredButtonIndex != -1)
            {
                IsHoveredButtonHeld = true;
                ButtonHeld?.Invoke(HoveredButtonIndex);
            }
            else if (e.ReleasedButtons.HasLeft)
            {
                if (HoveredButtonIndex != -1 && GetButtonBounds(HoveredButtonIndex).Contains(e.MousePosition - Boundaries.Location))
                {
                    ButtonPressed?.Invoke(HoveredButtonIndex);
                }

                IsHoveredButtonHeld = false;
                HandleHovering(e);
            }
        }

        private void HandleHovering(PopWindowMouseEventArgs e)
        {
            HoveredButtonIndex = -1;
            RelativeMouseToHovered = new Point(-1);

            for (int i = 0; i < ButtonCount; i++)
            {
                Rectangle bounds = GetButtonBounds(i);
                if (bounds.Contains(e.MousePosition - Boundaries.Location))
                {
                    HoveredButtonIndex = i;
                    RelativeMouseToHovered = e.MousePosition - Boundaries.Location - bounds.Location;
                    break;
                }
            }
        }
    }
}
