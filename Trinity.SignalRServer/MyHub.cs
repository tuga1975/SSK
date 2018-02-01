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


        public void UserLogined(string UserId)
        {
            if (!Program.ProfileConnected.Any(d => d.isUser && d.Station == Context.Headers["Station"] && d.UserID== UserId && d.ConnectionId==Context.ConnectionId))
            {
                Program.ProfileConnected.Add(new ProfileConnected()
                {
                    Context = Context,
                    Station = Context.Headers["Station"],
                    UserID = UserId
                });
                Program.MainForm.WriteToConsole("["+ Context.Headers["Station"] + "] => UserLogined " + UserId);
            }
        }
        public void UserLogout(string UserId)
        {
            Program.ProfileConnected.RemoveWhere(d => d.isUser && d.UserID==UserId && d.Station== Context.Headers["Station"]);
            Program.MainForm.WriteToConsole("[" + Context.Headers["Station"] + "] => UserLogout " + UserId);
        }
        public override Task OnConnected()
        {

            if(!Program.ProfileConnected.Any(d => d.isApp && d.Station == Context.Headers["Station"] && d.ConnectionId==Context.ConnectionId))
            {
                Program.ProfileConnected.Add(new ProfileConnected() {
                    Context = Context,
                    Station = Context.Headers["Station"]
                });
                Program.MainForm.WriteToConsole("[" + Context.Headers["Station"] + "] => App Connect");
            }
            foreach (var item in Program.ProfileConnected.Where(d => d.isOffline && d.ConnectionId == Context.ConnectionId))
            {
                item.DateOffline = null;
            }
            return base.OnConnected();
        }
        public override Task OnReconnected()
        {
            if (!Program.ProfileConnected.Any(d => d.isApp && d.Station == Context.Headers["Station"] && d.ConnectionId == Context.ConnectionId))
            {
                Program.ProfileConnected.Add(new ProfileConnected()
                {
                    Context = Context,
                    Station = Context.Headers["Station"]
                });
                Program.MainForm.WriteToConsole("[" + Context.Headers["Station"] + "] => App Connect");
            }
            else
            {
                Program.MainForm.WriteToConsole("[" + Context.Headers["Station"] + "] => App Reconnected");
            }
            return base.OnReconnected();
        }
        public override Task OnDisconnected(bool stopCalled)
        {

            Program.ProfileConnected.RemoveWhere(d => d.isOffline && d.HoursOffline>=5);
            foreach (var item in Program.ProfileConnected.Where(d => d.ConnectionId == Context.ConnectionId))
            {
                item.DateOffline = DateTime.Now;
                if (item.isApp)
                {
                    Program.MainForm.WriteToConsole("[" + item.Station + "] => App Disconnected");
                }
                else
                {
                    Program.MainForm.WriteToConsole("[" + item.Station + "] => User Disconnected "+item.UserID);
                }
            }
            return base.OnDisconnected(stopCalled);
        }
    }
}
