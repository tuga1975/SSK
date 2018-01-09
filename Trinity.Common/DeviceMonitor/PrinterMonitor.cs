using System;
using System.Collections.Generic;
using System.Configuration;
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

        public event EventHandler<PrintMUBAndTTLabelsSucceedEventArgs> OnPrintLabelSucceeded;
        public event EventHandler<ExceptionArgs> OnMonitorException;
        public event Action OnPrintBarcodeSucceeded;

        protected virtual void RaisePrintLabelSucceededEvent(PrintMUBAndTTLabelsSucceedEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            OnPrintLabelSucceeded?.Invoke(this, e);
        }

        protected virtual void RaiseMonitorExceptionEvent(ExceptionArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            OnMonitorException?.Invoke(this, e);
        }

        protected virtual void RaisePrintBarcodeSucceededEvent()
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            Action handler = OnPrintBarcodeSucceeded;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Use the () operator to raise the event.
                handler();
            }
        }

        public void PrintBarcodeLabel(LabelInfo labelInfo)
        {
            // validation
            if (string.IsNullOrEmpty(labelInfo.Name))
            {
                // username is null
                RaiseMonitorExceptionEvent(new ExceptionArgs(new FailedInfo()
                {
                    ErrorCode = (int)EnumErrorCodes.UserNameNull,
                    ErrorMessage = new ErrorInfo().GetErrorMessage(EnumErrorCodes.UserNameNull)
                }));

                return;
            }
            else if (string.IsNullOrEmpty(labelInfo.NRIC))
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
            BarcodePrinterUtils printerUtils = BarcodePrinterUtils.Instance;
            if (printerUtils.PrintBarcodeUserInfo(labelInfo))
            {
                // raise succeeded event
                RaisePrintBarcodeSucceededEvent();
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

        public void PrintMUBLabel(LabelInfo labelInfo)
        {
            // validation
            if (string.IsNullOrEmpty(labelInfo.Name))
            {
                // username is null
                RaiseMonitorExceptionEvent(new ExceptionArgs(new FailedInfo()
                {
                    ErrorCode = (int)EnumErrorCodes.UserNameNull,
                    ErrorMessage = new ErrorInfo().GetErrorMessage(EnumErrorCodes.UserNameNull)
                }));

                return;
            }
            else if (string.IsNullOrEmpty(labelInfo.NRIC))
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
            BarcodePrinterUtils printerUtils = BarcodePrinterUtils.Instance;

            // Print TT Label succeeded, then continue printing MUB Label
            if (printerUtils.PrintQRCodeUserInfo(labelInfo))
            {
                // raise succeeded event
                RaisePrintLabelSucceededEvent(new PrintMUBAndTTLabelsSucceedEventArgs(labelInfo));
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
            // get barcodePrinter name from appconfig
            string barcodePrinterName = ConfigurationManager.AppSettings["BarcodePrinterName"].ToUpper();
            return BarcodePrinterUtils.Instance.GetDeviceStatus(barcodePrinterName);
        }
    }
}
