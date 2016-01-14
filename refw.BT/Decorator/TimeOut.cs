using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace refw.BT {
    [DefaultBehavior]
    public class TimeOut: Decorator {
        public BehaviorProperty<TimeSpan> Timeout;

        private DateTime timeoutTime;
        protected override void OnInitialize(Blackboard blackboard) {
            timeoutTime = DateTime.Now.Add(Timeout.GetValue(blackboard));
        }

        protected override Status Update(Blackboard blackboard) {
            var status = Child.TickUpdate(blackboard);
            if (status == Status.Running && DateTime.Now >= timeoutTime) {
                if (Child.TickAbort(blackboard) == Status.Aborted)
                    return Status.Failure;
            }
            return status;
        }

    }
}
