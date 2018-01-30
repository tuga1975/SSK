using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Management;
using Trinity.Common;

namespace Trinity.Util
{
    public class BarcodePrinterUtil
    {
        #region Singleton Implementation
        // The variable is declared to be volatile to ensure that assignment to the instance variable completes before the instance variable can be accessed
        private static volatile BarcodePrinterUtil _instance;

        // Uses a syncRoot instance to lock on, rather than locking on the type itself, to avoid deadlocks.
        private static object syncRoot = new Object();

        private BarcodePrinterUtil() { }

        public static BarcodePrinterUtil Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (_instance == null)
                            _instance = new BarcodePrinterUtil();
                    }
                }

                return _instance;
            }
        }
        #endregion

        public bool PrintQRCodeUserInfo(LabelInfo labelInfo)
        {
            try
            {
                // user name can not be null
                if (string.IsNullOrEmpty(labelInfo.Name) || string.IsNullOrEmpty(labelInfo.NRIC))
                {
                    return false;
                }

                // The path of file imge contain QRCode
                string fileName = String.Format("{0}/Temp/{1}", System.IO.Directory.GetCurrentDirectory().ToLower().Replace("\\bin\\debug", string.Empty), "QRCode_" + labelInfo.NRIC + ".png");

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("BarcodeScannerUtils StartPrinting exception: " + ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// print TTLabel
        /// </summary>
        /// <param name="ttLabelInfo">Id, Name, MarkingNumber must not be null</param>
        /// <returns>successful</returns>
        public bool PrintTTLabel(TTLabelInfo ttLabelInfo)
        {
            try
            {
                // validate
                if (!ttLabelInfo.IsValid())
                {
                    return false;
                }

                // get printer name
                string printerName = EnumDeviceNames.TTLabelPrinter;

                //Open specified printer driver
                TSCLIB_DLL.openport(printerName);

                //Setup the media size and sensor type info
                // page size 2.8"x1.4"
                // actually 71.12mm x 42.5mm
                TSCLIB_DLL.setup("71.12", "42.5", "4", "8", "0", "0", "0");

                //Clear image buffer
                TSCLIB_DLL.clearbuffer();

                //Draw windows font
                TSCLIB_DLL.windowsfont(0, 0, 40, 0, 0, 0, "ARIAL", "Name: " + ttLabelInfo.Name);
                TSCLIB_DLL.windowsfont(0, 50, 40, 0, 0, 0, "ARIAL", "ID: " + ttLabelInfo.ID);

                //Drawing barcode
                TSCLIB_DLL.barcode("0", "100", "128", "100", "0", "0", "4", "6", ttLabelInfo.MarkingNumber);

                // Drawing barcode buildin function do not let us set text size of readable line, so we need to draw a line to display MarkingNumber
                TSCLIB_DLL.windowsfont(0, 200, 40, 0, 0, 0, "ARIAL", ttLabelInfo.MarkingNumber);

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
                Debug.WriteLine("Print exception: " + ex.ToString());
                return false;
            }
        }
        
        public bool PrintMUBLabel(string filePath)
        {
            try
            {
                // get printer name
                string printerName = EnumDeviceNames.MUBLabelPrinter;

                //// create printDocument
                //PrintDocument printDocument = new PrintDocument();
                //printDocument.PrinterSettings.PrinterName = printerName;
                //printDocument.DefaultPageSettings.Landscape = true; //or false!

                //PrintDocument pd = new PrintDocument();
                //pd.PrintPage += (sender, args) =>
                //{
                //    //Image image = Image.FromFile("C://Users//DucTu//Desktop//printimage2.png");
                //    ////Point p = new Point(100, 100);
                //    //args.Graphics.DrawImage(image, 0, 0);
                //    args.Graphics.DrawImage(MUBLabelImage, 0, 0);
                //};
                //pd.Print();

                //Open specified printer driver
                TSCLIB_DLL.openport(printerName);

                //Setup the media size and sensor type info
                // page size 2.8"x1.4"
                // actually 71.12mm x 42.5mm
                TSCLIB_DLL.setup("71.12", "42.5", "4", "8", "0", "0", "0");

                //Clear image buffer
                TSCLIB_DLL.clearbuffer();

                // Download PCX file into printer
                //TSCLIB_DLL.downloadpcx("UL.PCX", "UL.PCX");
                //Drawing PCX graphic
                TSCLIB_DLL.downloadpcx(filePath, "mublabel.bmp");  //Download PCX file into printer

                TSCLIB_DLL.sendcommand("PUTBMP 1,1, \"mublabel.bmp\""); //Drawing PCX graphic

                //Print labels
                //TSCLIB_DLL.printlabel("1", "1");
                TSCLIB_DLL.sendcommand("PRINT 1");
                TSCLIB_DLL.closeport();

                // Delete temp file
                File.Delete(filePath);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Print exception: " + ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// reset page position 
        /// </summary>
        /// <param name="printerName">printerName need to be reset page possition</param>
        public void ResetPagePossition(string printerName)
        {
            try
            {
                // create printDocument
                PrintDocument printDocument = new PrintDocument();
                printDocument.PrinterSettings.PrinterName = printerName;
                printDocument.PrintPage += new PrintPageEventHandler(PrintDocument_PrintPage);

                // Print the document.
                printDocument.Print();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ResetPagePossition exception: " + ex.ToString());
            }
        }

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs ev)
        {
            float leftMargin = ev.MarginBounds.Left;
            float topMargin = ev.MarginBounds.Top;

            ev.Graphics.DrawString("Initialize printer.", new Font("Arial", 10), Brushes.Black, leftMargin, topMargin);
        }

        /// <summary>
        /// get barcode printer status
        /// </summary>
        /// <returns>PrinterStatus</returns>
        public EnumDeviceStatuses[] GetDeviceStatus(string printerName)
        {
            // create default returnValue
            //List< EnumDeviceStatuses> returnValue = new List<EnumDeviceStatuses>();

            // check printer is connected or not
            // check with Win32_Printer
            if (IsPrinterConnected(printerName?.ToUpper()))
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
                            Debug.WriteLine(printerName + ": printer is not connected.");
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
