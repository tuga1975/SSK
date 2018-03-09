using System;
using System.Diagnostics;
using System.Management;
using System.Windows.Forms;
using Trinity.Common;

namespace Trinity.Device.Util
{
    public class ReceiptPrinterUtil : DeviceUtil
    {
        private string _printerName;

        #region Singleton Implementation
        // The variable is declared to be volatile to ensure that assignment to the instance variable completes before the instance variable can be accessed
        private static volatile ReceiptPrinterUtil _instance;

        // Uses a syncRoot instance to lock on, rather than locking on the type itself, to avoid deadlocks.
        private static object syncRoot = new Object();

        private ReceiptPrinterUtil()
        {
            _printerName = EnumDeviceNames.ReceiptPrinter;
        }

        public static ReceiptPrinterUtil Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (_instance == null)
                            _instance = new ReceiptPrinterUtil();
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

                int startX = 0;
                int startX_Detail = startX + 100;
                int startY = 0;
                int fontHeight = 30;
                int space = 30;
                string fontName = "ARIAL";

                //Setup the media size and sensor type info
                // page size 2.8"x1.4"
                // actually 71.12mm x 42.5mm
                // TSCLIB_DLL.setup("71.12", "42.5", "4", "8", "0", "0", "0");
                TSCLIB_DLL.setup("71.12", "100", "4", "8", "0", "0", "0").ToString();

                //Clear image buffer
                TSCLIB_DLL.clearbuffer();

                //Draw windows font
                // Company name line
                TSCLIB_DLL.windowsfont(startX, startY, fontHeight + 20, 0, 2, 1, fontName, appointmentDetails.CompanyName);
                // Next appointment line
                TSCLIB_DLL.windowsfont(startX, startY += fontHeight + 20 + space, fontHeight + 10, 0, 2, 0, fontName, "Next Appointment");
                // Name line
                TSCLIB_DLL.windowsfont(startX, startY += fontHeight + 10 + space, fontHeight, 0, 0, 0, fontName, "Name");
                TSCLIB_DLL.windowsfont(startX_Detail, startY, fontHeight, 0, 0, 0, fontName, ": " + appointmentDetails.Name);
                // Date line
                TSCLIB_DLL.windowsfont(startX, startY += fontHeight, fontHeight, 0, 0, 0, fontName, "Date");
                TSCLIB_DLL.windowsfont(startX_Detail, startY, fontHeight, 0, 0, 0, fontName, ": " + appointmentDetails.Date.ToString("dd/MM/yyyy"));
                // Time line
                TSCLIB_DLL.windowsfont(startX, startY += fontHeight, fontHeight, 0, 0, 0, fontName, "Time");
                TSCLIB_DLL.windowsfont(startX_Detail, startY, fontHeight, 0, 0, 0, fontName, ": " + (appointmentDetails.StartTime.HasValue ? appointmentDetails.StartTime.Value.ToString(@"hh\:mm") : ""));
                // Venue line
                TSCLIB_DLL.windowsfont(startX, startY += fontHeight, fontHeight, 0, 0, 0, fontName, "Venue");
                TSCLIB_DLL.windowsfont(startX_Detail, startY, fontHeight, 0, 0, 0, fontName, ": " + appointmentDetails.Venue);
                // Note line
                TSCLIB_DLL.windowsfont(startX, startY += fontHeight + space, fontHeight, 0, 2, 0, fontName, "Please be punctual to avoid cancellation");
                TSCLIB_DLL.windowsfont(startX, startY += fontHeight, fontHeight, 0, 2, 0, fontName, "of appointment.");
                // printed date line
                TSCLIB_DLL.windowsfont(startX + 272, startY += fontHeight + space + space, fontHeight - 4, 0, 1, 0, fontName, "Printed date: " + DateTime.Now.ToString("dd/MM/yyyy"));
                // printed time line
                TSCLIB_DLL.windowsfont(startX + 272, startY += fontHeight, fontHeight - 4, 0, 1, 0, fontName, "Printed time: " + DateTime.Now.ToString("hh:mm tt"));

                TSCLIB_DLL.printlabel("1", "1");
                TSCLIB_DLL.closeport();

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return false;
            }
        }

        public override EnumDeviceStatus[] GetDeviceStatus()
        {
            if (IsPrinterConnected(_printerName?.ToUpper()))
            {
                return new EnumDeviceStatus[] { EnumDeviceStatus.Connected };
            }
            else
            {
                return new EnumDeviceStatus[] { EnumDeviceStatus.Disconnected };
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
