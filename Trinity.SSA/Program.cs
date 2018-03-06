using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Trinity.Device;
using Trinity.Device.Util;

namespace SSA
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
            CameraMonitor.Start();
            DocumentScannerMonitor.Start();
            FingerprintReaderMonitor.Start();
            SmartCardReaderMonitor.Start();
            MUBLabelPrinterMonitor.Start();
            TTLabelPrinterMonitor.Start();

            // SSA is initialisation for use.
            // Turn on BLUE Light
            string comPort = ConfigurationManager.AppSettings["COMPort"];
            int baudRate = int.Parse(ConfigurationManager.AppSettings["BaudRate"]);
            string parity = ConfigurationManager.AppSettings["Parity"];
            LEDStatusLightingUtil.Instance.OpenPort("SSA", comPort, baudRate, parity);
            LEDStatusLightingUtil.Instance.TurnOffAllLEDs();
            LEDStatusLightingUtil.Instance.SwitchBLUELightOnOff(true);

            Application.Run(new Main());
        }
    }
}
