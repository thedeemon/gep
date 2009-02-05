using System;
using System.Collections.Generic;
using System.Windows.Forms;
using DirectShowLib;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Diagnostics;
using System.Threading;

namespace gep
{
    partial class Filterz : Form
    {
        public Filterz()
        {
            InitializeComponent();
            filtertree.Sorted = true;
        }

        private Dictionary<string, string> catnames = new Dictionary<string,string>(); //guid => name
        private Dictionary<string, Guid> catguids = new Dictionary<string, Guid>();
        public static Dictionary<string, string> friendlyNames = new Dictionary<string, string>(); //guid=>name
        public static RegistryChecker rch = new RegistryChecker();

        /*private string Catname(Guid g, string dft)
        {
            string sg = Graph.GuidToString(g);
            return catnames.ContainsKey(sg) ? catnames[sg] : dft;
        }*/

        private void Addcat(Guid g, string dft_name)
        {
            string sg = Graph.GuidToString(g);
            if (!catnames.ContainsKey(sg))
                catnames.Add(sg, dft_name);

            /*string name = Catname(g, dft);
            catguids.Add(name, g);
            catcombo.Items.Add(name);
            return name;*/
        }

        public void RefreshCategories()
        {
            object old_selection = catcombo.SelectedItem;

            //clear values
            catcombo.Items.Clear();
            catnames.Clear();
            catguids.Clear();

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
                    catnames.Add(sg, name);

                    Marshal.ReleaseComObject(bagObj);
                    Marshal.ReleaseComObject(mon[0]);
                }
            }
            
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

            //fill combo box and remember guids
            foreach (KeyValuePair<string, string> p in catnames)
            {
                Guid guid = new Guid(p.Key);
                catguids.Add(p.Value, guid);
                catcombo.Items.Add(p.Value);
            }

            RegistryChecker rch = new RegistryChecker();
            rch.CalcDays();
            catcombo.SelectedItem = old_selection ?? catnames[Graph.GuidToString(FilterCategory.LegacyAmFilterCategory)];
        }

        private void Filterz_Load(object sender, EventArgs e)
        {
            // Create the ToolTip and associate with the Form container.
            ToolTip toolTip = new ToolTip();

            // Set up the delays for the ToolTip.
            toolTip.AutoPopDelay = 5000;
            toolTip.InitialDelay = 1000;
            toolTip.ReshowDelay = 500;
            // Force the ToolTip text to be displayed whether or not the form is active.
            toolTip.ShowAlways = true;

            // Set up the ToolTip text for the Button and Checkbox.
            toolTip.SetToolTip(btnAdd, "Register a filter from file");
            toolTip.SetToolTip(btnEdit, "Change filter's merit or unregister filter");
            toolTip.SetToolTip(btnRefresh, "Refresh list of categories and filters");

            RefreshCategories();            
            filtertree.Focus();
        }
        
        private void catcombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            ICreateDevEnum devenum = new CreateDevEnum() as ICreateDevEnum;
            IEnumMoniker emon;
            string catname = catcombo.Items[catcombo.SelectedIndex].ToString();
            Guid cg = catguids[catname];
            int hr = devenum.CreateClassEnumerator(cg, out emon, 0);
            //filtertree.Nodes.Add("_category").Nodes.Add(Graph.GuidToString(cg));
            if (hr < 0) return;
            string old_selection = filtertree.SelectedNode==null ? null : filtertree.SelectedNode.Text;
            BuildFilterTree(emon, filtertree, cg);
            if (old_selection != null) 
                foreach (TreeNode nd in filtertree.Nodes)
                    if (nd.Text == old_selection)
                    {
                        filtertree.SelectedNode = nd;
                        break;
                    }            
        }

        public static void BuildFilterTree(IEnumMoniker emon, TreeView tree, Guid cg)
        {
            tree.Nodes.Clear();
            tree.BeginUpdate();
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
                    TreeNode nd = tree.Nodes.Add(name);

                    string dispname;
                    mon[0].GetDisplayName(null, null, out dispname);
                    nd.Nodes.Add(dispname);
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
                    nd.Nodes.Add(clsid);
                    if (clsid != null && !friendlyNames.ContainsKey(clsid))
                        friendlyNames.Add(clsid, name);

                    FilterProps fp = new FilterProps(name, dispname, clsid, Graph.GuidToString(cg));
                    nd.Tag = fp;
                    string fname = fp.MakeFileName();
                    nd.Nodes.Add(fname);

                    Marshal.ReleaseComObject(bagObj);
                    Marshal.ReleaseComObject(mon[0]);
                }
            }
            tree.EndUpdate();
        }

        public void RefreshTree()
        {
            catcombo_SelectedIndexChanged(null, null);
        }

        private void addbtn_Click(object sender, EventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.Filter = "DLL, AX, OCX files|*.dll;*.ax;*.ocx|All files (*.*)|*.*";
            if (fd.ShowDialog() != DialogResult.OK)
                return;
            Process.Start("regsvr32.exe", "\""+fd.FileName+"\"");
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
                GraphForm gf = Program.mainform.ActiveGraphForm;
                if (gf != null)
                    gf.AddFilter(fp);
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
    }//class

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