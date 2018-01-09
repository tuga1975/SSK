using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Trinity.Common;
using Trinity.Common.Common;
using Trinity.Common.DeviceMonitor;
using Trinity.Common.Utils;

namespace DutyOfficer.CodeBehind
{
    public class PrintMUBAndTTLabels
    {
        public event EventHandler<PrintMUBAndTTLabelsSucceedEventArgs> OnPrintMUBAndTTLabelsSucceeded;
        public event EventHandler<PrintMUBAndTTLabelsEventArgs> OnPrintMUBAndTTLabelsFailed;
        public event EventHandler<ExceptionArgs> OnPrintMUBAndTTLabelsException;

        WebBrowser _web;

        public PrintMUBAndTTLabels(WebBrowser web)
        {
            _web = web;
        }

        internal void Start(LabelInfo labelInfo)
        {
            try
            {
                _web.SetLoading(false);
                this._web.LoadPageHtml("PrintingMUBAndTTLabels.html");
                this._web.RunScript("$('.status-text').css('color','#000').text('Please wait');");

                System.Threading.Thread.Sleep(1500);

                PrinterMonitor printerMonitor = PrinterMonitor.Instance;
                printerMonitor.OnPrintLabelSucceeded += OnPrintMUBAndTTLabelsSucceeded;//RaisePrintMUBAndTTLabelsSucceededEvent;
                printerMonitor.OnMonitorException += OnPrintMUBAndTTLabelsException;

                BarcodePrinterUtils barcodeScannerUtils = BarcodePrinterUtils.Instance;

                Session session = Session.Instance;
                if (session.IsAuthenticated)
                {
                    // Print BarCode and QR Code
                    foreach (var item in barcodeScannerUtils.GetDeviceStatus())
                    {
                        if (item == EnumDeviceStatuses.Connected)
                        {
                            printerMonitor.PrintLabel(labelInfo);
                        }
                        else
                        {
                            RaisePrintMUBAndTTLabelsFailedEvent(new PrintMUBAndTTLabelsEventArgs("Printer have problem: " + CommonUtil.GetDeviceStatusText(item)));

                            Console.WriteLine("Barcode printer is not connected.");
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                RaisePrintMUBAndTTLabelsExceptionEvent(new ExceptionArgs(new FailedInfo()
                {
                    ErrorCode = (int)EnumErrorCodes.UnknownError,
                    ErrorMessage = new ErrorInfo().GetErrorMessage(EnumErrorCodes.UnknownError)
                }));
            }
        }


        // Wrap event invocations inside a protected virtual method
        // to allow derived classes to override the event invocation behavior
        protected virtual void RaisePrintMUBAndTTLabelsSucceededEvent(PrintMUBAndTTLabelsSucceedEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<PrintMUBAndTTLabelsSucceedEventArgs> handler = OnPrintMUBAndTTLabelsSucceeded;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Use the () operator to raise the event.
                handler(this, e);
            }
        }


        protected virtual void RaisePrintMUBAndTTLabelsFailedEvent(PrintMUBAndTTLabelsEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<PrintMUBAndTTLabelsEventArgs> handler = OnPrintMUBAndTTLabelsFailed;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Use the () operator to raise the event.
                handler(this, e);
            }
        }

        protected virtual void RaisePrintMUBAndTTLabelsExceptionEvent(ExceptionArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            OnPrintMUBAndTTLabelsException?.Invoke(this, e);
        }
    }

    #region Custom Events
    public class PrintMUBAndTTLabelsEventArgs : EventArgs
    {
        private string _message;
        public PrintMUBAndTTLabelsEventArgs(string message)
        {
            _message = message;
        }
        public string Message
        {
            get
            {
                return _message;
            }
            set
            {
                _message = value;
            }
        }
    }

    #endregion
}
