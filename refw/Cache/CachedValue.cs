using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace refw {
    namespace Cache {
        public abstract class CachedValue<T> {
            bool forceRefresh = false;
            readonly Func<T> getValue;
            T cachedValue = default(T);

            public CachedValue(Func<T> getValue) {
                this.getValue = getValue;
            }

            public T Value {
                get {
                    if (!IsValid || forceRefresh) {
                        cachedValue = getValue();
                        OnCacheUpdated();
                    }
                    forceRefresh = false;
                    return cachedValue;
                }
            }

            public static implicit operator T(CachedValue<T> cached) {
                return cached.Value;
            }

            public void Invalidate() {
                forceRefresh = true;
            }

            public abstract bool IsValid {
                get;
            }

            protected abstract void OnCacheUpdated();
        }
    }
}
