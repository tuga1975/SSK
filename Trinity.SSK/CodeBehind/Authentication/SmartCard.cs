using PCSC;
using SSK.Contstants;
using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using Trinity.Common;
using Trinity.DAL;

namespace SSK.CodeBehind.Authentication
{
    public class SmartCard
    {
        private WebBrowser _web;
        int _failedCount = 0;

        public event EventHandler<SmartCardEventArgs> OnSmartCardFailed;
        private Fingerprint _fingerprint;

        public SmartCard(WebBrowser web, Fingerprint fingerprint)
        {
            _web = web;
            //_fingerprint = new Fingerprint(_web);
            _fingerprint = fingerprint;
           
        }

        public void Start()
        {
            _web.LoadPageHtml("Authentication/SmartCard.html");
            _web.RunScript("$('.status-text').css('color','#000').text('Please place your smart card on the reader.');");
            DeviceMonitor.SCardMonitor.StartCardMonitor(OnCardInitialized, OnCardInserted, OnCardRemoved);
            // for testing purpose
            //_web.LoadPageHtml("Supervisee.html");
        }

        // Wrap event invocations inside a protected virtual method
        // to allow derived classes to override the event invocation behavior
        protected virtual void RaiseSmartCardFailedEvent(SmartCardEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<SmartCardEventArgs> handler = OnSmartCardFailed;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Use the () operator to raise the event.
                handler(this, e);
            }
        }

        private void OnCardInitialized(object sender, CardStatusEventArgs e)
        {
            Debug.WriteLine("onCardInitialized");
            string cardUID = DeviceMonitor.SCardMonitor.GetCardUID();
            if (string.IsNullOrEmpty(cardUID))
            {
                return;
            }
            Debug.WriteLine($"Card UID: {cardUID}");
            SmartCardLoginProcess(cardUID);
        }

        private void OnCardInserted(object sender, CardStatusEventArgs e)
        {
            Debug.WriteLine("OnCardInserted");
            string cardUID = DeviceMonitor.SCardMonitor.GetCardUID();
            Debug.WriteLine($"Card UID: {cardUID}");
            SmartCardLoginProcess(cardUID);
        }

        private void OnCardRemoved(object sender, CardStatusEventArgs e)
        {
            Debug.WriteLine("OnCardRemoved");
        }

        private void SmartCardLoginProcess(string cardUID)
        {
            Debug.WriteLine($"Card UID: {cardUID}");

            // get local user info
            DAL_User dAL_User = new DAL_User();
            var user = dAL_User.GetUserBySmartCardId(cardUID, true);

            // if local user is null, get user from centralized, and sync db
            if (user == null)
            {
                user = dAL_User.GetUserBySmartCardId(cardUID, false);
            }

            if (user != null)
            {
                //
                // SmartCard is authenticated
                //
                // Create a session object to store UserLogin information
                Session session = Session.Instance;
                session.IsSmartCardAuthenticated = true;
                session[CommonConstants.USER_LOGIN] = user;

                _web.RunScript("$('.status-text').css('color','#000').text('Your smart card is authenticated.');");
                DeviceMonitor.SCardMonitor.Dispose();
                Thread.Sleep(3000);

                //Fingerprint fingerprint = new Fingerprint(_web, user.Fingerprint);
                _fingerprint.FingerprintTemplate = user.Fingerprint;
                _fingerprint.Start();
            }
            else
            {
                _failedCount++;
                _web.RunScript("$('.status-text').css('color','#000').text('Please place your smart card on the reader. Failed: " + _failedCount + "');");
                if (_failedCount > 3)
                {
                    // If FailedCount > 3 then raise Smart Card Failure
                    RaiseSmartCardFailedEvent(new SmartCardEventArgs("Unable to read your smart card. Please report to the Duty Officer", _failedCount));
                    _failedCount = 0;
                    _web.RunScript("$('.status-text').css('color','#000').text('Please place your smart card on the reader.');");
                }
            }
        }
    }

    #region Custom Events
    public class SmartCardEventArgs : EventArgs
    {
        private int _failedCount;
        private string _message;
        public SmartCardEventArgs(string message, int failedCount)
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
        public int FailedCount {
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
