using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace refw.BT {
    [DefaultBehavior]
    public class FixedResult: Decorator {
        public BehaviorProperty<Status> FixedStatus;

        protected override Status Update(Blackboard blackboard) {
            if (Child.TickUpdate(blackboard) != Status.Running)
                return FixedStatus.GetValue(blackboard);
            return Status.Running;
        }

        public override bool CheckCondition(Blackboard blackboard) {
            var fstatus = FixedStatus.GetValue(blackboard);
            if (fstatus == Status.Failure && !Child.CheckCondition(blackboard))
                return false;

            return true;
        }
    }
}
