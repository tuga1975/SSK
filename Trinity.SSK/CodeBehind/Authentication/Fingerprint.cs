﻿using SSK.Contstants;
using SSK.DeviceMonitor;
using System.Threading;
using System.Windows.Forms;
using Trinity.Common;

namespace SSK.CodeBehind.Authentication
{
    class Fingerprint
    {
        WebBrowser _web;
        byte[] _fingerprint_Template;
        int _numberOfFailed = 0;
        public Fingerprint(WebBrowser web, byte[] fingerprint_Template)
        {
            _web = web;
            _fingerprint_Template = fingerprint_Template;
        }
        internal void Start()
        {
            _web.LoadPageHtml("Authentication/FingerPrint.html");
            _web.RunScript("$('.status-text').css('color','#000').text('Please place your Finger on the reader.');");

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

                // if supervisee login, redirect to Supervisee.html
                // if duty officer override, redirect to NRIC.html
                Trinity.BE.User user = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];
                //if (user.Type)
                //{

                //}
                _web.LoadPageHtml("Supervisee.html");
            }
            else
            {
                _numberOfFailed++;
                string script = "$('.status-text').css('color','#000').text('Please place your finger on the reader. Failed: " + _numberOfFailed + "');";
                _web.RunScript(script);

                FingerprintMonitor.StartVerification(OnVerificationComplete, _fingerprint_Template);
            }
        }
    }
}
