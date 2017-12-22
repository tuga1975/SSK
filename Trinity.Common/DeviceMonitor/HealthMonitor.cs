using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
            while (true)
            {

                // health.HealthStatus.FPrintStatus = _healthStatus.FPrintStatus;
                var health = GetHealthStatus();
                //add to db

                //sleep for  15 Minutes
                Thread.Sleep(1000 * 60 * 15);

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
