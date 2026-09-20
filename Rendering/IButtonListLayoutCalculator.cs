using Microsoft.Xna.Framework;

namespace SideKicker.Rendering
{
    internal interface IButtonListLayoutCalculator<TRegion>
    {
        public Rectangle ToCachedFileIconRegion(TRegion region, Rectangle localButtonBounds);
        public Rectangle ToSlidingTextRegion(TRegion region, Rectangle localButtonBounds);
        public Rectangle ToCommonIconRegion(TRegion region, Rectangle localButtonBounds);

        public int GetSlidingTextRegionTextSize(TRegion region);
        public bool TryGetHoveredCommonIconRegion(Point relativeMouseToHovered, Point buttonSize, out TRegion region);
    }
}
