using SSK.DeviceMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SSK.DriverScan
{
    public class SmartCard
    {
        private WebBrowser web = null;
        private FingerPrint fingerPrint = null;
        private int CountScan = 0;
        private Trinity.DAL.DBContext.TrinityCentralizedDBEntities sSKCentralizedEntities = null;
        public SmartCard(WebBrowser web)
        {
            this.web = web;
            sSKCentralizedEntities = new Trinity.DAL.DBContext.TrinityCentralizedDBEntities();
            this.fingerPrint = new FingerPrint(this, web);
        }

        public void Scanning()
        {
            Thread thread = new Thread(ThreadScan);
            thread.IsBackground = true;
            thread.Start();
        }

        private void ThreadScan()
        {


            #region Đóng lại nếu ko có Smart Card Driver
            // add event Smart Cart
            //APIUtils.SCardMonitor.CardInitialized += CardInitialized;
            //APIUtils.SCardMonitor.CardInserted += CardInserted;
            //APIUtils.SCardMonitor.CardRemoved += CardRemoved;
            #endregion

            web.LoadPageHtml("Authentication/SmartCard.html");
            web.RunScript("$('.status-text').css('color','#000').text('Please place your smart card on the reader.');");

            #region Mở ra nếu ko có Smart Card Driver
            Thread.Sleep(2000);
            fingerPrint.Scanning();
            #endregion 
        }
        private void CardInitialized(string cardInfo)
        {
            CheckLoginUser(cardInfo);

        }
        private void CardInserted(string cardInfo)
        {
            CheckLoginUser(cardInfo);
        }
        
        private void CardRemoved()
        {
            web.RunScript("$('.status-text').css('color','#000').text('Please place your smart card on the reader.');");
        }
        private void CheckLoginUser(string cardInfo)
        {
            CountScan++;
            if (!string.IsNullOrEmpty(cardInfo))
            {
                var user = sSKCentralizedEntities.Users.Where(d => d.UserId == cardInfo).FirstOrDefault();
                if (user != null)
                {
                    APIUtils.SCardMonitor.CardInitialized -= CardInitialized;
                    APIUtils.SCardMonitor.CardInserted -= CardInserted;
                    APIUtils.SCardMonitor.CardRemoved -= CardRemoved;
                    web.RunScript("$('.status-text').css('color','blue').text('Authentication Successful');");
                    Thread.Sleep(1000);
                    fingerPrint.Scanning();
                }else if (CountScan >= 3)
                {
                    CreateNoti(cardInfo);
                }
                else
                {
                    web.RunScript("$('.status-text').text('Smart Card Scanning Failure. Please try again.');");
                }
            }
            else if (string.IsNullOrEmpty(cardInfo) && CountScan >= 3)
            {
                CreateNoti(cardInfo);
            }
            else if (string.IsNullOrEmpty(cardInfo))
            {
                web.RunScript("$('.status-text').text('Smart Card Scanning Failure. Please try again.');");
            }
        }
        private void CreateNoti(string cardInfo)
        {
            sSKCentralizedEntities.Notifications.Add(new Trinity.DAL.DBContext.Notification() {
                Date = DateTime.Now,
                Content= "Authentication Smart Card Failure",
                Subject = "Authentication Smart Card Failure",
                ToUserId = cardInfo,
                FromUserId = cardInfo
            });
            sSKCentralizedEntities.SaveChanges();
            APIUtils.SignalR.AddNotification("Authentication Smart Card Failure", "Authentication Smart Card Failure");
            MessageBox.Show("Unable to read your smart card. Please report to the Duty Officer");
            CountScan = 0;
            web.RunScript("$('.status-text').css('color','#000').text('Please place your smart card on the reader.');");
        }
    }
}
