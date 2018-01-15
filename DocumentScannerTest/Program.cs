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

            // read/write smart card
            // write data
            PrintAndWriteSmartcardInfo_Demo data = new PrintAndWriteSmartcardInfo_Demo()
            {
                Name = "Ricardo Quaresma xxx",
                PrintedDate = DateTime.Now
            };

            //SmartCardReaderUtils smartCardReaderUtils = SmartCardReaderUtils.Instance;
            //bool writeSuccessful = smartCardReaderUtils.WriteData(data);

            //string getData = smartCardReaderUtils.ReadAllData_MifareClassic();
            //Console.WriteLine(getData);

            //ReleaseCard();
            TestSmartCardPrinter();
            Console.ReadKey();
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

        private static void TestEncodeContactlessSCard(PrintAndWriteSmartcardInfo_Demo data)
        {
            try
            {
                //  The goal of this program is to establish a connection with 
                // a Mifare 4k contactless microprocessor smart card through a ZXP printer.

                Job job = new Job();

                // Begin SDK communication with printer (using ZMotif SDK)
                //string deviceSerialNumber = "06C104500004";
                string deviceSerialNumber = EnumDeviceNames.SmartCardPrinterSerialNumber;
                job.Open(deviceSerialNumber);

                // Move card to smart card reader and suspend ZMotif SDK control of printer (using ZMotif SDK)
                int actionID = 0;
                job.JobControl.SmartCardConfiguration(SideEnum.Front, SmartCardTypeEnum.MIFARE, true);
                job.SmartCardDataOnly(1, out actionID);

                // Wait while card moves into encode position 
                Thread.Sleep(4000);

                string cardUID = SmartCardPrinterUtils.Instance.GetMifareCardUID("");

                //string cardUID = GetCardUID();

                //// Establish connection with encoder (using WinSCard.dll)
                //int encoderContext = 0;
                //SCardEstablishContext(0, 0, 0, ref encoderContext);

                //// At this point, call SCardListReaders to get available readers (code not included).
                //// Alternatively, refer to 'device manager >> smart card encoders' when printer is on.
                //string encoderName = "SCM Microsystems Inc. SDI010 Contactless Reader 0";

                //// Establish connection with card (using WinSCard.dll)
                //int cardContext = 0;
                //uint activeProtocol = 0;
                //SCardConnect(encoderContext, encoderName, 0x02, 0x02 | 0x01, ref cardContext, ref activeProtocol);

                //// Prepare to communicate with card.  sIO is a simple struct that contains the two elements (protocol and pciLength).
                //SCARD_IO_REQUEST sIO = new SCARD_IO_REQUEST();
                //sIO.dwProtocol = activeProtocol;
                //sIO.cbPciLength = 8;

                //// Choose which block to read/write
                //byte block = 0x01;

                //// Load key '0xFFFFFFFFFFFF' (A common default Mifare 4k key) into reader as Key A (using WinSCard.dll)
                //Console.WriteLine("Loading key into reader.");
                //byte[] key = { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
                //int response0Size = 2;
                //byte[] response0 = new byte[response0Size];
                //byte[] loadKeyCommand = { 0xFF, 0x82, 0x00, 0x60, (byte)key.Length, key[0], key[1], key[2], key[3], key[4], key[5] };
                //SCardTransmit(cardContext, ref sIO, ref loadKeyCommand[0], loadKeyCommand.Length, ref sIO, ref response0[0], ref response0Size); 0


                //// Authenticate connection by recalling key A (using WinSCard.dll)
                //int response1Size = 2;
                //byte[] response1 = new byte[response1Size];
                //byte[] authenticateCmd = { 0xFF, 0x86, 0x00, 0x00, 0x05, 0x01, 0x00, block, 0x60, 0x01 };
                //SCardTransmit(cardContext, ref sIO, ref authenticateCmd[0], authenticateCmd.Length, ref sIO, ref response1[0], ref response1Size);

                //// Mifare 4k Command to write 'HelloHelloHelloH' to card (using WinSCard.dll)
                //int response2Size = 8;
                //byte[] response2 = new byte[response2Size];
                //byte[] writeCmd = { 0xFF, 0xD6, 0x00, block, 0x10, 0x48, 0x65, 0x6C, 0x6C, 0x6F, 0x48, 0x65, 0x6C, 0x6C, 0x6F, 0x48, 0x65, 0x6C, 0x6C, 0x6F, 0x48 };
                //SCardTransmit(cardContext, ref sIO, ref writeCmd[0], writeCmd.Length, ref sIO, ref response2[0], ref response2Size);

                //// Mifare 1k or 4k command to read card (using WinSCard.dll)
                //int response3Size = 128;
                //byte[] response3 = new byte[response3Size];
                //byte[] readCmd = { 0xFF, 0xB0, 0x00, block, 0x00 };
                //SCardTransmit(cardContext, ref sIO, ref readCmd[0], readCmd.Length, ref sIO, ref response3[0], ref response3Size);

                //// Display the information read from the card; wait for user 
                //Console.WriteLine(ASCIIEncoding.ASCII.GetString(response3, 0, response3Size));
                //Console.ReadKey();

                //// Close connection with card (using WinSCard.dll)
                //SCardDisconnect(cardContext, 0x02);

                //// Release connection to encoder (using WinSCard.dll)
                //SCardReleaseContext(encoderContext);

                // Resume ZMotif SDK control of printer (using ZMotif SDK)
                job.JobResume();

                // Close ZMotif SDK control of job (using ZMotif SDK)
                job.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static void TestSmartCardPrinter()
        {
            Console.WriteLine("starting TestSmartCardPrinter...");
            PrintAndWriteSmartcardInfo printAndWriteSmartcardInfo = new PrintAndWriteSmartcardInfo()
            {
                SmartCardData = new SmartCardData()
                {
                    CardHolderInfo = new CardHolderInfo()
                    {
                        Name = "TuDD test smart card printer"
                    }
                }
            };

            SmartCardPrinterUtils smartCardPrinterUtils = SmartCardPrinterUtils.Instance;

            smartCardPrinterUtils.PrintAndWriteSmartcardData(printAndWriteSmartcardInfo, OnCompleted);
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
