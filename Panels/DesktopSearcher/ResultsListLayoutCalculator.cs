using Microsoft.Xna.Framework;
using SideKicker.Rendering;
using System;

namespace SideKicker.Panels.DesktopSearcher
{
    internal class ResultsListLayoutCalculator : IButtonListLayoutCalculator<ResultsListButtonRegion>
    {
        public const int ButtonHeight = 54;
        public const int FileNameTextSize = 16;
        public const int FilePathTextSize = 12;

        private const int IconSideLength = 32;
        private const int NameTextMargin = 4;
        private const int IconMargin = 8;
        private const int ButtonContentCenterizingMargin = (ButtonHeight - IconSideLength) / 2;

        private static readonly int FileNameTextLineSpacing = FontService.Regular.GetLineSpacing(FileNameTextSize);
        private static readonly int FilePathTextLineSpacing = FontService.Regular.GetLineSpacing(FilePathTextSize);

        public static ResultsListLayoutCalculator Instance { get; } = new ResultsListLayoutCalculator();

        private ResultsListLayoutCalculator() { }

        public bool HitCloseButton(Rectangle localButtonBounds, Point relativeMouseToHovered)
        {
            return ToCommonIconRegion(ResultsListButtonRegion.CloseButton, localButtonBounds).Contains(relativeMouseToHovered);
        }

        public Rectangle ToCachedFileIconRegion(ResultsListButtonRegion region, Rectangle localButtonBounds)
        {
            if (region == ResultsListButtonRegion.FileIcon)
            {
                localButtonBounds.X += IconMargin;
                localButtonBounds.Y += ButtonContentCenterizingMargin;
                localButtonBounds.Width = IconSideLength;
                localButtonBounds.Height = IconSideLength;

                return localButtonBounds;
            }

            throw new ArgumentException("Region not supported " + region);
        }

        public Rectangle ToSlidingTextRegion(ResultsListButtonRegion region, Rectangle localButtonBounds)
        {
            localButtonBounds.X += 2 * IconMargin + IconSideLength + NameTextMargin;
            localButtonBounds.Y += (localButtonBounds.Height - FileNameTextLineSpacing - FilePathTextLineSpacing) / 2;
            localButtonBounds.Width -= NameTextMargin + IconSideLength + 2 * IconMargin;

            if (region == ResultsListButtonRegion.FileName)
            {
                localButtonBounds.Height = FileNameTextLineSpacing;

                return localButtonBounds;
            }
            else if (region == ResultsListButtonRegion.FileRelativePath)
            {
                localButtonBounds.Y += FileNameTextLineSpacing;
                localButtonBounds.Height = FilePathTextLineSpacing;

                return localButtonBounds;
            }

            throw new ArgumentException("Region not supported " + region);
        }

        public Rectangle ToCommonIconRegion(ResultsListButtonRegion region, Rectangle localButtonBounds)
        {
            if (region == ResultsListButtonRegion.CloseButton)
            {
                localButtonBounds.X = localButtonBounds.Right - ButtonContentCenterizingMargin - IconSideLength;
                localButtonBounds.Y += ButtonContentCenterizingMargin;
                localButtonBounds.Width = IconSideLength;
                localButtonBounds.Height = IconSideLength;

                return localButtonBounds;
            }

            throw new ArgumentException("Region not supported " + region);
        }

        public int GetSlidingTextSize(ResultsListButtonRegion region)
        {
            return region switch
            {
                ResultsListButtonRegion.FileName => FileNameTextSize,
                ResultsListButtonRegion.FileRelativePath => FilePathTextSize,

                _ => throw new ArgumentException("Region not supported " + region)
            };
        }

        public bool TryGetHoveredCommonIconRegion(Point relativeMouseToHovered, Point buttonSize, out ResultsListButtonRegion region)
        {
            Rectangle closeButton = ToCommonIconRegion(ResultsListButtonRegion.CloseButton, new Rectangle(Point.Zero, buttonSize));
            if (closeButton.Contains(relativeMouseToHovered))
            {
                region = ResultsListButtonRegion.CloseButton;
                return true;
            }

            region = default;
            return false;
        }
    }
}
