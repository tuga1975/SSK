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
            SmartCardReaderMonitor.Start();
            FingerprintReaderMonitor.Start();
            DocumentScannerMonitor.Start();
            MUBLabelPrinterMonitor.Start();
            UBLabelPrinterMonitor.Start();
            TTLabelPrinterMonitor.Start();
            SmartCardPrinterMonitor.Start();

            // 
            Application.Run(new Form1());
        }
    }
}
