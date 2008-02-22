namespace gep
{
    partial class GraphForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GraphForm));
            this.hScrollBar = new System.Windows.Forms.HScrollBar();
            this.vScrollBar = new System.Windows.Forms.VScrollBar();
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.btnPlay = new System.Windows.Forms.ToolStripButton();
            this.btnPause = new System.Windows.Forms.ToolStripButton();
            this.btnStop = new System.Windows.Forms.ToolStripButton();
            this.labelState = new System.Windows.Forms.ToolStripLabel();
            this.labelPosition = new System.Windows.Forms.ToolStripLabel();
            this.zoomCombo = new System.Windows.Forms.ToolStripComboBox();
            this.btnSlider = new System.Windows.Forms.ToolStripButton();
            this.toolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // hScrollBar
            // 
            this.hScrollBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.hScrollBar.Location = new System.Drawing.Point(0, 484);
            this.hScrollBar.Name = "hScrollBar";
            this.hScrollBar.Size = new System.Drawing.Size(685, 16);
            this.hScrollBar.TabIndex = 0;
            this.hScrollBar.ValueChanged += new System.EventHandler(this.hScrollBar_ValueChanged);
            // 
            // vScrollBar
            // 
            this.vScrollBar.Dock = System.Windows.Forms.DockStyle.Right;
            this.vScrollBar.Location = new System.Drawing.Point(669, 0);
            this.vScrollBar.Name = "vScrollBar";
            this.vScrollBar.Size = new System.Drawing.Size(16, 484);
            this.vScrollBar.TabIndex = 1;
            this.vScrollBar.ValueChanged += new System.EventHandler(this.vScrollBar_ValueChanged);
            // 
            // toolStrip
            // 
            this.toolStrip.AutoSize = false;
            this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnPlay,
            this.btnPause,
            this.btnStop,
            this.labelState,
            this.labelPosition,
            this.zoomCombo,
            this.btnSlider});
            this.toolStrip.Location = new System.Drawing.Point(0, 0);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new System.Drawing.Size(669, 23);
            this.toolStrip.TabIndex = 2;
            this.toolStrip.Text = "toolStrip1";
            this.toolStrip.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OnToolStripMouseUp);
            this.toolStrip.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnToolStripMouseMove);
            this.toolStrip.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnToolStripMouseDown);
            this.toolStrip.MouseLeave += new System.EventHandler(this.OnToolStripMouseLeave);
            // 
            // btnPlay
            // 
            this.btnPlay.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnPlay.Image = ((System.Drawing.Image)(resources.GetObject("btnPlay.Image")));
            this.btnPlay.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnPlay.Name = "btnPlay";
            this.btnPlay.Size = new System.Drawing.Size(23, 20);
            this.btnPlay.Text = "toolStripButton1";
            this.btnPlay.ToolTipText = "Run graph";
            this.btnPlay.Click += new System.EventHandler(this.OnPlay);
            // 
            // btnPause
            // 
            this.btnPause.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnPause.Image = ((System.Drawing.Image)(resources.GetObject("btnPause.Image")));
            this.btnPause.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnPause.Name = "btnPause";
            this.btnPause.Size = new System.Drawing.Size(23, 20);
            this.btnPause.Text = "toolStripButton2";
            this.btnPause.ToolTipText = "Pause graph";
            this.btnPause.Click += new System.EventHandler(this.OnPause);
            // 
            // btnStop
            // 
            this.btnStop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnStop.Image = ((System.Drawing.Image)(resources.GetObject("btnStop.Image")));
            this.btnStop.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(23, 20);
            this.btnStop.Text = "toolStripButton3";
            this.btnStop.ToolTipText = "Stop graph";
            this.btnStop.Click += new System.EventHandler(this.OnStop);
            // 
            // labelState
            // 
            this.labelState.AutoSize = false;
            this.labelState.Name = "labelState";
            this.labelState.Size = new System.Drawing.Size(50, 20);
            this.labelState.Text = "Stopped";
            this.labelState.ToolTipText = "Graph state";
            // 
            // labelPosition
            // 
            this.labelPosition.AutoSize = false;
            this.labelPosition.BackColor = System.Drawing.Color.Silver;
            this.labelPosition.Name = "labelPosition";
            this.labelPosition.Size = new System.Drawing.Size(140, 20);
            this.labelPosition.Text = "00,00 / 00,00";
            this.labelPosition.ToolTipText = "Position and duration";
            // 
            // zoomCombo
            // 
            this.zoomCombo.AutoSize = false;
            this.zoomCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.zoomCombo.Name = "zoomCombo";
            this.zoomCombo.Size = new System.Drawing.Size(55, 23);
            this.zoomCombo.ToolTipText = "Zoom";
            this.zoomCombo.SelectedIndexChanged += new System.EventHandler(this.OnZoomChanged);
            // 
            // btnSlider
            // 
            this.btnSlider.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSlider.Image = ((System.Drawing.Image)(resources.GetObject("btnSlider.Image")));
            this.btnSlider.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSlider.Name = "btnSlider";
            this.btnSlider.Size = new System.Drawing.Size(23, 20);
            this.btnSlider.Text = "toolStripButton1";
            this.btnSlider.Visible = false;
            // 
            // GraphForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.LightGray;
            this.ClientSize = new System.Drawing.Size(685, 500);
            this.Controls.Add(this.toolStrip);
            this.Controls.Add(this.vScrollBar);
            this.Controls.Add(this.hScrollBar);
            this.DoubleBuffered = true;
            this.KeyPreview = true;
            this.Name = "GraphForm";
            this.Text = "Graph";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.GraphForm_Paint);
            this.SizeChanged += new System.EventHandler(this.GraphForm_SizeChanged);
            this.Activated += new System.EventHandler(this.GraphForm_Activated);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.GraphForm_MouseUp);
            this.MouseLeave += new System.EventHandler(this.GraphForm_MouseLeave);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.GraphForm_FormClosing);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.GraphForm_MouseMove);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.GraphForm_KeyDown);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.GraphForm_MouseDown);
            this.Load += new System.EventHandler(this.GraphForm_Load);
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.HScrollBar hScrollBar;
        private System.Windows.Forms.VScrollBar vScrollBar;
        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripButton btnPlay;
        private System.Windows.Forms.ToolStripButton btnPause;
        private System.Windows.Forms.ToolStripButton btnStop;
        private System.Windows.Forms.ToolStripLabel labelState;
        private System.Windows.Forms.ToolStripLabel labelPosition;
        private System.Windows.Forms.ToolStripComboBox zoomCombo;
        private System.Windows.Forms.ToolStripButton btnSlider;
    }
}