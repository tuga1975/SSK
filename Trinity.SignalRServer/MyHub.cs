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
        public void SendNotification(string subject, string content, string fromUserId, string toUserId, bool isFromSupervisee)
        {
            // Insert notification into centralized DB first
            Trinity.DAL.DAL_Notification dalNotification = new Trinity.DAL.DAL_Notification();
            dalNotification.InsertNotification(subject, content, fromUserId, toUserId, isFromSupervisee, false);

            // And notify all client about new notification
            // In later phase we should send to appropriate client
            Clients.All.OnNewNotification(subject, content, fromUserId, toUserId, isFromSupervisee);

            Program.MainForm.WriteToConsole("User:" + fromUserId + "' send notification|subject:" + subject + "|" + "|content:" + content + "|to user:" + toUserId);
        }

        public override Task OnConnected()
        {
            Program.MainForm.WriteToConsole("Client connected: " + Context.ConnectionId);
            return base.OnConnected();
        }
        public override Task OnDisconnected()
        {
            Program.MainForm.WriteToConsole("Client disconnected: " + Context.ConnectionId);
            return base.OnDisconnected();
        }
    }
}
