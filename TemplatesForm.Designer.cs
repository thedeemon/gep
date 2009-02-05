namespace gep
{
    partial class TemplatesForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.languageCombo = new System.Windows.Forms.ComboBox();
            this.snippetsList = new System.Windows.Forms.ListBox();
            this.templateText = new System.Windows.Forms.TextBox();
            this.label_template = new System.Windows.Forms.Label();
            this.label_variables = new System.Windows.Forms.Label();
            this.variablesText = new System.Windows.Forms.TextBox();
            this.label_example = new System.Windows.Forms.Label();
            this.exampleText = new System.Windows.Forms.TextBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnRestore = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Language:";
            // 
            // languageCombo
            // 
            this.languageCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.languageCombo.FormattingEnabled = true;
            this.languageCombo.Location = new System.Drawing.Point(75, 8);
            this.languageCombo.Name = "languageCombo";
            this.languageCombo.Size = new System.Drawing.Size(69, 21);
            this.languageCombo.TabIndex = 1;
            this.languageCombo.SelectedIndexChanged += new System.EventHandler(this.OnLangChange);
            // 
            // snippetsList
            // 
            this.snippetsList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.snippetsList.FormattingEnabled = true;
            this.snippetsList.Location = new System.Drawing.Point(15, 42);
            this.snippetsList.Name = "snippetsList";
            this.snippetsList.Size = new System.Drawing.Size(229, 355);
            this.snippetsList.TabIndex = 2;
            this.snippetsList.SelectedIndexChanged += new System.EventHandler(this.OnSelChange);
            // 
            // templateText
            // 
            this.templateText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.templateText.Location = new System.Drawing.Point(3, 19);
            this.templateText.Multiline = true;
            this.templateText.Name = "templateText";
            this.templateText.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.templateText.Size = new System.Drawing.Size(601, 106);
            this.templateText.TabIndex = 3;
            this.templateText.TextChanged += new System.EventHandler(this.OnTemplateChanged);
            // 
            // label_template
            // 
            this.label_template.AutoSize = true;
            this.label_template.Location = new System.Drawing.Point(3, 0);
            this.label_template.Name = "label_template";
            this.label_template.Size = new System.Drawing.Size(54, 13);
            this.label_template.TabIndex = 4;
            this.label_template.Text = "Template:";
            // 
            // label_variables
            // 
            this.label_variables.AutoSize = true;
            this.label_variables.Location = new System.Drawing.Point(3, 128);
            this.label_variables.Name = "label_variables";
            this.label_variables.Size = new System.Drawing.Size(53, 13);
            this.label_variables.TabIndex = 5;
            this.label_variables.Text = "Variables:";
            // 
            // variablesText
            // 
            this.variablesText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.variablesText.BackColor = System.Drawing.SystemColors.Window;
            this.variablesText.Location = new System.Drawing.Point(3, 147);
            this.variablesText.Multiline = true;
            this.variablesText.Name = "variablesText";
            this.variablesText.ReadOnly = true;
            this.variablesText.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.variablesText.Size = new System.Drawing.Size(601, 106);
            this.variablesText.TabIndex = 6;
            // 
            // label_example
            // 
            this.label_example.AutoSize = true;
            this.label_example.Location = new System.Drawing.Point(3, 256);
            this.label_example.Name = "label_example";
            this.label_example.Size = new System.Drawing.Size(50, 13);
            this.label_example.TabIndex = 7;
            this.label_example.Text = "Example:";
            // 
            // exampleText
            // 
            this.exampleText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.exampleText.BackColor = System.Drawing.SystemColors.Window;
            this.exampleText.Location = new System.Drawing.Point(3, 275);
            this.exampleText.Multiline = true;
            this.exampleText.Name = "exampleText";
            this.exampleText.ReadOnly = true;
            this.exampleText.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.exampleText.Size = new System.Drawing.Size(601, 108);
            this.exampleText.TabIndex = 8;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Location = new System.Drawing.Point(790, 408);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 9;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.OnCancel);
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.Location = new System.Drawing.Point(698, 408);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 10;
            this.btnOk.Text = "OK";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.OnOK);
            // 
            // btnRestore
            // 
            this.btnRestore.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRestore.Location = new System.Drawing.Point(534, 408);
            this.btnRestore.Name = "btnRestore";
            this.btnRestore.Size = new System.Drawing.Size(109, 23);
            this.btnRestore.TabIndex = 11;
            this.btnRestore.Text = "Restore defaults";
            this.btnRestore.UseVisualStyleBackColor = true;
            this.btnRestore.Click += new System.EventHandler(this.OnRestoreDefaults);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.exampleText, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.templateText, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label_example, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.variablesText, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.label_variables, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.label_template, 0, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(258, 11);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 6;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 16F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 16F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 16F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(607, 386);
            this.tableLayoutPanel1.TabIndex = 12;
            // 
            // TemplatesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(878, 443);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.btnRestore);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.snippetsList);
            this.Controls.Add(this.languageCombo);
            this.Controls.Add(this.label1);
            this.Name = "TemplatesForm";
            this.Text = "Code templates";
            this.Load += new System.EventHandler(this.OnLoad);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox languageCombo;
        private System.Windows.Forms.ListBox snippetsList;
        private System.Windows.Forms.TextBox templateText;
        private System.Windows.Forms.Label label_template;
        private System.Windows.Forms.Label label_variables;
        private System.Windows.Forms.TextBox variablesText;
        private System.Windows.Forms.Label label_example;
        private System.Windows.Forms.TextBox exampleText;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnRestore;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}