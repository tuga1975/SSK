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

namespace SSA.CodeBehind
{
    public class PrintMUBAndTTLabels
    {
        public event Action OnPrintMUBAndTTLabelsSucceeded;
        public event EventHandler<PrintMUBAndTTLabelsEventArgs> OnPrintMUBAndTTLabelsFailed;
        public event EventHandler<ExceptionArgs> OnPrintMUBAndTTLabelsException;

        WebBrowser _web;

        public PrintMUBAndTTLabels(WebBrowser web)
        {
            _web = web;
        }

        internal void Start()
        {
            this._web.LoadPageHtml("PrintingMUBAndTTLabels.html");
            this._web.RunScript("$('.status-text').css('color','#000').text('Please wait');");

            PrinterMonitor printerMonitor = PrinterMonitor.Instance;
            printerMonitor.OnPrintLabelSucceeded += RaisePrintMUBAndTTLabelsSucceededEvent;
            printerMonitor.OnMonitorException += OnPrintMUBAndTTLabelsException;
            
            BarcodePrinterUtils barcodeScannerUtils = BarcodePrinterUtils.Instance;           

            Session session = Session.Instance;
            if (session.IsAuthenticated)
            {
                Trinity.BE.User user = (Trinity.BE.User)session[Constants.CommonConstants.USER_LOGIN];

                var dalUser = new Trinity.DAL.DAL_User();
                var dalUserprofile = new Trinity.DAL.DAL_UserProfile();
                var userInfo = new UserInfo
                {
                    UserName = user.Name,
                    NRIC = user.NRIC,
                    DOB = dalUserprofile.GetUserProfileByUserId(user.UserId, true).DOB.ToString()
                };

                // Print BarCode
                //if (barcodeScannerUtils.GetDeviceStatus().Connected)
                //{
                //    printerMonitor.PrintLabel(userInfo);
                //}
                //else
                //{
                //    Console.WriteLine("Barcode printer is not connected.");
                //}

                // Print QR Code
                if (barcodeScannerUtils.PrintQRCodeUserInfo(userInfo))
                {
                    // raise succeeded event
                    RaisePrintMUBAndTTLabelsSucceededEvent();
                }
            }
        }


        // Wrap event invocations inside a protected virtual method
        // to allow derived classes to override the event invocation behavior
        protected virtual void RaisePrintMUBAndTTLabelsSucceededEvent()
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            Action handler = OnPrintMUBAndTTLabelsSucceeded;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Use the () operator to raise the event.
                handler();
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
