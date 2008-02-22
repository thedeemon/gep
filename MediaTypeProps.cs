using System;
using System.Collections.Generic;
using System.Text;
using DirectShowLib;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;

namespace gep
{
    [TypeConverterAttribute(typeof(ExpandableObjectConverter)),
    ReadOnlyAttribute(true)]
    class MediaTypeProps
    {
        AMMediaType mt;

        protected MediaTypeProps(AMMediaType pmt)
        {
            mt = pmt;
        }

        public override string ToString()
        {
            return DsToString.AMMediaTypeToString(mt).Replace('\0', ' ');
        }

        [ReadOnlyAttribute(true),
        DescriptionAttribute("Do all samples have same size?")]
        public bool FixedSizeSamples { get { return mt.fixedSizeSamples; } }

        //[ReadOnlyAttribute(true),
        //DescriptionAttribute("Format")]
        //public string Format { get { return "formad";  } }

        [ReadOnlyAttribute(true),
        DescriptionAttribute("Size of format.")]
        public int FormatSize { get { return mt.formatSize; } }

        [ReadOnlyAttribute(true),
        DescriptionAttribute("Format type guid.")]
        public string FormatType { get { return DsToString.MediaFormatTypeToString(mt.formatType); }}

        [ReadOnlyAttribute(true),
        DescriptionAttribute("Major type.")]
        public string MajorType { get { return DsToString.MediaTypeToString(mt.majorType); } }

        [ReadOnlyAttribute(true),
        DescriptionAttribute("Size of one sample.")]
        public int SampleSize { get { return mt.sampleSize; } }

        [ReadOnlyAttribute(true),
        DescriptionAttribute("Media SubType")]
        public string SubType { get { return DsToString.MediaSubTypeToString(mt.subType); } }

        [ReadOnlyAttribute(true),
        DescriptionAttribute("Media SubType GUID")]
        public string SubTypeGUID { get { return Graph.GuidToString(mt.subType); } }

        [ReadOnlyAttribute(true),
        DescriptionAttribute("Do some samples depend on others?")]
        public bool TemporalCompression { get { return mt.temporalCompression; } }

        public static MediaTypeProps CreateMTProps(AMMediaType pmt)
        {
            if (pmt.formatType==DirectShowLib.FormatType.VideoInfo)
                return new MTProps<VideoInfoHeader>(pmt, pmt.formatPtr);
            if (pmt.formatType == DirectShowLib.FormatType.VideoInfo2)
                return new MTProps<VideoInfoHeader2>(pmt, pmt.formatPtr);
            if (pmt.formatType == DirectShowLib.FormatType.WaveEx)
                return new MTProps<WaveFormatEx>(pmt, pmt.formatPtr);
            if (pmt.formatType == DirectShowLib.FormatType.MpegVideo)
                return new MTProps<MPEG1VideoInfo>(pmt, pmt.formatPtr);
            if (pmt.formatType == DirectShowLib.FormatType.AnalogVideo)
                return new MTProps<AnalogVideoInfo>(pmt, pmt.formatPtr);
            if (pmt.formatType == DirectShowLib.FormatType.DvInfo)
                return new MTProps<DVInfo>(pmt, pmt.formatPtr);
            if (pmt.formatType == DirectShowLib.FormatType.Mpeg2Video)
                return new MTProps<MPEG2VideoInfo>(pmt, pmt.formatPtr);
            //if (pmt.formatType == DirectShowLib.FormatType.None)
            //    return new MTProps<VideoInfoHeader2>(pmt, pmt.formatPtr); //usual mediatype props
            //if (pmt.formatType == DirectShowLib.FormatType.Null)
            //    return new MTProps<VideoInfoHeader2>(pmt, pmt.formatPtr);

            /*if (pmt.formatType == DirectShowLib.FormatType.DolbyAC3) //not described in DX SDK
                return new MTProps<VideoInfoHeader2>(pmt, pmt.formatPtr);
            if (pmt.formatType == DirectShowLib.FormatType.Mpeg2Audio)
                return new MTProps<VideoInfoHeader2>(pmt, pmt.formatPtr);
            if (pmt.formatType == DirectShowLib.FormatType.MpegStreams)
                return new MTProps<VideoInfoHeader2>(pmt, pmt.formatPtr);
            if (pmt.formatType == DirectShowLib.FormatType.WSS525)
                return new MTProps<VideoInfoHeader2>(pmt, pmt.formatPtr);*/

            return new MediaTypeProps(pmt);
        }
    }//class

    [TypeConverterAttribute(typeof(ExpandableObjectConverter)),
    ReadOnlyAttribute(true)]
    class MTProps<T> : MediaTypeProps
    {
        T format;

        public MTProps(AMMediaType pmt, IntPtr pFormat)
            : base(pmt)
        {
            format = (T)Marshal.PtrToStructure(pFormat, typeof(T));
        }

        [ReadOnlyAttribute(true),
        DescriptionAttribute("Format"),
        TypeConverterAttribute(typeof(ExpandableObjectConverter))]
        public object Format
        {
            get { return new FieldsToPropertiesProxyTypeDescriptor(format); }
        }
    }

}
