using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Diagnostics;

namespace gep
{
    partial class MainForm : Form
    {
        string filetoopen;
        public MainForm(string[] args)
        {
            InitializeComponent();
            if (args.Length > 0 && args[0]!="log")
                filetoopen = args[0];
        }

        private void OnNewGraph(object sender, EventArgs e)
        {
            if (RegistryChecker.R[0] == 0)
                Filterz.rch.StartChecking(email, code); //in sepatate thread
            GraphForm gf = new GraphForm();
            gf.MdiParent = this;
            gf.Show();
        }

        private GraphForm activeGraphForm;

        public GraphForm ActiveGraphForm
        {
            get { return activeGraphForm;  }
            set 
            { 
                activeGraphForm = value;
                if (activeGraphForm != null)
                {
                    disconnectFromRunningToolStripMenuItem.Enabled = activeGraphForm.IsFromRot;
                    useClockToolStripMenuItem.Checked = activeGraphForm.UseClock;
                }
            }
        }

        public Filterz filterz;
        public PropertiesForm propform;
        public string email, code;
        public string keyname = @"Software\Infognition\GraphEditPlus";

        private void MainForm_Load(object sender, EventArgs e)
        {
            Program.mainform = this;

            propform = new PropertiesForm();
            propform.MdiParent = this;
            propform.Show();

            filterz = new Filterz();
            filterz.MdiParent = this;
            filterz.Show();

            Size sz = ClientSize;
            filterz.Location = new Point(0, 0);
            int w = 300, h = (sz.Height-75) / 2;
            filterz.Size = new Size(w, h);
            //filterz.PerformAutoScale();
            //filterz.PerformLayout();
            propform.Location = new Point(0, h);
            propform.Size = new Size(w, h);
            //propform.PerformLayout();

            RegistryKey nrk = Registry.CurrentUser.OpenSubKey(keyname);
            if (nrk==null)
                nrk = Registry.CurrentUser.CreateSubKey(keyname);

            email = (string)nrk.GetValue("email", "");
            code = (string)nrk.GetValue("code", "");

            if (email.Length == 0)
            {   // try reading from old location
                RegistryKey ork = Registry.CurrentUser.OpenSubKey(@"Software\Dee Mon\GraphEditPlus");
                if (ork != null)
                {
                    email = (string)ork.GetValue("email", "");
                    code = (string)ork.GetValue("code", "");
                }
            }

            int sugg_url = (int)nrk.GetValue("suggestURL", 0);
            suggestURLs = sugg_url > 0;

            int useDirCon = (int) nrk.GetValue("useDirectConnect", 1);
            useDirectConnect = useDirCon > 0;

            createFiltersByName = (int)nrk.GetValue("createFiltersByName", 1) > 0;
            autoArrange = (int)nrk.GetValue("autoArrange", 0) > 0;

            if (filetoopen == null)
                OnNewGraph(null, null);
            else
                openToolStripMenuItem_Click(null, null);

            LoadFavorites(nrk);
        }

        public void HideRegisterButton()
        {
            if (RegistryChecker.R[1] == 1) //todo: make this thing happen after checker thread ends
                buyToolStripButton.Visible = false;
            else
            {
                ToolStripMenuItem item = new ToolStripMenuItem();
                item.Text = "Register";
                item.Click += buyToolStripButton_Click;
                helpToolStripMenuItem.DropDownItems.Add(item);
            }
        }

        private void newToolStripButton_Click(object sender, EventArgs e)
        {
            OnNewGraph(sender, e);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (RegistryChecker.R[0] == 0)
                Filterz.rch.CheckCode(email, code);
            GraphForm gf = new GraphForm();
            gf.MdiParent = this;
            string s = gf.Text;
            if (filetoopen == null)
                gf.LoadGraph(sender, e);
            else
            {                
                gf.DoLoad(filetoopen);
                filetoopen = null;
            }
            if (s == gf.Text)//did not open
                gf.Close();
            else
                gf.Show();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (activeGraphForm != null)
                activeGraphForm.SaveGraph(sender, e);
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (activeGraphForm != null)
                activeGraphForm.SaveGraphAs(sender, e);
        }

        private void OnConnectToROT(object sender, EventArgs e)
        {
            RotForm rf = new RotForm();
            rf.MdiParent = this;
            rf.Show();
        }

