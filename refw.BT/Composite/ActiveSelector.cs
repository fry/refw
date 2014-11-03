using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace refw.BT {
    /// <summary>
    /// Attempts to run its children in order, until one succeeds, or it reaches the end of its children.
    /// Unlike the normal Selector, the ActiveSelector continously re-checks children, that are Conditions,
    /// infront of the currently active child if they can run, and then attempts to abort the currently
    /// active behavior.
    /// </summary>
    public class ActiveSelector : Composite {
        int currentChild;

        protected override void OnInitialize(Blackboard blackboard) {
            currentChild = 0;
        }

        protected override Status Update(Blackboard blackboard) {
            // Check if any child up to our current child can execute
            // if so, it becomes our new current child
            for (int i = 0; i < currentChild; i++) {
                var child = Children[i];
                var can_run = (child is Condition) && ((Condition) child).CheckPredicate(blackboard);
                if (can_run) {
                    // Attempt to abort the child, and yield if it can't abort yet
                    var current = Children[currentChild];
                    if (current.TickAbort(blackboard) != Status.Aborted)
                        return current.Status;

                    currentChild = i;
                    break;
                }
            }

            // Otherwise execute children in order until we
            // reach the end or one succeeds
            while (currentChild < Children.Count) {
                var status = Children[currentChild].TickUpdate(blackboard);

                if (status != Status.Failure)
                    return status;

                currentChild ++;
            }

            return Status.Failure;
        }

        protected override Status Abort(Blackboard blackboard) {
            Status = Children[currentChild].TickAbort(blackboard);
            if (Status == Status.Aborted)
                Reset();
            return Status;
        }
    }
}
