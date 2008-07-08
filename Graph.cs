using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using DirectShowLib;
using System.Runtime.InteropServices;

namespace gep
{
    class Graph : IDisposable
    {
        List<Filter> filters = new List<Filter>();
        DynArray2<int> field = new DynArray2<int>(); //density for each cell
        public DynArray2<int> ownersmap = new DynArray2<int>(); //for connections
        List<PinConnection> connections = new List<PinConnection>();
        List<Filter> selectedFilters = new List<Filter>();
        PinConnection selectedConnection;
        IGraphBuilder graphBuilder;
        DsROTEntry rot_entry;
        GraphForm myform;
        IMediaControl mediaControl;
        IMediaSeeking mediaSeeking;
        IMediaEvent mediaEvent;
        int cellSize = 16;
        History history = new History();
        Dictionary<string, Point> filter_positions = new Dictionary<string, Point>(); //uniq name => pos

        public int cellsize
        {
            get { return cellSize; }
            set { 
                cellSize = value;
                foreach (Filter f in filters)
                {
                    f.RepositionPins();
                    f.Coords = f.Coords;//recalc rect
                }
                Filter.namefont = new Font("Arial", cellSize / 2);
                Pin.pinfont = new Font("Arial", cellSize / 2);
            }
        }


        public GraphForm Form { get { return myform; } }
        bool isFromRot;
        bool disconnecting = false; //we're just disconnecting from ROT
        public bool IsFromRot { get { return isFromRot; } }

        public Graph(GraphForm gf)
        {
            myform = gf;
            isFromRot = false;
            graphBuilder = (IGraphBuilder)new FilterGraph();
            rot_entry = new DsROTEntry(graphBuilder);
            mediaControl = (IMediaControl)graphBuilder;
            mediaSeeking = (IMediaSeeking)graphBuilder;
            mediaEvent = (IMediaEvent)graphBuilder;
        }

        public Graph(GraphForm gf, IGraphBuilder gb) //for remote graphs
        {
            myform = gf;
            graphBuilder = gb;
            isFromRot = true;
            mediaControl = (IMediaControl)graphBuilder;
            mediaSeeking = (IMediaSeeking)graphBuilder;
            mediaEvent = (IMediaEvent)graphBuilder;
            ReloadGraph();
        }

        void ReloadGraph()
        {
            ReloadFilters();
            LayoutFiltersAndPaths();
        }

        public void LayoutFiltersAndPaths()
        {
            LayoutFilters();
            if (!Program.mainform.autoArrange)
                LayoutFiltersManual();
            RecalcPaths();
        }

        public List<Filter> SelectedFilters
        {
            get { return selectedFilters; }
            //set { selectedFilter = value; }
        }

        public void SelectFilter(Filter f, bool select)
        {
            if (select)
            {
                if (!selectedFilters.Contains(f))
                    selectedFilters.Add(f);
                selectedConnection = null;
            } else
                selectedFilters.Remove(f);
        }

        public void ClearFiltersSelection()
        {
            selectedFilters.Clear();
            selectedConnection = null;
        }

        public PinConnection SelectedConnection
        {
            get { return selectedConnection; }
            set { selectedConnection = value; }
        }

        public void Draw(Graphics g, Point viewpoint)
        {
            foreach (Filter f in filters)
                f.Draw(g, viewpoint);
            foreach (PinConnection con in connections)
                if (con != selectedConnection)
                    con.Draw(g, false, viewpoint);
            if (selectedConnection != null)
                selectedConnection.Draw(g, true, viewpoint);
        }

        public void AddFilter(FilterProps fp) //create new IBaseFilter, add it to DS graph and this graph
        {
            Filter f;
            try
            {
                f = new Filter(fp);
                int hr = graphBuilder.AddFilter(f.BaseFilter, fp.Name);
                DsError.ThrowExceptionForHR(hr);
            }
            catch (COMException e)
            {
                ShowCOMException(e, "Error creating filter " + fp.Name);
                return;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error adding filter");
                return;
            }
            string srcfn = null, dstfn = null;
            if ((f.BaseFilter as IFileSourceFilter) != null)
                srcfn = f.ChooseSrcFileName();

            if ((f.BaseFilter as IFileSinkFilter) != null)
                dstfn = f.ChooseDstFileName();

            AddFilterHere(f, true);
            history.AddFilter(fp, f.Name, srcfn, dstfn);
        }

