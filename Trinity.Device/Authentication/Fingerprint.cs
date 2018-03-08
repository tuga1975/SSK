using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Trinity.Common;
using Trinity.Device.Util;

namespace Trinity.Device.Authentication
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

            // Get fingerprint reader status
            EnumDeviceStatus[] fingerprintReaderStatuses = FingerprintReaderUtil.Instance.GetDeviceStatus();
            // if status is disconnected, raise disconnected event
            if (!fingerprintReaderStatuses.Contains(EnumDeviceStatus.Connected))
            {
                RaiseDeviceDisconnectedEvent();
            }
            else
            {
                // start identification
                FingerprintReaderUtil.Instance.StartIdentification(_fingerprintTemplates, RaiseIdentificationCompletedEvent);
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