        private void OnGenCode(object sender, EventArgs e)
        {
            if (activeGraphForm != null)
                activeGraphForm.GenerateCode(Lang.CPP);
        }

        string lasthint;

        public void SetHint(string hint)
        {
            if (hint != lasthint)
            {
                status_label.Text = hint;
                lasthint = hint;
            }
        }

        private void buyToolStripButton_Click(object sender, EventArgs e)
        {
            using(var rf = new RegisterForm())
                rf.ShowDialog();
            if (RegistryChecker.R[1] == 1)
                buyToolStripButton.Visible = false;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

/*
        private void contentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //todo: show help..
        }
*/

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using(var af = new AboutForm())
                af.ShowDialog();
        }

        private void OnRenderFile(object sender, EventArgs e)
        {
            if (activeGraphForm == null)
                OnNewGraph(sender, e);
            if (activeGraphForm != null)
                activeGraphForm.RenderFile(null, null);
        }

        public bool autoArrange = false;
        public bool suggestURLs = false;
        public bool useDirectConnect = true; // in code generation
        public bool createFiltersByName = true;


        List<FilterPropsKernel> favlist = new List<FilterPropsKernel>();

        void LoadFavorites(RegistryKey rk) //rk must be open
        {
            string[] ss = (string[])rk.GetValue("favorites");
            if (ss == null) return;
            List<string> slist = new List<string>(ss);
            FilterPropsKernel fk = FilterPropsKernel.FromList(slist);
            while (fk != null)
            {
                favlist.Add(fk);
                AddToFavorites(fk.MkFilterProps());
                fk = FilterPropsKernel.FromList(slist);
            }           
        }

        void SaveFavorites(RegistryKey rk) //rk must be open
        {
            List<string> slist = new List<string>();
            foreach (FilterPropsKernel fk in favlist)
                fk.SaveTo(slist);            
            rk.SetValue("favorites", slist.ToArray());
        }

        public void AddToFavorites(FilterProps fp)
        {
            foreach (ToolStripItem it in favoritesToolStripMenuItem.DropDownItems)
                if (it.Text == fp.Name)
                    return;
            ToolStripMenuItem item = new ToolStripMenuItem();
            item.Text = fp.Name;
            item.Click += delegate{
                if (activeGraphForm != null)
                    activeGraphForm.AddFilter(fp);
            };
            favoritesToolStripMenuItem.DropDownItems.Add(item);
            favlist.Add(fp.Kernel());
        }

        private void OnClearFavorites(object sender, EventArgs e)
        {
            ToolStripItem[] items = new ToolStripItem[2];
            items[0] = favoritesToolStripMenuItem.DropDownItems[0];
            items[1] = favoritesToolStripMenuItem.DropDownItems[1];
            favoritesToolStripMenuItem.DropDownItems.Clear();
            favoritesToolStripMenuItem.DropDownItems.AddRange(items);
            favlist.Clear();
        }

        private void OnSaveImage(object sender, EventArgs e)
        {
            if (activeGraphForm != null)
            {
                using (Bitmap bmp = activeGraphForm.MakeImage())
                using (var fd = new SaveFileDialog())
                {
                    fd.DefaultExt = "*.png";
                    fd.Filter = "PNG files (*.png)|*.png|All files (*.*)|*.*";
                    if (fd.ShowDialog() == DialogResult.OK)
                        bmp.Save(fd.FileName);
                }
            }
        }

        private void disconnectFromRunningToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (activeGraphForm != null)
                activeGraphForm.Close();
        }

