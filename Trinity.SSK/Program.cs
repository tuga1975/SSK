using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Trinity.Device;

namespace SSK
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
            try
            {
                // Health checker
                CameraMonitor.Start();
                SpeakerMonitor.Start();
                DocumentScannerMonitor.Start();
                FingerprintReaderMonitor.Start();
                BarcodeScannerMonitor.Start();
                SmartCardReaderMonitor.Start();
                ReceiptPrinterMonitor.Start();
                LEDDisplayMonitor.Start();

                Application.Run(new Main());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Environment.Exit(1);
            }
            //Application.Run(new FormTextToSpeech());
            //Application.Run(new FormAppointmentDetails());
        }
    }
}
