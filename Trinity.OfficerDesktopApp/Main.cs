using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OfficerDesktopApp
{
    public partial class Main : Form
    {
        private String UserName { get; set; }
        private IHubProxy HubProxy { get; set; }
        const string ServerURI = "http://localhost:8080/signalr";
        private HubConnection Connection { get; set; }

        public Main()
        {
            InitializeComponent();
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            labelStatus.Text = "Connecting to server...";
            ConnectAsync();
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            HubProxy.Invoke("AddNotification", textBoxSubject.Text, richTextBoxContent.Text);
            richTextBoxContent.Text = String.Empty;
            textBoxSubject.Text = String.Empty;
            textBoxSubject.Focus();
        }

        private async void ConnectAsync()
        {
            Connection = new HubConnection(ServerURI);
            Connection.Closed += Connection_Closed;
            HubProxy = Connection.CreateHubProxy("MyHub");
            //Handle incoming event from server: use Invoke to write to console from SignalR's thread
            //HubProxy.On<string, string>("Add", (name, message) =>
            //    this.Invoke((Action)(() =>
            //        richTextBoxContent.AppendText(String.Format("{0}: {1}" + Environment.NewLine, name, message))
            //    ))
            //);

            try
            {
                await Connection.Start();
            }
            catch (HttpRequestException)
            {
                labelStatus.Text = "Unable to connect to server: Start server before connecting clients.";
                //No connection: Don't enable Send button or show chat UI
                return;
            }

            //Activate UI
            textBoxSubject.Enabled = true;
            richTextBoxContent.Enabled = true;
            buttonSend.Enabled = true;
            textBoxSubject.Focus();
            labelStatus.Text = "Connected to server at " + ServerURI;
        }
        private void Connection_Closed()
        {
            //Deactivate chat UI; show login UI. 
            this.Invoke((Action)(() => textBoxSubject.Enabled = false));
            this.Invoke((Action)(() => richTextBoxContent.Enabled = false));
            this.Invoke((Action)(() => labelStatus.Text = "You have been disconnected."));
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Connection != null)
            {
                Connection.Stop();
                Connection.Dispose();
            }
        }
    }
}
