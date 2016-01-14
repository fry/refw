using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace refw.BT {
    public abstract class Composite: Behavior, IEnumerable<Behavior> {
        public List<Behavior> Children = new List<Behavior>();

        protected override void OnTerminate(Status status) {
            //foreach (var child in Children)
                //child.Reset();
        }

        public override int GetMaxChildren() {
            return 20;
        }

        public override List<Behavior> GetChildren() {
            return Children;
        }

        public IEnumerator<Behavior> GetEnumerator() {
            return Children.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return Children.GetEnumerator();
        }

        public void Add(Behavior behavior) {
            if (Children.Count >= GetMaxChildren())
                throw new ArgumentOutOfRangeException(String.Format("Composite can only hold {0} elements", GetMaxChildren()));
            Children.Add(behavior);
        }
    }
}
