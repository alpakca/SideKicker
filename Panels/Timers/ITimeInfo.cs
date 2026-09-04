using System;
using System.Text.Json.Serialization;

namespace SideKicker.Panels.Timers
{
    [JsonDerivedType(typeof(AlarmInfo), typeDiscriminator: "alarm")]
    [JsonDerivedType(typeof(TimerInfo), typeDiscriminator: "timer")]
    internal interface ITimeInfo
    {
        [JsonIgnore]
        public bool HasElapsed { get; }
        public string LabelText { get; set; }

        public TimeSpan GetRemainingTime(DateTime asOf);
        public string DisplayString(DateTime now);
    }
}
