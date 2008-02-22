using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using DirectShowLib;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace gep
{

    class Pin
    {
        PinDirection direction;
        public PinDirection Direction { get { return direction; } }
        
        Rectangle rect;
        public Rectangle Rect { 
            get { return rect; }
            set { rect = value; }
        }
        public Point Location { set { rect.Location = value; } }

        Filter filter;
        public Filter Filter { get { return filter; } }

        PinConnection connection;
        public PinConnection Connection { get { return connection; } }

        string uniqname;
        public string UniqName { get { return uniqname; } }

        string name;
        public string Name { get { return name; } }

        IPin ipin;
        public IPin IPin { get { return ipin; } }

        public static Font pinfont = new Font("Arial", 8);
        static Brush pinbrush = new SolidBrush(Color.Cyan);

        int num;
        public int Num { get { return num; } }

        public Pin(PinDirection dir, Filter f, IPin _ipin, int _num) //x,y in pixels inside filter rect
        {
            direction = dir;
            filter = f;
            num = _num;
            rect = new Rectangle(0, 0, 10, 10);
            ipin = _ipin;
            PinInfo pinfo;
            ipin.QueryPinInfo(out pinfo);
            name = pinfo.name;
            uniqname = f.Name + "." + pinfo.name;
            DsUtils.FreePinInfo(pinfo);
        }

        public void Draw(Graphics g, int x, int y)
        {
            Rectangle rc = rect;
            rc.X += x;
            rc.Y += y;
            LinearGradientBrush br = new LinearGradientBrush(new Point(0, rc.Top), new Point(0, rc.Bottom),
                Color.FromArgb(200, 200, 200), Color.FromArgb(100, 100, 100));
            g.FillRectangle(br, rc);
            if (direction == PinDirection.Input)
                g.DrawString(name, pinfont, pinbrush, rc.Right + 1, rc.Top);
            else
            {
                SizeF sz = g.MeasureString(name, pinfont);
                g.DrawString(name, pinfont, pinbrush, rc.Left - 1 - sz.Width, rc.Top);
            }
        }

        public void CompleteConnect(PinConnection con, bool really)
        {
            connection = con;
            if (con == null && really)
            {
                Filter.Graph.Disconnect(ipin);
                //int hr = ipin.Disconnect();                
                //DsError.ThrowExceptionForHR(hr);
            }
        }

    }

    struct ConStep
    {
        public int x, y, dir;
    }

    class PinConnection
    {
        public Pin[] pins; //outpin, inpin
        public List<ConStep> path;
        int id;
        public int ID { get { return id; } }
        string uniqname;
        public string UniqName { get { return uniqname; } }
        Pen beigePen = new Pen(Brushes.Green);
        Pen yellowPen = new Pen(Brushes.Yellow);

        static int lastid = 1;
        
        public PinConnection(Pin outpin, Pin inpin)
        {
            id = lastid++;
            pins = new Pin[2] { outpin, inpin };
            inpin.CompleteConnect(this, true);
            outpin.CompleteConnect(this, true);
            uniqname = outpin.UniqName + "-" + inpin.UniqName;
        }

        public void Draw(Graphics g, bool selected, Point viewpoint)
        {
            //Point p1 = pins[0].Filter.Rect.Location;
            //Point p2 = pins[1].Filter.Rect.Location;
            //p1.X += pins[0].Rect.Left + 5;
            //p1.Y += pins[0].Rect.Top + 5;
            //p2.X += pins[1].Rect.Left + 5;
            //p2.Y += pins[1].Rect.Top + 5;
            //g.DrawLine(Pens.LightGreen, p1, p2);

            Pen pen = selected ? yellowPen : beigePen;
            int cellsize = pins[0].Filter.cellsize;
            foreach (ConStep cs in path)
            {
                int hc = cellsize / 2;
                int cell = cellsize;
                int cx = cs.x * cellsize + hc - viewpoint.X;
                int cy = cs.y * cellsize + hc - viewpoint.Y;
                switch (cs.dir)
                {
                    case 1: g.DrawLine(pen, cx, cy - hc, cx, cy + hc); break;
                    case 2: g.DrawLine(pen, cx - hc, cy, cx + hc, cy); break;
                    case 3: g.DrawArc(pen, cx, cy-cell, cell, cell, 90, 90); break;
                    case 4: g.DrawArc(pen, cx - cell, cy-cell, cell, cell, 0, 90); break;
                    case 5: g.DrawArc(pen, cx - cell, cy, cell, cell, 270, 90); break;
                    case 6: g.DrawArc(pen, cx, cy, cell, cell, 180, 90); break;
                    //case 5: g.DrawLine(pen, cx - hc, cy, cx, cy + hc); break;
                    //case 6: g.DrawLine(pen, cx + hc, cy, cx, cy + hc); break;
                }
            }
        }

        public void Disconnect(bool really) //really==true - disconnect ipins
        {
            pins[0].CompleteConnect(null, really);
            pins[1].CompleteConnect(null, really);
            pins = null;
            path = null;
        }
    }

}
