using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Trinity.Common.Common;
using Trinity.Common.DeviceMonitor;
using Trinity.DAL.DBContext;
using static Trinity.Common.Monitor.FingerprintMonitor;

namespace Trinity.Common.Monitor
{
    public class HealthMonitor
    {
        #region Singleton Implementation
        // The variable is declared to be volatile to ensure that assignment to the instance variable completes before the instance variable can be accessed
        private static volatile HealthMonitor _instance;

        // Uses a syncRoot instance to lock on, rather than locking on the type itself, to avoid deadlocks.
        private static object syncRoot = new Object();

        private HealthMonitor() { }

        public static HealthMonitor Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (_instance == null)
                            _instance = new HealthMonitor();
                    }
                }

                return _instance;
            }
        }
        #endregion

        SCardMonitor _sCardMonitor;
        PrinterMonitor _printerMonitor;
        DocumentScannerMonitor _documentScannerMonitor;
        FingerprintMonitor _fingerprintMonitor;
        HealthStatus _healthStatus;

        private HealthStatus GetHealthStatus()
        {
            _healthStatus = HealthStatus.Instance;
            _fingerprintMonitor = FingerprintMonitor.Instance;
            _fingerprintMonitor.OnGetDeviceStatusCompleted += FingerprintMonitor_OnGetDeviceStatusCompleted;
            _fingerprintMonitor.StartCheckingDeviceStatus();
            _sCardMonitor = SCardMonitor.Instance;
            _printerMonitor = PrinterMonitor.Instance;

            _documentScannerMonitor = DocumentScannerMonitor.Instance;
            _healthStatus.SCardStatus = _sCardMonitor.GetDeviceStatus();
            _healthStatus.DocStatus = _documentScannerMonitor.GetDocumentScannerStatus();
            _healthStatus.PrintStatus = _printerMonitor.GetBarcodePrinterStatus();
            return _healthStatus;


        }
        private void FingerprintMonitor_OnGetDeviceStatusCompleted(object sender, GetDeviceStatusCompletedArgs e)
        {
            _healthStatus.FPrintStatus = e.IsConnected;
        }

        public void Start()
        {
            //test purpose
            //Timer timer = new Timer(10000);

            Timer timer = new Timer(1000 * 60 * 15);
            timer.Elapsed += MonitorHandler;
            timer.Start();


        }


        private void MonitorHandler(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine("monitoring...\n");
            var health = Task.Run(() => GetHealthStatus());

            //add to db
            var dalDeviceStatus = new DAL.DAL_DeviceStatus();

            var appName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            var deviceID = 0;
            var statusMessage = "";
            var statusCode = 0;

            //Receipt Print Status
            AddReceiptPrintStatus(health, dalDeviceStatus, appName, out deviceID, out statusMessage, out statusCode);

            //Smart Cart Reader Status

            AddSmartCardReaderStatus(health, dalDeviceStatus, appName, out deviceID, out statusMessage);



            //Document Scanner Status
            AddDocumentScannerStatus(health, dalDeviceStatus, appName, out deviceID, out statusMessage);


            //Fingerprint Scanner Status
            AddFingerprintScannerStatus(health, dalDeviceStatus, appName, out deviceID, out statusMessage, out statusCode);



        }

        private static void AddFingerprintScannerStatus(Task<HealthStatus> health, DAL.DAL_DeviceStatus dalDeviceStatus, string appName, out int deviceID, out string statusMessage, out int statusCode)
        {
            var fingerPrintStatusCode = health.Result.FPrintStatus;
            if (fingerPrintStatusCode)
            {
                statusMessage = DeviceStatus.Connected.ToString();
                statusCode = (int)DeviceStatus.Connected;
            }
            else
            {
                statusMessage = DeviceStatus.Disconnected.ToString();
                statusCode = (int)DeviceStatus.Disconnected;
            }
            deviceID = dalDeviceStatus.GetDeviceId(EnumDeviceType.FingerprintScanner);

            Trinity.BE.DeviceStatus fingerScanDevice = dalDeviceStatus.SetInfo(appName, deviceID, statusMessage, statusCode);
            dalDeviceStatus.Insert(fingerScanDevice);
        }

        private static void AddDocumentScannerStatus(Task<HealthStatus> health, DAL.DAL_DeviceStatus dalDeviceStatus, string appName, out int deviceID, out string statusMessage)
        {
            var documnetScannerStatusCode = (int)health.Result.DocStatus;
            deviceID = dalDeviceStatus.GetDeviceId(EnumDeviceType.DocumentScanner);
            statusMessage = health.Result.DocStatus.ToString();

            Trinity.BE.DeviceStatus docScanDevice = dalDeviceStatus.SetInfo(appName, deviceID, statusMessage, documnetScannerStatusCode);
            dalDeviceStatus.Insert(docScanDevice);
        }

        private static void AddSmartCardReaderStatus(Task<HealthStatus> health, DAL.DAL_DeviceStatus dalDeviceStatus, string appName, out int deviceID, out string statusMessage)
        {
            var sCardStatusCode = (int)health.Result.SCardStatus;
            deviceID = dalDeviceStatus.GetDeviceId(EnumDeviceType.SmartCardReader);
            statusMessage = health.Result.SCardStatus.ToString();

            Trinity.BE.DeviceStatus sCardDevice = dalDeviceStatus.SetInfo(appName, deviceID, statusMessage, sCardStatusCode);
            dalDeviceStatus.Insert(sCardDevice);
        }

        private static void AddReceiptPrintStatus(Task<HealthStatus> health, DAL.DAL_DeviceStatus dalDeviceStatus, string appName, out int deviceID, out string statusMessage, out int statusCode)
        {


            var printStatusCode = health.Result.PrintStatus;
            deviceID = dalDeviceStatus.GetDeviceId(EnumDeviceType.ReceiptPrinter);


            if (printStatusCode.Connected)
            {
                statusMessage = nameof(health.Result.PrintStatus.Connected);
                statusCode = 1;
            }
            else if (printStatusCode.Busy)
            {
                statusMessage = nameof(health.Result.PrintStatus.Busy);
                statusCode = 2;
            }
            else
            {
                statusMessage = "Disconnected";
                statusCode = 0;
            }


            Trinity.BE.DeviceStatus printStatus = dalDeviceStatus.SetInfo(appName, deviceID, statusMessage, statusCode);
            dalDeviceStatus.Insert(printStatus);
        }
    }


    public class HealthStatus
    {
        #region Singleton Implementation
        // The variable is declared to be volatile to ensure that assignment to the instance variable completes before the instance variable can be accessed
        private static volatile HealthStatus _instance;

        // Uses a syncRoot instance to lock on, rather than locking on the type itself, to avoid deadlocks.
        private static object syncRoot = new Object();

        private HealthStatus() { }

        public static HealthStatus Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (_instance == null)
                            _instance = new HealthStatus();
                    }
                }

                return _instance;
            }
        }
        #endregion


        public DeviceStatus SCardStatus { get; set; }
        public bool FPrintStatus { get; set; }
        public DeviceStatus DocStatus { get; set; }
        public PrinterStatus PrintStatus { get; set; }


    }

}
