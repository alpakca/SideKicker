using System;
using System.Text.Json.Serialization;

namespace SideKicker.Panels.Timers
{
    internal class TimerInfo : ITimeInfo
    {
        [JsonInclude]
        public DateTime EndTime { get; private set; }

        [JsonInclude]
        public TimeSpan RemainingWhenPaused { get; private set; }

        [JsonInclude]
        public bool IsPaused { get; private set; }

        public string LabelText { get; set; }

        [JsonIgnore]
        public bool IsRepeating => false;

        [JsonIgnore]
        public bool HasElapsed
        {
            get
            {
                if (IsPaused) return RemainingWhenPaused <= TimeSpan.Zero;

                return EndTime <= DateTime.Now;
            }
        }

        public TimerInfo()
        {
            SetDuration(TimeSpan.Zero);
        }

        public void SetDuration(TimeSpan duration)
        {
            RemainingWhenPaused = duration;
            
            if (!IsPaused)
            {
                EndTime = DateTime.Now + duration;
            }
        }

        public void Pause()
        {
            if (IsPaused) return;

            IsPaused = true;
            RemainingWhenPaused = EndTime - DateTime.Now;

            if (RemainingWhenPaused <= TimeSpan.Zero)
            {
                RemainingWhenPaused = TimeSpan.Zero;
            }
        }

        public void Resume()
        {
            if (!IsPaused) return;

            IsPaused = false;
            EndTime = DateTime.Now + RemainingWhenPaused;
        }

        public TimeSpan GetRemainingTime(DateTime asOf)
        {
            TimeSpan remaining = IsPaused ? RemainingWhenPaused : EndTime - asOf;

            return remaining >= TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }

        public string DisplayString(DateTime now)
        {
            TimeSpan remaining = GetRemainingTime(now);
            return "Timer ends in: " + string.Join(", ", remaining.Hours > 0 ? $"{remaining.Hours}h" : string.Empty,
                                                         remaining.Minutes > 0 ? $"{remaining.Minutes}m" : string.Empty,
                                                         remaining.Seconds > 0 ? $"{remaining.Seconds}s" : string.Empty);
        }
    }
}
