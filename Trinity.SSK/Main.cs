using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Trinity.BE;
using Trinity.Common;
using Trinity.Common.Common;
using Trinity.DAL;
using Trinity.Device;
using Trinity.Device.Authentication;

namespace SSK
{
    public partial class Main : Form
    {
        private JSCallCS _jsCallCS;
        private CodeBehind.Authentication.NRIC _nric;
        private CodeBehind.Suppervisee _suppervisee;
        private NavigatorEnums _currentPage;
        private EventCenter _eventCenter;
        private int _smartCardFailed;
        private int _fingerprintFailed;
        private bool _displayLoginButtonStatus = false;
        private bool _isFirstTimeLoaded = true;

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
            SmartCard.Instance.GetCardInfoSucceeded += GetCardInfoSucceeded;
            // Fingerprint
            Fingerprint.Instance.OnIdentificationCompleted += Fingerprint_OnIdentificationCompleted;
            Fingerprint.Instance.OnDeviceDisconnected += Fingerprint_OnDeviceDisconnected;


            // NRIC
            _nric = CodeBehind.Authentication.NRIC.GetInstance(LayerWeb);
            _nric.OnNRICSucceeded += NRIC_OnNRICSucceeded;
            _nric.OnShowMessage += OnShowMessage;

            // Supervisee
            _suppervisee = new CodeBehind.Suppervisee(LayerWeb);

            _eventCenter = EventCenter.Default;

            _eventCenter.OnNewEvent += EventCenter_OnNewEvent;
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
        private void EventCenter_OnNewEvent(object sender, EventInfo e)
        {
            if (e.Name == EventNames.ALERT_MESSAGE)
            {
                LayerWeb.InvokeScript("alertBookAppointment", e.Message);
            }
            else if (e.Name == EventNames.ABSENCE_LESS_THAN_3)
            {
                LayerWeb.InvokeScript("alertBookAppointment", e.Message);
            }
            else if (e.Name == EventNames.ABSENCE_MORE_THAN_3)
            {
                LayerWeb.InvokeScript("alertBookAppointment", e.Message);
            }
            if (e.Name == EventNames.FINGERPRINT_FAILED_MORE_THAN_3)
            {
                NavigateTo(NavigatorEnums.Authentication_SmartCard);
                LayerWeb.InvokeScript("showMessage", e.Message);

            }

        }

        /// <summary>
        /// Fingerprint_OnDeviceDisconnected
        /// </summary>
        /// 
        private void Fingerprint_OnDeviceDisconnected()
        {
            // set message
            string message = "The fingerprint reader is not connected, please report to the Duty Officer!";

            // Send Notification to duty officer
            APIUtils.SignalR.SendAllDutyOfficer(((Trinity.BE.User)Session.Instance[CommonConstants.USER_LOGIN]).UserId,"The fingerprinter is not connected", "The fingerprinter is not connected.");


            // show message box to user
            MessageBox.Show(message, "Authentication failed!", MessageBoxButtons.OK, MessageBoxIcon.Error);

            // navigate to smartcard login page
            //NavigateTo(NavigatorEnums.Authentication_SmartCard);
            NavigateTo(NavigatorEnums.Authentication_Facial);
        }

        private void Fingerprint_OnIdentificationCompleted(bool bSuccess)
        {
            if (!bSuccess)
            {
                Fingerprint_OnFingerprintFailed();
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

            Trinity.BE.User user = dAL_User.GetUserBySmartCardId(cardUID);
            // if local user is null, get user from centralized, and sync db
            if (user == null)
            {
                user = dAL_User.GetUserBySmartCardId(cardUID);
            }

            if (user != null)
            {
                // Only enrolled supervisees are allowed to login
                if (user.Status == EnumUserStatuses.Blocked)
                {
                    SmartCard_OnSmartCardFailed("You have been blocked.");
                    return;
                }
                if (user.Status == EnumUserStatuses.New)
                {
                    SmartCard_OnSmartCardFailed("You haven't enrolled yet.");
                    return;
                }
                Session session = Session.Instance;
                session.IsSmartCardAuthenticated = true;
                session[CommonConstants.USER_LOGIN] = user;
                this.LayerWeb.RunScript("$('.status-text').css('color','#000').text('Your smart card is authenticated.');");
                // Stop SCardMonitor
                SmartCardReaderUtil sCardMonitor = SmartCardReaderUtil.Instance;
                sCardMonitor.StopSmartCardMonitor();
                // raise succeeded event
                SmartCard_OnSmartCardSucceeded();
            }
            else
            {
                // raise failed event
                SmartCard_OnSmartCardFailed("Unable to read your smart card. Please report to the Duty Officer");
            }
        }

        private void JSCallCS_OnLogOutCompleted()
        {
            NavigateTo(NavigatorEnums.Authentication_SmartCard);
        }

        private void LayerWeb_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            LayerWeb.InvokeScript("createEvent", JsonConvert.SerializeObject(_jsCallCS.GetType().GetMethods().Where(d => d.IsPublic && !d.IsVirtual && !d.IsSecuritySafeCritical).ToArray().Select(d => d.Name)));

            if (_isFirstTimeLoaded)
            {
                // Start page
                NavigateTo(NavigatorEnums.Authentication_SmartCard);

                ////// For testing purpose
                //Session session = Session.Instance;
                ////////Supervisee
                //Trinity.BE.User user = new DAL_User().GetUserByUserId("bb67863c-c330-41aa-b397-c220428ad16f", true);
                //////// Duty Officer
                //////Trinity.BE.User user = new DAL_User().GetUserByUserId("ead039f9-b9a1-45bb-8186-0bb7248aafac", true);
                //session[CommonConstants.USER_LOGIN] = user;
                //session.IsSmartCardAuthenticated = true;
                //session.IsFingerprintAuthenticated = true;

                //////NavigateTo(NavigatorEnums.Authentication_Fingerprint);
                //NavigateTo(NavigatorEnums.Supervisee);
                //////NavigateTo(NavigatorEnums.Authentication_NRIC);                

                _isFirstTimeLoaded = false;
            }
        }

