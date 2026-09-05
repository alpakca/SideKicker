using PopUpUI.Elements;
using System;

namespace SideKicker.Panels.Timers
{
    internal class StopwatchShowcase
    {
        private bool _isStarted;
        private bool _isPaused;
        private DateTime _startTime;
        private TimeSpan _elapsed;

        public bool IsStarted => _isStarted;
        public bool IsPaused => _isPaused;
        public TimeSpan Elapsed => _elapsed;

        public void Start()
        {
            if (_isStarted) return;

            _isStarted = true;
            _startTime = DateTime.Now;
        }

        public void Stop()
        {
            if (!_isStarted) return;

            _isStarted = false;
            _elapsed = new TimeSpan();
        }

        public void Update()
        {
            if (_isStarted && !_isPaused)
            {
                DateTime now = DateTime.Now;
                _elapsed += now - _startTime;
                _startTime = now;
            }
        }

        public void Pause()
        {
            if (_isPaused) return;

            _isPaused = true;
        }

        public void Resume()
        {
            if (!_isPaused) return;

            _isPaused = false;
            _startTime = DateTime.Now;
        }
    }
}