        void AddFilterHere(Filter f, bool recalcPaths)
        {
            f.JoinGraph(this, false);
            f.Coords = FindPlaceForFilter(f);
            filters.Add(f);
            PlaceFilter(f, true);
            if (recalcPaths)
                RecalcPaths();
        }

        public void RemoveFilter(Filter f, bool recalcPaths)
        {
            try
            {
                if (f.sampleGrabberForm!=null)
                    f.sampleGrabberForm.Close(); //deletes callback and links to the SG form
                if (!disconnecting)
                {
                    f.BaseFilter.Stop();
                    int hr = graphBuilder.RemoveFilter(f.BaseFilter);
                    DsError.ThrowExceptionForHR(hr);                    
                }
                Marshal.ReleaseComObject(f.BaseFilter);
            }
            catch (COMException e)
            {
                ShowCOMException(e, "Error removing filter " + f.Name);
                return;
            }
            history.RemoveFilter(f.Name);
            PlaceFilter(f, false);
            f.JoinGraph(null, disconnecting);
            filters.Remove(f);
            if (recalcPaths)
                RecalcPaths();
        }

        public Filter FilterInPoint(Point point)
        {
            foreach (Filter f in filters)
                if (f.Rect.Contains(point))
                    return f;
            return null;
        }

        public void PlaceFilter(Filter f, bool put) //put=true - mark its space on field, put=false - cleat
        {
            int val = put ? 10000 : 0;
            field.FillRect(f.Coords.X, f.Coords.Y, f.boxsize.X, f.boxsize.Y, val);
        }

        public bool CanPlaceFilter(Point place, Filter f, int border)
        {
            if (place.X-border < 0 || place.Y-border < 0) return false;
            for (int y = -border; y < f.boxsize.Y+border; y++)
                for (int x = -border; x < f.boxsize.X+border; x++)
                    if (field[x + place.X, y + place.Y] >= 10000)
                        return false;
            return true;
        }

        public bool CanPlaceFilter(Point place, Filter f)
        {
            return CanPlaceFilter(place, f, 0);
        }

        protected void Connect(Pin outpin, Pin inpin) //ipins must be already connected
        {
            PinConnection con = new PinConnection(outpin, inpin);
            List<ConStep> path = CalcPath(outpin, inpin, con.ID);
            con.path = path;
            connections.Add(con);
            history.ConnectIfNew(outpin, inpin, con);
        }

        public void Connect(Pin outpin, Pin inpin, bool intelligent)
        {
            try
            {
                FilterGraphTools.ConnectFilters(graphBuilder, outpin.IPin, inpin.IPin, intelligent);
            }
            catch (COMException e)
            {
                ShowCOMException(e, "Can't connect pins");
                return;
            }
            
            if (intelligent)
                ReloadGraph();
            else
            {
                Connect(outpin, inpin);
                history.CommitAdded(this);
                inpin.Filter.ReloadPins();
                outpin.Filter.ReloadPins();
            }
        }

        public void RemoveConnection(PinConnection con, bool recalcPaths)
        {
            if (con == null)
                return;
            history.RemoveConnection(con.pins[0], con.pins[1]);
            con.Disconnect(true);
            connections.Remove(con);
            if (recalcPaths)
                RecalcPaths();
        }

        public void RecalcPaths()
        {
            //clear field
            Point gsz = GetGraphSize();
            field.FillRect(0, 0, gsz.X, gsz.Y, 0);
            ownersmap.FillRect(0, 0, gsz.X, gsz.Y, 0);
            //recalc
            foreach (Filter f in filters)
                PlaceFilter(f, true);
            foreach (PinConnection con in connections)
                con.path = CalcPath(con.pins[0], con.pins[1], con.ID);
        }

        public Point GetGraphSize()
        {
            int mx = 0, my = 0;
            foreach (Filter f in filters)
            {
                int x = f.Coords.X + f.boxsize.X;
                mx = Math.Max(mx, x);
                int y = f.Coords.Y + f.boxsize.Y;
                my = Math.Max(my, y);
            }
            mx += 10; my += 10;
            return new Point(mx, my);
        }

