using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace refw.Cache {
    public class StaticCachedValue<T>: CachedValue<T> {
        bool isCached = false;

        public StaticCachedValue(Func<T> getValue): base(getValue) {
        }

        public override bool IsValid {
            get {
                return isCached && cachedValue != null;
            }
        }

        protected override void OnCacheUpdated() {
            isCached = true;
        }
    }
}
