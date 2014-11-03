using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace refw.BT {
    public abstract class Decorator: Behavior {
        public List<Behavior> Children = new List<Behavior>();

        public Behavior Child {
            get {
                return Children.Count > 0 ? Children[0] : null;
            }

            set {
                Children.Clear();
                Children.Add(value);
            }
        }

        public override int GetMaxChildren() {
            return 1;
        }

        public override List<Behavior> GetChildren() {
            return Children;
        }

        protected override Status Abort(Blackboard blackboard) {
            if (Child == null)
                return Status.Aborted;
            return Child.TickAbort(blackboard);
        }
    }
}
