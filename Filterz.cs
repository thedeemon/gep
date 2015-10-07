using System;
using System.Collections.Generic;
using System.Windows.Forms;
using DirectShowLib;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Diagnostics;
using System.Threading;
using System.Drawing;

namespace gep
{
    partial class Filterz : Form
    {
        public Filterz()
        {
            dmo_cats.Clear();
            dmo_cats.Add("DMO Audio Effects", DirectShowLib.DMO.DMOCategory.AudioEffect);
            dmo_cats.Add("DMO Video Effects", DirectShowLib.DMO.DMOCategory.VideoEffect);
            dmo_cats.Add("DMO Audio Capture Effects", DirectShowLib.DMO.DMOCategory.AudioCaptureEffect);

            InitializeComponent();
            string nbits = IntPtr.Size == 4 ? "32" : "64";
            this.Text = "Filters (" + nbits + " bits)";
            filtertree.Sorted = true;
        }

        public static Dictionary<string, string> catnames = new Dictionary<string,string>(); //guid => name
        private static Dictionary<string, Guid> catguids = new Dictionary<string, Guid>();
        public static Dictionary<string, string> friendlyNames = new Dictionary<string, string>(); //guid=>name
        public static RegistryChecker rch = new RegistryChecker();
        private static Dictionary<string, Guid> dmo_cats = new Dictionary<string,Guid>(); 

        private static void Addcat(Guid g, string dft_name)
        {
            string sg = Graph.GuidToString(g);
            if (!catnames.ContainsKey(sg))
                catnames.Add(sg, dft_name);
        }

