using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace refw.BT {
    [DefaultBehavior]
    public class SetIfNotSet: Behavior {
        public string Destination = "out";
        public Func<object> Func = null;
        public Func<Blackboard, object> FuncBlackboard = null;
        public BehaviorProperty<object> Value = null;

        protected override Status Update(Blackboard blackboard) {
            if (blackboard.Contains(Destination))
                return Status.Success;

            if (Func != null)
                blackboard.Set(Destination, Func());
            else if (Value != null)
                blackboard.Set(Destination, Value.GetValue(blackboard));
            else
                blackboard.Set(Destination, FuncBlackboard(blackboard));
            return Status.Success;
        }
    }
}
