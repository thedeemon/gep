using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.InteropServices;
using System.Diagnostics;
using DirectShowLib;

namespace gep
{
    partial class RotForm : Form
    {
        public RotForm()
        {
            InitializeComponent();
        }

        delegate bool procgraph(string name, IMoniker mon, IRunningObjectTable rot); //true to stop

        static void EnumROT(procgraph fun)
        {
            IRunningObjectTable tb;
            IBindCtx bindCtx;
            NativeMethods.CreateBindCtx(0, out bindCtx);
            bindCtx.GetRunningObjectTable(out tb);
            IEnumMoniker emon;
            tb.EnumRunning(out emon);
            IMoniker[] pmon = new IMoniker[1];
            IntPtr fetched = IntPtr.Zero;
            while (emon.Next(1, pmon, fetched) == 0)
            {
                IBindCtx pCtx;
                NativeMethods.CreateBindCtx(0, out pCtx);
                string str;
                pmon[0].GetDisplayName(pCtx, null, out str);
                if (fun(str, pmon[0], tb))
                    break;
            }
        }

        private void Refresh(object sender, EventArgs e)
        {
            string spid = "pid " + Process.GetCurrentProcess().Id.ToString("X8").ToLowerInvariant();
            listBox.Items.Clear();
            EnumROT(delegate(string name, IMoniker mon, IRunningObjectTable rot)
                {
                    if (name.Contains("FilterGraph") && !name.Contains(spid))
                        listBox.Items.Add(name);
                    return false;
                });
        }

        private void OnOK(object sender, EventArgs e)
        {
            if (listBox.SelectedItem == null)
                return;
            string str = listBox.SelectedItem.ToString();
            try
            {
                EnumROT(delegate(string name, IMoniker mon, IRunningObjectTable rot)
                {
                    if (name == str)
                    {
                        object obj;
                        rot.GetObject(mon, out obj);
                        IGraphBuilder graphBuilder = (IGraphBuilder)obj;
                        GraphForm gf = new GraphForm(graphBuilder);
                        gf.MdiParent = MdiParent;
                        gf.Text = "Remote graph: " + name;
                        gf.Show();                        
                        return true;
                    }
                    return false;
                });
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Can't get running graph");
            }
        }

        private void OnCancel(object sender, EventArgs e)
        {
            Close();
        }

        private void RotForm_Load(object sender, EventArgs e)
        {
            Refresh(sender, e);
        }

        private void OnListBoxDoubleClick(object sender, MouseEventArgs e)
        {
            OnOK(sender, null);
        }
    }
}