using System;
using System.Collections;
using System.Collections.Generic;

namespace SideKicker.Panels.DesktopSearcher
{
    internal class LastSearcherResultList : IReadOnlyList<SearcherResult>
    {
        private const int MaxResultCount = 6;

        private int _count;
        private readonly SearcherResult[] _items;
        private readonly IEqualityComparer<SearcherResult> _comparer;

        public int Count => _count;

        public LastSearcherResultList()
        {
            _items = new SearcherResult[MaxResultCount];
            _comparer = EqualityComparer<SearcherResult>.Create((f, s) => f.FileAbsolutePath == s.FileAbsolutePath,
                                                                item => item.FileAbsolutePath.GetHashCode());
        }

        public SearcherResult this[int index]
        {
            get
            {
                if (index >= Count) throw new IndexOutOfRangeException();

                return _items[index];
            }
        }

        public void Enqueue(SearcherResult item)
        {
            int index = _items.AsSpan().Slice(0, _count).IndexOf(item, _comparer);

            // if present and on the top, skip it
            if (index == 0)
            {
                return;
            }
            else if (index > 0) // bring as first if present
            {
                Array.Copy(_items, 0, _items, 1, index);
                _items[0] = item;

                return;
            }

            if (_count == _items.Length)
            {
                Array.Copy(_items, 0, _items, 1, _items.Length - 1);
            }
            else
            {
                Array.Copy(_items, 0, _items, 1, _count);
                _count++;
            }

            _items[0] = item;
        }

        public IEnumerator<SearcherResult> GetEnumerator()
        {
            for (int i = 0; i < _count; i++)
            {
                yield return _items[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
