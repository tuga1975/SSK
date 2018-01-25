using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Trinity.Util;

namespace Experiment
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // start health monitors
            bool result = SmartCardReaderMonitor.Start();
            bool result2 = FingerprintReaderMonitor.Start();

            // 
            Application.Run(new Form1());
        }
    }
}
