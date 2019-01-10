using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Nybus.Utils
{
    public class TwoWayDictionary<T1, T2>: IEnumerable<(T1 first, T2 second)> //IDictionary<T1, T2>, IDictionary<T2, T1>, 
    {
        private readonly IDictionary<T1, Tuple<T1, T2>> _firstDictionary;
        private readonly IDictionary<T2, Tuple<T1, T2>> _secondDictionary;

        public TwoWayDictionary(IEqualityComparer<T1> firstEqualityComparer, IEqualityComparer<T2> secondEqualityComparer)
        {
            if (firstEqualityComparer == null)
            {
                throw new ArgumentNullException(nameof(firstEqualityComparer));
            }

            if (secondEqualityComparer == null)
            {
                throw new ArgumentNullException(nameof(secondEqualityComparer));
            }

            _firstDictionary = new Dictionary<T1, Tuple<T1, T2>>(firstEqualityComparer);
            _secondDictionary = new Dictionary<T2, Tuple<T1, T2>>(secondEqualityComparer);
        }

        public TwoWayDictionary(IEqualityComparer<T1> firstEqualityComparer) : this (firstEqualityComparer, EqualityComparer<T2>.Default) { }

        public TwoWayDictionary(IEqualityComparer<T2> secondEqualityComparer) : this(EqualityComparer<T1>.Default, secondEqualityComparer) { }

        public TwoWayDictionary() : this (EqualityComparer<T1>.Default, EqualityComparer<T2>.Default) { }

        public IEnumerator<(T1 first, T2 second)> GetEnumerator()
        {
            return _firstDictionary.Values.Select(item => (item.Item1, item.Item2)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private readonly object _lock = new object();

        public void Clear()
        {
            lock (_lock)
            {
                _firstDictionary.Clear();
                _secondDictionary.Clear();
            }
        }

        public void Add(T1 first, T2 second)
        {
            lock (_lock)
            {
                var value = new Tuple<T1, T2>(first, second);
                _firstDictionary.Add(first, value);
                _secondDictionary.Add(second, value);
            }
        }

        public bool ContainsKey(T1 key)
        {
            lock (_lock)
            {
                return _firstDictionary.ContainsKey(key);
            }
        }

        public bool ContainsKey(T2 key)
        {
            lock (_lock)
            {
                return _secondDictionary.ContainsKey(key);
            }
        }

        public bool Remove(T1 key)
        {
            lock (_lock)
            {
                if (_firstDictionary.TryGetValue(key, out var values))
                {
                    _firstDictionary.Remove(values.Item1);
                    _secondDictionary.Remove(values.Item2);

                    return true;
                }

                return false;
            }
        }

        public bool Remove(T2 key)
        {
            lock (_lock)
            {
                if (_secondDictionary.TryGetValue(key, out var values))
                {
                    _firstDictionary.Remove(values.Item1);
                    _secondDictionary.Remove(values.Item2);

                    return true;
                }

                return false;
            }
        }

        public bool TryGetValue(T1 key, out T2 value)
        {
            lock (_lock)
            {
                if (_firstDictionary.TryGetValue(key, out var values))
                {
                    value = values.Item2;
                    return true;
                }
                else
                {
                    value = default(T2);
                    return false;
                }
            }
        }

        public bool TryGetValue(T2 key, out T1 value)
        {
            lock (_lock)
            {
                if (_secondDictionary.TryGetValue(key, out var values))
                {
                    value = values.Item1;
                    return true;
                }
                else
                {
                    value = default(T1);
                    return false;
                }
            }
        }

        public T2 this[T1 key]
        {
            get
            {
                lock (_lock)
                {
                    return _firstDictionary[key].Item2;
                }
            }
            set
            {
                lock (_lock)
                {
                    var tuple = new Tuple<T1, T2>(key, value);
                    _firstDictionary[tuple.Item1] = tuple;
                    _secondDictionary[tuple.Item2] = tuple;
                }
            }
        }

        public T1 this[T2 key]
        {
            get
            {
                lock (_lock)
                {
                    return _secondDictionary[key].Item1;
                }
            }
            set
            {
                lock (_lock)
                {
                    var tuple = new Tuple<T1, T2>(value, key);
                    _firstDictionary[tuple.Item1] = tuple;
                    _secondDictionary[tuple.Item2] = tuple;
                }
            }
        }

        public ICollection<T1> FirstItems => _firstDictionary.Keys;

        public ICollection<T2> SecondItems => _secondDictionary.Keys;
    }
}