        public Point GetRealGraphSize()
        {
            Point sz = GetGraphSize();
            sz.X -= 10; sz.Y -= 10;
            foreach(PinConnection con in connections)
                foreach (ConStep st in con.path)
                {
                    sz.X = Math.Max(sz.X, st.x);
                    sz.Y = Math.Max(sz.Y, st.y);
                }
            return sz;
        }

        int[,] cost;
        int[,] dirs;
        PriorityQueue<Point> changed;
        Point pdst;

        public List<ConStep> CalcPath(Pin outpin, Pin inpin, int con_id)
        {
            Point gsz = GetGraphSize();
            int mx = gsz.X, my = gsz.Y;
            //calc graph field size
            cost = new int[mx, my];
            dirs = new int[mx, my];

            //get pins coords
            Point psrc = new Point(outpin.Filter.Coords.X + outpin.Filter.boxsize.X,
                                 outpin.Filter.Coords.Y + outpin.Rect.Top / cellsize);
            pdst = new Point(inpin.Filter.Coords.X-1,
                                 inpin.Filter.Coords.Y + inpin.Rect.Top / cellsize);
            
            //make path field
            changed = new PriorityQueue<Point>();//Queue<Point>();
            cost[psrc.X, psrc.Y] = 1;
            dirs[psrc.X, psrc.Y] = 2;
            changed.Enqueue(psrc,1);
            Point p;
            while (changed.Count > 0)
            {
                p = changed.Dequeue().Value;
                if (p == pdst)
                    break;
                int x = p.X, y = p.Y;
                int c = cost[x, y];
                if (p.X>0) 
                    CheckPath(p.X - 1, p.Y, c, 0);
                if (p.X < mx-1)
                    CheckPath(p.X + 1, p.Y, c, 2);
                if (p.Y > 0)
                    CheckPath(p.X, p.Y - 1, c+1, 3);
                if (p.Y < my - 1)
                    CheckPath(p.X, p.Y + 1, c+1, 1);
            }

            //make path
            p = pdst;
            List<ConStep> path = new List<ConStep>();
            int cd = 2;
            //  43
            //  56
            while (true)
            {
                int nd = dirs[p.X, p.Y], d = 0;
                if (cd == 0 && nd == 0) d = 2;
                if (cd == 0 && nd == 1) d = 4;
                if (cd == 0 && nd == 2) d = 0;
                if (cd == 0 && nd == 3) d = 5;
                if (cd == 1 && nd == 0) d = 6;
                if (cd == 1 && nd == 1) d = 1;
                if (cd == 1 && nd == 2) d = 5;
                if (cd == 1 && nd == 3) d = 0;
                if (cd == 2 && nd == 0) d = 0;
                if (cd == 2 && nd == 1) d = 3;
                if (cd == 2 && nd == 2) d = 2;
                if (cd == 2 && nd == 3) d = 6;
                if (cd == 3 && nd == 0) d = 3;
                if (cd == 3 && nd == 1) d = 0;
                if (cd == 3 && nd == 2) d = 4;
                if (cd == 3 && nd == 3) d = 1;

                if (d == 0)
                    break;
                ConStep cs = new ConStep();
                cs.x = p.X; cs.y = p.Y; cs.dir = d;
                path.Add(cs);
                field[p.X, p.Y] += 100;
                ownersmap[p.X, p.Y] = con_id;
                if (p == psrc)
                    break;

                if (nd == 0) p.X++;
                if (nd == 2) p.X--;
                if (nd == 3) p.Y++;
                if (nd == 1) p.Y--;
                cd = nd;
            }

            changed = null; cost = null; //dirs = null;
            return path;
        }

        void CheckPath(int x, int y, int c, int dir)
        {
            int c1 = c + field[x, y] + 1 + Math.Abs(pdst.X - x) + Math.Abs(pdst.Y - y);
            if (cost[x, y] == 0)
            {
                cost[x, y] = c1;
                dirs[x, y] = dir;
                Point p1 = new Point(x, y);
                changed.Enqueue(p1, c1);
            }
        }

        public void SelectConnection(int con_id)
        {
            foreach(PinConnection con in connections)
                if (con.ID == con_id)
                {
                    SelectedConnection = con;
                    AMMediaType mt = new AMMediaType();
                    con.pins[0].IPin.ConnectionMediaType(mt);
                    MediaTypeProps mtp = MediaTypeProps.CreateMTProps(mt); //new MediaTypeProps(mt);
                    Program.mainform.propform.SetObject(mtp);
                    break;
                }
        }

