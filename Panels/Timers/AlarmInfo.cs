using System;

namespace SideKicker.Panels.Timers
{
    internal class AlarmInfo : ITimeInfo
    {
        public DateTime DueTime { get; set; }
        public string LabelText { get; set; }

        public bool HasElapsed => DueTime <= DateTime.Now;

        public string DisplayString(DateTime now)
        {
            return $"{DueTime.Day} {CalendarShowcase.MonthNames[DueTime.Month - 1]}, {DueTime.Year}";
        }

        public TimeSpan GetRemainingTime(DateTime asOf)
        {
            TimeSpan remaining = DueTime - asOf;
            return remaining >= TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }
    }
}
