using Microsoft.Xna.Framework;
using PopUpUI.Toolkit;
using SideKicker.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace SideKicker.Panels.Timers
{
    internal static class TimesListLayoutCalculator
    {
        public const int CalendarDaysTextSize = 16;
        public const int CalendarDayButtonMargin = 4;

        public const int TimesListButtonHeight = 32;
        private const int NameTextMargin = 4;

        private const int SlidingTextPauseStart = 1; // seconds
        private const int SlidingTextPauseEnd = 1; // seconds
        private const int SlidingTextSpeed = 40; // pixels/second

        public static readonly int CalendarDayButtonHeight = FontService.Regular.GetLineSpacing(CalendarDaysTextSize);

        public static Rectangle CalendarWeekDayBounds(int columnIndex)
        {
            return new Rectangle(CalendarDayButtonMargin + columnIndex * (CalendarDayButtonHeight + CalendarDayButtonMargin), 0, 
                                 CalendarDayButtonHeight, CalendarDayButtonHeight);
        }

        public static bool HitEditButton(Point buttonSize, Point relativeMousePosition)
        {
            return buttonSize.X - TimesListButtonHeight <= relativeMousePosition.X && relativeMousePosition.X < buttonSize.X &&
                   0 <= relativeMousePosition.Y && relativeMousePosition.Y < buttonSize.Y;
        }

        public static Rectangle EditButtonBounds(Rectangle buttonScreenBounds)
        {
            return new Rectangle(buttonScreenBounds.Right - TimesListButtonHeight, buttonScreenBounds.Y, TimesListButtonHeight, TimesListButtonHeight);
        }

        public static Rectangle ButtonNameBounds(Rectangle buttonScreenBounds)
        {
            return new Rectangle(buttonScreenBounds.X + NameTextMargin, buttonScreenBounds.Y, 
                                 ButtonMaxNameWidth(buttonScreenBounds.Width), buttonScreenBounds.Height);
        }

        private static int ButtonMaxNameWidth(int buttonWidth)
        {
            return buttonWidth - NameTextMargin - TimesListButtonHeight;
        }

        public static float GetTimesListTextSlideOffset(float textWidth, int buttonScreenWidth)
        {
            float scrollDistance = textWidth - ButtonMaxNameWidth(buttonScreenWidth);
            if (scrollDistance <= 0)
            {
                return 0;
            }

            float scrollTime = scrollDistance / SlidingTextSpeed;
            float cycleTime = SlidingTextPauseStart + scrollTime + SlidingTextPauseEnd;

            float currentCyleTimePosition = (float)(Time.ElapsedGameTime.TotalSeconds % cycleTime);

            if (currentCyleTimePosition < SlidingTextPauseStart) // start pause state
            {
                return 0;
            }

            if (currentCyleTimePosition < SlidingTextPauseStart + scrollTime) // sliding
            {
                return -(currentCyleTimePosition - SlidingTextPauseStart) * SlidingTextSpeed;
            }

            return -scrollDistance; // end pause state
        }
    }
}
