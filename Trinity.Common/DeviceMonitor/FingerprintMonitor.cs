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

        protected virtual void RaiseMonitorExceptionEvent(ExceptionArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            OnMonitorException?.Invoke(this, e);
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
    }

    //public class FingerprintMonitor_BK
    //{
    //    public static bool Started = false;
    //    public static bool IdentificationStarted = false;
    //    public static bool VerificationStarted = false;
    //    static FingerprintReaderUtils _fingerprintReaderUtils;
    //    public static void Start()
    //    {
    //        if (Started)
    //        {
    //            Debug.WriteLine("FingerprintMonitor is already started.");
    //            return;
    //        }

    //        //_fingerprintReaderUtils = new FingerprintReaderUtils();
    //        Started = true;
    //    }

    //    public static void StartIdentification(OnGetBaseTemplateCompleteHandler OnGetBaseTemplateComplete)
    //    {
    //        if (!Started)
    //        {
    //            Debug.WriteLine("Start FingerprintMonitor first please...");
    //            return;
    //        }

    //        if (IdentificationStarted)
    //        {
    //            Debug.WriteLine("Fingerprint Identification is already started.");
    //            return;
    //        }

    //        //Start StartIdentification thread
    //        Thread thread = new Thread(new ThreadStart(() => _fingerprintReaderUtils.StartIdentification(OnGetBaseTemplateComplete)));
    //        thread.Start();
    //        IdentificationStarted = true;
    //    }

    //    public static void StartVerification(OnVerificationCompleteHandler onVerificationComplete, byte[] fingerprint_Template)
    //    {
    //        if (!Started)
    //        {
    //            Debug.WriteLine("Start FingerprintMonitor first please...");
    //            return;
    //        }

    //        //if (VerificationStarted)
    //        //{
    //        //    Debug.WriteLine("Fingerprint Identification is already started.");
    //        //    return;
    //        //}

    //        //Start StartIdentification thread
    //        Thread thread = new Thread(new ThreadStart(() => _fingerprintReaderUtils.StartVerification(onVerificationComplete, fingerprint_Template)));
    //        thread.Start();
    //        VerificationStarted = true;
    //    }

    //    public static bool IdentificationResult(bool bSuccess, int nRetCode, byte[] fingerprint_Template)
    //    {
    //        if (!Started)
    //        {
    //            Debug.WriteLine("Start FingerprintMonitor first please...");
    //            return false;
    //        }

    //        if (!IdentificationStarted)
    //        {
    //            Debug.WriteLine("Start Fingerprint Identification first please...");
    //            return false;
    //        }
    //        return _fingerprintReaderUtils.IdentificationResult(bSuccess, nRetCode, fingerprint_Template);
    //    }

    //    public static bool VerificationResult(bool bSuccess, int nRetCode, bool bVerificationSuccess)
    //    {
    //        if (!Started)
    //        {
    //            Debug.WriteLine("Start FingerprintMonitor first please...");
    //            return false;
    //        }

    //        if (!VerificationStarted)
    //        {
    //            Debug.WriteLine("Start Fingerprint Verification first please...");
    //            return false;
    //        }

    //        return _fingerprintReaderUtils.VerificationResult(bSuccess, nRetCode, bVerificationSuccess);
    //    }
    //}
}
