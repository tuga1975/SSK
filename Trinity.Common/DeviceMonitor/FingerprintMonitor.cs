using Futronic.SDKHelper;
using System;
using System.Diagnostics;
using System.Threading;
using Trinity.Common.Common;
using Trinity.Common.DeviceMonitor;
using Trinity.Common.Utils;

namespace Trinity.Common.Monitor
{
    public class FingerprintMonitor : AbstractDeviceMonitor
    {
        #region Singleton Implementation
        // The variable is declared to be volatile to ensure that assignment to the instance variable completes before the instance variable can be accessed
        private static volatile FingerprintMonitor _instance;

        // Uses a syncRoot instance to lock on, rather than locking on the type itself, to avoid deadlocks.
        private static object syncRoot = new Object();

        private FingerprintMonitor() { }

        public static FingerprintMonitor Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (_instance == null)
                            _instance = new FingerprintMonitor();
                    }
                }

                return _instance;
            }
        }
        #endregion

        public event EventHandler<ExceptionArgs> OnMonitorException;
        public event EventHandler<GetDeviceStatusCompletedArgs> OnGetDeviceStatusCompleted;


        protected virtual void RaiseMonitorExceptionEvent(ExceptionArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            OnMonitorException?.Invoke(this, e);
        }

        protected virtual void RaiseGetDeviceStatusCompletedEvent(GetDeviceStatusCompletedArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            OnGetDeviceStatusCompleted?.Invoke(this, e);
        }

        // Start monitor
        public override void Start()
        {
            // start a thread for health check
        }

        public void StartVerification(OnVerificationCompleteHandler onVerificationComplete, byte[] fingerprint_Template)
        {
            FingerprintReaderUtils fingerprintReaderUtils = FingerprintReaderUtils.Instance;
            FingerprintScannerStartResult fingerprintScannerStartResult = fingerprintReaderUtils.StartVerification(onVerificationComplete, fingerprint_Template);

            // on failed
            if (!fingerprintScannerStartResult.Success)
            {
                // raise OnMonitorException event
                RaiseMonitorExceptionEvent(new ExceptionArgs(fingerprintScannerStartResult.FailedInfo));
            }
            //Thread thread = new Thread(new ThreadStart(() => fingerprintReaderUtils.StartVerification(onVerificationComplete, fingerprint_Template)));
            //thread.Start();
        }

        public void StopVerification()
        {
            FingerprintReaderUtils fingerprintReaderUtils = FingerprintReaderUtils.Instance;
            fingerprintReaderUtils.StopVerification();
        }

        /// <summary>
        /// Start checking device status, raise OnGetDeviceStatusCompleted event when completed
        /// </summary>
        public void StartCheckingDeviceStatus()
        {
            FingerprintReaderUtils.Instance.GetDeviceStatus(OnVerificationComplete);
        }

        private void OnVerificationComplete(bool bSuccess, int nRetCode, bool bVerificationSuccess)
        {
            // bSuccess = true when fingerprinter is connected
            // raise RaiseOnGetDeviceStatusCompletedEvent

            System.Diagnostics.Debug.WriteLine("OnGetDeviceStatusCompleted result: " + bSuccess);
            RaiseGetDeviceStatusCompletedEvent(new GetDeviceStatusCompletedArgs() { IsConnected = bSuccess });
        }
    }
}
