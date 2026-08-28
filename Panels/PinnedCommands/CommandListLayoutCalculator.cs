using Microsoft.Xna.Framework;
using PopUpUI.Toolkit;

namespace SideKicker.Panels.PinnedCommands
{
    internal static class CommandListLayoutCalculator
    {
        public const int ButtonHeight = 32;
        private const int NameTextMargin = 4;

        private const int SlidingTextPauseStart = 1; // seconds
        private const int SlidingTextPauseEnd = 1; // seconds
        private const int SlidingTextSpeed = 40; // pixels/second

        public static CommandButtonLayout GetLayout(Point buttonSize, Point relativeMousePosition)
        {
            if (0 <= relativeMousePosition.X && relativeMousePosition.X < ButtonHeight &&
                0 <= relativeMousePosition.Y && relativeMousePosition.Y < ButtonHeight)
            {
                return CommandButtonLayout.ReorderButton;
            }

            if (buttonSize.X - ButtonHeight <= relativeMousePosition.X && relativeMousePosition.X < buttonSize.X &&
                0 <= relativeMousePosition.Y && relativeMousePosition.Y < ButtonHeight)
            {
                return CommandButtonLayout.EditButton;
            }

            return CommandButtonLayout.Body;
        }

        public static Rectangle ReorderButtonBounds(Rectangle buttonScreenBounds)
        {
            return new Rectangle(buttonScreenBounds.X, buttonScreenBounds.Y, ButtonHeight, ButtonHeight);
        }

        public static Rectangle EditButtonBounds(Rectangle buttonScreenBounds)
        {
            return new Rectangle(buttonScreenBounds.Right - ButtonHeight, buttonScreenBounds.Y, ButtonHeight, ButtonHeight);
        }


        public static Rectangle ButtonFileIconBounds(Rectangle buttonScreenBounds)
        {
            return new Rectangle(buttonScreenBounds.X + ButtonHeight, buttonScreenBounds.Y, ButtonHeight, ButtonHeight);
        }

        public static Rectangle ButtonNameBounds(Rectangle buttonScreenBounds)
        {
            return new Rectangle(buttonScreenBounds.X + 2 * ButtonHeight + NameTextMargin, buttonScreenBounds.Y,
                                 ButtonNameMaxDisplayWidth(buttonScreenBounds.Width), buttonScreenBounds.Height);
        }

        public static int ButtonNameMaxDisplayWidth(int buttonScreenWidth)
        {
            // 3 heights: reorder button, file icon, edit button
            return buttonScreenWidth - 3 * ButtonHeight - NameTextMargin;
        }
        
        public static float GetCommandListTextSlideOffset(float textWidth, int buttonScreenWidth)
        {
            float scrollDistance = textWidth - ButtonNameMaxDisplayWidth(buttonScreenWidth);
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
