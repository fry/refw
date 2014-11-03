using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace refw.BT {
    public class Inverter: Decorator {
        protected override Status Update(Blackboard blackboard) {
            var result = Child.TickUpdate(blackboard);

            if (result == Status.Failure)
                return Status.Success;
            else if (result == Status.Success)
                return Status.Failure;

            return result;
        }
    }
}
