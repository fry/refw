using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace refw.BT {
    [DefaultBehavior]
    public class Throttled: Decorator {
        public BehaviorProperty<TimeSpan> Time;
        /// <summary>
        /// Throttle child no matter whether it fails or succeeds
        /// </summary>
        public BehaviorProperty<bool> Always = false;
        public BehaviorProperty<Status> ThrottleStatus = BT.Status.Success;
        public BehaviorProperty<Status> BlockOnStatus;

        private DateTime throttledUntil = DateTime.MinValue;

        protected override Status Update(Blackboard blackboard) {
            // If we're throttled, fail
            if (DateTime.Now < throttledUntil) {
                if (BlockOnStatus != null && Child.Status == BlockOnStatus.GetValue(blackboard))
                    return Status.Running;
                return Status.Failure;
            }

            // Otherwise, if we succeed, throttle
            var status = Child.TickUpdate(blackboard);
            if (status != Status.Running && (status == ThrottleStatus.GetValue(blackboard) || Always.GetValue(blackboard)))
                throttledUntil = DateTime.Now.Add(Time.GetValue(blackboard));

            return Child.Status;
        }

        protected override Status Abort(Blackboard blackboard, bool forced) {
            if (Always.GetValue(blackboard))
                throttledUntil = DateTime.Now.Add(Time.GetValue(blackboard));
            return base.Abort(blackboard, forced);
        }

        public override bool CheckCondition(Blackboard blackboard) {
            return DateTime.Now >= throttledUntil && base.CheckCondition(blackboard);
        }
    }
}
