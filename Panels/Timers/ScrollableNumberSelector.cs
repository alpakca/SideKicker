using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PopUpUI.Elements;
using PopUpUI.Input;
using PopUpUI.Toolkit;
using System;

namespace SideKicker.Panels.Timers
{
    internal class ScrollableNumberSelector : PopElement, IHoldable, IHoverable, IKeyReceiver, IMouseWheelReceiver
    {
        private const int ConsecutiveKeyDownTime = 250;

        private readonly int _startNumber;
        private readonly int _endNumber;

        private readonly string _digitsStringFormat;

        private int _currentNumber;
        private int _holdStartY;

        private int _lastKeyValue;
        private float _remainingNextKeyTime;

        public int CurrentNumber => _currentNumber;

        public string PreviousNumberFormatted { get; private set; }
        public string CurrentNumberFormatted { get; private set; }
        public string NextNumberFormatted { get; private set; }

        public int DistanceToChangeNumber { get; set; }

        #region PopElement Interface Members
        KeyReceiverPacket IKeyReceiver.KeyReceiverPacket { get; } = new KeyReceiverPacket();
        MouseWheelReceiverPacket IMouseWheelReceiver.MouseWheelReceiverPacket { get; } = new MouseWheelReceiverPacket();
        HoldablePacket IHoldable.HoldPacket { get; } = new HoldablePacket();
        HoverablePacket IHoverable.HoverablePacket { get; } = new HoverablePacket();

        public bool IsHeld { get; private set; }
        public bool IsHovered { get; private set; }
        public bool ReceivesKeyInput => IsHovered;
        #endregion

        public ScrollableNumberSelector(int startNumber, int endNumber)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(endNumber, startNumber);

            _startNumber = startNumber;
            _endNumber = endNumber;

            _digitsStringFormat = new string('0', endNumber.ToString().Length);

            _lastKeyValue = 0;
        }

        public void SetNumber(int value)
        {
            if (_startNumber > value || value > _endNumber)
            {
                throw new ArgumentOutOfRangeException("value must be inside or equal to start-end numbers: " + value);
            }

            SetCurrentNumber(value);
        }

        public void UpdateNumber(int delta)
        {
            int num = _currentNumber + delta;
            if (delta < 0 && num < _startNumber)
            {
                SetCurrentNumber(_endNumber + delta + 1);
            }
            else if (delta > 0 && num > _endNumber)
            {
                SetCurrentNumber(_startNumber + delta - 1);
            }
            else
            {
                SetCurrentNumber(num);
            }
        }

        public override void Update()
        {
            if (_remainingNextKeyTime > 0)
            {
                _remainingNextKeyTime -= Time.DeltaTime;

                if (_remainingNextKeyTime <= 0)
                {
                    SetCurrentNumber(Math.Clamp(_lastKeyValue, _startNumber, _endNumber));
                }
            }

            base.Update();
        }

        public void OnKeyDown(PopWindowMouseEventArgs e, Keys key)
        {
            const int D0Key = (int)Keys.D0;
            const int D9Key = (int)Keys.D9;

            int keyInt = (int)key;
            if (D0Key > keyInt || keyInt > D9Key)
            {
                // input was not a number
                return;
            }

            if (_remainingNextKeyTime <= 0)
            {
                _lastKeyValue = 0;
            }

            _lastKeyValue = _lastKeyValue * 10 + keyInt - D0Key;
            if (_lastKeyValue > _endNumber) // already bigger than possible, terminate consecutive presses
            {
                _remainingNextKeyTime = 0;
                SetCurrentNumber(_endNumber);
                return;
            }

            SetCurrentNumber(_lastKeyValue);
            _remainingNextKeyTime = ConsecutiveKeyDownTime;
        }

        private void SetCurrentNumber(int value)
        {
            _currentNumber = value;

            PreviousNumberFormatted = (_currentNumber == _startNumber ? _endNumber : _currentNumber - 1).ToString(_digitsStringFormat);
            CurrentNumberFormatted = _currentNumber.ToString(_digitsStringFormat);
            NextNumberFormatted = (_currentNumber == _endNumber ? _startNumber : _currentNumber + 1).ToString(_digitsStringFormat);
        }

        public void OnKeyUp(PopWindowMouseEventArgs e, Keys key)
        {
        }

        public void OnMouseVerticalScrollChangeReceive(PopWindowMouseEventArgs e, int change)
        {
            UpdateNumber(change > 0 ? -1 : 1);
        }

        public void OnMouseHorizontalScrollChangeReceive(PopWindowMouseEventArgs e, int change)
        {
        }

        public void OnHolding(PopWindowMouseEventArgs e)
        {
            // positive value when scrolled to down
            int numberOfUpdated = (_holdStartY - e.MousePosition.Y) / DistanceToChangeNumber;

            if (numberOfUpdated != 0)
            {
                UpdateNumber(numberOfUpdated);
                _holdStartY -= numberOfUpdated * DistanceToChangeNumber;
            }
        }

        public void OnHoldStateChange(PopWindowMouseEventArgs e, bool startedHolding)
        {
            IsHeld = startedHolding;
            _holdStartY = e.MousePosition.Y;
        }

        public void OnHovering(PopWindowMouseEventArgs e)
        {
        }

        public void OnHoverStateChange(PopWindowMouseEventArgs e, bool startedHovering)
        {
            IsHovered = startedHovering;
        }

        public bool CanBeHovered(Point point)
        {
            return Boundaries.Contains(point);
        }

        public bool CanReceiveWheelChangeInput(Point point)
        {
            return Boundaries.Contains(point);
        }

        public bool CanBeHeld(Point point)
        {
            return Boundaries.Contains(point);
        }
    }
}
