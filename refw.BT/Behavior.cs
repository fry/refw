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
        Aborted,

        Running,
        Aborting,
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

        protected Behavior() {
            RepeatMode = RepeatMode.Never;
            Status = Status.Invalid;
        }

        /// <summary>
        /// Execute the node, should return Success, Failure or Running
        /// </summary>
        /// <param name="blackboard"></param>
        /// <returns></returns>
        protected abstract Status Update(Blackboard blackboard);

        protected virtual Status Abort(Blackboard blackboard) {
            // Default behavior on abort is to reset the node
            Reset();
            return Status.Aborted;
        }

        protected virtual void OnInitialize(Blackboard blackboard) { }
        protected virtual void OnTerminate(Status status) { }
        
        //[DebuggerStepThrough]
        public Status TickUpdate(Blackboard blackboard) {
            if (!IsRunning) {
                OnInitialize(blackboard);
            }

            Status = Update(blackboard);

            if (!IsRunning) {
                OnTerminate(Status);
            }

            // Repeat until failure, until success or forever
            if (RepeatMode == RepeatMode.UntilFailure && Status == Status.Success
                || RepeatMode == RepeatMode.UntilSuccess && Status == Status.Failure
                || RepeatMode == RepeatMode.Forever && !IsRunning) {
                Reset();
                return Status.Running;
            }

            return Status;
        }

        public Status TickAbort(Blackboard blackboard) {
            if (!IsFinished)
                Status = Abort(blackboard);
            return Status;
        }

        public void Reset() {
            Status = Status.Invalid;
        }

        public bool IsFinished {
            get {
                return Status == Status.Success || Status == Status.Failure || Status == Status.Aborted;
            }
        }

        public bool IsRunning {
            get {
                return Status == Status.Running || Status == Status.Aborting;
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
