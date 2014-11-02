using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace refw.BT {
    public class Selector : Composite {
        List<Behavior>.Enumerator CurrentChild;

        protected override void OnInitialize(Blackboard blackboard) {
            CurrentChild = Children.GetEnumerator();
            if (!CurrentChild.MoveNext())
                throw new ArgumentException("Selector without children");
        }

        protected override Status Update(Blackboard blackboard) {
            while (true) {
                var status = CurrentChild.Current.Tick(blackboard);

                if (status != Status.Failure)
                    return status;

                if (!CurrentChild.MoveNext())
                    return Status.Failure;
            }
        }
    }
}
