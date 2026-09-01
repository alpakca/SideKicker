using System;

namespace SideKicker.Panels.Timers
{
    internal class TimeInfo
    {
        public bool IsAlarm { get; init; }
        public DateTime DueTime { get; init; }
        public string LabelText { get; init; }
    }
}
