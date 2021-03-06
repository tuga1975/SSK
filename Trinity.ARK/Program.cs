﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Trinity.Device;
using Trinity.Device.Util;

namespace ARK
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
                //Environment.Exit(1);
            }
            //Application.Run(new FormTextToSpeech());
            //Application.Run(new FormAppointmentDetails());
        }
    }
}
