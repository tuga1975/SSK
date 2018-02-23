using Microsoft.AspNet.SignalR.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
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
                saveNotification(NotificationID, UserId, Subject, Content, notificationType, Station);
            });
        }

        public override void Connection_Closed()
        {
            
        }


        private void saveNotification(string NotificationID, string UserId, string Subject, string Content, string notificationType, string Station)
        {
            string dutiOfficerId = ((Trinity.BE.User)Session.Instance[CommonConstants.USER_LOGIN]).UserId;
            if(dutiOfficerId != null && dutiOfficerId != "")
            {
                DAL_Notification dAL_Notification = new DAL_Notification();
                bool isSuccessed = dAL_Notification.SendToDutyOfficer(NotificationID, UserId, dutiOfficerId, Subject, Content, notificationType, Station) > 0;
                if (isSuccessed)
                {
                    Notification notification = new Notification();
                    notification.Subject = Subject;
                    notification.ToUserId = UserId;
                    notification.Content = Content;
                    notification.Source = Station;
                    notification.Type = notificationType;
                    notification.Datetime = DateTime.Now;
                    object result = JsonConvert.SerializeObject(notification, Formatting.Indented,
                    new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                    Lib.LayerWeb.Invoke((MethodInvoker)(() =>
                    {
                        string activeTab = Lib.LayerWeb.Document.InvokeScript("getActiveTab").ToString();
                        if (activeTab != "Alerts")
                        {
                            Lib.LayerWeb.InvokeScript("getRealtimeNotificationServer", result);
                        }
                        else
                        {
                            Lib.LayerWeb.InvokeScript("getNotificationInCurrentTab", result);
                        }
                    }));
                    
                }
            }
        }

    }
}
