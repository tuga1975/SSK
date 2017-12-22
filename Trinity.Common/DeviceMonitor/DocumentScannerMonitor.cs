using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity.Common.Common;
using Trinity.Common.Utils;

namespace Trinity.Common.DeviceMonitor
{
    public class DocumentScannerMonitor
    {
        #region Singleton Implementation
        // The variable is declared to be volatile to ensure that assignment to the instance variable completes before the instance variable can be accessed
        private static volatile DocumentScannerMonitor _instance;

        // Uses a syncRoot instance to lock on, rather than locking on the type itself, to avoid deadlocks.
        private static object syncRoot = new Object();

        private DocumentScannerMonitor() { }

        public static DocumentScannerMonitor Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (_instance == null)
                            _instance = new DocumentScannerMonitor();
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

        // Start monitoring
        public void Start()
        {
        }
        
        public object ScanDocument()
        {
            // Get DocumentScannerUtils Instance
            DocumentScannerUtils documentScannerUtils = DocumentScannerUtils.Instance;

            // Start scanner
            DocumentScannerResult documentScannerResult = documentScannerUtils.StartScanning();

            // Return value
            if (documentScannerResult.Success)
            {
                return documentScannerResult.Value;
            }
            else
            {
                // raise OnMonitorException
                if (documentScannerResult.FailedInfo != null)
                {
                    RaiseMonitorExceptionEvent(new ExceptionArgs()
                    {
                        ErrorCode = documentScannerResult.FailedInfo.ErrorCode,
                        ErrorMessage = documentScannerResult.FailedInfo.ErrorMessage
                    });
                }
                else
                {
                    RaiseMonitorExceptionEvent(new ExceptionArgs()
                    {
                        ErrorCode = 0,
                        ErrorMessage = "Error"
                    });
                }

                return null;
            }
        }

        public DeviceStatus GetDocumentScannerStatus()
        {
            return DocumentScannerUtils.Instance.GetDeviceStatus();
        }
    }
}
