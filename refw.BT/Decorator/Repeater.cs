using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace refw.BT {
    [DefaultBehavior]
    public class Repeater: Decorator {
        public BehaviorProperty<int> Count = null;
        
        private int? currentCount = null;
        private Status? lastStatus = null;

        protected override void OnInitialize(Blackboard blackboard) {
            currentCount = Count.GetValue(blackboard);
            lastStatus = null;
        }

        protected override Status Update(Blackboard blackboard) {
            if (currentCount.HasValue) {
                if (currentCount.Value <= 0) {
                    return lastStatus ?? Status.Failure;
                }
            }

            lastStatus = Child.TickUpdate(blackboard);
            if (lastStatus == Status.Running)
                return Status.Running;

            // Decrement counter, if set, otherwise
            if (currentCount.HasValue)
                currentCount--;

            Child.Reset();

            return Status.Running;
        }
    }
}
