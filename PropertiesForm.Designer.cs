namespace gep
{
    partial class PropertiesForm
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
            this.propgrid = new System.Windows.Forms.PropertyGrid();
            this.SuspendLayout();
            // 
            // propgrid
            // 
            this.propgrid.CausesValidation = false;
            this.propgrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propgrid.Location = new System.Drawing.Point(0, 0);
            this.propgrid.Name = "propgrid";
            this.propgrid.Size = new System.Drawing.Size(337, 463);
            this.propgrid.TabIndex = 0;
            // 
            // PropertiesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(337, 463);
            this.ControlBox = false;
            this.Controls.Add(this.propgrid);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Location = new System.Drawing.Point(600, 0);
            this.Name = "PropertiesForm";
            this.Text = "Properties";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.PropertiesForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PropertyGrid propgrid;
    }
}