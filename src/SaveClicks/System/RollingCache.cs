#region using

using System;
using System.Collections;
using System.Collections.Generic;

#endregion

namespace SaveClicks.System
{
    /// <summary>
    ///   Fast implementation of a collection where if maximum number of items is reached it will start overwriting oldest
    ///   items.
    /// </summary>
    public class RollingCache<T> : ICollection<T>
    {
        private readonly IEqualityComparer<T> _defEqualityComparer = EqualityComparer<T>.Default;
        private int _index;
        private readonly T[] _items;

        public RollingCache(int maxSize)
        {
            _items = new T[maxSize];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        ///   Returns an enumerator that iterates through the buffer from newest to oldest item.
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            var count = 0;
            var index = _index;
            while (count++ < Count)
            {
                if (--index == -1) index = Count - 1;
                var item = _items[index];

                yield return item;
            }
        }

        public void Add(T item)
        {
            _items[_index] = item;
            _index = ++_index % _items.Length;

            if (Count < _items.Length) Count++;
        }

        public void Clear()
        {
            Count = 0;
        }

        public bool Contains(T item)
        {
            return Array.Exists(_items, i => _defEqualityComparer.Equals(i, item));
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            foreach (var item in this)
            {
                array[arrayIndex++] = item;
            }
        }

        public bool Remove(T item)
        {
            return false;
        }

        public int Count { get; private set; }

        public bool IsReadOnly => false;
    }
}