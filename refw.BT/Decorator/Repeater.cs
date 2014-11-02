using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace refw.BT {
    public enum RepeatPolicy {
        UntilSuccess,
        UntilFailure
    }

    public class Repeater: Decorator {
        public RepeatPolicy RepeatPolicy = RepeatPolicy.UntilFailure;
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
            
            var result = Child.Tick(blackboard);

            // Succeed depending on the repeat policy
            if (RepeatPolicy == RepeatPolicy.UntilFailure && result == Status.Failure)
                return Status.Success;
            else if (RepeatPolicy == RepeatPolicy.UntilSuccess && result == Status.Success)
                return Status.Success;

            // Decrement counter, if set, otherwise
            if (currentCount.HasValue)
                currentCount--;
            Child.Reset();

            return Status.Running;
        }
    }
}
