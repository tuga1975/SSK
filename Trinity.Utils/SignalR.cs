using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using Trinity.BE;
using Trinity.Common;
using Trinity.DAL;

namespace Trinity.Utils
{
    class SignalR
    {
        private IHubProxy HubProxy { get; set; }
        const string ServerURI = "http://localhost:8080/signalr";
        private HubConnection Connection { get; set; }

        public SignalR()
        {
            ConnectAsync();
        }
        private async void ConnectAsync()
        {
            Connection = new HubConnection(ServerURI);
            Connection.Closed += Connection_Closed;
            HubProxy = Connection.CreateHubProxy("MyHub");
            //Handle incoming event from server: use Invoke to write to console from SignalR's thread
            HubProxy.On("OnNewNotification", () => GetLatestNotifications());

            try
            {
                await Connection.Start();
            }
            catch
            {
                //No connection: Don't enable Send button or show chat UI
                return;
            }
        }
        private void Connection_Closed()
        {
            Connection.Start();

        }

        public void SendNotificationToDutyOfficer(string subject, string content)
        {
            Session session = Session.Instance;
            User user = (User)session[CommonConstants.SUPERVISEE];
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

        /// <summary>
        /// Send notification to duty officer
        /// </summary>
        /// <param name="subject">Subject</param>
        /// <param name="content">Content</param>
        /// <param name="notificationType">NotificationType : {Error : 'E', Notification : 'N', Caution : 'C'}</param>
        /// <param name="source">EnumStations : {SSA, SSK, UHP, ASP, HSA, ASP}</param>
        public void SendNotificationToDutyOfficer(string subject, string content, string notificationType, string source)
        {
            Session session = Session.Instance;
            User user = (User)session[CommonConstants.SUPERVISEE];
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
                HubProxy.Invoke("SendNotification", subject, content, user.UserId, null, true, notificationType);
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
                List<Notification> myNotifications = dalNotification.GetMyNotifications(user.UserId, false);
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
