namespace refw.BT.Editor {
    partial class TreeEditor {
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.treeTemplate = new System.Windows.Forms.TreeView();
            this.tabBody = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.treeAddControls = new System.Windows.Forms.TreeView();
            this.btnAddNewBehavior = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tabBody.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.treeTemplate);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tabBody);
            this.splitContainer1.Size = new System.Drawing.Size(998, 558);
            this.splitContainer1.SplitterDistance = 279;
            this.splitContainer1.TabIndex = 0;
            // 
            // treeTemplate
            // 
            this.treeTemplate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeTemplate.Location = new System.Drawing.Point(0, 0);
            this.treeTemplate.Name = "treeTemplate";
            this.treeTemplate.Size = new System.Drawing.Size(279, 558);
            this.treeTemplate.TabIndex = 0;
            // 
            // tabBody
            // 
            this.tabBody.Controls.Add(this.tabPage1);
            this.tabBody.Controls.Add(this.tabPage2);
            this.tabBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabBody.Location = new System.Drawing.Point(0, 0);
            this.tabBody.Name = "tabBody";
            this.tabBody.SelectedIndex = 0;
            this.tabBody.Size = new System.Drawing.Size(715, 558);
            this.tabBody.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(707, 532);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Properties";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.btnAddNewBehavior);
            this.tabPage2.Controls.Add(this.treeAddControls);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(707, 532);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Controls";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // treeAddControls
            // 
            this.treeAddControls.Location = new System.Drawing.Point(6, 6);
            this.treeAddControls.Name = "treeAddControls";
            this.treeAddControls.Size = new System.Drawing.Size(250, 489);
            this.treeAddControls.TabIndex = 0;
            this.treeAddControls.DoubleClick += new System.EventHandler(this.treeAddControls_DoubleClick);
            // 
            // btnAddNewBehavior
            // 
            this.btnAddNewBehavior.Location = new System.Drawing.Point(6, 501);
            this.btnAddNewBehavior.Name = "btnAddNewBehavior";
            this.btnAddNewBehavior.Size = new System.Drawing.Size(75, 23);
            this.btnAddNewBehavior.TabIndex = 1;
            this.btnAddNewBehavior.Text = "Add";
            this.btnAddNewBehavior.UseVisualStyleBackColor = true;
            this.btnAddNewBehavior.Click += new System.EventHandler(this.btnAddNewBehavior_Click);
            // 
            // TreeEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(998, 558);
            this.Controls.Add(this.splitContainer1);
            this.Name = "TreeEditor";
            this.Text = "TreeEditor";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tabBody.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TreeView treeTemplate;
        private System.Windows.Forms.TabControl tabBody;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TreeView treeAddControls;
        private System.Windows.Forms.Button btnAddNewBehavior;
    }
}