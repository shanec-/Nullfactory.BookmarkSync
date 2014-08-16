using System;
using System.Collections.Generic;
using System.Linq;

namespace Nullfactory.PhoneClient.Helpers
{
    public class HashSet<T> : ICollection<T>
    {
        private List<T> _InternalCollection;
        private IEqualityComparer<T> _EqualityComparer;

        public HashSet()
            : this((IEqualityComparer<T>)EqualityComparer<T>.Default)
        {
        }

        public HashSet(IEqualityComparer<T> comparer)
        {
            if (comparer == null)
                _EqualityComparer = (IEqualityComparer<T>) EqualityComparer<T>.Default;

            _InternalCollection = new List<T>();
            _EqualityComparer = comparer;
        }

        public void Add(T item)
        {
            if (!_InternalCollection.Contains(item, _EqualityComparer))
                _InternalCollection.Add(item);
        }

        public void Clear()
        {
            _InternalCollection.Clear();
        }

        public bool Contains(T item)
        {
            return _InternalCollection.Contains(item, _EqualityComparer);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { return _InternalCollection.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            return _InternalCollection.Remove(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _InternalCollection.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _InternalCollection.GetEnumerator();
        }

        public List<T> ToList()
        {
            return _InternalCollection;
        }
    }
}
