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

            public BehaviorTreeNode(Behavior behavior) {
                this.Behavior = behavior;

                Text = Behavior.ToString();
            }

            public void Refresh() {
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

                foreach (var node in Nodes) {
                    ((BehaviorTreeNode)node).Refresh();
                }
            }
        }
        private Behavior root;
        private Timer refreshTimer = new Timer();
        public BehaviorTreeMonitor(Behavior root) {
            this.root = root;

            refreshTimer.Interval = 100;
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

            propertyGridBehavior.SelectedObject = bnode.Behavior;
        }
    }
}
