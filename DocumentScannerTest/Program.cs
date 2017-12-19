﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Trinity.Common.Common;
using Trinity.Common.DeviceMonitor;
using Trinity.Common.Utils;

namespace DocumentScannerTest
{
    class Program
    {
        static void Main(string[] args)
        {
            // for testing only

            // DocumentScanner
            // StartDocumentScannerMonitor();

            // TSC Bar code scanner
            StartBarcodeScanner();
        }

        private static void StartBarcodeScanner()
        {
            try
            {
                Console.WriteLine("StartBarcodeScanner is starting...");
                string userName = "Avril Lavigne";
                string nric = "S1234567G";  
                string dob = "01/01/1990";  
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
                printerMonitor.OnPrintUserInfo_BarcodeSucceeded += OnPrintUserInfo_BarcodeSucceeded;
                printerMonitor.OnMonitorException += OnMonitorException;

                BarcodeScannerUtils barcodeScannerUtils = BarcodeScannerUtils.Instance; 
                if (barcodeScannerUtils.GetDeviceStatus() == DeviceStatus.Connected)
                {
                    printerMonitor.PrintUserInfo_Barcode(userName, nric, dob);
                }
                else
                {
                    Console.WriteLine("printer is not connected.");
                }

                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadKey();
            }
        }

        private static void OnPrintUserInfo_BarcodeSucceeded()
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