using Microsoft.Xna.Framework;
using PopUpUI.Input;
using System;
using System.Globalization;

namespace SideKicker.Panels.Timers
{
    internal class CalendarShowcase
    {
        private const int MonthFirstDay = 1;
        private const int DaysInWeek = 7;
        private const int MonthsInYear = 12;

        private int _year;
        private int _month;
        private int _day;
        private string _shownDateText;

        private int _maxWeekRowIndex;
        private int _maxDayColumnIndex;
        private Point _size;

        private readonly GregorianCalendar _calendar;

        public int MaxDays => _calendar.GetDaysInMonth(_year, _month);
        public string ShownDateText => _shownDateText;
        public Point Size => _size;

        public static string[] WeekDayNames { get; } = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];
        public static string[] MonthNames { get; } = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];
        public int SelectedDay => _day;
        public int SelectedMonth => _month;
        public int SelectedYear => _year;

        public int ButtonHeight { get; set; }
        public int ButtonMargin { get; set; }
        public Point Location { get; set; }

        public CalendarShowcase()
        {
            _calendar = new GregorianCalendar();
            DateTime now = DateTime.Now;
            SetDate(now.Year, now.Month, now.Day);
        }

        public Rectangle GetDayButtonBounds(int weekRowIndex, int dayColumnIndex)
        {
            return new Rectangle(new Point(dayColumnIndex * (ButtonHeight + ButtonMargin), weekRowIndex * (ButtonHeight + ButtonMargin)),
                                 new Point(ButtonHeight, ButtonHeight));
        }

        public int GetDay(int weekRowIndex, int dayColumnIndex)
        {
            int firstDayColumn = GetDayColumn(MonthFirstDay);
            const int FirstDayRow = 0;

            int maxDays = _calendar.GetDaysInMonth(_year, _month);
            int day = (weekRowIndex - FirstDayRow) * DaysInWeek + (dayColumnIndex - firstDayColumn + 1);

            if (day > maxDays || day < 1)
            {
                return -1;
            }

            return day;
        }

        public Point GetDayIndexPosition(int day)
        {
            int column = GetDayColumn(day);
            int row = 0;
            while (day > 0)
            {
                row++;
                day -= DaysInWeek;
            }

            if (column >= GetDayColumn(MonthFirstDay))
            {
                row--;
            }

            return new Point(column, row);
        }

        public void SetDate(int year, int month, int day)
        {
            _year = year;
            _month = month;
            _day = day;

            _shownDateText = $"{MonthNames[month - 1]} {_year}";

            int maxDays = _calendar.GetDaysInMonth(year, month);
            int maxDayRowIndex = GetDayIndexPosition(maxDays).Y;

            Rectangle bottomRightDayButton = GetDayButtonBounds(maxDayRowIndex, 6);

            _maxWeekRowIndex = maxDayRowIndex;
            _maxDayColumnIndex = 6; // seven days in a week
            _size = new Point(bottomRightDayButton.Right, bottomRightDayButton.Bottom);
        }

        public void MoveMonth(int amount)
        {
            int newMonth = _month + amount;
            int newYear = _year;

            while (newMonth > MonthsInYear)
            {
                newMonth -= MonthsInYear;
                newYear++;
            }

            while (newMonth < 1)
            {
                newMonth += MonthsInYear;
                newYear--;
            }

            SetDate(newYear, newMonth, _day);
        }

        private int GetDayColumn(int day)
        {
            DayOfWeek firstDay = _calendar.GetDayOfWeek(new DateTime(_year, _month, day));
            return firstDay == DayOfWeek.Sunday ? 6 : (int)firstDay - 1;
        }

        public void MouseStateChangeUpdate(PopWindowMouseEventArgs e)
        {
            if (e.FirstTimeButtons.HasLeft && TryHitDay(e.MousePosition - Location, out int day))
            {
                SetDate(_year, _month, day);
            }
        }

        private bool TryHitDay(Point locationRelativeMouse, out int day)
        {
            int weekRowIndex = locationRelativeMouse.Y / (ButtonHeight + ButtonMargin);
            int dayColumnIndex = locationRelativeMouse.X / (ButtonHeight + ButtonMargin);

            if (0 > weekRowIndex || weekRowIndex > _maxDayColumnIndex ||
                0 > dayColumnIndex || dayColumnIndex > _maxDayColumnIndex)
            {
                day = -1;
                return false;
            }

            if (!GetDayButtonBounds(weekRowIndex, dayColumnIndex).Contains(locationRelativeMouse))
            {
                day = -1;
                return false;
            }

            day = GetDay(weekRowIndex, dayColumnIndex);
            if (day == -1) return false;

            return true;
        }
    }
}
