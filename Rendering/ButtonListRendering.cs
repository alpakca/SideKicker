using System;
using System.Collections.Generic;

namespace SideKicker.Rendering
{
    internal delegate string LayoutValueGatherer<TRegion, TSource>(TRegion region, TSource source);
    internal delegate bool CommonIconEligibility<TRegion>(TRegion region, int buttonIndex);

    internal class ButtonListRendering<TSource, TRegion>
    {
        private readonly LayoutValueGatherer<TRegion, TSource> _cachedFileIconGatherer;
        private readonly LayoutValueGatherer<TRegion, TSource> _slidingTextGatherer;
        private readonly CommonIconEligibility<TRegion> _commonIconPredicate; // button index

        private readonly List<TRegion> _cachedFileIconRegions;
        private readonly List<(TRegion, int TextSize)> _slidingTextRegions;
        private readonly List<(TRegion, IconType)> _commonIconRegions;

        public IEnumerable<TRegion> CachedFileIconRegions => _cachedFileIconRegions;
        public IEnumerable<TRegion> SlidingTextRegions
        {
            get
            {
                foreach (var (region, _) in _slidingTextRegions) yield return region;
            }
        }
        public IEnumerable<TRegion> CommonIconRegions
        {
            get
            {
                foreach (var (region, _) in _commonIconRegions) yield return region;
            }
        }

        public ButtonListRendering(LayoutValueGatherer<TRegion, TSource>? cachedFileIconGatherer, LayoutValueGatherer<TRegion, TSource>? slidingTextGatherer,
                                CommonIconEligibility<TRegion> commonIconPredicate)
        {
            _cachedFileIconGatherer = cachedFileIconGatherer;
            _slidingTextGatherer = slidingTextGatherer;
            _commonIconPredicate = commonIconPredicate;

            _cachedFileIconRegions = new List<TRegion>();
            _slidingTextRegions = new List<(TRegion, int TextSize)>();
            _commonIconRegions = new List<(TRegion, IconType)>();
        }

        public string GetCachedFileIconKey(TRegion region, TSource source) => _cachedFileIconGatherer(region, source);
        public string GetSlidingTextResult(TRegion region, TSource source) => _slidingTextGatherer(region, source);

        public bool TryGetRegionCommonIcon(TRegion region, int buttonIndex, out IconType iconType)
        {
            if (_commonIconPredicate(region, buttonIndex))
            {
                foreach (var (regionCandidate, iconCandidate) in _commonIconRegions)
                {
                    if (EqualityComparer<TRegion>.Default.Equals(regionCandidate, region))
                    {
                        iconType = iconCandidate;
                        return true;
                    }
                }
            }

            iconType = default;
            return false;
        }

        public int GetSlidingTextTextSize(TRegion region)
        {
            foreach (var (regionCandidate, textSize) in _slidingTextRegions)
            {
                if (EqualityComparer<TRegion>.Default.Equals(regionCandidate, region))
                {
                    return textSize;
                }
            }

            throw new ArgumentException();
        }

        public bool IsCommonIconRegionEligible(TRegion region, int buttonIndex)
        {
            foreach (var (regionCandidate, _) in _commonIconRegions)
            {
                if (EqualityComparer<TRegion>.Default.Equals(regionCandidate, region))
                {
                    return _commonIconPredicate(region, buttonIndex);
                }
            }

            return false;
        }

        public void AddRegionCachedFileIcon(TRegion region)
        {
            _cachedFileIconRegions.Add(region);
        }

        public void AddRegionSlidingText(TRegion region, int textSize)
        {
            _slidingTextRegions.Add((region, textSize));
        }

        public void AddRegionCommonIcon(TRegion region, IconType iconType)
        {
            _commonIconRegions.Add((region, iconType));
        }
    }
}
