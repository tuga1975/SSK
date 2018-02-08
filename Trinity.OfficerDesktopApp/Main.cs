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
using Trinity.BE;
using Trinity.DAL;

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
            //ConnectAsync();
            GetAllSupervisees();
            LoadNotifications();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            ConnectAsync();
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            HubProxy.Invoke("SendNotification", txtSubject.Text, rtbContent.Text, null, cboUsers.SelectedValue.ToString(), false);
            rtbContent.Text = String.Empty;
            txtSubject.Text = String.Empty;
            txtSubject.Focus();
        }

        private async void ConnectAsync()
        {
            this.Invoke((Action)(() => lblStatus.Text = "Connecting to server..."));
            Connection = new HubConnection(ServerURI);
            Connection.Closed += Connection_Closed;
            HubProxy = Connection.CreateHubProxy("MyHub");

            HubProxy.On<string, string, string, string>("OnNewNotification", (subject, content, fromUserId, toUserId) => OnNewNotificationHandler(subject, content, fromUserId, toUserId));
            try
            {
                await Connection.Start();
                this.Invoke((Action)(() => btnConnect.Enabled = false));
            }
            catch (HttpRequestException)
            {
                this.Invoke((Action)(() => btnConnect.Enabled = true));
                this.Invoke((Action)(() => rtbContent.Enabled = false));
                this.Invoke((Action)(() => lblStatus.Text = "Unable to connect. The SignalR Server doesn't seem to work. Please reconnect"));

                // Reconnect again
                //ConnectAsync();
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
            this.Invoke((Action)(() => txtSubject.Enabled = true));
            this.Invoke((Action)(() => txtSubject.Enabled = false));
            this.Invoke((Action)(() => rtbContent.Enabled = false));
            this.Invoke((Action)(() => lblStatus.Text = "Could not connect.Reconnecting..."));
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
                    //dgvNotifications.Rows.Insert(0, 1);
                    //dgvNotifications[colFromUserName.Index, 0].Value = fromUserId;
                    //dgvNotifications[ColSubject.Index, 0].Value = subject;
                    //dgvNotifications[ColContent.Index, 0].Value = content;
                    //dgvNotifications[ColDate.Index, 0].Value = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                    LoadNotifications();
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

        private void btnCreateNewUser_Click(object sender, EventArgs e)
        {
            FormNewUser f = new FormNewUser();
            f.MainForm = this;
            f.Show();
            this.Hide();
        }

        private void GetAllSupervisees()
        {
            
            DAL_User dalUser = new DAL_User();
            var dbUsers = CallCentralized.Get<List<Trinity.BE.User>>("User", "GetAllSupervisees");
            cboUsers.DataSource = dbUsers;
            cboUsers.DisplayMember = "Name";
            cboUsers.ValueMember = "UserId";
        }
        private void LoadNotifications()
        {
            DAL_Notification dalNotification = new DAL_Notification();
            List<Notification> notifications = dalNotification.GetNotificationsSentToDutyOfficer(true);
            if (notifications != null && notifications.Count > 0)
            {
                dgvNotifications.Rows.Clear();
                notifications = notifications.OrderBy(n => n.Datetime).ToList();
                for (int i = 0; i < notifications.Count; i++)
                {
                    dgvNotifications.Rows.Insert(0, 1);
                    dgvNotifications[colFromUserName.Index, 0].Value = notifications[i].FromUserName;
                    dgvNotifications[ColSubject.Index, 0].Value = notifications[i].Subject;
                    dgvNotifications[ColContent.Index, 0].Value = notifications[i].Content;
                    dgvNotifications[ColDate.Index, 0].Value = notifications[i].Datetime.ToString("dd/MM/yyyy HH:mm");
                }
            }
        }
    }
}
