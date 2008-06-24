using System.Collections.Generic;
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
            AddNodes(tree.Nodes, lst);
        }

        static void AddNodes(TreeNodeCollection nodes, IEnumerable<InterfaceInfo> lst)
        {
            foreach (InterfaceInfo info in lst)
            {
                TreeNode nd = nodes.Add(info.name);
                if (info.elements.Count > 0)
                    AddNodes(nd.Nodes, info.elements);
            }
        }
    }
}