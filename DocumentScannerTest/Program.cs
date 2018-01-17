using PCSC;
using PCSC.Iso7816;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Trinity.Common;
using Trinity.Common.Common;
using Trinity.Common.DeviceMonitor;
using Trinity.Common.Monitor;
using Trinity.Common.Utils;
using Trinity.DAL;
using ZMOTIFPRINTERLib;

namespace DocumentScannerTest
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SCARD_IO_REQUEST
    {
        public uint dwProtocol;
        public int cbPciLength;
    }
    
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
            //string TTLabelPrinterName = ConfigurationManager.AppSettings["TTLabelPrinterName"]?.ToUpper();
            //TTLabelInfo labelInfo = new TTLabelInfo()
            //{
            //    ID = "ABC20180107XYZ",
            //    Name = "Ricardo Quaresma",
            //    MarkingNumber = "CSA17000809"
            //};
            //BarcodePrinterUtils.Instance.ResetPagePossition(TTLabelPrinterName);
            //BarcodePrinterUtils.Instance.PrintTTLabel(labelInfo);
            //BarcodePrinterUtils.Instance.Print();
            //BarcodePrinterUtils.Instance.ResetPagePossition(TTLabelPrinterName);
            //StartBarcodeScanner();
            //StartBarcodeScanner2();

            // print appointment details
            //PrintAppointmentDetails();

            // PrinterMonitor checkstatus
            //ReportPrinterStatus();

            // StartIdentification
            //TestStartIdentification();
            //TestStartIdentification();
            //TestStartIdentification();

            // TestSmartCardPrinter
            //SmartCardReaderUtils smartCardReaderUtils = SmartCardReaderUtils.Instance;
            //smartCardReaderUtils.StartSmartCardReaderMonitor();
            //smartCardReaderUtils.StartSmartCardMonitor(onCardInitialized, onCardInserted, onCardRemoved);
            //TestEncodeContactlessSCard();
            
            //SmartCardReaderUtils smartCardReaderUtils = SmartCardReaderUtils.Instance;
            //bool writeSuccessful = smartCardReaderUtils.WriteData(data);

            //string getData = smartCardReaderUtils.ReadAllData_MifareClassic();
            //Console.WriteLine(getData);

            ////ReleaseCard();
            //TestSmartCardPrinter();
            string status = string.Empty;
            //SmartCardPrinterUtils.Instance.Print_Type1(EnumDeviceNames.SmartCardPrinterSerialNumber, null, "Front.bmp", "Back.bmp", ref status);
            //SmartCardPrinterUtils.Instance.Print_Label(EnumDeviceNames.SmartCardPrinterSerialNumber, "Front.bmp", "Back.bmp", ref status);
            //SmartCardPrinterUtils.Instance.PrintLabel("Front.bmp", "Back.bmp", PrintLabelCompleted);

            //SmartCardPrinterUtils.Instance.WriteData(WriteDataCompleted);

            SuperviseeBiodata superviseeBiodata = new SuperviseeBiodata()
            {
                UserId = Guid.NewGuid().ToString(),
                Name = "Wong Teck Seng",
                NRIC = "S999999G",
                SupervisionFrom = new DateTime(2018, 1, 1),
                SupervisionTo = new DateTime(2020, 1, 1),
                DrugProfile = "Cannabis NPS",
                SupervisionOfficer = "Lim Yong Tai",
                SupervisionContactNo = "81234567"
            };

            //SmartCardReaderUtils smartCardReaderUtils = SmartCardReaderUtils.Instance;
            //bool result2 = smartCardReaderUtils.ReadAllData_MifareClassic();
            //bool result = smartCardReaderUtils.WriteSuperviseeBiodata(superviseeBiodata);

            HistoricalRecord historicalRecord2 = new HistoricalRecord()
            {
                ReportingDate = DateTime.Now,
                CNB = "OK",
                HSAResult = "test",
                IUTResult = "something"
            };
            SmartCardData smartCardData = SmartCardData.Instance;
            DateTime startRead = DateTime.Now;
            smartCardData.ReadData_FromSmartCard();

            DateTime startWrite = DateTime.Now;
            bool writeResult = smartCardData.WriteHistoricalRecord(historicalRecord2);
            DateTime endWrite = DateTime.Now;

            string uid = smartCardData.CardUID;


            //DateTime date = new DateTime(2018, 1, 1);
            //for (int i = 0; i < 1000; i++)
            //{
            //    HistoricalRecord historicalRecord = new HistoricalRecord()
            //    {
            //        ReportingDate = date.AddDays(i),
            //        CNB = "OK" + i,
            //        HSAResult = "test",
            //        IUTResult = "something"
            //    };

            //    bool result1 = smartCardReaderUtils.WriteHistoricalRecord(historicalRecord);
            //    Console.WriteLine(i + ": " + result1);
            //}


            Console.ReadKey();

        }

        private static void WriteDataCompleted(string result)
        {
            Console.WriteLine("WriteData: " + result);
        }

            private static void PrintLabelCompleted(bool result)
        {
            Console.WriteLine("PrintLabel: " + result);
        }
        private static void ReleaseCard()
        {
            Job job = new Job();

            // Begin SDK communication with printer (using ZMotif SDK)
            //string deviceSerialNumber = "06C104500004";
            string deviceSerialNumber = EnumDeviceNames.SmartCardPrinterSerialNumber;
            job.Open(deviceSerialNumber);

            // Move card to smart card reader and suspend ZMotif SDK control of printer (using ZMotif SDK)
            int actionID = 0;
            job.JobControl.SmartCardConfiguration(SideEnum.Front, SmartCardTypeEnum.MIFARE, true);
            job.SmartCardDataOnly(1, out actionID);

            // Resume ZMotif SDK control of printer (using ZMotif SDK)
            job.JobResume();

            // Close ZMotif SDK control of job (using ZMotif SDK)
            job.Close();
        }

        private static void onCardRemoved(object sender, CardStatusEventArgs e)
        {
            Debug.WriteLine("remove: " + e.ReaderName);
        }

        private static void onCardInserted(object sender, CardStatusEventArgs e)
        {
            Debug.WriteLine("insert: " + e.ReaderName);
        }

        private static void onCardInitialized(object sender, CardStatusEventArgs e)
        {
            Debug.WriteLine("initialize: " + e.ReaderName);
        }

        private static void OnCompleted(PrintAndWriteSmartcardResult result)
        {
            Console.WriteLine(result.Success);
            Console.ReadKey();
        }

        private static void PrintAppointmentDetails()
        {
            Console.WriteLine("starting PrintAppointmentDetails...");

            ReceiptPrinterUtils receiptPrinterUtils = ReceiptPrinterUtils.Instance;
            var status = receiptPrinterUtils.GetDeviceStatus();

            //BarcodePrinterUtils barcodePrinterUtils = BarcodePrinterUtils.Instance;
            //barcodePrinterUtils.ResetPagePossition(ConfigurationManager.AppSettings["ReceiptPrinterName"]);
            AppointmentDetails appointmentDetails = new AppointmentDetails()
            {
                Name = "Ricardo Quaresma",
                NRICNo = "S1234567G",
                Date = new DateTime(2018, 1, 11, 15, 30, 00)
            };

            receiptPrinterUtils.PrintAppointmentDetails(appointmentDetails);

            Console.ReadKey();
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
                printerMonitor.OnPrintTTLabelSucceeded += OnPrintUserInfo_BarcodeSucceeded;
                printerMonitor.OnMonitorException += OnMonitorException;

                var test = DeviceManagement.GetUSBDevices();

                BarcodePrinterUtils barcodeScannerUtils = BarcodePrinterUtils.Instance;
                LabelInfo labelInfo = new LabelInfo()
                {
                    Name = "Avril Lavigne",
                    NRIC = "S1234567G",
                    Date = "01/01/1970"
                };

                string printerName = ConfigurationManager.AppSettings["TTLabelPrinterName"]?.ToUpper();
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
