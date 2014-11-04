using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace refw.BT {
    public class Atomic : Decorator {
        protected override Status Update(Blackboard blackboard) {
            return Child.TickUpdate(blackboard);
        }

        protected override Status Abort(Blackboard blackboard) {
            if (!Child.IsFinished)
                Child.TickUpdate(blackboard);
            return Child.IsFinished ? Status.Aborted : Status.Aborting;
        }
    }
}
