using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Trinity.Device;

namespace DutyOfficer
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

            // Health checker
            SmartCardReaderMonitor.Start();
            FingerprintReaderMonitor.Start();
            MUBLabelPrinterMonitor.Start();
            UBLabelPrinterMonitor.Start();
            TTLabelPrinterMonitor.Start();
            BarcodeScannerMonitor.Start();

            Application.Run(new Main());
        }
    }
}
