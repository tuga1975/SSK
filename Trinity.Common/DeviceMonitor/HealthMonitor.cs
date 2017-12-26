using System;
using System.Collections.Generic;
using System.Timers;
using Trinity.BE;
using Trinity.Common.Common;
using Trinity.Common.Monitor;


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

        //public event EventHandler<ExceptionArgs> OnMonitorException;
        public EventHandler<HealthMonitorEventArgs> OnHealthCheck;

        public void CheckHealth()
        {
            RaiseHealthCheck(new HealthMonitorEventArgs(HealthStatus.Instance.GetHealthStatus()));


        }
        public virtual void RaiseHealthCheck(HealthMonitorEventArgs e)
        {
            OnHealthCheck?.Invoke(this, e);
        }


    }
    public class HealthMonitorEventArgs : EventArgs
    {
        public EnumDeviceStatuses[] SCardStatus { get; set; }
        public EnumDeviceStatuses[] FPrintStatus { get; set; }
        public EnumDeviceStatuses[] DocStatus { get; set; }
        public EnumDeviceStatuses[] PrintStatus { get; set; }
        public HealthMonitorEventArgs(HealthStatus healthStatus)
        {
            SCardStatus = healthStatus.SCardStatus;
            FPrintStatus = healthStatus.FPrintStatus;
            DocStatus = healthStatus.DocStatus;
            PrintStatus = healthStatus.PrintStatus;


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

        SCardMonitor _sCardMonitor;
        PrinterMonitor _printerMonitor;
        DocumentScannerMonitor _documentScannerMonitor;
        FingerprintMonitor _fingerprintMonitor;
        HealthStatus healthStatus;
        public EnumDeviceStatuses[] SCardStatus { get; set; }
        public EnumDeviceStatuses[] FPrintStatus { get; set; }
        public EnumDeviceStatuses[] DocStatus { get; set; }
        public EnumDeviceStatuses[] PrintStatus { get; set; }


        public HealthStatus GetHealthStatus()
        {
             healthStatus =  HealthStatus.Instance;
            _fingerprintMonitor = FingerprintMonitor.Instance;
            _fingerprintMonitor.OnGetDeviceStatusCompleted += _fingerprintMonitor_OnGetDeviceStatusCompleted; ;
            _fingerprintMonitor.StartCheckingDeviceStatus();
            _sCardMonitor = SCardMonitor.Instance;
            _printerMonitor = PrinterMonitor.Instance;
            _documentScannerMonitor = DocumentScannerMonitor.Instance;
            healthStatus.SCardStatus = _sCardMonitor.GetDeviceStatus();
            healthStatus.DocStatus = _documentScannerMonitor.GetDocumentScannerStatus();
            healthStatus.PrintStatus = _printerMonitor.GetBarcodePrinterStatus();
            return healthStatus;


        }

        private void _fingerprintMonitor_OnGetDeviceStatusCompleted(object sender, GetDeviceStatusCompletedArgs e)
        {
            if (e.IsConnected)
            {
                healthStatus.FPrintStatus = new EnumDeviceStatuses[]{EnumDeviceStatuses.Connected };
            }
            else
            {
                healthStatus.FPrintStatus = new EnumDeviceStatuses[] { EnumDeviceStatuses.Disconnected };
            }
        }
    }
}




