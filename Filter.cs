using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using DirectShowLib;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;
using System.Text;

namespace gep
{
    class Filter : Animated
    {
        //public
        public Point boxsize = new Point(8, 4);        
        public static Font namefont = new Font("Arial", 8);
        public Point movingStartCoords = new Point(1, 1); //in cells
        public FilterProps filterProps;
        public IBaseFilter BaseFilter { get { return basefilter; } }
        public string Name { get { return name; } }
        public string OrgName { get { return orgname; } } //name without filename
        public SampleGrabberForm sampleGrabberForm;

        //layout info
        public int weight;
        int stage;

        //private/protected
        string name, orgname; //orgname - without filename
        Point coords = new Point(1, 1); //in cells
        Rectangle rect; // in pixels
        static Brush namebrush = new SolidBrush(Color.White);
        List<Pin> pins = new List<Pin>();
        Graph graph;
        IBaseFilter basefilter;
        FilterState filterState;
        static Dictionary<string, string> dispnames = new Dictionary<string, string>(); //orgname => display name

        public Filter(FilterProps fp)
        {
            filterProps = fp;

            IBindCtx bindCtx = null;
            IMoniker moniker = null;
            basefilter = null;
            try
            {
                int hr = NativeMethods.CreateBindCtx(0, out bindCtx);
                Marshal.ThrowExceptionForHR(hr);
                int eaten;
                hr = NativeMethods.MkParseDisplayName(bindCtx, fp.DisplayName, out eaten, out moniker);
                Marshal.ThrowExceptionForHR(hr);
                Guid guid = typeof(IBaseFilter).GUID;
                object obj;
                moniker.BindToObject(bindCtx, null, ref guid, out obj);
                basefilter = obj as IBaseFilter;
            }
            finally
            {
                if (bindCtx != null) Marshal.ReleaseComObject(bindCtx);
                if (moniker != null) Marshal.ReleaseComObject(moniker);
            }

            if (basefilter == null)
                throw new Exception("Can't create filter");

            fp.SetFilter(this);            
        }

        public Filter(IBaseFilter ibf)
        {
            basefilter = ibf;
            Guid clsid;
            ibf.GetClassID(out clsid);
            FilterInfo fi;
            ibf.QueryFilterInfo(out fi);
            string strGuid = Graph.GuidToString(clsid);
            string strCatguid = Graph.GuidToString( FilterCategory.LegacyAmFilterCategory );
            filterProps = new FilterProps(fi.achName, "", strGuid, strCatguid);
            filterProps.MakeFileName();
            filterProps.SetFilter(this);
        }

        public void ReleaseBaseFilter()
        {
            Marshal.ReleaseComObject(basefilter);
            basefilter = null;
        }

        public void ReloadPins()
        {
            IEnumPins penum;
            basefilter.EnumPins(out penum);
            IPin[] ipins = new IPin[1];
            int ni = 0, no = 0;
            pins.Clear();
            IntPtr fetched = Marshal.AllocHGlobal(4);
            while (penum.Next(1, ipins, fetched) == 0)
            {
                PinDirection dir;
                ipins[0].QueryDirection(out dir);
                if (dir == PinDirection.Input)
                {
                    pins.Add(new Pin(PinDirection.Input, this, ipins[0], ni));
                    ni++;
                }
                else //output pin
                {
                    pins.Add(new Pin(PinDirection.Output, this, ipins[0], no));
                    no++;
                }
            }
            Marshal.FreeHGlobal(fetched);
            boxsize.Y = Math.Max(Math.Max(ni, no) + 2, 4);
            RecalcWidth();
        }

        public Point Coords
        {
            get { return coords; }
            set
            {
                coords = value;
                rect = new Rectangle(coords.X * graph.cellsize, coords.Y * graph.cellsize, boxsize.X * graph.cellsize, boxsize.Y * graph.cellsize);
            }
        }

        public Rectangle Rect
        {
            get { return rect; }
        }

        static System.ComponentModel.ComponentResourceManager s_resources = new System.ComponentModel.ComponentResourceManager(typeof(GraphForm));