        private void windowsToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            windowsToolStripMenuItem.DropDownItems.Clear();
            foreach (Form f in MdiChildren)
            {
                ToolStripMenuItem item = new ToolStripMenuItem();
                item.Text = f.Text;
                Form ff = f;
                item.Click += delegate { ff.BringToFront(); };
                windowsToolStripMenuItem.DropDownItems.Add(item);
            }
        }

        private void windowsToolStripMenuItem_DropDownClosed(object sender, EventArgs e)
        {
            windowsToolStripMenuItem.DropDownItems.Clear();
        }

        private void OnRenderURL(object sender, EventArgs e)
        {
            using (var rf = new RenderURLForm("Render URL"))
            {
                rf.ShowDialog();
                if (rf.selectedURL != null)
                {
                    if (activeGraphForm == null)
                        OnNewGraph(sender, e);
                    if (activeGraphForm != null)
                        activeGraphForm.RenderURL(rf.selectedURL);
                }
            }
        }

        private void OnClosing(object sender, FormClosingEventArgs e)
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey(keyname, true) ??
                             Registry.CurrentUser.CreateSubKey(keyname);
            if (rk != null)
            {
                rk.SetValue("suggestURL", suggestURLs ? 1 : 0);
                rk.SetValue("useDirectConnect", useDirectConnect ? 1 : 0);
                rk.SetValue("createFiltersByName", createFiltersByName ? 1 : 0);
                rk.SetValue("autoArrange", autoArrange ? 1 : 0);
                SaveFavorites(rk);
            }
        }

        private void OnUseClock(object sender, EventArgs e)
        {
            if (activeGraphForm == null) return;
            activeGraphForm.UseClock = !activeGraphForm.UseClock;
            useClockToolStripMenuItem.Checked = activeGraphForm.UseClock;
        }

        private void OnGenCodeCS(object sender, EventArgs e)
        {
            if (activeGraphForm != null)
                activeGraphForm.GenerateCode(Lang.CS);
        }

        private void OnCodeTemplates(object sender, EventArgs e)
        {
            using(var tf = new TemplatesForm())
                tf.ShowDialog();
        }

        private void OnDragDrop(object sender, DragEventArgs e)
        {
            if (e == null || e.Data == null || !e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            foreach (string fname in (IEnumerable<string>) e.Data.GetData(DataFormats.FileDrop))
            {
                if (fname.ToLowerInvariant().EndsWith(".grf")) {
                    filetoopen = fname;
                    openToolStripMenuItem_Click(null, null);
                } 
                else
                {
                    if (activeGraphForm == null) OnNewGraph(sender, e);
                    if (activeGraphForm != null) activeGraphForm.RenderThisFile(fname);
                }
            }
        }

        private void OnDragEnter(object sender, DragEventArgs e)
        {
            if (e == null || e.Data == null || !e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            e.Effect = DragDropEffects.Copy;
        }

        public Rectangle MaximumRectangle
        {
            get  {
                List<Rectangle> obs = new List<Rectangle>( new Rectangle[] {
                    filterz.Bounds, propform.Bounds });
                int maxx = ClientSize.Width - 10, maxy = ClientSize.Height - 80;
                List<int> xs = new List<int>(new int[] { 0, maxx });
                List<int> ys = new List<int>(new int[] { 0, maxy });
                foreach (Rectangle or in obs)
                {
                    xs.Add(or.Left); xs.Add(or.Right);
                    ys.Add(or.Top); ys.Add(or.Bottom);
                }
                xs.RemoveAll(delegate(int x) { return x > maxx; });
                ys.RemoveAll(delegate(int y) { return y > maxy; });
                xs.Sort(); ys.Sort();

                Rectangle bestrec = new Rectangle(100,100,300,300);
                int bestarea = 0;

                for(int l=0; l<xs.Count-1; l++)
                    for(int r=1; r<xs.Count; r++)
                        for(int t=0; t<ys.Count-1; t++)
                            for (int b = 1; b < ys.Count; b++)
                            {
                                int rcl = xs[l]+2, rcr = xs[r]-2 , rct = ys[t]+2, rcb = ys[b]-2;
                                Rectangle rc = new Rectangle(Math.Min(rcl, rcr), Math.Min(rct, rcb),
                                    Math.Abs(rcr-rcl), Math.Abs(rcb-rct));
                                int area = rc.Width * rc.Height;
                                if (!obs.Exists(delegate(Rectangle or) { return rc.IntersectsWith(or); })
                                    && area > bestarea)
                                {
                                    bestarea = area;
                                    bestrec = rc;
                                }
                            }

                return bestrec;                
            }
        }

        private void OnPreferences(object sender, EventArgs e)
        {
            using(var frm = new PreferencesForm(autoArrange, suggestURLs, useDirectConnect, createFiltersByName))
            if (frm.ShowDialog() == DialogResult.OK)
            {
                autoArrange = frm.autoArrange;
                useDirectConnect = frm.useDirectConnect;
                suggestURLs = frm.suggestURLs;
                createFiltersByName = frm.createFiltersByName;

                if (autoArrange && activeGraphForm != null)
                    activeGraphForm.LayoutFilters();
            }
        }
    }//class

    
}