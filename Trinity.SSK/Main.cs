using Newtonsoft.Json;
using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace SSK
{
    public partial class Main : Form
    {
        private JSCallCS jsCallCS = null;
        private CodeBehind.Authentication.SmartCard smartCard = null;
        //private SmartCard smartCard = null;

        public Main()
        {
            InitializeComponent();

            APIUtils.LayerWeb = LayerWeb;
            jsCallCS = new JSCallCS(this.LayerWeb);
            jsCallCS.OnNRICFailed += JSCallCS_OnNRICFailed;
            jsCallCS.OnShowMessage += JSCallCS_ShowMessage;
            //smartCard = new SmartCard(this.LayerWeb);
            this.LayerWeb.Url = new Uri(String.Format("file:///{0}/View/html/Layout.html", CSCallJS.curDir));
            this.LayerWeb.ObjectForScripting = jsCallCS;

            //
            // For testing purpose only
            // 
            //Trinity.DAL.DAL_User dalUser = new Trinity.DAL.DAL_User();
            //Trinity.BE.User localUser = dalUser.GetUserBySmartCardId("123456789", true);
            //Trinity.BE.User centralizedUser = dalUser.GetUserBySmartCardId("999", false);
            //Trinity.DAL.DAL_Notification dalNotification = new Trinity.DAL.DAL_Notification();
            //List<Trinity.BE.Notification> myLocaNotifications = dalNotification.GetMyNotifications("dfkkmdkg", true);
            //List<Trinity.BE.Notification> myCentralizedNotifications = dalNotification.GetMyNotifications("minhdq", false);
            //APIUtils.SignalR.SendNotificationToDutyOfficer("Hello Mr. Duty Officer!", "Hello Mr. Duty Officer! I'm a Supervisee");

        }

        private void LayerWeb_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            this.LayerWeb.InvokeScript("createEvent", JsonConvert.SerializeObject(jsCallCS.GetType().GetMethods().Where(d => d.IsPublic && !d.IsVirtual && !d.IsSecuritySafeCritical).ToArray().Select(d => d.Name)));
            APIUtils.SignalR.GetLatestNotifications();
            //smartCard.Scanning();
            smartCard = new CodeBehind.Authentication.SmartCard(this.LayerWeb);
            smartCard.OnSmartCardFailed += SmartCard_OnSmartCardFailed;
            smartCard.Start();
            //CodeBehind.Authentication.NRIC nric = new CodeBehind.Authentication.NRIC(this.LayerWeb);
            //nric.Start();
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.ExitThread();
            APIUtils.Dispose();
        }

        #region events
        private void JSCallCS_OnNRICFailed(object sender, NRICEventArgs e)
        {
            MessageBox.Show(e.Message, "Authentication failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void JSCallCS_ShowMessage(object sender, ShowMessageEventArgs e)
        {
            MessageBox.Show(e.Message, e.Caption, e.Button, e.Icon);
        }

        private void SmartCard_OnSmartCardFailed(object sender, CodeBehind.Authentication.SmartCardEventArgs e)
        {
            APIUtils.SignalR.SendNotificationToDutyOfficer(e.Message, e.Message);
            MessageBox.Show(e.Message, "Authentication failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        #endregion
    }
}