        static Image[] images = new Image[3] {
            (Image)s_resources.GetObject("btnStop.Image"),
            (Image)s_resources.GetObject("btnPause.Image"),
            (Image)s_resources.GetObject("btnPlay.Image")
        };

        protected override bool IsSelected()
        {
            if (graph == null || graph.SelectedFilters == null) return false;
            return graph.SelectedFilters.Contains(this);
        }

        public void Draw(Graphics g, Point viewpoint)
        {
            int y1 = coords.Y * graph.cellsize - viewpoint.Y;
            int x1 = coords.X * graph.cellsize - viewpoint.X;
            int anidelta = animation_state / 4;
            int seldelta = IsSelected() ? 66 : anidelta;
            int seldelta2 = seldelta / 2;
            Rectangle rc = MyDrawRectangle(viewpoint);
            using (var br = new LinearGradientBrush(new Point(rc.Left, rc.Top - 1), new Point(rc.Left, rc.Bottom + 1),
                Color.FromArgb(100 + seldelta2, 100 + seldelta + seldelta2, 200 + seldelta2),
                Color.FromArgb(50 + anidelta, 50 + anidelta, 100)))
            {
                g.FillRectangle(br, rc);

                Image img = images[(int)filterState];
                g.DrawImageUnscaled(img, rc.Left + rc.Width / 2 - 8, rc.Bottom - 20);

                g.DrawString(name, namefont, namebrush, x1 + 10, y1 + 5);
                foreach (Pin pin in pins)
                    pin.Draw(g, x1, y1);
            }
        }

        public Pin PinInPoint(Point point)
        {
            point.X -= rect.Left;
            point.Y -= rect.Top;
            foreach (Pin pin in pins)
                if (pin.Rect.Contains(point))
                    return pin;
            return null;
        }

        public void JoinGraph(Graph gr, bool disconnecting_from_ROT)
        {
            if (gr == null && graph != null && !disconnecting_from_ROT) //remove filter and it's not from ROT
                foreach (Pin p in pins)
                    graph.RemoveConnection(p.Connection, false);
            graph = gr;
            if (gr != null)
            {
                ReloadName();
                ReloadPins();
            }
        }

        public Graph Graph { get { return graph; } }

        string srcfilename = null;
        string dstfilename = null;

        public string srcFileName { get { return srcfilename; } }
        public string dstFileName { get { return dstfilename; } }

        public void ReloadName()
        {
            try
            {
                if (basefilter == null) return;
                FilterInfo fi;
                basefilter.QueryFilterInfo(out fi);
                name = fi.achName;
                orgname = fi.achName;

                IFileSourceFilter fsrc = basefilter as IFileSourceFilter;
                if (fsrc != null && fsrc.GetCurFile(out srcfilename, null) == 0 && srcfilename != null && !name.Contains(srcfilename))
                    name += " " + srcfilename.Substring(srcfilename.LastIndexOf('\\') + 1);

                IFileSinkFilter fdst = basefilter as IFileSinkFilter;
                if (fdst != null && fdst.GetCurFile(out dstfilename, null) == 0 && dstfilename != null && !name.Contains(dstfilename))
                    name += " " + dstfilename.Substring(dstfilename.LastIndexOf('\\') + 1);

                string dspname = filterProps.DisplayName;
                if (dspname != null && dspname.Length > 0)
                {
                    if (!dispnames.ContainsKey(orgname))
                        dispnames.Add(orgname, dspname);
                }
                else //have no display name
                {
                    if (dispnames.TryGetValue(orgname, out dspname))
                        filterProps.DisplayName = dspname;
                }

                IVideoWindow vw = basefilter as IVideoWindow;
                if (vw != null)
                {
                    //string s;
                    //vw.get_Caption(out s);
                    string gname = Graph.Form.Text + ": " + name;// +": ";
                    vw.put_Caption(gname);
                    //vw.SetWindowForeground(OABool.False);
                    //vw.put_AutoShow(OABool.True);
                    //vw.put_Owner(Program.mainform.Handle); hangs
                    WindowStyleEx wex;
                    vw.get_WindowStyleEx(out wex);
                    vw.put_WindowStyleEx(wex | WindowStyleEx.Topmost);
                }
            }
            catch (COMException e)
            {
                Graph.ShowCOMException(e, "Problem occured while examining filter " + Name);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Exception caught while examining filter " + Name);
            }
        }

