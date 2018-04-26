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
using Trinity.Common.Utils;
using Trinity.DAL;
using Trinity.Device;
using Trinity.Device.Authentication;
using Trinity.Device.Util;
using Trinity.SignalR;
using Trinity.Util;

namespace ARK
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
        private Client _signalrClient = null;
        private System.Timers.Timer _timerCheckLogout;
        private long? _timeActionApp;
        public Main()
        {
            InitializeComponent();
            APIUtils.Start();
            //Notification
            Trinity.SignalR.Client.Instance.OnQueueCompleted += OnQueueCompleted_Handler;
            Trinity.SignalR.Client.Instance.OnDOUnblockSupervisee += DOUnblockSupervisee_Handler;
            Trinity.SignalR.Client.Instance.OnAppointmentBooked += OnAppointmentBooked_Handler;
            // setup variables
            _smartCardFailed = 0;
            _fingerprintFailed = 0;
            _displayLoginButtonStatus = false;

            #region Initialize and register events
            // _jsCallCS
            _jsCallCS = new JSCallCS(this.LayerWeb, this);
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


            Lib.LayerWeb = LayerWeb;
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
            //Trinity.SignalR.Client.SignalR.Instance.SendNotificationToDutyOfficer("Hello Mr. Duty Officer!", "Hello Mr. Duty Officer! I'm a Supervisee");

        }
        private void OnAppointmentBooked_Handler(object sender, NotificationInfo e)
        {

        }
        private void DOUnblockSupervisee_Handler(object sender, NotificationInfo e)
        {
            APIUtils.FormQueueNumber.RefreshQueueNumbers();
        }
        private void OnQueueCompleted_Handler(object sender, EventInfo e)
        {
            APIUtils.FormQueueNumber.RefreshQueueNumbers();
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
            else if (e.Name == EventNames.LOGIN_SUCCEEDED)
            {
                NavigateTo(NavigatorEnums.Authentication_NRIC);
            }
            else if (e.Name.Equals(EventNames.LOGIN_FAILED))
            {
                LayerWeb.ShowMessage("Login Failed", e.Message);
            }
        }

        /// <summary>
        /// Fingerprint_OnDeviceDisconnected
        /// </summary>
        /// 
        private void Fingerprint_OnDeviceDisconnected()
        {
            // set message
            string message = "The fingerprint reader is not connected. Please report to the Duty Officer!";

            // Send Notification to duty officer
            Trinity.SignalR.Client.Instance.SendToAppDutyOfficers(null, "The fingerprint reader is not connected.", "The fingerprint reader is not connected.", EnumNotificationTypes.Error);


            // show message box to user
            CSCallJS.ShowMessage(LayerWeb, "Authentication failed!", message);
            //MessageBox.Show(message, "Authentication failed!", MessageBoxButtons.OK, MessageBoxIcon.Error);

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
            DAL_User dAL_User = new DAL_User();
            Trinity.BE.User user = dAL_User.GetUserBySmartCardId(cardUID);

            if (user != null)
            {
                // Only enrolled supervisees are allowed to login
                if (user.Role == EnumUserRoles.Supervisee || user.Role == EnumUserRoles.DutyOfficer)
                {
                    if (user.Status == EnumUserStatuses.New)
                    {
                        SmartCard_OnSmartCardFailed("You haven't enrolled yet.");
                        return;
                    }
                    if (user.Role == EnumUserRoles.Supervisee)
                    {
                        var smartCard =  new DAL_IssueCard().GetIssueCardBySmartCardId(cardUID);
                        if (smartCard == null)
                        {
                            SmartCard_OnSmartCardFailed("Your smart card does not exist");
                            return;
                        }else if (smartCard.Status== EnumIssuedCards.Inactive)
                        {
                            SmartCard_OnSmartCardFailed("Your smart card does not work");
                            return;
                        }else if (smartCard.Status == EnumIssuedCards.Active && smartCard.Expired_Date<DateTime.Today)
                        {
                            SmartCard_OnSmartCardFailed("Your smart card has expired");
                            return;
                        }
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
                    SmartCard_OnSmartCardFailed("You do not have permission to login to this system");
                }
            }
            else
            {
                // raise failed event
                SmartCard_OnSmartCardFailed("Unable to read your smart card.<br/>Please report to the Duty officer.");
            }
        }

        private void JSCallCS_OnLogOutCompleted()
        {
            try
            {
                // Disconnect the BarcodeScanner
                BarcodeScannerUtil.Instance.Disconnect();

                // Reset ApplicationStatusManager IsBusy status
                ApplicationStatusManager.Instance.IsBusy = false;

                // navigate
                NavigateTo(NavigatorEnums.Authentication_SmartCard);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void LayerWeb_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            LogManager.Debug("WebBrowser Document Completed");
            LayerWeb.InvokeScript("createEvent", JsonConvert.SerializeObject(_jsCallCS.GetType().GetMethods().Where(d => d.IsPublic && !d.IsVirtual && !d.IsSecuritySafeCritical).ToArray().Select(d => d.Name)));
            if (_isFirstTimeLoaded)
            {
                // Start page
                //NavigateTo(NavigatorEnums.Authentication_SmartCard);

                string startFrom = System.Configuration.ConfigurationManager.AppSettings["startFrom"];

                // 50.132
                //string superviseeId = "2FFD1A82-E5EC-4884-A5C6-1A68F661DAED";
                //string dutyOfficerId = "9903e059-7209-45b6-a889-6c4cfdfaeea3";
                // 1.120
                //string superviseeId = "BEA35A8C-097E-41F2-94BE-6EE7228DA696";
                //string dutyOfficerId = "f1748cb4-3bb5-4129-852d-2aba28bb8cec";

                string superviseeId = System.Configuration.ConfigurationManager.AppSettings["superviseeId"];
                string dutyOfficerId = System.Configuration.ConfigurationManager.AppSettings["dutyOfficerId"];

                Session session = Session.Instance;

                if (startFrom == "Supervisee")
                {
                    Trinity.BE.User user = new DAL_User().GetUserByUserId(superviseeId).Data;
                    session[CommonConstants.USER_LOGIN] = user;
                    session.IsSmartCardAuthenticated = true;
                    session.IsFingerprintAuthenticated = true;
                    NavigateTo(NavigatorEnums.Supervisee);
                }
                else if (startFrom == "Authentication_Fingerprint")
                {
                    Trinity.BE.User user = new DAL_User().GetUserByUserId(superviseeId).Data;
                    session[CommonConstants.USER_LOGIN] = user;
                    session.IsSmartCardAuthenticated = true;
                    session.IsFingerprintAuthenticated = true;
                    NavigateTo(NavigatorEnums.Authentication_Fingerprint);
                }
                else if (startFrom == "Authentication_NRIC")
                {
                    Trinity.BE.User user = new DAL_User().GetUserByUserId(dutyOfficerId).Data;
                    session[CommonConstants.USER_LOGIN] = user;
                    session.IsSmartCardAuthenticated = true;
                    session.IsFingerprintAuthenticated = true;
                    NavigateTo(NavigatorEnums.Authentication_NRIC);
                }
                else
                {
                    NavigateTo(NavigatorEnums.Authentication_SmartCard);
                }

                this._timerCheckLogout = new System.Timers.Timer();
                this._timerCheckLogout.AutoReset = true;
                this._timerCheckLogout.Interval = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["seconds_check_logout"])*1000;
                this._timerCheckLogout.Elapsed += TimeCheckLogout_EventHandler; ;
                this._timerCheckLogout.Start();

                _isFirstTimeLoaded = false;

                // LayerWeb initiation is compeleted, update application status
                ApplicationStatusManager.Instance.LayerWebInitilizationCompleted();
            }
        }

        private void TimeCheckLogout_EventHandler(object sender, System.Timers.ElapsedEventArgs e)
        {
            long time = long.Parse(LayerWeb.InvokeScript("getTimeActionApp").ToString());
            if (_timeActionApp.HasValue && time - _timeActionApp.Value==0 && (Trinity.BE.User)Session.Instance[CommonConstants.USER_LOGIN]!=null)
            {
                _jsCallCS.LogOut();
            }
            _timeActionApp = time;
        }
        private void NRIC_OnNRICSucceeded()
        {
            // navigate to Supervisee page
            Trinity.BE.User currentUser = (Trinity.BE.User)Session.Instance[CommonConstants.USER_LOGIN];
            Trinity.BE.User supervisee = currentUser;
            if (currentUser.Role == EnumUserRoles.DutyOfficer)
            {
                supervisee = (Trinity.BE.User)Session.Instance[CommonConstants.SUPERVISEE];
            }
            if (supervisee != null)
            {
                if (supervisee.Status == EnumUserStatuses.Blocked)
                {
                    LayerWeb.ShowMessage("This supervisee was blocked");
                }
                else
                {
                    Trinity.SignalR.Client.Instance.UserLoggedIn(supervisee.UserId);
                    NavigateTo(NavigatorEnums.Supervisee);
                }
            }
        }


        #region Smart Card Authentication
        private void SmartCard_OnSmartCardSucceeded()
        {
            // Set application status is busy
            ApplicationStatusManager.Instance.IsBusy = true;
            LayerWeb.RunScript("$('[status-authentication]').text('');");
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
            LayerWeb.ShowMessage("Authentication failed", message);
            // exceeded max failed
            if (_smartCardFailed > 3)
            {
                // Send Notification to duty officer
                Trinity.SignalR.Client.Instance.SendToAppDutyOfficers(null, message, message, EnumNotificationTypes.Error);
                // show message box to user
                //MessageBox.Show(message, "Authentication failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // reset counter
                _smartCardFailed = 0;
                // display failed on UI
                LayerWeb.RunScript("$('.status-text').css('color','#000').text('Please place your smart card on the reader.');");
                LayerWeb.RunScript("$('[status-authentication]').text('');");
                return;
            }
            else
            {
                // display failed on UI
                LayerWeb.RunScript("$('.status-text').css('color','#000').text('Please place your smart card on the reader.');");
                LayerWeb.RunScript("$('[status-authentication]').text('SmartCard Verification Failed : " + _smartCardFailed + "');");
            }
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
            FacialRecognition.Instance.OnCameraInitialized -= Main_OnCameraInitialized;
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
                Trinity.SignalR.Client.Instance.UserLoggedIn(user.UserId);
                // navigate to Supervisee page
                NavigateTo(NavigatorEnums.Supervisee);
            }
        }

        private void Main_OnFacialRecognitionProcessing()
        {
            LayerWeb.RunScript("$('.status-text').css('color','#000').text('Processing...');");
        }

        private void Main_OnCameraInitialized()
        {
            //LayerWeb.RunScript("$('.facialRecognition').hide();");
        }

        private void Main_OnFacialRecognitionFailed()
        {
            FacialRecognition.Instance.OnFacialRecognitionFailed -= Main_OnFacialRecognitionFailed;
            FacialRecognition.Instance.OnFacialRecognitionSucceeded -= Main_OnFacialRecognitionSucceeded;
            FacialRecognition.Instance.OnFacialRecognitionProcessing -= Main_OnFacialRecognitionProcessing;
            FacialRecognition.Instance.OnCameraInitialized -= Main_OnCameraInitialized;

            this.Invoke((MethodInvoker)(() =>
            {
                FacialRecognition.Instance.Dispose();
            }));
            Session session = Session.Instance;
            Trinity.BE.User user = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];
            string errorMessage = "User '" + user.Name + "' cannot complete facial authentication";
            Trinity.SignalR.Client.Instance.SendToAppDutyOfficers(user.UserId, "Facial authentication failed", errorMessage, EnumNotificationTypes.Error);

            // show message box to user
            CSCallJS.ShowMessage(LayerWeb, "Facial Authentication", "Facial authentication failed");
            //MessageBox.Show("Facial authentication failed", "Facial Authentication", MessageBoxButtons.OK, MessageBoxIcon.Error);

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
            LayerWeb.RunScript("$('[status-authentication]').text('');");
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

                Trinity.SignalR.Client.Instance.UserLoggedIn(user.UserId);

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
                Trinity.SignalR.Client.Instance.SendToAppDutyOfficers(user.UserId, "Fingerprint Authentication failed", errorMessage, EnumNotificationTypes.Error);

                //NavigateTo(NavigatorEnums.Authentication_SmartCard);
                //LayerWeb.ShowMessage("Authentication failed", "Fingerprint's Authenication failed!<br /> Please contact your officer.");

                //for testing purpose
                // Pause for 1 second and goto Facial Login Screen
                //Thread.Sleep(1000);
                _fingerprintFailed = 0;

                // Navigate to next page: Facial Authentication
                NavigateTo(NavigatorEnums.Authentication_Facial);

                return;
            }

            // display failed on UI
            LayerWeb.RunScript("$('.status-text').css('color','#000').text('Please place your finger on the reader');");
            LayerWeb.RunScript("$('[status-authentication]').text('FingerPrint Verification Failed : " + _fingerprintFailed + "');");

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
            // Turn off all LED(s)
            if (LEDStatusLightingUtil.Instance.IsPortOpen)
            {
                //LEDStatusLightingUtil.Instance.TurnOffAllLEDs();
                LEDStatusLightingUtil.Instance.ClosePort();
            }

            //if (DocumentScannerUtil.Instance.EnableFeeder)
            //{
            //    DocumentScannerUtil.Instance.StopScanning();
            //}

            if (DocumentScannerUtil.Instance.Scanner_Connected)
            {
                DocumentScannerUtil.Instance.Disconnect();
            }

            BarcodeScannerUtil.Instance.Disconnect();
            FacialRecognition.Instance.Dispose();

            Application.ExitThread();
            APIUtils.Dispose();
        }

        #region events
        private void JSCallCS_OnNRICFailed(object sender, NRICEventArgs e)
        {
            //MessageBox.Show(e.Message, "Authentication failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //LayerWeb.ShowMessage("Authentication failed", e.Message);
            CSCallJS.ShowMessage(LayerWeb, "Authentication failed", e.Message);
        }

        private void JSCallCS_ShowMessage(object sender, ShowMessageEventArgs e)
        {
            //MessageBox.Show(e.Message, e.Caption, e.Button, e.Icon);
            CSCallJS.ShowMessage(LayerWeb, e.Caption, e.Message);
        }

        private void OnShowMessage(object sender, ShowMessageEventArgs e)
        {
            //MessageBox.Show(e.Message, e.Caption, e.Button, e.Icon);
            CSCallJS.ShowMessage(LayerWeb, e.Caption, e.Message);
        }

        public void NavigateTo(NavigatorEnums navigatorEnum)
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
                    LayerWeb.RunScript("$('.status-text').css('color','#000').text('Please place your thumb print on the reader.');");
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
                if ((user.User_Photo1 == null || user.User_Photo1.Length == 0) && (user.User_Photo2 == null || user.User_Photo2.Length == 0))
                {
                    //Trinity.BE.PopupModel popupModel = new Trinity.BE.PopupModel();
                    //popupModel.Title = "Authorization Failed";
                    //popupModel.Message = "User '" + user.Name + "' doesn't have any photos";
                    //popupModel.IsShowLoading = false;
                    //popupModel.IsShowOK = true;

                    //LayerWeb.InvokeScript("showPopupModal", JsonConvert.SerializeObject(popupModel));

                    // Navigate to smartcard login page
                    NavigateTo(NavigatorEnums.Authentication_SmartCard);
                    return;
                }
                LayerWeb.LoadPageHtml("Authentication/FacialRecognition.html");
                LayerWeb.RunScript("$('.status-text').css('color','#000').text('Please remain still as Facial Recognition Check takes place.');");
                FacialRecognition.Instance.OnFacialRecognitionFailed += Main_OnFacialRecognitionFailed;
                FacialRecognition.Instance.OnFacialRecognitionSucceeded += Main_OnFacialRecognitionSucceeded;
                FacialRecognition.Instance.OnFacialRecognitionProcessing += Main_OnFacialRecognitionProcessing;
                FacialRecognition.Instance.OnCameraInitialized += Main_OnCameraInitialized;

                List<byte[]> FaceJpg = new System.Collections.Generic.List<byte[]>() { user.User_Photo1, user.User_Photo2 };
                this.Invoke((MethodInvoker)(() =>
                {
                    Point startLocation = new Point((Screen.PrimaryScreen.Bounds.Size.Width / 2) - 800 / 2, (Screen.PrimaryScreen.Bounds.Size.Height / 2) - 450 / 2);
                    FacialRecognition.Instance.StartFacialRecognition(startLocation, FaceJpg);
                    //LayerWeb.RunScript("$('.status-text').css('color','#000').text('Face authentication');");
                }));
            }
            else if (navigatorEnum == NavigatorEnums.Authentication_NRIC)
            {
                _nric.Start();
            }
            else if (navigatorEnum == NavigatorEnums.Supervisee)
            {
                // Handle income notifications

                Session session = Session.Instance;
                Trinity.BE.User user = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];
                if (user.Role == EnumUserRoles.Supervisee && user.Status == EnumUserStatuses.Blocked)
                {
                    LayerWeb.ShowMessageAsync("You have been blocked<br/>Contact Duty Officer for help");
                    _jsCallCS.LogOut();
                    return;
                }
                _signalrClient = Client.Instance;
                _signalrClient.OnNewNotification += _signalrClient_OnNewNotification;
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

        #region Handle Incoming Notifications
        private void _signalrClient_OnNewNotification(object sender, NotificationInfo e)
        {
            _jsCallCS.LoadNotications();
        }

        #endregion


        private void Main_Load(object sender, EventArgs e)
        {
            try
            {
                APIUtils.FormQueueNumber = FormQueueNumber.GetInstance();
                APIUtils.FormQueueNumber.ShowOnSecondaryScreen();
                APIUtils.FormQueueNumber.Show();
            }
            catch (Exception ex)
            {

                this.LayerWeb.ShowMessageAsync("Error Queue", ex.Message);
            }
        }
    }
}
