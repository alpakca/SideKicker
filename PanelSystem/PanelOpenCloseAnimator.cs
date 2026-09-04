using Microsoft.Xna.Framework;
using PopUpUI.Toolkit;
using System;
using System.Collections.Generic;
using System.Text;

namespace SideKicker.PanelSystem
{
    internal class PanelOpenCloseAnimator
    {
        private const int Duration = 500;

        private bool _isRunning;
        private float _elapsed;

        private int _startPosition;
        private int _endPosition;

        public bool IsRunning => _isRunning;

        public int CurrentOffset
        {
            get
            {
                float timePercentage = _elapsed / Duration;
                float easedPosition = SmoothStep(timePercentage);

                return (int)(_startPosition + (_endPosition - _startPosition) * easedPosition);
            }
        }

        public event Action AnimationDone;

        public void StartOffseting(int leftSideStart, int leftSideStop)
        {
            if (!_isRunning)
            {
                _startPosition = leftSideStart;
            }

            _endPosition = leftSideStop;
            _elapsed = 0;
            _isRunning = true;
        }

        public void Update()
        {
            if (!_isRunning) return;

            _elapsed += Time.DeltaTime;
            if (_elapsed >= Duration)
            {
                _elapsed = Duration;
                _isRunning = false;
                AnimationDone?.Invoke();
            }
        }

        private static float SmoothStep(float timePercentage)
        {
            timePercentage = Math.Clamp(timePercentage, 0f, 1f);
            return timePercentage * timePercentage * (3f - 2f * timePercentage);
        }
    }
}