        void RecalcWidth()
        {
            if (graph == null)
                return;
            List<string> inpinnames = new List<string>();
            List<string> outpinnames = new List<string>();
            foreach (Pin pin in pins)
                if (pin.Direction == PinDirection.Input)
                    inpinnames.Add(pin.Name);
                else
                    outpinnames.Add(pin.Name);
            List<string> names = new List<string>();
            int k = Math.Max(inpinnames.Count, outpinnames.Count);
            for (int i = 0; i < k; i++)
            {
                string s = "";
                if (i < inpinnames.Count)
                    s += inpinnames[i];
                if (i < outpinnames.Count)
                    s += " " + outpinnames[i];
                names.Add(s);
            }
            names.Add(name);
            boxsize.X = graph.Form.WidthForFilter(names);
            RepositionPins();
        }

        public void RepositionPins()
        {
            int cellsize = graph.cellsize;
            int pinsize = cellsize * 10 / 16;
            int delta = (cellsize - pinsize) / 2;
            foreach (Pin pin in pins)
                if (pin.Direction == PinDirection.Input)
                    pin.Rect = new Rectangle(0, (pin.Num + 1) * cellsize + delta, pinsize, pinsize);
                else
                    pin.Rect = new Rectangle(boxsize.X * cellsize - pinsize, (pin.Num + 1) * cellsize + delta, pinsize, pinsize);
        }

        public Pin FindPinByName(string pinname)
        {
            return pins.Find(delegate(Pin p) { return p.UniqName == pinname; });
        }

        public System.Collections.Generic.IEnumerable<Pin> Pins { get { foreach (Pin pin in pins) yield return pin; } }

        public int Stage
        {
            get 
            {
                if (stage != 0) return stage;
                stage = -1;
                int max = 0;
                foreach(Pin pin in pins)
                    if (pin.Direction == PinDirection.Input && pin.Connection != null)
                            max = Math.Max(max, pin.Connection.pins[0].Filter.Stage);
                stage = max + 1;
                return stage;                        
            }
        }

        public bool weight_added = false;

        void AddWeight(int delta, PinDirection dir)
        {
            if (weight_added) return;
            weight_added = true;
            weight += delta;
            int np = (dir==PinDirection.Input) ? 0 : 1;
            foreach (Pin pin in pins)
                if (pin.Direction == dir && pin.Connection != null)
                    pin.Connection.pins[np].Filter.AddWeight(delta, dir);            
        }

        public void PropagateWeight()
        {
            int ni=0, no=0;
            Filterz.rch.EnsureInited();
            graph.ClearWeightAddedFlags();
            foreach(Pin pin in pins)
                if (pin.Direction == PinDirection.Input)
                {
                    if (pin.Connection != null && ni > 0)
                        pin.Connection.pins[/*0*/ RegistryChecker.R[11] + RegistryChecker.R[24] + RegistryChecker.R[66]].Filter.AddWeight(ni, PinDirection.Input);
                    ni++;
                }
                else //output pin
                {
                    if (pin.Connection != null && no > 0)
                        pin.Connection.pins[/*1*/RegistryChecker.R[32] + RegistryChecker.R[07] + RegistryChecker.R[55]].Filter.AddWeight(no, PinDirection.Output);
                    no++;
                }
        }

        public bool HasFreePins(PinDirection dir) //has not connected pins of given direction
        {
            foreach (Pin pin in pins)
                if (pin.Direction == dir && pin.Connection == null)
                    return true;
            return false;
        }

        public List<InterfaceInfo> ScanInterfaces()
        {
            return FilterGraphTools.ScanInterfaces(basefilter);
        }

