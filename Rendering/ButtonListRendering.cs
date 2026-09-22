using System;
using System.Collections;
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
        public TupleEnumerator<TRegion, int> SlidingTextRegions => new TupleEnumerator<TRegion, int>(_slidingTextRegions);
        public TupleEnumerator<TRegion, IconType> CommonIconRegions => new TupleEnumerator<TRegion, IconType>(_commonIconRegions);

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

        public struct TupleEnumerator<T, U> : IEnumerator<T>
        {
            private readonly List<(T, U)> _list;

            private int _index;

            public readonly T Current => _list[_index].Item1;
            readonly object IEnumerator.Current => Current;

            public TupleEnumerator(List<(T, U)> list)
            {
                _list = list;
                _index = -1;
            }

            public readonly TupleEnumerator<T, U> GetEnumerator()
            {
                return this;
            }

            public readonly void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (++_index >= _list.Count)
                    return false;

                return true;
            }

            public void Reset()
            {
                _index = -1;
            }
        }
    }
}