        public void RenderFile(string filename)
        {
            RenderSomething(filename, "Can't render file ");
        }

        public void RenderURL(string url)
        {
            RenderSomething(url, "Can't render URL ");
        }

        void RenderSomething(string filename, string errmsg)
        {
            try {
                int hr = graphBuilder.RenderFile(filename, null);
                DsError.ThrowExceptionForHR(hr);
            }
            catch (COMException e)
            {
                ShowCOMException(e, errmsg + filename);
                return;
            }
            ReloadGraph();
        }

        public void AddSourceFilter(string filename)
        {
            try
            {
                IBaseFilter ifilter;
                int hr = graphBuilder.AddSourceFilter(filename, filename, out ifilter);
                DsError.ThrowExceptionForHR(hr);
            }
            catch (COMException e)
            {
                ShowCOMException(e, "Can't add source filter for " + filename);
                return;
            }
            ReloadGraph();            
        }

        public void RenderPin(Pin pin)
        {
            try
            {
                int hr = graphBuilder.Render(pin.IPin);
                DsError.ThrowExceptionForHR(hr);
            }
            catch (COMException e)
            {
                ShowCOMException(e, "Can't render pin");
                return;
            }
            ReloadGraph();
        }

        public void ReloadFilters()
        {
            IEnumFilters ef;
            ClearFiltersSelection();
            ClearConnections();

            filter_positions.Clear();
            foreach (Filter f in filters)
                filter_positions.Add(f.Name, f.Coords);
            filters.Clear();
            try
            {
                int hr = graphBuilder.EnumFilters(out ef);
                DsError.ThrowExceptionForHR(hr);
                IBaseFilter[] fs = new IBaseFilter[1];
                IntPtr fetched = Marshal.AllocHGlobal(4);
                while ((hr = ef.Next(1, fs, fetched)) == 0)
                {
                    FilterInfo fi;
                    fs[0].QueryFilterInfo(out fi);
                    Filter ff = FindFilterByName(fi.achName);
                    if (ff == null) //not found
                    {
                        ff = new Filter(fs[0]);
                        AddFilterHere(ff, false);
                        history.AddFilterIfNew(ff.filterProps, ff.Name, ff.srcFileName, ff.dstFileName, ff);
                    } else
                        ff.ReloadPins();

                    foreach (Pin pin in ff.Pins)
                    {
                        IPin ip = pin.IPin, connected_ipin;
                        hr = ip.ConnectedTo(out connected_ipin);
                        if (hr != 0) continue;
                        PinInfo cpi;
                        connected_ipin.QueryPinInfo(out cpi);
                        FilterInfo cfi;
                        cpi.filter.QueryFilterInfo(out cfi);
                        Filter connected_filter = FindFilterByName(cfi.achName);
                        if (connected_filter != null)
                        {
                            Pin cp = connected_filter.FindPinByName(connected_filter.Name + "." + cpi.name);
                            if (cp != null)
                            {
                                string con_uniqname = (pin.Direction == PinDirection.Input) ?
                                       cp.UniqName + "-" + pin.UniqName : pin.UniqName + "-" + cp.UniqName;
                                PinConnection con = FindConnectionByName(con_uniqname);
                                if (con == null)
                                    if (pin.Direction == PinDirection.Input)
                                        Connect(cp, pin);
                                    else
                                        Connect(pin, cp);
                            }
                        }
                        DsUtils.FreePinInfo(cpi);
                    }
                }
                Marshal.FreeHGlobal(fetched);
                if (hr < 0)
                    DsError.ThrowExceptionForHR(hr);
            }
            catch (COMException e)
            {
                ShowCOMException(e, "Error while enumerating filters in the graph");
                return;
            }
            history.CommitAdded(this);
        }

        public Filter FindFilterByName(string name)
        {
            return filters.Find(delegate(Filter t) { return t.OrgName == name; });
        }

        PinConnection FindConnectionByName(string name)
        {
            return connections.Find(delegate(PinConnection c) { return c.UniqName == name; });
        }

        void ClearConnections()
        {
            foreach (PinConnection con in connections)
                con.Disconnect(false);
            connections.Clear();
        }

