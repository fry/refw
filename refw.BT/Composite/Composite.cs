using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace refw.BT {
    public abstract class Composite: Behavior {
        public List<Behavior> Children = new List<Behavior>();

        protected override void OnTerminate(Status status) {
            //foreach (var child in Children)
                //child.Reset();
        }

        public override int GetMaxChildren() {
            return 10;
        }

        public override List<Behavior> GetChildren() {
            return Children;
        }
    }
}
