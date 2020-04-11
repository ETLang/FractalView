using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FractalView
{
    public class Relation<A,B> : IDictionary<A,B>, IEnumerable<KeyValuePair<A,B>>
    {
        private Dictionary<A, B> _ab = new Dictionary<A, B>();
        private Dictionary<B, A> _ba = new Dictionary<B, A>();

        public A this[B key]
        {
            get => ((IDictionary<B, A>)_ba)[key];
            set
            {
                _ba[key] = value;
                _ab[value] = key;
            }
        }

        public B this[A key]
        {
            get => ((IDictionary<A, B>)_ab)[key];
            set
            {
                _ab[key] = value;
                _ba[value] = key;
            }
        }

        public ICollection<A> As => _ab.Keys;

        public ICollection<B> Bs => _ab.Values;

        public int Count => _ab.Count;

        public bool IsReadOnly => false;

        ICollection<A> IDictionary<A, B>.Keys => ((IDictionary<A, B>)_ab).Keys;

        ICollection<B> IDictionary<A, B>.Values => ((IDictionary<A, B>)_ab).Values;

        public void Add(A key, B value)
        {
            if (_ba.ContainsKey(value))
                throw new Exception();

            _ab.Add(key, value);
            _ba.Add(value, key);
        }

        public void Add(B key, A value)
        {
            if (_ab.ContainsKey(value))
                throw new Exception();

            _ba.Add(key, value);
            _ab.Add(value, key);
        }

        public void Add(KeyValuePair<A, B> item)
        {
            if (_ba.ContainsKey(item.Value))
                throw new Exception();

            _ab.Add(item.Key, item.Value);
            _ba.Add(item.Value, item.Key);
        }

        public void Add(KeyValuePair<B, A> item)
        {
            if (_ab.ContainsKey(item.Value))
                throw new Exception();

            _ba.Add(item.Key, item.Value);
            _ab.Add(item.Value, item.Key);
        }

        public void Clear()
        {
            _ab.Clear();
            _ba.Clear();
        }

        public bool Contains(KeyValuePair<A, B> item)
        {
            return ((IDictionary<A, B>)_ab).Contains(item);
        }

        public bool Contains(KeyValuePair<B, A> item)
        {
            return ((IDictionary<B, A>)_ba).Contains(item);
        }

        public bool Contains(A key)
        {
            return ((IDictionary<A, B>)_ab).ContainsKey(key);
        }

        public bool Contains(B key)
        {
            return ((IDictionary<B, A>)_ba).ContainsKey(key);
        }

        bool IDictionary<A,B>.ContainsKey(A key)
        {
            return ((IDictionary<A, B>)_ab).ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<B, A>[] array, int arrayIndex)
        {
            ((IDictionary<B, A>)_ba).CopyTo(array, arrayIndex);
        }

        public void CopyTo(KeyValuePair<A, B>[] array, int arrayIndex)
        {
            ((IDictionary<A, B>)_ab).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<A, B>> GetEnumerator()
        {
            return _ab.GetEnumerator();
        }

        public bool Remove(A key)
        {
            if (!_ab.TryGetValue(key, out var val))
                return false;

            _ab.Remove(key);
            _ba.Remove(val);
            return true;
        }

        public bool Remove(B key)
        {
            if (!_ba.TryGetValue(key, out var val))
                return false;

            _ba.Remove(key);
            _ab.Remove(val);
            return true;
        }

        public bool Remove(KeyValuePair<B, A> item)
        {
            if (!_ab.ContainsKey(item.Value))
                return false;
            if (!_ba.ContainsKey(item.Key))
                return false;

            _ab.Remove(item.Value);
            _ba.Remove(item.Key);
            return true;
        }

        public bool Remove(KeyValuePair<A, B> item)
        {
            if (!_ba.ContainsKey(item.Value))
                return false;
            if (!_ab.ContainsKey(item.Key))
                return false;

            _ba.Remove(item.Value);
            _ab.Remove(item.Key);
            return true;
        }

        public bool TryGet(B key, out A value)
        {
            return ((IDictionary<B, A>)_ba).TryGetValue(key, out value);
        }

        public bool TryGet(A key, out B value)
        {
            return ((IDictionary<A, B>)_ab).TryGetValue(key, out value);
        }

        bool IDictionary<A, B>.TryGetValue(A key, out B value)
        {
            return ((IDictionary<A, B>)_ab).TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _ab.GetEnumerator();
        }

        IEnumerator<KeyValuePair<A, B>> IEnumerable<KeyValuePair<A, B>>.GetEnumerator()
        {
            return _ab.GetEnumerator();
        }
    }
}
