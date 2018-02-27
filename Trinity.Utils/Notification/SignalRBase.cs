using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace Trinity.SignalRClient.Notification
{
    public abstract class SignalRBase : ISignalR
    {
        protected IHubProxy HubProxy { get; set; }
        protected HubConnection Connection { get; set; }
        protected string Station
        {
            get
            {
                return System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
            }
        }
        protected bool IsConnected
        {
            get
            {
                if (Connection == null || (Connection != null && string.IsNullOrEmpty(Connection.ConnectionId)))
                    return false;
                return true;
            }
        }
        protected void StartConnect()
        {
            ConnectAsync();
            Lib.SignalR = this;
        }
        private async Task WaitConnectFalse()
        {
            while (!IsConnected)
            {
                await Task.Delay(1000);
            }
        }
        private async void ConnectAsync()
        {
            Connection = new HubConnection(EnumAppConfig.NotificationServerUrl + "/signalr");
            Connection.Headers.Add("Station", Station);
            Connection.Closed += _Connection_Closed;

            HubProxy = Connection.CreateHubProxy("MyHub");
            //Handle incoming event from server: use Invoke to write to console from SignalR's thread
            _IncomingEvents();
            try
            {
                await Connection.Start();
            }
            catch
            {
            }
        }
        private void _Connection_Closed()
        {
            Connection_Closed();
            Connection.Start();
        }
        private void _IncomingEvents()
        {
            IncomingEvents();
        }



        public abstract void IncomingEvents();
        public abstract void Connection_Closed();

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
        public void Dispose()
        {
            Connection.Closed -= _Connection_Closed;
            Connection.Stop();
            Connection = null;
            HubProxy = null;
        }


    }
}
