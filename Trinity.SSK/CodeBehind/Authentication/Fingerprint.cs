using SSK.DeviceMonitor;
using System;
using System.Threading;
using System.Windows.Forms;
using Trinity.Common;

namespace SSK.CodeBehind.Authentication
{
    public class Fingerprint
    {
        public event EventHandler<FingerprintEventArgs> OnFingerprintFailed;

        WebBrowser _web;
        byte[] _fingerprint_Template;
        int _failedCount = 0;

        public Fingerprint(WebBrowser web)
        {
            _web = web;
        }

        public Fingerprint(WebBrowser web, byte[] fingerprint_Template)
        {
            _web = web;
            _fingerprint_Template = fingerprint_Template;
        }

        public byte[] FingerprintTemplate
        {
            get
            {
                return _fingerprint_Template;
            }
            set
            {
                _fingerprint_Template = value;
            }
        }

        internal void Start()
        {
            StartVerification();
        }

        private void StartVerification()
        {
            FingerprintMonitor.StartVerification(OnVerificationComplete, _fingerprint_Template);
        }

        private void OnVerificationComplete(bool bSuccess, int nRetCode, bool bVerificationSuccess)
        {
            var result = FingerprintMonitor.VerificationResult(bSuccess, nRetCode, bVerificationSuccess);
            if (result)
            {
                //
                // Login successfully
                //
                // Create a session object to store UserLogin information
                // Create a session object to store UserLogin information
                Session session = Session.Instance;
                session.IsFingerprintAuthenticated = true;

                _web.RunScript("$('.status-text').css('color','#000').text('Fingerprint authentication is successful.');");
                Thread.Sleep(2000);

                _web.LoadPageHtml("Supervisee.html");
            }
            else
            {
                _failedCount++;
                string script = "$('.status-text').css('color','#000').text('Please place your finger on the reader. Failed: " + _failedCount + "');";
                _web.RunScript(script);
                if (_failedCount > 3)
                {
                    // If FailedCount > 3 then raise Smart Card Failure
                    RaiseFingerprintFailedEvent(new FingerprintEventArgs("Unable to read your fingerprint. Please report to the Duty Officer", _failedCount));
                }
                FingerprintMonitor.StartVerification(OnVerificationComplete, _fingerprint_Template);
            }
        }
        // Wrap event invocations inside a protected virtual method
        // to allow derived classes to override the event invocation behavior
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

    }

    #region Custom Events
    public class FingerprintEventArgs : EventArgs
    {
        private int _failedCount;
        private string _message;
        public FingerprintEventArgs(string message, int failedCount)
        {
            _message = message;
            _failedCount = failedCount;
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
        public int FailedCount
        {
            get
            {
                return _failedCount;
            }
            set
            {
                _failedCount = value;
            }
        }
    }

    #endregion
}