        private void NRIC_OnNRICSucceeded()
        {
            // navigate to Supervisee page
            APIUtils.SignalR.UserLogined(((Trinity.BE.User)Session.Instance[CommonConstants.USER_LOGIN]).UserId);
            NavigateTo(NavigatorEnums.Supervisee);
        }


        #region Smart Card Authentication
        private void SmartCard_OnSmartCardSucceeded()
        {
            // Pause for 1 second and goto Fingerprint Login Screen
            Thread.Sleep(1000);

            // navigate to next page: Authentication_Fingerprint
            NavigateTo(NavigatorEnums.Authentication_Fingerprint);

            // Testing purpose
            //NavigateTo(NavigatorEnums.Authentication_Facial);
        }

        private void SmartCard_OnSmartCardFailed(string message)
        {
            // increase counter
            _smartCardFailed++;

            // exceeded max failed
            if (_smartCardFailed > 3)
            {
                // Send Notification to duty officer
                APIUtils.SignalR.SendAllDutyOfficer(null,message, message);
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

        #endregion

        #region Facial Authentication Event Handlers
        private void Main_OnFacialRecognitionSucceeded()
        {
            _fingerprintFailed = 0;
            LayerWeb.RunScript("$('.status-text').css('color','#000').text('You have been authenticated.');");
            FacialRecognition.Instance.OnFacialRecognitionFailed -= Main_OnFacialRecognitionFailed;
            FacialRecognition.Instance.OnFacialRecognitionSucceeded -= Main_OnFacialRecognitionSucceeded;
            FacialRecognition.Instance.OnFacialRecognitionProcessing -= Main_OnFacialRecognitionProcessing;
            this.Invoke((MethodInvoker)(() =>
            {
                FacialRecognition.Instance.Dispose();
            }));

            //
            // Login successfully
            //
            // Create a session object to store UserLogin information
            Session session = Session.Instance;
            session.IsFacialAuthenticated = true;


            Thread.Sleep(1000);

            // if role = 0 (duty officer), redirect to NRIC.html
            // else (supervisee), redirect to Supervisee.html
            Trinity.BE.User user = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];
            if (user.Role == EnumUserRoles.DutyOfficer)
            {
                // navigate to Authentication_NRIC
                NavigateTo(NavigatorEnums.Authentication_NRIC);
            }
            else
            {
                APIUtils.SignalR.UserLogined(user.UserId);
                // navigate to Supervisee page
                NavigateTo(NavigatorEnums.Supervisee);
            }
        }

        private void Main_OnFacialRecognitionProcessing()
        {
            LayerWeb.RunScript("$('.status-text').css('color','#000').text('Scanning your face...');");
        }

        private void Main_OnFacialRecognitionFailed()
        {
            FacialRecognition.Instance.OnFacialRecognitionFailed -= Main_OnFacialRecognitionFailed;
            FacialRecognition.Instance.OnFacialRecognitionSucceeded -= Main_OnFacialRecognitionSucceeded;
            FacialRecognition.Instance.OnFacialRecognitionProcessing -= Main_OnFacialRecognitionProcessing;
            this.Invoke((MethodInvoker)(() =>
            {
                FacialRecognition.Instance.Dispose();
            }));
            Session session = Session.Instance;
            Trinity.BE.User user = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];
            string errorMessage = "User '" + user.Name + "' cannot complete facial authentication";
            APIUtils.SignalR.SendAllDutyOfficer(user.UserId,"Facial authentication failed", errorMessage);

            // show message box to user
            MessageBox.Show("Facial authentication failed", "Facial Authentication", MessageBoxButtons.OK, MessageBoxIcon.Error);

            // navigate to smartcard login page
            NavigateTo(NavigatorEnums.Authentication_SmartCard);

            // reset counter
            _fingerprintFailed = 0;
        }

        #endregion

        #region Fingerprint Authentication Event Handlers

