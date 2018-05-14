﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Trinity.Device;

namespace Enrolment
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Health checker
            SmartCardPrinterMonitor.Start();
            CameraMonitor.Start();
            SmartCardReaderMonitor.Start();
            FingerprintReaderMonitor.Start();
            BarcodeScannerMonitor.Start();

            Application.Run(new Main());
        }
    }
}
