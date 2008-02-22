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
    partial class RegisterForm : Form
    {
        public RegisterForm()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (Filterz.rch.CheckCode(emailTextBox.Text, codeTextBox.Text))
            {
                MessageBox.Show("Thanks!");
                Program.mainform.email = emailTextBox.Text;
                Program.mainform.code = codeTextBox.Text;
                string keyname = @"Software\Dee Mon\GraphEditPlus";
                RegistryKey rk = Registry.CurrentUser.OpenSubKey(keyname, true);
                if (rk == null)
                    rk = Registry.CurrentUser.CreateSubKey(keyname);
                if (rk != null)
                {
                    rk.SetValue("email", Program.mainform.email);
                    rk.SetValue("code", Program.mainform.code);
                }
                Close();
            }
            else
                MessageBox.Show("This code is invalid.");
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        string regGetBuyURL(string publisher, string appName, string appVer )
        {	        
	        string buyURL = "";
	        // form the registry key path
	        string keyPath = "SOFTWARE\\Digital River\\SoftwarePassport\\" + publisher + "\\" + appName + "\\" + appVer;
	        // read the "BuyURL" value from HKEY_LOCAL_MACHINE branch first
            RegistryKey rk = Registry.LocalMachine.OpenSubKey(keyPath, true);
	        if (rk == null) {
		        // fail to read from HKEY_LOCAL_MACHINE branch, try HKEY_CURRENT_USER
                rk = Registry.CurrentUser.OpenSubKey(keyPath, true);
	        };
	        if (rk != null) {
                buyURL = rk.GetValue("BuyURL") as string;
                rk.Close();            
	        }            
	        return buyURL;
        }

        private void btnOrder_Click(object sender, EventArgs e)
        {
            string buyURL = regGetBuyURL("Dee Mon", "GraphEditPlus", "0");
            if (buyURL==null || buyURL.Length == 0)
                buyURL = "http://www.thedeemon.com/GraphEditPlus/register.html";
            //MessageBox.Show(buyURL);
            //System.Diagnostics.Process.Start(buyURL);
            Program.OpenBrowser(buyURL);
        }

    }
}