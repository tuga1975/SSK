using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using Owin;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Trinity.Common;
using System.Linq;

namespace Trinity.NotificationServer
{
    public partial class WinFormsServer : Form
    {
        private IDisposable NotificationServer { get; set; }
        private string ServerURI = EnumAppConfig.NotificationServerUrl;

        private Thread _threadCheck = null;

        internal WinFormsServer()
        {
            InitializeComponent();

            // Check if another instance of Notification Server is running
            if (CommonUtil.CheckIfAnotherInstanceIsRunning("NS"))
            {
                MessageBox.Show("An instance of Notification Server is already running.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }
        }

        private void ButtonStart_Click(object sender, EventArgs e)
        {
            WriteToConsole("Starting server...");
            ButtonStart.Enabled = false;
            Task.Run(() => StartServer());
            //var context = GlobalHost.ConnectionManager.GetHubContext<TrinityHub>();
            //context.Clients.All.addNewMessageToPage(message);
        }
        private bool isStopThread = true;
        private void CheckInDrugNew()
        {

            while (!isStopThread)
            {
                try
                {
                    var lisCheck = new DAL.DAL_DrugResults().CheckDrugResult();
                    if (lisCheck.Count>0)
                    {
                        GlobalHost.ConnectionManager.GetHubContext<TrinityHub>().Clients.Clients(Program.ProfileConnected.Where(d => d.isApp && d.Station == EnumStation.DUTYOFFICER).Select(d => d.ConnectionId).ToList()).OnNewNotification(new NotificationInfo() { Name = NotificationNames.SHP_COMPLETED, NRIC = string.Empty });
                    }
                }
                catch
                {
                    
                }
                Thread.Sleep(60000);
            }
        }
        private void ButtonStop_Click(object sender, EventArgs e)
        {
            //SignalR will be disposed in the FormClosing event
            //Close();
            isStopThread = true;
            NotificationServer.Dispose();
            ButtonStop.Enabled = false;
            ButtonStart.Enabled = true;
        }

        private void StartServer()
        {
            try
            {
                NotificationServer = WebApp.Start(ServerURI);
                isStopThread = false;
                _threadCheck = new Thread(CheckInDrugNew);
                _threadCheck.IsBackground = true;
                _threadCheck.Start();
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
            Application.ExitThread();
        }
    }
}
