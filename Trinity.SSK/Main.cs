﻿using Newtonsoft.Json;
using SSK.Common;
using SSK.Contstants;
using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Trinity.Common;
using Trinity.Common.Common;
using Trinity.Common.Monitor;
using Trinity.DAL;

namespace SSK
{
    public partial class Main : Form
    {
        private JSCallCS _jsCallCS;
        private CodeBehind.Authentication.SmartCard _smartCard;
        private CodeBehind.Authentication.Fingerprint _fingerprint;
        private CodeBehind.Authentication.NRIC _nric;
        private CodeBehind.Suppervisee _suppervisee;
        private NavigatorEnums _currentPage;

        private int _smartCardFailed;
        private int _fingerprintFailed;
        private bool _displayLoginButtonStatus = false;

        public Main()
        {
            InitializeComponent();

            // setup variables
            _smartCardFailed = 0;
            _fingerprintFailed = 0;
            _displayLoginButtonStatus = false;

            #region Initialize and register events
            // _jsCallCS
            _jsCallCS = new JSCallCS(this.LayerWeb);
            _jsCallCS.OnNRICFailed += JSCallCS_OnNRICFailed;
            _jsCallCS.OnShowMessage += JSCallCS_ShowMessage;
            _jsCallCS.OnLogOutCompleted += JSCallCS_OnLogOutCompleted;

            // SmartCard
            _smartCard = new CodeBehind.Authentication.SmartCard(LayerWeb);
            _smartCard.OnSmartCardSucceeded += SmartCard_OnSmartCardSucceeded;
            _smartCard.OnSmartCardFailed += SmartCard_OnSmartCardFailed;

            // Fingerprint
            _fingerprint = new CodeBehind.Authentication.Fingerprint(LayerWeb);
            _fingerprint.OnFingerprintSucceeded += Fingerprint_OnFingerprintSucceeded;
            _fingerprint.OnFingerprintFailed += Fingerprint_OnFingerprintFailed;
            _fingerprint.OnShowMessage += OnShowMessage;

            // NRIC
            _nric = CodeBehind.Authentication.NRIC.GetInstance(LayerWeb);
            _nric.OnNRICSucceeded += NRIC_OnNRICSucceeded;
            _nric.OnShowMessage += OnShowMessage;

            // Supervisee
            _suppervisee = new CodeBehind.Suppervisee(LayerWeb);
            #endregion


            APIUtils.LayerWeb = LayerWeb;
            LayerWeb.Url = new Uri(String.Format("file:///{0}/View/html/Layout.html", CSCallJS.curDir));
            LayerWeb.ObjectForScripting = _jsCallCS;

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

        private void JSCallCS_OnLogOutCompleted()
        {
            NavigateTo(NavigatorEnums.Authentication_SmartCard);
        }

        private void LayerWeb_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            LayerWeb.InvokeScript("createEvent", JsonConvert.SerializeObject(_jsCallCS.GetType().GetMethods().Where(d => d.IsPublic && !d.IsVirtual && !d.IsSecuritySafeCritical).ToArray().Select(d => d.Name)));

            // Start page
            NavigateTo(NavigatorEnums.Authentication_SmartCard);

            //for testing purpose
            //Session session = Session.Instance;
            //Trinity.BE.User user = new DAL_User().GetUserByUserId("656ebbb1-190b-4c8a-9d77-ffa4ff4c9e93", true);
            //session[CommonConstants.USER_LOGIN] = user;

            //NavigateTo(NavigatorEnums.Supervisee);
        }

        private void NRIC_OnNRICSucceeded()
        {
            // navigate to Supervisee page
            NavigateTo(NavigatorEnums.Supervisee_NRIC);
        }

        private void Fingerprint_OnFingerprintSucceeded()
        {
            //
            // Login successfully
            //
            // Create a session object to store UserLogin information
            Session session = Session.Instance;
            session.IsFingerprintAuthenticated = true;

            LayerWeb.RunScript("$('.status-text').css('color','#000').text('Fingerprint authentication is successful.');");
            APIUtils.SignalR.GetLatestNotifications();

            Thread.Sleep(1000);

            // if role = 0 (duty officer), redirect to NRIC.html
            // else (supervisee), redirect to Supervisee.html
            Trinity.BE.User user = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];
            if (user.Role == (int)UserRoles.DutyOfficer)
            {
                // navigate to Authentication_NRIC
                NavigateTo(NavigatorEnums.Authentication_NRIC);
            }
            else
            {
                // navigate to Supervisee page
                NavigateTo(NavigatorEnums.Supervisee);
            }
        }

        private void SmartCard_OnSmartCardSucceeded()
        {
            // Pause for 1 second and goto Fingerprint Login Screen
            Thread.Sleep(1000);

            // navigate to next page: Authentication_Fingerprint
            NavigateTo(NavigatorEnums.Authentication_Fingerprint);
        }

        private void SmartCard_OnSmartCardFailed(object sender, CodeBehind.Authentication.SmartCardEventArgs e)
        {
            // increase counter
            _smartCardFailed++;

            // exceeded max failed
            if (_smartCardFailed > 3)
            {
                // Send Notification to duty officer
                APIUtils.SignalR.SendNotificationToDutyOfficer(e.Message, e.Message);

                // show message box to user
                MessageBox.Show(e.Message, "Authentication failed", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // reset counter
                _smartCardFailed = 0;

                // display failed on UI
                LayerWeb.RunScript("$('.status-text').css('color','#000').text('Please place your smart card on the reader.');");

                return;
            }

            // display failed on UI
            LayerWeb.RunScript("$('.status-text').css('color','#000').text('Please place your smart card on the reader. Failed: " + _smartCardFailed + "');");
        }

        private void Fingerprint_OnFingerprintFailed(object sender, CodeBehind.Authentication.FingerprintEventArgs e)
        {
            // increase counter
            _fingerprintFailed++;

            // exceeded max failed
            if (_fingerprintFailed > 3)
            {
                // Send Notification to duty officer
                APIUtils.SignalR.SendNotificationToDutyOfficer(e.Message, e.Message);
                
                // show message box to user
                MessageBox.Show(e.Message, "Authentication failed", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // navigate to smartcard login page
                NavigateTo(NavigatorEnums.Authentication_SmartCard);

                // reset counter
                _fingerprintFailed = 0;

                return;
            }

            // display failed on UI
            LayerWeb.RunScript("$('.status-text').css('color','#000').text('Please place your finger on the reader. Failed: " + _fingerprintFailed + "');");
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

        private void OnShowMessage(object sender, ShowMessageEventArgs e)
        {
            MessageBox.Show(e.Message, e.Caption, e.Button, e.Icon);
        }

        private void NavigateTo(NavigatorEnums navigatorEnum)
        {
            // navigate
            if (navigatorEnum == NavigatorEnums.Authentication_SmartCard)
            {
                _smartCard.Start();
            }
            else if (navigatorEnum == NavigatorEnums.Authentication_Fingerprint)
            {
                _fingerprint.Start();
            }
            else if (navigatorEnum == NavigatorEnums.Authentication_NRIC)
            {
                _nric.Start();
            }
            else if (navigatorEnum == NavigatorEnums.Supervisee)
            {
                _suppervisee.Start();
            }
            else if (navigatorEnum == NavigatorEnums.Supervisee_NRIC)
            {
                _suppervisee.Start();
                CSCallJS.DisplayNRICLogin(LayerWeb);
            }

            // set current page
            _currentPage = navigatorEnum;

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
