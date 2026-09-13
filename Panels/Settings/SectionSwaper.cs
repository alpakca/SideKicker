using Microsoft.Xna.Framework;
using PopUpUI.Input;
using System;

namespace SideKicker.Panels.Settings
{
    internal class SectionSwaper
    {
        private bool _isHeld;

        public int SectionCount { get; set; }
        public int SectionLength { get; set; }
        public int SectionMargin { get; set; }
        public int HoldingSectionIndex { get; private set; }
        public Point Location { get; set; }

        public event Action<int, int> SectionsSwapped;

        public Rectangle GetSectionBounds(int index)
        {
            return new Rectangle(0, index * (SectionLength + SectionMargin), SectionLength, SectionLength);
        }

        public Rectangle GetBoundaries()
        {
            return new Rectangle(Location, new Point(SectionLength, SectionCount * (SectionLength + SectionMargin) - SectionMargin));
        }

        public void OnMouseStateChanged(PopWindowMouseEventArgs e)
        {
            if (e.FirstTimeButtons.HasLeft && GetBoundaries().Contains(e.MousePosition))
            {
                _isHeld = true;

                HoldingSectionIndex = GetSectionIndex(e.MousePosition - Location);
            }
            else if (e.ReleasedButtons.HasLeft)
            {
                _isHeld = false;

                HoldingSectionIndex = -1;
            }
        }

        public void OnMousePositionChanged(PopWindowMouseEventArgs e)
        {
            if (_isHeld)
            {
                int newIndex = GetSectionIndex(e.MousePosition - Location);
                if (newIndex != - 1 && newIndex != HoldingSectionIndex)
                {
                    SectionsSwapped?.Invoke(HoldingSectionIndex, newIndex);

                    HoldingSectionIndex = newIndex;
                }
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
            return GetSectionIndex(point - Location) != -1;
        }
    }
}
