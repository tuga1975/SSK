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
        public async void SendToDutyOfficer(string UserId, string DutyOfficerID, string Subject, string Content, string notificationType)
        {
            if (IsConnected)
            {
                string IDMessage = await HubProxy.Invoke<string>("SendToDutyOfficer", UserId, DutyOfficerID, Subject, Content, notificationType);
                if (IDMessage == null)
                {
                    Lib.LayerWeb.InvokeScript("ShowMessageBox", EnumMessage.NotConnectCentralized);
                }
                else
                {
                    new DAL.DAL_Notification().SendToDutyOfficer(IDMessage, UserId, DutyOfficerID, Subject, Content, notificationType,Station);
                }
            }
        }
        public async void SendAllDutyOfficer(string UserId, string Subject, string Content, string notificationType)
        {
            if (IsConnected)
            {
                List<Trinity.BE.Notification> arrraySend = await HubProxy.Invoke<List<Trinity.BE.Notification>>("SendAllDutyOfficer", UserId, Subject, Content, notificationType);
                if (arrraySend != null)
                {
                    new DAL.DAL_Notification().SendAllDutyOfficer(arrraySend);
                }
                else
                {
                    Lib.LayerWeb.InvokeScript("ShowMessageBox", EnumMessage.NotConnectCentralized);
                }
            }
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
