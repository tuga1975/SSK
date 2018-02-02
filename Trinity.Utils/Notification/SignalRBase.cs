using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.Utils.Notification
{
    public abstract class SignalRBase
    {
        protected IHubProxy HubProxy { get; set; }
        protected HubConnection Connection { get; set; }
        protected void StartConnect(string station)
        {
            ConnectAsync(station);
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
            }
            catch
            {
                //No connection: Don't enable Send button or show chat UI
                return;
            }
        }
        private void _Connection_Closed()
        {
            Connection_Closed();
            if (Connection != null)
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
            HubProxy.Invoke("UserLogined", userID);
        }
        public void UserLogout(string userID)
        {
            HubProxy.Invoke("UserLogout", userID);
        }

        public void Dispose()
        {
            Connection.Stop();
            Connection = null;
            HubProxy = null;
        }


    }
}
