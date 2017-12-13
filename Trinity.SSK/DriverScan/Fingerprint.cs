using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Trinity.DAL;

namespace SSK.DriverScan
{
    public class FingerPrint
    {
        private WebBrowser web = null;
        private SmartCard smartCard = null;
        private int CountScan = 0;
        private Trinity.DAL.DBContext.TrinityCentralizedDBEntities sSKCentralizedEntities = null;
        public FingerPrint(SmartCard smartCard, WebBrowser web)
        {
            this.web = web;
            sSKCentralizedEntities = new Trinity.DAL.DBContext.TrinityCentralizedDBEntities();
            this.smartCard = smartCard;
        }

        public void Scanning()
        {
            Thread thread = new Thread(ThreadScan);
            thread.IsBackground = true;
            thread.Start();
        }



        private void ThreadScan()
        {
            // Add event Finger
            //APIUtils.SCardMonitor.CardInitialized += CardInitialized;
            //APIUtils.SCardMonitor.CardInserted += CardInserted;
            //APIUtils.SCardMonitor.CardRemoved += CardRemoved;


            web.LoadPageHtml("Authentication/FingerPrint.html");
            web.RunScript("$('.status-text').css('color','#000').text('Please place your Finger on the reader.');");


            Thread.Sleep(2000);
            web.LoadPageHtml("Supervisee.html");
        }



        private void FingerInitialized(string cardInfo)
        {
            CheckLoginUser(cardInfo);

        }
        private void FingerInserted(string cardInfo)
        {
            CheckLoginUser(cardInfo);
        }

        private void FingerRemoved()
        {
            web.RunScript("$('.status-text').css('color','#000').text('Please place your finger on the reader.');");
        }
        private void CheckLoginUser(string cardInfo)
        {
            CountScan++;
            if (!string.IsNullOrEmpty(cardInfo))
            {
                var user = sSKCentralizedEntities.Users.Where(d => d.UserId == cardInfo).FirstOrDefault();
                if (user != null)
                {
                    // remove event Finger
                    //APIUtils.SCardMonitor.CardInitialized -= CardInitialized;
                    //APIUtils.SCardMonitor.CardInserted -= CardInserted;
                    //APIUtils.SCardMonitor.CardRemoved -= CardRemoved;

                    web.RunScript("$('.status-text').css('color','blue').text('Authentication Successful');");
                    Thread.Sleep(1000);
                    web.LoadPageHtml("Supervisee.html");
                }
                else if (CountScan >= 3)
                {
                    CreateNoti(cardInfo);
                }
                else
                {
                    web.RunScript("$('.status-text').text('Finger Scanning Failure. Please try again.');");
                }
            }
            else if (string.IsNullOrEmpty(cardInfo) && CountScan >= 3)
            {
                CreateNoti(cardInfo);
            }
            else if (string.IsNullOrEmpty(cardInfo))
            {
                web.RunScript("$('.status-text').text('Finger Scanning Failure. Please try again.');");
            }
        }
        private void CreateNoti(string cardInfo)
        {
            APIUtils.SignalR.SendNotificationToDutyOfficer("Authentication Finger Failure", "Authentication Finger Failure");
            MessageBox.Show("Unable to read your fingerprint. Please report to the Duty Officer");
            CountScan = 0;
            // remove event Finger
            //APIUtils.SCardMonitor.CardInitialized -= CardInitialized;
            //APIUtils.SCardMonitor.CardInserted -= CardInserted;
            //APIUtils.SCardMonitor.CardRemoved -= CardRemoved;
            smartCard.Scanning();
        }









    }
}
