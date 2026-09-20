using Microsoft.Xna.Framework;
using SideKicker.Rendering;
using System;

namespace SideKicker.Panels.Timers
{
    internal class TimesListLayoutCalculator : IButtonListLayoutCalculator<TimesListRegion>
    {
        public const int CalendarDaysTextSize = 16;
        public const int CalendarDayButtonMargin = 4;

        public const int IconSideLength = 32;
        public const int TimesListTextSize = 12;
        private const int NameTextMargin = 4;

        public static readonly int CalendarDayButtonHeight = FontService.Regular.GetLineSpacing(CalendarDaysTextSize);

        public static TimesListLayoutCalculator Instance { get; } = new TimesListLayoutCalculator();

        public static Rectangle CalendarWeekDayBounds(int columnIndex)
        {
            return new Rectangle(CalendarDayButtonMargin + columnIndex * (CalendarDayButtonHeight + CalendarDayButtonMargin), 0, 
                                 CalendarDayButtonHeight, CalendarDayButtonHeight);
        }

        public Rectangle ToCachedFileIconRegion(TimesListRegion region, Rectangle localButtonBounds)
        {
            throw new ArgumentException();
        }

        public Rectangle ToSlidingTextRegion(TimesListRegion region, Rectangle localButtonBounds)
        {
            if (region == TimesListRegion.TimeInfoDescription)
            {
                localButtonBounds.X += NameTextMargin;
                localButtonBounds.Width -= NameTextMargin - IconSideLength;
                localButtonBounds.Height = IconSideLength;

                return localButtonBounds;
            }

            throw new ArgumentException();
        }

        public Rectangle ToCommonIconRegion(TimesListRegion region, Rectangle localButtonBounds)
        {
            if (region == TimesListRegion.EditButton)
            {
                localButtonBounds.X = localButtonBounds.Right - IconSideLength;
                localButtonBounds.Width = IconSideLength;
                localButtonBounds.Height = IconSideLength;

                return localButtonBounds;
            }

            throw new ArgumentException();
        }

        public int GetSlidingTextRegionTextSize(TimesListRegion region)
        {
            if (region == TimesListRegion.TimeInfoDescription)
            {
                return TimesListTextSize;
            }

            throw new ArgumentException();
        }

        public bool TryGetHoveredCommonIconRegion(Point relativeMouseToHovered, Point buttonSize, out TimesListRegion region)
        {
            Rectangle current = ToCommonIconRegion(TimesListRegion.EditButton, new Rectangle(Point.Zero, buttonSize));
            if (current.Contains(relativeMouseToHovered))
            {
                region = TimesListRegion.EditButton;
            }
            else
            {
                region = TimesListRegion.Body;
            }

            return true;
        }
    }
}
