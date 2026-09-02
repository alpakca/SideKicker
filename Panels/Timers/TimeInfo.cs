using System;

namespace SideKicker.Panels.Timers
{
    internal class TimeInfo
    {
        private bool _isRunning = true;
        private TimeSpan _remainingTimeWhenPaused;

        public bool IsAlarm { get; init; }
        public DateTime DueTime { get; private set; }
        public string LabelText { get; init; }

        public bool IsRunning => _isRunning;

        public TimeInfo(DateTime dueTime)
        {
            DueTime = dueTime;
        }

        public void Pause()
        {
            if (!_isRunning) return;

            _remainingTimeWhenPaused = DueTime - DateTime.Now;
            _isRunning = false;
        }

        public void Resume()
        {
            if (_isRunning) return;

            DueTime = DateTime.Now.Add(_remainingTimeWhenPaused);
            _isRunning = true;
        }

        public TimeSpan GetRemainingTime(DateTime now)
        {
            if (!_isRunning)
                return _remainingTimeWhenPaused;

            return DueTime - now;
        }
    }
}
