using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace refw.BT {
    public class IsSet: Decorator {
        public string Source;

        protected override Status Update(Blackboard blackboard) {
            if (blackboard.Contains(Source) && blackboard.Get<object>(Source) != null) {
                if (Child != null)
                    return Child.Tick(blackboard);
                return Status.Success;
            }
            return Status.Failure;
        }
    }
}
