using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace refw.BT {
    public class Template: Decorator {
        public BehaviorProperty<string> TemplateName;

        protected override void OnInitialize(Blackboard blackboard) {
            if (Child == null)
                Child = blackboard.InstanciateTemplate(TemplateName.GetValue(blackboard));
        }

        protected override Status Update(Blackboard blackboard) {
            return Child.Tick(blackboard);
        }
    }
}
