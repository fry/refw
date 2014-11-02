using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace refw.BT {
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
                var status = child.Tick(blackboard);
                if (status != Status.Failure) {
                    // Reset the previous child
                    Children[currentChild].Reset();
                    currentChild = i;
                    return status;
                }
            }

            // Otherwise execute children in order until we
            // reach the end or one succeeds
            while (currentChild < Children.Count) {
                var status = Children[currentChild].Tick(blackboard);

                if (status != Status.Failure)
                    return status;

                currentChild ++;
            }

            return Status.Failure;
        }
    }
}
