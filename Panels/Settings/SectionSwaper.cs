using Microsoft.Xna.Framework;
using PopUpUI.Elements;
using PopUpUI.Input;
using System;

namespace SideKicker.Panels.Settings
{
    internal class SectionSwaper : PopElement, IHoldable
    {
        public bool IsHeld { get; private set; }

        HoldablePacket IHoldable.HoldPacket { get; } = new HoldablePacket();

        public int SectionCount { get; set; }
        public int SectionLength { get; set; }
        public int SectionMargin { get; set; }
        public int HoldingSectionIndex { get; private set; }

        public event Action<int, int> SectionsSwapped;

        public Rectangle GetSectionBounds(int index)
        {
            return new Rectangle(0, index * (SectionLength + SectionMargin), SectionLength, SectionLength);
        }

        public void OnHolding(PopWindowMouseEventArgs e)
        {
        }

        public void OnHoldStateChange(PopWindowMouseEventArgs e, bool startedHolding)
        {
            IsHeld = startedHolding;

            if (startedHolding)
            {
                HoldingSectionIndex = GetSectionIndex(e.MousePosition - this.Boundaries.Location);
            }
            else
            {
                int newIndex = GetSectionIndex(e.MousePosition - this.Boundaries.Location);
                if (newIndex != HoldingSectionIndex)
                {
                    SectionsSwapped?.Invoke(HoldingSectionIndex, newIndex);
                }

                HoldingSectionIndex = -1;
            }
        }

        private int GetSectionIndex(Point relativeMouse)
        {
            int bucket = relativeMouse.Y / (SectionLength + SectionMargin);
            if (bucket >= SectionCount)
            {
                return -1;
            }

            if (GetSectionBounds(bucket).Contains(relativeMouse))
            {
                return bucket;
            }

            return -1;
        }

        public bool CanBeHeld(Point point)
        {
            return GetSectionIndex(point - this.Boundaries.Location) != -1;
        }
    }
}
