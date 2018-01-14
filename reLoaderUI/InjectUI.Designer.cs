namespace reLoaderUI {
    partial class InjectUI {
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
            this.comboProcessList = new System.Windows.Forms.ComboBox();
            this.pickDLLDialog = new System.Windows.Forms.OpenFileDialog();
            this.btnPickDLL = new System.Windows.Forms.Button();
            this.textInjectDLL = new System.Windows.Forms.TextBox();
            this.textDLLArguments = new System.Windows.Forms.TextBox();
            this.btnInject = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.comboProcessName = new System.Windows.Forms.ComboBox();
            this.radioTargetProcess = new System.Windows.Forms.RadioButton();
            this.radioProcessName = new System.Windows.Forms.RadioButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // comboProcessList
            // 
            this.comboProcessList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboProcessList.DropDownWidth = 500;
            this.comboProcessList.Font = new System.Drawing.Font("QuickType Mono", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboProcessList.FormattingEnabled = true;
            this.comboProcessList.Location = new System.Drawing.Point(3, 26);
            this.comboProcessList.Name = "comboProcessList";
            this.comboProcessList.Size = new System.Drawing.Size(515, 22);
            this.comboProcessList.Sorted = true;
            this.comboProcessList.TabIndex = 0;
            this.comboProcessList.DropDown += new System.EventHandler(this.comboProcessList_DropDown);
            // 
            // pickDLLDialog
            // 
            this.pickDLLDialog.FileName = global::reLoaderUI.reLoaderUI.Default.InjectDLL;
            this.pickDLLDialog.Filter = "Executables|*.exe;*.dll|All files|*.*";
            // 
            // btnPickDLL
            // 
            this.btnPickDLL.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPickDLL.Location = new System.Drawing.Point(455, 118);
            this.btnPickDLL.Name = "btnPickDLL";
            this.btnPickDLL.Size = new System.Drawing.Size(75, 23);
            this.btnPickDLL.TabIndex = 1;
            this.btnPickDLL.Text = "Pick DLL";
            this.btnPickDLL.UseVisualStyleBackColor = true;
            this.btnPickDLL.Click += new System.EventHandler(this.btnPickDLL_Click);
            // 
            // textInjectDLL
            // 
            this.textInjectDLL.AllowDrop = true;
            this.textInjectDLL.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textInjectDLL.Location = new System.Drawing.Point(12, 120);
            this.textInjectDLL.Name = "textInjectDLL";
            this.textInjectDLL.Size = new System.Drawing.Size(437, 20);
            this.textInjectDLL.TabIndex = 2;
            this.textInjectDLL.DragDrop += new System.Windows.Forms.DragEventHandler(this.textInjectDLL_DragDrop);
            this.textInjectDLL.DragEnter += new System.Windows.Forms.DragEventHandler(this.textInjectDLL_DragEnter);
            // 
            // textDLLArguments
            // 
            this.textDLLArguments.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textDLLArguments.Location = new System.Drawing.Point(12, 159);
            this.textDLLArguments.Multiline = true;
            this.textDLLArguments.Name = "textDLLArguments";
            this.textDLLArguments.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textDLLArguments.Size = new System.Drawing.Size(437, 128);
            this.textDLLArguments.TabIndex = 4;
            // 
            // btnInject
            // 
            this.btnInject.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnInject.Location = new System.Drawing.Point(455, 159);
            this.btnInject.Name = "btnInject";
            this.btnInject.Size = new System.Drawing.Size(75, 23);
            this.btnInject.TabIndex = 5;
            this.btnInject.Text = "Inject";
            this.btnInject.UseVisualStyleBackColor = true;
            this.btnInject.Click += new System.EventHandler(this.btnInject_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 104);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(120, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = ".NET Assembly to Inject";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 143);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(168, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Assembly Arguments (one per line)";
            // 
            // comboProcessName
            // 
            this.comboProcessName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboProcessName.Font = new System.Drawing.Font("QuickType Mono", 9F);
            this.comboProcessName.FormattingEnabled = true;
            this.comboProcessName.Location = new System.Drawing.Point(3, 76);
            this.comboProcessName.Name = "comboProcessName";
            this.comboProcessName.Size = new System.Drawing.Size(515, 22);
            this.comboProcessName.Sorted = true;
            this.comboProcessName.TabIndex = 11;
            this.comboProcessName.DropDown += new System.EventHandler(this.comboProcessName_DropDown);
            // 
            // radioTargetProcess
            // 
            this.radioTargetProcess.AutoSize = true;
            this.radioTargetProcess.Checked = true;
            this.radioTargetProcess.Location = new System.Drawing.Point(3, 3);
            this.radioTargetProcess.Name = "radioTargetProcess";
            this.radioTargetProcess.Size = new System.Drawing.Size(97, 17);
            this.radioTargetProcess.TabIndex = 12;
            this.radioTargetProcess.TabStop = true;
            this.radioTargetProcess.Text = "Target Process";
            this.radioTargetProcess.UseVisualStyleBackColor = true;
            // 
            // radioProcessName
            // 
            this.radioProcessName.AutoSize = true;
            this.radioProcessName.Location = new System.Drawing.Point(3, 54);
            this.radioProcessName.Name = "radioProcessName";
            this.radioProcessName.Size = new System.Drawing.Size(205, 17);
            this.radioProcessName.TabIndex = 13;
            this.radioProcessName.Text = "OR: Target Process Name (first found)";
            this.radioProcessName.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.comboProcessName);
            this.panel1.Controls.Add(this.radioProcessName);
            this.panel1.Controls.Add(this.comboProcessList);
            this.panel1.Controls.Add(this.radioTargetProcess);
            this.panel1.Location = new System.Drawing.Point(12, 1);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(518, 100);
            this.panel1.TabIndex = 14;
            // 
            // InjectUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(542, 299);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnInject);
            this.Controls.Add(this.textDLLArguments);
            this.Controls.Add(this.textInjectDLL);
            this.Controls.Add(this.btnPickDLL);
            this.Name = "InjectUI";
            this.Text = "reLoader UI";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboProcessList;
        private System.Windows.Forms.OpenFileDialog pickDLLDialog;
        private System.Windows.Forms.Button btnPickDLL;
        private System.Windows.Forms.TextBox textInjectDLL;
        private System.Windows.Forms.TextBox textDLLArguments;
        private System.Windows.Forms.Button btnInject;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboProcessName;
        private System.Windows.Forms.RadioButton radioTargetProcess;
        private System.Windows.Forms.RadioButton radioProcessName;
        private System.Windows.Forms.Panel panel1;
    }
}

