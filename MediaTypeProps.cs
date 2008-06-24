using System;
using System.Collections.Generic;
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
        protected AMMediaType mt;

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

        public virtual List<KeyValuePair<string, string>> FormatFields(bool show_zeroes, bool cs_enums)
        {
            return new List<KeyValuePair<string, string>>(); //empty list
        }

        public virtual string FormatClass() { return "?"; }
    }//class

    [TypeConverterAttribute(typeof(ExpandableObjectConverter)),
    ReadOnlyAttribute(true)]
    class MTProps<T> : MediaTypeProps
    {
        T format;
        FieldsToPropertiesProxyTypeDescriptor format_ftp;

        public MTProps(AMMediaType pmt, IntPtr pFormat)
            : base(pmt)
        {
            format = (T)Marshal.PtrToStructure(pFormat, typeof(T));
            format_ftp = new FieldsToPropertiesProxyTypeDescriptor(format);
        }

        [ReadOnlyAttribute(true),
        DescriptionAttribute("Format"),
        TypeConverterAttribute(typeof(ExpandableObjectConverter))]
        public object Format
        {
            get { return format_ftp; }
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2} {3} {4} {5} SampleSize={6}",
                DsToString.MediaTypeToString(mt.majorType).Replace('\0', ' '),
                DsToString.MediaSubTypeToString(mt.subType).Replace('\0', ' '),
                DsToString.MediaFormatTypeToString(mt.formatType).Replace('\0', ' '),
                format_ftp.ToString(),
                (mt.fixedSizeSamples ? "FixedSamples" : "NotFixedSamples"),
                (mt.temporalCompression ? "TemporalCompression" : "NotTemporalCompression"),
                mt.sampleSize.ToString());
        }

        public override string FormatClass() { return typeof(T).Name; }

        public override List<KeyValuePair<string, string>> FormatFields(bool show_zeroes, bool cs_enums) 
        {
            List<KeyValuePair<string, string>> fields = new List<KeyValuePair<string, string>>();
            DumpFields(format, fields, "", show_zeroes, cs_enums);
            return fields;
        }

        void DumpFields(object o, List<KeyValuePair<string, string>> fields, string prefix, bool show_zeroes, bool cs_enums)
        {            
            foreach (FieldInfo field in o.GetType().GetFields())
            {
                object val = field.GetValue(o);
                if (field.FieldType.IsPrimitive)
                {
                    string s = val.ToString().ToLowerInvariant();
                    if (show_zeroes || s!="0")
                        fields.Add(new KeyValuePair<string, string>(prefix + field.Name, s));
                }
                else
                if (field.FieldType.IsEnum)
                {
                    string s = val.ToString();
                    string str;
                    if (cs_enums)
                    {
                        if (s[0] >= '0' && s[0] <= '9')
                            str = string.Format("({1}) 0x{0:X}",  val, field.FieldType.Name);
                        else
                            str = field.FieldType.Name + "." + val.ToString();
                    }
                    else
                        str = string.Format("0x{0:X}", val); // Enum.Format(field.FieldType, val, "X"));

                    fields.Add(new KeyValuePair<string, string>(prefix + field.Name, str));
                }
                else
                {
                    fields.Add(new KeyValuePair<string, string>(prefix + field.Name, "new " + field.FieldType.Name + "()"));
                    DumpFields(val, fields, prefix + field.Name + ".", show_zeroes, cs_enums);
                }               
            }
        }

    } // end of class

} // end of namespace
