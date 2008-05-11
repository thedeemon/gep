using System;
using DirectShowLib;
using System.ComponentModel;

namespace gep
{
    [TypeConverterAttribute(typeof(ExpandableObjectConverter)),
    ReadOnlyAttribute(true)]
    class StreamCaps<T>
    {
        MediaTypeProps mt;
        T caps;

        public StreamCaps(AMMediaType _mt, T _caps)
        {
            mt = MediaTypeProps.CreateMTProps(_mt); caps = _caps;
        }

        [ReadOnlyAttribute(true),
        DescriptionAttribute("Media type"),
        TypeConverterAttribute(typeof(ExpandableObjectConverter))]
        public MediaTypeProps MediaType { get { return mt; } }

        [ReadOnlyAttribute(true),
        DescriptionAttribute("Stream config caps"),
        TypeConverterAttribute(typeof(ExpandableObjectConverter))]
        public object ConfigCaps { get { return new FieldsToPropertiesProxyTypeDescriptor(caps); } }

        public override string ToString()
        {
            return mt.ToString();
        }
    }
}
