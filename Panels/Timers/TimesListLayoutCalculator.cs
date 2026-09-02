using Microsoft.Xna.Framework;
using SideKicker.Theming;
using System;
using System.Collections.Generic;
using System.Text;

namespace SideKicker.Panels.Timers
{
    internal static class TimesListLayoutCalculator
    {
        public const int CalendarDaysTextSize = 16;
        public const int CalendarDayButtonMargin = 4;

        public static readonly int CalendarDayButtonHeight = FontService.Regular.GetLineSpacing(CalendarDaysTextSize);

        public static Rectangle CalendarWeekDayBounds(int columnIndex)
        {
            return new Rectangle(CalendarDayButtonMargin + columnIndex * (CalendarDayButtonHeight + CalendarDayButtonMargin), 0, 
                                 CalendarDayButtonHeight, CalendarDayButtonHeight);
        }
    }
}
