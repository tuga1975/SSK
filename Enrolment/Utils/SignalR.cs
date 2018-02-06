using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using Trinity.BE;
using Trinity.Common;
using Trinity.DAL;

namespace Enrolment.Utils
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
    }
}
