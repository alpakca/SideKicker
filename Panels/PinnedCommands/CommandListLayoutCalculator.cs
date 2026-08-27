using Microsoft.Xna.Framework;

namespace SideKicker.Panels.PinnedCommands
{
    internal static class CommandListLayoutCalculator
    {
        public const int ButtonHeight = 32;
        private const int NameTextMargin = 4;

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

        public static Vector2 ButtonNamePosition(Rectangle buttonScreenBounds)
        {
            return new Vector2(buttonScreenBounds.X + ButtonHeight + NameTextMargin, buttonScreenBounds.Y);
        }
    }
}
