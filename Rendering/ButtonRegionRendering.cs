using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace SideKicker.Rendering
{
    internal delegate void LayoutRenderer(Rectangle layoutBounds, int buttonIndex);

    internal class ButtonRegionRendering<TRegion> where TRegion : struct, Enum
    {
        private readonly List<(TRegion, LayoutRenderer)> _renderers;

        public ButtonRegionRendering<TRegion> Define(TRegion region, LayoutRenderer renderer)
        {
            _renderers.Add((region, renderer));
            return this;
        }

        public LayoutRenderer GetRenderer(TRegion region)
        {
            foreach (var (regionCandidate, renderer) in _renderers)
            {
                if (EqualityComparer<TRegion>.Default.Equals(region, regionCandidate))
                {
                    return renderer;
                }
            }

            throw new ArgumentException("Renderer for region doesn't exist " + region);
        }
    }
}
