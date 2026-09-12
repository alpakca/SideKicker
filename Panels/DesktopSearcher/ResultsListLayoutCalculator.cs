using Microsoft.Xna.Framework;
using PopUpUI.Toolkit;

namespace SideKicker.Panels.DesktopSearcher
{
    internal static class ResultsListLayoutCalculator
    {
        public const int ButtonHeight = 54;
        private const int ButtonIconHeight = 32;
        private const int NameTextMargin = 4;
        private const int IconMargin = 8;

        private const int ButtonContentCenterizingMargin = (ButtonHeight - ButtonIconHeight) / 2;

        private const int SlidingTextPauseStart = 1; // seconds
        private const int SlidingTextPauseEnd = 1; // seconds
        private const int SlidingTextSpeed = 40; // pixels/second

        public static Rectangle ButtonNameBounds(Rectangle buttonScreenBounds)
        {
            return new Rectangle(buttonScreenBounds.X + 2 * IconMargin + ButtonIconHeight + NameTextMargin, buttonScreenBounds.Y,
                                 ButtonMaxNameWidth(buttonScreenBounds.Width), buttonScreenBounds.Height);
        }

        public static Rectangle ButtonIconBounds(Rectangle buttonScreenBounds)
        {
            return new Rectangle(buttonScreenBounds.X + IconMargin, buttonScreenBounds.Y + ButtonContentCenterizingMargin,
                                 ButtonIconHeight, ButtonIconHeight);
        }

        public static Rectangle ButtonCloseBounds(Rectangle buttonScreenBounds)
        {
            return new Rectangle(buttonScreenBounds.Right - ButtonContentCenterizingMargin - ButtonIconHeight,
                                 buttonScreenBounds.Y + ButtonContentCenterizingMargin,
                                 ButtonIconHeight, ButtonIconHeight);
        }

        public static bool HitCloseBounds(Point buttonSize, Point relativeMousePosition)
        {
            return buttonSize.X - ButtonContentCenterizingMargin - ButtonHeight <= relativeMousePosition.X && 
                   relativeMousePosition.X < buttonSize.X - ButtonContentCenterizingMargin &&

                   ButtonContentCenterizingMargin <= relativeMousePosition.Y && 
                   relativeMousePosition.Y <= buttonSize.Y - ButtonContentCenterizingMargin;
        }
        
        private static int ButtonMaxNameWidth(int buttonWidth)
        {
            return buttonWidth - NameTextMargin - ButtonIconHeight - 2 * IconMargin;
        }

        public static float GetTextSlideOffset(float textWidth, int buttonScreenWidth)
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
