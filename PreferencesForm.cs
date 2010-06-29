using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace gep
{
    public partial class PreferencesForm : Form
    {
        public bool autoArrange;
        public bool suggestURLs;
        public bool useDirectConnect; 
        public bool createFiltersByName;

        public PreferencesForm(bool _autoArrange, bool _suggestURLs, bool _useDirectConnect, bool _createFiltersByName)
        {
            autoArrange = _autoArrange;
            suggestURLs = _suggestURLs;
            useDirectConnect = _useDirectConnect;
            createFiltersByName = _createFiltersByName;
            InitializeComponent();
        }

        private void okBtn_Click(object sender, EventArgs e)
        {
            autoArrange = autoArrangeChkBox.Checked;
            suggestURLs = suggestURLsChkBox.Checked;
            useDirectConnect = findPinsBtn.Checked;
            createFiltersByName = createByNameBtn.Checked;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void cancelBtn_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void PreferencesForm_Load(object sender, EventArgs e)
        {
            autoArrangeChkBox.Checked = autoArrange;
            suggestURLsChkBox.Checked = suggestURLs;
            createByMonBtn.Checked = !createFiltersByName;
            createByNameBtn.Checked = createFiltersByName;
            findPinsBtn.Checked = useDirectConnect;
            renderStreamBtn.Checked = !useDirectConnect;
        }
    }
}