using Microsoft.AspNet.SignalR.Client;
using Newtonsoft.Json;
using SSK.Business;
using SSK.DriverScan;
using SSK.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SSK
{
    public partial class Main : Form
    {
        private IHubProxy HubProxy { get; set; }
        const string ServerURI = "http://localhost:8080/signalr";
        private HubConnection Connection { get; set; }
        private JSCallCS jsCallCS = null;

        private SmartCard smartCard = null;
        public Main()
        {
            InitializeComponent();
            jsCallCS = new JSCallCS(this.LayerWeb);
            smartCard = new SmartCard(this.LayerWeb);
            this.LayerWeb.Url = new Uri(String.Format("file:///{0}/View/html/Layer.html", CSCallJS.curDir));
            this.LayerWeb.ObjectForScripting = jsCallCS;
            
            ConnectAsync();

        }

        private void LayerWeb_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

            //this.LayerWeb.LoadPageHtml("login.html");
            this.LayerWeb.InvokeScript("createEvent", JsonConvert.SerializeObject(jsCallCS.GetType().GetMethods().Where(d => d.IsPublic && !d.IsVirtual && !d.IsSecuritySafeCritical).ToArray().Select(d => d.Name)));
            checkNotification();
            smartCard.Scanning();

        }

        private async void ConnectAsync()
        {
            Connection = new HubConnection(ServerURI);
            Connection.Closed += Connection_Closed;
            HubProxy = Connection.CreateHubProxy("MyHub");
            //Handle incoming event from server: use Invoke to write to console from SignalR's thread
            HubProxy.On("checkNotification", () => checkNotification());

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
        private void checkNotification()
        {
            //DbContext.SSKCentralizedEntities sSKCentralizedEntities = new DbContext.SSKCentralizedEntities();
            //var unread = sSKCentralizedEntities.Notifications.Where(item => item.Read != true).Select(d => d.Id).Count();
            LayerWeb.Invoke((MethodInvoker)(() =>
            {
                LayerWeb.PushNoti(10);
            }));
        }
    }
}
