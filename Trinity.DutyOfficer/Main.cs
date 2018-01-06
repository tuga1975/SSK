using Newtonsoft.Json;
using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Trinity.Common;
using Trinity.Common.Common;
using Trinity.DAL;

namespace DutyOfficer
{
    public partial class Main : Form
    {
        private JSCallCS _jsCallCS;
        
        private EventCenter _eventCenter;
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

            // SmartCard
            Trinity.Common.Authentication.SmartCard.Instance.GetCardInfoSucceeded += GetCardInfoSucceeded;
            // Fingerprint
            Trinity.Common.Authentication.Fingerprint.Instance.GetVerification += GetVerificationFingerprint;
            Trinity.Common.Authentication.Fingerprint.Instance.GetHealthMonitor += GetHealthMonitorFingerprint;

            #endregion


            APIUtils.LayerWeb = LayerWeb;
            LayerWeb.Url = new Uri(String.Format("file:///{0}/View/html/Layout.html", CSCallJS.curDir));
            LayerWeb.ObjectForScripting = _jsCallCS;

        }

        private void GetHealthMonitorFingerprint(bool status)
        {
            if (!status)
            {
                LayerWeb.RunScript("alert('The fingerprint does not work');");
            }
        }
        private void GetVerificationFingerprint(bool bVerificationSuccess)
        {
            if (!bVerificationSuccess)
            {
                Fingerprint_OnFingerprintFailed("Unable to read your fingerprint. Please report to the Duty Officer");
            }
            else
            {
                Fingerprint_OnFingerprintSucceeded();
            }
        }
        private void GetCardInfoSucceeded(string cardUID)
        {
            // get local user info
            DAL_User dAL_User = new DAL_User();
            var user = dAL_User.GetUserBySmartCardId(cardUID, true);

            // if local user is null, get user from centralized, and sync db
            if (user == null)
            {
                user = dAL_User.GetUserBySmartCardId(cardUID, false);
                if (user != null && user.Role != EnumUserRoles.DutyOfficer)
                    user = null;
            }

            if (user != null)
            {
                Session session = Session.Instance;
                session.IsSmartCardAuthenticated = true;
                session[CommonConstants.USER_LOGIN] = user;
                this.LayerWeb.RunScript("$('.status-text').css('color','#000').text('Your smart card is authenticated.');");
                // Stop SCardMonitor
                Trinity.Common.Monitor.SCardMonitor sCardMonitor = Trinity.Common.Monitor.SCardMonitor.Instance;
                sCardMonitor.Stop();
                // raise succeeded event
                SmartCard_OnSmartCardSucceeded();
            }
            else
            {
                // raise failed event
                SmartCard_OnSmartCardFailed("Unable to read your smart card. Please report to the Duty Officer");
            }
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

            LayerWeb.LoadPageHtml("Table.html");
        }

        private void SmartCard_OnSmartCardSucceeded()
        {
            // Pause for 1 second and goto Fingerprint Login Screen
            Thread.Sleep(1000);

            // navigate to next page: Authentication_Fingerprint
            NavigateTo(NavigatorEnums.Authentication_Fingerprint);
        }

        private void SmartCard_OnSmartCardFailed(string message)
        {
            // increase counter
            _smartCardFailed++;

            // exceeded max failed
            if (_smartCardFailed > 3)
            {
                // Send Notification to duty officer
                APIUtils.SignalR.SendNotificationToDutyOfficer(message, message);

                // show message box to user
                MessageBox.Show(message, "Authentication failed", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // reset counter
                _smartCardFailed = 0;

                // display failed on UI
                LayerWeb.RunScript("$('.status-text').css('color','#000').text('Please place your smart card on the reader.');");

                return;
            }

            // display failed on UI
            LayerWeb.RunScript("$('.status-text').css('color','#000').text('Please place your smart card on the reader. Failed: " + _smartCardFailed + "');");
        }

        private void Fingerprint_OnFingerprintFailed(string message)
        {
            // increase counter
            _fingerprintFailed++;

            // exceeded max failed
            if (_fingerprintFailed > 3)
            {
                // Send Notification to duty officer
                APIUtils.SignalR.SendNotificationToDutyOfficer(message, message);

                // show message box to user
                MessageBox.Show(message, "Authentication failed", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // navigate to smartcard login page
                NavigateTo(NavigatorEnums.Authentication_SmartCard);

                // reset counter
                _fingerprintFailed = 0;

                return;
            }

            // display failed on UI
            LayerWeb.RunScript("$('.status-text').css('color','#000').text('Please place your finger on the reader. Failed: " + _fingerprintFailed + "');");
        }

        private void NavigateTo(NavigatorEnums navigatorEnum)
        {
            // navigate
            if (navigatorEnum == NavigatorEnums.Authentication_SmartCard)
            {
                LayerWeb.LoadPageHtml("Authentication/SmartCard.html");
                LayerWeb.RunScript("$('.status-text').css('color','#000').text('Please place your smart card on the reader.');");
                Trinity.Common.Authentication.SmartCard.Instance.Start();
            }
            else if (navigatorEnum == NavigatorEnums.Authentication_Fingerprint)
            {
                try
                {
                    Session session = Session.Instance;
                    Trinity.BE.User user = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];
                    LayerWeb.LoadPageHtml("Authentication/FingerPrint.html");
                    LayerWeb.RunScript("$('.status-text').css('color','#000').text('Please place your finger on the reader.');");
                    Trinity.Common.Authentication.Fingerprint.Instance.Start(new System.Collections.Generic.List<byte[]>() { user.LeftThumbFingerprint, user.RightThumbFingerprint });
                }
                catch (System.IO.FileNotFoundException ex)
                {
                    Console.WriteLine("File missing:\n");
                    Console.WriteLine(ex.FileName);
                }
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
        private void OnShowMessage(object sender, ShowMessageEventArgs e)
        {
            MessageBox.Show(e.Message, e.Caption, e.Button, e.Icon);
        }
        private void Main_Load(object sender, EventArgs e)
        {
            //FormQueueNumber f = FormQueueNumber.GetInstance();
            //f.ShowOnSecondaryScreen();
            //f.Show();
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.ExitThread();
            APIUtils.Dispose();
        }

        private void LayerWeb_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            LayerWeb.InvokeScript("createEvent", JsonConvert.SerializeObject(_jsCallCS.GetType().GetMethods().Where(d => d.IsPublic && !d.IsVirtual && !d.IsSecuritySafeCritical).ToArray().Select(d => d.Name)));
            NavigateTo(NavigatorEnums.Authentication_SmartCard);
        }
    }
}
