using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DirectShowLib;
using System.Runtime.InteropServices;

namespace gep
{
    partial class GraphForm : Form
    {
        Graph graph;
        string savedFileName;
        static int ngraphs = 0;
        int toolStripHeight;
        Timer timer = new Timer();
        public static int slider_start = 330;
        

        public GraphForm()
        {
            graph = new Graph(this);
            Init();
        }

        public GraphForm(IGraphBuilder gb)           
        {
            graph = new Graph(this, gb);
            Init();
            RecalcScrolls();
            //Invalidate();
        }

        private void Init()
        {
            InitializeComponent();
            ngraphs++;
            Text = "Graph " + ngraphs.ToString();
            RecalcScrolls();
            slider_start = zoomCombo.Bounds.Right + 12;
            showCode[0] = delegate(string code) { return new GenerateCodeGfxForm(code); };
            showCode[1] = delegate(string code) { return new GenerateCodeForm(code); };
        }
         
        public bool IsFromRot { get { return graph.IsFromRot; } }

        public bool UseClock
        {
            get { return graph.UseClock; }
            set { graph.UseClock = value; }
        }

        private void GraphForm_Paint(object sender, PaintEventArgs e)
        {
            Point viewpoint = new Point(hScrollBar.Value, vScrollBar.Value - toolStripHeight);
            graph.Draw(e.Graphics, viewpoint);
            if (connectingPin != null && connectingPin.Connection==null)
            {
                Pen pen = otherPin==null ? Pens.Red : Pens.LightGreen;
                Point mstart = movingStart;
                mstart.X -= viewpoint.X;
                mstart.Y -= viewpoint.Y;
                Point mpos = mousepos;
                mpos.X -= viewpoint.X;
                mpos.Y -= viewpoint.Y;
                e.Graphics.DrawLine(pen, mstart, mpos);
            }
            if (selecting)
            {
                Point mstart = movingStart;
                mstart.X -= viewpoint.X;
                mstart.Y -= viewpoint.Y;
                Point mpos = mousepos;
                mpos.X -= viewpoint.X;
                mpos.Y -= viewpoint.Y;
                Rectangle rc = new Rectangle(Math.Min(mstart.X, mpos.X), Math.Min(mstart.Y, mpos.Y), Math.Abs(mstart.X - mpos.X), Math.Abs(mstart.Y - mpos.Y));
                e.Graphics.DrawRectangle(Pens.Cyan, rc);
            }
        }

        public int WidthForFilter(List<string> names)
        {
            int max = 0;
            using (Graphics g = CreateGraphics())
                foreach (string s in names)
                    max = Math.Max(max, g.MeasureString(s, Filter.namefont).ToSize().Width);
            int cellsize = (graph != null) ? graph.cellsize : 16;
            return Math.Max(max / cellsize + 2, 8);
        }

        public void AddFilter(FilterProps fp)
        {
            AddFilter(fp, null);
        }

        private void AddFilter(FilterProps fp, Point? desired_pos)
        {
            graph.AddFilter(fp, desired_pos);
            RecalcScrolls();
            Invalidate();
        }

        private void GraphForm_Activated(object sender, EventArgs e)
        {
            MainForm mf = (MainForm)MdiParent;
            mf.ActiveGraphForm = this;
            mf.Text = Text + " - GraphEditPlus";
        }

        public void RenderFile(object sender, EventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.DefaultExt = "*.*";
            if (fd.ShowDialog() != DialogResult.OK)
                return;
            RenderThisFile(fd.FileName);
        }

        public void RenderThisFile(string fname)
        {
            graph.RenderFile(fname);
            RecalcScrolls();
            Invalidate();
        }

        public void AddSourceFilter(object sender, EventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.DefaultExt = "*.*";
            if (fd.ShowDialog() != DialogResult.OK)
                return;
            graph.AddSourceFilter(fd.FileName);
            RecalcScrolls();
            Invalidate();
        }

        public EventLogForm eventlogform;

