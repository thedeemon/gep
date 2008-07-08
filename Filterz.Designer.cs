namespace gep
{
    partial class Filterz
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
            this.catcombo = new System.Windows.Forms.ComboBox();
            this.filtertree = new System.Windows.Forms.TreeView();
            this.addbtn = new System.Windows.Forms.Button();
            this.btnEdit = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // catcombo
            // 
            this.catcombo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.catcombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.catcombo.FormattingEnabled = true;
            this.catcombo.Location = new System.Drawing.Point(3, 2);
            this.catcombo.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.catcombo.MaxDropDownItems = 20;
            this.catcombo.Name = "catcombo";
            this.catcombo.Size = new System.Drawing.Size(405, 24);
            this.catcombo.Sorted = true;
            this.catcombo.TabIndex = 0;
            this.catcombo.SelectedIndexChanged += new System.EventHandler(this.catcombo_SelectedIndexChanged);
            // 
            // filtertree
            // 
            this.filtertree.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.filtertree.FullRowSelect = true;
            this.filtertree.HideSelection = false;
            this.filtertree.Location = new System.Drawing.Point(3, 33);
            this.filtertree.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.filtertree.Name = "filtertree";
            this.filtertree.Size = new System.Drawing.Size(405, 659);
            this.filtertree.TabIndex = 1;
            this.filtertree.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.filtertree_MouseDoubleClick);
            this.filtertree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.filtertree_AfterSelect);
            // 
            // addbtn
            // 
            this.addbtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.addbtn.Location = new System.Drawing.Point(3, 700);
            this.addbtn.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.addbtn.Name = "addbtn";
            this.addbtn.Size = new System.Drawing.Size(100, 31);
            this.addbtn.TabIndex = 2;
            this.addbtn.Text = "Add...";
            this.addbtn.UseVisualStyleBackColor = true;
            this.addbtn.Click += new System.EventHandler(this.addbtn_Click);
            // 
            // btnEdit
            // 
            this.btnEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnEdit.Location = new System.Drawing.Point(124, 700);
            this.btnEdit.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new System.Drawing.Size(100, 31);
            this.btnEdit.TabIndex = 3;
            this.btnEdit.Text = "Edit";
            this.btnEdit.UseVisualStyleBackColor = true;
            this.btnEdit.Click += new System.EventHandler(this.OnEdit);
            // 
            // Filterz
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(412, 735);
            this.ControlBox = false;
            this.Controls.Add(this.btnEdit);
            this.Controls.Add(this.addbtn);
            this.Controls.Add(this.filtertree);
            this.Controls.Add(this.catcombo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "Filterz";
            this.Text = "Filters";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.Filterz_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox catcombo;
        private System.Windows.Forms.TreeView filtertree;
        private System.Windows.Forms.Button addbtn;
        private System.Windows.Forms.Button btnEdit;

    }
}