using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace refw.BT.Editor {
    public partial class BehaviorPropertyControl : UserControl {
        private BehaviorPropertyBinding binding;
        public BehaviorPropertyControl(BehaviorPropertyBinding binding) {
            InitializeComponent();

            this.binding = binding;

            Refresh();
        }

        void Refresh() {
            labelPropertyName.Text = binding.PropertyName;
            labelPropertyType.Text = binding.PropertyType.Name;

            // Only allow static values for types we can parse
            var type_converter = TypeDescriptor.GetConverter(binding.PropertyType);
            radioTypeStatic.Enabled = type_converter.CanConvertFrom(typeof(string));

            var current_binding = binding.Get();
            if (current_binding == null) {
                radioTypeEval.Checked = true;
            } else {
                var value_source = current_binding.GetValueSource();
                if (value_source == BehaviorPropertyType.Blackboard)
                    radioTypeBlackboard.Checked = true;
                else if (value_source == BehaviorPropertyType.Func)
                    radioTypeEval.Checked = true;
                else if (value_source == BehaviorPropertyType.Static)
                    radioTypeStatic.Checked = true;
                else {
                    throw new ArgumentException("BehaviorPropertyType has an invalid value");
                }
            }

        }
    }
}
