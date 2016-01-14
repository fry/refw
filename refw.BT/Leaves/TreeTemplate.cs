using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace refw.BT {
    public abstract class TreeTemplate: Decorator {
        protected abstract Behavior GetTree();

        public override Behavior Child {
            get {
                if (base.Child == null)
                    base.Child = GetTree();
                return base.Child;
            }
            set {
                base.Child = value;
            }
        }

        protected override Status Update(Blackboard blackboard) {
            return Child.TickUpdate(blackboard);
        }
    }
}
