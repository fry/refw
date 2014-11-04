namespace refw.BT.Editor {
    partial class BehaviorTreeMonitor {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.button1 = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.treeBehavior = new System.Windows.Forms.TreeView();
            this.propertyGridBehavior = new System.Windows.Forms.PropertyGrid();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(12, 12);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(80, 17);
            this.checkBox1.TabIndex = 1;
            this.checkBox1.Text = "checkBox1";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(98, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(12, 41);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.treeBehavior);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.propertyGridBehavior);
            this.splitContainer1.Size = new System.Drawing.Size(912, 547);
            this.splitContainer1.SplitterDistance = 661;
            this.splitContainer1.TabIndex = 3;
            // 
            // treeBehavior
            // 
            this.treeBehavior.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeBehavior.Location = new System.Drawing.Point(0, 0);
            this.treeBehavior.Name = "treeBehavior";
            this.treeBehavior.Size = new System.Drawing.Size(661, 547);
            this.treeBehavior.TabIndex = 1;
            this.treeBehavior.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeBehavior_AfterSelect);
            // 
            // propertyGridBehavior
            // 
            this.propertyGridBehavior.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGridBehavior.Location = new System.Drawing.Point(0, 0);
            this.propertyGridBehavior.Name = "propertyGridBehavior";
            this.propertyGridBehavior.Size = new System.Drawing.Size(247, 547);
            this.propertyGridBehavior.TabIndex = 0;
            // 
            // BehaviorTreeMonitor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(936, 600);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.checkBox1);
            this.Name = "BehaviorTreeMonitor";
            this.Text = "BehaviorTreeMonitor";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TreeView treeBehavior;
        private System.Windows.Forms.PropertyGrid propertyGridBehavior;
    }
}