using System;
using System.Collections.Generic;

namespace SideKicker.Panels.Timers
{
    internal class TimeInfoComparer : IComparer<ITimeInfo>
    {
        private readonly DateTime _dateTime; // captured once to compare

        public TimeInfoComparer()
        {
            _dateTime = DateTime.Now;
        }

        public int Compare(ITimeInfo x, ITimeInfo y)
        {
            return x.GetRemainingTime(_dateTime).CompareTo(y.GetRemainingTime(_dateTime));
        }
    }
}
