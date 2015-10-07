using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace gep
{
    partial class GenerateCodeForm : Form
    {
        string Code;
        public GenerateCodeForm(string code)
        {
            Code = code;
            InitializeComponent();
        }

        private void GenerateCodeForm_Load(object sender, EventArgs e)
        {
            using (var ff = new FontFamily(System.Drawing.Text.GenericFontFamilies.Monospace))
            {
                textBox.Font = new Font(ff, 10, FontStyle.Regular);
                textBox.Text = Code;
            }
        }
    }
}