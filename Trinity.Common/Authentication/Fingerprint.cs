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
        public void Start(ICollection<byte[]> FingerprintArray)
        {
            
            
        }
        private void OnVerificationComplete(bool bVerificationSuccess)
        {
            if (GetVerification != null)
                GetVerification(bVerificationSuccess);
        }
    }
}
