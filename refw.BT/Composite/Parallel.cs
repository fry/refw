using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace refw.BT {
    [DefaultBehavior]
    public class Parallel: Composite {
        public Policy FailurePolicy = Policy.One;
        public Policy SuccessPolicy = Policy.All;

        private bool abortMode;
        private Status abortResultStatus;

        protected override void OnInitialize(Blackboard blackboard) {
            // Prevent a deadlock
            Debug.Assert(FailurePolicy != Policy.All || SuccessPolicy != Policy.All);
            abortMode = false;
        }

        protected override Status Update(Blackboard blackboard) {
            // We should cancel our running children and return our final status
            if (abortMode) {
                if (Abort(blackboard, false) != Status.Aborted)
                    return Status.Running;
                return abortResultStatus;
            }

            var success_count = 0;
            var failure_count = 0;

            // Tick all children that haven't succeeded yet.
            // Depending on the policy, fail if one child fails, or succeed if one succeeds.
            // Abort all other children
            foreach (var child in Children) {
                var status = child.IsFinished ? child.Status : child.TickUpdate(blackboard);
                if (status == Status.Success) {
                    success_count ++;
                    // One child succeeded and we should abort the remaining children and succeed
                    if (SuccessPolicy == Policy.One) {
                        if (Abort(blackboard, false) == Status.Aborted)
                            return Status.Success;

                        abortResultStatus = Status.Success;
                        abortMode = true;
                        return Status.Running;
                    }
                } else if (status == Status.Failure) {
                    failure_count ++;
                    // One child failed and we should abort the remaining children and fail
                    if (FailurePolicy == Policy.One) {
                        if (Abort(blackboard, false) == Status.Aborted)
                            return Status.Failure;

                        abortResultStatus = Status.Failure;
                        abortMode = true;
                        return Status.Running;
                    }
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

        protected override Status Abort(Blackboard blackboard, bool forced) {
            // Abort all children
            int aborting_count = 0;
            foreach (var child in Children) {
                var status = (child.IsFinished || child.Status == Status.Invalid) ? child.Status : child.TickAbort(blackboard, forced);
                if (status == Status.Aborting)
                    aborting_count++;
            }

            if (aborting_count > 0)
                return Status.Aborting;

            return Status.Aborted;
        }

        public override bool CheckCondition(Blackboard blackboard) {
            var success_count = 0;
            var failure_count = 0;

            foreach (var child in Children) {
                var result = child.CheckCondition(blackboard);
                if (!result) {
                    failure_count ++;
                    if (FailurePolicy == Policy.One)
                        return false;
                } else {
                    success_count++;
                    if (SuccessPolicy == Policy.One)
                        return true;
                }
            }

            // Succeed/Fail depending on Policy.All
            if (FailurePolicy == Policy.All && failure_count == Children.Count)
                return false;

            if (SuccessPolicy == Policy.All && success_count == Children.Count)
                return true;

            return true;
        }
    }
}
