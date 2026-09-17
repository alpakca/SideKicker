using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace SideKicker.Rendering
{
    internal delegate Rectangle LocalToScreenBoundsConverter(Rectangle localBounds);

    internal class ButtonRegionLayout<TRegion> where TRegion : struct, Enum
    {
        private readonly List<(TRegion, LocalToScreenBoundsConverter)> _converters;

        public ButtonRegionLayout<TRegion> Define(TRegion region, LocalToScreenBoundsConverter converter)
        {
            _converters.Add((region, converter));
            return this;
        }

        public Rectangle GetLocalBounds(TRegion region, Point size)
        {
            Rectangle local = new Rectangle(Point.Zero, size);
            foreach (var (regionCandidate, converter) in _converters)
            {
                if (EqualityComparer<TRegion>.Default.Equals(region, regionCandidate))
                {
                    return converter(local);
                }
            }

            return local;
        }

        public bool TryHitLocalBounds(Point size, Point relativeMouse, out TRegion region, out Rectangle regionBounds)
        {
            Rectangle local = new Rectangle(Point.Zero, size);
            foreach (var (regionCandidate, converter) in _converters)
            {
                Rectangle convertedCandidate = converter(local);
                if (convertedCandidate.Contains(relativeMouse))
                {
                    regionBounds = convertedCandidate;
                    region = regionCandidate;

                    return true;
                }
            }

            region = default;
            regionBounds = local;
            return false;
        }
    }
}
