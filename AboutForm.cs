using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace gep
{
    public partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string url = "http://www.thedeemon.com";
            Program.OpenBrowser(url);
            //System.Diagnostics.Process.Start(url);
            /*Process process = new Process();
            process.StartInfo.FileName = url;
            //process.StartInfo.Arguments = url;
            process.StartInfo.UseShellExecute = true;
            process.Start();*/
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}