        public void ShowEventLog(object sender, EventArgs e)
        {
            if (eventlogform == null)
            {
                eventlogform = new EventLogForm(graph);
                eventlogform.MdiParent = MdiParent;
                eventlogform.Show();
            }
            else eventlogform.BringToFront();
        }

        private void RenderPin(object sender, EventArgs e)
        {
            if (connectingPin != null)
                graph.RenderPin(connectingPin);
            connectingPin = null;
            otherPin = null;
            RecalcScrolls();
            Invalidate();
        }

        public void RenderURL(string url)
        {
            graph.RenderURL(url);
            RecalcScrolls();
            Invalidate();
        }

        private void ShowPropertyPage(object sender, EventArgs e)
        {
            if (rightClickedFilter != null)
            {
                FilterGraphTools.ShowFilterPropertyPage(rightClickedFilter.BaseFilter, Handle);
                rightClickedFilter = null;
            }

            if (connectingPin != null)
            {
                connectingPin.ShowPropertyPage(Handle);
                connectingPin = null;
            }
        }

        private void ScanInterfaces(object sender, EventArgs e)
        {
            List<InterfaceInfo> lst = null;
            string name = null;
            if (rightClickedFilter != null)
            {
                lst = rightClickedFilter.ScanInterfaces();     
                name = rightClickedFilter.Name + " interfaces";
                rightClickedFilter = null;
            }
            
            if (connectingPin != null)
            {
                lst = connectingPin.ScanInterfaces();
                name = connectingPin.UniqName + " interfaces";
                connectingPin = null;
            }

            if (lst != null)
            {
                InterfacesListForm ilf = new InterfacesListForm();
                ilf.MdiParent = MdiParent;
                ilf.Text = name;
                ilf.SetList(lst);
                ilf.Show();
            }            
        }

        private void VfWConfig(object sender, EventArgs e)
        {
            if (rightClickedFilter != null)
                (rightClickedFilter.BaseFilter as IAMVfwCompressDialogs).ShowDialog(VfwCompressDialogs.Config, Handle);
            rightClickedFilter = null;
        }

        private void VfWAbout(object sender, EventArgs e)
        {
            if (rightClickedFilter != null)
                (rightClickedFilter.BaseFilter as IAMVfwCompressDialogs).ShowDialog(VfwCompressDialogs.About, Handle);
            rightClickedFilter = null;
        }

        private void ShowMatchingFilters(object sender, EventArgs e)
        {
            if (connectingPin != null)
            {
                MatchingFiltersForm mf = new MatchingFiltersForm(connectingPin);
                mf.MdiParent = MdiParent;
                mf.Show();
            }                
            connectingPin = null;
        }

        private void WatchSampleGrabber(object sender, EventArgs e)
        {
            if (rightClickedFilter != null)
            {
                if (rightClickedFilter.sampleGrabberForm == null)
                {
                    ISampleGrabber sg = rightClickedFilter.BaseFilter as ISampleGrabber;
                    if (sg != null)
                    {
                        SampleGrabberForm sf = new SampleGrabberForm(sg, rightClickedFilter);
                        sf.MdiParent = MdiParent;
                        sf.Show();
                    }
                }
                else
                    rightClickedFilter.sampleGrabberForm.BringToFront();
            }
            rightClickedFilter = null;
        }

        private void SetSGMediaType(object sender, EventArgs e)
        {
            if (rightClickedFilter != null)
            {
                ISampleGrabber sg = rightClickedFilter.BaseFilter as ISampleGrabber;
                if (sg != null)
                {
                    MediaTypeForm sf = new MediaTypeForm(sg);
                    sf.ShowDialog();
                }
            }
            rightClickedFilter = null;
        }

        private void AddToFavorites(object sender, EventArgs e)
        {
            if (rightClickedFilter != null)
                Program.mainform.AddToFavorites(rightClickedFilter.filterProps);
            rightClickedFilter = null;
        }

