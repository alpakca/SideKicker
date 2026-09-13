using Microsoft.Xna.Framework;
using PopUpUI.Elements;
using PopUpUI.Input;
using System;

namespace SideKicker.Panels.Settings
{
    internal class OriginMover : PopElement, IHoldable
    {
        private Point _holdStartPosition;

        public bool IsHeld { get; private set; }

        HoldablePacket IHoldable.HoldPacket { get; } = new HoldablePacket();

        public event Action<int> MouseVerticalDifferenceUpdated;

        public void OnHolding(PopWindowMouseEventArgs e)
        {
            Point difference = e.MousePosition - _holdStartPosition;
            if (difference.Y != 0)
            {
                MouseVerticalDifferenceUpdated?.Invoke(difference.Y);
            }

            _holdStartPosition = e.MousePosition;
        }

        public void OnHoldStateChange(PopWindowMouseEventArgs e, bool startedHolding)
        {
            IsHeld = startedHolding;

            if (startedHolding)
            {
                _holdStartPosition = e.MousePosition;
            }
            else
            {
                _holdStartPosition = Point.Zero;
            }
        }

        public bool CanBeHeld(Point point)
        {
            return Boundaries.Contains(point);
        }
    }
}
