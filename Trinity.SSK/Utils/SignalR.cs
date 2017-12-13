using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity.DAL.DAL;

namespace SSK.Utils
{
    class SignalR
    {
        private IHubProxy HubProxy { get; set; }
        const string ServerURI = "http://localhost:8080/signalr";
        private HubConnection Connection { get; set; }

        public SignalR()
        {
            ConnectAsync();
        }
        private async void ConnectAsync()
        {
            Connection = new HubConnection(ServerURI);
            Connection.Closed += Connection_Closed;
            HubProxy = Connection.CreateHubProxy("MyHub");
            //Handle incoming event from server: use Invoke to write to console from SignalR's thread
            HubProxy.On("checkNotification", () => CheckNotification());

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
        private void Connection_Closed()
        {
            Connection.Start();

        }

        public void AddNotification(string Subject,string Content)
        {
            HubProxy.Invoke("AddNotification", Subject, Content);
        }

        public void CheckNotification()
        {
            DAL_Notification dal_Notification = new DAL_Notification();
            APIUtils.LayerWeb.Invoke((System.Windows.Forms.MethodInvoker)(() =>
            {
                APIUtils.LayerWeb.PushNoti(dal_Notification.GetNumberOfUnreadNotification());
            }));
        }
    }
}
