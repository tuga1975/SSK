using Futronic.SDKHelper;
using SSK.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SSK.DeviceMonitor
{
    public class FingerprintMonitor
    {
        public static bool Started = false;
        public static bool IdentificationStarted = false;
        public static bool VerificationStarted = false;
        static FingerprintReaderUtils _fingerprintReaderUtils;
        public static void Start()
        {
            if (Started)
            {
                Debug.WriteLine("FingerprintMonitor is already started.");
                return;
            }

            _fingerprintReaderUtils = new FingerprintReaderUtils();
            Started = true;
        }

        public static void StartIdentification(OnGetBaseTemplateCompleteHandler OnGetBaseTemplateComplete)
        {
            if (!Started)
            {
                Debug.WriteLine("Start FingerprintMonitor first please...");
                return;
            }

            if (IdentificationStarted)
            {
                Debug.WriteLine("Fingerprint Identification is already started.");
                return;
            }

            //Start StartIdentification thread
            Thread thread = new Thread(new ThreadStart(() => _fingerprintReaderUtils.StartIdentification(OnGetBaseTemplateComplete)));
            thread.Start();
            IdentificationStarted = true;
        }

        public static void StartVerification(OnVerificationCompleteHandler onVerificationComplete, byte[] fingerprint_Template)
        {
            if (!Started)
            {
                Debug.WriteLine("Start FingerprintMonitor first please...");
                return;
            }

            //if (VerificationStarted)
            //{
            //    Debug.WriteLine("Fingerprint Identification is already started.");
            //    return;
            //}
            //Start StartIdentification thread

            Thread thread = new Thread(new ThreadStart(() => _fingerprintReaderUtils.StartVerification(onVerificationComplete, fingerprint_Template)));
            thread.Start();
            VerificationStarted = true;
        }

        public static bool IdentificationResult(bool bSuccess, int nRetCode, byte[] fingerprint_Template)
        {
            if (!Started)
            {
                Debug.WriteLine("Start FingerprintMonitor first please...");
                return false;
            }

            if (!IdentificationStarted)
            {
                Debug.WriteLine("Start Fingerprint Identification first please...");
                return false;
            }
            return _fingerprintReaderUtils.IdentificationResult(bSuccess, nRetCode, fingerprint_Template);
        }

        public static bool VerificationResult(bool bSuccess, int nRetCode, bool bVerificationSuccess)
        {
            if (!Started)
            {
                Debug.WriteLine("Start FingerprintMonitor first please...");
                return false;
            }

            if (!VerificationStarted)
            {
                Debug.WriteLine("Start Fingerprint Verification first please...");
                return false;
            }

            return _fingerprintReaderUtils.VerificationResult(bSuccess, nRetCode, bVerificationSuccess);
        }

        public static void StopVerification()
        {
            if (!Started)
            {
                Debug.WriteLine("Start FingerprintMonitor first please...");
                return;
            }

            if (!VerificationStarted)
            {
                Debug.WriteLine("Start Fingerprint Verification first please...");
                return;
            }

            _fingerprintReaderUtils.StopVerification();
        }
    }
}
