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

        public event Action OnPrintLabelSucceeded;
        public event EventHandler<ExceptionArgs> OnMonitorException;

        protected virtual void RaisePrintLabelSucceededEvent()
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            OnPrintLabelSucceeded?.Invoke();
        }

        protected virtual void RaiseMonitorExceptionEvent(ExceptionArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            OnMonitorException?.Invoke(this, e);
        }

        public void PrintLabel(UserInfo userInfo)
        {
            // validation
            if (string.IsNullOrEmpty(userInfo.UserName))
            {
                // username is null
                RaiseMonitorExceptionEvent(new ExceptionArgs(new FailedInfo()
                {
                    ErrorCode = (int)EnumErrorCodes.UserNameNull,
                    ErrorMessage = new ErrorInfo().GetErrorMessage(EnumErrorCodes.UserNameNull)
                }));

                return;
            }
            else if (string.IsNullOrEmpty(userInfo.NRIC))
            {
                // NRIC is null
                RaiseMonitorExceptionEvent(new ExceptionArgs(new FailedInfo()
                {
                    ErrorCode = (int)EnumErrorCodes.NRICNull,
                    ErrorMessage = new ErrorInfo().GetErrorMessage(EnumErrorCodes.NRICNull)
                }));

                return;
            }

            // print label
            BarcodePrinterUtils barcodeScannerUtils = BarcodePrinterUtils.Instance;
            if (barcodeScannerUtils.PrintUserInfo(userInfo))
            {
                // Print TT Label succeeded, then continue printing MUB Label
                if (barcodeScannerUtils.PrintQRCodeUserInfo(userInfo))
                {
                    // raise succeeded event
                    RaisePrintLabelSucceededEvent();
                }
                else
                {
                    // raise failed event
                    RaiseMonitorExceptionEvent(new ExceptionArgs(new FailedInfo()
                    {
                        ErrorCode = (int)EnumErrorCodes.UnknownError,
                        ErrorMessage = new ErrorInfo().GetErrorMessage(EnumErrorCodes.UnknownError)
                    }));
                }
            }
            else
            {
                // raise failed event
                RaiseMonitorExceptionEvent(new ExceptionArgs(new FailedInfo()
                {
                    ErrorCode = (int)EnumErrorCodes.UnknownError,
                    ErrorMessage = new ErrorInfo().GetErrorMessage(EnumErrorCodes.UnknownError)
                }));
            }
        }

        public EnumDeviceStatuses[] GetBarcodePrinterStatus()
        {
            return BarcodePrinterUtils.Instance.GetDeviceStatus();
        }
    }
}
