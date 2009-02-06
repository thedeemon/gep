using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace gep
{
    public partial class SearchFilterForm : Form
    {
        private SearchFilterForm(IEnumerable<string> cats, bool allcats, string namepart, string current_cat)
        {
            all_categories = allcats;
            categories = cats;
            name_part = namepart;
            current_category = current_cat;

            InitializeComponent();
            filtertree.Sorted = true;
        }

        private static SearchFilterForm form;

        public static void ShowSearchFilterForm(IEnumerable<string> cats, bool allcats, string namepart, string current_cat, Form mdiparent)
        {
            if (form == null)
            {
                form = new SearchFilterForm(cats, allcats, namepart, current_cat);
                form.MdiParent = mdiparent;
                form.Show();
            }
            else
                form.BringToFront();
        }

        bool all_categories;
        IEnumerable<string> categories;
        string name_part;
        string current_category;

        private void OnLoad(object sender, EventArgs e)
        {
            ToolTip toolTip = new ToolTip();
            toolTip.AutoPopDelay = 5000;
            toolTip.InitialDelay = 500;
            toolTip.ReshowDelay = 500;
            toolTip.ShowAlways = true;
            toolTip.SetToolTip(catcombo, "Filter category to search in");
            toolTip.SetToolTip(textBoxName, "Substring to be found in filter's friendly name");
            toolTip.SetToolTip(textBoxDispName, "Substring to be found in filter's display name");
            toolTip.SetToolTip(textBoxCLSID, "Part of filter's CLSID");
            toolTip.SetToolTip(textBoxPathName, "Part of filter's file name or path");
            toolTip.SetToolTip(filtertree, "Double click or drag a filter to add it to current graph");

            catcombo.Items.Add("All categories");
            foreach(string catname in categories)
                catcombo.Items.Add(catname);
            if (all_categories)
                catcombo.SelectedIndex = 0;
            else
                foreach(object item in catcombo.Items)
                    if (item.ToString() == current_category)
                    {
                        catcombo.SelectedItem = item;
                        break;
                    }
            textBoxName.Text = name_part;
            textBoxName.Focus();
        }

        List<FilterProps> filters;

        private void OnCategoryChanged(object sender, EventArgs e)
        {
            if (catcombo.SelectedIndex == 0)
                filters = Filterz.GetAllFilters();
            else
                filters = Filterz.GetFiltersOfCategory(catcombo.SelectedItem.ToString());
            ShowFoundFilters();
        }

        bool match(string str, string substr)
        {
            if (str == null) return false;
            return str.ToLowerInvariant().Contains(substr);
        }

        IEnumerable<FilterProps> SearchFilters(IEnumerable<FilterProps> flist)
        {
            string namepart = textBoxName.Text.ToLowerInvariant();
            string dispname = textBoxDispName.Text.ToLowerInvariant();
            string clsdpart = textBoxCLSID.Text.ToLowerInvariant();
            string pathpart = textBoxPathName.Text.ToLowerInvariant();
            foreach (FilterProps fp in flist)
                if (match(fp.Name, namepart) && match(fp.DisplayName, dispname) && 
                    match(fp.CLSID, clsdpart) && match(fp.GetFileName(), pathpart))
                        yield return fp;
        }

        void ShowFoundFilters()
        {
            Filterz.FillFilterTree(filtertree, SearchFilters(filters));
        }

        private void OnTextChanged(object sender, EventArgs e)
        {
            ShowFoundFilters();
        }

        private void OnDblClick(object sender, MouseEventArgs e)
        {
            TreeNode nd = filtertree.SelectedNode;
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

        private void OnClose(object sender, FormClosedEventArgs e)
        {
            form = null;
        }

        private void OnItemDrag(object sender, ItemDragEventArgs e)
        {
            DoDragDrop(e.Item, DragDropEffects.Copy);
        }
    }
}