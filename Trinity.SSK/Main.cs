using Microsoft.AspNet.SignalR.Client;
using Newtonsoft.Json;
using SSK.DeviceMonitor;
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
        
        private JSCallCS jsCallCS = null;

        private SmartCard smartCard = null;
        
        public Main()
        {
            InitializeComponent();

            APIUtils.LayerWeb = LayerWeb;
            jsCallCS = new JSCallCS(this.LayerWeb);
            smartCard = new SmartCard(this.LayerWeb);
            this.LayerWeb.Url = new Uri(String.Format("file:///{0}/View/html/Layout.html", CSCallJS.curDir));
            this.LayerWeb.ObjectForScripting = jsCallCS;

            //
            // For testing purpose only
            // 
            Trinity.DAL.Local.DAL_User dalUser = new Trinity.DAL.Local.DAL_User();
            Trinity.BE.User user = dalUser.GetUserBySmartCardId("123456789");
            Trinity.DAL.Local.DAL_Notification dalNotification = new Trinity.DAL.Local.DAL_Notification();
            List<Trinity.BE.Notification> myNotifications = dalNotification.GetMyNotifications("dfkkmdkg");
            string a = "";
        }
        private void LayerWeb_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            this.LayerWeb.InvokeScript("createEvent", JsonConvert.SerializeObject(jsCallCS.GetType().GetMethods().Where(d => d.IsPublic && !d.IsVirtual && !d.IsSecuritySafeCritical).ToArray().Select(d => d.Name)));
            APIUtils.SignalR.CheckNotification();
            smartCard.Scanning();
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.ExitThread();
            APIUtils.Dispose();
        }
    }
}
