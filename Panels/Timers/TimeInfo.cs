using System;
using System.Text.Json.Serialization;

namespace SideKicker.Panels.Timers
{
    internal class TimeInfo
    {
        private bool _isRunning = true;
        private DateTime _dueTime;
        private TimeSpan _remainingTimeWhenPaused;

        public bool IsAlarm { get; init; }
        public string LabelText { get; init; }

        public bool IsRunning => _isRunning;
        public DateTime DueTime => _dueTime;
        public TimeSpan RemainingTimeWhenPaused => _remainingTimeWhenPaused;

        public bool HasElapsed
        {
            get
            {
                if (!_isRunning)
                    return _remainingTimeWhenPaused <= TimeSpan.Zero;

                return _dueTime <= DateTime.Now;
            }
        }

        public TimeInfo(DateTime dueTime)
        {
            _dueTime = dueTime;
        }

        [JsonConstructor]
        private TimeInfo(DateTime dueTime, bool isRunning, TimeSpan remainingTimeWhenPaused)
        {
            _dueTime = dueTime;
            _isRunning = isRunning;
            _remainingTimeWhenPaused = remainingTimeWhenPaused;
        }

        public void Pause()
        {
            if (!_isRunning) return;

            _remainingTimeWhenPaused = _dueTime - DateTime.Now;

            if (_remainingTimeWhenPaused <= TimeSpan.Zero)
            {
                _remainingTimeWhenPaused = TimeSpan.Zero;
            }

            _isRunning = false;
        }

        public void Resume()
        {
            if (_isRunning) return;

            _dueTime = DateTime.Now.Add(_remainingTimeWhenPaused);
            _isRunning = true;
        }

        public TimeSpan GetRemainingTime(DateTime now)
        {
            if (!_isRunning)
                return _remainingTimeWhenPaused;

            return _dueTime - now;
        }
    }
}
