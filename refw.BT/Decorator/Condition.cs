using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace refw.BT {
    public class Condition : Decorator {
        public BehaviorProperty<bool> Predicate = null;

        protected override Status Update(Blackboard blackboard) {
            if (CheckPredicate(blackboard)) {
                if (Child != null)
                    return Child.TickUpdate(blackboard);
                return Status.Success;
            }
            return Status.Failure;
        }

        public bool CheckPredicate(Blackboard blackboard) {
            return Predicate.GetValue(blackboard);
        }
    }
}
