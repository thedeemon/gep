using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Reflection;
using System.Threading;

namespace gep
{
    class FieldsToPropertiesProxyTypeDescriptor : ICustomTypeDescriptor
    {
        private object _target;

        public FieldsToPropertiesProxyTypeDescriptor(object target)
        {
            if (target == null) throw new ArgumentNullException("target");
            _target = target;
        }

        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
        {
            return _target;
        }

        AttributeCollection ICustomTypeDescriptor.GetAttributes()
        {
            return TypeDescriptor.GetAttributes(_target, true);
        }

        string ICustomTypeDescriptor.GetClassName()
        {
            return TypeDescriptor.GetClassName(_target, true);
        }

        string ICustomTypeDescriptor.GetComponentName()
        {
            return TypeDescriptor.GetComponentName(_target, true);
        }

        TypeConverter ICustomTypeDescriptor.GetConverter()
        {
            return TypeDescriptor.GetConverter(_target, true);
        }

        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(_target, true);
        }

        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
        {
            return null;// TypeDescriptor.GetDefaultProperty(_target, true);
        }

        object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
        {
            return null;// TypeDescriptor.GetEditor(_target, editorBaseType, true);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(_target, attributes, true);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
        {
            return TypeDescriptor.GetEvents(_target, true);
        }

        private PropertyDescriptorCollection _propCache;

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
        {
            return ((ICustomTypeDescriptor)this).GetProperties(null);
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
        {
            PropertyDescriptorCollection props = _propCache;
            if (props != null)
                return props;

            // Create the property collection and filter if necessary
            props = new PropertyDescriptorCollection(null);
            //foreach (PropertyDescriptor prop in TypeDescriptor.GetProperties(_target, attributes, true))
            //{
            //    props.Add(prop);
            //}
            foreach (FieldInfo field in _target.GetType().GetFields())
            {
                Attribute[] attrs = (Attribute[])field.GetCustomAttributes(typeof(Attribute), true);
                int n = attrs.Length;
                PropertyDescriptor fieldDesc;
                if (field.FieldType.IsPrimitive || field.FieldType.IsEnum || field.FieldType == typeof(Guid)
                    || field.FieldType == typeof(System.Drawing.Size))
                {
                    Array.Resize<Attribute>(ref attrs, n + 1);
                    attrs[n] = new ReadOnlyAttribute(true);
                    object fld_value = field.GetValue(_target);
                    string str = fld_value.ToString();
                    if ((field.FieldType != typeof(Guid)) && (fld_value is IFormattable) && (str.Length > 1)) {
                        StringBuilder sb = new StringBuilder();
                        IFormattable frm = (IFormattable)fld_value;
                        string hex = frm.ToString("X", Thread.CurrentThread.CurrentCulture);
                        if (field.Name == "Compression")
                        {
                            UInt32 ival = UInt32.Parse(str);
                            char[] fourcc = new char[4];
                            fourcc[0] = (char)(ival & 0xFF);
                            fourcc[1] = (char)((ival >> 8) & 0xFF);
                            fourcc[2] = (char)((ival >> 16) & 0xFF);
                            fourcc[3] = (char)((ival >> 24) & 0xFF);
                            sb.Append("'");
                            sb.Append(fourcc);
                            sb.Append("' "+str+" (0x" + hex + ")");
                        }
                        else                        
                            sb.Append(str + " (0x" + hex + ")");
                        str = sb.ToString();
                    }                    
                    fieldDesc = new CustomFieldPropertyDescriptor(str, attrs, field.Name, _target.GetType());
                    //fieldDesc = new FieldPropertyDescriptor(field, attrs);
                }
                else
                {
                    Array.Resize<Attribute>(ref attrs, n + 2);
                    attrs[n] = new ReadOnlyAttribute(true);
                    attrs[n + 1] = new TypeConverterAttribute(typeof(ExpandableObjectConverter));
                    object fld_value = new FieldsToPropertiesProxyTypeDescriptor(field.GetValue(_target));
                    fieldDesc = new CustomFieldPropertyDescriptor(fld_value, attrs, field.Name, _target.GetType());
                }
                props.Add(fieldDesc);
            }
            _propCache = props;
            return props;
        }

        public override string ToString()
        {
            //return _target.GetType().Name;
            Dictionary<string, string> fields = new Dictionary<string, string>();
            DumpProps(this as ICustomTypeDescriptor, fields, "");
            StringBuilder sb = new StringBuilder();
            sb.Append(fval(fields, "{0}", "BmiHeader.Width"));
            sb.Append(fval(fields, "x{0} ", "BmiHeader.Height"));
            sb.Append(fval(fields, "{0} bit ", "BmiHeader.BitCount"));
            sb.Append(fval(fields, "ImgSz={0} ", "BmiHeader.ImageSize"));
            sb.Append(fval(fields, "TimePerFrame={0} ", "AvgTimePerFrame"));
            sb.Append(fval(fields, "Aspect={0}", "PictAspectRatioX"));
            sb.Append(fval(fields, "x{0} ", "PictAspectRatioY"));
            sb.Append(fval(fields, "{0} Hz ", "nSamplesPerSec"));
            sb.Append(fval(fields, "{0} bit ", "wBitsPerSample"));
            sb.Append(fval(fields, "{0} channels ", "nChannels"));
            sb.Append(fval(fields, "nBlockAlign={0} ", "nBlockAlign"));
            sb.Append(fval(fields, "Bytes/Sec={0} ", "nAvgBytesPerSec"));
            sb.Append(fval(fields, "Tag={0} ", "wFormatTag", false));
            if (sb.Length < 1) sb.Append(_target.GetType().Name);
            return sb.ToString();
        }

        string fval(Dictionary<string, string> fields, string fmt, string key, bool strip)
        {
            string val;
            if (fields.TryGetValue(key, out val))
            {
                int i = val.IndexOf(' ');
                if (i >= 0 && strip) val = val.Remove(i);
                return string.Format(fmt, val);
            }
            return "";
        }

        string fval(Dictionary<string, string> fields, string fmt, string key)
        {
            return fval(fields, fmt, key, true);
        }

        public void DumpProps(ICustomTypeDescriptor td, Dictionary<string, string> fields, string prefix)
        {
            foreach (PropertyDescriptor pd in td.GetProperties())
            {
                object val = pd.GetValue(td);
                if (val is ICustomTypeDescriptor)
                    DumpProps(val as ICustomTypeDescriptor, fields, prefix + pd.DisplayName + ".");
                else
                    fields.Add(prefix + pd.DisplayName, val.ToString());
            }
        }

    }

