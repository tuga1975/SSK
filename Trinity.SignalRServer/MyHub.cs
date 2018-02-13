using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalRChat
{
    public class MyHub : Hub
    {
        private string Station
        {
            get
            {
                return Context.Headers["Station"];
            }
        }

        public void QueueFinished(Trinity.BE.Queue queue)
        {
            Clients.Clients(Program.ProfileConnected.Where(d => d.isApp && d.Station == EnumStations.SSK).Select(d => d.ConnectionId).ToList()).QueueFinished(queue);
        }
        public string SendToDutyOfficer(string UserId, string DutyOfficerID, string Subject, string Content, string notificationType)
        {
            if (EnumAppConfig.ByPassCentralizedDB)
            {
                string MessageID = Guid.NewGuid().ToString();
                Clients.Clients(Program.ProfileConnected.Where(d => d.isUser && d.UserID == DutyOfficerID && d.Station == EnumStations.DUTYOFFICER).Select(d => d.ConnectionId).ToList()).MessageTo(MessageID, UserId, Subject, Content, notificationType, Station);
                return MessageID;
            }
            else
            {
                bool statusCentralized;
                string MessageID = CallCentralized.Post<string>("Notification", "SendToDutyOfficer", out statusCentralized, "UserId=" + UserId, "DutyOfficerID=" + DutyOfficerID, "Subject=" + Subject, "Content=" + Content, "Type=" + notificationType, "Station=" + Station);
                if (statusCentralized)
                {
                    List<string> connectId = Program.ProfileConnected.Where(d => d.isUser && d.UserID == DutyOfficerID && d.Station == EnumStations.DUTYOFFICER).Select(d => d.ConnectionId).ToList();
                    if (connectId.Count > 0)
                    {
                        Clients.Clients(connectId).MessageTo(MessageID, UserId, Subject, Content, notificationType, Station);
                    }
                }
                return MessageID;
            }
        }
        public List<Trinity.BE.Notification> SendAllDutyOfficer(string UserId, string Subject, string Content, string notificationType)
        {
            if (EnumAppConfig.ByPassCentralizedDB)
            {
                foreach (var item in Program.ProfileConnected.Where(d => d.isUser && d.Station == EnumStations.DUTYOFFICER))
                {
                    Clients.Client(item.ConnectionId).MessageTo(Guid.NewGuid().ToString().Trim(), UserId, Subject, Content, notificationType, Station);
                }
                return new List<Trinity.BE.Notification>();
            }
            else
            {
                bool statusCentralized;
                List<Trinity.BE.Notification> arraySend = CallCentralized.Post<List<Trinity.BE.Notification>>("Notification", "SendAllDutyOfficer", out statusCentralized, "UserId=" + UserId, "Subject=" + Subject, "Content=" + Content, "Type=" + notificationType, "Station=" + Station);
                if (statusCentralized)
                {
                    foreach (var item in Program.ProfileConnected.Where(d => d.isUser && d.Station == EnumStations.DUTYOFFICER))
                    {
                        Trinity.BE.Notification dataNoti = arraySend.FirstOrDefault(d => d.ToUserId == item.UserID);
                        if (dataNoti != null)
                        {
                            Clients.Client(item.ConnectionId).MessageTo(dataNoti.NotificationID, UserId, Subject, Content, notificationType, Station);
                        }
                    }
                }
                return arraySend;
            }
        }

        //public void SendNotification(string subject, string content, string fromUserId, string toUserId, bool isFromSupervisee)
        //{
        //    // Insert notification into centralized DB first
        //    Trinity.DAL.DAL_Notification dalNotification = new Trinity.DAL.DAL_Notification();
        //    dalNotification.InsertNotification(subject, content, fromUserId, toUserId, isFromSupervisee, false);

        //    // And notify all client about new notification
        //    // In later phase we should send to appropriate client
        //    Clients.All.OnNewNotification(subject, content, fromUserId, toUserId, isFromSupervisee);

        //    Program.MainForm.WriteToConsole("User:" + fromUserId + "' send notification|subject:" + subject + "|" + "|content:" + content + "|to user:" + toUserId);
        //}

        public void DeviceStatusUpdate(int deviceId, EnumDeviceStatuses[] deviceStatuses)
        {
            CallCentralized.Post<bool>("DeviceStatus", "Update", new object[] { deviceId, deviceStatuses, Station });
            Clients.Clients(Program.ProfileConnected.Where(d => d.isApp && d.Station == EnumStations.DUTYOFFICER).Select(d => d.ConnectionId).ToList()).DeviceStatusUpdate(deviceId, deviceStatuses, Station);
        }

        public void UserLogined(string UserId)
        {
            if (!Program.ProfileConnected.Any(d => d.isUser && d.Station == Station && d.UserID == UserId && d.ConnectionId == Context.ConnectionId))
            {
                Program.ProfileConnected.Add(new ProfileConnected()
                {
                    Context = Context,
                    Station = Station,
                    UserID = UserId
                });
                Program.MainForm.WriteToConsole("[" + Station + "] => UserLogined " + UserId);
            }
        }
        public void UserLogout(string UserId)
        {
            Program.ProfileConnected.RemoveWhere(d => d.isUser && d.UserID == UserId && d.Station == Station);
            Program.MainForm.WriteToConsole("[" + Station + "] => UserLogout " + UserId);
        }
        public override Task OnConnected()
        {

            if (!Program.ProfileConnected.Any(d => d.isApp && d.Station == Station && d.ConnectionId == Context.ConnectionId))
            {
                Program.ProfileConnected.Add(new ProfileConnected()
                {
                    Context = Context,
                    Station = Station
                });
                Program.MainForm.WriteToConsole("[" + Station + "] => App Connect");
            }
            foreach (var item in Program.ProfileConnected.Where(d => d.isOffline && d.ConnectionId == Context.ConnectionId))
            {
                item.DateOffline = null;
            }
            return base.OnConnected();
        }
        public override Task OnReconnected()
        {
            if (!Program.ProfileConnected.Any(d => d.isApp && d.Station == Station && d.ConnectionId == Context.ConnectionId))
            {
                Program.ProfileConnected.Add(new ProfileConnected()
                {
                    Context = Context,
                    Station = Station
                });
                Program.MainForm.WriteToConsole("[" + Station + "] => App Connect");
            }
            else
            {
                Program.MainForm.WriteToConsole("[" + Station + "] => App Reconnected");
            }
            return base.OnReconnected();
        }
        public override Task OnDisconnected(bool stopCalled)
        {

            Program.ProfileConnected.RemoveWhere(d => d.isOffline && d.HoursOffline >= 5);
            foreach (var item in Program.ProfileConnected.Where(d => d.ConnectionId == Context.ConnectionId))
            {
                item.DateOffline = DateTime.Now;
                if (item.isApp)
                {
                    Program.MainForm.WriteToConsole("[" + item.Station + "] => App Disconnected");
                }
                else
                {
                    Program.MainForm.WriteToConsole("[" + item.Station + "] => User Disconnected " + item.UserID);
                }
            }
            return base.OnDisconnected(stopCalled);
        }
    }
}
