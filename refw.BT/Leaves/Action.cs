using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace refw.BT {
    public class Action: Behavior {
        Func<Blackboard, Status> action;

        public Action(Func<Blackboard, Status> action) {
            this.action = action;
        }

        public Action(Func<Blackboard, bool> action) {
            this.action = delegate(Blackboard blackboard) {
                if (action(blackboard))
                    return Status.Success;
                return Status.Failure;
            };
        }

        public Action(System.Action action) {
            this.action = delegate {
                action();
                return Status.Success;
            };
        }

        protected override Status Update(Blackboard blackboard) {
            return action(blackboard);
        }
    }
}
