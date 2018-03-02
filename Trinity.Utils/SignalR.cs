using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Trinity.SignalR.Client
{
    public class SignalR:ISignalR
    {
        #region Singleton Implementation
        private static volatile SignalR _instance;
        private static object syncRoot = new Object();
        private IHubProxy HubProxy { get; set; }
        private HubConnection Connection { get; set; }
        private SignalR()
        {
            ConnectAsync();
            Lib.SignalR = this;
        }
        public static SignalR Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (_instance == null)
                            _instance = new SignalR();
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
                return System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
            }
        }
        private bool IsConnected
        {
            get
            {
                if (Connection!=null && Connection.State == ConnectionState.Connected)
                    return true;
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
            Connection.Closed += _Connection_Closed;
            HubProxy = Connection.CreateHubProxy("MyHub");
            IncomingEvents();
            ConnectSignalr();
        }
        private void _Connection_Closed()
        {
            if(Event_ConnectionClosed!=null)
                Event_ConnectionClosed();
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
        public event Action Event_ConnectionClosed;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="queue"></param>
        public event Action<Trinity.BE.Queue> Event_QueueFinished;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="deviceStatuses"></param>
        /// <param name="Station"></param>
        public event Action<int, EnumDeviceStatuses[], string> Event_DeviceStatusUpdate;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="NotificationID"></param>
        /// <param name="UserId"></param>
        /// <param name="Subject"></param>
        /// <param name="Content"></param>
        /// <param name="notificationType"></param>
        /// <param name="Station"></param>
        public event Action<string, string, string, string, string, string> Event_MessageTo;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Station"></param>
        public event Action<string> Event_AppDisconnect;
        private void IncomingEvents()
        {
            HubProxy.On<Trinity.BE.Queue>("QueueFinished", (queue) =>
            {
                if (Event_QueueFinished != null)
                    Event_QueueFinished(queue);
            });
            HubProxy.On<int, EnumDeviceStatuses[], string>("DeviceStatusUpdate", (deviceId, deviceStatuses, Station) =>
            {
                if (Event_DeviceStatusUpdate != null)
                    Event_DeviceStatusUpdate(deviceId, deviceStatuses, Station);

            });
            HubProxy.On<string, string, string, string, string, string>("MessageTo", (NotificationID, UserId, Subject, Content, notificationType, Station) =>
            {
                if (Event_MessageTo != null)
                    Event_MessageTo(NotificationID, UserId, Subject, Content, notificationType, Station);
            });
            HubProxy.On<string>("AppDisconnect", (Station) =>
            {
                if (Event_AppDisconnect != null)
                    Event_AppDisconnect(Station);
            });
        }
        public async void UserLogined(string userID)
        {
            await WaitConnectFalse();
            if (IsConnected)
                await HubProxy.Invoke("UserLogined", userID);
        }
        public async void UserLogout(string userID)
        {
            await WaitConnectFalse();
            await HubProxy.Invoke("UserLogout", userID);
        }

        public async void DeviceStatusUpdate(int deviceId, EnumDeviceStatuses[] deviceStatuses)
        {
            await WaitConnectFalse();
            await HubProxy.Invoke("DeviceStatusUpdate", deviceId, deviceStatuses);
        }
        public async void SendToDutyOfficer(string UserId, string DutyOfficerID, string Subject, string Content, string notificationType)
        {
            await WaitConnectFalse();
            string IDMessage = Guid.NewGuid().ToString().Trim();
            if (new DAL.DAL_Notification().SendToDutyOfficer(IDMessage, UserId, DutyOfficerID, Subject, Content, notificationType, Station) > 0)
            {
                bool status = await HubProxy.Invoke<bool>("SendToDutyOfficer", IDMessage, UserId, DutyOfficerID, Subject, Content, notificationType);
            }
        }
        public async void SendAllDutyOfficer(string UserId, string Subject, string Content, string notificationType)
        {
            await WaitConnectFalse();
            Dictionary<string, string> arrraySend = new DAL.DAL_Notification().SendAllDutyOfficer(UserId, Subject, Content, notificationType, Station).ToDictionary(f => f.ToUserId, f => f.NotificationID);
            bool status = await HubProxy.Invoke<bool>("SendAllDutyOfficer", arrraySend, UserId, Subject, Content, notificationType);
        }
        public async void QueueFinished(string UserId)
        {
            await WaitConnectFalse();
            Trinity.BE.Queue queue = new DAL.DAL_QueueNumber().GetMyQueueToday(UserId);
            await HubProxy.Invoke("QueueFinished", queue);
        }
        #endregion
    }
}
