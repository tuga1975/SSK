using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using Trinity.Common.Common;

namespace Trinity.Common.Utils
{
    public class SmartCardPrinterUtils : DeviceUtils
    {
        private string _smartCardPrinterName;

        #region Singleton Implementation
        // The variable is declared to be volatile to ensure that assignment to the instance variable completes before the instance variable can be accessed
        private static volatile SmartCardPrinterUtils _instance;

        // Uses a syncRoot instance to lock on, rather than locking on the type itself, to avoid deadlocks.
        private static object syncRoot = new Object();

        private SmartCardPrinterUtils()
        {
            _smartCardPrinterName = ConfigurationManager.AppSettings["TTLabelPrinterName"];
        }

        public static SmartCardPrinterUtils Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (_instance == null)
                            _instance = new SmartCardPrinterUtils();
                    }
                }

                return _instance;
            }
        }
        #endregion

        public void PrintAndWriteSmartcardData(PrintAndWriteSmartcardInfo printAndWriteSmartcardInfo, Action<PrintAndWriteSmartcardResult> OnCompleted)
        {
            try
            {
                PrintAndWriteSmartcardResult printAndWriteSmartcardResult = new PrintAndWriteSmartcardResult()
                {
                    Success = true,
                    Description = string.Empty,
                    SmartCardData = new SmartCardData()
                    {
                        CardInfo = new CardInfo() { UID = "sample-UID", CreatedBy = "sample-userID", CreatedDate = DateTime.Now }
                    }
                };

                OnCompleted(printAndWriteSmartcardResult);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("PrintAndWriteSmartcardData exception: " + ex.ToString());

                OnCompleted(new PrintAndWriteSmartcardResult() { Success = false, Description = "Oops!.. Something went wrong ..." });
            }
        }

        public override EnumDeviceStatuses[] GetDeviceStatus()
        {
            // create default returnValue
            //List< EnumDeviceStatuses> returnValue = new List<EnumDeviceStatuses>();

            // check printer is connected or not
            // check with Win32_Printer
            if (IsPrinterConnected(_smartCardPrinterName))
            {
                return new EnumDeviceStatuses[] { EnumDeviceStatuses.Connected };
            }
            else
            {
                return new EnumDeviceStatuses[] { EnumDeviceStatuses.Disconnected };
            }

            // can not check printer status with PrintServer
            #region check printer status with PrintServer
            //// double check connected status with PrintServer
            //// Get a list of available printers (installed printers).
            //var printServer = new PrintServer();
            //var printQueues = printServer.GetPrintQueues(new[] { EnumeratedPrintQueueTypes.Local, EnumeratedPrintQueueTypes.Connections });

            //// Get barcode printer
            //PrintQueue printQueue = printQueues.FirstOrDefault(p => p.Name.ToUpper() == printerName);

            //// if barcode printer is null, return disconnected
            //if (printQueue == null)
            //{
            //    return new EnumDeviceStatuses[] { EnumDeviceStatuses.Disconnected };
            //}
            //else
            //{
            //    //return list status here
            //    var status = printQueue.QueueStatus;
            //    return new EnumDeviceStatuses[] { (EnumDeviceStatuses)status };
            //}
            #endregion
        }

        private bool IsPrinterConnected(string printerName)
        {
            try
            {
                //ManagementScope scope = new ManagementScope(@"\root\cimv2");
                //scope.Connect();

                // Select Printers from WMI Object Collections
                //ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Printer");

                //string printerTempName = string.Empty;
                //foreach (ManagementObject printer in searcher.Get())
                //{
                //    printerTempName = printer["Name"].ToString().ToUpper();
                //    if (printerTempName.Equals(printerName))
                //    {
                //        if (printer["WorkOffline"].ToString().ToLower().Equals("true"))
                //        {
                //            // printer is offline by user
                //            Debug.WriteLine(printerName + ": printer is not connected.");
                //            return false;
                //        }
                //        else
                //        {
                //            // printer is not offline
                //            Debug.WriteLine(printerName + ": printer is connected.");
                //            return true;
                //        }
                //    }
                //}

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
