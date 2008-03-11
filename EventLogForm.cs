using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace gep
{
    partial class EventLogForm : Form
    {
        Graph graph;
        Timer timer = new Timer();

        public EventLogForm(Graph _graph)
        {
            graph = _graph;
            InitializeComponent();
            Text = "Events log for " + graph.Form.Text;
            timer.Interval = 1000;
            timer.Tick += OnTimer;
            timer.Start();
        }

        private void OnTimer(object sender, EventArgs e)
        {
            Refresh(null, null);
        }

        private void Refresh(object sender, EventArgs e)
        {
            textBox.Text = graph.GetEventLog();
        }

        private void OnClear(object sender, EventArgs e)
        {
            graph.ClearEventLog();
            textBox.Clear();
        }

        private void OnLoad(object sender, EventArgs e)
        {
            Refresh(null, null);
        }

        private void OnClosing(object sender, FormClosingEventArgs e)
        {
            graph.Form.eventlogform = null;
        }
    }
}