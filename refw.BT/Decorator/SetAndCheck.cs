using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace refw.BT {
    public class SetAndCheck: Condition {
        public string Destination = "out";
        public Func<object> Func = null;
        public Func<Blackboard, object> FuncBlackboard = null;

        protected override void OnInitialize(Blackboard blackboard) {
            if (Predicate == null) {
                Predicate = BehaviorProperty<bool>.Func(bb => {
                    object value;
                    if (Func != null)
                        value = Func();
                    else
                        value = FuncBlackboard(blackboard);

                    if (value == null)
                        return false;

                    bb.Set(Destination, value);
                    return true;
                });
            }

            base.OnInitialize(blackboard);
        }
    }
}
