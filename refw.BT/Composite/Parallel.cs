using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace refw.BT {
    public class Parallel: Composite {
        public Policy FailurePolicy = Policy.One;
        public Policy SuccessPolicy = Policy.All;

        protected override Status Update(Blackboard blackboard) {
            var success_count = 0;
            var failure_count = 0;

            // Tick all children that haven't succeeded yet.
            // Depending on the policy, fail if one child fails, or succeed if one succeeds.
            // Abort all other children
            foreach (var child in Children) {
                var status = child.IsFinished ? child.Status : child.TickUpdate(blackboard);
                if (status == Status.Success) {
                    success_count ++;
                    if (SuccessPolicy == Policy.One && TickAbort(blackboard) != Status.Aborting)
                        return Status.Success;
                } else if (status == Status.Failure) {
                    failure_count ++;
                    if (FailurePolicy == Policy.One && TickAbort(blackboard) != Status.Aborting)
                        return Status.Failure;
                }
            }

            // Succeed/Fail depending on Policy.All
            if (FailurePolicy == Policy.All && failure_count == Children.Count)
                return Status.Failure;

            if (SuccessPolicy == Policy.All && success_count == Children.Count)
                return Status.Success;

            return Status.Running;
        }

        protected override void OnTerminate(Status status) {
            foreach (var behavior in Children)
                behavior.Reset();
        }

        protected override Status Abort(Blackboard blackboard) {
            // Abort all children
            int aborting_count = 0;
            foreach (var child in Children) {
                var status = child.IsFinished ? child.Status : child.TickAbort(blackboard);
                if (status == Status.Aborting)
                    aborting_count++;
            }

            if (aborting_count > 0)
                return Status.Aborting;

            return Status.Aborted;
        }
    }
}
