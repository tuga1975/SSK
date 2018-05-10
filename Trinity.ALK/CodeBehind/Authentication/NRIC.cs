using System;
using System.Linq;
using System.Windows.Forms;
using Trinity.Common;
using Trinity.Common.Utils;
using Trinity.DAL;
using Trinity.Device.Util;

namespace ALK.CodeBehind.Authentication
{
    public class NRIC
    {
        WebBrowser _web;
        public event Action OnNRICSucceeded;
        public event EventHandler<ShowMessageEventArgs> OnShowMessage;

        #region Singleton Implementation
        // The variable is declared to be volatile to ensure that assignment to the instance variable completes before the instance variable can be accessed
        private static volatile NRIC _instance;

        // Uses a syncRoot instance to lock on, rather than locking on the type itself, to avoid deadlocks.
        private static object syncRoot = new Object();

        public static NRIC GetInstance(WebBrowser web)
        {
            if (_instance == null)
            {
                lock (syncRoot) // now I can claim some form of thread safety...
                {
                    if (_instance == null)
                    {
                        _instance = new NRIC(web);
                    }
                }
            }

            return _instance;
        }
        #endregion

        public NRIC(WebBrowser web)
        {
            _web = web;
        }

        internal void Start()
        {
            _web.LoadPageHtml("Authentication/NRIC.html");

            if (BarcodeScannerUtil.Instance.GetDeviceStatus().Contains(EnumDeviceStatus.Connected))
            {
                System.Threading.Tasks.Task.Factory.StartNew(() => BarcodeScannerUtil.Instance.StartScanning(BarcodeScannerCallback));
            }
        }

        public void BarcodeScannerCallback(string value, string error)
        {
            //MessageBox.Show("BarcodeScannerCallback");
            if (string.IsNullOrEmpty(error))
            {
                // Fill value to the textbox
                CSCallJS.InvokeScript(_web, "updateNRICTextValue", value.Trim());

                // Execute authentication
                NRICAuthentication(value.Trim());
            }
            else
            {
                LogManager.Error("BarcodeScannerCallback ERROR: " + error);
                //CSCallJS.ShowMessageAsync(_web, "BarcodeScanner ERROR", error);
            }
        }

        // Wrap event invocations inside a protected virtual method
        // to allow derived classes to override the event invocation behavior
        protected virtual void RaiseNRICSucceededEvent()
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            Action handler = OnNRICSucceeded;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Use the () operator to raise the event.
                handler();
            }
        }

        protected virtual void RaiseShowMessage(ShowMessageEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<ShowMessageEventArgs> handler = OnShowMessage;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Use the () operator to raise the event.
                handler(this, e);
            }
        }

        public void NRICAuthentication(string nric)
        {
            DAL_User dal_User = new DAL_User();
            var supervisee = dal_User.GetSuperviseeByNRIC(nric);

            if (supervisee == null)
            {
                CSCallJS.ShowMessage(_web, "NRIC " + nric + " is not registered");
                System.Threading.Tasks.Task.Factory.StartNew(() => BarcodeScannerUtil.Instance.StartScanning(BarcodeScannerCallback));
                return;
            }
            else
            {
                if (supervisee.Status == EnumUserStatuses.Blocked)
                {
                    CSCallJS.ShowMessage(_web, "This supervisee was blocked");
                    System.Threading.Tasks.Task.Factory.StartNew(() => BarcodeScannerUtil.Instance.StartScanning(BarcodeScannerCallback));
                    return;
                }

                // Create a session object to store UserLogin information
                Session session = Session.Instance;
                session.IsSmartCardAuthenticated = true;
                session.IsFingerprintAuthenticated = true;
                session[CommonConstants.SUPERVISEE] = supervisee;

                BarcodeScannerUtil.Instance.Disconnect();

                // raise succeeded event
                RaiseNRICSucceededEvent();
            }
        }
    }
}
