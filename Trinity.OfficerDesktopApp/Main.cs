using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
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

        private void Main_Load(object sender, EventArgs e)
        {
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
            lblStatus.Text = "Connecting to server...";
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
                this.Invoke((Action)(() => rtbContent.Enabled = false));
                this.Invoke((Action)(() => lblStatus.Text = "Unable to connect. The SignalR Server doesn't seem to work. Reconnecting..."));

                // Reconnect again
                ConnectAsync();
                return;
            }

            //Activate UI
            txtSubject.Enabled = true;
            rtbContent.Enabled = true;
            btnSend.Enabled = true;
            txtSubject.Focus();
            lblStatus.Text = "Connected to server at " + ServerURI;
        }
        private void Connection_Closed()
        {
            //Deactivate chat UI; show login UI. 
            this.Invoke((Action)(() => txtSubject.Enabled = false));
            this.Invoke((Action)(() => rtbContent.Enabled = false));
            this.Invoke((Action)(() => lblStatus.Text = "Could not connect.Reconnecting..."));

            Thread.Sleep(1000);
            ConnectAsync();
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

        private void button1_Click(object sender, EventArgs e)
        {
            FormNewUser f = new FormNewUser();
            f.Show();
            this.Hide();
        }
    }
}
