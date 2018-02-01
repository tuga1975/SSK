using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SignalRChat
{
    public static class Program
    {
        internal static WinFormsServer MainForm { get; set; }
        internal static HashSet<ProfileConnected> ProfileConnected = new HashSet<ProfileConnected>();
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            MainForm = new WinFormsServer();
            Application.Run(MainForm);
        }
    }
}
