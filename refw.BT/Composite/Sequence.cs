using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace refw.BT {
    public class Sequence : Composite {
        List<Behavior>.Enumerator CurrentChild;

        protected override void OnInitialize(Blackboard blackboard) {
            CurrentChild = Children.GetEnumerator();
            if (!CurrentChild.MoveNext())
                throw new ArgumentException("Sequence without children");
        }

        protected override Status Update(Blackboard blackboard) {
            while (true) {
                var status = CurrentChild.Current.TickUpdate(blackboard);

                if (status != Status.Success)
                    return status;

                if (!CurrentChild.MoveNext())
                    return Status.Success;
            }
        }

        protected override Status Abort(Blackboard blackboard) {
            Status = CurrentChild.Current.TickAbort(blackboard);
            return Status;
        }

        public override bool CheckCondition(Blackboard blackboard) {
            foreach (var child in Children) {
                if (!child.CheckCondition(blackboard))
                    return false;
            }

            return true;
        }
    }
}
