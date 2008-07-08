namespace gep
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.mainmenu = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.connectToRunningToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.disconnectFromRunningToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renderFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renderURLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.generateCodeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.saveGraphAsImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.autoLayoutFiltersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.suggestURLsForSourcesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.useClockToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.codeTemplatesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.useDirectConnectMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.favoritesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearFavoritesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.windowsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.maintoolbar = new System.Windows.Forms.ToolStrip();
            this.newToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.openToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.saveToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.buyToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.status = new System.Windows.Forms.StatusStrip();
            this.status_label = new System.Windows.Forms.ToolStripStatusLabel();
            this.mainmenu.SuspendLayout();
            this.maintoolbar.SuspendLayout();
            this.status.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainmenu
            // 
            this.mainmenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.optionsToolStripMenuItem,
            this.favoritesToolStripMenuItem,
            this.windowsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.mainmenu.Location = new System.Drawing.Point(0, 0);
            this.mainmenu.Name = "mainmenu";
            this.mainmenu.Size = new System.Drawing.Size(1016, 24);
            this.mainmenu.TabIndex = 0;
            this.mainmenu.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.openToolStripMenuItem,
            this.connectToRunningToolStripMenuItem,
            this.disconnectFromRunningToolStripMenuItem,
            this.renderFileToolStripMenuItem,
            this.renderURLToolStripMenuItem,
            this.toolStripSeparator,
            this.saveToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.generateCodeToolStripMenuItem,
            this.saveGraphAsImageToolStripMenuItem,
            this.toolStripSeparator1,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("newToolStripMenuItem.Image")));
            this.newToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.newToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.newToolStripMenuItem.Text = "&New";
            this.newToolStripMenuItem.Click += new System.EventHandler(this.OnNewGraph);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("openToolStripMenuItem.Image")));
            this.openToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.openToolStripMenuItem.Text = "&Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // connectToRunningToolStripMenuItem
            // 
            this.connectToRunningToolStripMenuItem.Name = "connectToRunningToolStripMenuItem";
            this.connectToRunningToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.connectToRunningToolStripMenuItem.Text = "Connect to running...";
            this.connectToRunningToolStripMenuItem.Click += new System.EventHandler(this.OnConnectToROT);
            // 
            // disconnectFromRunningToolStripMenuItem
            // 
            this.disconnectFromRunningToolStripMenuItem.Enabled = false;
            this.disconnectFromRunningToolStripMenuItem.Name = "disconnectFromRunningToolStripMenuItem";
            this.disconnectFromRunningToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.disconnectFromRunningToolStripMenuItem.Text = "Disconnect from running";
            this.disconnectFromRunningToolStripMenuItem.Click += new System.EventHandler(this.disconnectFromRunningToolStripMenuItem_Click);
            // 
            // renderFileToolStripMenuItem
            // 
            this.renderFileToolStripMenuItem.Name = "renderFileToolStripMenuItem";
            this.renderFileToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.renderFileToolStripMenuItem.Text = "Render file...";
            this.renderFileToolStripMenuItem.Click += new System.EventHandler(this.OnRenderFile);
            // 
            // renderURLToolStripMenuItem
            // 
            this.renderURLToolStripMenuItem.Name = "renderURLToolStripMenuItem";
            this.renderURLToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.renderURLToolStripMenuItem.Text = "Render URL...";
            this.renderURLToolStripMenuItem.Click += new System.EventHandler(this.OnRenderURL);
            // 
            // toolStripSeparator
            // 
            this.toolStripSeparator.Name = "toolStripSeparator";
            this.toolStripSeparator.Size = new System.Drawing.Size(204, 6);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("saveToolStripMenuItem.Image")));
            this.saveToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.saveToolStripMenuItem.Text = "&Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.saveAsToolStripMenuItem.Text = "Save &As";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
            // 
            // generateCodeToolStripMenuItem
            // 
            this.generateCodeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cToolStripMenuItem,
            this.cToolStripMenuItem1});
            this.generateCodeToolStripMenuItem.Name = "generateCodeToolStripMenuItem";
            this.generateCodeToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.generateCodeToolStripMenuItem.Text = "Generate code";
            // 
            // cToolStripMenuItem
            // 
            this.cToolStripMenuItem.Name = "cToolStripMenuItem";
            this.cToolStripMenuItem.Size = new System.Drawing.Size(98, 22);
            this.cToolStripMenuItem.Text = "C++";
            this.cToolStripMenuItem.Click += new System.EventHandler(this.OnGenCode);
            // 
            // cToolStripMenuItem1
            // 
            this.cToolStripMenuItem1.Name = "cToolStripMenuItem1";
            this.cToolStripMenuItem1.Size = new System.Drawing.Size(98, 22);
            this.cToolStripMenuItem1.Text = "C#";
            this.cToolStripMenuItem1.Click += new System.EventHandler(this.OnGenCodeCS);
            // 
            // saveGraphAsImageToolStripMenuItem
            // 
            this.saveGraphAsImageToolStripMenuItem.Name = "saveGraphAsImageToolStripMenuItem";
            this.saveGraphAsImageToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.saveGraphAsImageToolStripMenuItem.Text = "Save graph as image";
            this.saveGraphAsImageToolStripMenuItem.Click += new System.EventHandler(this.OnSaveImage);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(204, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.autoLayoutFiltersToolStripMenuItem,
            this.suggestURLsForSourcesToolStripMenuItem,
            this.useClockToolStripMenuItem,
            this.codeTemplatesToolStripMenuItem,
            this.useDirectConnectMenuItem});
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.optionsToolStripMenuItem.Text = "Options";
            // 
            // autoLayoutFiltersToolStripMenuItem
            // 
            this.autoLayoutFiltersToolStripMenuItem.Name = "autoLayoutFiltersToolStripMenuItem";
            this.autoLayoutFiltersToolStripMenuItem.Size = new System.Drawing.Size(274, 22);
            this.autoLayoutFiltersToolStripMenuItem.Text = "AutoArrange filters";
            this.autoLayoutFiltersToolStripMenuItem.Click += new System.EventHandler(this.OnAutoArrage);
            // 
            // suggestURLsForSourcesToolStripMenuItem
            // 
            this.suggestURLsForSourcesToolStripMenuItem.Name = "suggestURLsForSourcesToolStripMenuItem";
            this.suggestURLsForSourcesToolStripMenuItem.Size = new System.Drawing.Size(274, 22);
            this.suggestURLsForSourcesToolStripMenuItem.Text = "Suggest URLs for sources";
            this.suggestURLsForSourcesToolStripMenuItem.Click += new System.EventHandler(this.OnSuggestURLs);
            // 
            // useClockToolStripMenuItem
            // 
            this.useClockToolStripMenuItem.Name = "useClockToolStripMenuItem";
            this.useClockToolStripMenuItem.Size = new System.Drawing.Size(274, 22);
            this.useClockToolStripMenuItem.Text = "Use clock";
            this.useClockToolStripMenuItem.Click += new System.EventHandler(this.OnUseClock);
            // 
            // codeTemplatesToolStripMenuItem
            // 
            this.codeTemplatesToolStripMenuItem.Name = "codeTemplatesToolStripMenuItem";
            this.codeTemplatesToolStripMenuItem.Size = new System.Drawing.Size(274, 22);
            this.codeTemplatesToolStripMenuItem.Text = "Code templates...";
            this.codeTemplatesToolStripMenuItem.Click += new System.EventHandler(this.OnCodeTemplates);
            // 
            // useDirectConnectMenuItem
            // 
            this.useDirectConnectMenuItem.Name = "useDirectConnectMenuItem";
            this.useDirectConnectMenuItem.Size = new System.Drawing.Size(274, 22);
            this.useDirectConnectMenuItem.Text = "Use direct connect in code generation";
            this.useDirectConnectMenuItem.Click += new System.EventHandler(this.OnUseDirectConnect);
            // 
            // favoritesToolStripMenuItem
            // 
            this.favoritesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.clearFavoritesToolStripMenuItem,
            this.toolStripSeparator2});
            this.favoritesToolStripMenuItem.Name = "favoritesToolStripMenuItem";
            this.favoritesToolStripMenuItem.Size = new System.Drawing.Size(66, 20);
            this.favoritesToolStripMenuItem.Text = "Favorites";
            // 
            // clearFavoritesToolStripMenuItem
            // 
            this.clearFavoritesToolStripMenuItem.Name = "clearFavoritesToolStripMenuItem";
            this.clearFavoritesToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
            this.clearFavoritesToolStripMenuItem.Text = "Clear favorites";
            this.clearFavoritesToolStripMenuItem.Click += new System.EventHandler(this.OnClearFavorites);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(146, 6);
            // 
            // windowsToolStripMenuItem
            // 
            this.windowsToolStripMenuItem.Name = "windowsToolStripMenuItem";
            this.windowsToolStripMenuItem.Size = new System.Drawing.Size(68, 20);
            this.windowsToolStripMenuItem.Text = "Windows";
            this.windowsToolStripMenuItem.DropDownOpening += new System.EventHandler(this.windowsToolStripMenuItem_DropDownOpening);
            this.windowsToolStripMenuItem.DropDownClosed += new System.EventHandler(this.windowsToolStripMenuItem_DropDownClosed);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "&Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.aboutToolStripMenuItem.Text = "&About...";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // maintoolbar
            // 
            this.maintoolbar.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.maintoolbar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripButton,
            this.openToolStripButton,
            this.saveToolStripButton,
            this.toolStripSeparator6,
            this.buyToolStripButton});
            this.maintoolbar.Location = new System.Drawing.Point(0, 24);
            this.maintoolbar.Name = "maintoolbar";
            this.maintoolbar.Size = new System.Drawing.Size(1016, 25);
            this.maintoolbar.TabIndex = 2;
            this.maintoolbar.Text = "toolStrip1";
            // 
            // newToolStripButton
            // 
            this.newToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.newToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("newToolStripButton.Image")));
            this.newToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.newToolStripButton.Name = "newToolStripButton";
            this.newToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.newToolStripButton.Text = "&New";
            this.newToolStripButton.ToolTipText = "New graph";
            this.newToolStripButton.Click += new System.EventHandler(this.newToolStripButton_Click);
            // 
            // openToolStripButton
            // 
            this.openToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.openToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("openToolStripButton.Image")));
            this.openToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.openToolStripButton.Name = "openToolStripButton";
            this.openToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.openToolStripButton.Text = "&Open";
            this.openToolStripButton.ToolTipText = "Open graph";
            this.openToolStripButton.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // saveToolStripButton
            // 
            this.saveToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.saveToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("saveToolStripButton.Image")));
            this.saveToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.saveToolStripButton.Name = "saveToolStripButton";
            this.saveToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.saveToolStripButton.Text = "&Save";
            this.saveToolStripButton.ToolTipText = "Save graph";
            this.saveToolStripButton.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(6, 25);
            // 
            // buyToolStripButton
            // 
            this.buyToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buyToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("buyToolStripButton.Image")));
            this.buyToolStripButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.buyToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buyToolStripButton.Name = "buyToolStripButton";
            this.buyToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.buyToolStripButton.Text = "toolStripButton1";
            this.buyToolStripButton.ToolTipText = "Register now to unlock code generation, use more than 30 days and get rights for " +
                "support.";
            this.buyToolStripButton.Click += new System.EventHandler(this.buyToolStripButton_Click);
            // 
            // status
            // 
            this.status.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.status_label});
            this.status.Location = new System.Drawing.Point(0, 691);
            this.status.Name = "status";
            this.status.Size = new System.Drawing.Size(1016, 22);
            this.status.TabIndex = 4;
            this.status.Text = "statusStrip1";
            // 
            // status_label
            // 
            this.status_label.Name = "status_label";
            this.status_label.Size = new System.Drawing.Size(71, 17);
            this.status_label.Text = "status string";
            // 
            // MainForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1016, 713);
            this.Controls.Add(this.status);
            this.Controls.Add(this.maintoolbar);
            this.Controls.Add(this.mainmenu);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.IsMdiContainer = true;
            this.MainMenuStrip = this.mainmenu;
            this.Name = "MainForm";
            this.Text = "GraphEditPlus";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.OnDragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.OnDragEnter);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnClosing);
            this.mainmenu.ResumeLayout(false);
            this.mainmenu.PerformLayout();
            this.maintoolbar.ResumeLayout(false);
            this.maintoolbar.PerformLayout();
            this.status.ResumeLayout(false);
            this.status.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip mainmenu;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStrip maintoolbar;
        private System.Windows.Forms.ToolStripButton newToolStripButton;
        private System.Windows.Forms.ToolStripButton openToolStripButton;
        private System.Windows.Forms.ToolStripButton saveToolStripButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem connectToRunningToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem generateCodeToolStripMenuItem;
        private System.Windows.Forms.StatusStrip status;
        private System.Windows.Forms.ToolStripStatusLabel status_label;
        private System.Windows.Forms.ToolStripButton buyToolStripButton;
        private System.Windows.Forms.ToolStripMenuItem renderFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem autoLayoutFiltersToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem favoritesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearFavoritesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem saveGraphAsImageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem disconnectFromRunningToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem windowsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem renderURLToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem suggestURLsForSourcesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem useClockToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem codeTemplatesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem useDirectConnectMenuItem;
    }
}

