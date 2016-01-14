using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace refw.BT
{
    public class DefaultBehaviorAttribute: Attribute {}

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
        public string Name { get; set; }
        public bool DoLog { get; set; }

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

        protected virtual Status Abort(Blackboard blackboard, bool forced) {
            Reset();
            return Status.Aborted;
        }

        protected virtual void OnInitialize(Blackboard blackboard) { }
        protected virtual void OnTerminate(Status status) { }
        
        //[DebuggerStepThrough]
        public Status TickUpdate(Blackboard blackboard) {
            if (!IsRunning) {
                Log("Initialized");
                OnInitialize(blackboard);
            }

            try {
                Status = Update(blackboard);
                Log("Updated with status " + Status.ToString());
            } catch (Exception e) {
                Trace.WriteLine("Exception in behavior!");
                Trace.WriteLine(e.ToString());
                Status = Status.Failure;
            }

            if (!IsRunning) {
                Log("Terminated");
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

        public Status TickAbort(Blackboard blackboard, bool forced = false) {
            if (Status == Status.Invalid || IsFinished)
                Status = Status.Aborted;

            if (!IsFinished)
                Status = Abort(blackboard, forced);

            if (!IsRunning)
                OnTerminate(Status);

            Log("Abort with status " + Status.ToString());
            return Status;
        }

        public void Reset() {
            Log("Reset");
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

        public virtual bool CheckCondition(Blackboard blackboard) {
            return true;//return RepeatMode == RepeatMode.Forever || RepeatMode == RepeatMode.UntilSuccess;
        }

        public virtual int GetMaxChildren() {
            return 0;
        }

        public virtual List<Behavior> GetChildren() {
            return new List<Behavior>();
        }

        public IEnumerable<FieldInfo> GetBehaviorProperties() {
            return GetType()
                .GetFields()
                .Where(p => p.FieldType.IsSubclassOf(typeof (BasicBehaviorProperty)));
        }

        public override string ToString() {
            return Name ?? GetType().Name;
        }

        public void Log(object text) {
            if (DoLog)
                Trace.WriteLine(String.Format("{0}: {1}", ToString(), text.ToString()));
        }

        public Behavior FindChildByName(string name) {
            return FindChildren(b => b.Name == name).FirstOrDefault();
        }

        public IEnumerable<Behavior> FindChildren(Predicate<Behavior> pred) {
            foreach (var behavior in GetChildren()) {
                if (pred(behavior))
                    yield return behavior;
                foreach (var result in behavior.FindChildren(pred)) {
                    yield return result;
                }
            }
        }

        public bool IsDefaultBehavior {
            get {
                return GetType().GetCustomAttribute<DefaultBehaviorAttribute>() != null;
            }
        }
    }
}
