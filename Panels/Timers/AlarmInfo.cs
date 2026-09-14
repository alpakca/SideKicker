using System;
using System.Text.Json.Serialization;

namespace SideKicker.Panels.Timers
{
    internal class AlarmInfo : ITimeInfo
    {
        [JsonInclude]
        public TimeSpan AlarmTime { get; private set; }

        [JsonInclude]
        public DayOfWeek[]? RepeatingDays { get; private set; }

        [JsonInclude]
        public DateTime DueTime { get; private set; }

        public string LabelText { get; set; }

        [JsonIgnore]
        public bool IsRepeating => RepeatingDays != null;

        [JsonIgnore]
        public bool HasElapsed
        {
            get
            {
                DateTime now = DateTime.Now;
                if (DueTime <= now)
                {
                    if (RepeatingDays != null)
                    {
                        // set due time to next time
                        DueTime = GetNextRemainRepeatingTime(now);
                    }

                    return true;
                }

                return false;
            }
        }

        public void SetSingle(DateTime date)
        {
            AlarmTime = date.TimeOfDay;
            RepeatingDays = null;

            DueTime = date;
        }

        public void SetRepeating(TimeSpan alarmTime, DayOfWeek[] repeatingDays)
        {
            AlarmTime = alarmTime;
            RepeatingDays = repeatingDays;

            DateTime now = DateTime.Now;
            DueTime = GetNextRemainRepeatingTime(now);
        }

        public string DisplayString(DateTime now)
        {
            return $"{DueTime.Day} {CalendarShowcase.MonthNames[DueTime.Month - 1]}, {DueTime.Year}";
        }

        public TimeSpan GetRemainingTime(DateTime asOf)
        {
            TimeSpan remaining = DueTime - asOf;
            return remaining >= TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }

        private DateTime GetNextRemainRepeatingTime(DateTime asOf)
        {
            int asOfDay = (int)asOf.DayOfWeek;
            TimeSpan minimumTime = TimeSpan.MaxValue;

            foreach (DayOfWeek repeatDay in RepeatingDays)
            {
                int daysUntil = ((int)repeatDay - asOfDay + 7) % 7;

                TimeSpan time = new TimeSpan(daysUntil, 0, 0, 0).Add(AlarmTime).Subtract(asOf.TimeOfDay);

                // check for same day, passed alarm time
                if (time <= TimeSpan.Zero)
                {
                    time += TimeSpan.FromDays(7);
                }

                if (time < minimumTime)
                {
                    minimumTime = time;
                }
            }

            return new DateTime(asOf.Ticks + minimumTime.Ticks);
        }
    }
}