        public void LayoutFilters()
        {
            DynArray<List<Filter>> grid = new DynArray<List<Filter>>();
            DynArray<int> colWidths = new DynArray<int>();
            int maxst = 0;
            foreach (Filter flt in filters)
            {
                int st = flt.Stage;
                maxst = Math.Max(maxst, st);
                if (grid[st] == null)
                    grid[st] = new List<Filter>();
                grid[st].Add(flt);
                colWidths[st] = Math.Max(colWidths[st], flt.boxsize.X);
                flt.PropagateWeight();
                PlaceFilter(flt, false);
            }

            DynArray<int> Xs = new DynArray<int>();
            int x = 1;
            for(int i=1; i<=maxst; i++) 
            { 
                Xs[i] = x;
                x += colWidths[i] + 2;
                int y = 1;
                if (grid[i] != null)
                {
                    grid[i].Sort(delegate(Filter a, Filter b) { return a.weight - b.weight; });
                    for (int j = 0; j < grid[i].Count; j++)
                    {
                        grid[i][j].Coords = new Point(Xs[i], y);
                        PlaceFilter(grid[i][j], true);
                        y += grid[i][j].boxsize.Y + 1;
                    }
                }
            }
        }

        void LayoutFiltersManual() //must be called after LayoutFilters
        {
            foreach (Filter f in filters)
                PlaceFilter(f, false);

            Point pos;
            foreach (Filter f in filters)
                if (filter_positions.TryGetValue(f.Name, out pos))
                {
                    f.Coords = pos;
                    PlaceFilter(f, true);
                }
            foreach (Filter f in filters)
                if (!filter_positions.TryGetValue(f.Name, out pos))
                {
                    f.Coords = FindNearPlaceForFilter(f);
                    PlaceFilter(f, true);
                }
        }

        Point FindPlaceForFilter(Filter f)
        {
            Point bestplace = new Point(1, 1);
            if (f.HasFreePins(PinDirection.Input)) 
            {
                int maxd = 10000;
                foreach (Filter ft in filters)
                    if (ft.HasFreePins(PinDirection.Output))
                    {
                        Point place = ft.Coords;
                        place.X += ft.boxsize.X;
                        while (!CanPlaceFilter(place, f))
                            place.X++;
                        if (place.X + place.Y < maxd)
                        {
                            bestplace = place;
                            maxd = place.X + place.Y;
                        }
                    }
                if (maxd < 10000) //found something
                    bestplace.X += 2;
            }
            while (!CanPlaceFilter(bestplace, f)) {
                while (!CanPlaceFilter(bestplace, f))
                    bestplace.Y++;
                bestplace.Y++;
            }
            return bestplace;
        }

        Point FindNearPlaceForFilter(Filter f)
        {
            Point orgpos = f.Coords;
            if (CanPlaceFilter(orgpos, f))
                return orgpos;
            for (int r = 1; r < 99; r++)
            {
                for (int x = -r; x < r; x++)
                {
                    Point p = new Point(orgpos.X + x, orgpos.Y - r);
                    if (CanPlaceFilter(p, f, 1)) return p;
                }
                for (int y = -r; y < r; y++)
                {
                    Point p = new Point(orgpos.X + r, orgpos.Y + y);
                    if (CanPlaceFilter(p, f, 1)) return p;
                }
                for (int x = r-1; x >= -r; x--)
                {
                    Point p = new Point(orgpos.X + x, orgpos.Y + r);
                    if (CanPlaceFilter(p, f, 1)) return p;
                }
                for (int y = r-1; y >= -r; y--)
                {
                    Point p = new Point(orgpos.X - r, orgpos.Y + y);
                    if (CanPlaceFilter(p, f, 1)) return p;
                }                    
            }
            return orgpos;
        }

        public static string GuidToString(Guid g)
        {
            return "{" + g.ToString().ToUpperInvariant()+ "}";
        }