        private void Fingerprint_OnFingerprintSucceeded()
        {
            //
            // Login successfully
            //
            // Create a session object to store UserLogin information
            _fingerprintFailed = 0;

            Session session = Session.Instance;
            session.IsFingerprintAuthenticated = true;

            LayerWeb.RunScript("$('.status-text').css('color','#000').text('Fingerprint authentication is successful.');");

            Thread.Sleep(1000);

            // if role = 0 (duty officer), redirect to NRIC.html
            // else (supervisee), redirect to Supervisee.html
            Trinity.BE.User user = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];
            if (user.Role == EnumUserRoles.DutyOfficer)
            {
                // navigate to Authentication_NRIC
                NavigateTo(NavigatorEnums.Authentication_NRIC);
            }
            else
            {
                // navigate to Supervisee page

                APIUtils.SignalR.UserLogined(user.UserId);

                NavigateTo(NavigatorEnums.Supervisee);
            }
        }

        private void Fingerprint_OnFingerprintFailed()
        {
            // increase counter
            _fingerprintFailed++;

            // get USER_LOGIN
            Session session = Session.Instance;
            Trinity.BE.User user = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];

            // exceeded max failed
            if (_fingerprintFailed > 3)
            {
                // set message
                string errorMessage = "Unable to read " + user.Name + "'s fingerprint.";

                // Send Notification to duty officer
                APIUtils.SignalR.SendAllDutyOfficer(user.UserId,"Fingerprint Authentication failed", errorMessage);

                //for testing purpose
                EventCenter.Default.RaiseEvent(new EventInfo {Name=EventNames.FINGERPRINT_FAILED_MORE_THAN_3 ,Code=-1,Message= "Fingerprint's Authenication failed!\n Please contact your officer."});
                
                // Pause for 1 second and goto Facial Login Screen
                //Thread.Sleep(1000);
                //_fingerprintFailed = 0;

               

                // Navigate to next page: Facial Authentication
                //NavigateTo(NavigatorEnums.Authentication_Facial);

                return;
            }

            // display failed on UI
            LayerWeb.RunScript("$('.status-text').css('color','#000').text('Please place your finger on the reader. Failed: " + _fingerprintFailed + "');");

            // restart identification
            if (user != null)
            {
                List<byte[]> fingerprintTemplates = new List<byte[]>()
                {
                    user.LeftThumbFingerprint,
                    user.RightThumbFingerprint
                };

                FingerprintReaderUtil.Instance.StartIdentification(fingerprintTemplates, Fingerprint_OnIdentificationCompleted);
            }
            else
            {
                Debug.WriteLine("Fingerprint_OnFingerprintFailed warning: USER_LOGIN is null, can not restart identification.");
            }
        }
        #endregion

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
                LayerWeb.LoadPageHtml("Authentication/SmartCard.html");
                LayerWeb.RunScript("$('.status-text').css('color','#000').text('Please place your smart card on the reader.');");
                SmartCard.Instance.Start();
            }
            else if (navigatorEnum == NavigatorEnums.Authentication_Fingerprint)
            {
                try
                {
                    Session session = Session.Instance;
                    Trinity.BE.User user = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];
                    LayerWeb.LoadPageHtml("Authentication/FingerPrint.html");
                    LayerWeb.RunScript("$('.status-text').css('color','#000').text('Please place your finger on the reader.');");
                    Fingerprint.Instance.Start(new System.Collections.Generic.List<byte[]>() { user.LeftThumbFingerprint, user.RightThumbFingerprint });
                }
                catch (System.IO.FileNotFoundException ex)
                {
                    Console.WriteLine("File missing:\n");
                    Console.WriteLine(ex.FileName);
                }
            }
            else if (navigatorEnum == NavigatorEnums.Authentication_Facial)
            {
                Session session = Session.Instance;
                Trinity.BE.User user = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];
                LayerWeb.RunScript("$('.status-text').css('color','#000').text('Please wait while initializing camera...');");
                FacialRecognition.Instance.OnFacialRecognitionFailed += Main_OnFacialRecognitionFailed;
                FacialRecognition.Instance.OnFacialRecognitionSucceeded += Main_OnFacialRecognitionSucceeded;
                FacialRecognition.Instance.OnFacialRecognitionProcessing += Main_OnFacialRecognitionProcessing;

                this.Invoke((MethodInvoker)(() =>
                {
                    Point startLocation = new Point((Screen.PrimaryScreen.Bounds.Size.Width / 2) - 400 / 2, (Screen.PrimaryScreen.Bounds.Size.Height / 2) - 400 / 2);
                    FacialRecognition.Instance.StartFacialRecognition(startLocation, new System.Collections.Generic.List<byte[]>() { user.User_Photo1, user.User_Photo2 });
                }));
            }
            else if (navigatorEnum == NavigatorEnums.Authentication_NRIC)
            {
                _nric.Start();
            }
            else if (navigatorEnum == NavigatorEnums.Supervisee)
            {
                _suppervisee.Start();
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
