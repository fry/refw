using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace refw.BT {
    public class Repeater: Decorator {
        public BehaviorProperty<int> Count = null;
        
        private int? currentCount = null;

        protected override void OnInitialize(Blackboard blackboard) {
            currentCount = Count.GetValue(blackboard);
        }

        protected override Status Update(Blackboard blackboard) {
            if (currentCount.HasValue) {
                if (currentCount.Value <= 0)
                    return Status.Success;
            }
            
            var result = Child.TickUpdate(blackboard);

            // Decrement counter, if set, otherwise
            if (currentCount.HasValue)
                currentCount--;
            Child.Reset();

            return Status.Running;
        }
    }
}
