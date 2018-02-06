using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using Trinity.BE;
using Trinity.Common;
using Trinity.DAL;

namespace SSA.Utils
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
        //public void SendNotificationToDutyOfficer(string subject, string content)
        //{
        //    Session session = Session.Instance;
        //    User user = (User)session[CommonConstants.SUPERVISEE];
        //    if (user == null)
        //    {
        //        // User hasn't authenticated yet
        //        return;
        //    }
        //    DAL_Notification dalNotification = new DAL_Notification();
        //    // Insert notification to local DB and also CentralizedDB
        //    dalNotification.InsertNotification(subject, content, user.UserId, null, true, true);
        //    //dalNotification.InsertNotification(subject, content, user.UserId, null, true, false);

        //    try
        //    {
        //        HubProxy.Invoke("SendNotification", subject, content, user.UserId, null, true);
        //    }
        //    catch (Exception)
        //    {
        //    }
        //}

        ///// <summary>
        ///// Send notification to duty officer
        ///// </summary>
        ///// <param name="subject">Subject</param>
        ///// <param name="content">Content</param>
        ///// <param name="notificationType">NotificationType : {Error, Notification, Caution}</param>
        ///// <param name="source">EnumStations : {SSA, SSK, UHP, ASP, HSA, ASP}</param>
        //public void SendNotificationToDutyOfficer(string subject, string content, string notificationType, string source)
        //{
        //    Session session = Session.Instance;
        //    User user = (User)session[CommonConstants.SUPERVISEE];
        //    if (user == null)
        //    {
        //        // User hasn't authenticated yet
        //        return;
        //    }
        //    DAL_Notification dalNotification = new DAL_Notification();
        //    // Insert notification to local DB and also CentralizedDB
        //    dalNotification.InsertNotification(subject, content, user.UserId, null, true, true, notificationType, source);
        //    //dalNotification.InsertNotification(subject, content, user.UserId, null, true, false);

        //    try
        //    {
        //        HubProxy.Invoke("SendNotification", subject, content, user.UserId, null, true, notificationType);
        //    }
        //    catch (Exception)
        //    {
        //    }
        //}

        //public void GetLatestNotifications()
        //{
        //    Trinity.DAL.DAL_Notification dalNotification = new Trinity.DAL.DAL_Notification();

        //    // Get notifications from local db
        //    // In this demo we get notifications from centralized db directly
        //    Session currentSession = Session.Instance;
        //    User user = (User)currentSession[CommonConstants.USER_LOGIN];
        //    if (user != null)
        //    {
        //        List<Notification> myNotifications = dalNotification.GetNotificationsByUserId(user.UserId);
        //        if (myNotifications != null)
        //        {
        //            var unReadCount = myNotifications.Count;
        //            APIUtils.LayerWeb.Invoke((System.Windows.Forms.MethodInvoker)(() =>
        //            {
        //                APIUtils.LayerWeb.PushNoti(unReadCount);
        //            }));
        //        }
        //    }
        //}
    }
}
