using Microsoft.AspNet.SignalR.Client;

namespace Trinity.Enrolment.Utils
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
            HubProxy.On("OnNewNotification", () => GetLatestNotifications());

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

        public void SendNotificationToDutyOfficer(string subject, string content)
        {
            
        }

        public void GetLatestNotifications()
        {
            
        }
    }
}
