using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Trinity.Common;
using Trinity.Common.Common;
using Trinity.Common.DeviceMonitor;
using Trinity.Common.Monitor;
using Trinity.Common.Utils;
using Trinity.DAL;

namespace DocumentScannerTest
{
    class Program
    {
        static private Font printFont;
        static private StreamReader streamToPrint;
        static string filePath;

        static void Main(string[] args)
        {
            // for testing only

            // DocumentScanner
            // StartDocumentScannerMonitor();

            // TSC Bar code scanner
            //ResetPagePossition();
            string TTLabelPrinterName = ConfigurationManager.AppSettings["TTLabelPrinterName"];
            TTLabelInfo labelInfo = new TTLabelInfo()
            {
                ID = "ABC20180107XYZ",
                Name = "Ricardo Quaresma",
                MarkingNumber = "CSA17000809"
            };
            //BarcodePrinterUtils.Instance.ResetPagePossition(TTLabelPrinterName);
            //BarcodePrinterUtils.Instance.Print(labelInfo);
            //BarcodePrinterUtils.Instance.Print();
            //BarcodePrinterUtils.Instance.ResetPagePossition(TTLabelPrinterName);
            //StartBarcodeScanner();
            //StartBarcodeScanner2();

            // PrinterMonitor checkstatus
            //ReportPrinterStatus();

            // StartIdentification
            //TestStartIdentification();
            //TestStartIdentification();
            //TestStartIdentification();
        }

