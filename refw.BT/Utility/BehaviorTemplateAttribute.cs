using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace refw.BT {
    [AttributeUsage(AttributeTargets.Method)]
    public class BehaviorTemplateAttribute: Attribute {
        public string Name { get; set; }

        public BehaviorTemplateAttribute() {}
        public BehaviorTemplateAttribute(string name) {
            Name = name;
        }
    }
}
