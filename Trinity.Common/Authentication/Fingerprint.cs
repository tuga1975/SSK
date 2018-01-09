using System;
using System.Collections.Generic;
using System.Linq;
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

        public event Action<bool> OnIdentificationCompleted;
        public event Action OnDeviceDisconnected;

        private List<byte[]> _fingerprintTemplates;

        public void Start(List<byte[]> fingerprintTemplates)
        {
            _fingerprintTemplates = fingerprintTemplates;

            // get fingerprint reader status
            var fingerprintReaderStatus = FingerprintReaderUtils.Instance.GetDeviceStatus();

            // if status is disconnected, raise disconnected event
            if (!fingerprintReaderStatus.Contains(EnumDeviceStatuses.Connected))
            {
                RaiseDeviceDisconnectedEvent();
            }
            else
            {
                // start identification
                FingerprintReaderUtils.Instance.StartIdentification(_fingerprintTemplates, RaiseIdentificationCompletedEvent);
            }
        }

        private void RaiseIdentificationCompletedEvent(bool bVerificationSuccess)
        {
            OnIdentificationCompleted?.Invoke(bVerificationSuccess);
        }

        private void RaiseDeviceDisconnectedEvent()
        {
            OnDeviceDisconnected?.Invoke();
        }
    }
}
