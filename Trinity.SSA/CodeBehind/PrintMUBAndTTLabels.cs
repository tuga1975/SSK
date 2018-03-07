using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Trinity.Common;
using Trinity.Device;
using Trinity.Device.Util;

namespace SSA.CodeBehind
{
    public class PrintMUBAndTTLabels
    {
        public event EventHandler<Trinity.Common.PrintMUBAndTTLabelsEventArgs> OnPrintMUBLabelsSucceeded;
        public event EventHandler<Trinity.Common.PrintMUBAndTTLabelsEventArgs> OnPrintTTLabelsSucceeded;
        public event EventHandler<Trinity.Common.PrintMUBAndTTLabelsEventArgs> OnPrintMUBLabelsFailed;
        public event EventHandler<Trinity.Common.PrintMUBAndTTLabelsEventArgs> OnPrintTTLabelsFailed;
        public event EventHandler<ExceptionArgs> OnPrintMUBAndTTLabelsException;

        WebBrowser _web;
        JSCallCS _jsCallCs;

        public PrintMUBAndTTLabels(JSCallCS _jsCallCs)
        {
            _web = Lib.LayerWeb;
            this._jsCallCs = _jsCallCs;
        }

        internal void Start(LabelInfo labelInfo)
        {
            try
            {
                Lib.LayerWeb.SetLoading(false);
                //this._web.LoadPageHtml("PrintingMUBAndTTLabels.html");
                //this._web.RunScript("$('.status-text').css('color','#000').text('Please wait');");
                Lib.LayerWeb.LoadPageHtml("SuperviseeParticulars.html", labelInfo);
                System.Threading.Thread.Sleep(500);
                Trinity.BE.PopupModel popupModel = new Trinity.BE.PopupModel();
                popupModel.Title = "MUB and TT Labels \n\nPrinting in Progress";
                popupModel.Message = "Please wait a moment";
                popupModel.IsShowLoading = true;
                popupModel.IsShowOK = false;
                Lib.LayerWeb.InvokeScript("showPopupModal", JsonConvert.SerializeObject(popupModel));

                System.Threading.Thread.Sleep(200);

                PrinterMonitor printerMonitor = PrinterMonitor.Instance;
                printerMonitor.OnPrintMUBLabelSucceeded += OnPrintMUBLabelsSucceeded;
                printerMonitor.OnPrintTTLabelSucceeded += OnPrintTTLabelsSucceeded;//RaisePrintMUBAndTTLabelsSucceededEvent;
                printerMonitor.OnMonitorException += OnPrintMUBAndTTLabelsException;
                
                Session session = Session.Instance;
                if (session.IsAuthenticated)
                {
                    List<Task> tasks = new List<Task>();
                    tasks.Add(Task.Run(() => { PrintMUBLabel(printerMonitor, labelInfo); }));
                    tasks.Add(Task.Run(() => { PrintTTLabel(printerMonitor, labelInfo); }));
                    Task.WaitAll(tasks.ToArray());

                    Lib.LayerWeb.InvokeScript("closePopup");

                    _jsCallCs.OnEventPrintFinished();
                }
            }
            catch (Exception ex)
            {
                RaisePrintMUBAndTTLabelsExceptionEvent(new ExceptionArgs(new FailedInfo()
                {
                    ErrorCode = (int)EnumErrorCodes.UnknownError,
                    ErrorMessage = ex.Message
                }));
            }
        }

        private void PrintTTLabel(PrinterMonitor printerMonitor, LabelInfo labelInfo)
        {
            BarcodePrinterUtil barcodeScannerUtils = BarcodePrinterUtil.Instance;
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

                labelInfo.PrintStatus = EnumPrintStatus.Failed;
                labelInfo.Message = causeOfPrintFailure;
                RaisePrintTTLabelsFailedEvent(new Trinity.Common.PrintMUBAndTTLabelsEventArgs(labelInfo));
            }
        }

        private void PrintMUBLabel(PrinterMonitor printerMonitor, LabelInfo labelInfo)
        {
            BarcodePrinterUtil barcodeScannerUtils = BarcodePrinterUtil.Instance;
            // Check status of Barcode printer
            string mubLabelPrinterName = ConfigurationManager.AppSettings["MUBLabelPrinterName"];
            EnumDeviceStatuses[] mubLabelPrinterStatuses = barcodeScannerUtils.GetDeviceStatus(mubLabelPrinterName);
            if (mubLabelPrinterStatuses.Contains(EnumDeviceStatuses.Connected))
            {
                printerMonitor.PrintMUBLabel(labelInfo);
            }
            else
            {
                // Printer disconnect, get list status of the causes disconnected
                string causeOfPrintMUBFailure = string.Empty;
                foreach (var item in mubLabelPrinterStatuses)
                {
                    causeOfPrintMUBFailure = causeOfPrintMUBFailure + CommonUtil.GetDeviceStatusText(item) + "; ";
                }

                labelInfo.PrintStatus = EnumPrintStatus.Failed;
                labelInfo.Message = causeOfPrintMUBFailure;
                RaisePrintMUBLabelsFailedEvent(new Trinity.Common.PrintMUBAndTTLabelsEventArgs(labelInfo));
            }
        }

        // Wrap event invocations inside a protected virtual method
        // to allow derived classes to override the event invocation behavior
        protected virtual void RaisePrintMUBLabelsSucceededEvent(Trinity.Common.PrintMUBAndTTLabelsEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<Trinity.Common.PrintMUBAndTTLabelsEventArgs> handler = OnPrintMUBLabelsSucceeded;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Use the () operator to raise the event.
                handler(this, e);
            }
        }


        protected virtual void RaisePrintMUBLabelsFailedEvent(Trinity.Common.PrintMUBAndTTLabelsEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<Trinity.Common.PrintMUBAndTTLabelsEventArgs> handler = OnPrintMUBLabelsFailed;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Use the () operator to raise the event.
                handler(this, e);
            }
        }

        // Wrap event invocations inside a protected virtual method
        // to allow derived classes to override the event invocation behavior
        protected virtual void RaisePrintTTLabelsSucceededEvent(Trinity.Common.PrintMUBAndTTLabelsEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<Trinity.Common.PrintMUBAndTTLabelsEventArgs> handler = OnPrintTTLabelsSucceeded;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Use the () operator to raise the event.
                handler(this, e);
            }
        }


        protected virtual void RaisePrintTTLabelsFailedEvent(Trinity.Common.PrintMUBAndTTLabelsEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<Trinity.Common.PrintMUBAndTTLabelsEventArgs> handler = OnPrintTTLabelsFailed;

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
