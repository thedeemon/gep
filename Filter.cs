using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using DirectShowLib;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;
using DirectShowLib.BDA;
using DirectShowLib.DES;
using DirectShowLib.DMO;
using DirectShowLib.Dvd;
using DirectShowLib.MultimediaStreaming;
using DirectShowLib.SBE;
using System.Reflection;

namespace gep
{
    class Filter
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
        public int stage, weight;
        public bool movedManually = false;

        //private/protected
        string name, orgname; //orgname - without filename
        Point coords = new Point(1, 1); //in cells
        Rectangle rect; // in pixels
        static Brush namebrush = new SolidBrush(Color.White);
        List<Pin> pins = new List<Pin>();
        static Random rnd = new Random();
        Graph graph;
        IBaseFilter basefilter;
        FilterState filterState;
        static Dictionary<string, string> dispnames = new Dictionary<string, string>(); //orgname => display name


        public Filter(FilterProps fp)
        {
            filterProps = fp;

            int hr = 0, eaten;
            IBindCtx bindCtx = null;
            IMoniker moniker = null;
            basefilter = null;
            try
            {
                hr = NativeMethods.CreateBindCtx(0, out bindCtx);
                Marshal.ThrowExceptionForHR(hr);
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

        public void Draw(Graphics g, Point viewpoint)
        {
            int y1 = coords.Y * graph.cellsize - viewpoint.Y;
            int x1 = coords.X * graph.cellsize - viewpoint.X;
            int sy = boxsize.Y * graph.cellsize;
            int seldelta = graph.SelectedFilters.Contains(this) ? 50 : 0;
            int seldelta2 = seldelta / 2;
            Rectangle rc = rect;
            Point c = rc.Location;
            c.X -= viewpoint.X;
            c.Y -= viewpoint.Y;
            rc.Location = c;
            LinearGradientBrush br = new LinearGradientBrush(new Point(rc.Left, rc.Top-1), new Point(rc.Left, rc.Bottom+1),
                Color.FromArgb(100 + seldelta2, 100 + seldelta+seldelta2, 200+seldelta2), Color.FromArgb(50, 50, 100));
            g.FillRectangle(br, rc);

            Image img = images[(int)filterState];
            g.DrawImageUnscaled(img, rc.Left + rc.Width / 2 - 8, rc.Bottom - 20);

            g.DrawString(name, namefont, namebrush, x1 + 10, y1 + 5);
            foreach (Pin pin in pins)
                pin.Draw(g, x1, y1);
            br.Dispose();
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
            if (basefilter == null) return;
            FilterInfo fi;
            basefilter.QueryFilterInfo(out fi);
            name = fi.achName;
            orgname = fi.achName;

            IFileSourceFilter fsrc = basefilter as IFileSourceFilter;
            if (fsrc != null && fsrc.GetCurFile(out srcfilename, null) == 0 && srcfilename!=null && !name.Contains(srcfilename))
                name += " " + srcfilename.Substring(srcfilename.LastIndexOf('\\') + 1);

            IFileSinkFilter fdst = basefilter as IFileSinkFilter;
            if (fdst != null && fdst.GetCurFile(out dstfilename, null) == 0 && dstfilename!=null && !name.Contains(dstfilename))
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
                string s;
                vw.get_Caption(out s);
                string gname = Graph.Form.Text + ": " + name + ": ";
                vw.put_Caption(gname + s);
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

        public System.Collections.IEnumerable Connections
        {
            get 
            {
                foreach (Pin pin in pins)
                    if (pin.Connection != null)
                        yield return pin.Connection;
            }
        }

        public Pin FindPinByName(string pinname)
        {
            return pins.Find(delegate(Pin p) { return p.UniqName == pinname; });
        }

        public System.Collections.IEnumerable Pins { get { foreach (Pin pin in pins) yield return pin; } }

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
            Type[] ifaces = new Type[] {
            typeof(IAMAnalogVideoDecoder),
            typeof(IAMAudioInputMixer),
            typeof(IAMAudioRendererStats),
            typeof(IAMBufferNegotiation),
            typeof(IAMClockAdjust),
            typeof(IAMClockSlave),
            typeof(IAMCopyCaptureFileProgress),
            typeof(IAMCrossbar),
            typeof(IAMDecoderCaps),
            typeof(IAMDirectSound),
            typeof(IAMDroppedFrames),
            typeof(IAMErrorLog),
            typeof(IAMExtDevice),
            typeof(IAMExtendedSeeking),
            typeof(IAMExtTransport),
            typeof(IAMFilterMiscFlags),
            typeof(IAMGraphBuilderCallback),
            typeof(IAMGraphStreams),
            typeof(IAMLine21Decoder),
            typeof(IAMMediaContent),
            typeof(IAMMediaContent2),
            typeof(IAMMediaStream),
            typeof(IAMMediaTypeSample),
            typeof(IAMMediaTypeStream),
            typeof(IAMMultiMediaStream),
            typeof(IAMOpenProgress),
            typeof(IAMOverlayFX),
            typeof(IAMResourceControl),
            typeof(IAMSetErrorLog),
            typeof(IAMStats),
            typeof(IAMStreamConfig),
            typeof(IAMStreamControl),
            typeof(IAMStreamSelect),
            typeof(IAMTimeline),
            typeof(IAMTimelineComp),
            typeof(IAMTimelineEffect),
            typeof(IAMTimelineEffectable),
            typeof(IAMTimelineGroup),
            typeof(IAMTimelineObj),
            typeof(IAMTimelineSplittable),
            typeof(IAMTimelineSrc),
            typeof(IAMTimelineTrack),
            typeof(IAMTimelineTrans),
            typeof(IAMTimelineTransable),
            typeof(IAMTimelineVirtualTrack),
            typeof(IAMTuner),
            typeof(IAMTunerNotification),
            typeof(IAMTVAudio),
            typeof(IAMTVTuner),
            typeof(IAMVfwCompressDialogs),
            typeof(IAMVideoCompression),
            typeof(IAMVideoControl),
            typeof(IAMVideoDecimationProperties),
            typeof(IAMVideoProcAmp),
            typeof(IAMWstDecoder),
            typeof(IAnalogTVTuningSpace),
            typeof(IAsyncReader),
            typeof(IATSCChannelTuneRequest),
            typeof(IATSCComponentType),
            typeof(IATSCLocator),
            typeof(IATSCTuningSpace),
            typeof(IAudioMediaStream),
            typeof(IAuxInTuningSpace),
            typeof(IBaseFilter),
            typeof(IBasicAudio),
            typeof(IBasicVideo),
            typeof(IBasicVideo2),
            typeof(IBDA_AutoDemodulate),
            typeof(IBDA_DeviceControl),
            typeof(IBDA_DigitalDemodulator),
            typeof(IBDA_EthernetFilter),
            typeof(IBDA_FrequencyFilter),
            typeof(IBDA_IPSinkControl),
            typeof(IBDA_IPSinkInfo),
            typeof(IBDA_IPV4Filter),
            typeof(IBDA_IPV6Filter),
            typeof(IBDA_LNBInfo),
            typeof(IBDA_SignalProperties),
            typeof(IBDA_SignalStatistics),
            typeof(IBDA_Topology),
            typeof(IBDAComparable),
            typeof(IBroadcastEvent),
            typeof(ICaptureGraphBuilder2),
            typeof(IChannelTuneRequest),
            typeof(IComponent),
            typeof(IComponents),
            typeof(IComponentsNew),
            typeof(IComponentType),
            typeof(IComponentTypes),
            typeof(IConfigAsfWriter),
            typeof(IConfigAviMux),
            typeof(IConfigInterleaving),
            typeof(ICreateDevEnum),
            typeof(ICreatePropBagOnRegKey),
            typeof(IDeferredCommand),
            typeof(IDMOVideoOutputOptimizations),
            typeof(IDMOWrapperFilter),
            typeof(IDVBCLocator),
            typeof(IDVBSLocator),
            typeof(IDVBSTuningSpace),
            typeof(IDVBTLocator),
            typeof(IDVBTuneRequest),
            typeof(IDVBTuningSpace),
            typeof(IDVBTuningSpace2),
            typeof(IDvdCmd),
            typeof(IDvdControl2),
            typeof(IDvdGraphBuilder),
            typeof(IDvdInfo2),
            typeof(IDvdState),
            typeof(IDVEnc),
            typeof(IDVRGB219),
            typeof(IDVSplitter),
            typeof(IEnumComponents),
            typeof(IEnumComponentTypes),
            typeof(IEnumDMO),
            typeof(IEnumFilters),
            typeof(IEnumMediaTypes),
            typeof(IEnumPins),
            typeof(IEnumStreamBufferRecordingAttrib),
            typeof(IEnumTuningSpaces),
            typeof(IErrorLog),
            typeof(IFileSinkFilter),
            typeof(IFileSinkFilter2),
            typeof(IFileSourceFilter),
            typeof(IFilterChain),
            typeof(IFilterGraph),
            typeof(IFilterGraph2),
            typeof(IFilterMapper2),
            typeof(IFilterMapper3),
            typeof(IFrequencyMap),
            typeof(IGraphBuilder),
            typeof(IGraphConfig),
            typeof(IGraphConfigCallback),
            typeof(IGraphVersion),
            typeof(IIPDVDec),
            typeof(IKsPin),
            typeof(IKsPropertySet),
            typeof(ILanguageComponentType),
            typeof(ILocator),
            typeof(IMediaBuffer),
            typeof(IMediaControl),
            typeof(IMediaDet),
            typeof(IMediaEvent),
            typeof(IMediaEventEx),
            typeof(IMediaEventSink),
            typeof(IMediaFilter),
            typeof(IMediaLocator),
            typeof(IMediaObject),
            typeof(IMediaObjectInPlace),
            typeof(IMediaParamInfo),
            typeof(IMediaParams),
            typeof(IMediaPosition),
            typeof(IMediaPropertyBag),
            typeof(IMediaSample),
            typeof(IMediaSample2),
            typeof(IMediaSeeking),
            typeof(IMediaStream),
            typeof(IMediaStreamFilter),
            typeof(IMemAllocator),
            typeof(IMemAllocatorCallbackTemp),
            typeof(IMemAllocatorNotifyCallbackTemp),
            typeof(IMixerOCX),
            typeof(IMixerOCXNotify),
            typeof(IMixerPinConfig),
            typeof(IMixerPinConfig2),
            typeof(IMPEG2Component),
            typeof(IMPEG2ComponentType),
            typeof(IMpeg2Data),
            typeof(IMpeg2Demultiplexer),
            typeof(IMPEG2PIDMap),
            typeof(IMpeg2Stream),
            typeof(IMPEG2StreamIdMap),
            typeof(IMPEG2TuneRequest),
            typeof(IMPEG2TuneRequestFactory),
            typeof(IMPEG2TuneRequestSupport),
            typeof(IMpegAudioDecoder),
            typeof(IMultiMediaStream),
            typeof(IObjectWithSite),
            typeof(IPersist),
            typeof(IPersistMediaPropertyBag),
            typeof(IPersistStream),
            typeof(IPin),
            typeof(IPinConnection),
            typeof(IPinFlowControl),
            typeof(IPropertyBag),
            typeof(IPropertySetter),
            typeof(IQualityControl),
            typeof(IQualProp),
            typeof(IQueueCommand),
            typeof(IReferenceClock),
            typeof(IRegisterServiceProvider),
            typeof(IRenderEngine),
            typeof(IRenderEngine2),
            typeof(ISampleGrabber),
            typeof(ISampleGrabberCB),
            typeof(ISectionList),
            typeof(ISeekingPassThru),
            typeof(DirectShowLib.IServiceProvider),
            typeof(ISmartRenderEngine),
            typeof(ISpecifyPropertyPages),
            typeof(IStreamBufferConfigure),
            typeof(IStreamBufferConfigure2),
            typeof(IStreamBufferDataCounters),
            typeof(IStreamBufferInitialize),
            typeof(IStreamBufferMediaSeeking),
            typeof(IStreamBufferMediaSeeking2),
            typeof(IStreamBufferRecComp),
            typeof(IStreamBufferRecordControl),
            typeof(IStreamBufferRecordingAttribute),
            typeof(IStreamBufferSink),
            typeof(IStreamBufferSink2),
            typeof(IStreamBufferSink3),
            typeof(IStreamBufferSource),
            typeof(IStreamSample),
            typeof(ITuner),
            typeof(ITuneRequest),
            typeof(ITuningSpace),
            typeof(ITuningSpaceContainer),
            typeof(ITuningSpaces),
            typeof(IVideoFrameStep),
            typeof(IVideoWindow),
            typeof(IVMRAspectRatioControl),
            typeof(IVMRAspectRatioControl9),
            typeof(IVMRDeinterlaceControl),
            typeof(IVMRDeinterlaceControl9),
            typeof(IVMRFilterConfig),
            typeof(IVMRFilterConfig9),
            typeof(IVMRImageCompositor),
            typeof(IVMRImageCompositor9),
            typeof(IVMRImagePresenter9),
            typeof(IVMRImagePresenterConfig),
            typeof(IVMRImagePresenterConfig9),
            typeof(IVMRMixerBitmap),
            typeof(IVMRMixerBitmap9),
            typeof(IVMRMixerControl),
            typeof(IVMRMixerControl9),
            typeof(IVMRMonitorConfig),
            typeof(IVMRMonitorConfig9),
            typeof(IVMRSurfaceAllocator9),
            typeof(IVMRSurfaceAllocatorEx9),
            typeof(IVMRSurfaceAllocatorNotify9),
            typeof(IVMRVideoStreamControl),
            typeof(IVMRVideoStreamControl9),
            typeof(IVMRWindowlessControl),
            typeof(IVMRWindowlessControl9),
            typeof(IVPBaseNotify),
            typeof(IVPManager),
            typeof(IVPNotify),
            typeof(IVPNotify2),
            typeof(IXml2Dex),
            typeof(IAMCameraControl),
            typeof(IConfigAsfWriter2),
            typeof(IDTFilter),
            typeof(IDTFilterConfig),
            typeof(IETFilterConfig),
            typeof(IEvalRat),
            typeof(IATSCLocator2),
            typeof(IBroadcastEventEx),
            typeof(ICAT),
            typeof(ICCSubStreamFiltering),
            typeof(IDigitalLocator),
            typeof(IDTFilter2),
            typeof(IDTFilter3),
            typeof(IDVB_BAT),
            typeof(IDVB_EIT),
            typeof(IDVB_NIT),
            typeof(IDVB_SDT),
            typeof(IDVB_TOT),
            typeof(IDvbLogicalChannelDescriptor),
            typeof(IDvbSatelliteDeliverySystemDescriptor),
            typeof(IDvbTerrestrialDeliverySystemDescriptor),
            typeof(IFilterGraph3),
            typeof(IGenericDescriptor),
            typeof(IPAT),
            typeof(IPMT),
            typeof(IReferenceClockTimerControl),
            typeof(IStreamBufferConfigure3),
            typeof(ITunerCap),
            typeof(IXDSCodecConfig),
            typeof(IAnalogRadioTuningSpace),
            typeof(IAnalogRadioTuningSpace2),
            typeof(IAuxInTuningSpace2),
            typeof(IKsTopologyInfo)
            };

            List<InterfaceInfo> lst = new List<InterfaceInfo>();
            IntPtr objectPointer = Marshal.GetComInterfaceForObject(basefilter, typeof(IBaseFilter));
            foreach (Type t in ifaces)
            {
                IntPtr pInterface = IntPtr.Zero;
                Guid queryInterface = t.GUID;                
                Marshal.QueryInterface(objectPointer, ref queryInterface, out pInterface);
                if (pInterface != IntPtr.Zero)
                {
                    InterfaceInfo ii = new InterfaceInfo(t.Name);
                    MethodInfo[] mtds = t.GetMethods();
                    foreach (MethodInfo mi in mtds)
                        ii.methods.Add(mi.ToString());
                    lst.Add(ii);
                    Marshal.Release(pInterface);
                }
            }
            Marshal.Release(objectPointer);
            //lst.Sort();
            return lst;
        }

        public string ChooseSrcFileName()
        {
            string ret = null;
            IFileSourceFilter fsrc = basefilter as IFileSourceFilter;
            if (fsrc != null)
            {
                OpenFileDialog fd = new OpenFileDialog();
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
                }
                else
                    if (Program.mainform.suggestURLs)
                    {
                        RenderURLForm rf = new RenderURLForm("Open URL");
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
                        }
                    }
            }
            return ret;
        }

        public string ChooseDstFileName()
        {
            string ret = null;
            IFileSinkFilter fdst = basefilter as IFileSinkFilter;
            if (fdst != null)
            {
                SaveFileDialog fd = new SaveFileDialog();
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
                }
            }
            return ret;
        }

        public void UpdateState()
        {
            basefilter.GetState(20, out filterState);
        }

        public int cellsize { get { return graph.cellsize; }}

    }//class

    class InterfaceInfo
    {
        public string name;
        public List<string> methods;

        public InterfaceInfo(string _name)
        {
            name = _name;
            methods = new List<string>();
        }
    }
}
