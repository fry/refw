using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace refw.BT {
    public class Condition : Decorator {
        public BehaviorProperty<bool> Predicate = null;

        protected override Status Update(Blackboard blackboard) {
            if (Predicate.GetValue(blackboard)) {
                if (Child != null)
                    return Child.Tick(blackboard);
                return Status.Success;
            }
            return Status.Failure;
        }
    }
}
