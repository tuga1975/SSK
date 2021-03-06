﻿using DutyOfficer.Contstants;
using PCSC;
using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using Trinity.Common;
using Trinity.DAL;

namespace DutyOfficer.CodeBehind.Authentication
{
    public class SmartCard
    {
        private WebBrowser _web;

        public event Action OnSmartCardSucceeded;
        public event EventHandler<SmartCardEventArgs> OnSmartCardFailed;

        public SmartCard(WebBrowser web)
        {
            _web = web;
           
        }

        public void Start()
        {
            // redirect to Authentication/SmartCard
            _web.LoadPageHtml("Authentication/SmartCard.html");
            _web.RunScript("$('.status-text').css('color','#000').text('Please place your smart card on the reader.');");

            // StartCardMonitor
            Trinity.Common.Monitor.SCardMonitor sCardMonitor = Trinity.Common.Monitor.SCardMonitor.Instance;
            sCardMonitor.StartCardMonitor(OnCardInitialized, OnCardInserted, OnCardRemoved);
        }

        // Wrap event invocations inside a protected virtual method
        // to allow derived classes to override the event invocation behavior
        protected virtual void RaiseSmartCardSucceededEvent()
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            Action handler = OnSmartCardSucceeded;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Use the () operator to raise the event.
                handler();
            }
        }

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
            Trinity.Common.Monitor.SCardMonitor sCardMonitor = Trinity.Common.Monitor.SCardMonitor.Instance;
            string cardUID = sCardMonitor.GetCardUID();
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
            Trinity.Common.Monitor.SCardMonitor sCardMonitor = Trinity.Common.Monitor.SCardMonitor.Instance;
            string cardUID = sCardMonitor.GetCardUID();
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
                if (user!=null && user.Role != EnumUserRoles.DutyOfficer)
                    user = null;
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

                // Stop SCardMonitor
                Trinity.Common.Monitor.SCardMonitor sCardMonitor = Trinity.Common.Monitor.SCardMonitor.Instance;
                sCardMonitor.Stop();

                // raise succeeded event
                RaiseSmartCardSucceededEvent();
            }
            else
            {
                // raise failed event
                RaiseSmartCardFailedEvent(new SmartCardEventArgs("Unable to read your smart card. Please report to the Duty Officer"));
            }
        }
    }

    #region Custom Events
    public class SmartCardEventArgs : EventArgs
    {
        private string _message;
        public SmartCardEventArgs(string message)
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
