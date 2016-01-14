using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace refw.BT {
    public class ActiveListSelector : Decorator {
        public BehaviorProperty<IEnumerable<object>> List;

        public string VariableName = "current";

        private List<object> currentList;
        private int currentIndex;
        protected override void OnInitialize(Blackboard blackboard) {
            currentList = List.GetValue(blackboard).ToList();
            currentIndex = 0;
        }

        protected override Status Update(Blackboard blackboard) {
            // Check if the child runs for an earlier list index
            for (int i = 0; i < currentIndex; i++) {
                var item = currentList[i];
                if (CheckConditionFor(blackboard, item)) {
                    // Attempt to abort the child, and yield if it can't abort yet
                    Log(String.Format("Aborting child with item {0} because item {1} can run", currentIndex, i));

                    blackboard.Set(VariableName, currentList[currentIndex]);
                    if (Child.TickAbort(blackboard) != Status.Aborted)
                        return Status.Running;

                    currentIndex = i;
                    break;
                }
            }

            // Otherwise, run children in order
            while (true) {
                if (currentIndex >= currentList.Count)
                    return Status.Failure;

                blackboard.Set(VariableName, currentList[currentIndex]);
                if (Child.TickUpdate(blackboard) != Status.Failure)
                    return Child.Status;

                currentIndex++;
            } 
        }

        bool CheckConditionFor(Blackboard blackboard, object item) {
            object old_val = null;
            if (blackboard.Contains(VariableName))
                old_val = blackboard.Get<object>(VariableName);

            blackboard.Set(VariableName, item);

            bool can_run = Child.CheckCondition(blackboard);

            // Restore old blackboard value
            if (old_val != null)
                blackboard.Set(VariableName, old_val);

            return can_run;
        }

        public override bool CheckCondition(Blackboard blackboard) {
            foreach (var item in currentList) {
                if (CheckConditionFor(blackboard, item))
                    return true;
            }

            return false;
        }

        protected override Status Abort(Blackboard blackboard, bool forced) {
            if (!Child.IsFinished) {
                blackboard.Set(VariableName, currentList[currentIndex]);
                return Child.TickAbort(blackboard, forced);
            }
            return Status.Aborted;
        }
    }
}
