using Microsoft.Xna.Framework;

namespace SideKicker.Panels.PinnedCommands
{
    internal static class CommandListLayoutCalculator
    {
        public const int ButtonHeight = 32;

        public static CommandButtonLayout GetLayout(Rectangle buttonBounds, Point relativeMousePosition)
        {
            if (0 <= relativeMousePosition.X && relativeMousePosition.X < ButtonHeight &&
                0 <= relativeMousePosition.Y && relativeMousePosition.Y < ButtonHeight)
            {
                return CommandButtonLayout.ReorderButton;
            }

            if (buttonBounds.Right - ButtonHeight <= relativeMousePosition.X && relativeMousePosition.X < buttonBounds.Right &&
                0 <= relativeMousePosition.Y && relativeMousePosition.Y < ButtonHeight)
            {
                return CommandButtonLayout.EditButton;
            }

            return CommandButtonLayout.Body;
        }

        public static Rectangle ReorderButtonBounds(Rectangle buttonBounds)
        {
            return new Rectangle(buttonBounds.X, buttonBounds.Y, ButtonHeight, ButtonHeight);
        }
    }
}
