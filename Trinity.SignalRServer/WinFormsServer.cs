using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using Owin;
using SignalRChat.DbContext;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SignalRChat
{
    public partial class WinFormsServer : Form
    {
        private IDisposable SignalR { get; set; }
        const string ServerURI = "http://localhost:8080";

        internal WinFormsServer()
        {
            InitializeComponent();
        }

        private void ButtonStart_Click(object sender, EventArgs e)
        {
            WriteToConsole("Starting server...");
            ButtonStart.Enabled = false;
            Task.Run(() => StartServer());
        }

        private void ButtonStop_Click(object sender, EventArgs e)
        {
            //SignalR will be disposed in the FormClosing event
            Close();
        }

        private void StartServer()
        {
            try
            {
                SignalR = WebApp.Start(ServerURI);
            }
            catch (TargetInvocationException)
            {
                WriteToConsole("Server could not start. Another instance is running...");
                //Re-enable button to let user try to start server again
                this.Invoke((Action)(() => ButtonStart.Enabled = true));
                return;
            }
            this.Invoke((Action)(() => ButtonStop.Enabled = true));
            WriteToConsole("Server started at " + ServerURI);
        }

        internal void WriteToConsole(String message)
        {
            if (RichTextBoxConsole.InvokeRequired)
            {
                this.Invoke((Action)(() =>
                    WriteToConsole(message)
                ));
                return;
            }
            RichTextBoxConsole.AppendText(message + Environment.NewLine);
        }

        private void WinFormsServer_FormClosing(object sender, FormClosingEventArgs e)
        {

            if (SignalR != null)
            {
                SignalR.Dispose();
            }
        }
    }
    class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCors(CorsOptions.AllowAll);
            app.MapSignalR();
        }
    }

    public class MyHub : Hub
    {
        public void SendNotification(string subject, string content, string fromUserId, string toUserId, bool isFromSupervisee)
        {
            // Insert notification into centralized DB first
            Trinity.DAL.DAL_Notification dalNotification = new Trinity.DAL.DAL_Notification();
            dalNotification.InsertNotification(subject, content, fromUserId, toUserId, isFromSupervisee, false);

            // And notify all client about new notification
            // In later phase we should send to appropriate client
            Clients.All.OnNewNotification(subject, content, fromUserId, toUserId, isFromSupervisee);

            Program.MainForm.WriteToConsole("User:" + fromUserId + "' send notification|subject:" + subject + "|" + "|content:" + content + "|to user:" + toUserId);
        }

        public override Task OnConnected()
        {
            Program.MainForm.WriteToConsole("Client connected: " + Context.ConnectionId);
            return base.OnConnected();
        }
        public override Task OnDisconnected()
        {
            Program.MainForm.WriteToConsole("Client disconnected: " + Context.ConnectionId);
            return base.OnDisconnected();
        }
    }
}
