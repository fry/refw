using PostSharp.Aspects;
using PostSharp.Extensibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace refw {
    [Serializable]
    public class CacheAttribute : MethodInterceptionAspect {
        private string _methodName;
        [NonSerialized]
        private object cachedValue = null;

        protected virtual bool IsValid {
            get {
                return cachedValue != null;
            }
            set {

            }
        }

        public override void CompileTimeInitialize(MethodBase method, AspectInfo aspectInfo) {
            _methodName = method.Name;
        }

        public override void OnInvoke(MethodInterceptionArgs args) {
            if (IsValid) {
                args.ReturnValue = cachedValue;
            } else {
                var returnVal = args.Invoke(args.Arguments);
                args.ReturnValue = returnVal;
                cachedValue = returnVal;
                IsValid = true;
            }
        }
    }
}
