using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace refw.Debug {
    public class Inspection {
        public static void DumpAttributes(StringBuilder sb, object obj) {
            var type = obj.GetType();
            foreach (var prop in type.GetProperties().OrderBy(p => p.Name)) {
                var name = prop.Name;
                var val = prop.GetValue(obj, null);
                string val_str;
                if (val is IEnumerable && !(val is string)) {
                    val_str = "[" + string.Join(", ", (val as IEnumerable).Cast<object>().Where(q => q != null).Select(s => s.ToString())) + "]";
                } else if (val != null) {
                    val_str = val.ToString();
                } else {
                    val_str = "<null>";
                }
                sb.AppendLine(name + ": " + val_str);
            }
        }

        public static void DumpFields(StringBuilder sb, object obj) {
            var type = obj.GetType();
            foreach (var prop in type.GetFields().OrderBy(p => p.Name)) {
                var name = prop.Name;
                var val = prop.GetValue(obj);
                string val_str;
                if (val is Array) {
                    val_str = "[" + string.Join(", ", (val as Array).Cast<object>().Where(q => q != null).Select(s => s.ToString())) + "]";
                } else
                    val_str = val.ToString();
                sb.AppendLine(name + ": " + val_str);
            }
        }
    }
}
