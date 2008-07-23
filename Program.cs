using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Diagnostics;

namespace gep
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            dolog = args.Length > 0 && args[0] == "log";
            Application.Run(new MainForm(args));
        }

        public static bool DoLog { get { return dolog; } }
        static bool dolog = false;

        public static MainForm mainform;

        public static void OpenBrowser(string url)
        {
            /*
             Try
                'Return the path used to access http from the registry
                sBrws = LCase(My.Computer.Registry.ClassesRoot.OpenSubKey("http\shell\open\command", False).GetValue(Nothing))
                'Trim the file path (removing everything after the ".exe") and get rid of quotation marks
                sBrws = Replace(Left(sBrws, InStrRev(sBrws, ".exe") + 3), """", "")
                'Open the URL using the path
                Process.Start(sBrws, sURL)
            Catch
                'Add redundant attempts to open the URL, in case the code fails
                Try
                    'First just try using Process.Start, which will open the default browser, but might reuse IE's window
                    Process.Start(sURL)
                Catch
                    Try
                        'If Process.Start fails (due to known bug), try launching IE directly and passing it the URL
                        Dim psi As New ProcessStartInfo("IExplore.exe", sURL)
                        Process.Start(psi)
                        psi = Nothing
                    Catch ex As Exception
                        MsgBox("Could not launch the browser. Details: " & ex.Message)
                    End Try
                End Try
            End Try*/
            try {
                string brws = (string)Registry.ClassesRoot.OpenSubKey(@"http\shell\open\command", false).GetValue(null);
                brws = brws.ToLowerInvariant();
                brws = brws.Substring(0, brws.LastIndexOf(".exe")+4).Replace("\"","");
                //MessageBox.Show(brws, "browser");
                Process.Start(brws, url);
            }
            catch(Exception e1) {
                try { Process.Start(url); }
                catch(Exception e2) {
                    try {
                        ProcessStartInfo psi = new ProcessStartInfo("iexplore.exe", url);
                        Process.Start(psi);                        
                    }
                    catch(Exception e3) {
                        MessageBox.Show("Can't launch browser with following URL:\n"+url
                            + "\nDetails:\n" + e1.Message + "; " + e2.Message + "; " + e3.Message, "Error opening browser");                            
                    }
                }
            }
        }
    }
}