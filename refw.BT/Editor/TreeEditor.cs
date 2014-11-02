using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace refw.BT.Editor {
    public partial class TreeEditor : Form {
        class TreeBehaviorWrapper : TreeNode {
            public Behavior Node;

            public TreeBehaviorWrapper(Behavior node) {
                Node = node;
                Update();
            }

            public void Update() {
                Text = Node.ToString();
            }
        }

        class TreeAddControlNode : TreeNode {
            public Type BehaviorType;

            public TreeAddControlNode(Type type) {
                BehaviorType = type;
                Text = type.Name;
            }
        }

        private BehaviorTreeConfig activeConfig;

        public TreeEditor() {
            InitializeComponent();

            activeConfig = new BehaviorTreeConfig();
            RefreshAddControls();
        }

        private void RefreshAddControls() {
            var assembly = Assembly.GetExecutingAssembly();
            var behavior_types = assembly.GetTypes().Where(t => t.IsSubclassOf(typeof (Behavior)) && !t.IsAbstract);

            var node_categories = new List<Tuple<Type, List<Type>>> {
                Tuple.Create(typeof (Composite), new List<Type>()),
                Tuple.Create(typeof (Decorator), new List<Type>()),
                Tuple.Create(typeof (Behavior), new List<Type>())
            };

            foreach (var type in behavior_types) {
                var category = node_categories.FirstOrDefault(t => type.IsSubclassOf(t.Item1));
                if (category == null)
                    throw new ArgumentOutOfRangeException("No node category for type " + type.ToString());
                category.Item2.Add(type);
            }

            treeAddControls.Nodes.Clear();
            foreach (var category in node_categories) {
                var node = new TreeNode(category.Item1.Name);
                foreach (var type in category.Item2) {
                    node.Nodes.Add(new TreeAddControlNode(type));
                }

                treeAddControls.Nodes.Add(node);
            }
        }

        private TreeBehaviorWrapper BuildTree(Behavior node) {
            var wrap = new TreeBehaviorWrapper(node);
            foreach (var child in node.GetChildren()) {
                wrap.Nodes.Add(BuildTree(child));
            }

            return wrap; 
        }

        bool AddChildToNode(Behavior node, Behavior child) {
            if (node.GetChildren().Count < node.GetMaxChildren()) {
                node.GetChildren().Add(child);
                return true;
            }

            return false;
        }

        void AddNewNodeToCurrent(Behavior node) {
            if (treeTemplate.Nodes.Count == 0) {
                // No existing root node, add the child as a new root
                activeConfig.Root = node;
                treeTemplate.Nodes.Add(BuildTree(node));
            } else if (treeTemplate.SelectedNode == null) {
                // No node selected, add the child to the root node
                if (AddChildToNode(activeConfig.Root, node))
                    treeTemplate.Nodes[0].Nodes.Add(BuildTree(node));
            } else {
                // Node selected, add the child to the selected node
                var selected_node = (TreeBehaviorWrapper)treeTemplate.SelectedNode;
                if (AddChildToNode(selected_node.Node, node))
                    selected_node.Nodes.Add(BuildTree(node));
            }
        }

        void AddCurrentNewNodeToCurrent() {
            var node = treeAddControls.SelectedNode as TreeAddControlNode;
            if (node == null)
                return;

            var new_node = (Behavior)Activator.CreateInstance(node.BehaviorType);

            AddNewNodeToCurrent(new_node);
        }

        private void btnAddNewBehavior_Click(object sender, EventArgs e) {
            AddCurrentNewNodeToCurrent();
        }

        private void treeAddControls_DoubleClick(object sender, EventArgs e) {
            AddCurrentNewNodeToCurrent();
        }
    }
}
