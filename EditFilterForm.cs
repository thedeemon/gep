using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;

namespace gep
{
    partial class EditFilterForm : Form
    {
        FilterProps fp;
        int[] merits = new int[] { 0x800000, 0x600000, 0x400000, 0x200000, 0x100000, 0x100050 };

        public EditFilterForm(FilterProps _fp)
        {
            InitializeComponent();
            fp = _fp;
        }

        private void EditFilterForm_Load(object sender, EventArgs e)
        {
            filterCLSID.Text = fp.CLSID;
            filterName.Text = fp.Name;
            textMerit.Text = fp.Merit;
            string[] str_merits = new string[] {
                "0x800000: MERIT_PREFERRED",
                "0x600000: MERIT_NORMAL",
                "0x400000: MERIT_UNLIKELY",
                "0x200000: MERIT_DO_NOT_USE",
                "0x100000: MERIT_SW_COMPRESSOR",
                "0x100050: MERIT_HW_COMPRESSOR"
            };
            meritCombo.Items.AddRange(str_merits);
            int mr = fp.GetMerit();
            for(int i=0;i<merits.Length;i++)
                if (merits[i] == mr)
                {
                    meritCombo.SelectedIndex = i;
                    break;
                }            
        }

        private void OnSetMerit(object sender, EventArgs e)
        {
            string s = textMerit.Text.Trim().ToLowerInvariant();
            if (s.StartsWith("0x"))
                s = s.Substring(2);
            int x=0;
            try
            {
                x = int.Parse(s, System.Globalization.NumberStyles.HexNumber);
            }
            catch
            {
                MessageBox.Show("Please enter a hexadecimal number.");
                return;
            }
            fp.SetMerit(x);
            Program.mainform.filterz.RefreshTree();
        }

        private void OnUnregister(object sender, EventArgs e)
        {
            if (DialogResult.Yes == MessageBox.Show("Do you really want to remove information about "+fp.Name+" from system?", "Unregister filter?", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
            {
                Process.Start("regsvr32.exe", "/u \"" + fp.GetFileName()+ "\"");
                Thread.Sleep(300);
                Program.mainform.filterz.RefreshTree();
            }
        }

        private void OnClose(object sender, EventArgs e)
        {
            Close();
        }

        private void OnMeritSelChange(object sender, EventArgs e)
        {
            string s = meritCombo.SelectedItem.ToString();
            textMerit.Text = s.Remove(s.IndexOf(':'));
        }

        private void EditFilterForm_FormClosing(object sender, FormClosingEventArgs e)
        {
        }
    }
}