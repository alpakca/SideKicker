using PopUpUI.Toolkit;
using System;

namespace SideKicker.PanelSystem
{
    internal class PanelOpenCloseAnimator
    {
        public const float DefaultDuration = 250f;

        private float _fromValue;
        private float _toValue;
        private float _currentValue;
        private float _elapsed;

        public bool IsRunning { get; private set; }

        public int CurrentX => (int)Math.Round(_currentValue, MidpointRounding.AwayFromZero);
        public int TargetX => (int)Math.Round(_toValue, MidpointRounding.AwayFromZero);

        public event Action AnimationDone;

        public PanelOpenCloseAnimator(int fromValue)
        {
            _fromValue = fromValue;
            _currentValue = fromValue;
        }

        public void AnimateTo(int target)
        {
            if (IsRunning && TargetX == target) return;
            if (!IsRunning && CurrentX == target) return;

            _fromValue = _currentValue; // always continue from the real current position
            _toValue = target;
            _elapsed = 0f;
            IsRunning = true;
        }

        public void Update()
        {
            if (!IsRunning) return;

            _elapsed += Time.DeltaTime;
            float t = DefaultDuration <= 0f ? 1f : Math.Clamp(_elapsed / DefaultDuration, 0f, 1f);

            _currentValue = Lerp(_fromValue, _toValue, SmoothStep(t));

            if (t >= 1f)
            {
                _currentValue = _toValue; // snap exactly — guarantees the final resting position, no float drift
                IsRunning = false;
                AnimationDone?.Invoke();
            }
        }

        private static float Lerp(float a, float b, float t) => a + (b - a) * t;
        private static float SmoothStep(float t) => t * t * (3f - 2f * t);
    }
}
