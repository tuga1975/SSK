using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Trinity.Device;
using Trinity.Device.Monitor;
using Trinity.Device.Util;

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
                // Start application status monitor and update application status
                ApplicationStatusMonitor.Instance.StartInitialization();
                //ApplicationStatusMonitor.Instance.UpdateApplicationStatus(EnumApplicationStatus.Initiation);
                
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
        
        //private static void StartHealthChecker()
        //{
        //    CameraMonitor.Start();
        //    SpeakerMonitor.Start();
        //    DocumentScannerMonitor.Start();
        //    FingerprintReaderMonitor.Start();
        //    BarcodeScannerMonitor.Start();
        //    SmartCardReaderMonitor.Start();
        //    ReceiptPrinterMonitor.Start();
        //    QueueScreenMonitor.Start();
        //}
    }
}
