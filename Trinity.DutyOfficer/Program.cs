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
            try
            {
                Task.Factory.StartNew(StartHealthChecker);
                Application.Run(new Main());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Environment.Exit(1);
            }
        }
        private static void StartHealthChecker()
        {
            SmartCardReaderMonitor.Start();
            FingerprintReaderMonitor.Start();
            MUBLabelPrinterMonitor.Start();
            UBLabelPrinterMonitor.Start();
            TTLabelPrinterMonitor.Start();
            SmartCardReaderMonitor.Start();
            BarcodeScannerMonitor.Start();
        }
    }
}
