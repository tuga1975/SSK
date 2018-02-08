using Microsoft.AspNet.SignalR.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Trinity.BE;
using Trinity.Common;
using Trinity.DAL;

namespace DutyOfficer.Utils
{
    class SignalR:Trinity.Utils.Notification.SignalRBase
    {
        public SignalR()
        {
            StartConnect();
        }
        public override void IncomingEvents()
        {
            HubProxy.On<int, EnumDeviceStatuses[],string>("DeviceStatusUpdate", (deviceId, deviceStatuses, Station) => {
                // Xử lý status device
            });

            HubProxy.On<string, string, string, string, string,string>("MessageTo", (NotificationID, UserId, Subject, Content, notificationType, Station) => {
                // Xử lý Message
                 Notification notification = new Notification();
                notification.Subject = Subject;
                notification.ToUserId = UserId;
                notification.Content = Content;
                notification.Source = Station;
                notification.Type = notificationType;
                updateAlertNotification(notification);
            });
        }

        public override void Connection_Closed()
        {
            
        }

        private void updateAlertNotification(Notification notification)
        {
            object result = JsonConvert.SerializeObject(notification, Formatting.Indented,
                new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            APIUtils.LayerWeb.InvokeScript("getRealtimeNotificationServer", result);
        }


    }
}
