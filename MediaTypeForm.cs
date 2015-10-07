using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DirectShowLib;
using System.Reflection;

namespace gep
{
    partial class MediaTypeForm : Form
    {
        ISampleGrabber sampleGrabber;
        IAMStreamConfig streamConfig;

        public MediaTypeForm(ISampleGrabber sg)
        {
            sampleGrabber = sg;
            InitializeComponent();
        }

        public MediaTypeForm(IAMStreamConfig isc)//not used anymore
        {
            streamConfig = isc;
            InitializeComponent();
        }

        private void MediaTypeForm_Load(object sender, EventArgs e)
        {
            AMMediaType mt = new AMMediaType();
            Guid majortype = Guid.Empty, subtype = Guid.Empty;
            if (sampleGrabber!=null && sampleGrabber.GetConnectedMediaType(mt)==0)
            {
                majortype = mt.majorType;
                subtype = mt.subType;
                DsUtils.FreeAMMediaType(mt);
            }
            if (streamConfig != null && streamConfig.GetFormat(out mt) == 0)
            {
                majortype = mt.majorType;
                subtype = mt.subType;
                DsUtils.FreeAMMediaType(mt);
            }
            WalkClass(typeof(MediaType), comboMajorType, majortype);
            WalkClass(typeof(MediaSubType), comboSubType, subtype);
        }

        static void WalkClass(Type MyType, ComboBox cb, Guid chosen)
        {
            string s="";
            foreach (FieldInfo m in MyType.GetFields())
            {
                cb.Items.Add(m.Name);
                if ((Guid)m.GetValue(null) == chosen)
                    s = m.Name;
            }
            for(int i=0;i<cb.Items.Count;i++)
                if (cb.Items[i].ToString() == s)
                {
                    cb.SelectedIndex = i;
                    break;
                }
        }

        private void OnOK(object sender, EventArgs e)
        {
            AMMediaType mt = new AMMediaType();
            string strMajor = comboMajorType.SelectedItem.ToString();
            string strSub = comboSubType.SelectedItem.ToString();
            foreach (FieldInfo m in typeof(MediaType).GetFields())
                if (m.Name == strMajor)
                    mt.majorType = (Guid)m.GetValue(null);
            foreach (FieldInfo m in typeof(MediaSubType).GetFields())
                if (m.Name == strSub)
                    mt.subType = (Guid)m.GetValue(null);
            if (sampleGrabber != null)
                sampleGrabber.SetMediaType(mt);
            if (streamConfig != null)
                streamConfig.SetFormat(mt);
            DsUtils.FreeAMMediaType(mt);
            Close();
        }

        private void OnCancel(object sender, EventArgs e)
        {
            Close();
        }

        private void MediaTypeForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            sampleGrabber = null;
            streamConfig = null;
        }
    }
}