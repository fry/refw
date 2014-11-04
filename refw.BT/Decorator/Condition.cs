using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace refw.BT {
    public class Condition : Decorator {
        public BehaviorProperty<bool> Predicate = null;

        private bool shouldRun;
        protected override void OnInitialize(Blackboard blackboard) {
            shouldRun = CheckCondition(blackboard);
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
            return Predicate.GetValue(blackboard);// && (Child == null || base.CheckCondition(blackboard));
        }
    }
}
