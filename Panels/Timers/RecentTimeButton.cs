using System;
using System.Collections.Generic;
using System.Text;

namespace SideKicker.Panels.Timers
{
    internal class RecentTimeButton : TextedButton
    {
        private const string DoubleDecimalFormat = "00";

        public TimeSpan Timer { get; private set; }

        public void SetNew(TimeSpan value)
        {
            Timer = value;
            Text = $"{value.Hours.ToString(DoubleDecimalFormat)}:{value.Minutes.ToString(DoubleDecimalFormat)}:{value.Seconds.ToString(DoubleDecimalFormat)}";
        }

        public void SetValues(RecentTimeButton button)
        {
            Timer = button.Timer;
            Text = button.Text;
        }
    }
}
