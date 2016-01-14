using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace refw.Cache {
    public class StaticCache {
        public static uint CacheMisses { get; private set; }
        public static uint CacheHits { get; private set; }

        protected readonly Dictionary<object, object> cache = new Dictionary<object, object>();

        public T Get<T>(object key) where T : class {
            if (!cache.ContainsKey(key)) {
                CacheMisses ++;
                return null;
            }

            CacheHits ++;
            OnCacheAccess(key);
            return cache[key] as T;
        }

        public List<T> GetAll<T>() {
            var results = cache.Values.OfType<T>().ToList();
            foreach (var result in results) {
                OnCacheAccess(result);
            }

            return results;
        }

        public List<T> Get<T>(Predicate<T> pred) {
            var results = cache.Values.OfType<T>().Where(p => pred(p)).ToList();
            foreach (var result in results) {
                OnCacheAccess(result);
            }

            return results;
        }

        public T Get<T>(object key, Func<T> calc) where T : class {
            object value;
            if (!cache.TryGetValue(key, out value)) {
                OnCacheSet(key);
                CacheMisses++;
                value = calc();
                cache[key] = value;
            } else {
                CacheHits++;
            }

            OnCacheAccess(key);
            return value as T;
        }

        public void Set<T>(object key, T value) where T : class {
            cache[key] = value;
            OnCacheAccess(key);
            OnCacheSet(key);
        }

        public virtual void Clear() {
            cache.Clear();
        }

        protected virtual void OnCacheAccess(object key) {
            
        }

        protected virtual void OnCacheSet(object key) {
            
        }
    }
}
