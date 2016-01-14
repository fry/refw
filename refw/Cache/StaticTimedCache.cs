using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace refw.Cache {
    public class StaticTimedCache : StaticCache {
        private Dictionary<object, DateTime> cacheExpiration = new Dictionary<object, DateTime>();

        private TimeSpan expireTime;
        public StaticTimedCache(TimeSpan expireTime) {
            this.expireTime = expireTime;
        }

        public void ClearExpired() {
            var clear_keys = new List<object>();
            foreach (var entr in cacheExpiration) {
                if (DateTime.Now > entr.Value) {
                    clear_keys.Add(entr.Key);
                }
            }

            foreach (var clear_key in clear_keys) {
                cache.Remove(clear_key);
                cacheExpiration.Remove(clear_key);
            }
        }

        protected override void OnCacheAccess(object key) {
            cacheExpiration[key] = DateTime.Now.Add(expireTime);
        }

        protected override void OnCacheSet(object key) {
            ClearExpired();
        }

        public override void Clear() {
            cacheExpiration.Clear();
            base.Clear();
        }
    }
}