        public static void ShowCOMException(COMException e, string message)
        {
            MessageBox.Show(e.ErrorCode.ToString("X") + ": " + e.Message, message, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void SaveGraph(string filename)
        {
            try
            {
                FilterGraphTools.SaveGraphFile(graphBuilder, filename);
            }
            catch (COMException e)
            {
                ShowCOMException(e, "Error saving graph "+filename);
            }
        }

        public delegate void on_graph_load();

        public bool LoadGraph(string filename, on_graph_load onload)
        {
            try
            {
                Clear();
                FilterGraphTools.LoadGraphFile(graphBuilder, filename);
                onload();
                ReloadGraph();
                return true;
            }
            catch (COMException e)
            {
                ShowCOMException(e, "Error loading graph " + filename);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Can't load graph");
            }
            return false;
        }

        public void Clear() //clears selection, filters and connections, removing filters from DS graph
        {
            if (!disconnecting) Stop();
            ClearFiltersSelection();
            selectedFilters.AddRange(filters);
            foreach (Filter f in selectedFilters)
                RemoveFilter(f, false);
            ClearFiltersSelection();
        }

        public void Close() //release all objects
        {
            disconnecting = isFromRot;
            Clear();
            mediaControl = null;
            mediaSeeking = null;
            mediaEvent = null;
            //Marshal.FreeHGlobal(evpar1);
            //Marshal.FreeHGlobal(evpar2);
            Marshal.ReleaseComObject(graphBuilder);
            graphBuilder = null;
            if (rot_entry != null)
            {
                rot_entry.Dispose();
                rot_entry = null;
            }
            GC.Collect();
        }

        /// <summary> release everything. </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            //Close();
        }

        ~Graph()
        {
            Dispose();
        }

        public void Run()
        {
            try
            {
                int hr = mediaControl.Run();
                DsError.ThrowExceptionForHR(hr);
            }
            catch (COMException e)
            {
                ShowCOMException(e, "Can't run the graph");
            }
        }

        public void Pause()
        {
            try
            {
                int hr = mediaControl.Pause();
                DsError.ThrowExceptionForHR(hr);
            }
            catch (COMException e)
            {
                ShowCOMException(e, "Can't pause the graph");
            }
        }

        public void Stop()
        {
            try
            {
                int hr = mediaControl.Stop();
                DsError.ThrowExceptionForHR(hr);
            }
            catch (COMException e)
            {
                ShowCOMException(e, "Can't stop the graph");
            }
        }

        string RefTimeToString(long rt)
        {
            StringBuilder sb = new StringBuilder();
            long totsec = rt / 10000000;
            long ssec = (rt / 100000) % 100;
            long sec = totsec % 60;
            long min = (totsec / 60) % 60;
            long hr = totsec / 3600;
            if (hr > 0)
                sb.AppendFormat("{0}:",hr);
            if (hr > 0 || min > 0)
                sb.AppendFormat("{0:D2}:", min);
            sb.AppendFormat("{0:D2},{1:D2}", sec, ssec);
            return sb.ToString();
        }

        public void UpdateState() //called from timer once a second
        {
            try
            {
                if (mediaControl != null)
                {
                    FilterState fs;
                    if (mediaControl.GetState(200, out fs) >= 0)
                        state = fs.ToString();
                }
                if (mediaSeeking != null)
                {
                    mediaSeeking.GetPositions(out currentPosition, out totalDuration);
                    positions = RefTimeToString(currentPosition) + " / " + RefTimeToString(totalDuration);
                }
                foreach (Filter f in filters)
                    f.UpdateState();
            }
            catch
            { }
        }

        string state;
        public string State { get { return state; } }

        string positions; //"00,50 / 13,42"
        public string Positions { get { return positions; } }

        long currentPosition=0, totalDuration=0;
        public long Position
        {
            get { return currentPosition; }
            set
            {                
                if (mediaSeeking != null)
                {
                    currentPosition = value;
                    mediaSeeking.SetPositions(currentPosition, AMSeekingSeekingFlags.AbsolutePositioning, totalDuration, AMSeekingSeekingFlags.AbsolutePositioning);
                }
            }
        }

        public long Duration { get { return totalDuration; } }

        private bool useClock = true;
        public bool UseClock
        {
            get { return useClock; }
            set
            {
                if (value)
                {
                    graphBuilder.SetDefaultSyncSource();
                    useClock = true;
                }
                else
                {
                    try
                    {
                        IMediaFilter imf = (IMediaFilter)graphBuilder;
                        int hr = imf.SetSyncSource(null);
                        DsError.ThrowExceptionForHR(hr);
                        useClock = false;
                    }
                    catch (COMException e)
                    {
                        ShowCOMException(e, "Exception caught when setting clock to NULL");
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.ToString(), "Exception caught when setting clock to NULL");
                    }
                }
            }
        }

