using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace gep
{
    partial class GenerateCodeGfxForm : Form
    {
        public GenerateCodeGfxForm(string code)
        {
            InitializeComponent();
            using (var ff = new FontFamily(System.Drawing.Text.GenericFontFamilies.Monospace))
            using (var font = new Font(ff, 10, FontStyle.Regular))
            using (Graphics og = CreateGraphics())
            {
                Size sz = og.MeasureString(code, font).ToSize();
                Bitmap bmp = new Bitmap(sz.Width, sz.Height);
                Graphics g = Graphics.FromImage(bmp);
                g.FillRectangle(Brushes.White, 0, 0, sz.Width, sz.Height);
                g.DrawString(code, font, Brushes.Black, 0, 0);
                g.Dispose();
                vScrollBar.Maximum = sz.Height;
                vScrollBar.SmallChange = sz.Height / 10;
                vScrollBar.LargeChange = panel.Size.Height;//sz.Height / 4;
                panel.bmp = bmp;
            }            
        }

        private void vScrollBar_ValueChanged(object sender, EventArgs e)
        {
            panel.dy = -vScrollBar.Value;
            panel.Invalidate();
        }

        private void OnRegister(object sender, EventArgs e)
        {
            using(var f = new RegisterForm())
                f.ShowDialog();
        }
    }

    class CodePanel : Panel
    {
        public Bitmap bmp;
        public int dy = 0;

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (bmp != null)
            {
                Rectangle rc = ClientRectangle;
                Size sz = bmp.Size;
                rc.X += sz.Width;
                e.Graphics.FillRectangle(Brushes.White, rc);
                rc = ClientRectangle;
                rc.Y += sz.Height + dy;
                e.Graphics.FillRectangle(Brushes.White, rc);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (bmp != null)
                e.Graphics.DrawImageUnscaled(bmp, 0, dy);
        }
    }

    
}