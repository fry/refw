using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace refw.BT {
    /// <summary>
    /// The opposite of the Atomic decorator. The child will always be aborted if a higher up parent wants to run.
    /// </summary>
    public class AtomicContext: Decorator {
        protected override Status Update(Blackboard blackboard) {
            return Child.TickUpdate(blackboard);
        }

        protected override Status Abort(Blackboard blackboard, bool forced) {
            if (Child == null)
                return Status.Aborted;
            return Child.TickAbort(blackboard, true);
        }
    }
}
