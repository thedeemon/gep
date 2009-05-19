using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DirectShowLib;
using System.Runtime.InteropServices;

namespace gep
{
    partial class SampleGrabberForm : Form
    {
        ISampleGrabber sampleGrabber;
        SampleGrabberCallback cb;
        Timer timer = new Timer();
        Filter filter;

        public SampleGrabberForm(ISampleGrabber sg, Filter f)
        {
            sampleGrabber = sg;
            f.sampleGrabberForm = this;
            filter = f;
            cb = new SampleGrabberCallback(this);
            InitializeComponent();
            timer.Interval = 1000;
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
            Text = "Samples grabbed by " + f.Name;
        }

        void timer_Tick(object sender, EventArgs e)
        {
            lock (cb)
            {
                textBox.AppendText(cb.sb.ToString());
                cb.sb = new StringBuilder();
            }
        }

        private void SampleGrabberForm_Load(object sender, EventArgs e)
        {
            try
            {
                int hr = sampleGrabber.SetCallback(cb, 0);
                DsError.ThrowExceptionForHR(hr);
            }
            catch (COMException ex)
            {
                Graph.ShowCOMException(ex, "Can't set callback");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception caught while setting callback for samplegrabber");
            }
        }

        public void AddText(string text)
        {
            textBox.AppendText(text);
        }

        private void SampleGrabberForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                filter.sampleGrabberForm = null;
                filter = null;
                int hr = sampleGrabber.SetCallback(null, 0);
                DsError.ThrowExceptionForHR(hr);
                timer.Stop();
                sampleGrabber = null;
            }
            catch (COMException ex)
            {
                Graph.ShowCOMException(ex, "Can't set callback to null");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception caught while setting callback for samplegrabber");
            }
        }
    }//class

    class SampleGrabberCallback : ISampleGrabberCB
    {
        SampleGrabberForm sgform;
        public StringBuilder sb = new StringBuilder();

        public SampleGrabberCallback(SampleGrabberForm sf)
        {
            sgform = sf;
        }

        public int BufferCB(double SampleTime, IntPtr pBuffer, int BufferLen)
        {
            return 0;
        }

        public int SampleCB(double SampleTime, IMediaSample pSample)
        {            
            DateTime dt = DateTime.Now;
            lock (this)
            {
                sb.AppendFormat("{0:T}.{1:D3}: ", dt, dt.Millisecond);
                sb.AppendFormat("SampleTime={0:G4}, ", SampleTime);
                if (pSample != null)
                {
                    long start = 0, end = 0;
                    pSample.GetTime(out start, out end);
                    sb.AppendFormat("Time(start={0}, end={1}), ", start, end);
                    pSample.GetMediaTime(out start, out end);
                    sb.AppendFormat("MediaTime(start={0}, end={1}), ", start, end);
                    int len = pSample.GetActualDataLength();
                    sb.AppendFormat("data length={0}, ", len);
                    bool syncpoint = pSample.IsSyncPoint() == 0;
                    sb.AppendFormat("keyframe={0}", syncpoint);
                    if (pSample.IsDiscontinuity() == 0)
                        sb.Append(", Discontinuity");
                    if (pSample.IsPreroll() == 0)
                        sb.Append(", Preroll");
                    int n = Math.Min(len, 8);
                    IntPtr pbuf;
                    if (pSample.GetPointer(out pbuf) == 0)
                    {
                        byte[] buf = new byte[n];
                        Marshal.Copy(pbuf, buf, 0, n);
                        sb.Append(", Data=");
                        for (int i = 0; i < n; i++)
                            sb.AppendFormat("{0:X2}", buf[i]);
                        sb.Append("...");
                    }
                    
                }
                else
                    sb.Append("pSample==NULL!");
                sb.Append(Environment.NewLine);
            }
            Marshal.ReleaseComObject(pSample);
            return 0;
        }


    }
}