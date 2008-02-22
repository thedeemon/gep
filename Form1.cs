using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace gep
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }


        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        public Filterz filterz;
        public PropertiesForm propform;

        private void MainForm_Load(object sender, EventArgs e)
        {
            Program.mainform = this;
            filterz = new Filterz();
            filterz.MdiParent = this;
            filterz.Show();

            propform = new PropertiesForm();
            propform.MdiParent = this;
            propform.Show();
            propform.Location = new Point(600, 0);
        }
    }
}