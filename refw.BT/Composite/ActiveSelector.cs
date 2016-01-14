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
    [DefaultBehavior]
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
                if (child.CheckCondition(blackboard)) {
                    // Attempt to abort the child, and yield if it can't abort yet
                    var current = Children[currentChild];
                    Log(String.Format("Aborting child {0} because {1} wants to run", currentChild, i));
                    if (current.TickAbort(blackboard) != Status.Aborted)
                        return Status.Running;

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

        protected override Status Abort(Blackboard blackboard, bool forced) {
            var child = Children[currentChild];
            if (!child.IsFinished)
                return child.TickAbort(blackboard, forced);
            return Status.Aborted;
        }

        public override bool CheckCondition(Blackboard blackboard) {
            foreach (var child in Children) {
                if (child.CheckCondition(blackboard))
                    return true;
            }

            return false;
        }
    }
}
