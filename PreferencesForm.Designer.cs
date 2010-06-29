namespace gep
{
    partial class PreferencesForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.suggestURLsChkBox = new System.Windows.Forms.CheckBox();
            this.autoArrangeChkBox = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.createByNameBtn = new System.Windows.Forms.RadioButton();
            this.createByMonBtn = new System.Windows.Forms.RadioButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.findPinsBtn = new System.Windows.Forms.RadioButton();
            this.renderStreamBtn = new System.Windows.Forms.RadioButton();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.cancelBtn = new System.Windows.Forms.Button();
            this.okBtn = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.suggestURLsChkBox);
            this.groupBox1.Controls.Add(this.autoArrangeChkBox);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(318, 71);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Behaviour";
            // 
            // suggestURLsChkBox
            // 
            this.suggestURLsChkBox.AutoSize = true;
            this.suggestURLsChkBox.Location = new System.Drawing.Point(11, 43);
            this.suggestURLsChkBox.Name = "suggestURLsChkBox";
            this.suggestURLsChkBox.Size = new System.Drawing.Size(198, 17);
            this.suggestURLsChkBox.TabIndex = 1;
            this.suggestURLsChkBox.Text = "Suggest URLs for sources and sinks";
            this.suggestURLsChkBox.UseVisualStyleBackColor = true;
            // 
            // autoArrangeChkBox
            // 
            this.autoArrangeChkBox.AutoSize = true;
            this.autoArrangeChkBox.Location = new System.Drawing.Point(11, 20);
            this.autoArrangeChkBox.Name = "autoArrangeChkBox";
            this.autoArrangeChkBox.Size = new System.Drawing.Size(112, 17);
            this.autoArrangeChkBox.TabIndex = 0;
            this.autoArrangeChkBox.Text = "AutoArrange filters";
            this.autoArrangeChkBox.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.panel2);
            this.groupBox2.Controls.Add(this.panel1);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Location = new System.Drawing.Point(12, 94);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(318, 138);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Code generation";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.createByNameBtn);
            this.panel2.Controls.Add(this.createByMonBtn);
            this.panel2.Location = new System.Drawing.Point(125, 81);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(181, 51);
            this.panel2.TabIndex = 4;
            // 
            // createByNameBtn
            // 
            this.createByNameBtn.AutoSize = true;
            this.createByNameBtn.Location = new System.Drawing.Point(3, 3);
            this.createByNameBtn.Name = "createByNameBtn";
            this.createByNameBtn.Size = new System.Drawing.Size(65, 17);
            this.createByNameBtn.TabIndex = 5;
            this.createByNameBtn.TabStop = true;
            this.createByNameBtn.Text = "by name";
            this.createByNameBtn.UseVisualStyleBackColor = true;
            // 
            // createByMonBtn
            // 
            this.createByMonBtn.AutoSize = true;
            this.createByMonBtn.Location = new System.Drawing.Point(3, 26);
            this.createByMonBtn.Name = "createByMonBtn";
            this.createByMonBtn.Size = new System.Drawing.Size(174, 17);
            this.createByMonBtn.TabIndex = 4;
            this.createByMonBtn.TabStop = true;
            this.createByMonBtn.Text = "by moniker string (display name)";
            this.createByMonBtn.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.findPinsBtn);
            this.panel1.Controls.Add(this.renderStreamBtn);
            this.panel1.Location = new System.Drawing.Point(125, 16);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(163, 51);
            this.panel1.TabIndex = 6;
            // 
            // findPinsBtn
            // 
            this.findPinsBtn.AutoSize = true;
            this.findPinsBtn.Location = new System.Drawing.Point(3, 2);
            this.findPinsBtn.Name = "findPinsBtn";
            this.findPinsBtn.Size = new System.Drawing.Size(163, 17);
            this.findPinsBtn.TabIndex = 1;
            this.findPinsBtn.TabStop = true;
            this.findPinsBtn.Text = "find pins and connect directly";
            this.findPinsBtn.UseVisualStyleBackColor = true;
            // 
            // renderStreamBtn
            // 
            this.renderStreamBtn.AutoSize = true;
            this.renderStreamBtn.Location = new System.Drawing.Point(3, 25);
            this.renderStreamBtn.Name = "renderStreamBtn";
            this.renderStreamBtn.Size = new System.Drawing.Size(113, 17);
            this.renderStreamBtn.TabIndex = 2;
            this.renderStreamBtn.TabStop = true;
            this.renderStreamBtn.Text = "use RenderStream";
            this.renderStreamBtn.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 86);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(108, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Create unusual filters:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Connect filters:";
            // 
            // cancelBtn
            // 
            this.cancelBtn.Location = new System.Drawing.Point(255, 248);
            this.cancelBtn.Name = "cancelBtn";
            this.cancelBtn.Size = new System.Drawing.Size(75, 23);
            this.cancelBtn.TabIndex = 2;
            this.cancelBtn.Text = "Cancel";
            this.cancelBtn.UseVisualStyleBackColor = true;
            this.cancelBtn.Click += new System.EventHandler(this.cancelBtn_Click);
            // 
            // okBtn
            // 
            this.okBtn.Location = new System.Drawing.Point(163, 248);
            this.okBtn.Name = "okBtn";
            this.okBtn.Size = new System.Drawing.Size(75, 23);
            this.okBtn.TabIndex = 3;
            this.okBtn.Text = "OK";
            this.okBtn.UseVisualStyleBackColor = true;
            this.okBtn.Click += new System.EventHandler(this.okBtn_Click);
            // 
            // PreferencesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(341, 285);
            this.Controls.Add(this.okBtn);
            this.Controls.Add(this.cancelBtn);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PreferencesForm";
            this.Text = "Preferences";
            this.Load += new System.EventHandler(this.PreferencesForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox autoArrangeChkBox;
        private System.Windows.Forms.CheckBox suggestURLsChkBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton createByMonBtn;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RadioButton renderStreamBtn;
        private System.Windows.Forms.RadioButton findPinsBtn;
        private System.Windows.Forms.RadioButton createByNameBtn;
        private System.Windows.Forms.Button cancelBtn;
        private System.Windows.Forms.Button okBtn;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel1;
    }
}