using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Trinity.Common.Monitor;

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
            //
            DeviceMonitor.Start();
            //Application.Run(new Main());
            //Application.Run(new FormTextToSpeech());
            Application.Run(new FormAppointmentDetails());
        }
    }
}
