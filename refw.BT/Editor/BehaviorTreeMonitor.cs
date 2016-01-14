using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace refw.BT.Editor {
    public partial class BehaviorTreeMonitor : Form {
        class BehaviorTreeNode : TreeNode {
            public Behavior Behavior;
            private Status oldStatus;

            public BehaviorTreeNode(Behavior behavior) {
                this.Behavior = behavior;

                Text = Behavior.ToString();
            }

            public void Refresh() {
                UpdateStatus();

                foreach (var node in Nodes) {
                    ((BehaviorTreeNode)node).Refresh();
                }
            }

            void UpdateStatus() {
                if (Behavior.Status == oldStatus)
                    return;

                if (Behavior.Status != Status.Running)
                    Collapse();
                else
                    Expand();
            
                if (Behavior.Status == Status.Success) {
                    BackColor = Color.Green;
                    ForeColor = Color.White;
                } else if (Behavior.Status == Status.Failure) {
                    BackColor = Color.Red;
                    ForeColor = Color.White;
                } else if (Behavior.Status == Status.Running) {
                    BackColor = Color.DeepSkyBlue;
                    ForeColor = Color.White;
                } else if (Behavior.Status == Status.Invalid) {
                    BackColor = Color.White;
                    ForeColor = Color.Black;
                } else if (Behavior.Status == Status.Aborting) {
                    BackColor = Color.Orange;
                    ForeColor = Color.White;
                } else if (Behavior.Status == Status.Aborted) {
                    BackColor = Color.Brown;
                    ForeColor = Color.White;
                }

                oldStatus = Behavior.Status;
            }
        }
        private Behavior root;
        private Timer refreshTimer = new Timer();
        public BehaviorTreeMonitor(Behavior root) {
            this.root = root;

            refreshTimer.Interval = 50;
            refreshTimer.Tick += refreshTimer_Tick;
            refreshTimer.Start();
            InitializeComponent();
        }

        void refreshTimer_Tick(object sender, EventArgs e) {
            if (checkBox1.Checked && treeBehavior.Nodes.Count > 0) {
                treeBehavior.BeginUpdate();
                ((BehaviorTreeNode) treeBehavior.Nodes[0]).Refresh();
                treeBehavior.EndUpdate();
            }
        }

        public void CreateTree() {
            treeBehavior.Nodes.Clear();
            treeBehavior.BeginUpdate();

            treeBehavior.Nodes.Add(BuildNode(root));

            treeBehavior.EndUpdate();
        }

        BehaviorTreeNode BuildNode(Behavior behavior) {
            var node = new BehaviorTreeNode(behavior);
            foreach (var child in behavior.GetChildren()) {
                node.Nodes.Add(BuildNode(child));
            }

            return node;
        }

        private void button1_Click(object sender, EventArgs e) {
            CreateTree();
            treeBehavior.ExpandAll();
        }

        private void treeBehavior_AfterSelect(object sender, TreeViewEventArgs e) {
            if (e.Node == null)
                return;

            var bnode = (BehaviorTreeNode) e.Node;
            textVars.Clear();
            foreach (var field in bnode.Behavior.GetType().GetFields()) {
                if (field.FieldType.IsSubclassOf(typeof(BasicBehaviorProperty))) {
                    var prop = (BasicBehaviorProperty)field.GetValue(bnode.Behavior);
                    var name = field.Name;
                    object val = null;
                    try {
                        val = prop.GetLastValue();
                    } catch (Exception ex) {
                        val = "<exception>";
                    }

                    try {
                        textVars.AppendText(String.Format("{0} ({1}, {2}) = {3}", name, prop.GetValueType().Name, prop.GetValueSource(), val));
                        textVars.AppendText(Environment.NewLine);
                    } catch (Exception ex) {
                    }
                }
            }

            propertyGridBehavior.SelectedObject = bnode.Behavior;
        }
    }
}
