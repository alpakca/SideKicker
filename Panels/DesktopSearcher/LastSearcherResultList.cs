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

        public LastSearcherResultList(SearcherResult[] results)
        {
            _items = new SearcherResult[MaxResultCount];
            _comparer = EqualityComparer<SearcherResult>.Create((f, s) => f.FileAbsolutePath == s.FileAbsolutePath,
                                                                item => item.FileAbsolutePath.GetHashCode());

            Array.Copy(results, _items, results.Length);
            _count = results.Length;
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

        public void Remove(SearcherResult item)
        {
            int index = _items.AsSpan().Slice(0, _count).IndexOf(item, _comparer);
            if (index == -1) return;

            if (index == _count - 1)
            {
                _count--;
                return;
            }

            Array.Copy(_items, index + 1, _items, index, _count - index - 1);
            _count--;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(_items, _count);
        }

        IEnumerator<SearcherResult> IEnumerable<SearcherResult>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public struct Enumerator : IEnumerator<SearcherResult>
        {
            private readonly int _count;
            private readonly SearcherResult[] _array;

            private int _index;

            public readonly SearcherResult Current => _array[_index];
            readonly object IEnumerator.Current => _array[_index];

            public Enumerator(SearcherResult[] array, int count)
            {
                _count = count;
                _array = array;
                _index = -1;
            }

            public readonly void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (++_index >= _count)
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
