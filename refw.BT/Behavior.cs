using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace refw.BT
{
    public enum Status {
        Invalid,
        Success,
        Failure,
        Running
    }

    public enum RepeatMode {
        Never,
        UntilSuccess,
        UntilFailure,
        Forever
    }

    public abstract class Behavior {
        public Status Status { get; protected set; }
        public RepeatMode RepeatMode { get; set; }

        public Behavior() {
            RepeatMode = RepeatMode.Never;
            Status = Status.Invalid;
        }

        protected abstract Status Update(Blackboard blackboard);

        protected virtual void OnInitialize(Blackboard blackboard) { }
        protected virtual void OnTerminate(Status status) { }
        
        [DebuggerStepThrough]
        public Status Tick(Blackboard blackboard) {
            if (Status != Status.Running)
                OnInitialize(blackboard);

            Status = Update(blackboard);

            if (Status != Status.Running)
                OnTerminate(Status);

            // Repeat until failure, until success or forever
            if (RepeatMode == RepeatMode.UntilFailure && Status == Status.Success
                || RepeatMode == RepeatMode.UntilSuccess && Status == Status.Failure
                || RepeatMode == RepeatMode.Forever && Status != Status.Running) {
                Reset();
                return Status.Running;
            }

            return Status;
        }

        public void Reset() {
            // Node is reset in the middle of execution, trigger clean-up
            if (Status != Status.Invalid)
                OnTerminate(Status);

            Status = Status.Invalid;
        }

        public bool IsFinished {
            get {
                return Status == Status.Success || Status == Status.Failure;
            }
        }

        public bool IsRunning {
            get {
                return Status == Status.Running;
            }
        }

        public virtual int GetMaxChildren() {
            return 0;
        }

        public virtual List<Behavior> GetChildren() {
            return null;
        }

        public IEnumerable<FieldInfo> GetBehaviorProperties() {
            return GetType()
                .GetFields()
                .Where(p => p.FieldType.IsSubclassOf(typeof (BasicBehaviorProperty)));
        }
    }
}