        private void ConfigStream(object sender, EventArgs e)
        {
            if (connectingPin != null)
            {
                IAMStreamConfig isc = connectingPin.IPin as IAMStreamConfig;
                if (isc != null)
                {
                    MediaTypeListForm f = new MediaTypeListForm(isc);
                    if (f.ShowDialog() == DialogResult.OK)
                        graph.PinSetFormat(connectingPin, f.selected_mt);
                }
            }
            connectingPin = null;
        }

        private void GetStreamCaps(object sender, EventArgs e)
        {
            try
            {
                if (connectingPin != null)
                    connectingPin.GetStreamCaps();
            }
            catch (COMException ex)
            {
                Graph.ShowCOMException(ex, "Error getting stream caps");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            connectingPin = null;
        }

        private void ShowAllocatorProperties(object sender, EventArgs e)
        {
            if (connectingPin != null)
            {
                IMemInputPin imip = connectingPin.IPin as IMemInputPin;
                if (imip != null)
                {
                    IMemAllocator ma = null;
                    imip.GetAllocator(out ma);
                    if (ma != null)
                    {
                        AllocatorProperties pr = new AllocatorProperties();
                        ma.GetProperties(pr);
                        Program.mainform.propform.SetObject(new FieldsToPropertiesProxyTypeDescriptor(pr));
                    }
                }
            }
            connectingPin = null;
        }

        private void ShowAllocatorRequirements(object sender, EventArgs e)
        {
            if (connectingPin != null)
            {
                IMemInputPin imip = connectingPin.IPin as IMemInputPin;
                if (imip != null)
                {
                    try
                    {
                        AllocatorProperties pr = new AllocatorProperties();
                        int hr = imip.GetAllocatorRequirements(out pr);
                        DsError.ThrowExceptionForHR(hr);
                        if (pr != null)
                            Program.mainform.propform.SetObject(new FieldsToPropertiesProxyTypeDescriptor(pr));
                    }
                    catch (COMException ex)
                    {
                        Graph.ShowCOMException(ex, "Can't get requirements");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Exception caught in IMemInputPin::GetAllocatorRequirements");
                    }
                }
            }
            connectingPin = null;
        }

        Filter movingFilter, rightClickedFilter;
        Point movingStart; //click point in pixels
        Pin connectingPin, otherPin;
        Point mousepos;
        bool selecting = false;

        private void OnLButtonDown()
        {
            movingFilter = null;
            connectingPin = null;
            Filter filter = graph.FilterInPoint(mousepos);
            if (filter != null)
            {
                Pin pin = filter.PinInPoint(mousepos);
                if (pin != null && pin.Connection == null) //pin clicked
                {
                    movingStart = mousepos;
                    connectingPin = pin;
                }
                else
                { //filter clicked
                    movingStart = mousepos;
                    movingFilter = filter;
                    movingFilter.movingStartCoords = movingFilter.Coords;
                    graph.PlaceFilter(movingFilter, false); //clear
                    if (ModifierKeys != Keys.Shift)
                    {
                        graph.ClearFiltersSelection();
                        Program.mainform.propform.SetObject(movingFilter.filterProps);
                    }
                    graph.SelectFilter(movingFilter, true);
                    Invalidate();
                }
            }
            else //no filter clicked
            {
                int con_id = graph.ownersmap[mousepos.X / graph.cellsize, mousepos.Y / graph.cellsize];
                graph.ClearFiltersSelection();
                if (con_id > 0)
                    graph.SelectConnection(con_id);
                else //click on empty place
                {
                    movingStart = mousepos;
                    selecting = true;
                }
                Invalidate();
            }
        }

        private void OnRButtonDown(Point eLocation)
        {
            Filter filter = graph.FilterInPoint(mousepos);
            if (filter != null)
            {
                Pin pin = filter.PinInPoint(mousepos);
                if (pin != null)
                {                    
                        movingStart = mousepos;
                        connectingPin = pin;                       
                }
                else //filter right click
                {
                    rightClickedFilter = filter;
                    ContextMenu menu = new ContextMenu();
                    if (FilterGraphTools.HasPropertyPages(filter.BaseFilter))
                        menu.MenuItems.Add("Property page", ShowPropertyPage);

                    IAMVfwCompressDialogs vfw = filter.BaseFilter as IAMVfwCompressDialogs;
                    if (vfw != null)
                    {
                        if (vfw.ShowDialog(VfwCompressDialogs.QueryConfig, Handle) == 0)
                            menu.MenuItems.Add("VfW compressor: Config", VfWConfig);
                        if (vfw.ShowDialog(VfwCompressDialogs.QueryAbout, Handle) == 0)
                            menu.MenuItems.Add("VfW compressor: About", VfWAbout);
                    }

                    if ((filter.BaseFilter as ISampleGrabber) != null)
                    {
                        menu.MenuItems.Add("Set media type", SetSGMediaType);
                        menu.MenuItems.Add("Watch grabbed samples", WatchSampleGrabber);
                    }

                    menu.MenuItems.Add("Scan interfaces", ScanInterfaces);
                    if (filter.filterProps.DisplayName.Length > 0)
                        menu.MenuItems.Add("Add to favorites", AddToFavorites);
                    menu.Show(this, eLocation);
                }
            }
            else //out of filter right click
            {
                ContextMenu menu = new ContextMenu();
                menu.MenuItems.Add("Render file...", RenderFile);
                menu.MenuItems.Add("Add source filter...", AddSourceFilter);
                menu.MenuItems.Add("Load graph...", LoadGraph);
                if (savedFileName != null)
                    menu.MenuItems.Add("Save graph", SaveGraph);
                menu.MenuItems.Add("Save graph as...", SaveGraphAs);
                menu.MenuItems.Add("See event log...", ShowEventLog);
                menu.MenuItems.Add("Arrange filters", delegate
                {
                    graph.LayoutFilters();
                    graph.RecalcPaths();
                    Invalidate();
                });
                menu.Show(this, eLocation);
            }
        }

        private void GraphForm_MouseDown(object sender, MouseEventArgs e)
        {
            mousepos = e.Location;
            mousepos.X += hScrollBar.Value;
            mousepos.Y += vScrollBar.Value - toolStripHeight;
            if (e.Button == MouseButtons.Left)
                OnLButtonDown();
            if (e.Button == MouseButtons.Right)
                OnRButtonDown(e.Location);
            Focus();
        }

        private void OnLButtonUp()
        {
            if (movingFilter != null)
                foreach (Filter f in graph.SelectedFilters)
                    f.movingStartCoords = f.Coords;
            movingFilter = null;

            if (connectingPin != null)
            {
                if (mousepos == movingStart) //just click on pin
                {
                    if (connectingPin.Connection != null)
                    {
                        AMMediaType mt = new AMMediaType();
                        connectingPin.IPin.ConnectionMediaType(mt);
                        MediaTypeProps mtp = MediaTypeProps.CreateMTProps(mt); //new MediaTypeProps(mt);
                        Program.mainform.propform.SetObject(mtp);
                    }
                    else
                    {
                        IEnumMediaTypes mtenum;
                        if (connectingPin.IPin.EnumMediaTypes(out mtenum) >= 0)
                        {
                            AMMediaType[] mts = new AMMediaType[1];
                            List<MediaTypeProps> mtypes = new List<MediaTypeProps>();
                            IntPtr fetched = Marshal.AllocHGlobal(4);
                            while (mtenum.Next(1, mts, fetched) == 0)
                                mtypes.Add(MediaTypeProps.CreateMTProps(mts[0]));
                            Marshal.FreeHGlobal(fetched);
                            Program.mainform.propform.SetObject(mtypes.ToArray());
                        }
                    }
                }
                else
                {
                    if (otherPin != null)
                    {
                        Pin inpin, outpin;
                        if (connectingPin.Direction == PinDirection.Input)
                        {
                            inpin = connectingPin;
                            outpin = otherPin;
                        }
                        else
                        {
                            inpin = otherPin;
                            outpin = connectingPin;
                        }
                        graph.Connect(outpin, inpin, true);
                    }
                }
                Invalidate();
            }

            if (selecting)
            {
                Rectangle rc = new Rectangle(Math.Min(mousepos.X, movingStart.X), Math.Min(mousepos.Y, movingStart.Y),
                    Math.Abs(mousepos.X - movingStart.X), Math.Abs(mousepos.Y - movingStart.Y));
                if (ModifierKeys != Keys.Shift)
                    graph.ClearFiltersSelection();
                graph.SelectSeveralFilters(rc);
                selecting = false;
                Invalidate();
            }
            connectingPin = null;
            otherPin = null;
        }

        private void OnRButtonUp(Point eLocation)
        {
            if (connectingPin != null)
            {
                if (mousepos == movingStart) //pin right click
                {
                    ContextMenu menu = new ContextMenu();
                    if (connectingPin.Direction == PinDirection.Output)
                        menu.MenuItems.Add("Render pin", RenderPin);
                    if (connectingPin.HasPropertyPage())
                        menu.MenuItems.Add("Property page", ShowPropertyPage);
                    menu.MenuItems.Add("Scan interfaces", ScanInterfaces);
                    menu.MenuItems.Add("Show matching filters", ShowMatchingFilters);

                    if ((connectingPin.IPin as IAMStreamConfig) != null)
                    {
                        menu.MenuItems.Add("IAMStreamConfig::SetFormat", ConfigStream);
                        menu.MenuItems.Add("IAMStreamConfig::GetStreamCaps", GetStreamCaps);
                    }
                    if ((connectingPin.IPin as IMemInputPin) != null)
                    {
                        menu.MenuItems.Add("See allocator properties", ShowAllocatorProperties);
                        menu.MenuItems.Add("See allocator requirements", ShowAllocatorRequirements);
                    }
                    menu.Show(this, eLocation);
                    return;
                }
                if (otherPin != null) //direct connect
                {
                    Pin inpin, outpin;
                    if (connectingPin.Direction == PinDirection.Input)
                    {
                        inpin = connectingPin;
                        outpin = otherPin;
                    }
                    else
                    {
                        inpin = otherPin;
                        outpin = connectingPin;
                    }
                    graph.Connect(outpin, inpin, false);
                }
            }
            connectingPin = null;            
            otherPin = null;
            Invalidate();
        }

        private void GraphForm_MouseUp(object sender, MouseEventArgs e)
        {
            mousepos = e.Location;
            mousepos.X += hScrollBar.Value;
            mousepos.Y += vScrollBar.Value - toolStripHeight;
            if (e.Button == MouseButtons.Left)
                OnLButtonUp();
            if (e.Button == MouseButtons.Right)
                OnRButtonUp(e.Location);
        }

        private void GraphForm_MouseMove(object sender, MouseEventArgs e)
        {
            mousepos = e.Location;
            mousepos.X += hScrollBar.Value;
            mousepos.Y += vScrollBar.Value - toolStripHeight;

            if (movingFilter != null)
            {
                int dx = (mousepos.X - movingStart.X) / graph.cellsize;
                int dy = (mousepos.Y - movingStart.Y) / graph.cellsize;
                if (dx != 0 || dy != 0)
                {
                    Point c = movingFilter.movingStartCoords;
                    c.X += dx;
                    c.Y += dy;
                    c.X = Math.Max(c.X, 1);
                    c.Y = Math.Max(c.Y, 0);
                    if (c != movingFilter.Coords)
                    {
                        foreach(Filter f in graph.SelectedFilters)
                            graph.PlaceFilter(f, false);
                        bool canplace = true;
                        foreach (Filter f in graph.SelectedFilters)
                        {
                            c = f.movingStartCoords;
                            c.X += dx;
                            c.Y += dy;
                            c.X = Math.Max(c.X, 1);
                            c.Y = Math.Max(c.Y, 0);
                            if (!graph.CanPlaceFilter(c, f))
                            {
                                canplace = false;
                                break;
                            }
                        }

                        if (canplace)
                        {
                            foreach (Filter f in graph.SelectedFilters)
                            {
                                c = f.movingStartCoords;
                                c.X += dx;
                                c.Y += dy;
                                c.X = Math.Max(c.X, 1);
                                c.Y = Math.Max(c.Y, 0);
                                f.Coords = c;
                                graph.PlaceFilter(f, true);
                            }
                            graph.RecalcPaths();
                            RecalcScrolls();
                            Invalidate();
                        }
                        else
                            foreach (Filter f in graph.SelectedFilters)
                                graph.PlaceFilter(f, true);
                    }
                }
            }
            if (connectingPin != null && connectingPin.Connection==null)
            {
                otherPin = null;
                Filter f = graph.FilterInPoint(mousepos);
                if (f != null)
                {
                    Pin p = f.PinInPoint(mousepos);
                    if (p != null && p.Direction != connectingPin.Direction && p.Connection==null)
                        otherPin = p;                        
                }
                Invalidate();
            }
            if (selecting)
            {
                Invalidate();
            }
            DescribeActions();
        }

        void DescribeActions()
        {
            StringBuilder sb = new StringBuilder();
            if (movingFilter == null && connectingPin == null) //no acton yet
            {
                sb.Append("Right click for menu. ");
                Filter filter = graph.FilterInPoint(mousepos);
                if (filter != null)
                {                    
                    Pin pin = filter.PinInPoint(mousepos);
                    if (pin != null && pin.Connection == null)
                        sb.Append("Drag with left button down for intelligent connect, with right button for direct connect, left click to see media types. ");
                    else
                        sb.Append("Drag with left button down to move filter, left click to select and see properties. Shift+click to add to selection. ");
                } else {                   
                    int con_id = graph.ownersmap[mousepos.X / graph.cellsize, mousepos.Y / graph.cellsize];
                    if (con_id > 0)
                    {
                        sb.Append("Left click to see connection mediatype. ");
                        if (graph.SelectedConnection != null && graph.SelectedConnection.ID == con_id)
                            sb.Append("Press Del to remove connection. ");
                    }
                    else
                        if (selecting)
                            sb.Append("Release mouse button to select filters in rectangle. ");
                        else
                            if (graph.HasFilters)
                                sb.Append("Drag mouse with left button down to select several filters. ");
                    
                }
            }
            if (movingFilter != null)
                sb.Append("Release mouse button to stop moving filters. ");
            if (graph.SelectedFilters.Count > 0)
                sb.Append("Press Del to remove selected filters. ");
            if (connectingPin != null)
                if (otherPin != null)
                    sb.Append("Release mouse button to attempt connection. ");
                else
                    sb.Append("Move mouse to the pin you want to connect to. ");
            if (RegistryChecker.R[1]==0)
                sb.AppendFormat("{0} days to evaluate.", RegistryChecker.R[93]);
            Program.mainform.SetHint(sb.ToString());
        }

        private void GraphForm_KeyDown(object sender, KeyEventArgs e)
        {
            int k = e.KeyValue;
            //MessageBox.Show(k.ToString());

            if (k == 46) //Del button
            {
                graph.Stop();
                if (graph.SelectedFilters.Count > 0)
                {
                    foreach (Filter f in graph.SelectedFilters)
                        graph.RemoveFilter(f, false);
                    graph.ClearFiltersSelection();
                    graph.RecalcPaths();
                    Invalidate();
                }
                PinConnection con = graph.SelectedConnection;
                if (con != null)
                {
                    graph.SelectedConnection = null;
                    graph.RemoveConnection(con, true);
                    Invalidate();
                }
                e.Handled = true;
            }
        }

        public delegate Form ShowCodeDel(string code);
        ShowCodeDel[] showCode = new ShowCodeDel[2];

        void RecalcScrolls()
        {
            Point sz = graph.GetRealGraphSize();
            Size cs = ClientSize;
            hScrollBar.Maximum = (sz.X+1) * graph.cellsize;
            vScrollBar.Maximum = (sz.Y+1) * graph.cellsize;
            hScrollBar.Visible = (hScrollBar.Maximum > cs.Width || hScrollBar.Value > 0);
            vScrollBar.Visible = (vScrollBar.Maximum > cs.Height || vScrollBar.Value > 0);
            hScrollBar.LargeChange = hScrollBar.Maximum / 10;
            hScrollBar.SmallChange = hScrollBar.Maximum / 20;
            vScrollBar.LargeChange = vScrollBar.Maximum / 10;
            vScrollBar.SmallChange = vScrollBar.Maximum / 20;
        }

        private void hScrollBar_ValueChanged(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void vScrollBar_ValueChanged(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void GraphForm_SizeChanged(object sender, EventArgs e)
        {
            RecalcScrolls();
        }

        public void SaveGraphAs(object sender, EventArgs e)
        {
            SaveFileDialog fd = new SaveFileDialog();
            fd.DefaultExt = "*.grf";
            fd.Filter = "Graph files (*.grf)|*.grf|All files (*.*)|*.*";
            if (fd.ShowDialog() == DialogResult.OK)
            {
                graph.SaveGraph(fd.FileName);
                savedFileName = fd.FileName;
                Text = savedFileName;
            }
        }

        public void LoadGraph(object sender, EventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.DefaultExt = "*.grf";
            fd.Filter = "Graph files (*.grf)|*.grf|All files (*.*)|*.*";
            if (fd.ShowDialog() == DialogResult.OK)
                DoLoad(fd.FileName);
        }

        public void DoLoad(string file)
        {
            if (graph.LoadGraph(file, delegate() { Text = file; }))
            {
                RecalcScrolls();
                Invalidate();
                savedFileName = file;
            }
        }

        public void SaveGraph(object sender, EventArgs e)
        {
            if (savedFileName == null)
                SaveGraphAs(sender, e);
            else
                graph.SaveGraph(savedFileName);
        }

        private void GraphForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (eventlogform != null)
                eventlogform.Close();
            graph.Close();
            //graph.Dispose();
            Program.mainform.ActiveGraphForm = null;
            Program.mainform.Text = "GraphEditPlus";
        }
        Timer regtimer = new Timer();

        private void GraphForm_Load(object sender, EventArgs e)
        {
            int n = Math.Min(ngraphs, 15);
            Location = new Point(300 + n * 10, (n-1) * 30);
            zoomCombo.Items.Add("25%");
            zoomCombo.Items.Add("50%");
            zoomCombo.Items.Add("75%");
            int i = zoomCombo.Items.Add("100%");
            zoomCombo.SelectedIndex = i;

            regtimer.Interval = 250;
            regtimer.Tick += delegate{
                regtimer.Stop();
                if (RegistryChecker.R[1] < 1 && RegistryChecker.R[93] < 1)
                {
                    MessageBox.Show("Evaluation period has expired. Register the program to continue using it.");
                    Close();
                }
            };
            regtimer.Start();
            toolStripHeight = toolStrip.Size.Height;
            graph.SetEventWindow(Handle);
            timer.Interval = 1000;
            timer.Tick += OnTimer;
            timer.Start();
            toolStrip.Renderer = new SickToolStripRenderer(graph);
        }

        public void SetScale(int _scale) //0..3
        {
            graph.cellsize = (_scale + 1) * 4;
            Invalidate();
        }

        private void OnZoomChanged(object sender, EventArgs e)
        {
            SetScale(zoomCombo.SelectedIndex);            
            Focus();
        }

        private void OnPlay(object sender, EventArgs e)
        {
            graph.Run();
        }

        private void OnPause(object sender, EventArgs e)
        {
            graph.Pause();
        }

        private void OnStop(object sender, EventArgs e)
        {
            graph.Stop();            
        }

        private void OnTimer(Object myObject, EventArgs myEventArgs)
        {
            graph.UpdateState();
            labelState.Text = graph.State;
            labelPosition.Text = graph.Positions;
            Invalidate();
        }

        bool sliding = false;

        private void OnToolStripMouseDown(object sender, MouseEventArgs e)
        {
            Slide(e);
        }

        private void OnToolStripMouseMove(object sender, MouseEventArgs e)
        {
            if (sliding)
                Slide(e);
        }

        void Slide(MouseEventArgs e)
        {
            int start = slider_start;
            int end = toolStrip.Size.Width - 8;
            if (e.X >= start && e.X < end)
            {
                if (end > start)
                {
                    sliding = true;
                    long cur = graph.Position / 100000;
                    long dur = graph.Duration / 100000;
                    long x = (long)((e.X - start) * dur / (end - start) * 100000 );
                    graph.Position = x;
                    toolStrip.Invalidate();
                    //Invalidate();                    
                }
            } else
                sliding = false;
        }

        private void OnToolStripMouseUp(object sender, MouseEventArgs e)
        {
            sliding = false;
        }

        private void OnToolStripMouseLeave(object sender, EventArgs e)
        {
            sliding = false;
        }

        public void GenerateCode(lang lng)
        {            
            graph.GenerateCode(showCode[RegistryChecker.R[27] + RegistryChecker.R[14] + RegistryChecker.R[52]], lng,
                Program.mainform.useDirectConnect);
        }

        private void GraphForm_MouseLeave(object sender, EventArgs e)
        {
            Program.mainform.SetHint("");
        }

        public Bitmap MakeImage()
        {
            Point psz = graph.GetRealGraphSize();
            int sx = (psz.X+1) * graph.cellsize, sy = (psz.Y+1) * graph.cellsize;
            Bitmap bmp = new Bitmap(sx, sy);
            Graphics g = Graphics.FromImage(bmp);
            g.FillRectangle(Brushes.LightGray, 0, 0, sx, sy);
            graph.Draw(g, new Point(0, 0));
            return bmp;
        }

        public const int WM_GRAPHEVENT = 0x8000 + 1350;

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_GRAPHEVENT:
                    graph.OnGraphEvent();
                    break;
            }
            base.WndProc(ref m);
        }

