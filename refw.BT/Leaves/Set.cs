using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace refw.BT {
    public class Set: Behavior {
        public string Destination = "out";
        public Func<object> Func = null;
        public Func<Blackboard, object> FuncBlackboard = null;

        protected override Status Update(Blackboard blackboard) {
            if (Func != null)
                blackboard.Set(Destination, Func());
            else
                blackboard.Set(Destination, FuncBlackboard(blackboard));
            return Status.Success;
        }
    }
}
