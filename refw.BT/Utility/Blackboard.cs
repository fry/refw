using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace refw.BT {
    public class Blackboard {
        static Dictionary<string, MethodInfo> behaviorTemplates = new Dictionary<string, MethodInfo>();

        static Blackboard() {
            // Iterate through all types in all loaded assemblies looking for methods
            // with the BehaviorTemplate attribute
            foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                foreach (var type in assembly.ExportedTypes) {
                    foreach (var method in type.GetMethods().Where(m => m.IsStatic)) {
                        var prop = method.GetCustomAttribute<BehaviorTemplateAttribute>();
                        if (prop == null)
                            continue;

                        var name = prop.Name;
                        if (name == null)
                            name = method.Name;
                        behaviorTemplates.Add(name, method);
                    }
                }
            }
        }

        Dictionary<string, object> data = new Dictionary<string, object>();

        public bool Contains(string name) {
            return data.ContainsKey(name);
        }

        public T Get<T>(string name) {
            return (T)data[name];
        }

        public void Set<T>(string name, T value) {
            data[name] = value;
        }

        public Behavior InstanciateTemplate(string name) {
            var method = behaviorTemplates[name];

            var behavior = (Behavior) method.Invoke(null, new object[] {});

            return behavior;
        }
    }
}
