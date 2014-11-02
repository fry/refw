using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace refw.BT {
    public abstract class Decorator: Behavior {
        public List<Behavior> Children = new List<Behavior>();

        protected override void OnTerminate(Status status) {
            //if (Children.Count > 0)
            //    Children[0].Reset();
        }

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
    }
}
