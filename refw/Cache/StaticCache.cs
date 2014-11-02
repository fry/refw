using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace refw.Cache {
    public class StaticCache {
        public static uint CacheMisses { get; private set; }
        public static uint CacheHits { get; private set; }

        private readonly Dictionary<object, object> cache = new Dictionary<object, object>();

        public T Get<T>(object key) where T : class {
            if (!cache.ContainsKey(key)) {
                CacheMisses ++;
                return null;
            }

            CacheHits ++;
            return cache[key] as T;
        }

        public T Get<T>(object key, Func<T> calc) where T : class {
            object value;
            if (!cache.TryGetValue(key, out value)) {
                CacheMisses++;
                value = calc();
                cache[key] = value;
            } else {
                CacheHits++;
            }

            return value as T;
        }

        public void Set<T>(object key, T value) where T : class {
            cache[key] = value;
        }
    }
}
