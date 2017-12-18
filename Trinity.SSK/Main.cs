using Newtonsoft.Json;
using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace SSK
{
    public partial class Main : Form
    {
        private JSCallCS _jsCallCS = null;
        private CodeBehind.Authentication.SmartCard _smartCard = null;
        private CodeBehind.Authentication.Fingerprint _fingerprint = null;
        private CodeBehind.Authentication.NRIC _nric = null;
        private NavigatorEnums _currentPage;
        private bool _displayLoginButtonStatus = false;
        //private SmartCard smartCard = null;

        public Main()
        {
            InitializeComponent();

            APIUtils.LayerWeb = LayerWeb;
            _jsCallCS = new JSCallCS(this.LayerWeb);
            _jsCallCS.OnNRICFailed += JSCallCS_OnNRICFailed;
            _jsCallCS.OnShowMessage += JSCallCS_ShowMessage;
            _jsCallCS.OnNavigate += OnNavigate;
            //smartCard = new SmartCard(this.LayerWeb);
            this.LayerWeb.Url = new Uri(String.Format("file:///{0}/View/html/Layout.html", CSCallJS.curDir));
            this.LayerWeb.ObjectForScripting = _jsCallCS;

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
            this.LayerWeb.InvokeScript("createEvent", JsonConvert.SerializeObject(_jsCallCS.GetType().GetMethods().Where(d => d.IsPublic && !d.IsVirtual && !d.IsSecuritySafeCritical).ToArray().Select(d => d.Name)));

            //smartCard.Scanning();


            // vừa dóng
            //_nric = new CodeBehind.Authentication.NRIC(this.LayerWeb);
            //_nric.OnNavigate += OnNavigate;
            //_fingerprint = new CodeBehind.Authentication.Fingerprint(this.LayerWeb, _nric);
            //_fingerprint.OnFingerprintFailed += Fingerprint_OnFingerprintFailed;
            //_fingerprint.OnShowMessage += Fingerprint_ShowMessage;
            //_fingerprint.OnNavigate += OnNavigate;
            //_smartCard = new CodeBehind.Authentication.SmartCard(this.LayerWeb, _fingerprint);
            //_smartCard.OnSmartCardFailed += SmartCard_OnSmartCardFailed;
            //OnNavigate(new object(), new NavigateEventArgs(NavigatorEnums.Authentication_SmartCard));

            //_smartCard.Start();
            //_nric.Start();
            //_fingerprint.Start();


            Trinity.Common.Session session = Trinity.Common.Session.Instance;
            
            session[Contstants.CommonConstants.USER_LOGIN] = new Trinity.DAL.DAL_User().GetUserByUserId("656ebbb1-190b-4c8a-9d77-ffa4ff4c9e93", true);
            this.LayerWeb.LoadPageHtml("Supervisee.html");


        }

        private void Fingerprint_OnFingerprintFailed(object sender, CodeBehind.Authentication.FingerprintEventArgs e)
        {
            APIUtils.SignalR.SendNotificationToDutyOfficer(e.Message, e.Message);
            MessageBox.Show(e.Message, "Authentication failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void Fingerprint_ShowMessage(object sender, ShowMessageEventArgs e)
        {
            MessageBox.Show(e.Message, e.Caption, e.Button, e.Icon);
        }

        private void SmartCard_OnSmartCardFailed(object sender, CodeBehind.Authentication.SmartCardEventArgs e)
        {
            APIUtils.SignalR.SendNotificationToDutyOfficer(e.Message, e.Message);
            MessageBox.Show(e.Message, "Authentication failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void OnNavigate(object sender, NavigateEventArgs e)
        {
            // navigate
            if (e.navigatorEnum == NavigatorEnums.Authentication_SmartCard)
            {
                _smartCard.Start();
            }
            else if (e.navigatorEnum == NavigatorEnums.Authentication_Fingerprint)
            {
                _fingerprint.Start();
                CSCallJS.DisplayLogoutButton(this.LayerWeb, true);
            }
            else if (e.navigatorEnum == NavigatorEnums.Authentication_NRIC)
            {
                _nric.Start();
                CSCallJS.DisplayLogoutButton(this.LayerWeb, true);
            }

            // set current page
            _currentPage = e.navigatorEnum;

            // display options in Authentication_SmartCard page
            if (_displayLoginButtonStatus && _currentPage == NavigatorEnums.Authentication_SmartCard)
            {
                _displayLoginButtonStatus = false;
                CSCallJS.DisplayLogoutButton(this.LayerWeb, _displayLoginButtonStatus);
            }

            // display options in the rest
            if (!_displayLoginButtonStatus && _currentPage != NavigatorEnums.Authentication_SmartCard)
            {
                _displayLoginButtonStatus = true;
                CSCallJS.DisplayLogoutButton(this.LayerWeb, _displayLoginButtonStatus);
            }
        }
        #endregion

        private void Main_Load(object sender, EventArgs e)
        {
            FormQueueNumber f = FormQueueNumber.GetInstance();            
            f.ShowOnSecondaryScreen();
            f.Show();
        }
    }
}
