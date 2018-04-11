using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Trinity.Device;
using Trinity.Device.Util;

namespace ALK
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
                Task.Factory.StartNew(ApplicationStatusManager.Instance.StartInitialization);
                Application.Run(new Main());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// Just backup, remove it later. Use ApplicationStatusManager instead
        /// </summary>
        private static void StartHealthChecker()
        {
            // Health checker
            CameraMonitor.Start();
            DocumentScannerMonitor.Start();
            FingerprintReaderMonitor.Start();
            SmartCardReaderMonitor.Start();
            MUBLabelPrinterMonitor.Start();
            TTLabelPrinterMonitor.Start();

            // ALK is initialisation for use.
            // Turn on BLUE Light
            string comPort = ConfigurationManager.AppSettings["COMPort"];
            int baudRate = int.Parse(ConfigurationManager.AppSettings["BaudRate"]);
            string parity = ConfigurationManager.AppSettings["Parity"];
            Task.Factory.StartNew(ApplicationStatusManager.Instance.StartInitialization);
            LEDStatusLightingUtil.Instance.OpenPort("ALK", comPort, baudRate, parity);
            LEDStatusLightingUtil.Instance.TurnOffAllLEDs();
            LEDStatusLightingUtil.Instance.SwitchBLUELightOnOff(true);
        }
    }
}
