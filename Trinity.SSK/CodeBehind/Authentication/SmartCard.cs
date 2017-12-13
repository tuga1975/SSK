using PCSC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Trinity.DAL.DAL;

namespace SSK.CodeBehind.Authentication
{
    class SmartCard
    {
        private WebBrowser _web;
        int _numberOfFailed = 0;

        public SmartCard(WebBrowser web)
        {
            _web = web;

            web.LoadPageHtml("Authentication/SmartCard.html");
            web.RunScript("$('.status-text').css('color','#000').text('Please place your smart card on the reader.');");
            StartCardMonitor();
        }

        private void StartCardMonitor()
        {
            DeviceMonitor.SCardMonitor.StartCardMonitor(OnCardInitialized, OnCardInserted, OnCardRemoved);
        }

        private void OnCardInitialized(object sender, CardStatusEventArgs e)
        {
            Debug.WriteLine("onCardInitialized");
            string cardUID = DeviceMonitor.SCardMonitor.GetCardUID();
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
            DAL_User dAL_User = new DAL_User();
            var user = dAL_User.GetUserBySmartCardId(cardUID);

            if (user != null)
            {
                _web.RunScript("$('.status-text').css('color','#000').text('Smart card authentication is sucessful.');");
                Thread.Sleep(3000);

                _web.LoadPageHtml("Authentication/FingerPrint.html");
                _web.RunScript("$('.status-text').css('color','#000').text('Please place your Finger on the reader.');");

                DeviceMonitor.SCardMonitor.Dispose();

                Fingerprint fingerprint = new Fingerprint(_web, user.Fingerprint_Template);
                fingerprint.Start();
            }
            else
            {
                _numberOfFailed++;
                _web.RunScript("$('.status-text').css('color','#000').text('Please place your smart card on the reader. Failed: " + _numberOfFailed + "');");
            }
        }
    }
}
