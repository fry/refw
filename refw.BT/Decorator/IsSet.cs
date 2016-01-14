using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace refw.BT {
    [DefaultBehavior]
    public class IsSet: Condition {
        public string Source;

        public IsSet() {
            Predicate = BehaviorProperty<bool>.Func(bb => bb.Contains(Source) && bb.Get<object>(Source) != null);
        }
    }
}
