using System;
using System.Collections.Generic;

namespace MMDataStructures
{
    public interface IDictionaryPersist<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>, IDisposable
    {
        int Count { get; }
        bool ContainsKey(TKey key);
        bool ContainsValue(TValue value);
        void Add(TKey key, TValue value);
        bool Remove(TKey key);
        bool TryGetValue(TKey key, out TValue value);
        bool ByteCompare(TValue value, TValue existing);
        IEnumerable<TKey> AllKeys();
        IEnumerable<TValue> AllValues();
        void Clear();
    }
}