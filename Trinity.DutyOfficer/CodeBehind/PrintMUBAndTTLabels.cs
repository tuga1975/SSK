using System;
using System.Configuration;
using System.Linq;
using System.Windows.Forms;
using Trinity.Common;
using Trinity.Util;

namespace DutyOfficer.CodeBehind
{
    public class PrintMUBAndTTLabels
    {
        public event EventHandler<PrintMUBAndTTLabelsSucceedEventArgs> OnPrintMUBLabelsSucceeded;
        public event EventHandler<PrintMUBAndTTLabelsSucceedEventArgs> OnPrintTTLabelsSucceeded;
        public event EventHandler<PrintMUBAndTTLabelsEventArgs> OnPrintMUBLabelsFailed;
        public event EventHandler<PrintMUBAndTTLabelsEventArgs> OnPrintUBLabelsFailed;
        public event EventHandler<PrintMUBAndTTLabelsEventArgs> OnPrintTTLabelsFailed;
        public event EventHandler<ExceptionArgs> OnPrintMUBAndTTLabelsException;

        WebBrowser _web;

        public PrintMUBAndTTLabels(WebBrowser web)
        {
            _web = web;
        }

        internal void StartPrintMUB(LabelInfo labelInfo)
        {
            try
            {
                _web.SetLoading(false);
                if (labelInfo.IsMUB)
                {
                    this._web.LoadPageHtml("MUBAndTTLabel.html");
                }
                else
                {
                    this._web.LoadPageHtml("UBlabel.html");
                }
                //this._web.RunScript("$('.status-text').css('color','#000').text('Please wait');");

                //System.Threading.Thread.Sleep(1000);

                PrinterMonitor printerMonitor = PrinterMonitor.Instance;
                printerMonitor.OnPrintMUBLabelSucceeded += OnPrintMUBLabelsSucceeded;
                printerMonitor.OnMonitorException += OnPrintMUBAndTTLabelsException;

                BarcodePrinterUtil barcodeScannerUtils = BarcodePrinterUtil.Instance;

                Session session = Session.Instance;
                if (session.IsAuthenticated)
                {
                    #region Print MUBLabel
                    // Check status of Barcode printer
                    string mubLabelPrinterName = ConfigurationManager.AppSettings["MUBLabelPrinterName"];
                    var mubLabelPrinterStatus = barcodeScannerUtils.GetDeviceStatus(mubLabelPrinterName);

                    if (mubLabelPrinterStatus.Contains(EnumDeviceStatuses.Connected))
                    {
                        printerMonitor.PrintMUBLabel(labelInfo);
                    }
                    else
                    {
                        // Printer disconnect, get list status of the causes disconnected
                        string causeOfPrintMUBFailure = string.Empty;
                        foreach (var item in mubLabelPrinterStatus)
                        {
                            causeOfPrintMUBFailure = causeOfPrintMUBFailure + CommonUtil.GetDeviceStatusText(item) + "; ";
                        }

                        if (labelInfo.IsMUB)
                        {
                            RaisePrintMUBLabelsFailedEvent(new PrintMUBAndTTLabelsEventArgs("MUB Printer have problem: " + causeOfPrintMUBFailure));
                        }
                        else
                        {
                            RaisePrintUBLabelsFailedEvent(new PrintMUBAndTTLabelsEventArgs("UB Printer have problem: " + causeOfPrintMUBFailure));
                        }
                    }
                    #endregion
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

        internal void StartPrintTT(LabelInfo labelInfo)
        {
            try
            {
                _web.SetLoading(false);
                this._web.LoadPageHtml("MUBAndTTLabel.html");
                //this._web.RunScript("$('.status-text').css('color','#000').text('Please wait');");

                //System.Threading.Thread.Sleep(1000);

                PrinterMonitor printerMonitor = PrinterMonitor.Instance;
                printerMonitor.OnPrintTTLabelSucceeded += OnPrintTTLabelsSucceeded;
                printerMonitor.OnMonitorException += OnPrintMUBAndTTLabelsException;

                BarcodePrinterUtil barcodeScannerUtils = BarcodePrinterUtil.Instance;

                Session session = Session.Instance;
                if (session.IsAuthenticated)
                {
                    #region Print TTLabel
                    // Check status of Barcode printer
                    string ttLabelPrinterName = ConfigurationManager.AppSettings["TTLabelPrinterName"];
                    var ttLabelPrinterStatus = barcodeScannerUtils.GetDeviceStatus(ttLabelPrinterName);

                    if (ttLabelPrinterStatus.Contains(EnumDeviceStatuses.Connected))
                    {
                        printerMonitor.PrintBarcodeLabel(labelInfo);
                    }
                    else
                    {
                        // Printer disconnect, get list status of the causes disconnected
                        string causeOfPrintFailure = "";
                        foreach (var item in ttLabelPrinterStatus)
                        {
                            causeOfPrintFailure = causeOfPrintFailure + CommonUtil.GetDeviceStatusText(item) + "; ";
                        }

                        RaisePrintTTLabelsFailedEvent(new PrintMUBAndTTLabelsEventArgs("TTLabel Printer have problem: " + causeOfPrintFailure));

                        return;
                    }
                    #endregion
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
        protected virtual void RaisePrintMUBLabelsSucceededEvent(PrintMUBAndTTLabelsSucceedEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<PrintMUBAndTTLabelsSucceedEventArgs> handler = OnPrintMUBLabelsSucceeded;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Use the () operator to raise the event.
                handler(this, e);
            }
        }


        protected virtual void RaisePrintMUBLabelsFailedEvent(PrintMUBAndTTLabelsEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<PrintMUBAndTTLabelsEventArgs> handler = OnPrintMUBLabelsFailed;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Use the () operator to raise the event.
                handler(this, e);
            }
        }

        protected virtual void RaisePrintUBLabelsFailedEvent(PrintMUBAndTTLabelsEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<PrintMUBAndTTLabelsEventArgs> handler = OnPrintUBLabelsFailed;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Use the () operator to raise the event.
                handler(this, e);
            }
        }

        // Wrap event invocations inside a protected virtual method
        // to allow derived classes to override the event invocation behavior
        protected virtual void RaisePrintTTLabelsSucceededEvent(PrintMUBAndTTLabelsSucceedEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<PrintMUBAndTTLabelsSucceedEventArgs> handler = OnPrintTTLabelsSucceeded;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Use the () operator to raise the event.
                handler(this, e);
            }
        }


        protected virtual void RaisePrintTTLabelsFailedEvent(PrintMUBAndTTLabelsEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<PrintMUBAndTTLabelsEventArgs> handler = OnPrintTTLabelsFailed;

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
