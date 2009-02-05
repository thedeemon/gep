using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DirectShowLib;
using System.Runtime.InteropServices;

namespace gep
{
    public partial class MediaTypeListForm : Form
    {
        IAMStreamConfig isc;
        int size;
        IntPtr scc = IntPtr.Zero;
        public AMMediaType selected_mt;

        public MediaTypeListForm(IAMStreamConfig _isc)
        {
            isc = _isc;
            InitializeComponent();
        }

        private void MediaTypeListForm_Load(object sender, EventArgs e)
        {
            int count;
            try
            {
                listBox.Items.Add("null (resets to default on some filters)");
                listBox.Items.Add(MediaTypeProps.CreateMTProps(new AMMediaType()).ToString());

                if (isc.GetNumberOfCapabilities(out count, out size) >= 0)
                {
                    scc = Marshal.AllocHGlobal(size);
                    for (int i = 0; i < count; i++)
                    {
                        AMMediaType mt;
                        int hr = isc.GetStreamCaps(i, out mt, scc);
                        DsError.ThrowExceptionForHR(hr);
                        string s = MediaTypeProps.CreateMTProps(mt).ToString();
                        listBox.Items.Add(s);
                    }
                }
            }
            catch (COMException ex)
            {
                Graph.ShowCOMException(ex, "Error getting stream capabilites");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void OnOK(object sender, EventArgs e)
        {
            try
            {
                int hr = isc.SetFormat(selected_mt);
                DsError.ThrowExceptionForHR(hr);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (COMException ex)
            {
                Graph.ShowCOMException(ex, "Error trying to SetFormat");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void OnCancel(object sender, EventArgs e)
        {
            selected_mt = null;
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void OnSelChange(object sender, EventArgs e)
        {
            try
            {
                int i = listBox.SelectedIndex;
                if (i == 0) selected_mt = null;
                if (i == 1) selected_mt = new AMMediaType();
                if (i >= 2)
                {
                    AMMediaType mt;
                    int hr = isc.GetStreamCaps(i-2, out mt, scc);
                    DsError.ThrowExceptionForHR(hr);
                    selected_mt = mt;
                }
                propertyGrid.SelectedObject = selected_mt != null ?
                                    MediaTypeProps.CreateMTProps(selected_mt) : null;
            }
            catch (COMException ex)
            {
                Graph.ShowCOMException(ex, "Error trying to GetStreamCaps");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void OnClosed(object sender, FormClosedEventArgs e)
        {
            if (scc != IntPtr.Zero) 
                Marshal.FreeHGlobal(scc);
        }
    }
}