        public void SelectSeveralFilters(Rectangle rc)
        {
            foreach (Filter f in filters)
                if (f.Rect.IntersectsWith(rc))
                    SelectFilter(f, true);
        }


        public void GenerateCode(GraphForm.ShowCodeDel shcode, lang lng, bool useDirectConnect)
        {
            CodeGenBase cg = null;
            switch (lng)
            {
                case lang.CPP: cg = new CodeGenCPP(); break;
                case lang.CS: cg = new CodeGenCS(); break;
            }
            cg.History = history;
            string code = cg.GenCode(useDirectConnect);
            Form cf = shcode(code);
            cf.Text = "Generated code for " + myform.Text;
            cf.ShowDialog();
        }

        public void ClearWeightAddedFlags()
        {
            foreach (Filter f in filters)
                f.weight_added = false;
        }

        public void Disconnect(IPin ipin)
        {
            try
            {
                int hr = graphBuilder.Disconnect(ipin);
                DsError.ThrowExceptionForHR(hr);
            }
            catch (COMException e)
            {
                ShowCOMException(e, "Error disconnecting pin");
            }
            catch 
            {
                //MessageBox.Show(e.Message, "Error disconnecting pin");
            }
        }

        LinkedList<GraphEvent> eventlog = new LinkedList<GraphEvent>();

        public void SetEventWindow(IntPtr hwnd)
        {
            try
            {
                IMediaEventEx me = graphBuilder as IMediaEventEx;
                if (me != null)
                {
                    int hr = me.SetNotifyWindow(hwnd, GraphForm.WM_GRAPHEVENT, IntPtr.Zero);
                    DsError.ThrowExceptionForHR(hr);
                }
            }
            catch (COMException ex)
            {
                ShowCOMException(ex, "Can't set notification window for graph events");
            }
        }

        public void OnGraphEvent()
        {
            try
            {
                IntPtr evpar1;
                IntPtr evpar2;// = Marshal.AllocHGlobal(4);
                EventCode ev;                
                if (mediaEvent.GetEvent(out ev, out evpar1, out evpar2, 10) == 0) //got event
                {
                    int ip1 = (int)evpar1; //Marshal.ReadInt32(evpar1);
                    int ip2 = (int)evpar2; //Marshal.ReadInt32(evpar2);
                    lock (eventlog)
                    {
                        eventlog.AddLast(new GraphEvent(ev, ip1, ip2));
                    }
                    if (ev == EventCode.Complete || ev == EventCode.UserAbort)
                        mediaControl.Stop();
                    if (ev == EventCode.ErrorAbort)
                    {
                        int hr = ip1;
                        DsError.ThrowExceptionForHR(hr);
                    }
                }
                mediaEvent.FreeEventParams(ev, evpar1, evpar2);                
            }
            catch (COMException ex)
            {
                if (ex.ErrorCode != unchecked((int)0x80070006)) //E_HANDLE
                    ShowCOMException(ex, "An error occured in the graph");
            }
        }

        public string GetEventLog()
        {
            StringBuilder sb = new StringBuilder();
            lock (eventlog)
            {
                foreach (GraphEvent ge in eventlog)
                {
                    ge.AddDescription(sb);
                }
            }
            return sb.ToString();
        }

        public void ClearEventLog()
        {
            lock (eventlog)
            {
                eventlog.Clear();
            }
        }

        public void PinSetFormat(Pin pin, AMMediaType mt)
        {
            history.SetFormat(pin, mt);
        }

    } //class

    class GraphEvent
    {
        public EventCode eventcode;        
        public long param1, param2;
        public DateTime time;

        public GraphEvent(EventCode evcode, long par1, long par2)
        {
            eventcode = evcode; param1 = par1; param2 = par2;
            time = DateTime.Now;
        }

        public void AddDescription(StringBuilder sb)
        {
            sb.AppendFormat("{0:T}.{1:D3}: ", time, time.Millisecond);
            sb.Append(eventcode.ToString());
            sb.AppendFormat(" ({0}), param1={1} (0x{1:X}), param2={2} (0x{2:X})", (int)eventcode, param1, param2);
            sb.AppendLine();
        }
    }
}//namespace
