using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Printing;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Trinity.Common.Common;

namespace Trinity.Common.Utils
{
    public class BarcodePrinterUtils
    {
        #region Singleton Implementation
        // The variable is declared to be volatile to ensure that assignment to the instance variable completes before the instance variable can be accessed
        private static volatile BarcodePrinterUtils _instance;

        // Uses a syncRoot instance to lock on, rather than locking on the type itself, to avoid deadlocks.
        private static object syncRoot = new Object();

        private BarcodePrinterUtils() { }

        public static BarcodePrinterUtils Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (_instance == null)
                            _instance = new BarcodePrinterUtils();
                    }
                }

                return _instance;
            }
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName">user name: can not be null</param>
        /// <param name="nric">nric (e.g S1234567G)</param>
        /// <param name="dob">date of birth (dd/MM/yyyy)</param>
        /// <returns>call printer success or not</returns>
        public bool PrintUserInfo(UserInfo userInfo)
        {
            try
            {
                // user name can not be null
                if (string.IsNullOrEmpty(userInfo.UserName) || string.IsNullOrEmpty(userInfo.NRIC))
                {
                    return false;
                }

                // barcodePrinter = "TSC TTP-244 Pro"
                string barcodePrinter = ConfigurationManager.AppSettings["BarcodePrinterName"];
                //TSCLIB_DLL.about();  //Show the DLL version
                TSCLIB_DLL.openport(barcodePrinter);                                           //Open specified printer driver
                // page size 2.8"x1.4"
                // actually 71.12mm x 42.5mm
                TSCLIB_DLL.setup("71.12", "42.5", "4", "8", "0", "0", "0");                           //Setup the media size and sensor type info
                TSCLIB_DLL.clearbuffer();                                                           //Clear image buffer
                TSCLIB_DLL.barcode("170", "0", "128", "100", "1", "0", "2", "6", userInfo.NRIC); //Drawing barcode
                //TSCLIB_DLL.printerfont("100", "300", "3", "0", "1", "1", "Print Font Test");        //Drawing printer font
                // max 35 chars per line
                // 20~ units per char => 700~ units per line
                int x = (35 - userInfo.UserName.Length) / 2;
                TSCLIB_DLL.windowsfont(x * 17, 150, 40, 0, 0, 0, "ARIAL", userInfo.UserName);  //Draw windows font
                TSCLIB_DLL.windowsfont(210, 200, 36, 0, 0, 0, "ARIAL", userInfo.DOB);  //Draw windows font
                TSCLIB_DLL.downloadpcx("UL.PCX", "UL.PCX");                                         //Download PCX file into printer
                TSCLIB_DLL.sendcommand("PUTPCX 100,400,\"UL.PCX\"");                                //Drawing PCX graphic
                TSCLIB_DLL.printlabel("1", "1");                                                    //Print labels
                TSCLIB_DLL.closeport();

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("BarcodeScannerUtils StartPrinting exception: " + ex.ToString());
                return false;
            }
        }

        public bool PrintQRCodeUserInfo(UserInfo userInfo)
        {
            try
            {
                // user name can not be null
                if (string.IsNullOrEmpty(userInfo.UserName) || string.IsNullOrEmpty(userInfo.NRIC))
                {
                    return false;
                }
                
                // The path of file imge contain QRCode
                string fileName = String.Format("{0}/View/img/{1}", System.IO.Directory.GetCurrentDirectory().ToLower().Replace("\\bin\\debug", string.Empty), "QRCode_" + userInfo.NRIC + ".png");

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("BarcodeScannerUtils StartPrinting exception: " + ex.ToString());
                return false;
            }
        }
        
        /// <summary>
        /// get barcode printer status
        /// </summary>
        /// <returns>PrinterStatus</returns>
        public EnumDeviceStatuses[] GetDeviceStatus()
        {
            // get barcodePrinter name from appconfig
            string barcodePrinterName = ConfigurationManager.AppSettings["BarcodePrinterName"].ToUpper();

            // check printer is connected or not
            if (IsPrinterConnected(barcodePrinterName))
            {
                return new EnumDeviceStatuses[] { EnumDeviceStatuses.Connected };
            }
          

            // Get a list of available printers (installed printers).
            var printServer = new PrintServer();
            var printQueues = printServer.GetPrintQueues(new[] { EnumeratedPrintQueueTypes.Local, EnumeratedPrintQueueTypes.Connections });

            // Get barcode printer
            PrintQueue printQueue = printQueues.FirstOrDefault(p => p.Name.ToUpper() == barcodePrinterName);
           
            // if barcode printer is null, return disconnected
            if (printQueue == null)
            {
                return new EnumDeviceStatuses[] { EnumDeviceStatuses.Disconnected };
            }
            else
            {
                //return list status here
                var status = printQueue.QueueStatus;
                return new EnumDeviceStatuses[] { (EnumDeviceStatuses)status };
            }
            
        }


        private bool IsPrinterConnected(string barcodePrinterName)
        {
            try
            {
                //ManagementScope scope = new ManagementScope(@"\root\cimv2");
                //scope.Connect();

                // Select Printers from WMI Object Collections
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Printer");

                string printerName = "";
                foreach (ManagementObject printer in searcher.Get())
                {
                    printerName = printer["Name"].ToString().ToUpper();
                    if (printerName.Equals(barcodePrinterName))
                    {
                        if (printer["WorkOffline"].ToString().ToLower().Equals("true"))
                        {
                            // printer is offline by user
                            Debug.WriteLine(barcodePrinterName + ": Your barcode printer is not connected.");
                            return false;
                        }
                        else
                        {
                            // printer is not offline
                            Debug.WriteLine(barcodePrinterName + ": Your barcode printer is connected.");
                            return true;
                        }
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("IsPrinterConnected exception: " + ex.ToString());
                return false;
            }
        }
    }

    class TSCLIB_DLL
    {
        [DllImport("TSCLIB.dll", EntryPoint = "about")]
        public static extern int about();

        [DllImport("TSCLIB.dll", EntryPoint = "openport")]
        public static extern int openport(string printername);

        [DllImport("TSCLIB.dll", EntryPoint = "barcode")]
        public static extern int barcode(string x, string y, string type,
                    string height, string readable, string rotation,
                    string narrow, string wide, string code);

        [DllImport("TSCLIB.dll", EntryPoint = "clearbuffer")]
        public static extern int clearbuffer();

        [DllImport("TSCLIB.dll", EntryPoint = "closeport")]
        public static extern int closeport();

        [DllImport("TSCLIB.dll", EntryPoint = "downloadpcx")]
        public static extern int downloadpcx(string filename, string image_name);

        [DllImport("TSCLIB.dll", EntryPoint = "formfeed")]
        public static extern int formfeed();

        [DllImport("TSCLIB.dll", EntryPoint = "nobackfeed")]
        public static extern int nobackfeed();

        [DllImport("TSCLIB.dll", EntryPoint = "printerfont")]
        public static extern int printerfont(string x, string y, string fonttype,
                        string rotation, string xmul, string ymul,
                        string text);

        [DllImport("TSCLIB.dll", EntryPoint = "printlabel")]
        public static extern int printlabel(string set, string copy);

        [DllImport("TSCLIB.dll", EntryPoint = "sendcommand")]
        public static extern int sendcommand(string printercommand);

        [DllImport("TSCLIB.dll", EntryPoint = "setup")]
        public static extern int setup(string width, string height,
                  string speed, string density,
                  string sensor, string vertical,
                  string offset);

        [DllImport("TSCLIB.dll", EntryPoint = "windowsfont")]
        public static extern int windowsfont(int x, int y, int fontheight,
                        int rotation, int fontstyle, int fontunderline,
                        string szFaceName, string content);

        #region for testing purposeg 
        //userName = "John Smith";  // 16
        ////TSCLIB_DLL.about();                                                                 //Show the DLL version
        //TSCLIB_DLL.openport("TSC TTP-244 PRO");                                           //Open specified printer driver
        //// 2.8"x1.4"
        //        TSCLIB_DLL.setup("71.12", "42.5", "4", "8", "0", "0", "0");                           //Setup the media size and sensor type info
        //        TSCLIB_DLL.clearbuffer();                                                           //Clear image buffer
        //        TSCLIB_DLL.barcode("170", "0", "128", "100", "1", "0", "2", "6", "S1234567G"); //Drawing barcode
        //                                                                                       //TSCLIB_DLL.printerfont("100", "300", "3", "0", "1", "1", "Print Font Test");        //Drawing printer font
        //                                                                                       // max 35 chars per line
        //                                                                                       // 20 units per char => 700 units per line
        //        int x = (35 - userName.Length) / 2;
        //        TSCLIB_DLL.windowsfont(x* 17, 150, 40, 0, 0, 0, "ARIAL", userName);  //Draw windows font
        //        TSCLIB_DLL.windowsfont(210, 200, 36, 0, 0, 0, "ARIAL", "01/01/1990");  //Draw windows font
        //        TSCLIB_DLL.downloadpcx("UL.PCX", "UL.PCX");                                         //Download PCX file into printer
        //        TSCLIB_DLL.sendcommand("PUTPCX 100,400,\"UL.PCX\"");                                //Drawing PCX graphic
        //        TSCLIB_DLL.printlabel("1", "1");                                                    //Print labels
        //        TSCLIB_DLL.closeport();
        #endregion
    }
}
