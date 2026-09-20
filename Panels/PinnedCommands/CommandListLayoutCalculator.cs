using Microsoft.Xna.Framework;
using SideKicker.Rendering;
using System;

namespace SideKicker.Panels.PinnedCommands
{
    internal class CommandListLayoutCalculator : IButtonListLayoutCalculator<CommandsListButtonRegion>
    {
        public const int IconSideLength = 32;
        public const int PanelTextSize = 12;
        private const int NameTextMargin = 4;

        public static CommandListLayoutCalculator Instance { get; } = new CommandListLayoutCalculator();

        public Rectangle ToCachedFileIconRegion(CommandsListButtonRegion region, Rectangle localButtonBounds)
        {
            if (region == CommandsListButtonRegion.FileIcon)
            {
                localButtonBounds.X += IconSideLength;
                localButtonBounds.Width = IconSideLength;
                localButtonBounds.Height = IconSideLength;

                return localButtonBounds;
            }

            throw new ArgumentException();
        }

        public Rectangle ToSlidingTextRegion(CommandsListButtonRegion region, Rectangle localButtonBounds)
        {
            if (region == CommandsListButtonRegion.SavedName)
            {
                localButtonBounds.X += 2 * IconSideLength + NameTextMargin;
                localButtonBounds.Width -= 3 * IconSideLength - NameTextMargin;
                localButtonBounds.Height = IconSideLength;

                return localButtonBounds;
            }

            throw new ArgumentException();
        }

        public Rectangle ToCommonIconRegion(CommandsListButtonRegion region, Rectangle localButtonBounds)
        {
            if (region == CommandsListButtonRegion.ReorderButton)
            {
                localButtonBounds.Width = IconSideLength;
                localButtonBounds.Height = IconSideLength;

                return localButtonBounds;
            }
            else if (region == CommandsListButtonRegion.EditButton)
            {
                localButtonBounds.X = localButtonBounds.Right - IconSideLength;
                localButtonBounds.Width = IconSideLength;
                localButtonBounds.Height = IconSideLength;

                return localButtonBounds;
            }

            throw new ArgumentException();
        }

        public int GetSlidingTextRegionTextSize(CommandsListButtonRegion region)
        {
            if (region == CommandsListButtonRegion.SavedName)
            {
                return PanelTextSize;
            }

            throw new ArgumentException();
        }

        public bool TryGetHoveredCommonIconRegion(Point relativeMouseToHovered, Point buttonSize, out CommandsListButtonRegion region)
        {
            Rectangle current = ToCommonIconRegion(CommandsListButtonRegion.ReorderButton, new Rectangle(Point.Zero, buttonSize));
            if (current.Contains(relativeMouseToHovered))
            {
                region = CommandsListButtonRegion.ReorderButton;
                return true;
            }

            current = ToCommonIconRegion(CommandsListButtonRegion.EditButton, new Rectangle(Point.Zero, buttonSize));
            if (current.Contains(relativeMouseToHovered))
            {
                region = CommandsListButtonRegion.EditButton;
                return true;
            }

            region = CommandsListButtonRegion.Body;
            return false;
        }
    }
}
