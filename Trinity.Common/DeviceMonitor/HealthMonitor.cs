using System;
using System.Collections.Generic;
using System.Timers;
using Trinity.BE;
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

        public event EventHandler<ExceptionArgs> OnMonitorException;
        public event EventHandler<GetDeviceStatusCompletedArgs> OnGetDeviceStatusCompleted;

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
            if (e.IsConnected)
            {
                _healthStatus.FPrintStatus = new EnumDeviceStatuses[] { EnumDeviceStatuses.Connected };
            }
            else
            {
                _healthStatus.FPrintStatus = new EnumDeviceStatuses[] { EnumDeviceStatuses.Disconnected };
            }
            
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
            var dalDeviceStatus = new  DAL.DAL_DeviceStatus();

            var listDeviceStatus = new List<DeviceStatus>();

            //Receipt Print Status
            var status = health.PrintStatus;
            listDeviceStatus.Add(SetDeviceStatus(dalDeviceStatus.GetDeviceId(EnumDeviceTypes.ReceiptPrinter), status));
           
            //Smart Cart Reader Status
             status =  health.SCardStatus;
            listDeviceStatus.Add(SetDeviceStatus(dalDeviceStatus.GetDeviceId(EnumDeviceTypes.SmartCardReader), status));

            //Document Scanner Status
            status = health.DocStatus;
            listDeviceStatus.Add(SetDeviceStatus(dalDeviceStatus.GetDeviceId(EnumDeviceTypes.DocumentScanner), status));

            //Fingerprint Scanner Status
            status = health.FPrintStatus;
            listDeviceStatus.Add(SetDeviceStatus(dalDeviceStatus.GetDeviceId(EnumDeviceTypes.FingerprintScanner), status));

            dalDeviceStatus.Insert(listDeviceStatus);

        }


        private BE.DeviceStatus SetDeviceStatus(int? deviceId,EnumDeviceStatuses[] statusCode)
        {
            var deviceStatus = new BE.DeviceStatus();
            deviceStatus.ApplicationType = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
            deviceStatus.DeviceID = deviceId;
            deviceStatus.StatusCode = statusCode;
            return deviceStatus;
        }

            
        }
    }

public class HealthMonitorEventArgs : EventArgs
{
    public EnumDeviceStatuses[] SCardStatus { get; set; }
    public EnumDeviceStatuses[] FPrintStatus { get; set; }
    public EnumDeviceStatuses[] DocStatus { get; set; }
    public EnumDeviceStatuses[] PrintStatus { get; set; }
    public HealthMonitorEventArgs()
    {
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


        public EnumDeviceStatuses[] SCardStatus { get; set; }
        public EnumDeviceStatuses[] FPrintStatus { get; set; }
        public EnumDeviceStatuses[] DocStatus { get; set; }
        public EnumDeviceStatuses[] PrintStatus { get; set; }


    }