        public void RefreshCategories()
        {
            object old_selection = catcombo.SelectedItem;

            //clear values
            catcombo.Items.Clear();
            catnames.Clear();
            catguids.Clear();
            all_filters = null;

            try
            {
                //fill catnames dictionary (guid_string => category_name)
                ICreateDevEnum devenum = new CreateDevEnum() as ICreateDevEnum;
                IEnumMoniker emon;
                int hr = devenum.CreateClassEnumerator(FilterCategory.ActiveMovieCategories, out emon, 0);

                if (0 == hr)
                {
                    IMoniker[] mon = new IMoniker[1];
                    while (0 == emon.Next(1, mon, IntPtr.Zero))
                    {
                        string name;
                        mon[0].GetDisplayName(null, null, out name);
                        string sg = name.Substring(name.Length - 38, 38).ToUpperInvariant();

                        object bagObj;
                        Guid propertyBagId = typeof(IPropertyBag).GUID;
                        mon[0].BindToStorage(null, null, ref propertyBagId, out bagObj);
                        IPropertyBag bag = (IPropertyBag)bagObj;
                        object nameObj;
                        bag.Read("FriendlyName", out nameObj, null);
                        name = nameObj as string;
                        if (!catnames.ContainsKey(sg))
                            catnames.Add(sg, name);

                        Marshal.ReleaseComObject(bagObj);
                        Marshal.ReleaseComObject(mon[0]);
                    }
                }
            }
            catch (COMException e)
            {
                Graph.ShowCOMException(e, "Can't enumerate filter categories");
                return;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Can't enumerate filter categories");
                return;
            }

            Type ty = typeof(Application);
            
            Addcat(FilterCategory.ActiveMovieCategories, "ActiveMovieCategories");
            Addcat(FilterCategory.AMKSAudio, "KS Audio");
            Addcat(FilterCategory.AMKSCapture, "KS Capture");
            Addcat(FilterCategory.AMKSCrossbar, "KS Crossbar");
            Addcat(FilterCategory.AMKSDataCompressor, "KS Data Compressor");
            Addcat(FilterCategory.AMKSRender, "KS Render");
            Addcat(FilterCategory.AMKSSplitter, "KS Splitter");
            Addcat(FilterCategory.AMKSTVAudio, "KS TV Audio");
            Addcat(FilterCategory.AMKSTVTuner, "KS TV Tuner");
            Addcat(FilterCategory.AMKSVBICodec, "KS VBI Codec");
            Addcat(FilterCategory.AMKSVideo, "KS Video");
            Addcat(FilterCategory.AudioCompressorCategory, "Audio Compressors");
            Addcat(FilterCategory.AudioEffects1Category, "Audio Effects 1");
            Addcat(FilterCategory.AudioEffects2Category, "Audio Effects 2");
            Addcat(FilterCategory.AudioInputDevice, "Audio Input Devices");
            Addcat(FilterCategory.AudioRendererCategory, "Audio Renderers");
            Addcat(FilterCategory.BDANetworkProvidersCategory, "BDA Network Providers");
            Addcat(FilterCategory.BDAReceiverComponentsCategory, "BDA Receiver Components");
            Addcat(FilterCategory.BDARenderingFiltersCategory, "BDA Rendering Filters");
            Addcat(FilterCategory.BDASourceFiltersCategory, "BDA Source Filters");
            Addcat(FilterCategory.BDATransportInformationRenderersCategory, "BDA Transport Information Renderers");
            Addcat(FilterCategory.CPCAFiltersCategory, "CPCA Filters");
            Addcat(FilterCategory.DeviceControlCategory, "Device Controls");
            Addcat(FilterCategory.DMOFilterCategory, "DMO Filters");
            Addcat(FilterCategory.KSAudioDevice, "KS Audio Devices");
            Addcat(FilterCategory.KSCommunicationsTransform, "KS Communications Transforms");
            Addcat(FilterCategory.KSDataDecompressor, "KS Data Decompressors");
            Addcat(FilterCategory.KSDataTransform, "KS Data Transforms");
            Addcat(FilterCategory.KSInterfaceTransform, "KS Interface Transforms");
            Addcat(FilterCategory.KSMixer, "KS Mixers");
            Addcat(FilterCategory.LegacyAmFilterCategory, "DirectShow Filters"); //
            Addcat(FilterCategory.LTMMVideoProcessors, "LTMM Video Processors");
            Addcat(FilterCategory.MediaEncoderCategory, "Media Encoders");
            Addcat(FilterCategory.MediaMultiplexerCategory, "Media Multiplexers");
            Addcat(FilterCategory.MidiRendererCategory, "Midi Renderers");
            Addcat(FilterCategory.TransmitCategory, "Transmits");
            Addcat(FilterCategory.VideoCompressorCategory, "Video Compressors");
            Addcat(FilterCategory.VideoEffects1Category, "Video Effects 1");
            Addcat(FilterCategory.VideoEffects2Category, "Video Effects 2");
            Addcat(FilterCategory.VideoInputDevice, "Video Input Devices");
            Addcat(FilterCategory.WDMStreamingEncoderDevices, "WDM Streaming Encoder Devices");
            Addcat(FilterCategory.WDMStreamingMultiplexerDevices, "WDM Streaming Multiplexer Devices");

            foreach (KeyValuePair<string, Guid> p in dmo_cats)
                Addcat(p.Value, p.Key);

            //fill combo box and remember guids
            foreach (KeyValuePair<string, string> p in catnames)
            {
                Guid guid = new Guid(p.Key);
                if (!catguids.ContainsKey(p.Value))
                    catguids.Add(p.Value, guid);
                catcombo.Items.Add(p.Value);
            }

            RegistryChecker rch = new RegistryChecker();
            RegistryChecker.CalcDays(ty);
            catcombo.SelectedItem = old_selection ?? catnames[Graph.GuidToString(FilterCategory.LegacyAmFilterCategory)];
        }

