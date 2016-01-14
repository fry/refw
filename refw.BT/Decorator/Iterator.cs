using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace refw.BT {
    public class Iterator: Decorator {
        public BehaviorProperty<IEnumerable<object>> List;

        private List<object> currentList;
        private int index;
        protected override void OnInitialize(Blackboard blackboard) {
            currentList = List.GetValue(blackboard).ToList();
            index = 0;
        }

        protected override Status Update(Blackboard blackboard) {
            if (index >= currentList.Count)
                return Status.Success;

            blackboard.Set("current", currentList[index]);
            if (Child.TickUpdate(blackboard) != Status.Running)
                return Child.Status;

            return Status.Running;
        }

        public override bool CheckCondition(Blackboard blackboard) {
            var list = List.GetValue(blackboard).ToList();
            if (list.Count == 0)
                return false;

            blackboard.Set("current", currentList[index]);
            return base.CheckCondition(blackboard);
        }
    }
}
