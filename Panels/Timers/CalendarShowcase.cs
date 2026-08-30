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
        private string _shownDateText;

        private int _maxWeekRowIndex;
        private int _maxDayColumnIndex;
        private Point _size;

        private readonly GregorianCalendar _calendar;
        private readonly string[] _monthNames = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];

        public int MaxDays => _calendar.GetDaysInMonth(_year, _month);
        public string ShownDateText => _shownDateText;
        public Point Size => _size;

        public string[] WeekDayNames { get; } = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];
        public int SelectedDay { get; private set; }

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
            return new Rectangle(GetButtonSectionOffset() + new Point(dayColumnIndex * (ButtonHeight + ButtonMargin), weekRowIndex * (ButtonHeight + ButtonMargin)),
                                 new Point(ButtonHeight, ButtonHeight));
        }

        private Point GetButtonSectionOffset()
        {
            return new Point(ButtonMargin, ButtonMargin + 2 * ButtonHeight);
        }

        public Rectangle NextMonthButtonBounds()
        {
            return new Rectangle(_size.X - ButtonHeight - ButtonMargin, 0, ButtonHeight, ButtonHeight);
        }

        public Rectangle PreviousMonthButtonBounds()
        {
            return new Rectangle(_size.X - 2 * ButtonHeight - ButtonMargin, 0, ButtonHeight, ButtonHeight);
        }

        public Rectangle ShownMonthDisplayBounds()
        {
            return new Rectangle(ButtonMargin, 0, _size.X - 2 * (ButtonHeight + ButtonMargin), ButtonHeight);
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

        private void SetDate(int year, int month, int day)
        {
            _year = year;
            _month = month;
            SelectedDay = day;

            _shownDateText = $"{_monthNames[month - 1]} {_year}";

            int maxDays = _calendar.GetDaysInMonth(year, month);
            int maxDayRowIndex = GetDayIndexPosition(maxDays).Y;

            Rectangle bottomRightDayButton = GetDayButtonBounds(maxDayRowIndex, 6);

            _maxWeekRowIndex = maxDayRowIndex;
            _maxDayColumnIndex = 6; // seven days in a week
            _size = new Point(bottomRightDayButton.Right, bottomRightDayButton.Bottom);
        }

        private int GetDayColumn(int day)
        {
            DayOfWeek firstDay = _calendar.GetDayOfWeek(new DateTime(_year, _month, day));
            return firstDay == DayOfWeek.Sunday ? 6 : (int)firstDay - 1;
        }

        public void MouseStateChangeUpdate(PopWindowMouseEventArgs e)
        {
            if (e.FirstTimeButtons.HasLeft)
            {
                Point locationRelativeMouse = e.MousePosition - Location;
                if (TryHitDay(locationRelativeMouse, out int day))
                {
                    SetDate(_year, _month, day);
                }
                else if (PreviousMonthButtonBounds().Contains(locationRelativeMouse))
                {
                    if (_month == 1) // going back one year
                    {
                        SetDate(_year - 1, MonthsInYear, SelectedDay);
                    }
                    else
                    {
                        SetDate(_year, _month - 1, SelectedDay);
                    }
                }
                else if (NextMonthButtonBounds().Contains(locationRelativeMouse))
                {
                    if (_month == MonthsInYear) // go to next year
                    {
                        SetDate(_year + 1, month: 1, SelectedDay);
                    }
                    else
                    {
                        SetDate(_year, _month + 1, SelectedDay);
                    }
                }
            }
        }

        private bool TryHitDay(Point locationRelativeMouse, out int day)
        {
            Point removedOffset = locationRelativeMouse - GetButtonSectionOffset();

            int weekRowIndex = removedOffset.Y / (ButtonHeight + ButtonMargin);
            int dayColumnIndex = removedOffset.X / (ButtonHeight + ButtonMargin);

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
