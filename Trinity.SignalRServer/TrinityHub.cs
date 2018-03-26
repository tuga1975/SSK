using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity.Common;

namespace Trinity.NotificationServer
{
    public class TrinityHub : Hub
    {
        private string Station
        {
            get
            {
                return Context.Headers["Station"];
            }
        }

        public void OnNewNotification(NotificationInfo notificationInfo)
        {
            if (notificationInfo.Name == NotificationNames.QUEUE_COMPLETED)
            {
                Program.MainForm.WriteToConsole("[" + Station + "] => User '" + notificationInfo.FromUserId + "' has been processed. Queue Number has been removed.");

                Clients.Clients(Program.ProfileConnected.Where(d => d.isApp && d.Station == EnumStation.SSK).Select(d => d.ConnectionId).ToList()).OnNewNotification(notificationInfo);
            }
            else if (notificationInfo.Name == NotificationNames.DEVICE_STATUS_CHANGED)
            {
                Clients.Clients(Program.ProfileConnected.Where(d => d.isApp && d.Station == EnumStation.DUTYOFFICER).Select(d => d.ConnectionId).ToList()).OnNewNotification(notificationInfo);
            }
            else if (notificationInfo.Name == NotificationNames.USER_LOGGED_IN)
            {
                string userId = notificationInfo.FromUserId;
                if (!Program.ProfileConnected.Any(d => d.isUser && d.Station == Station && d.UserID == userId && d.ConnectionId == Context.ConnectionId))
                {
                    Program.ProfileConnected.Add(new ProfileConnected()
                    {
                        Context = Context,
                        Station = Station,
                        UserID = userId
                    });
                    Program.MainForm.WriteToConsole("[" + Station + "] => User '" + userId + "' has logged in");
                }
            }
            else if (notificationInfo.Name == NotificationNames.USER_LOGGED_OUT)
            {
                string userId = notificationInfo.FromUserId;
                Program.ProfileConnected.RemoveWhere(d => d.isUser && d.UserID == userId && d.Station == Station);
                Program.MainForm.WriteToConsole("[" + Station + "] => User '" + userId + "' has logged out");
            }
            else if (notificationInfo.Name == NotificationNames.ALERT_MESSAGE)
            {
                string[] toUserIDs = notificationInfo.ToUserIds;
                if (toUserIDs != null && toUserIDs.Length > 0)
                {
                    string fromUserId = notificationInfo.FromUserId;

                    Program.MainForm.WriteToConsole("[" + Station + "] => User '" + fromUserId + "' send notification to:" + string.Join(",", toUserIDs));
                    for (int i = 0; i < toUserIDs.Length; i++)
                    {
                        Clients.Clients(Program.ProfileConnected.Where(d => d.isUser && d.UserID == toUserIDs[i]).Select(d => d.ConnectionId).ToList()).OnNewNotification(notificationInfo);
                    }
                }
                else if(notificationInfo.ToUserIds==null)
                {
                    Clients.Clients(Program.ProfileConnected.Where(d => d.isApp && d.Station == EnumStation.DUTYOFFICER).Select(d => d.ConnectionId).ToList()).OnNewNotification(notificationInfo);
                }
            }
            else if (notificationInfo.Name == NotificationNames.SSP_COMPLETED || notificationInfo.Name == NotificationNames.SHP_COMPLETED)
            {
                Clients.Clients(Program.ProfileConnected.Where(d => d.isApp && d.Station == EnumStation.DUTYOFFICER).Select(d => d.ConnectionId).ToList()).OnNewNotification(notificationInfo);
            }
            else if (notificationInfo.Name == NotificationNames.DO_UNBLOCK_SUPERVISEE)
            {
                Clients.Clients(Program.ProfileConnected.Where(d => d.isApp && d.Station == EnumStation.SSK).Select(d => d.ConnectionId).ToList()).OnNewNotification(notificationInfo);
            }
            else if (notificationInfo.Name == NotificationNames.APPOINTMENT_BOOKED)
            {
                Clients.Clients(Program.ProfileConnected.Where(d => d.isApp && (d.Station == EnumStation.DUTYOFFICER || d.Station == EnumStation.SSK)).Select(d => d.ConnectionId).ToList()).OnNewNotification(notificationInfo);
            }
            else if (notificationInfo.Name == NotificationNames.APPOINTMENT_REPORTED)
            {
                Clients.Clients(Program.ProfileConnected.Where(d => d.isApp && d.Station == EnumStation.DUTYOFFICER).Select(d => d.ConnectionId).ToList()).OnNewNotification(notificationInfo);
            }
            else if (notificationInfo.Name == NotificationNames.QUEUE_INSERTED)
            {
                Clients.Clients(Program.ProfileConnected.Where(d => d.isApp && d.Station == EnumStation.DUTYOFFICER).Select(d => d.ConnectionId).ToList()).OnNewNotification(notificationInfo);
            }
            else if (notificationInfo.Name == NotificationNames.SSA_COMPLETED)
            {
                Clients.Clients(Program.ProfileConnected.Where(d => d.isApp && d.Station == EnumStation.DUTYOFFICER).Select(d => d.ConnectionId).ToList()).OnNewNotification(notificationInfo);
            }
            else if (notificationInfo.Name == NotificationNames.SSA_INSERTED_LABEL)
            {
                Clients.Clients(Program.ProfileConnected.Where(d => d.isApp && d.Station == EnumStation.DUTYOFFICER).Select(d => d.ConnectionId).ToList()).OnNewNotification(notificationInfo);
            }
            else if (notificationInfo.Name == NotificationNames.BACKEND_API_SEND_DO)
            {
                Clients.Clients(Program.ProfileConnected.Where(d => d.isApp && d.Station == EnumStation.DUTYOFFICER).Select(d => d.ConnectionId).ToList()).OnNewNotification(notificationInfo);
            }
        }

        #region Connection functions
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
            foreach (var item in Program.ProfileConnected.Where(d => d.ConnectionId == Context.ConnectionId))
            {
                item.DateOffline = DateTime.Now;
                if (item.isApp)
                {
                    Program.MainForm.WriteToConsole("[" + item.Station + "] => App Disconnected");
                    foreach (var itemConnect in Program.ProfileConnected.Where(d => d.isUser && d.Station == EnumStation.DUTYOFFICER))
                    {
                        NotificationInfo notificationInfo = new NotificationInfo()
                        {
                            Name = NotificationNames.APP_DISCONNECTED,
                            Source = Station
                        };
                        Clients.Client(itemConnect.ConnectionId).OnNewNotification(notificationInfo);
                        //Clients.Client(itemConnect.ConnectionId).AppDisconnect(Station);
                    }
                }
                else
                {
                    Program.MainForm.WriteToConsole("[" + item.Station + "] => User Disconnected " + item.UserID);
                }
            }

            Program.ProfileConnected.RemoveWhere(d => d.isOffline);

            return base.OnDisconnected(stopCalled);
        }

        #endregion
    }
}
