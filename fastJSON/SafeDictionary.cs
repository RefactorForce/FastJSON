using System.Collections.Generic;

namespace FastJSON
{
    public sealed class SafeDictionary<TKey, TValue>
    {
        object Mutex { get; } = new object { };

        public Dictionary<TKey, TValue> Storage { get; }

        public SafeDictionary(int capacity) => Storage = new Dictionary<TKey, TValue>(capacity);

        public SafeDictionary() => Storage = new Dictionary<TKey, TValue>();

        public bool TryGetValue(TKey key, out TValue value)
        {
            lock (Mutex)
                return Storage.TryGetValue(key, out value);
        }

        public int Count()
        {
            lock (Mutex)
                return Storage.Count;
        }

        public TValue this[TKey key]
        {
            get
            {
                lock (Mutex)
                    return Storage[key];
            }
            set
            {
                lock (Mutex)
                    Storage[key] = value;
            }
        }

        public void Add(TKey key, TValue value)
        {
            lock (Mutex)
            {
                if (Storage.ContainsKey(key) == false)
                    Storage.Add(key, value);
            }
        }
    }
}
