using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace gep
{
    public partial class RenderURLForm : Form
    {
        public RenderURLForm(string caption)
        {
            InitializeComponent();
            Text = caption;
        }

        private void OnCancel(object sender, EventArgs e)
        {
            Close();
        }

        private void OnOK(object sender, EventArgs e)
        {
            string cur_url = comboURL.Text;
            url_list.Remove(cur_url);
            url_list.Insert(0, cur_url);
            int k = Math.Min(url_list.Count, 10);
            using (RegistryKey rk = Registry.CurrentUser.OpenSubKey(keyname, true))
            {
                for (int i = 0; i < k; i++)
                {
                    string url_name = "url" + i.ToString();
                    rk.SetValue(url_name, url_list[i]);
                }
            }
            selectedURL = cur_url;
            Close();
        }

        List<string> url_list = new List<string>();
        string keyname = @"Software\Dee Mon\GraphEditPlus";
        public string selectedURL;

        private void RenderURLForm_Load(object sender, EventArgs e)
        {            
            using (RegistryKey rk = Registry.CurrentUser.OpenSubKey(keyname))
            {
                for (int i = 0; i < 10; i++)
                {
                    string url_name = "url" + i.ToString();
                    string url = (string)rk.GetValue(url_name, "");
                    url_list.Add(url);
                    comboURL.Items.Add(url);
                }
            }
        }
    }
}