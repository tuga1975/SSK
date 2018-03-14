using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Trinity.Common;

namespace Trinity.SignalR
{
    public class Client
    {
        #region Singleton Implementation
        private static volatile Client _instance;
        private static object syncRoot = new Object();
        private IHubProxy HubProxy { get; set; }
        private HubConnection Connection { get; set; }
        private Client()
        {
            ConnectAsync();
            //Lib.SignalR = this;
        }
        public static Client Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (_instance == null)
                            _instance = new Client();
                    }
                }
                return _instance;
            }
        }
        #endregion

        private string Station
        {
            get
            {
                string _Station = ConfigurationManager.AppSettings["Station"];
                if (string.IsNullOrEmpty(_Station))
                    _Station = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;

                return _Station;
            }
        }
        private bool IsConnected
        {
            get
            {
                if (Connection != null && Connection.State == ConnectionState.Connected)
                {
                    return true;
                }
                return false;
            }
        }
        private async Task WaitConnectFalse()
        {
            if (Connection == null)
                ConnectSignalr();
            while (!IsConnected)
            {
                await Task.Delay(1000);
            }
        }
        private void ConnectAsync()
        {
            Connection = new HubConnection(EnumAppConfig.NotificationServerUrl + "/signalr");
            Connection.Headers.Add("Station", Station);
            Connection.Closed += ConnectionClosed_Handler;
            HubProxy = Connection.CreateHubProxy("TrinityHub");
            IncomingEvents();
            ConnectSignalr();
        }
        private void ConnectionClosed_Handler()
        {
            if (OnDisconnected != null)
            {
                OnDisconnected(this, new EventArgs());
            }

            // Reconnect to SignalR Server
            ConnectSignalr();
        }

        private async void ConnectSignalr()
        {
            try
            {
                await Connection.Start();
            }
            catch
            {
            }
        }
        #region event
        public event EventHandler<NotificationInfo> OnNewNotification;
        public event EventHandler<EventArgs> OnDisconnected;
        public event EventHandler<EventInfo> OnQueueCompleted;
        public event EventHandler<EventInfo> OnDeviceStatusChanged;
        public event EventHandler<EventInfo> OnAppDisconnected;
        public event EventHandler<NotificationInfo> OnSSPCompleted;
        public event EventHandler<NotificationInfo> OnDOUnblockSupervisee;
        public event EventHandler<NotificationInfo> OnAppointmentBookedOrReported;
        public event EventHandler<NotificationInfo> OnQueueInserted;
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="queue"></param>
        //public event Action<Trinity.BE.Queue> Event_QueueFinished;
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="deviceId"></param>
        ///// <param name="deviceStatuses"></param>
        ///// <param name="Station"></param>
        //public event Action<int, EnumDeviceStatuses[], string> Event_DeviceStatusUpdate;
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="NotificationID"></param>
        ///// <param name="UserId"></param>
        ///// <param name="Subject"></param>
        ///// <param name="Content"></param>
        ///// <param name="notificationType"></param>
        ///// <param name="Station"></param>
        //public event Action<string, string, string, string, string, string> Event_MessageTo;
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="Station"></param>
        //public event Action<string> Event_AppDisconnect;

        private void IncomingEvents()
        {
            HubProxy.On<NotificationInfo>("OnNewNotification", (notificationInfo) =>
            {
                if (notificationInfo.Name == NotificationNames.QUEUE_COMPLETED)
                {
                    OnQueueCompleted?.Invoke(this, new EventInfo() { Name = EventNames.QUEUE_COMPLETED, Data = notificationInfo.FromUserId });
                }
                else if (notificationInfo.Name == NotificationNames.DEVICE_STATUS_CHANGED)
                {
                    OnDeviceStatusChanged?.Invoke(this, new EventInfo() { Name = EventNames.DEVICE_STATUS_CHANGED, Data = notificationInfo.Data });
                }
                else if (notificationInfo.Name == NotificationNames.APP_DISCONNECTED)
                {
                    OnAppDisconnected?.Invoke(this, new EventInfo() { Name = EventNames.APP_DISCONNECTED, Source = notificationInfo.Source, Data = notificationInfo.Data });
                }
                else if (notificationInfo.Name == NotificationNames.SSP_COMPLETED)
                {
                    OnSSPCompleted?.Invoke(this, notificationInfo);
                }
                else if (notificationInfo.Name == NotificationNames.DO_UNBLOCK_SUPERVISEE)
                {
                    OnDOUnblockSupervisee?.Invoke(this, notificationInfo);
                }
                else if (notificationInfo.Name == NotificationNames.APPOINTMENT_BOOKED_OR_REPORTED)
                {
                    OnAppointmentBookedOrReported?.Invoke(this, notificationInfo);
                }
                else if (notificationInfo.Name == NotificationNames.QUEUE_INSERTED)
                {
                    OnQueueInserted?.Invoke(this, notificationInfo);
                }
                else
                {
                    OnNewNotification?.Invoke(this, notificationInfo);
                }
            });
            //HubProxy.On<Trinity.BE.Queue>("QueueFinished", (queue) =>
            //{
            //    if (Event_QueueFinished != null)
            //        Event_QueueFinished(queue);
            //});
            //HubProxy.On<int, EnumDeviceStatuses[], string>("DeviceStatusUpdate", (deviceId, deviceStatuses, Station) =>
            //{
            //    if (Event_DeviceStatusUpdate != null)
            //        Event_DeviceStatusUpdate(deviceId, deviceStatuses, Station);

            //});
            //HubProxy.On<string, string, string, string, string, string>("MessageTo", (NotificationID, UserId, Subject, Content, notificationType, Station) =>
            //{
            //    if (Event_MessageTo != null)
            //        Event_MessageTo(NotificationID, UserId, Subject, Content, notificationType, Station);
            //});
            //HubProxy.On<string>("AppDisconnect", (Station) =>
            //{
            //    if (Event_AppDisconnect != null)
            //        Event_AppDisconnect(Station);
            //});
        }

        #endregion

        #region Public functions
        public async void PostNotification(NotificationInfo notificationInfo)
        {
            await WaitConnectFalse();
            await HubProxy.Invoke("OnNewNotification", notificationInfo);
        }

        public void UserLoggedIn(string userId)
        {
            PostNotification(notificationInfo: new NotificationInfo() { Name = NotificationNames.USER_LOGGED_IN, FromUserId = userId, Type = EnumNotificationTypes.Notification });
            //await WaitConnectFalse();
            //if (IsConnected) { }
            //await HubProxy.Invoke("UserLogined", userId);
        }
        public void UserLoggedOut(string userId)
        {
            PostNotification(notificationInfo: new NotificationInfo() { Name = NotificationNames.USER_LOGGED_OUT, FromUserId = userId, Type = EnumNotificationTypes.Notification });
            //await WaitConnectFalse();
            //await HubProxy.Invoke("UserLogout", userID);
        }

        public void DeviceStatusChanged(int deviceId, EnumDeviceStatus[] newDeviceStatuses)
        {
            PostNotification(notificationInfo: new NotificationInfo() { Name = NotificationNames.DEVICE_STATUS_CHANGED, Type = EnumNotificationTypes.Notification, Data = new object[] { deviceId, newDeviceStatuses } });

            //await WaitConnectFalse();
            //try
            //{
            //    await HubProxy.Invoke("DeviceStatusUpdate", deviceId, deviceStatuses);
            //}
            //catch (Exception)
            //{


            //}
        }
        public void SendToDutyOfficer(string fromUserId, string dutyOfficerID, string subject, string content, string notificationType)
        {
            int result = new DAL.DAL_Notification().SendToDutyOfficer(fromUserId, dutyOfficerID, subject, content, notificationType, Station);
            if (result > 0)
            {
                NotificationInfo notificationInfo = new NotificationInfo()
                {
                    Name = NotificationNames.ALERT_MESSAGE,
                    FromUserId = fromUserId,
                    ToUserIds = new string[] { dutyOfficerID },
                    Content = content,
                    Subject = subject,
                    Type = notificationType,
                    Source = Station
                };
                PostNotification(notificationInfo);
            }
            //await WaitConnectFalse();
            //string IDMessage = Guid.NewGuid().ToString().Trim();
            //if (new DAL.DAL_Notification().SendToDutyOfficer(IDMessage, fromUserId, dutyOfficerID, subject, content, notificationType, Station) > 0)
            //{
            //    bool status = await HubProxy.Invoke<bool>("SendToDutyOfficer", IDMessage, fromUserId, dutyOfficerID, subject, content, notificationType);
            //}
        }
        public void SendToAllDutyOfficers(string fromUserId, string subject, string content, string notificationType)
        {
            List<BE.Notification> notificationList = new DAL.DAL_Notification().SendToAllDutyOfficers(fromUserId, subject, content, notificationType, Station);
            if (notificationList != null && notificationList.Count > 0)
            {
                string[] dutyOfficers = notificationList.Select(n => n.ToUserId).ToArray();
                NotificationInfo notificationInfo = new NotificationInfo()
                {
                    Name = NotificationNames.ALERT_MESSAGE,
                    FromUserId = fromUserId,
                    ToUserIds = dutyOfficers,
                    Content = content,
                    Subject = subject,
                    Type = notificationType,
                    Source = Station
                };
                PostNotification(notificationInfo);
            }

            //await WaitConnectFalse();
            //Dictionary<string, string> arrraySend = new DAL.DAL_Notification().SendToAllDutyOfficers(fromUserId, subject, content, notificationType, Station).ToDictionary(f => f.ToUserId, f => f.NotificationID);
            //bool status = await HubProxy.Invoke<bool>("SendToAllDutyOfficers", arrraySend, fromUserId, subject, content, notificationType);
        }

        /// <summary>
        /// Case Officer use this API to send notifications to supervisee
        /// </summary>
        /// <param name="fromUserId"></param>
        /// <param name="subject"></param>
        /// <param name="content"></param>
        /// <param name="notificationType"></param>
        public void SendToSupervisee(string fromUserId, string toUserId, string subject, string content, string notificationType)
        {
            int result = new DAL.DAL_Notification().SendToSupervisee(fromUserId, toUserId, subject, content, notificationType, Station);
            if (result > 0)
            {
                NotificationInfo notificationInfo = new NotificationInfo()
                {
                    Name = NotificationNames.ALERT_MESSAGE,
                    FromUserId = fromUserId,
                    ToUserIds = new string[] { toUserId },
                    Content = content,
                    Subject = subject,
                    Type = notificationType,
                    Source = Station
                };
                PostNotification(notificationInfo);
            }
        }

        public void QueueCompleted(string userId)
        {
            PostNotification(notificationInfo: new NotificationInfo() { Name = NotificationNames.QUEUE_COMPLETED, Type = EnumNotificationTypes.Notification, FromUserId = userId });
            //await WaitConnectFalse();
            //Trinity.BE.Queue queue = new DAL.DAL_QueueNumber().GetMyQueueToday(UserId);
            //await HubProxy.Invoke("QueueFinished", queue);
        }

        public void SSPCompleted(string NRIC)
        {
            PostNotification(notificationInfo: new NotificationInfo() { Name = NotificationNames.SSP_COMPLETED, NRIC = NRIC });
        }

        public void DOUnblockSupervisee(string UserId)
        {
            PostNotification(notificationInfo: new NotificationInfo() { Name = NotificationNames.DO_UNBLOCK_SUPERVISEE, UserID=UserId });
        }
        public void AppointmentBookedOrReported(string AppointmentID,string Status)
        {
            PostNotification(notificationInfo: new NotificationInfo() { Name = NotificationNames.APPOINTMENT_BOOKED_OR_REPORTED, AppointmentID = AppointmentID, Status = Status });
        }
        public void QueueInserted(string QueueID)
        {
            PostNotification(notificationInfo: new NotificationInfo() { Name = NotificationNames.QUEUE_INSERTED, QueueID = QueueID });
        }
        #endregion
    }
}
