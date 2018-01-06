using System;
using System.Threading;
using System.Windows.Forms;
using Trinity.Common;
using Trinity.Common.Common;
using Trinity.Common.Monitor;

namespace SSA.CodeBehind.Authentication
{
    public class Fingerprint
    {
        public event Action OnFingerprintSucceeded;
        public event EventHandler<FingerprintEventArgs> OnFingerprintFailed;
        public event EventHandler<ShowMessageEventArgs> OnShowMessage;

        WebBrowser _web;

        public Fingerprint(WebBrowser web)
        {
            _web = web;
        }

        internal void Start()
        {
            // redirect to Fingerprint html
            _web.LoadPageHtml("Authentication/FingerPrint.html");
            _web.RunScript("$('.status-text').css('color','#000').text('Please place your finger on the reader.');");


            // start verification
            FingerprintMonitor fingerprintMonitor = FingerprintMonitor.Instance;
            Session session = Session.Instance;
            var user = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];
            fingerprintMonitor.StartVerification(OnVerificationComplete, user.RightThumbFingerprint);
        }

        private void OnVerificationComplete(bool bSuccess, int nRetCode, bool bVerificationSuccess)
        {
            FingerprintMonitor fingerprintMonitor = FingerprintMonitor.Instance;

            // if fingerprinter is connected
            if (bSuccess)
            {
                // if true: Verification is successful
                if (bVerificationSuccess)
                {
                    // Raise SmartCard Succeeded Event
                    RaiseFingerprintSucceededEvent();
                }
                else
                {
                    // raise failed event
                    RaiseFingerprintFailedEvent(new FingerprintEventArgs("Unable to read your fingerprint. Please report to the Duty Officer"));

                    Session session = Session.Instance;
                    var user = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];
                    fingerprintMonitor.StartVerification(OnVerificationComplete, user.RightThumbFingerprint);
                }
            }
            else
            {
                string error = Futronic.SDKHelper.FutronicSdkBase.SdkRetCode2Message(nRetCode);

                // raise show message event
                RaiseFingerprintShowMessage(new ShowMessageEventArgs(error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning));
            }
        }

        // Wrap event invocations inside a protected virtual method
        // to allow derived classes to override the event invocation behavior
        protected virtual void RaiseFingerprintSucceededEvent()
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            Action handler = OnFingerprintSucceeded;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Use the () operator to raise the event.
                handler();
            }
        }


        protected virtual void RaiseFingerprintFailedEvent(FingerprintEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<FingerprintEventArgs> handler = OnFingerprintFailed;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Use the () operator to raise the event.
                handler(this, e);
            }
        }

        protected virtual void RaiseFingerprintShowMessage(ShowMessageEventArgs e)
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
    }

    #region Custom Events
    public class FingerprintEventArgs : EventArgs
    {
        private string _message;
        public FingerprintEventArgs(string message)
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
