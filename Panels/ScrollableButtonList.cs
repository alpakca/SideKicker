using Microsoft.Xna.Framework;
using PopUpUI.Elements;
using PopUpUI.Input;
using System;
using System.Collections.Generic;

namespace SideKicker.Panels
{
    internal class ScrollableButtonList
    {
        private int scrolledLength;

        public int ButtonMargin { get; set; }
        public int ButtonHeight { get; set; }

        public Rectangle Boundaries { get; set; }
        public List<Clickable> Buttons { get; } = new List<Clickable>();

        public int HoveredButtonIndex { get; private set; }
        public bool IsHoveredButtonHeld { get; private set; }

        public Rectangle GetButtonOffset(int index)
        {
            return new Rectangle(ButtonMargin, ButtonMargin + index * (ButtonHeight + ButtonMargin) - scrolledLength,
                                 Boundaries.Width - 2 * ButtonMargin, ButtonHeight);
        }

        public void Scroll(PopWindowMouseEventArgs e, int amount)
        {
            int maxScroll = ButtonMargin + Buttons.Count * (ButtonHeight + ButtonMargin) - Boundaries.Height;
            if (maxScroll <= 0)
                return;

            scrolledLength = Math.Clamp(scrolledLength + amount, 0, maxScroll);
            MousePositionChanged(e); // update hover
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
            }
            else if (e.ReleasedButtons.HasLeft)
            {
                if (HoveredButtonIndex != -1 && GetButtonOffset(HoveredButtonIndex).Contains(e.MousePosition - Boundaries.Location))
                {
                    Buttons[HoveredButtonIndex].ClickCommand?.Execute(Buttons[HoveredButtonIndex]);
                }

                IsHoveredButtonHeld = false;
                HandleHovering(e);
            }
        }

        private void HandleHovering(PopWindowMouseEventArgs e)
        {
            if (!IsHoveredButtonHeld)
            {
                HoveredButtonIndex = -1;
                for (int i = 0; i < Buttons.Count; i++)
                {
                    if (GetButtonOffset(i).Contains(e.MousePosition - Boundaries.Location))
                    {
                        HoveredButtonIndex = i;
                        break;
                    }
                }
            }
        }
    }
}