        private void Filterz_Load(object sender, EventArgs e)
        {
            ToolTip toolTip = new ToolTip();
            toolTip.AutoPopDelay = 5000;
            toolTip.InitialDelay = 500;
            toolTip.ReshowDelay = 500;
            toolTip.ShowAlways = true;
            toolTip.SetToolTip(btnAdd, "Register a filter from file");
            toolTip.SetToolTip(btnEdit, "Change filter's merit or unregister filter");
            toolTip.SetToolTip(btnRefresh, "Refresh list of categories and filters");
            toolTip.SetToolTip(btnClearSearch, "Clear the search field");
            toolTip.SetToolTip(textBoxSearch, "Enter part of filter name to search");
            toolTip.SetToolTip(checkBoxAllCats, "Search in all categories");
            toolTip.SetToolTip(linkSearchForm, "Advanced search");
            toolTip.SetToolTip(catcombo, "Filter category");

            RefreshCategories();
            filtertree.Focus();
        }

        List<FilterProps> current_category_filters;
        List<FilterProps> all_filters;

        private void catcombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                current_category_filters = GetFiltersOfCategory(catcombo.SelectedItem.ToString());
                ShowSearchedFilters();
            }
            catch (COMException ex)
            {
                Graph.ShowCOMException(ex, "Can't enumerate filter category");
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Can't enumerate filter category");
                return;
            }
        }

        public static List<FilterProps> GetFiltersOfCategory(string cat_name)
        {
            if (dmo_cats.ContainsKey(cat_name))
                return GetDMOFilters(cat_name);
            ICreateDevEnum devenum = new CreateDevEnum() as ICreateDevEnum;
            IEnumMoniker emon;            
            Guid cg = catguids[cat_name];
            int hr = devenum.CreateClassEnumerator(cg, out emon, 0);
            if (hr < 0) return new List<FilterProps>();
            return GetFiltersFromEnum(emon, cg);
        }

        private static List<FilterProps> GetDMOFilters(string cat_name)
        {
            Guid g = dmo_cats[cat_name];
            List<FilterProps> lst = new List<FilterProps>();
            DirectShowLib.DMO.IEnumDMO pEnum;
            if (0 != DirectShowLib.DMO.DMOUtils.DMOEnum(g, DirectShowLib.DMO.DMOEnumerator.IncludeKeyed, 0, 
                        null, 0, null, out pEnum))
                return lst;
            Guid[] clsids = new Guid[1];
            string[] names = new string[1];
            string cat_guid_str = Graph.GuidToString(g);
            while (pEnum.Next(1, clsids, names, IntPtr.Zero) == 0)
            {
                string clsid = Graph.GuidToString(clsids[0]);
                string devname = "@device:dmo:" + clsid + cat_guid_str;
                lst.Add(new FilterProps(names[0], devname, clsid, cat_guid_str));
            }
            return lst;
        }

        public static List<FilterProps> GetAllFilters()
        {
            List<FilterProps> list = new List<FilterProps>();
            try
            {
                Application.UseWaitCursor = true;
                Cursor.Current = Cursors.WaitCursor;
                ICreateDevEnum devenum = new CreateDevEnum() as ICreateDevEnum;
                foreach (KeyValuePair<string, Guid> p in catguids)
                {
                    Guid cg = p.Value;
                    if (dmo_cats.ContainsKey(p.Key))
                        list.AddRange(GetDMOFilters(p.Key));
                    else
                    if (cg != FilterCategory.ActiveMovieCategories)
                    {
                        IEnumMoniker emon;
                        int hr = devenum.CreateClassEnumerator(cg, out emon, 0);
                        if (hr >= 0)
                            list.AddRange(GetFiltersFromEnum(emon, cg));
                    }
                }
            }
            finally
            {
                Cursor.Current = Cursors.Default;
                Application.UseWaitCursor = false;
            }
            return list;
        }

        void ShowSearchedFilters()
        {
            string old_selection = filtertree.SelectedNode == null ? null : filtertree.SelectedNode.Text;
            IEnumerable<FilterProps> list;

            if (checkBoxAllCats.Checked)
            {
                if (all_filters == null) all_filters = GetAllFilters();
                list = SearchFilters(textBoxSearch.Text, all_filters);
            } else            
                list = SearchFilters(textBoxSearch.Text, current_category_filters);            

            FillFilterTree(filtertree, list);
            if (old_selection != null)
                foreach (TreeNode nd in filtertree.Nodes)
                    if (nd.Text == old_selection)
                    {
                        filtertree.SelectedNode = nd;
                        break;
                    }            
        }

        static public void FillFilterTree(TreeView tree, IEnumerable<FilterProps> flist)
        {
            tree.BeginUpdate();
            tree.Nodes.Clear();
            foreach (FilterProps fp in flist)
            {
                TreeNode nd = tree.Nodes.Add(fp.Name);
                nd.Tag = fp;
                nd.Nodes.Add(fp.DisplayName);
                nd.Nodes.Add(fp.CLSID);
                nd.Nodes.Add(fp.MakeFileName());
            }
            tree.EndUpdate();
        }

        static IEnumerable<FilterProps> SearchFilters(string namepart, IEnumerable<FilterProps> list)
        {
            string part = namepart.ToLowerInvariant();
            foreach (FilterProps fp in list)
                if (fp.Name!=null && fp.Name.ToLowerInvariant().Contains(part)) yield return fp;
        }

        static public List<FilterProps> GetFiltersFromEnum(IEnumMoniker emon, Guid cg)
        {
            List<FilterProps> list = new List<FilterProps>();
            if (emon != null)
            {
                IMoniker[] mon = new IMoniker[1];
                while (0 == emon.Next(1, mon, IntPtr.Zero))
                {
                    object bagObj = null;
                    Guid propertyBagId = typeof(IPropertyBag).GUID;
                    mon[0].BindToStorage(null, null, ref propertyBagId, out bagObj);
                    IPropertyBag bag = (IPropertyBag)bagObj;
                    object nameObj;
                    bag.Read("FriendlyName", out nameObj, null);
                    string name = nameObj as string;

                    string dispname;
                    mon[0].GetDisplayName(null, null, out dispname);
                    bag.Read("CLSID", out nameObj, null);
                    string clsid = nameObj as string;
                    if (clsid == null && dispname.Contains(":dmo:"))
                    {
                        int st = dispname.IndexOf('{');
                        int ed = dispname.IndexOf('}');
                        if (st >= 0 && ed >= 0)
                        {
                            clsid = dispname.Substring(st, ed - st + 1);
                        }
                    }
                    if (clsid != null && !friendlyNames.ContainsKey(clsid))
                        friendlyNames.Add(clsid, name);

                    FilterProps fp = new FilterProps(name, dispname, clsid, Graph.GuidToString(cg));
                    list.Add(fp);

                    Marshal.ReleaseComObject(bagObj);
                    Marshal.ReleaseComObject(mon[0]);
                }
            }
            return list;
        }

        public void RefreshTree()
        {
            catcombo_SelectedIndexChanged(null, null);
        }

        private void addbtn_Click(object sender, EventArgs e)
        {
            using (var fd = new OpenFileDialog())
            {
                fd.Filter = "DLL, AX, OCX files|*.dll;*.ax;*.ocx|All files (*.*)|*.*";
                if (fd.ShowDialog() != DialogResult.OK)
                    return;
                var si = new ProcessStartInfo("regsvr32.exe", "\"" + fd.FileName + "\"");
                si.Verb = "runas";
                Process.Start(si);
            }
            Thread.Sleep(300);
            RefreshTree();
        }

        private void filtertree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode nd = filtertree.SelectedNode;
            while (nd.Parent != null && nd.Tag == null)
                nd = nd.Parent;
            FilterProps fp = (FilterProps)nd.Tag;
            if (fp != null)
                fp.Prepare();
            Program.mainform.propform.SetObject(fp);
        }

        private void filtertree_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            TreeNode nd = filtertree.SelectedNode;
            if (nd == null)
                return;
            while (nd.Parent != null && nd.Tag == null)
                nd = nd.Parent;
            FilterProps fp = (FilterProps)nd.Tag;
            if (fp != null)
            {
                string catname = catcombo.SelectedItem.ToString();
                if (catguids[catname] == FilterCategory.ActiveMovieCategories)
                    SelectCategory(fp.Name);
                else
                {
                    GraphForm gf = Program.mainform.ActiveGraphForm;
                    if (gf != null)
                        gf.AddFilter(fp);
                }
            }
        }

        void SelectCategory(string cname)
        {
            foreach (object item in catcombo.Items)
                if (item.ToString() == cname)
                {
                    catcombo.SelectedItem = item;
                    break;
                }
        }

        private void OnEdit(object sender, EventArgs e)
        {
            TreeNode nd = filtertree.SelectedNode;
            if (nd == null)
                return;
            while (nd.Parent != null && nd.Tag == null)
                nd = nd.Parent;
            FilterProps fp = (FilterProps)nd.Tag;
            if (fp != null)
            {
                EditFilterForm ef = new EditFilterForm(fp);
                ef.MdiParent = MdiParent;
                ef.Show();
            }
        }

        private void OnRefreshButton(object sender, EventArgs e)
        {
            RefreshCategories();
        }

        private void OnSearchTextChanged(object sender, EventArgs e)
        {
            ShowSearchedFilters();
        }

        private void btnClearSearch_Click(object sender, EventArgs e)
        {
            textBoxSearch.Clear();
        }

        private void OnAllCatsChecked(object sender, EventArgs e)
        {
            int row0height = 22;
            if (checkBoxAllCats.Checked)
            {
                catcombo.Visible = false;
                tableLayoutPanel1.RowStyles[0].Height = 0;
                Cursor.Position = new System.Drawing.Point(Cursor.Position.X, Cursor.Position.Y - row0height);
            }
            else
            {
                tableLayoutPanel1.RowStyles[0].Height = row0height;
                catcombo.Visible = true;
                Cursor.Position = new System.Drawing.Point(Cursor.Position.X, Cursor.Position.Y + row0height);
            }
            ShowSearchedFilters();
        }

        private void OnLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            List<string> catlist = new List<string>();
            catlist.AddRange(catnames.Values);
            catlist.Remove(catnames[Graph.GuidToString(FilterCategory.ActiveMovieCategories)]);
            catlist.Sort();
            SearchFilterForm.ShowSearchFilterForm(catlist, checkBoxAllCats.Checked, 
                textBoxSearch.Text, catcombo.SelectedItem.ToString(), MdiParent);
        }

        private void OnItemDrag(object sender, ItemDragEventArgs e)
        {
            DoDragDrop(e.Item, DragDropEffects.Copy);
        }

        private void filtertree_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Y < 12 || e.Y > filtertree.Height - 12)
                Cursor.Current = Cursors.HSplit;

            if (tree_resizing_bottom)
            {
                if (e.Y - tree_resizing_bottom_start > 1 && tableLayoutPanel1.RowStyles[4].Height > 1)
                {
                    tableLayoutPanel1.RowStyles[4].Height = 0;
                    tree_resizing_bottom = false;
                }
                if (tree_resizing_bottom_start - e.Y > 1 && tableLayoutPanel1.RowStyles[4].Height < 1)
                {
                    tableLayoutPanel1.RowStyles[4].Height = 25;
                    tree_resizing_bottom = false;
                }
            }

            if (tree_resizing_top)
            {
                if (tree_resizing_top_start - e.Y > 1) //moving up
                {
                    if (tableLayoutPanel1.RowStyles[2].Height > 1)
                        tableLayoutPanel1.RowStyles[2].Height = 0;
                    else
                        if (tableLayoutPanel1.RowStyles[1].Height > 1)
                            tableLayoutPanel1.RowStyles[1].Height = 0;
                    tree_resizing_top = false;
                }
                if (e.Y - tree_resizing_top_start > 1) // moving down
                {
                    if (tableLayoutPanel1.RowStyles[1].Height < 1)
                        tableLayoutPanel1.RowStyles[1].Height = 23;
                    else
                        if (tableLayoutPanel1.RowStyles[2].Height < 1)
                            tableLayoutPanel1.RowStyles[2].Height = 18;
                    tree_resizing_top = false;
                }
            }

            Program.mainform.SetHint("Double click or drag a filter to add to graph");
        }

        bool tree_resizing_bottom = false;
        int tree_resizing_bottom_start;
        bool tree_resizing_top = false;
        int tree_resizing_top_start;

        private void filtertree_MouseDown(object sender, MouseEventArgs e)
        {           
            if (e.Y > filtertree.Height - 12)
            {
                tree_resizing_bottom = true;
                tree_resizing_bottom_start = e.Y;
            }
            if (e.Y < 12)
            {
                tree_resizing_top = true;
                tree_resizing_top_start = e.Y;
            }
        }

        private void filtertree_MouseLeave(object sender, EventArgs e)
        {
            tree_resizing_bottom = false;
            tree_resizing_top = false;
            Program.mainform.SetHint("");
        }

        private void filtertree_MouseUp(object sender, MouseEventArgs e)
        {
            tree_resizing_bottom = false;
            tree_resizing_top = false;
        }

        public void FindFilterInList(string dispname)
        {
            textBoxSearch.Text = "";
            if (checkBoxAllCats.Checked)
                SelectNode(dispname);
            else
            {
                if (!SelectNode(dispname))
                {
                    if (all_filters == null)
                        all_filters = GetAllFilters();
                    foreach(FilterProps fp in all_filters)
                        if (fp.DisplayName == dispname)
                        {
                            SelectCategory(fp.CategoryName);
                            break;
                        }
                    SelectNode(dispname);                    
                }
            }
        }

        bool SelectNode(string dispname)
        {
            foreach (TreeNode nd in filtertree.Nodes)
            {
                FilterProps fp = (FilterProps)nd.Tag;
                if (fp.DisplayName == dispname)
                {
                    filtertree.SelectedNode = nd;
                    return true;
                }
            }
            return false;
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case GraphForm.WM_NCLBUTTONDBLCLK:                    
                    return;                    
            }
            base.WndProc(ref m);
        }

    }//Filterz class

    /*
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("6B652FFF-11FE-4fce-92AD-0266B5D7C78F"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ISampleGrabber
    {
        [PreserveSig]
        int SetOneShot(
            [In, MarshalAs(UnmanagedType.Bool)] bool OneShot);

        [PreserveSig]
        int SetMediaType(
            [In, MarshalAs(UnmanagedType.LPStruct)] AMMediaType pmt);

        [PreserveSig]
        int GetConnectedMediaType(
            [Out, MarshalAs(UnmanagedType.LPStruct)] AMMediaType pmt);

        [PreserveSig]
        int SetBufferSamples(
            [In, MarshalAs(UnmanagedType.Bool)] bool BufferThem);

        [PreserveSig]
        int GetCurrentBuffer(ref int pBufferSize, IntPtr pBuffer);

        [PreserveSig]
        int GetCurrentSample(out IMediaSample ppSample);

        [PreserveSig]
        int SetCallback(ISampleGrabberCB pCallback, int WhichMethodToCallback);
    }
     
     */

    /*
     cpp:
        [uuid("97f7c4d4-547b-4a5f-8332-536430ad2e4d")]
        interface IAMFilterData : public IUnknown
        {
	        STDMETHOD (ParseFilterData) (BYTE* rgbFilterData, ULONG cb, BYTE** prgbRegFilter2) PURE;
	        STDMETHOD (CreateFilterData) (REGFILTER2* prf2, BYTE** prgbFilterData, ULONG* pcb) PURE;
        };     
     * http://www.koders.com/cpp/fidBDB7054598ED2DE3EDC3700536B7CB0E773EB06B.aspx?s=sort
     */

    /*[ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("97f7c4d4-547b-4a5f-8332-536430ad2e4d"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IAMFilterData 
    {
        [PreserveSig]
        int ParseFilterData([MarshalAs(UnmanagedType.LPArray)] byte[] rgbFilterData, int cb, out IntPtr prgbRegFilter2);
        [PreserveSig]
    	int CreateFilterData (IntPtr prf2, out IntPtr prgbFilterData, IntPtr pcb);
    };*/

}