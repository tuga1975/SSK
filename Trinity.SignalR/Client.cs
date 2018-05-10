using Microsoft.AspNet.SignalR.Client;
using System;
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
                return Lib.Station;
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
            RegisterIncomingEvents();
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
        public event EventHandler<NotificationInfo> OnDOUnblockSupervisee;
        public event EventHandler<NotificationInfo> OnQueueInserted;
        public event EventHandler<NotificationInfo> OnSSACompleted;
        public event EventHandler<NotificationInfo> OnSSAPrintingLabel;
        public event EventHandler<NotificationInfo> OnBackendAPISend;
        public event EventHandler<NotificationInfo> OnAppointmentBooked;
        public event EventHandler<NotificationInfo> OnAppointmentReported;

        private void RegisterIncomingEvents()
        {
            HubProxy.On<NotificationInfo>("OnNewNotification", (notificationInfo) =>
            {
                if (notificationInfo.Name == NotificationNames.QUEUE_COMPLETED)
                {
                    OnQueueCompleted?.Invoke(this, new EventInfo() { Name = EventNames.QUEUE_COMPLETED, Data = notificationInfo.FromUserId });
                }
                else if (notificationInfo.Name == NotificationNames.DEVICE_STATUS_CHANGED)
                {
                    OnDeviceStatusChanged?.Invoke(this, new EventInfo() { Name = EventNames.DEVICE_STATUS_CHANGED, Data = notificationInfo.Data,Source = notificationInfo.Source });
                }
                else if (notificationInfo.Name == NotificationNames.APP_DISCONNECTED)
                {
                    OnAppDisconnected?.Invoke(this, new EventInfo() { Name = EventNames.APP_DISCONNECTED, Source = notificationInfo.Source, Data = notificationInfo.Data });
                }
                else if (notificationInfo.Name == NotificationNames.DO_UNBLOCK_SUPERVISEE)
                {
                    OnDOUnblockSupervisee?.Invoke(this, notificationInfo);
                }
                else if (notificationInfo.Name == NotificationNames.APPOINTMENT_BOOKED)
                {
                    OnAppointmentBooked?.Invoke(this, notificationInfo);
                }
                else if (notificationInfo.Name == NotificationNames.APPOINTMENT_REPORTED)
                {
                    OnAppointmentReported?.Invoke(this, notificationInfo);
                }
                else if (notificationInfo.Name == NotificationNames.QUEUE_INSERTED)
                {
                    OnQueueInserted?.Invoke(this, notificationInfo);
                }
                else if (notificationInfo.Name == NotificationNames.SSA_COMPLETED)
                {
                    OnSSACompleted?.Invoke(this, notificationInfo);
                }
                else if (notificationInfo.Name == NotificationNames.SSA_PRINTING_LABEL)
                {
                    OnSSAPrintingLabel?.Invoke(this, notificationInfo);
                }
                else if (notificationInfo.Name == NotificationNames.SHP_COMPLETED || notificationInfo.Name == NotificationNames.SSP_COMPLETED || notificationInfo.Name == NotificationNames.SSP_ERROR)
                {
                    OnBackendAPISend?.Invoke(this, notificationInfo);
                }
                else
                {
                    OnNewNotification?.Invoke(this, notificationInfo);
                }
            });
        }

        #endregion

        #region Public functions
        public async void PostNotification(NotificationInfo notificationInfo)
        {
            await WaitConnectFalse();
            if (notificationInfo != null)
                notificationInfo.Source = notificationInfo.Source == null ? Station : notificationInfo.Source;

            notificationInfo.dateSend = DateTime.Now;
            await HubProxy.Invoke("OnNewNotification", notificationInfo);
        }

        public void UserLoggedIn(string userId)
        {
            PostNotification(notificationInfo: new NotificationInfo() { Name = NotificationNames.USER_LOGGED_IN, FromUserId = userId, Type = EnumNotificationTypes.Notification });
        }

        public void UserLoggedOut(string userId)
        {
            PostNotification(notificationInfo: new NotificationInfo() { Name = NotificationNames.USER_LOGGED_OUT, FromUserId = userId, Type = EnumNotificationTypes.Notification });
        }

        public void DeviceStatusChanged(int deviceId, EnumDeviceStatus[] newDeviceStatuses)
        {
            PostNotification(notificationInfo: new NotificationInfo() { Name = NotificationNames.DEVICE_STATUS_CHANGED, Type = EnumNotificationTypes.Notification,Source = Station, Data = new object[] { deviceId, newDeviceStatuses } });
        }

        public void SendToDutyOfficer(string fromUserId, string dutyOfficerID, string subject, string content, string notificationType)
        {
            string NotificationID = Guid.NewGuid().ToString().Trim();
            int result = new DAL.DAL_Notification().SendToDutyOfficer(NotificationID,fromUserId, dutyOfficerID, subject, content, notificationType, Station);
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
                    Source = Station,
                    NotificationID = NotificationID
                };
                PostNotification(notificationInfo);
            }
        }

        public void SendToAppDutyOfficers(string fromUserId, string subject, string content, string notificationType, string station = null, bool isInsertDB = true,string NotificationID = null)
        {


            NotificationInfo notificationInfo = new NotificationInfo()
            {
                Name = NotificationNames.ALERT_MESSAGE,
                FromUserId = fromUserId,
                Content = content,
                Subject = subject,
                Type = notificationType,
                Source = station == null ? Station : station,
                NotificationID = !isInsertDB && !string.IsNullOrEmpty(NotificationID)? NotificationID: new DAL.DAL_Notification().InsertNotification(fromUserId, null, subject, content, !string.IsNullOrEmpty(fromUserId), DateTime.Now, null, notificationType, Station)
            };
            PostNotification(notificationInfo);
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
            string result = new DAL.DAL_Notification().InsertNotification(fromUserId, toUserId, subject, content, true, DateTime.Now, null, notificationType, Station);
            if (!string.IsNullOrEmpty(result))
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
        }

        public void BackendAPISend(string NotificationNames, object Data)
        {
            PostNotification(notificationInfo: new NotificationInfo() { Name = NotificationNames, Data = Data });
        }

        public void DOUnblockSupervisee(string UserId)
        {
            PostNotification(notificationInfo: new NotificationInfo() { Name = NotificationNames.DO_UNBLOCK_SUPERVISEE, UserID = UserId });
        }
        public void AppointmentBooked(string UserID, string AppointmentID, string TimeSlotID, string _Station = null)
        {
            PostNotification(notificationInfo: new NotificationInfo() { Name = NotificationNames.APPOINTMENT_BOOKED, AppointmentID = AppointmentID, UserID = UserID, TimeSlotID = TimeSlotID, Source = string.IsNullOrEmpty(_Station) ? Station : _Station });
        }

        public void AppointmentReported(string QueueID, string AppointmentID)
        {
            PostNotification(notificationInfo: new NotificationInfo() { Name = NotificationNames.APPOINTMENT_REPORTED, AppointmentID = AppointmentID, QueueID = QueueID });
        }

        public void QueueInserted(string QueueID)
        {
            PostNotification(notificationInfo: new NotificationInfo() { Name = NotificationNames.QUEUE_INSERTED, QueueID = QueueID });
        }
        public void SSACompleted(string UserId)
        {
            PostNotification(notificationInfo: new NotificationInfo() { Name = NotificationNames.SSA_COMPLETED, UserID = UserId });
        }
        public void SSALabelPrinted(string UserId)
        {
            PostNotification(notificationInfo: new NotificationInfo() { Name = NotificationNames.SSA_PRINTING_LABEL, UserID = UserId });
        }

        /// <summary>
        /// An application inform Notification Server its status
        /// </summary>
        /// <param name="appName">SHP or SSP</param>
        /// <param name="appStatus">OK or Error or Caution</param>
        /// <param name="message">error message</param>
        public void AppStatusChanged(string appName, string appStatus, string message)
        {
            PostNotification(notificationInfo: new NotificationInfo() { Name = NotificationNames.DEVICE_STATUS_CHANGED, Type = EnumNotificationTypes.Notification, Source = appName, Status = appStatus, Content = message });
        }
        #endregion
    }
}
