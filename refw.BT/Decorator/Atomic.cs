﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace refw.BT {
    /// <summary>
    /// This decorator prevents preemption of its child, blocking aborts until it finishes running.
    /// </summary>
    public class Atomic : Decorator {
        protected override Status Update(Blackboard blackboard) {
            return Child.TickUpdate(blackboard);
        }

        protected override Status Abort(Blackboard blackboard, bool forced) {
            if (forced)
                return base.Abort(blackboard, true);
            if (!Child.IsFinished)
                Child.TickUpdate(blackboard);
            return Child.IsFinished ? Status.Aborted : Status.Aborting;
        }
    }
}
