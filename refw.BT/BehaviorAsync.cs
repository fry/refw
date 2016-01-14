using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace refw.BT {
    public abstract class BehaviorCoroutine: Behavior {
        private IEnumerator<Status> routine;
        
        protected abstract IEnumerable<Status> UpdateAsync(Blackboard blackboard);

        protected override void OnInitialize(Blackboard blackboard) {
            routine = null;
        }

        protected override Status Update(Blackboard blackboard) {
            if (routine == null) {
                Status = Status.Failure;
                routine = UpdateAsync(blackboard).GetEnumerator();
            }

            if (!routine.MoveNext())
                return Status;

            Status = routine.Current;
            return Status;
        }
    }
}
