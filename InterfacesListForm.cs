using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace gep
{
    partial class InterfacesListForm : Form
    {
        public InterfacesListForm()
        {
            InitializeComponent();
        }

        public void SetList(List<InterfaceInfo> lst)
        {
            //textBox.Lines = lst.ToArray();
            //textBox.Select(0, 0);
            foreach (InterfaceInfo ii in lst)
            {
                TreeNode nd = tree.Nodes.Add(ii.name);
                foreach (string mt in ii.methods)
                    nd.Nodes.Add(mt);
            }
            
        }
    }
}