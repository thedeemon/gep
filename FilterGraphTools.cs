/****************************************************************************
While the underlying libraries are covered by LGPL, this sample is released 
as public domain.  It is distributed in the hope that it will be useful, but 
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
or FITNESS FOR A PARTICULAR PURPOSE.  
*****************************************************************************/

using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Collections.Generic;
using DirectShowLib;
using DirectShowLib.BDA;
using DirectShowLib.DES;
using DirectShowLib.DMO;
using DirectShowLib.Dvd;
using DirectShowLib.MultimediaStreaming;
using DirectShowLib.SBE;
using System.Reflection;
#if !USING_NET11
using System.Runtime.InteropServices.ComTypes;
#endif

namespace gep
{
    class InterfaceInfo
    {
        public string name;
        public List<InterfaceInfo> elements;

        public InterfaceInfo(string _name)
        {
            name = _name;
            elements = new List<InterfaceInfo>();
        }
    }
    /// <summary>
    /// A collection of methods to do common DirectShow tasks.
    /// </summary>
    static class FilterGraphTools
    {
        /// <summary>
        /// Add a filter to a DirectShow Graph using its CLSID
        /// </summary>
        /// <param name="graphBuilder">the IGraphBuilder interface of the graph</param>
        /// <param name="clsid">a valid CLSID. This object must implement IBaseFilter</param>
        /// <param name="name">the name used in the graph (may be null)</param>
        /// <returns>an instance of the filter if the method successfully created it, null if not</returns>
        /// <remarks>
        /// You can use <see cref="IsThisComObjectInstalled">IsThisComObjectInstalled</see> to check is the CLSID is valid before calling this method
        /// </remarks>
        /// <example>This sample shows how to programmatically add a NVIDIA Video decoder filter to a graph
        /// <code>
        /// Guid nvidiaVideoDecClsid = new Guid("71E4616A-DB5E-452B-8CA5-71D9CC7805E9");
        /// 
        /// if (FilterGraphTools.IsThisComObjectInstalled(nvidiaVideoDecClsid))
        /// {
        ///   filter = FilterGraphTools.AddFilterFromClsid(graphBuilder, nvidiaVideoDecClsid, "NVIDIA Video Decoder");
        /// }
        /// else
        /// {
        ///   // use another filter...
        /// }
        /// </code>
        /// </example>
        /// <seealso cref="IsThisComObjectInstalled"/>
        /// <exception cref="System.ArgumentNullException">Thrown if graphBuilder is null</exception>
        /// <exception cref="System.Runtime.InteropServices.COMException">Thrown if errors occur when the filter is add to the graph</exception>

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode=true)]
        public static IBaseFilter AddFilterFromClsid(IGraphBuilder graphBuilder, Guid clsid, string name)
        {
            IBaseFilter filter = null;

            if (graphBuilder == null)
                throw new ArgumentNullException("graphBuilder");

            try
            {
                Type type = Type.GetTypeFromCLSID(clsid);
                filter = (IBaseFilter) Activator.CreateInstance(type);

                int hr = graphBuilder.AddFilter(filter, name);
                DsError.ThrowExceptionForHR(hr);
            }
            catch
            {
                if (filter != null)
                {
                    Marshal.ReleaseComObject(filter);
                    filter = null;
                }
            }

            return filter;
        }

        /// <summary>
        /// Add a filter to a DirectShow Graph using its name
        /// </summary>
        /// <param name="graphBuilder">the IGraphBuilder interface of the graph</param>
        /// <param name="deviceCategory">the filter category (see DirectShowLib.FilterCategory)</param>
        /// <param name="friendlyName">the filter name (case-sensitive)</param>
        /// <returns>an instance of the filter if the method successfully created it, null if not</returns>
        /// <example>This sample shows how to programmatically add a NVIDIA Video decoder filter to a graph
        /// <code>
        /// filter = FilterGraphTools.AddFilterByName(graphBuilder, FilterCategory.LegacyAmFilterCategory, "NVIDIA Video Decoder");
        /// </code>
        /// </example>
        /// <exception cref="System.ArgumentNullException">Thrown if graphBuilder is null</exception>
        /// <exception cref="System.Runtime.InteropServices.COMException">Thrown if errors occur when the filter is add to the graph</exception>

        public static IBaseFilter AddFilterByName(IGraphBuilder graphBuilder, Guid deviceCategory, string friendlyName)
        {
            IBaseFilter filter = null;

            if (graphBuilder == null)
                throw new ArgumentNullException("graphBuilder");

            DsDevice[] devices = DsDevice.GetDevicesOfCat(deviceCategory);

            for(int i = 0; i < devices.Length; i++)
            {
                if (!devices[i].Name.Equals(friendlyName))
                    continue;

                int hr = (graphBuilder as IFilterGraph2).AddSourceFilterForMoniker(devices[i].Mon, null, friendlyName, out filter);
                DsError.ThrowExceptionForHR(hr);

                break;
            }

            return filter;
        }

        /// <summary>
        /// Add a filter to a DirectShow Graph using its Moniker's device path
        /// </summary>
        /// <param name="graphBuilder">the IGraphBuilder interface of the graph</param>
        /// <param name="devicePath">a moniker path</param>
        /// <param name="name">the name to use for the filter in the graph</param>
        /// <returns>an instance of the filter if the method successfully creates it, null if not</returns>
        /// <example>This sample shows how to programmatically add a NVIDIA Video decoder filter to a graph
        /// <code>
        /// string devicePath = @"@device:sw:{083863F1-70DE-11D0-BD40-00A0C911CE86}\{71E4616A-DB5E-452B-8CA5-71D9CC7805E9}";
        /// filter = FilterGraphTools.AddFilterByDevicePath(graphBuilder, devicePath, "NVIDIA Video Decoder");
        /// </code>
        /// </example>
        /// <exception cref="System.ArgumentNullException">Thrown if graphBuilder is null</exception>
        /// <exception cref="System.Runtime.InteropServices.COMException">Thrown if errors occur when the filter is add to the graph</exception>

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode=true)]
        public static IBaseFilter AddFilterByDevicePath(IGraphBuilder graphBuilder, string devicePath, string name)
        {
            IBaseFilter filter = null;
#if USING_NET11
			UCOMIBindCtx bindCtx = null;
			UCOMIMoniker moniker = null;
#else
			IBindCtx bindCtx = null;
			IMoniker moniker = null;
#endif

            if (graphBuilder == null)
                throw new ArgumentNullException("graphBuilder");

            try
            {
                int hr = NativeMethods.CreateBindCtx(0, out bindCtx);
                Marshal.ThrowExceptionForHR(hr);

                int eaten;
                hr = NativeMethods.MkParseDisplayName(bindCtx, devicePath, out eaten, out moniker);
                Marshal.ThrowExceptionForHR(hr);

                hr = (graphBuilder as IFilterGraph2).AddSourceFilterForMoniker(moniker, bindCtx, name, out filter);
                DsError.ThrowExceptionForHR(hr);
            }
            catch
            {
                // An error occur. Just returning null...
            }
            finally
            {
                if (bindCtx != null) Marshal.ReleaseComObject(bindCtx);
                if (moniker != null) Marshal.ReleaseComObject(moniker);
            }

            return filter;
        }

        /// <summary>
        /// Find a filter in a DirectShow Graph using its name
        /// </summary>
        /// <param name="graphBuilder">the IGraphBuilder interface of the graph</param>
        /// <param name="filterName">the filter name to find (case-sensitive)</param>
        /// <returns>an instance of the filter if found, null if not</returns>
        /// <seealso cref="FindFilterByClsid"/>
        /// <exception cref="System.ArgumentNullException">Thrown if graphBuilder is null</exception>

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode=true)]
        public static IBaseFilter FindFilterByName(IGraphBuilder graphBuilder, string filterName)
        {
            IBaseFilter filter = null;
            IEnumFilters enumFilters = null;

            if (graphBuilder == null)
                throw new ArgumentNullException("graphBuilder");

            int hr = graphBuilder.EnumFilters(out enumFilters);
            if (hr == 0)
            {
                IBaseFilter[] filters = new IBaseFilter[1];

                while(enumFilters.Next(filters.Length, filters, IntPtr.Zero) == 0)
                {
                    FilterInfo filterInfo;

                    hr = filters[0].QueryFilterInfo(out filterInfo);
                    if (hr == 0)
                    {
                        if (filterInfo.pGraph != null)
                            Marshal.ReleaseComObject(filterInfo.pGraph);

                        if (filterInfo.achName.Equals(filterName))
                        {
                            filter = filters[0];
                            break;
                        }
                    }

                    Marshal.ReleaseComObject(filters[0]);
                }
                Marshal.ReleaseComObject(enumFilters);
            }

            return filter;
        }

        /// <summary>
        /// Find a filter in a DirectShow Graph using its CLSID
        /// </summary>
        /// <param name="graphBuilder">the IGraphBuilder interface of the graph</param>
        /// <param name="filterClsid">the CLSID to find</param>
        /// <returns>an instance of the filter if found, null if not</returns>
        /// <seealso cref="FindFilterByName"/>
        /// <exception cref="System.ArgumentNullException">Thrown if graphBuilder is null</exception>

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode=true)]
        public static IBaseFilter FindFilterByClsid(IGraphBuilder graphBuilder, Guid filterClsid)
        {
            IBaseFilter filter = null;
            IEnumFilters enumFilters;

            if (graphBuilder == null)
                throw new ArgumentNullException("graphBuilder");

            int hr = graphBuilder.EnumFilters(out enumFilters);
            if (hr == 0)
            {
                IBaseFilter[] filters = new IBaseFilter[1];

                while(enumFilters.Next(filters.Length, filters, IntPtr.Zero) == 0)
                {
                    Guid clsid;

                    hr = filters[0].GetClassID(out clsid);

                    if ((hr == 0) && (clsid == filterClsid))
                    {
                        filter = filters[0];
                        break;
                    }

                    Marshal.ReleaseComObject(filters[0]);
                }
                Marshal.ReleaseComObject(enumFilters);
            }

            return filter;
        }

        /// <summary>
        /// Render a filter's pin in a DirectShow Graph
        /// </summary>
        /// <param name="graphBuilder">the IGraphBuilder interface of the graph</param>
        /// <param name="source">the filter containing the pin to render</param>
        /// <param name="pinName">the pin name</param>
        /// <returns>true if rendering is a success, false if not</returns>
        /// <example>
        /// <code>
        /// hr = graphBuilder.AddSourceFilter(@"foo.avi", "Source Filter", out filter);
        /// DsError.ThrowExceptionForHR(hr);
        /// 
        /// if (!FilterGraphTools.RenderPin(graphBuilder, filter, "Output"))
        /// {
        ///   // Something went wrong...
        /// }
        /// </code>
        /// </example>
        /// <exception cref="System.ArgumentNullException">Thrown if graphBuilder or source is null</exception>
        /// <remarks>This method assumes that the filter is part of the given graph</remarks>
    
        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode=true)]
        public static bool RenderPin(IGraphBuilder graphBuilder, IBaseFilter source, string pinName)
        {
            if (graphBuilder == null)
                throw new ArgumentNullException("graphBuilder");

            if (source == null)
                throw new ArgumentNullException("source");

            IPin pin = DsFindPin.ByName(source, pinName);

            if (pin != null)
            {
                int hr = graphBuilder.Render(pin);
                Marshal.ReleaseComObject(pin);

                return (hr >= 0);
            }

            return false;
        }

        /// <summary>
        /// Disconnect all pins on a given filter
        /// </summary>
        /// <param name="filter">the filter on which to disconnect all the pins</param>
        /// <exception cref="System.ArgumentNullException">Thrown if filter is null</exception>
        /// <exception cref="System.Runtime.InteropServices.COMException">Thrown if errors occured during the disconnection process</exception>
        /// <remarks>Both input and output pins are disconnected</remarks>

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode=true)]
        public static void DisconnectPins(IBaseFilter filter)
        {
            if (filter == null)
                throw new ArgumentNullException("filter");

            IEnumPins enumPins;
            IPin[] pins = new IPin[1];

            int hr = filter.EnumPins(out enumPins);
            DsError.ThrowExceptionForHR(hr);

            try
            {
                while(enumPins.Next(pins.Length, pins, IntPtr.Zero) == 0)
                {
                    try
                    {
                        hr = pins[0].Disconnect();
                        DsError.ThrowExceptionForHR(hr);
                    }
                    finally
                    {
                        Marshal.ReleaseComObject(pins[0]);
                    }
                }
            }
            finally
            {
                Marshal.ReleaseComObject(enumPins);
            }
        }

        /// <summary>
        /// Disconnect pins of all the filters in a DirectShow Graph
        /// </summary>
        /// <param name="graphBuilder">the IGraphBuilder interface of the graph</param>
        /// <exception cref="System.ArgumentNullException">Thrown if graphBuilder is null</exception>
        /// <exception cref="System.Runtime.InteropServices.COMException">Thrown if the method can't enumerate its filters</exception>
        /// <remarks>This method doesn't throw an exception if an error occurs during pin disconnections</remarks>
    
        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode=true)]
        public static void DisconnectAllPins(IGraphBuilder graphBuilder)
        {
            IEnumFilters enumFilters;

            if (graphBuilder == null)
                throw new ArgumentNullException("graphBuilder");

            int hr = graphBuilder.EnumFilters(out enumFilters);
            DsError.ThrowExceptionForHR(hr);

            try
            {
                IBaseFilter[] filters = new IBaseFilter[1];

                while(enumFilters.Next(filters.Length, filters, IntPtr.Zero) == 0)
                {
                    try
                    {
                        DisconnectPins(filters[0]);
                    }
                    catch{}
                    Marshal.ReleaseComObject(filters[0]);
                }
            }
            finally
            {
                Marshal.ReleaseComObject(enumFilters);
            }
        }

        /// <summary>
        /// Remove and release all filters from a DirectShow Graph
        /// </summary>
        /// <param name="graphBuilder">the IGraphBuilder interface of the graph</param>
        /// <exception cref="System.ArgumentNullException">Thrown if graphBuilder is null</exception>
        /// <exception cref="System.Runtime.InteropServices.COMException">Thrown if the method can't enumerate its filters</exception>
    
        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode=true)]
        public static void RemoveAllFilters(IGraphBuilder graphBuilder)
        {
            IEnumFilters enumFilters;
            ArrayList filtersArray = new ArrayList();

            if (graphBuilder == null)
                throw new ArgumentNullException("graphBuilder");

            int hr = graphBuilder.EnumFilters(out enumFilters);
            DsError.ThrowExceptionForHR(hr);

            try
            {
                IBaseFilter[] filters = new IBaseFilter[1];

                while(enumFilters.Next(filters.Length, filters, IntPtr.Zero) == 0)
                {
                    filtersArray.Add(filters[0]);
                }
            }
            finally
            {
                Marshal.ReleaseComObject(enumFilters);
            }

            foreach(IBaseFilter filter in filtersArray)
            {
                graphBuilder.RemoveFilter(filter);
                Marshal.ReleaseComObject(filter);
            }
        }

        /// <summary>
        /// Save a DirectShow Graph to a GRF file
        /// </summary>
        /// <param name="graphBuilder">the IGraphBuilder interface of the graph</param>
        /// <param name="fileName">the file to be saved</param>
        /// <exception cref="System.ArgumentNullException">Thrown if graphBuilder is null</exception>
        /// <exception cref="System.Runtime.InteropServices.COMException">Thrown if errors occur during the file creation</exception>
        /// <seealso cref="LoadGraphFile"/>
        /// <remarks>This method overwrites any existing file</remarks>
    
        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode=true)]
        public static void SaveGraphFile(IGraphBuilder graphBuilder, string fileName)
        {
            IStorage storage = null;
#if USING_NET11
            UCOMIStream stream = null;
#else
            IStream stream = null;
#endif

            if (graphBuilder == null)
                throw new ArgumentNullException("graphBuilder");

            try
            {
                int hr = NativeMethods.StgCreateDocfile(
                    fileName, 
                    STGM.Create | STGM.Transacted | STGM.ReadWrite | STGM.ShareExclusive,
                    0,
                    out storage
                    );

                Marshal.ThrowExceptionForHR(hr);

                hr = storage.CreateStream(
                    @"ActiveMovieGraph",
                    STGM.Write | STGM.Create | STGM.ShareExclusive,
                    0,
                    0,
                    out stream
                    );

                Marshal.ThrowExceptionForHR(hr);

                hr = (graphBuilder as IPersistStream).Save(stream, true);
                Marshal.ThrowExceptionForHR(hr);

                hr = storage.Commit(STGC.Default);
                Marshal.ThrowExceptionForHR(hr);
            }
            finally
            {
                if (stream != null)
                    Marshal.ReleaseComObject(stream);
                if (storage != null)
                    Marshal.ReleaseComObject(storage);
            }
        }

        /// <summary>
        /// Load a DirectShow Graph from a file
        /// </summary>
        /// <param name="graphBuilder">the IGraphBuilder interface of the graph</param>
        /// <param name="fileName">the file to be loaded</param>
        /// <exception cref="System.ArgumentNullException">Thrown if graphBuilder is null</exception>
        /// <exception cref="System.ArgumentException">Thrown if the given file is not a valid graph file</exception>
        /// <exception cref="System.Runtime.InteropServices.COMException">Thrown if errors occur during loading</exception>
        /// <seealso cref="SaveGraphFile"/>
    
        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode=true)]
        public static void LoadGraphFile(IGraphBuilder graphBuilder, string fileName)
        {
            IStorage storage = null;
#if USING_NET11
			UCOMIStream stream = null;
#else
			IStream stream = null;
#endif

            if (graphBuilder == null)
                throw new ArgumentNullException("graphBuilder");

            try
            {
                if (NativeMethods.StgIsStorageFile(fileName) != 0)
                    throw new ArgumentException();

                int hr = NativeMethods.StgOpenStorage(
                    fileName,
                    null,
                    STGM.Transacted | STGM.Read | STGM.ShareDenyWrite,
                    IntPtr.Zero,
                    0,
                    out storage
                    );

                Marshal.ThrowExceptionForHR(hr);

                hr = storage.OpenStream(
                    @"ActiveMovieGraph",
                    IntPtr.Zero,
                    STGM.Read | STGM.ShareExclusive,
                    0,
                    out stream
                    );

                Marshal.ThrowExceptionForHR(hr);

                hr = (graphBuilder as IPersistStream).Load(stream);
                Marshal.ThrowExceptionForHR(hr);
            }
            finally
            {
                if (stream != null)
                    Marshal.ReleaseComObject(stream);
                if (storage != null)
                    Marshal.ReleaseComObject(storage);
            }
        }

        /// <summary>
        /// Check if a DirectShow filter can display Property Pages
        /// </summary>
        /// <param name="filter">A DirectShow Filter</param>
        /// <exception cref="System.ArgumentNullException">Thrown if filter is null</exception>
        /// <seealso cref="ShowFilterPropertyPage"/>
        /// <returns>true if the filter has Property Pages, false if not</returns>
        /// <remarks>
        /// This method is intended to be used with <see cref="ShowFilterPropertyPage">ShowFilterPropertyPage</see>
        /// </remarks>
    
        public static bool HasPropertyPages(IBaseFilter filter)
        {
            if (filter == null)
                throw new ArgumentNullException("filter");

            return ((filter as ISpecifyPropertyPages) != null);
        }

        /// <summary>
        /// Display Property Pages of a given DirectShow filter
        /// </summary>
        /// <param name="filter">A DirectShow Filter</param>
        /// <param name="parent">A hwnd handle of a window to contain the pages</param>
        /// <exception cref="System.ArgumentNullException">Thrown if filter is null</exception>
        /// <seealso cref="HasPropertyPages"/>
        /// <remarks>
        /// You can check if a filter supports Property Pages with the <see cref="HasPropertyPages">HasPropertyPages</see> method.<br/>
        /// <strong>Warning</strong> : This method is blocking. It only returns when the Property Pages are closed.
        /// </remarks>
        /// <example>This sample shows how to check if a filter supports Property Pages and displays them
        /// <code>
        /// if (FilterGraphTools.HasPropertyPages(myFilter))
        /// {
        ///   FilterGraphTools.ShowFilterPropertyPage(myFilter, myForm.Handle);
        /// }
        /// </code>
        /// </example>
    
        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode=true)]
        public static void ShowFilterPropertyPage(IBaseFilter filter, IntPtr parent)
        {
            FilterInfo filterInfo;
            DsCAUUID caGuid;
            object[] objs;

            if (filter == null)
                throw new ArgumentNullException("filter");

            if (HasPropertyPages(filter))
            {
                int hr = filter.QueryFilterInfo(out filterInfo);
                DsError.ThrowExceptionForHR(hr);

                if (filterInfo.pGraph != null)
                    Marshal.ReleaseComObject(filterInfo.pGraph);

                hr = (filter as ISpecifyPropertyPages).GetPages(out caGuid);
                DsError.ThrowExceptionForHR(hr);

                try
                {
                    objs = new object[1];
                    objs[0] = filter;

                    NativeMethods.OleCreatePropertyFrame(
                        parent, 0, 0, 
                        filterInfo.achName, 
                        objs.Length, objs, 
                        caGuid.cElems, caGuid.pElems, 
                        0, 0, 
                        IntPtr.Zero
                        );
                }
                finally
                {
                    Marshal.FreeCoTaskMem(caGuid.pElems);
                }
            }
        }

        /// <summary>
        /// Check if a COM Object is available
        /// </summary>
        /// <param name="clsid">The CLSID of this object</param>
        /// <example>This sample shows how to check if the MPEG-2 Demultiplexer filter is available
        /// <code>
        /// if (FilterGraphTools.IsThisComObjectInstalled(typeof(MPEG2Demultiplexer).GUID))
        /// {
        ///   // Use it...
        /// }
        /// </code>
        /// </example>
        /// <returns>true if the object is available, false if not</returns>
    
        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode=true)]
        public static bool IsThisComObjectInstalled(Guid clsid)
        {
            bool retval = false;

            try
            {
                Type type = Type.GetTypeFromCLSID(clsid);
                object o = Activator.CreateInstance(type);
                retval = true;
                Marshal.ReleaseComObject(o);
            }
            catch{}

            return retval;
        }

        /// <summary>
        /// Check if the Video Mixing Renderer 9 Filter is available
        /// <seealso cref="IsThisComObjectInstalled"/>
        /// </summary>
        /// <remarks>
        /// This method uses <see cref="IsThisComObjectInstalled">IsThisComObjectInstalled</see> internally
        /// </remarks>
        /// <returns>true if VMR9 is present, false if not</returns>
    
        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode=true)]
        public static bool IsVMR9Present()
        {
            return IsThisComObjectInstalled(typeof(VideoMixingRenderer9).GUID);
        }

        /// <summary>
        /// Check if the Video Mixing Renderer 7 Filter is available
        /// <seealso cref="IsThisComObjectInstalled"/>
        /// </summary>
        /// <remarks>
        /// This method uses <see cref="IsThisComObjectInstalled">IsThisComObjectInstalled</see> internally
        /// </remarks>
        /// <returns>true if VMR7 is present, false if not</returns>
    
        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode=true)]
        public static bool IsVMR7Present()
        {
            return IsThisComObjectInstalled(typeof(VideoMixingRenderer).GUID);
        }

        /// <summary>
        /// Connect pins from two filters
        /// </summary>
        /// <param name="graphBuilder">the IGraphBuilder interface of the graph</param>
        /// <param name="upFilter">the upstream filter</param>
        /// <param name="sourcePinName">the upstream filter pin name</param>
        /// <param name="downFilter">the downstream filter</param>
        /// <param name="destPinName">the downstream filter pin name</param>
        /// <param name="useIntelligentConnect">indicate if the method should use DirectShow's Intelligent Connect</param>
        /// <exception cref="System.ArgumentNullException">Thrown if graphBuilder, upFilter or downFilter are null</exception>
        /// <exception cref="System.ArgumentException">Thrown if pin names are not found in filters</exception>
        /// <exception cref="System.Runtime.InteropServices.COMException">Thrown if pins can't connect</exception>
        /// <remarks>
        /// If useIntelligentConnect is true, this method can add missing filters between the two pins.<br/>
        /// If useIntelligentConnect is false, this method works only if the two media types are compatible.
        /// </remarks>

        public static void ConnectFilters(IGraphBuilder graphBuilder, IBaseFilter upFilter, string sourcePinName, IBaseFilter downFilter, string destPinName, bool useIntelligentConnect)
        {
            if (graphBuilder == null)
                throw new ArgumentNullException("graphBuilder");

            if (upFilter == null)
                throw new ArgumentNullException("upFilter");

            if (downFilter == null)
                throw new ArgumentNullException("downFilter");

            IPin sourcePin = DsFindPin.ByName(upFilter, sourcePinName);
            if (sourcePin == null)
                throw new ArgumentException("The source filter has no pin called : " + sourcePinName, sourcePinName);

            IPin destPin = DsFindPin.ByName(downFilter, destPinName);
            if (destPin == null)
                throw new ArgumentException("The downstream filter has no pin called : " + destPinName, destPinName);

            try
            {
                ConnectFilters(graphBuilder, sourcePin, destPin, useIntelligentConnect);
            }
            finally
            {
                Marshal.ReleaseComObject(sourcePin);
                Marshal.ReleaseComObject(destPin);
            }
        }

        /// <summary>
        /// Connect pins from two filters
        /// </summary>
        /// <param name="graphBuilder">the IGraphBuilder interface of the graph</param>
        /// <param name="sourcePin">the source (upstream / output) pin</param>
        /// <param name="destPin">the destination (downstream / input) pin</param>
        /// <param name="useIntelligentConnect">indicates if the method should use DirectShow's Intelligent Connect</param>
        /// <exception cref="System.ArgumentNullException">Thrown if graphBuilder, sourcePin or destPin are null</exception>
        /// <exception cref="System.Runtime.InteropServices.COMException">Thrown if pins can't connect</exception>
        /// <remarks>
        /// If useIntelligentConnect is true, this method can add missing filters between the two pins.<br/>
        /// If useIntelligentConnect is false, this method works only if the two media types are compatible.
        /// </remarks>

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode=true)]
        public static void ConnectFilters(IGraphBuilder graphBuilder, IPin sourcePin, IPin destPin, bool useIntelligentConnect)
        {
            if (graphBuilder == null)
                throw new ArgumentNullException("graphBuilder");

            if (sourcePin == null)
                throw new ArgumentNullException("sourcePin");

            if (destPin == null)
                throw new ArgumentNullException("destPin");

            int hr;
            if (useIntelligentConnect)
            {
                hr = graphBuilder.Connect(sourcePin, destPin);
                DsError.ThrowExceptionForHR(hr);
            }
            else
            {
                hr = graphBuilder.ConnectDirect(sourcePin, destPin, null);
                DsError.ThrowExceptionForHR(hr);
            }
        }

        public static List<InterfaceInfo> ScanInterfaces(object scannedObject) //scannedObject - ibasefilter or ipin
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
            typeof(IMemInputPin),
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
            IntPtr objectPointer = Marshal.GetIUnknownForObject(scannedObject); 
            foreach (Type t in ifaces)
            {
                IntPtr pInterface;
                Guid queryInterface = t.GUID;
                Marshal.QueryInterface(objectPointer, ref queryInterface, out pInterface);
                if (pInterface == IntPtr.Zero) continue;
                InterfaceInfo ii = new InterfaceInfo(t.Name);
                MethodInfo[] mtds = t.GetMethods();
                foreach (MethodInfo mi in mtds)
                {
                    InterfaceInfo mtd = new InterfaceInfo(mi.ToString());
                    if (mi.Name.StartsWith("Get") || mi.Name.StartsWith("get_"))
                    {
                        ParameterInfo[] pars = mi.GetParameters();
                        if (Array.TrueForAll(pars, delegate(ParameterInfo pi) { return pi.IsOut; }))
                        {
                            if (pars.Length > 0)
                            {
                                object[] parvals = new object[pars.Length];
                                object res = mi.Invoke(scannedObject, parvals);
                                if (res != null)
                                    mtd.elements.Add(new InterfaceInfo("returns " + res.ToString()));
                                for(int i=0;i<pars.Length;i++)
                                    if (parvals[i]!=null)
                                        mtd.elements.Add(new InterfaceInfo(pars[i].Name+" = "+parvals[i].ToString()));
                            }
                            else
                            {
                                object res = mi.Invoke(scannedObject, null);
                                if (res != null)
                                    mtd.elements.Add(new InterfaceInfo("returns " + res.ToString()));
                            }
                        }
                    }
                    ii.elements.Add(mtd);
                }
                lst.Add(ii);
                Marshal.Release(pInterface);
            }
            Marshal.Release(objectPointer);
            return lst;
        }

        [System.Runtime.InteropServices.DllImport("OLE32.DLL", EntryPoint = "CreateStreamOnHGlobal")] // Create a COM stream from a pointer in unmanaged memory
        extern public static int CreateStreamOnHGlobal(IntPtr ptr, bool delete, out IStream pOutStm);
    }

    #region Unmanaged Code declarations

    [Flags]
    internal enum STGM
    {
        Read = 0x00000000,
        Write = 0x00000001,
        ReadWrite = 0x00000002,
        ShareDenyNone = 0x00000040,
        ShareDenyRead = 0x00000030,
        ShareDenyWrite = 0x00000020,
        ShareExclusive = 0x00000010,
        Priority = 0x00040000,
        Create = 0x00001000,
        Convert = 0x00020000,
        FailIfThere = 0x00000000,
        Direct = 0x00000000,
        Transacted = 0x00010000,
        NoScratch = 0x00100000,
        NoSnapShot = 0x00200000,
        Simple = 0x08000000,
        DirectSWMR = 0x00400000,
        DeleteOnRelease = 0x04000000,
    }

    [Flags]
    internal enum STGC
    {
        Default        = 0,
        Overwrite      = 1,
        OnlyIfCurrent  = 2,
        DangerouslyCommitMerelyToDiskCache = 4,
        Consolidate    = 8
    }

    [Guid("0000000b-0000-0000-C000-000000000046"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IStorage
    {
        [PreserveSig]
        int CreateStream(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pwcsName,
            [In] STGM grfMode,
            [In] int reserved1,
            [In] int reserved2,
#if USING_NET11
			[Out] out UCOMIStream ppstm
#else
			[Out] out IStream ppstm
#endif
            );

        [PreserveSig]
        int OpenStream(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pwcsName,
            [In] IntPtr reserved1,
            [In] STGM grfMode,
            [In] int reserved2,
#if USING_NET11
			[Out] out UCOMIStream ppstm
#else
			[Out] out IStream ppstm
#endif
			);

        [PreserveSig]
        int CreateStorage(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pwcsName,
            [In] STGM grfMode,
            [In] int reserved1,
            [In] int reserved2,
            [Out] out IStorage ppstg
            );

        [PreserveSig]
        int OpenStorage(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pwcsName,
            [In] IStorage pstgPriority,
            [In] STGM grfMode,
            [In] int snbExclude,
            [In] int reserved,
            [Out] out IStorage ppstg
            );

        [PreserveSig]
        int CopyTo(
            [In] int ciidExclude,
            [In] Guid[] rgiidExclude,
            [In] string[] snbExclude,
            [In] IStorage pstgDest
            );

        [PreserveSig]
        int MoveElementTo(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pwcsName,
            [In] IStorage pstgDest,
            [In, MarshalAs(UnmanagedType.LPWStr)] string pwcsNewName,
            [In] STGM grfFlags
            );

        [PreserveSig]
        int Commit([In] STGC grfCommitFlags);

        [PreserveSig]
        int Revert();

        [PreserveSig]
        int EnumElements(
            [In] int reserved1, 
            [In] IntPtr reserved2, 
            [In] int reserved3, 
            [Out, MarshalAs(UnmanagedType.Interface)] out object ppenum
            );

        [PreserveSig]
        int DestroyElement([In, MarshalAs(UnmanagedType.LPWStr)] string pwcsName);

        [PreserveSig]
        int RenameElement(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pwcsOldName, 
            [In, MarshalAs(UnmanagedType.LPWStr)] string pwcsNewName
            );

        [PreserveSig]
        int SetElementTimes(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pwcsName, 
#if USING_NET11
			[In] FILETIME pctime,
			[In] FILETIME patime,
			[In] FILETIME pmtime
#else
			[In] System.Runtime.InteropServices.ComTypes.FILETIME pctime,
            [In] System.Runtime.InteropServices.ComTypes.FILETIME patime,
            [In] System.Runtime.InteropServices.ComTypes.FILETIME pmtime
#endif
			);

        [PreserveSig]
        int SetClass([In, MarshalAs(UnmanagedType.LPStruct)] Guid clsid);

        [PreserveSig]
        int SetStateBits(
            [In] int grfStateBits, 
            [In] int grfMask
            );

        [PreserveSig]
        int Stat(
#if USING_NET11
			[Out] out STATSTG pStatStg, 
#else
			[Out] out System.Runtime.InteropServices.ComTypes.STATSTG pStatStg, 
#endif
			[In] int grfStatFlag
            );
    }

    internal static class NativeMethods
    {
        [DllImport("ole32.dll")]
#if USING_NET11
		public static extern int CreateBindCtx(int reserved, out UCOMIBindCtx ppbc);
#else
		public static extern int CreateBindCtx(int reserved, out IBindCtx ppbc);
#endif

        [DllImport("ole32.dll")]
#if USING_NET11
		public static extern int MkParseDisplayName(UCOMIBindCtx pcb, [MarshalAs(UnmanagedType.LPWStr)] string szUserName, out int pchEaten, out UCOMIMoniker ppmk);
#else
		public static extern int MkParseDisplayName(IBindCtx pcb, [MarshalAs(UnmanagedType.LPWStr)] string szUserName, out int pchEaten, out IMoniker ppmk);
#endif

        [DllImport("oleaut32.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
        public static extern int OleCreatePropertyFrame(
            [In] IntPtr hwndOwner, 
            [In] int x, 
            [In] int y,
            [In, MarshalAs(UnmanagedType.LPWStr)] string lpszCaption, 
            [In] int cObjects,
            [In, MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.IUnknown)] object[] ppUnk,
            [In] int cPages,	
            [In] IntPtr pPageClsID, 
            [In] int lcid, 
            [In] int dwReserved, 
            [In] IntPtr pvReserved 
            );

        [DllImport("ole32.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
        public static extern int StgCreateDocfile(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pwcsName,
            [In] STGM grfMode,
            [In] int reserved,
            [Out] out IStorage ppstgOpen
            );

        [DllImport("ole32.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
        public static extern int StgIsStorageFile([In, MarshalAs(UnmanagedType.LPWStr)] string pwcsName);

        [DllImport("ole32.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
        public static extern int StgOpenStorage(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pwcsName,
            [In] IStorage pstgPriority,
            [In] STGM grfMode,
            [In] IntPtr snbExclude,
            [In] int reserved,
            [Out] out IStorage ppstgOpen
            );

    }
    #endregion
    
    //MIDL_INTERFACE("56a8689d-0ad4-11ce-b03a-0020af0ba770")
    //IMemInputPin : public IUnknown
    //{
    //public:
    //    virtual HRESULT STDMETHODCALLTYPE GetAllocator( 
    //        /* [out] */ IMemAllocator **ppAllocator) = 0;
        
    //    virtual HRESULT STDMETHODCALLTYPE NotifyAllocator( 
    //        /* [in] */ IMemAllocator *pAllocator,
    //        /* [in] */ BOOL bReadOnly) = 0;
        
    //    virtual HRESULT STDMETHODCALLTYPE GetAllocatorRequirements( 
    //        /* [out] */ ALLOCATOR_PROPERTIES *pProps) = 0;
        
    //    virtual HRESULT STDMETHODCALLTYPE Receive( 
    //        /* [in] */ IMediaSample *pSample) = 0;
        
    //    virtual HRESULT STDMETHODCALLTYPE ReceiveMultiple( 
    //        /* [size_is][in] */ IMediaSample **pSamples,
    //        /* [in] */ long nSamples,
    //        /* [out] */ long *nSamplesProcessed) = 0;
        
    //    virtual HRESULT STDMETHODCALLTYPE ReceiveCanBlock( void) = 0;
        
    //};
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("56a8689d-0ad4-11ce-b03a-0020af0ba770"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMemInputPin
    {
        [PreserveSig]
        int GetAllocator([Out] out IMemAllocator ppAllocator);

        [PreserveSig]
        int NotifyAllocator(
            [In] IMemAllocator pAllocator,
            [In, MarshalAs(UnmanagedType.Bool)] bool bReadOnly
            );

        [PreserveSig]
        int GetAllocatorRequirements([Out] out AllocatorProperties pProps);

        [PreserveSig]
        int Receive([In] IMediaSample pSample);

        [PreserveSig]
        int ReceiveMultiple(
            [In] IntPtr pSamples, // IMediaSample[]
            [In] int nSamples,
            [Out] out int nSamplesProcessed
            );

        [PreserveSig]
        int ReceiveCanBlock();
    }

}
