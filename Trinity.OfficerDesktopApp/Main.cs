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
            HubProxy.Invoke("SendNotification", txtSubject.Text, rtbContent.Text, "dutyofficer", "supervisee", false);
            rtbContent.Text = String.Empty;
            txtSubject.Text = String.Empty;
            txtSubject.Focus();
        }

        private async void ConnectAsync()
        {
            Connection = new HubConnection(ServerURI);
            Connection.Closed += Connection_Closed;
            HubProxy = Connection.CreateHubProxy("MyHub");

            HubProxy.On<string, string, string, string>("OnNewNotification", (subject, content, fromUserId, toUserId) => OnNewNotificationHandler(subject, content, fromUserId, toUserId));
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
            txtSubject.Enabled = true;
            rtbContent.Enabled = true;
            buttonSend.Enabled = true;
            txtSubject.Focus();
            labelStatus.Text = "Connected to server at " + ServerURI;
        }
        private void Connection_Closed()
        {
            //Deactivate chat UI; show login UI. 
            this.Invoke((Action)(() => txtSubject.Enabled = false));
            this.Invoke((Action)(() => rtbContent.Enabled = false));
            this.Invoke((Action)(() => labelStatus.Text = "You have been disconnected."));
        }

        delegate void OnNewNotificationHandlerDelegate(string subject, string content, string fromUserId, string toUserId);
        private void OnNewNotificationHandler(string subject, string content, string fromUserId, string toUserId)
        {
            if (string.IsNullOrEmpty(toUserId))
            {
                if (this.dgvNotifications.InvokeRequired)
                {
                    OnNewNotificationHandlerDelegate onNewNotificationHandlerDelegated = new OnNewNotificationHandlerDelegate(OnNewNotificationHandler);
                    this.Invoke(onNewNotificationHandlerDelegated, new object[] { subject, content, fromUserId, toUserId });
                }
                else
                {
                    dgvNotifications.Rows.Insert(0, 1);
                    dgvNotifications[colFromUserId.Index, 0].Value = fromUserId;
                    dgvNotifications[ColSubject.Index, 0].Value = subject;
                    dgvNotifications[ColContent.Index, 0].Value = content;
                }
            }
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
