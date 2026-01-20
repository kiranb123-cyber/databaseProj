using Simpledb;
using System;
using System.Collections.Generic;

namespace SimpleDbIndexes
{
    public interface IIndex<TKey, TValue>
    {
        bool TryGet(TKey key, out TValue value);
        void Upsert(TKey key, TValue value);
        bool Remove(TKey key);
        IEnumerable<KeyValuePair<TKey, TValue>> Scan();
    }
}