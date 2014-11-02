using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace refw.BT {
    public class Wait: Behavior {
        public BehaviorProperty<TimeSpan> Time;

        DateTime waitUntil;

        protected override void OnInitialize(Blackboard blackboard) {
            waitUntil = DateTime.Now.Add(Time.GetValue(blackboard));
        }

        protected override Status Update(Blackboard blackboard) {
            if (DateTime.Now >= waitUntil)
                return Status.Success;
            return Status.Running;
        }
    }
}
