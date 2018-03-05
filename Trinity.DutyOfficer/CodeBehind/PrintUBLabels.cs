using System;
using System.Configuration;
using System.Linq;
using System.Windows.Forms;
using Trinity.Common;
using Trinity.Device;
using Trinity.Device.Util;

namespace DutyOfficer.CodeBehind
{
    public class PrintUBLabels
    {
        public event EventHandler<PrintUBLabelsSucceedEventArgs> OnPrintUBLabelsSucceeded;
        public event EventHandler<PrintUBLabelsEventArgs> OnPrintUBLabelsFailed;
        public event EventHandler<ExceptionArgs> OnPrintUBLabelsException;

        WebBrowser _web;

        public PrintUBLabels(WebBrowser web)
        {
            _web = web;
        }

        internal void Start(LabelInfo labelInfo)
        {
            try
            {
                _web.SetLoading(false);
                this._web.LoadPageHtml("UBlabel.html");
                //this._web.RunScript("$('.status-text').css('color','#000').text('Please wait');");

                System.Threading.Thread.Sleep(1500);

                PrinterMonitor printerMonitor = PrinterMonitor.Instance;
                //printerMonitor.OnPrintLabelSucceeded += OnPrintUBLabelsSucceeded;//RaisePrintUBLabelsSucceededEvent;
                printerMonitor.OnMonitorException += OnPrintUBLabelsException;

                BarcodePrinterUtil barcodeScannerUtils = BarcodePrinterUtil.Instance;

                Session session = Session.Instance;
                if (session.IsAuthenticated)
                {
                    #region Print Barcode
                    // Check status of Barcode printer
                    string barcodePrinterName = ConfigurationManager.AppSettings["BarcodePrinterName"].ToUpper();
                    var statusPrinterBarcode = barcodeScannerUtils.GetDeviceStatus(barcodePrinterName);

                    if (statusPrinterBarcode.Count() == 1 && statusPrinterBarcode[0] == EnumDeviceStatuses.Connected)
                        printerMonitor.PrintBarcodeLabel(labelInfo);
                    else
                    {
                        // Printer disconnect, get list status of the causes disconnected
                        string causeOfPrintFailure = "";
                        foreach (var item in statusPrinterBarcode)
                        {
                            causeOfPrintFailure = causeOfPrintFailure + CommonUtil.GetDeviceStatusText(item) + "; ";
                        }

                        RaisePrintUBLabelsFailedEvent(new PrintUBLabelsEventArgs("Barcode Printer have problem: " + causeOfPrintFailure));

                        return;
                    }
                    #endregion

                    #region Print UB (QRCode)
                    // Check status of Barcode printer
                    string UBPrinterName = ConfigurationManager.AppSettings["UBPrinterName"].ToUpper();
                    var statusPrinterUB = barcodeScannerUtils.GetDeviceStatus(UBPrinterName);

                    if (statusPrinterUB.Count() == 1 && statusPrinterUB[0] == EnumDeviceStatuses.Connected)
                        printerMonitor.PrintMUBLabel(labelInfo);
                    else
                    {
                        // Printer disconnect, get list status of the causes disconnected
                        string causeOfPrintUBFailure = "";
                        foreach (var item in statusPrinterUB)
                        {
                            causeOfPrintUBFailure = causeOfPrintUBFailure + CommonUtil.GetDeviceStatusText(item) + "; ";
                        }

                        RaisePrintUBLabelsFailedEvent(new PrintUBLabelsEventArgs("UB Printer have problem: " + causeOfPrintUBFailure));
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                RaisePrintUBLabelsExceptionEvent(new ExceptionArgs(new FailedInfo()
                {
                    ErrorCode = (int)EnumErrorCodes.UnknownError,
                    ErrorMessage = new ErrorInfo().GetErrorMessage(EnumErrorCodes.UnknownError)
                }));
            }
        }


        // Wrap event invocations inside a protected virtual method
        // to allow derived classes to override the event invocation behavior
        protected virtual void RaisePrintUBLabelsSucceededEvent(PrintUBLabelsSucceedEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<PrintUBLabelsSucceedEventArgs> handler = OnPrintUBLabelsSucceeded;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Use the () operator to raise the event.
                handler(this, e);
            }
        }


        protected virtual void RaisePrintUBLabelsFailedEvent(PrintUBLabelsEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<PrintUBLabelsEventArgs> handler = OnPrintUBLabelsFailed;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Use the () operator to raise the event.
                handler(this, e);
            }
        }

        protected virtual void RaisePrintUBLabelsExceptionEvent(ExceptionArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            OnPrintUBLabelsException?.Invoke(this, e);
        }
    }

    #region Custom Events
    public class PrintUBLabelsEventArgs : EventArgs
    {
        private string _message;
        public PrintUBLabelsEventArgs(string message)
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
