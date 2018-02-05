using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using Trinity.BE;
using Trinity.Common;
using Trinity.DAL;

namespace SSK.Utils
{
    class SignalR : Trinity.Utils.Notification.SignalRBase
    {
        public SignalR()
        {
            StartConnect();
        }
        public override void IncomingEvents()
        {

        }
        public override void Connection_Closed()
        {

        }

        public void SendNotificationToDutyOfficer(string subject, string content)
        {
            Session session = Session.Instance;
            User user = (User)session[CommonConstants.USER_LOGIN];
            if (user == null)
            {
                // User hasn't authenticated yet
                return;
            }
            DAL_Notification dalNotification = new DAL_Notification();
            // Insert notification to local DB and also CentralizedDB
            dalNotification.InsertNotification(subject, content, user.UserId, null, true, true);
            //dalNotification.InsertNotification(subject, content, user.UserId, null, true, false);

            try
            {
                HubProxy.Invoke("SendNotification", subject, content, user.UserId, null, true);
            }
            catch (Exception)
            {
            }
        }

        public void SendNotificationToDutyOfficer(string subject, string content, string notificationType, string source)
        {
            Session session = Session.Instance;
            User user = (User)session[CommonConstants.USER_LOGIN];
            if (user == null)
            {
                // User hasn't authenticated yet
                return;
            }
            DAL_Notification dalNotification = new DAL_Notification();
            // Insert notification to local DB and also CentralizedDB
            dalNotification.InsertNotification(subject, content, user.UserId, null, true, true, notificationType, source);
            //dalNotification.InsertNotification(subject, content, user.UserId, null, true, false);

            try
            {
                HubProxy.Invoke("SendNotification", subject, content, user.UserId, null, true);
            }
            catch (Exception)
            {
            }
        }

        public void GetLatestNotifications()
        {
            Trinity.DAL.DAL_Notification dalNotification = new Trinity.DAL.DAL_Notification();

            // Get notifications from local db
            // In this demo we get notifications from centralized db directly
            Session currentSession = Session.Instance;
            User user = (User)currentSession[CommonConstants.USER_LOGIN];
            if (user != null)
            {
                List<Notification> myNotifications = dalNotification.GetMyNotifications(user.UserId).Data;
                if (myNotifications != null)
                {
                    var unReadCount = myNotifications.Count;
                    APIUtils.LayerWeb.Invoke((System.Windows.Forms.MethodInvoker)(() =>
                    {
                        APIUtils.LayerWeb.PushNoti(unReadCount);
                    }));
                }
            }
        }
    }
}