    class FieldPropertyDescriptor : PropertyDescriptor
    {
        private FieldInfo _field;

        public FieldPropertyDescriptor(FieldInfo field, Attribute[] attrs)
            : base(field.Name, attrs)
        {
            _field = field;
        }

        //public FieldInfo Field { get { return _field; } }

        public override bool Equals(object obj)
        {
            FieldPropertyDescriptor other = obj as FieldPropertyDescriptor;
            return other != null && other._field.Equals(_field);
        }

        public override int GetHashCode() { return _field.GetHashCode(); }

        public override bool IsReadOnly { get { return true; } }
        public override void ResetValue(object component) { }
        public override bool CanResetValue(object component) { return false; }
        public override bool ShouldSerializeValue(object component) { return false; }

        public override Type ComponentType { get { return _field.DeclaringType; } }
        public override Type PropertyType { get { return _field.FieldType; } }

        public override object GetValue(object component) { return _field.GetValue(component); }

        public override void SetValue(object component, object value)
        {
            _field.SetValue(component, value);
            OnValueChanged(component, EventArgs.Empty);
        }
    }

    class CustomFieldPropertyDescriptor : PropertyDescriptor
    {
        private object _field;
        Type compType;

        public CustomFieldPropertyDescriptor(object field_value, Attribute[] attrs, string name, Type comp_type)
            : base(name, attrs)
        {
            _field = field_value;
            compType = comp_type;
        }

        //public FieldInfo Field { get { return _field; } }

        public override bool Equals(object obj)
        {
            CustomFieldPropertyDescriptor other = obj as CustomFieldPropertyDescriptor;
            return other != null && other._field.Equals(_field);
        }

        public override int GetHashCode() { return _field.GetHashCode(); }

        public override bool IsReadOnly { get { return true; } }
        public override void ResetValue(object component) { }
        public override bool CanResetValue(object component) { return false; }
        public override bool ShouldSerializeValue(object component) { return false; }

        public override Type ComponentType { get { return compType; } }
        public override Type PropertyType { get { return _field.GetType(); } }

        public override object GetValue(object component) { return _field; }

        public override void SetValue(object component, object value)
        {
            //_field.SetValue(component, value);
            //OnValueChanged(component, EventArgs.Empty);
        }
    }

}
