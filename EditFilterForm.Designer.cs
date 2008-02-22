namespace gep
{
    partial class EditFilterForm
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
            this.btnUnregister = new System.Windows.Forms.Button();
            this.btnSetMerit = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.meritCombo = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textMerit = new System.Windows.Forms.TextBox();
            this.filterCLSID = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.filterName = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnUnregister
            // 
            this.btnUnregister.Location = new System.Drawing.Point(8, 180);
            this.btnUnregister.Name = "btnUnregister";
            this.btnUnregister.Size = new System.Drawing.Size(100, 23);
            this.btnUnregister.TabIndex = 2;
            this.btnUnregister.Text = "Unregister filter";
            this.btnUnregister.UseVisualStyleBackColor = true;
            this.btnUnregister.Click += new System.EventHandler(this.OnUnregister);
            // 
            // btnSetMerit
            // 
            this.btnSetMerit.Location = new System.Drawing.Point(138, 138);
            this.btnSetMerit.Name = "btnSetMerit";
            this.btnSetMerit.Size = new System.Drawing.Size(100, 23);
            this.btnSetMerit.TabIndex = 1;
            this.btnSetMerit.Text = "Set new merit";
            this.btnSetMerit.UseVisualStyleBackColor = true;
            this.btnSetMerit.Click += new System.EventHandler(this.OnSetMerit);
            // 
            // btnClose
            // 
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(138, 180);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(100, 23);
            this.btnClose.TabIndex = 0;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.OnClose);
            // 
            // meritCombo
            // 
            this.meritCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.meritCombo.FormattingEnabled = true;
            this.meritCombo.Location = new System.Drawing.Point(8, 105);
            this.meritCombo.Name = "meritCombo";
            this.meritCombo.Size = new System.Drawing.Size(230, 21);
            this.meritCombo.TabIndex = 3;
            this.meritCombo.SelectionChangeCommitted += new System.EventHandler(this.OnMeritSelChange);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 89);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(33, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Merit:";
            // 
            // textMerit
            // 
            this.textMerit.Location = new System.Drawing.Point(8, 140);
            this.textMerit.Name = "textMerit";
            this.textMerit.Size = new System.Drawing.Size(100, 20);
            this.textMerit.TabIndex = 5;
            // 
            // filterCLSID
            // 
            this.filterCLSID.BackColor = System.Drawing.SystemColors.Window;
            this.filterCLSID.Location = new System.Drawing.Point(8, 45);
            this.filterCLSID.Name = "filterCLSID";
            this.filterCLSID.ReadOnly = true;
            this.filterCLSID.Size = new System.Drawing.Size(230, 20);
            this.filterCLSID.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(5, 30);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "CLSID:";
            // 
            // filterName
            // 
            this.filterName.AutoSize = true;
            this.filterName.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.filterName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.filterName.Location = new System.Drawing.Point(5, 8);
            this.filterName.Name = "filterName";
            this.filterName.Size = new System.Drawing.Size(28, 13);
            this.filterName.TabIndex = 8;
            this.filterName.Text = "filta";
            // 
            // EditFilterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(248, 214);
            this.Controls.Add(this.filterName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.filterCLSID);
            this.Controls.Add(this.textMerit);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.meritCombo);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnSetMerit);
            this.Controls.Add(this.btnUnregister);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EditFilterForm";
            this.Text = "Edit filter info";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.EditFilterForm_FormClosing);
            this.Load += new System.EventHandler(this.EditFilterForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnUnregister;
        private System.Windows.Forms.Button btnSetMerit;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.ComboBox meritCombo;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textMerit;
        private System.Windows.Forms.TextBox filterCLSID;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label filterName;
    }
}