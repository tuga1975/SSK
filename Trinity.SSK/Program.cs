using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Trinity.Device;
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
                // Health checker
                CameraMonitor.Start();
                SpeakerMonitor.Start();
                DocumentScannerMonitor.Start();
                FingerprintReaderMonitor.Start();
                BarcodeScannerMonitor.Start();
                SmartCardReaderMonitor.Start();
                ReceiptPrinterMonitor.Start();
                QueueScreenMonitor.Start();

                // SSK is initialisation for use.
                // Turn on BLUE Light
                string comPort = ConfigurationManager.AppSettings["COMPort"];
                int baudRate = int.Parse(ConfigurationManager.AppSettings["BaudRate"]);
                string parity = ConfigurationManager.AppSettings["Parity"];
                LEDStatusLightingUtil.Instance.OpenPort("SSK", comPort, baudRate, parity);
                LEDStatusLightingUtil.Instance.TurnOffAllLEDs();
                LEDStatusLightingUtil.Instance.SwitchBLUELightOnOff(true);

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
