namespace gep
{
    partial class SearchFilterForm
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
            this.filtertree = new System.Windows.Forms.TreeView();
            this.catcombo = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxDispName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxCLSID = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxPathName = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // filtertree
            // 
            this.filtertree.Location = new System.Drawing.Point(12, 158);
            this.filtertree.Name = "filtertree";
            this.filtertree.Size = new System.Drawing.Size(449, 330);
            this.filtertree.TabIndex = 0;
            this.filtertree.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.OnDblClick);
            this.filtertree.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.OnItemDrag);
            // 
            // catcombo
            // 
            this.catcombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.catcombo.FormattingEnabled = true;
            this.catcombo.Location = new System.Drawing.Point(111, 12);
            this.catcombo.Name = "catcombo";
            this.catcombo.Size = new System.Drawing.Size(350, 21);
            this.catcombo.TabIndex = 1;
            this.catcombo.SelectedIndexChanged += new System.EventHandler(this.OnCategoryChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Category:";
            // 
            // textBoxName
            // 
            this.textBoxName.Location = new System.Drawing.Point(111, 40);
            this.textBoxName.Name = "textBoxName";
            this.textBoxName.Size = new System.Drawing.Size(350, 20);
            this.textBoxName.TabIndex = 3;
            this.textBoxName.TextChanged += new System.EventHandler(this.OnTextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 43);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Name:";
            // 
            // textBoxDispName
            // 
            this.textBoxDispName.Location = new System.Drawing.Point(111, 68);
            this.textBoxDispName.Name = "textBoxDispName";
            this.textBoxDispName.Size = new System.Drawing.Size(350, 20);
            this.textBoxDispName.TabIndex = 5;
            this.textBoxDispName.TextChanged += new System.EventHandler(this.OnTextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 71);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(73, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Display name:";
            // 
            // textBoxCLSID
            // 
            this.textBoxCLSID.Location = new System.Drawing.Point(111, 96);
            this.textBoxCLSID.Name = "textBoxCLSID";
            this.textBoxCLSID.Size = new System.Drawing.Size(350, 20);
            this.textBoxCLSID.TabIndex = 7;
            this.textBoxCLSID.TextChanged += new System.EventHandler(this.OnTextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 99);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(41, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "CLSID:";
            // 
            // textBoxPathName
            // 
            this.textBoxPathName.Location = new System.Drawing.Point(111, 124);
            this.textBoxPathName.Name = "textBoxPathName";
            this.textBoxPathName.Size = new System.Drawing.Size(350, 20);
            this.textBoxPathName.TabIndex = 9;
            this.textBoxPathName.TextChanged += new System.EventHandler(this.OnTextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 127);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(58, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "Pathname:";
            // 
            // SearchFilterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(473, 500);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.textBoxPathName);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBoxCLSID);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBoxDispName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.catcombo);
            this.Controls.Add(this.filtertree);
            this.Name = "SearchFilterForm";
            this.Text = "Find filters";
            this.Load += new System.EventHandler(this.OnLoad);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnClose);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView filtertree;
        private System.Windows.Forms.ComboBox catcombo;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxDispName;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxCLSID;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBoxPathName;
        private System.Windows.Forms.Label label5;
    }
}