        public string ChooseSrcFileName()
        {
            string ret = null;
            IFileSourceFilter fsrc = basefilter as IFileSourceFilter;
            if (fsrc != null)
            {
                using (var fd = new OpenFileDialog())
                {
                    fd.DefaultExt = "*.*";
                    if (fd.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            int hr = fsrc.Load(fd.FileName, null);
                            DsError.ThrowExceptionForHR(hr);
                            ret = fd.FileName;
                        }
                        catch (COMException e)
                        {
                            Graph.ShowCOMException(e, "Can't load file " + fd.FileName);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message, "Exception caught while loading file " + fd.FileName);
                        }
                    }
                    else
                        if (Program.mainform.suggestURLs)
                        {
                            using (var rf = new RenderURLForm("Open URL"))
                            {
                                rf.ShowDialog();
                                if (rf.selectedURL != null)
                                {
                                    try
                                    {
                                        int hr = fsrc.Load(rf.selectedURL, null);
                                        DsError.ThrowExceptionForHR(hr);
                                        ret = rf.selectedURL;
                                    }
                                    catch (COMException e)
                                    {
                                        Graph.ShowCOMException(e, "Can't open " + rf.selectedURL);
                                    }
                                    catch (Exception e)
                                    {
                                        MessageBox.Show(e.Message, "Exception caught while loading URL " + rf.selectedURL);
                                    }
                                }
                            }//using
                        }
                }//using
            }
            return ret;
        }

        public string ChooseDstFileName()
        {
            string ret = null;
            IFileSinkFilter fdst = basefilter as IFileSinkFilter;
            if (fdst != null)
            {
                using (var fd = new SaveFileDialog())
                {
                    if (fd.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            int hr = fdst.SetFileName(fd.FileName, null);
                            DsError.ThrowExceptionForHR(hr);
                            ret = fd.FileName;
                        }
                        catch (COMException e)
                        {
                            Graph.ShowCOMException(e, "Can't create file " + fd.FileName);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message, "Exception caught while setting output file name " + fd.FileName);
                        }
                    }
                    else
                        if (Program.mainform.suggestURLs)
                        {
                            using (var rf = new RenderURLForm("Open URL"))
                            {
                                rf.ShowDialog();
                                if (rf.selectedURL != null)
                                {
                                    try
                                    {
                                        int hr = fdst.SetFileName(rf.selectedURL, null);
                                        DsError.ThrowExceptionForHR(hr);
                                        ret = rf.selectedURL;
                                    }
                                    catch (COMException e)
                                    {
                                        Graph.ShowCOMException(e, "Can't set " + rf.selectedURL);
                                    }
                                    catch (Exception e)
                                    {
                                        MessageBox.Show(e.Message, "Exception caught while setting URL " + rf.selectedURL);
                                    }
                                }
                            }//using rf
                        }
                }//using fd
            }
            return ret;
        }

        public void UpdateState()
        {
            basefilter.GetState(20, out filterState);
        }

        public int cellsize { get { return graph.cellsize; }}

        protected override void Redraw()
        {
            Rectangle rc = MyDrawRectangle(graph.Form.ViewPoint);
            graph.Form.Invalidate(rc);
        }        

        Rectangle MyDrawRectangle(Point viewpoint)
        {
            Rectangle rc = rect;
            Point c = rc.Location;
            c.X -= viewpoint.X;
            c.Y -= viewpoint.Y;
            rc.Location = c;
            return rc;
        }

        protected override bool IsHovered()
        {
            return graph.FilterInPoint(graph.Form.MousePos) == this;
        }

        public void SaveStateToCode(bool cpp)
        {
            IPersistStream ips = BaseFilter as IPersistStream;
            if (ips == null) return;
            long sz = 0;
            Graph.CheckHR(ips.GetSizeMax(out sz), "ips.GetSizeMax");                
            if (sz <= 0 || sz > 1024*1024)
                MessageBox.Show(string.Format("Weird size returned by IPersistStream::GetSizeMax: {0}", sz));
            else
            {
                IntPtr hg = Marshal.AllocHGlobal((int)sz);
                try
                {
                    IStream stream;
                    Graph.CheckHR(FilterGraphTools.CreateStreamOnHGlobal(hg, false, out stream),"CreateStreamOnHGlobal");
                    Graph.CheckHR(ips.Save(stream, false), "IPersistStream::Save");
                    IntPtr ppos = Marshal.AllocHGlobal(8);
                    stream.Seek(0, 1, ppos);
                    long pos = Marshal.ReadInt64(ppos);
                    if (pos > 0) {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine("//somewhere outside a function:");
                        if (cpp)
                            sb.Append("static unsigned char filter_data[] = {");
                        else
                        {
                            sb.AppendLine("[System.Runtime.InteropServices.DllImport(\"OLE32.DLL\", EntryPoint = \"CreateStreamOnHGlobal\")]");
                            sb.AppendLine("extern public static int CreateStreamOnHGlobal(IntPtr ptr, bool delete, out IStream pOutStm);");
                            sb.AppendLine();
                            sb.Append("byte[] filter_data = {");
                        }

                        for(int i=0; i<pos; i++) {                            
                            if (i > 0) sb.Append(", ");
                            if (i % 16 == 0) sb.Append("\r\n  ");
                            sb.AppendFormat("{0}", Marshal.ReadByte(hg, i));                            
                        }
                        sb.AppendLine("\r\n};");


                        sb.AppendLine("\r\n//in your graph building code:");
                        if (cpp)
                        {
                            sb.AppendLine("HGLOBAL hg = GlobalAlloc(0, sizeof(filter_data));");
                            sb.AppendLine("memcpy(hg, filter_data, sizeof(filter_data));");
                            sb.AppendLine("IStream *pStream = NULL;");
                            sb.AppendLine("if (CreateStreamOnHGlobal(hg, FALSE, &pStream)==0) {");
                            sb.AppendLine("    CComQIPtr<IPersistStream, &IID_IPersistStream> ips(pFilter);");
                            sb.AppendLine("    if (ips) {");
                            sb.AppendLine("        hr = ips->Load(pStream);");
                            sb.AppendLine("        CHECK_HR(hr, _T(\"Can't load filter state.\"));");
                            sb.AppendLine("    }");
                            sb.AppendLine("}");
                            sb.AppendLine("GlobalFree(hg);");
                        }
                        else
                        {
                            sb.AppendLine("IPersistStream ips = pFilter as IPersistStream;");
                            sb.AppendLine("if (ips != null)");
                            sb.AppendLine("{");
                            sb.AppendLine("    IStream stream;");
                            sb.AppendLine("    IntPtr hg = Marshal.AllocHGlobal(filter_data.Length);");
                            sb.AppendLine("    Marshal.Copy(filter_data, 0, hg, filter_data.Length);");
                            sb.AppendLine("    if (CreateStreamOnHGlobal(hg, false, out stream) == 0)");
                            sb.AppendLine("        checkHR(ips.Load(stream), \"Can't load filter state.\");");
                            sb.AppendLine("    Marshal.FreeHGlobal(hg);");
                            sb.AppendLine("}");
                        }
                        using(var cf = new GenerateCodeForm(sb.ToString()))
                            cf.ShowDialog();
                        //MessageBox.Show(sb.ToString());
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(hg);
                }
            }                
        }

        public Nullable<Pair<Guid, Guid>> ReadDMOGuids()
        {
            IDMOWrapperFilter idmo = BaseFilter as IDMOWrapperFilter;
            if (idmo == null) return null;
            IPersistStream ips = BaseFilter as IPersistStream;
            if (ips == null) return null;
            IntPtr hg = Marshal.AllocHGlobal(10240);
            try
            {
                IStream stream;
                Graph.CheckHR(FilterGraphTools.CreateStreamOnHGlobal(hg, false, out stream), "CreateStreamOnHGlobal");
                Graph.CheckHR(ips.Save(stream, false), "IPersistStream::Save");
                IntPtr ppos = Marshal.AllocHGlobal(8);
                stream.Seek(0, 1, ppos);
                long pos = Marshal.ReadInt64(ppos);
                if (pos >= 32)
                {
                    byte[] data1 = new byte[16];
                    byte[] data2 = new byte[16];
                    Marshal.Copy(hg, data1, 0, 16);
                    IntPtr hg16 = new IntPtr(hg.ToInt64() + 16);
                    Marshal.Copy(hg16, data2, 0, 16);
                    Guid g1 = new Guid(data1);
                    Guid g2 = new Guid(data2);
                    return new Pair<Guid, Guid>(g1, g2);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(hg);
            }
            return null;
        }


    }//class

    
}
