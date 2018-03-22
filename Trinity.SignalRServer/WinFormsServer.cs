using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using Owin;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Trinity.NotificationServer
{
    public partial class WinFormsServer : Form
    {
        private IDisposable NotificationServer { get; set; }
        private string ServerURI = EnumAppConfig.NotificationServerUrl;
        

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
            //Close();
            NotificationServer.Dispose();
            ButtonStop.Enabled = false;
            ButtonStart.Enabled = true;
        }

        private void StartServer()
        {
            try
            {
                NotificationServer = WebApp.Start(ServerURI);
            }
            catch (Exception ex)
            {
                WriteToConsole("Server could not start. Details:" + ex.Message);
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

            if (NotificationServer != null)
            {
                NotificationServer.Dispose();
            }
        }
    }
}
