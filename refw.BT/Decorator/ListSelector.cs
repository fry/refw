using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace refw.BT {
    public class ListSelector : Decorator {
        public BehaviorProperty<IEnumerable<object>> List;

        private List<object> currentList;
        private int currentIndex;
        protected override void OnInitialize(Blackboard blackboard) {
            currentList = List.GetValue(blackboard).ToList();
            currentIndex = 0;
        }

        protected override Status Update(Blackboard blackboard) {
            while (true) {
                if (currentIndex >= currentList.Count)
                    return Status.Failure;

                blackboard.Set("current", currentList[currentIndex]);

                if (Child.TickUpdate(blackboard) == Status.Failure) {
                    currentIndex++;
                } else {
                    return Child.Status;
                }
            } 
        }

        public override bool CheckCondition(Blackboard blackboard) {
            int test_index = 0;
            while (true) {
                if (test_index >= currentList.Count)
                    return false;

                object old_val = null;
                if (blackboard.Contains("current"))
                    old_val = blackboard.Get<object>("current");

                blackboard.Set("current", currentList[test_index]);

                bool can_run = Child.CheckCondition(blackboard);
                if (!can_run)
                    test_index++;

                // Restore old blackboard value
                if (old_val != null)
                    blackboard.Set("current", old_val);

                if (can_run)
                    return true;
            }
        }
    }
}
