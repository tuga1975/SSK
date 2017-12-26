using System;
using System.Collections.Generic;
using System.Timers;
using Trinity.Common.Common;
using Trinity.Common.Monitor;
using Trinity.DAL.DBContext;

namespace Trinity.Common.DeviceMonitor
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
            Timer timer = new Timer(30000);

            //Timer timer = new Timer(1000 * 60 * 15);
            timer.Elapsed += MonitorHandler;
            timer.Start();


        }


        private void MonitorHandler(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine("monitoring...\n");
            var health = GetHealthStatus();

            //add to db
            var dalDeviceStatus = new DAL.DAL_DeviceStatus();

            var listDeviceStatus = new List<ApplicationDevice_Status>();

            //Receipt Print Status
            var status = GetPrintDeviceStatus(health);
            listDeviceStatus.Add(SetDeviceStatus(dalDeviceStatus.GetDeviceId(EnumDeviceType.ReceiptPrinter),status.Key, status.Value));
            //Smart Cart Reader Status
             status = GetSmartCardDeviceStatus(health, health.SCardStatus);
            listDeviceStatus.Add(SetDeviceStatus(dalDeviceStatus.GetDeviceId(EnumDeviceType.SmartCardReader), status.Key, status.Value));

            //Document Scanner Status
             status = GetDocumnetDeviceStatus(health, health.DocStatus);
            listDeviceStatus.Add(SetDeviceStatus(dalDeviceStatus.GetDeviceId(EnumDeviceType.DocumentScanner), status.Key, status.Value));

            //Fingerprint Scanner Status
             status = GetFingerPrintDeviceStatus(health);
            listDeviceStatus.Add(SetDeviceStatus(dalDeviceStatus.GetDeviceId(EnumDeviceType.FingerprintScanner), status.Key, status.Value));

            dalDeviceStatus.Insert(listDeviceStatus);

        }


        private ApplicationDevice_Status SetDeviceStatus(int? deviceId,int statusCode,string statusMessage)
        {
            var deviceStatus = new ApplicationDevice_Status();
            deviceStatus.ApplicationType = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
            deviceStatus.DeviceID = deviceId;
            deviceStatus.StatusMessage = statusMessage;
            deviceStatus.StatusCode = statusCode;
            deviceStatus.ID = Guid.NewGuid();
            return deviceStatus;
        }

        private KeyValuePair<int,string> GetPrintDeviceStatus(HealthStatus health)
        {
            var statusCode = health.PrintStatus.Connected;
            return SetCodeAndMessage(health, statusCode);
        }
        private KeyValuePair<int, string> GetFingerPrintDeviceStatus(HealthStatus health)
        {
            var statusCode = health.FPrintStatus;
            
            return SetCodeAndMessage(health, statusCode);
           
        }
        private KeyValuePair<int, string> GetDocumnetDeviceStatus(HealthStatus health,DeviceStatus deviceStatus)
        {
            var statusCode = health.DocStatus;
          
            return SetCodeAndMessage(health, deviceStatus);
            
        }
        private KeyValuePair<int, string> GetSmartCardDeviceStatus(HealthStatus health, DeviceStatus deviceStatus)
        {
            var statusCode = health.SCardStatus;
           
            return SetCodeAndMessage(health, deviceStatus);
            
        }



        private static KeyValuePair<int, string> SetCodeAndMessage(HealthStatus health, bool statusCode)
        {
            KeyValuePair<int, string> returnValue;
            if (statusCode)
            {
                returnValue = new KeyValuePair<int, string>(1, "Connected");

            }
            else
            {
                returnValue = new KeyValuePair<int, string>(0, "Disconnected");

            }

            return returnValue;
        }
        private static KeyValuePair<int, string> SetCodeAndMessage(HealthStatus health,DeviceStatus deviceStatus)
        {
            
            if (deviceStatus.HasFlag(DeviceStatus.Connected))
            {
                return new KeyValuePair<int, string>(1, "Connected");

            }
            else if (deviceStatus.HasFlag(DeviceStatus.Busy))
            {
                return new KeyValuePair<int, string>(2, "Busy");
            }
            else
            {
                return new KeyValuePair<int, string>(0, "Disconnected");

            }
            
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
