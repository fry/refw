namespace refw.BT.Editor {
    partial class BehaviorPropertyControl {
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.labelPropertyName = new System.Windows.Forms.Label();
            this.labelPropertyType = new System.Windows.Forms.Label();
            this.textValue = new System.Windows.Forms.TextBox();
            this.radioTypeStatic = new System.Windows.Forms.RadioButton();
            this.radioTypeBlackboard = new System.Windows.Forms.RadioButton();
            this.radioTypeEval = new System.Windows.Forms.RadioButton();
            this.SuspendLayout();
            // 
            // labelPropertyName
            // 
            this.labelPropertyName.AutoSize = true;
            this.labelPropertyName.Location = new System.Drawing.Point(3, 0);
            this.labelPropertyName.Name = "labelPropertyName";
            this.labelPropertyName.Size = new System.Drawing.Size(74, 13);
            this.labelPropertyName.TabIndex = 0;
            this.labelPropertyName.Text = "PropertyName";
            // 
            // labelPropertyType
            // 
            this.labelPropertyType.AutoSize = true;
            this.labelPropertyType.Location = new System.Drawing.Point(3, 13);
            this.labelPropertyType.Name = "labelPropertyType";
            this.labelPropertyType.Size = new System.Drawing.Size(70, 13);
            this.labelPropertyType.TabIndex = 1;
            this.labelPropertyType.Text = "PropertyType";
            // 
            // textValue
            // 
            this.textValue.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textValue.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textValue.Location = new System.Drawing.Point(120, 23);
            this.textValue.Multiline = true;
            this.textValue.Name = "textValue";
            this.textValue.Size = new System.Drawing.Size(515, 22);
            this.textValue.TabIndex = 2;
            // 
            // radioTypeStatic
            // 
            this.radioTypeStatic.AutoSize = true;
            this.radioTypeStatic.Location = new System.Drawing.Point(120, 0);
            this.radioTypeStatic.Name = "radioTypeStatic";
            this.radioTypeStatic.Size = new System.Drawing.Size(52, 17);
            this.radioTypeStatic.TabIndex = 3;
            this.radioTypeStatic.TabStop = true;
            this.radioTypeStatic.Text = "Static";
            this.radioTypeStatic.UseVisualStyleBackColor = true;
            // 
            // radioTypeBlackboard
            // 
            this.radioTypeBlackboard.AutoSize = true;
            this.radioTypeBlackboard.Location = new System.Drawing.Point(178, 0);
            this.radioTypeBlackboard.Name = "radioTypeBlackboard";
            this.radioTypeBlackboard.Size = new System.Drawing.Size(79, 17);
            this.radioTypeBlackboard.TabIndex = 4;
            this.radioTypeBlackboard.TabStop = true;
            this.radioTypeBlackboard.Text = "Blackboard";
            this.radioTypeBlackboard.UseVisualStyleBackColor = true;
            // 
            // radioTypeEval
            // 
            this.radioTypeEval.AutoSize = true;
            this.radioTypeEval.Location = new System.Drawing.Point(263, 0);
            this.radioTypeEval.Name = "radioTypeEval";
            this.radioTypeEval.Size = new System.Drawing.Size(46, 17);
            this.radioTypeEval.TabIndex = 5;
            this.radioTypeEval.TabStop = true;
            this.radioTypeEval.Text = "Eval";
            this.radioTypeEval.UseVisualStyleBackColor = true;
            // 
            // BehaviorPropertyControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.radioTypeEval);
            this.Controls.Add(this.radioTypeBlackboard);
            this.Controls.Add(this.radioTypeStatic);
            this.Controls.Add(this.textValue);
            this.Controls.Add(this.labelPropertyType);
            this.Controls.Add(this.labelPropertyName);
            this.Name = "BehaviorPropertyControl";
            this.Size = new System.Drawing.Size(638, 48);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelPropertyName;
        private System.Windows.Forms.Label labelPropertyType;
        private System.Windows.Forms.TextBox textValue;
        private System.Windows.Forms.RadioButton radioTypeStatic;
        private System.Windows.Forms.RadioButton radioTypeBlackboard;
        private System.Windows.Forms.RadioButton radioTypeEval;
    }
}
