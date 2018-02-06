using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.Utils.Notification
{
    public abstract class SignalRBase : ISignalR
    {
        protected IHubProxy HubProxy { get; set; }
        protected HubConnection Connection { get; set; }
        protected bool IsConnected = false;
        protected void StartConnect()
        {
            ConnectAsync(System.Reflection.Assembly.GetEntryAssembly().GetName().Name);
            Lib.SignalR = this;
        }
        private async void ConnectAsync(string station)
        {
            Connection = new HubConnection(EnumAppConfig.NotificationServerUrl + "/signalr");
            Connection.Headers.Add("Station", station);
            Connection.Closed += _Connection_Closed;

            HubProxy = Connection.CreateHubProxy("MyHub");
            //Handle incoming event from server: use Invoke to write to console from SignalR's thread
            _IncomingEvents();
            try
            {
                await Connection.Start();
                if (Connection.ConnectionId != null)
                {
                    IsConnected = true;
                }
            }
            catch
            {
                IsConnected = false;
            }
        }
        private void _Connection_Closed()
        {
            IsConnected = false;
            Connection_Closed();
            if (Connection != null)
            {
                Connection.Start();
                if (Connection.ConnectionId != null)
                {
                    IsConnected = true;
                }
            }
        }
        private void _IncomingEvents()
        {
            IncomingEvents();
        }



        public abstract void IncomingEvents();
        public abstract void Connection_Closed();

        public void UserLogined(string userID)
        {
            if (IsConnected)
                HubProxy.Invoke("UserLogined", userID);
        }
        public void UserLogout(string userID)
        {
            if (IsConnected)
                HubProxy.Invoke("UserLogout", userID);
        }

        public void DeviceStatusUpdate(int deviceId, EnumDeviceStatuses[] deviceStatuses)
        {
            if (IsConnected)
                HubProxy.Invoke("DeviceStatusUpdate", deviceId, deviceStatuses);
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
