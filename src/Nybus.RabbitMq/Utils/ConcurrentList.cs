using System.Collections.Concurrent;

namespace Nybus.Utils
{
    public class ConcurrentList<T>
    {
        private const int DefaultValue = 0;
        private readonly ConcurrentDictionary<T, int> _innerDictionary = new ConcurrentDictionary<T, int>();

        public void Add(T item)
        {
            _innerDictionary.AddOrUpdate(item, i => DefaultValue, (k, v) => v);
        }

        public bool TryRemoveItem(T item)
        {
            return _innerDictionary.TryRemove(item, out int value);
        }

        public bool Contains(T item)
        {
            return _innerDictionary.ContainsKey(item);
        }

        public bool IsEmpty => _innerDictionary.IsEmpty;
    }
}