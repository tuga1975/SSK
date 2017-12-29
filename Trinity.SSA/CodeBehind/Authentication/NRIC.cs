using SSA.Constants;
using System;
using System.Windows.Forms;
using Trinity.Common;
using Trinity.DAL;

namespace SSA.CodeBehind.Authentication
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

            #region Start Barcode Scanner here to scan NRIC
            // Start Barcode Scanner to scan NRIC
            //var barcodeScanner = BarcodeSannerMonitor.Instance;
            // if succeeded, auto get NRIC info to textbox NIRC on screen
            // else -- scan failed, show message failed
            //    RaiseShowMessage(new ShowMessageEventArgs(" Unable to scan  NRIC. \n Please enter manually.", "", MessageBoxButtons.OK, MessageBoxIcon.Error));
            #endregion
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
            var user = dal_User.GetSuperviseeByNRIC(nric, true);

            // if local user is null, get user from centralized, and sync db
            if (user == null)
            {
                user = dal_User.GetSuperviseeByNRIC(nric, false);
            }

            // if centralized user is null
            // raise failsed event and return false
            if (user == null)
            {
                // raise show message event, then return
                RaiseShowMessage(new ShowMessageEventArgs("NRIC " + nric + ": not found. Please check NRIC again.", "Not found", MessageBoxButtons.OK, MessageBoxIcon.Warning));
                return;
            }

            // Create a session object to store UserLogin information
            Session session = Session.Instance;
            session.IsSmartCardAuthenticated = true;
            session.IsFingerprintAuthenticated = true;
            session[CommonConstants.SUPERVISEE] = user;

            // raise succeeded event
            RaiseNRICSucceededEvent();
        }
    }
}
