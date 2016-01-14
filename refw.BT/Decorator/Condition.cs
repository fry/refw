using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace refw.BT {
    [DefaultBehavior]
    public class Condition : Decorator {
        public BehaviorProperty<bool> Predicate = null;
        public BehaviorProperty<bool> Negate = false;

        private bool shouldRun;
        protected override void OnInitialize(Blackboard blackboard) {
            shouldRun = Predicate.GetValue(blackboard);
            if (Negate.GetValue(blackboard))
                shouldRun = !shouldRun;
        }

        protected override Status Update(Blackboard blackboard) {
            if (shouldRun) {
                if (Child != null)
                    return Child.TickUpdate(blackboard);
                return Status.Success;
            }
            return Status.Failure;
        }

        public override bool CheckCondition(Blackboard blackboard) {
            if (!Predicate.GetValue(blackboard) && !Negate.GetValue(blackboard))
                return false;

            // If we have a child, and it can't run, abort. 
            // If we don't have a child, but we can run, continue
            if (Child != null && !Child.CheckCondition(blackboard))
                return false;

            return true;
        }
    }
}
