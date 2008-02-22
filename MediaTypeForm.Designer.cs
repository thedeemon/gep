namespace gep
{
    partial class MediaTypeForm
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
            this.comboMajorType = new System.Windows.Forms.ComboBox();
            this.comboSubType = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // comboMajorType
            // 
            this.comboMajorType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboMajorType.FormattingEnabled = true;
            this.comboMajorType.Location = new System.Drawing.Point(8, 26);
            this.comboMajorType.Name = "comboMajorType";
            this.comboMajorType.Size = new System.Drawing.Size(252, 21);
            this.comboMajorType.TabIndex = 0;
            // 
            // comboSubType
            // 
            this.comboSubType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboSubType.FormattingEnabled = true;
            this.comboSubType.Location = new System.Drawing.Point(8, 74);
            this.comboSubType.Name = "comboSubType";
            this.comboSubType.Size = new System.Drawing.Size(252, 21);
            this.comboSubType.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Major type:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 57);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(49, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Subtype:";
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(88, 118);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 4;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.OnOK);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(185, 118);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.OnCancel);
            // 
            // MediaTypeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(268, 163);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comboSubType);
            this.Controls.Add(this.comboMajorType);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "MediaTypeForm";
            this.Text = "Media type";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MediaTypeForm_FormClosing);
            this.Load += new System.EventHandler(this.MediaTypeForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboMajorType;
        private System.Windows.Forms.ComboBox comboSubType;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
    }
}