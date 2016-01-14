using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace refw.BT {
    public class Template: Decorator {
        public BehaviorProperty<string> TemplateName;

        private string resolvedTemplateName = "NOT RESOLVED";
        protected override void OnInitialize(Blackboard blackboard) {
            if (Child == null) {
                resolvedTemplateName = TemplateName.GetValue(blackboard);
                Child = blackboard.InstanciateTemplate(resolvedTemplateName);
            }
        }

        protected override Status Update(Blackboard blackboard) {
            return Child.TickUpdate(blackboard);
        }

        public override string ToString() {
            return "Template: " + resolvedTemplateName;
        }
    }
}
