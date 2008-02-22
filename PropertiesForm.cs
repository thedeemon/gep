using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace gep
{
    partial class PropertiesForm : Form
    {
        public PropertiesForm()
        {
            InitializeComponent();
        }

        public void SetObject(Object o)
        {
            propgrid.SelectedObject = o;
        }

        private void PropertiesForm_Load(object sender, EventArgs e)
        {
            propgrid.ViewForeColor = Color.FromArgb(1, 1, 1);
        }
    }
}