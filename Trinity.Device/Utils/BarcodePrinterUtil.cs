using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Management;
using System.Threading;
using System.Windows.Forms;
using Trinity.Common;

namespace Trinity.Device.Util
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
                //MessageBox.Show("PrintTTLabel ");
                // validate
                if (!ttLabelInfo.IsValid())
                {
                    MessageBox.Show("!ttLabelInfo.IsValid");
                    return false;
                }

                //Open specified printer driver

                // Wait for 200 miliseconds
                Thread.Sleep(200);
                //MessageBox.Show(EnumDeviceNames.TTLabelPrinter);
                TSCLIB_DLL.openport(EnumDeviceNames.TTLabelPrinter);

                //Setup the media size and sensor type info
                // page size 55mm x 30mm
                // template size 45mm x 30mm (actually 55mm x 32.5mm)
                TSCLIB_DLL.setup("55", "32.5", "4", "8", "0", "0", "0");
                //MessageBox.Show("Setup compeleted");

                //Clear image buffer
                TSCLIB_DLL.clearbuffer();

                // DPI = 203 => 8px = 1 mm
                //Draw windows font
                int startX = 54;
                int startY = 8;
                string fontName = "ARIAL";
                int fontStyle = 2; // Bold
                int fontHeight = 30;
                int maxChar = 17;   // max char of name at first name line

                // Name line
                TSCLIB_DLL.windowsfont(startX, startY, fontHeight, 0, fontStyle, 0, fontName, "Name");
                if (ttLabelInfo.Name.Length > maxChar)
                {
                    TSCLIB_DLL.windowsfont(startX + 70, startY, fontHeight, 0, fontStyle, 0, fontName, " : " + ttLabelInfo.Name.Substring(0, maxChar));
                    // Add name line if name is too long. Need to improve (split name by space char)
                    TSCLIB_DLL.windowsfont(startX + 100, startY += fontHeight, fontHeight, 0, fontStyle, 0, fontName, "-" + ttLabelInfo.Name.Substring(maxChar, ttLabelInfo.Name.Length - maxChar));
                }
                else
                {
                    TSCLIB_DLL.windowsfont(startX + 70, startY, fontHeight, 0, fontStyle, 0, fontName, " : " + ttLabelInfo.Name);
                }

                // ID line
                TSCLIB_DLL.windowsfont(startX, startY += fontHeight, fontHeight, 0, fontStyle, 0, fontName, "ID");
                TSCLIB_DLL.windowsfont(startX + 70, startY, fontHeight, 0, fontStyle, 0, fontName, " : " + ttLabelInfo.ID);

                //Drawing barcode
                TSCLIB_DLL.barcode(startX.ToString(), (startY += fontHeight + 8).ToString(), "39", "72", "0", "0", "1", "3", ttLabelInfo.MarkingNumber);

                // Drawing barcode buildin function do not let us set text size of readable line, so we need to draw a line to display MarkingNumber
                TSCLIB_DLL.windowsfont(startX, startY += 80, fontHeight, 0, fontStyle, 0, fontName, ttLabelInfo.MarkingNumber);

                //Download PCX file into printer
                //TSCLIB_DLL.downloadpcx("UL.PCX", "UL.PCX");
                //Drawing PCX graphic
                //TSCLIB_DLL.sendcommand("PUTPCX 100,400,\"UL.PCX\"");
                //Print labels
                TSCLIB_DLL.printlabel("1", "1");
                TSCLIB_DLL.closeport();

                //MessageBox.Show("Print OK");
                return true;
            }
            catch (Exception ex)
            {
                //Debug.WriteLine("Print exception: " + ex.ToString());
                MessageBox.Show("Print exception: " + ex.ToString());
                return false;
            }
        }

        public bool PrintMUBLabel(MUBLabelInfo mubLabelInfo)
        {
            try
            {
                // validate
                if (!mubLabelInfo.IsValid())
                {
                    //MessageBox.Show("Model is not valid.");
                    return false;
                }

                //Open specified printer driver
                TSCLIB_DLL.openport(EnumDeviceNames.MUBLabelPrinter);

                //Setup the media size and sensor type info
                // page size 55mm x 100mm
                // template size 55mm x 100mm (actually 56mm x 82mm)
                TSCLIB_DLL.setup("55", "82.5", "4", "8", "0", "0", "0");

                //Clear image buffer
                TSCLIB_DLL.clearbuffer();

                // DPI = 203 => 8px = 1 mm
                //Draw windows font
                int startX = 348;
                int startY = 134;
                int startY_Value = startY + 160;
                string fontName = "ARIAL";
                int fontStyle = 0; // Normal
                int fontHeight = 30;
                int lineSpacing = 10;
                int maxChar = 17;   // max char of name at first name line
                int rotation = 270;

                // Title line
                TSCLIB_DLL.windowsfont(startX, 48, fontHeight + 8, rotation, 2, 0, fontName, "CENTRAL NARCOTICS BUREAU");
                //TSCLIB_DLL.windowsfont(startX-54, 0, fontHeight + 8, rotation, fontStyle, 0, fontName, "| Start");
                //TSCLIB_DLL.windowsfont(startX-54, 600, fontHeight + 6, rotation, fontStyle, 0, fontName, "| End");


                // Name line
                TSCLIB_DLL.windowsfont(startX -= 64, startY, fontHeight, rotation, fontStyle, 0, fontName, "Name");
                if (mubLabelInfo.Name.Length > maxChar)
                {
                    TSCLIB_DLL.windowsfont(startX, startY_Value, fontHeight, rotation, fontStyle, 0, fontName, " : " + mubLabelInfo.Name.Substring(0, maxChar));
                    // Addition line if name is too long. Need to improve (detech addition row by space char)
                    TSCLIB_DLL.windowsfont(startX -= (fontHeight + lineSpacing), startY_Value + 32, fontHeight, rotation, fontStyle, 0, fontName, "-" + mubLabelInfo.Name.Substring(maxChar, mubLabelInfo.Name.Length - maxChar));
                }
                else
                {
                    TSCLIB_DLL.windowsfont(startX, startY_Value, fontHeight, rotation, fontStyle, 0, fontName, " : " + mubLabelInfo.Name);
                    startX -= (fontHeight + lineSpacing);
                }

                // ID line
                TSCLIB_DLL.windowsfont(startX -= (fontHeight + lineSpacing), startY, fontHeight, rotation, fontStyle, 0, fontName, "ID No.");
                TSCLIB_DLL.windowsfont(startX, startY_Value, fontHeight, rotation, fontStyle, 0, fontName, " : " + mubLabelInfo.ID);

                // Date line
                TSCLIB_DLL.windowsfont(startX -= (fontHeight + lineSpacing), startY, fontHeight, rotation, fontStyle, 0, fontName, "Date");
                TSCLIB_DLL.windowsfont(startX, startY_Value, fontHeight, rotation, fontStyle, 0, fontName, " : " + DateTime.Now.ToString("dd/MM/yyyy"));

                // Marking no
                TSCLIB_DLL.windowsfont(startX -= (fontHeight + lineSpacing), startY, fontHeight, rotation, fontStyle, 0, fontName, "Marking No.");
                TSCLIB_DLL.windowsfont(startX, startY_Value, fontHeight, rotation, fontStyle, 0, fontName, " : " + mubLabelInfo.MarkingNumber);

                //Drawing barcode
                //TSCLIB_DLL.barcode(startX.ToString(), (startY += fontHeight + 8).ToString(), "39", "72", "0", "0", "1", "3", mubLabelInfo.QRCodeString);
                TSCLIB_DLL.sendcommand("DMATRIX 144,8,400,400,x3, \"" + mubLabelInfo.QRCodeString + "\"");

                //Download PCX file into printer
                //TSCLIB_DLL.downloadpcx("UL.PCX", "UL.PCX");
                //Drawing PCX graphic
                //TSCLIB_DLL.sendcommand("PUTPCX 100,400,\"UL.PCX\"");
                //Print labels
                TSCLIB_DLL.printlabel("1", "1");
                TSCLIB_DLL.closeport();

                return true;
            }
            catch (Exception ex)
            {
                //Debug.WriteLine("Print exception: " + ex.ToString());
                MessageBox.Show("Print exception: " + ex.ToString());
                return false;
            }
        }


        public bool PrintMUBLabel(string filePath)
        {
            try
            {
                // Open specified printer driver
                TSCLIB_DLL.openport(EnumDeviceNames.MUBLabelPrinter);

                // Setup the media size and sensor type info
                // page size 100mm x 55mm
                TSCLIB_DLL.setup("55", "100", "4", "8", "0", "0", "0");

                //Clear image buffer
                TSCLIB_DLL.clearbuffer();

                // Download PCX file into printer
                // TSCLIB_DLL.downloadpcx("UL.PCX", "UL.PCX");
                // Drawing PCX graphic
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
                MessageBox.Show(ex.Message);
                Debug.WriteLine("Print exception: " + ex.ToString());
                // Delete temp file
                File.Delete(filePath);
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
        public EnumDeviceStatus[] GetDeviceStatus(string printerName)
        {
            // create default returnValue
            //List< EnumDeviceStatuses> returnValue = new List<EnumDeviceStatuses>();

            // check printer is connected or not
            // check with Win32_Printer
            if (IsPrinterConnected(printerName?.ToUpper()))
            {
                return new EnumDeviceStatus[] { EnumDeviceStatus.Connected };
            }
            else
            {
                return new EnumDeviceStatus[] { EnumDeviceStatus.Disconnected };
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
