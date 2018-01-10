using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using Trinity.Common.Common;

namespace Trinity.Common.Utils
{
    public class ReceiptPrinterUtils : DeviceUtils
    {
        private string _printerName;

        #region Singleton Implementation
        // The variable is declared to be volatile to ensure that assignment to the instance variable completes before the instance variable can be accessed
        private static volatile ReceiptPrinterUtils _instance;

        // Uses a syncRoot instance to lock on, rather than locking on the type itself, to avoid deadlocks.
        private static object syncRoot = new Object();

        private ReceiptPrinterUtils()
        {
            _printerName = ConfigurationManager.AppSettings["ReceiptPrinterName"];
        }

        public static ReceiptPrinterUtils Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (_instance == null)
                            _instance = new ReceiptPrinterUtils();
                    }
                }

                return _instance;
            }
        }
        #endregion

        public bool PrintAppointmentDetails(AppointmentDetails appointmentDetails)
        {
            try
            {
                // validate
                if (!appointmentDetails.IsValid())
                {
                    return false;
                }

                //Open specified printer driver
                TSCLIB_DLL.openport(_printerName);

                //Setup the media size and sensor type info
                // page size 2.8"x1.4"
                // actually 71.12mm x 42.5mm
                TSCLIB_DLL.setup("71.12", "42.5", "4", "8", "0", "0", "0");

                //Clear image buffer
                TSCLIB_DLL.clearbuffer();

                //Draw windows font
                TSCLIB_DLL.windowsfont(100, 0, 40, 0, 0, 0, "ARIAL", "APPOINTMENT DETAILS");
                TSCLIB_DLL.windowsfont(50, 70, 30, 0, 0, 0, "ARIAL", "Supervisee Information");
                TSCLIB_DLL.windowsfont(70, 100, 25, 0, 0, 0, "ARIAL", "Name           : " + appointmentDetails.Name);
                TSCLIB_DLL.windowsfont(70, 130, 25, 0, 0, 0, "ARIAL", "NRICNo.      : " + appointmentDetails.NRICNo);
                
                TSCLIB_DLL.windowsfont(50, 180, 30, 0, 0, 0, "ARIAL", "Next Appointment");
                TSCLIB_DLL.windowsfont(70, 210, 25, 0, 0, 0, "ARIAL", "Date              : " + appointmentDetails.Date.ToString("dd/MM/yyyyy HH:mm"));

                // TSCLIB_DLL.windowsfont(100, 230, 25, 0, 0, 0, "ARIAL", "Printed at: " + DateTime.Now.ToString("dd/MM/yyyyy HH:mm"));

                //Download PCX file into printer
                TSCLIB_DLL.downloadpcx("UL.PCX", "UL.PCX");
                //Drawing PCX graphic
                TSCLIB_DLL.sendcommand("PUTPCX 100,400,\"UL.PCX\"");
                //Print labels
                TSCLIB_DLL.printlabel("1", "1");
                TSCLIB_DLL.closeport();

                return true;
            }
            catch (Exception ex)
            {

                return false;
            }
        }

        public override EnumDeviceStatuses[] GetDeviceStatus()
        {
            if (IsPrinterConnected(_printerName))
            {
                return new EnumDeviceStatuses[] { EnumDeviceStatuses.Connected };
            }
            else
            {
                return new EnumDeviceStatuses[] { EnumDeviceStatuses.Disconnected };
            }
        }

        private bool IsPrinterConnected(string printerName)
        {
            try
            {
                //ManagementScope scope = new ManagementScope(@"\root\cimv2");
                //scope.Connect();

                // Select Printers from WMI Object Collections
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Printer");

                string printerTempName = string.Empty;
                foreach (ManagementObject printer in searcher.Get())
                {
                    printerTempName = printer["Name"].ToString().ToUpper();
                    if (printerTempName.Equals(printerName))
                    {
                        if (printer["WorkOffline"].ToString().ToLower().Equals("true"))
                        {
                            // printer is offline by user
                            Debug.WriteLine(printerName + ": printer is offline by user.");
                            return false;
                        }
                        else
                        {
                            // printer is not offline
                            Debug.WriteLine(printerName + ": printer is connected.");
                            return true;
                        }
                    }
                }

                Debug.WriteLine(printerName + ": printer is not connected.");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("IsPrinterConnected exception: " + ex.ToString());
                return false;
            }
        }
    }
}
