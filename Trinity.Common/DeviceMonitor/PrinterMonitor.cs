using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity.Common.Common;
using Trinity.Common.Utils;

namespace Trinity.Common.DeviceMonitor
{
    public class PrinterMonitor
    {
        #region Singleton Implementation
        // The variable is declared to be volatile to ensure that assignment to the instance variable completes before the instance variable can be accessed
        private static volatile PrinterMonitor _instance;

        // Uses a syncRoot instance to lock on, rather than locking on the type itself, to avoid deadlocks.
        private static object syncRoot = new Object();

        private PrinterMonitor() { }

        public static PrinterMonitor Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (_instance == null)
                            _instance = new PrinterMonitor();
                    }
                }

                return _instance;
            }
        }
        #endregion

        public event Action OnPrintUserInfo_BarcodeSucceeded;
        public event EventHandler<ExceptionArgs> OnMonitorException;

        protected virtual void RaisePrintUserInfo_BarcodeSucceededEvent()
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            OnPrintUserInfo_BarcodeSucceeded?.Invoke();
        }

        protected virtual void RaiseMonitorExceptionEvent(ExceptionArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            OnMonitorException?.Invoke(this, e);
        }

        public void PrintUserInfo_Barcode(string userName, string nric, string dob)
        {
            BarcodeScannerUtils barcodeScannerUtils = BarcodeScannerUtils.Instance;
            if (barcodeScannerUtils.PrintUserInfo(userName, nric, dob))
            {
                // raise succeeded event
                RaisePrintUserInfo_BarcodeSucceededEvent();
            }
            else
            {
                // raise failed event
                if (string.IsNullOrEmpty(userName))
                {
                    // username is null
                    RaiseMonitorExceptionEvent(new ExceptionArgs(new FailedInfo()
                    {
                        ErrorCode = (int)ErrorCodes.UserNameNull,
                        ErrorMessage = ErrorMessages.UserNameNull
                    }));
                }
                else
                {
                    // fatal error
                    RaiseMonitorExceptionEvent(new ExceptionArgs(new FailedInfo()
                    {
                        ErrorCode = (int)ErrorCodes.UnknownError,
                        ErrorMessage = ErrorMessages.UnknownError
                    }));
                }
            }
        }
    }
}
