using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace refw.BT {
    public class IsSet: Condition {
        public string Source;

        protected override void OnInitialize(Blackboard blackboard) {
            if (Predicate == null) {
                Predicate = BehaviorProperty<bool>.Func(bb => bb.Contains(Source) && bb.Get<object>(Source) != null);
            }
        }
    }
}
