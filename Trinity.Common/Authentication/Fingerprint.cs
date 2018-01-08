using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using Trinity.Common;
using Trinity.Common.Common;
using Trinity.Common.Monitor;

namespace Trinity.Common.Authentication
{
    public class Fingerprint
    {

        #region Singleton Implementation
        private static volatile Fingerprint _instance;

        private static object syncRoot = new Object();

        private Fingerprint()
        {
            
        }

        public static Fingerprint Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (_instance == null)
                            _instance = new Fingerprint();
                    }
                }
                return _instance;
            }
        }
        #endregion


        public event Action<bool> GetVerification;
        public event Action<bool> GetHealthMonitor;

        private List<byte[]> fingerprintArray;
        public void Start(List<byte[]> fingerprintArray)
        {
            this.fingerprintArray = fingerprintArray;
            Trinity.Common.FingerprintReaderUtils.Instance.GetDeviceStatus(GetHealthMonitorEvent);
        }
        private void GetHealthMonitorEvent(bool bSuccess, int nResult, bool bVerificationSuccess)
        {
            if (bSuccess)
            {
                Trinity.Common.FingerprintReaderUtils.Instance.StartIdentification(this.fingerprintArray, OnVerificationComplete);
            }
            if (GetHealthMonitor != null)
                GetHealthMonitor(bSuccess);
        }
        private void OnVerificationComplete(bool bVerificationSuccess)
        {
            if (GetVerification != null)
                GetVerification(bVerificationSuccess);
        }
    }
}
