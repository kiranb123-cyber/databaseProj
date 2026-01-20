using System;
using System.Collections.Generic;
using Simpledb;
using SimpleDbIndexes;
using System.Linq;

namespace SimpleDbIndexes
{
            //wraps dictionary in a hash index
        public sealed class HashIndex<TKey, TValue> : IIndex<TKey, TValue>
            where TKey : notnull
        {
            private readonly Dictionary<TKey, TValue> _dict = new();

            public bool TryGet(TKey key, out TValue value)
                => _dict.TryGetValue(key, out value);

            public void Upsert(TKey key, TValue value)
                => _dict[key] = value;

            public bool Remove(TKey key)
                => _dict.Remove(key);

            public IEnumerable<KeyValuePair<TKey, TValue>> Scan()
                => _dict;
        }
}
        