        public void LayoutFilters()
        {
            graph.LayoutFiltersAndPaths();
            Invalidate();
        }

        private void OnDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("System.Windows.Forms.TreeNode", false))
                e.Effect = DragDropEffects.Copy;
        }

        private void OnDragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("System.Windows.Forms.TreeNode", false)) {
                TreeNode nd = (TreeNode)e.Data.GetData("System.Windows.Forms.TreeNode", false);
                if (nd == null)
                    return;
                while (nd.Parent != null && nd.Tag == null)
                    nd = nd.Parent;
                FilterProps fp = (FilterProps)nd.Tag;
                if (fp != null)
                {
                    Point p = PointToClient(new Point(e.X, e.Y));                    
                    p.X += hScrollBar.Value;
                    p.Y += vScrollBar.Value - toolStripHeight;
                    if (p.Y < 0) p.Y = 0;
                    AddFilter(fp, new Point(p.X / graph.cellsize, p.Y / graph.cellsize));
                }
            }
        }

    }// GraphForm class

    class SickToolStripRenderer : ToolStripProfessionalRenderer
    {
        Image img_slider;
        Graph graph;

        public SickToolStripRenderer(Graph _graph) : base()
        {
            graph = _graph;
            ComponentResourceManager resources = new ComponentResourceManager(typeof(GraphForm));
            img_slider = ((Image)(resources.GetObject("btnSlider.Image")));
        }

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            base.OnRenderToolStripBackground(e);
            int start = GraphForm.slider_start;
            int w = e.ToolStrip.Size.Width - start - 8;
            long cur = graph.Position / 100000;
            long dur = graph.Duration / 100000;
            int pos = dur > 0 ? (int)(cur * w / dur) : 0;
            e.Graphics.FillRectangle(Brushes.White, start, 9, w, 6);
            e.Graphics.DrawRectangle(Pens.Black, start, 9, w, 6);
            e.Graphics.DrawImageUnscaled(img_slider, start+pos-5, 6);
        }
    }

    public enum lang
    {
        CPP = 1,
        CS = 2
    }

}//namespace