        private static void ResetPagePossition()
        {
            try
            {
                string TTLabelPrinterName = ConfigurationManager.AppSettings["TTLabelPrinterName"];
                    
                PrintDocument printDocument = new PrintDocument();
                printDocument.PrinterSettings.PrinterName = TTLabelPrinterName;
                printDocument.PrintPage += new PrintPageEventHandler(pd_PrintPage);

                // Print the document.
                printDocument.Print();
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
        }

        private static void pd_PrintPage(object sender, PrintPageEventArgs ev)
        {
            float linesPerPage = 0;
            float yPos = 0;
            int count = 0;
            float leftMargin = ev.MarginBounds.Left;
            float topMargin = ev.MarginBounds.Top;
            String line = null;

            ev.Graphics.DrawString("reset position", new Font("Arial", 10), Brushes.Black, PointF.Empty);

            // Calculate the number of lines per page.
            //linesPerPage = ev.MarginBounds.Height /
            //   printFont.GetHeight(ev.Graphics);

            // Iterate over the file, printing each line.
            //while (count < linesPerPage &&
            //   ((line = streamToPrint.ReadLine()) != null))
            //{
            //    yPos = topMargin + (count * printFont.GetHeight(ev.Graphics));
            //    ev.Graphics.DrawString(line, printFont, Brushes.Black,
            //       leftMargin, yPos, new StringFormat());
            //    count++;
            //}

            // If more lines exist, print another page.
            //if (line != null)
            //    ev.HasMorePages = true;
            //else
            //    ev.HasMorePages = false;
        }

        private static void StartBarcodeScanner2()
        {
            Console.WriteLine("starting StartBarcodeScanner2...");


            string printerName = ConfigurationManager.AppSettings["TTLabelPrinterName"];
            var status = BarcodePrinterUtils.Instance.GetDeviceStatus(printerName);

            Console.ReadKey();
        }

        private static void TestStartIdentification()
        {
            Console.WriteLine("Starting TestStartIdentification ...");
            DAL_User dAL_User = new DAL_User();
            var users = dAL_User.GetAllSupervisees(true);

            List<byte[]> lstFingerprint_Templates = new List<byte[]>();
            foreach (var item in users)
            {
                lstFingerprint_Templates.Add(item.LeftThumbFingerprint);
                lstFingerprint_Templates.Add(item.RightThumbFingerprint);
            }

            Trinity.Common.FingerprintReaderUtils fingerprintReaderUtils = Trinity.Common.FingerprintReaderUtils.Instance;
            fingerprintReaderUtils.StartIdentification(lstFingerprint_Templates, IdentificationCompleted);

            Console.ReadKey();
        }

        private static void IdentificationCompleted(bool result)
        {
            Console.WriteLine("return value: " + result);
        }

        //private static void ReportPrinterStatus()
        //{
        //    Console.WriteLine("ReportPrinterStatus is starting...");
        //    FingerprintMonitor fingerprintMonitor = FingerprintMonitor.Instance;
        //    fingerprintMonitor.OnGetDeviceStatusCompleted += OnGetDeviceStatusCompleted;
        //    //fingerprintMonitor.StartVerification(OnVerificationComplete, new byte[10]);
        //    fingerprintMonitor.StartCheckingDeviceStatus();

        //    Console.ReadKey();
        //}

        private static void OnVerificationComplete(bool bSuccess, int nResult, bool bVerificationSuccess)
        {

            Console.WriteLine("OnVerificationComplete :" + bSuccess);
        }

        private static void OnGetDeviceStatusCompleted(object sender, GetDeviceStatusCompletedArgs e)
        {
            Console.WriteLine("OnGetDeviceStatusCompleted :" + e.IsConnected);

            // report the result to server
        }

        private static void StartBarcodeScanner()
        {
            try
            {
                Console.WriteLine("StartBarcodeScanner is starting...");
                //TSCLIB_DLL.about();                                                                 //Show the DLL version
                //BarcodeScannerUtils.openport("TSC TTP-244 PRO");                                           //Open specified printer driver
                //                                                                                           // 2.8"x1.4"
                //BarcodeScannerUtils.setup("71.12", "42.5", "4", "8", "0", "0", "0");                           //Setup the media size and sensor type info
                //BarcodeScannerUtils.clearbuffer();                                                           //Clear image buffer
                //BarcodeScannerUtils.barcode("170", "0", "128", "100", "1", "0", "2", "6", "S1234567G"); //Drawing barcode
                //                                                                               //TSCLIB_DLL.printerfont("100", "300", "3", "0", "1", "1", "Print Font Test");        //Drawing printer font
                //                                                                               // max 35 chars per line
                //                                                                               // 20 units per char => 700~ units per line
                //int x = (35 - userName.Length) / 2;
                //BarcodeScannerUtils.windowsfont(x * 17, 150, 40, 0, 0, 0, "ARIAL", userName);  //Draw windows font
                //BarcodeScannerUtils.windowsfont(210, 200, 36, 0, 0, 0, "ARIAL", "01/01/1990");  //Draw windows font
                //BarcodeScannerUtils.downloadpcx("UL.PCX", "UL.PCX");                                         //Download PCX file into printer
                //BarcodeScannerUtils.sendcommand("PUTPCX 100,400,\"UL.PCX\"");                                //Drawing PCX graphic
                //BarcodeScannerUtils.printlabel("1", "1");                                                    //Print labels
                //BarcodeScannerUtils.closeport();

                PrinterMonitor printerMonitor = PrinterMonitor.Instance;
                printerMonitor.OnPrintLabelSucceeded += OnPrintUserInfo_BarcodeSucceeded;
                printerMonitor.OnMonitorException += OnMonitorException;

                var test = DeviceManagement.GetUSBDevices();

                BarcodePrinterUtils barcodeScannerUtils = BarcodePrinterUtils.Instance;
                LabelInfo labelInfo = new LabelInfo()
                {
                    Name = "Avril Lavigne",
                    NRIC = "S1234567G",
                    Date = "01/01/1970"
                };

                string printerName = ConfigurationManager.AppSettings["TTLabelPrinterName"];
                var statusPrinterBarcode = barcodeScannerUtils.GetDeviceStatus(printerName);

                if (statusPrinterBarcode.Contains(EnumDeviceStatuses.Connected))
                {
                    printerMonitor.PrintBarcodeLabel(labelInfo);
                }
                else
                {
                    Console.WriteLine("Barcode printer is not connected.");
                }

                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadKey();
            }
        }

        private static void OnPrintUserInfo_BarcodeSucceeded(object sender, PrintMUBAndTTLabelsSucceedEventArgs e)
        {
            Console.WriteLine("StartBarcodeScanner is succeeded...");
        }

        private static void StartDocumentScannerMonitor()
        {
            Console.WriteLine("DocumentScannerTest is starting...");

            DocumentScannerMonitor documentScannerMonitor = DocumentScannerMonitor.Instance;
            documentScannerMonitor.OnMonitorException += OnMonitorException;
            documentScannerMonitor.OnMonitorException -= OnMonitorException;
            documentScannerMonitor.OnMonitorException += OnMonitorException;
            documentScannerMonitor.OnMonitorException += OnMonitorException;
            var result = documentScannerMonitor.ScanDocument();

            if (result != null)
            {
                Console.WriteLine("Scan document is completed.");
            }

            Console.ReadKey();

        }

        private static void OnMonitorException(object sender, ExceptionArgs e)
        {
            Console.Write($"{e.ErrorCode}: {e.ErrorMessage}");
        }


        public class TSCLIB_DLL
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

        }
    }
}
