using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DirectShowLib;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.InteropServices;

namespace gep
{
    partial class MatchingFiltersForm : Form
    {
        Pin pin;
        public MatchingFiltersForm(Pin _pin)
        {
            pin = _pin;
            InitializeComponent();
        }

        private void MatchingFiltersForm_Load(object sender, EventArgs e)
        {
            List<Guid> guids = new List<Guid>();
            try {
                if (pin.Connection != null)
                {
                    AMMediaType mt = new AMMediaType();
                    pin.IPin.ConnectionMediaType(mt);
                    guids.Add(mt.majorType);
                    guids.Add(mt.subType);
                    DsUtils.FreeAMMediaType(mt);
                }
                else
                {
                    IEnumMediaTypes mtenum;
                    if (pin.IPin.EnumMediaTypes(out mtenum) >= 0)
                    {
                        AMMediaType[] mts = new AMMediaType[1];
                        IntPtr fetched = Marshal.AllocHGlobal(4);
                        while (mtenum.Next(1, mts, fetched) == 0)
                        {
                            guids.Add(mts[0].majorType);
                            guids.Add(mts[0].subType);
                            DsUtils.FreeAMMediaType(mts[0]);
                        }
                        Marshal.FreeHGlobal(fetched);
                    }
                }
                IFilterMapper2 fm = (IFilterMapper2)new FilterMapper2();
                IEnumMoniker emon, emon_rend; //non-renderers, renderers
                int hr = 0;
                List<FilterProps> fplist = new List<FilterProps>();
                if (pin.Direction == PinDirection.Output)
                {
                    hr = fm.EnumMatchingFilters(out emon, 0, true, Merit.None, true, guids.Count / 2, guids.ToArray(),
                        null, null, false, false, 0, null, null, null);
                    DsError.ThrowExceptionForHR(hr);
                    hr = fm.EnumMatchingFilters(out emon_rend, 0, true, Merit.None, true, guids.Count / 2, guids.ToArray(),
                        null, null, true, false, 0, null, null, null);
                    DsError.ThrowExceptionForHR(hr);
                    fplist.AddRange(Filterz.GetFiltersFromEnum(emon, Guid.Empty));
                    fplist.AddRange(Filterz.GetFiltersFromEnum(emon_rend, Guid.Empty));
                }
                else
                {
                    hr = fm.EnumMatchingFilters(out emon, 0, true, Merit.None, false, 0, null, null, null, false,
                        true, guids.Count / 2, guids.ToArray(), null, null);
                    DsError.ThrowExceptionForHR(hr);
                    hr = fm.EnumMatchingFilters(out emon_rend, 0, true, Merit.None, false, 0, null, null, null, true,
                        true, guids.Count / 2, guids.ToArray(), null, null);
                    DsError.ThrowExceptionForHR(hr);
                    fplist.AddRange(Filterz.GetFiltersFromEnum(emon, Guid.Empty));
                    fplist.AddRange(Filterz.GetFiltersFromEnum(emon_rend, Guid.Empty));
                }

                //Filterz.BuildFilterTree(emon, treeView, Guid.Empty);
                Filterz.FillFilterTree(treeView, fplist);
            } 
            catch(COMException ex)
            {
                Graph.ShowCOMException(ex, "Can't enumerate matching filters");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception caught while enumerating matching filters");
            }
        }

        private void treeView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            TreeNode nd = treeView.SelectedNode;
            if (nd == null)
                return;
            while (nd.Parent != null && nd.Tag == null)
                nd = nd.Parent;
            FilterProps fp = (FilterProps)nd.Tag;
            if (fp != null)
            {
                GraphForm gf = Program.mainform.ActiveGraphForm;
                if (gf != null)
                    gf.AddFilter(fp);
            }
        }



    }//class
}