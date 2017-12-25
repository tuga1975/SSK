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
    public class PrintTTLabel
    {
        public event Action OnPrintTTLabelSucceeded;
        public event EventHandler<PrintTTLabelEventArgs> OnPrintTTLabelFailed;
        public event EventHandler<ExceptionArgs> OnPrintTTLabelException;

        WebBrowser _web;

        public PrintTTLabel(WebBrowser web)
        {
            _web = web;
        }

        internal void Start()
        {
            this._web.LoadPageHtml("PrintingMUBAndTTLabels.html");
            this._web.RunScript("$('.status-text').css('color','#000').text('Please wait');");

            PrinterMonitor printerMonitor = PrinterMonitor.Instance;
            printerMonitor.OnPrintLabelSucceeded += RaisePrintTTLabelSucceededEvent;
            printerMonitor.OnMonitorException += OnPrintTTLabelException;
            
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
                    RaisePrintTTLabelSucceededEvent();
                }
            }
        }


        // Wrap event invocations inside a protected virtual method
        // to allow derived classes to override the event invocation behavior
        protected virtual void RaisePrintTTLabelSucceededEvent()
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            Action handler = OnPrintTTLabelSucceeded;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Use the () operator to raise the event.
                handler();
            }
        }


        protected virtual void RaisePrintTTLabelFailedEvent(PrintTTLabelEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<PrintTTLabelEventArgs> handler = OnPrintTTLabelFailed;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Use the () operator to raise the event.
                handler(this, e);
            }
        }

        protected virtual void RaisePrintTTLabelExceptionEvent(ExceptionArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            OnPrintTTLabelException?.Invoke(this, e);
        }
    }

    #region Custom Events
    public class PrintTTLabelEventArgs : EventArgs
    {
        private string _message;
        public PrintTTLabelEventArgs(string message)
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
