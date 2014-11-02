using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace refw {
    [Serializable]
    public class StateCapture {
        List<Tuple<string, byte[]>> stateCapture = new List<Tuple<string,byte[]>>();

        public static StateCapture CaptureState() {
            var stateCapture = new StateCapture();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                var types = assembly.GetExportedTypes();
                foreach (var reloadable in types.Where(t => t.IsSubclassOf(typeof(Reloadable)))) {
                    var state = (byte[])reloadable.GetMethod("SaveState", BindingFlags.Public | BindingFlags.Static).Invoke(null, null);
                    stateCapture.stateCapture.Add(new Tuple<string, byte[]>(reloadable.FullName, state));
                }
            }

            return stateCapture;
        }

        public void RestoreState() {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                foreach (var reloadable in assembly.GetExportedTypes().Where(t => t.IsSubclassOf(typeof(Reloadable)))) {
                    var data = stateCapture.FirstOrDefault(t => t.Item1 == reloadable.FullName);
                    if (data != null) {
                        reloadable.GetMethod("RestoreState", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { data.Item2 });
                    }
                }
            }
        }
    }
}
