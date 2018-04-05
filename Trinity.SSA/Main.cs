using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Trinity.Common;
using Trinity.Common.Common;
using Trinity.DAL;
using Trinity.Device.Authentication;
using Trinity.Device.Util;

namespace SSA
{
    public partial class Main : Form
    {
        private JSCallCS _jsCallCS;
        private EventCenter _eventCenter;
        private CodeBehind.Authentication.NRIC _nric;
        private CodeBehind.SupperviseeParticulars _supperviseeParticulars;
        private NavigatorEnums _currentPage;

        private int _smartCardFailed;
        private int _fingerprintFailed;
        private bool _displayLoginButtonStatus = false;
        private bool _isFirstTimeLoaded = true;
        private Trinity.BE.PopupModel _popupModel;

        public Main()
        {
            InitializeComponent();

            APIUtils.Start();
            //Notification
            Trinity.SignalR.Client signalrClient = Trinity.SignalR.Client.Instance;
            // setup variables
            _smartCardFailed = 0;
            _fingerprintFailed = 0;
            _displayLoginButtonStatus = false;
            _popupModel = new Trinity.BE.PopupModel();

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
            _supperviseeParticulars = new CodeBehind.SupperviseeParticulars(LayerWeb);

            _eventCenter = EventCenter.Default;
            _eventCenter.OnNewEvent += EventCenter_OnNewEvent;
            #endregion


            Lib.LayerWeb = LayerWeb;
            LayerWeb.Url = new Uri(String.Format("file:///{0}/View/html/Layout.html", CSCallJS.curDir));
            LayerWeb.ObjectForScripting = _jsCallCS;

        }
        private void Fingerprint_OnDeviceDisconnected()
        {
            // set message
            string message = "The fingerprint reader is not connected, please report to the Duty Officer!";

            // Send Notification to duty officer
            Trinity.SignalR.Client.Instance.SendToAppDutyOfficers(null, "The fingerprinter is not connected", "The fingerprinter is not connected.", EnumNotificationTypes.Error);

            // show message box to user
            MessageBox.Show(message, "Authentication failed!", MessageBoxButtons.OK, MessageBoxIcon.Error);

            // navigate to smartcard login page
            //NavigateTo(NavigatorEnums.Authentication_SmartCard);
            NavigateTo(NavigatorEnums.Authentication_Facial);
        }

        private void Fingerprint_OnIdentificationCompleted(bool bVerificationSuccess)
        {
            if (!bVerificationSuccess)
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
            //MessageBox.Show(cardUID);
            // get local user info
            DAL_User dAL_User = new DAL_User();
            var user = dAL_User.GetUserBySmartCardId(cardUID);

            // if local user is null, get user from centralized, and sync db
            //if (user == null)
            //{
            //    user = dAL_User.GetUserBySmartCardId(cardUID);
            //}

            if (user != null)
            {
                if (user.Role == EnumUserRoles.Supervisee || user.Role == EnumUserRoles.DutyOfficer)
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
                    SmartCardReaderUtil.Instance.StopSmartCardMonitor();
                    // raise succeeded event
                    SmartCard_OnSmartCardSucceeded();
                    //MessageBox.Show("SmartCard_OnSmartCardSucceeded");
                }
                else
                {
                    SmartCard_OnSmartCardFailed("You do not have permission to access this page");
                }
            }
            else
            {
                // raise failed event
                SmartCard_OnSmartCardFailed("Unable to read your smart card. Please report to the Duty Officer");
                //SmartCard_OnSmartCardSucceeded();
            }
        }

        private void Main_Load(object sender, EventArgs e)
        {
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                // Turn off all LED(s)
                if (LEDStatusLightingUtil.Instance.IsPortOpen)
                {
                    //LEDStatusLightingUtil.Instance.TurnOffAllLEDs();
                    LEDStatusLightingUtil.Instance.ClosePort();
                }

                FacialRecognition.Instance.Dispose();

                BarcodeScannerUtil.Instance.Disconnect();

                //_autoPrintTimer.Enabled = false;
                Application.ExitThread();
                APIUtils.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void LayerWeb_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            LayerWeb.InvokeScript("createEvent", JsonConvert.SerializeObject(_jsCallCS.GetType().GetMethods().Where(d => d.IsPublic && !d.IsVirtual && !d.IsSecuritySafeCritical).ToArray().Select(d => d.Name)));

            if (_isFirstTimeLoaded)
            {
                // Start page
                //NavigateTo(NavigatorEnums.Authentication_SmartCard);

                string startFrom = "";
                // 50.132
                //string superviseeId = "06a91b1b-99c3-428d-8a55-83892c2adf4c";
                //string dutyOfficerId = "f1748cb4-3bb5-4129-852d-2aba28bb8cec";
                // 1.120
                string superviseeId = "9043d88e-94d1-4c01-982a-02d41965a621";
                string dutyOfficerId = "f1748cb4-3bb5-4129-852d-2aba28bb8cec";
                Session session = Session.Instance;

                if (startFrom == "Supervisee_Particulars")
                {
                    Trinity.BE.User user = new DAL_User().GetUserByUserId(superviseeId).Data;
                    session[CommonConstants.USER_LOGIN] = user;
                    session.IsSmartCardAuthenticated = true;
                    session.IsFingerprintAuthenticated = true;
                    NavigateTo(NavigatorEnums.Supervisee_Particulars);
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

                _isFirstTimeLoaded = false;
            }
            // SSA is ready to use - all is well
            // Turn on GREEN Light
            if (LEDStatusLightingUtil.Instance.IsPortOpen)
            {
                LEDStatusLightingUtil.Instance.TurnOffAllLEDs();
                LEDStatusLightingUtil.Instance.SwitchGREENLightOnOff(true);
            }
        }

        private void _autoPrintTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _jsCallCS.CheckPrintingAndLabellingProgress();
        }

        private void JSCallCS_OnLogOutCompleted()
        {
            NavigateTo(NavigatorEnums.Authentication_SmartCard);
        }

        private void NRIC_OnNRICSucceeded()
        {
            // navigate to Supervisee page
            Trinity.SignalR.Client.Instance.UserLoggedIn(((Trinity.BE.User)Session.Instance[CommonConstants.USER_LOGIN]).UserId);

            NavigateTo(NavigatorEnums.Supervisee_NRIC);
        }

        #region Smart Card Authentication Event Handlers

        private void SmartCard_OnSmartCardSucceeded()
        {
            // Pause for 1 second and goto Fingerprint Login Screen
            Thread.Sleep(400);

            // navigate to next page: Authentication_Fingerprint
            NavigateTo(NavigatorEnums.Authentication_Fingerprint);

            // For testing purpose only
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
                Trinity.SignalR.Client.Instance.SendToAppDutyOfficers(null, message, message, EnumNotificationTypes.Error);

                // show message box to user
                //MessageBox.Show(message, "Authentication failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _popupModel.Title = "Authorization Failed";
                _popupModel.Message = message;
                _popupModel.IsShowLoading = false;
                _popupModel.IsShowOK = true;

                LayerWeb.InvokeScript("showPopupModal", JsonConvert.SerializeObject(_popupModel));

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

        #region Fingerprint Authentication Event Handlers
        private void Fingerprint_OnFingerprintSucceeded()
        {
            try
            {


                //
                // Login successfully
                //
                // Create a session object to store UserLogin information
                Session session = Session.Instance;
                session.IsFingerprintAuthenticated = true;

                LayerWeb.RunScript("$('.status-text').css('color','#000').text('Fingerprint authentication is successful.');");

                Thread.Sleep(200);

                // if role = 0 (duty officer), redirect to NRIC.html
                // else (supervisee), redirect to Supervisee.html
                Trinity.BE.User currentUser = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];
                if (currentUser.Role == EnumUserRoles.DutyOfficer)
                {
                    // navigate to Authentication_NRIC
                    NavigateTo(NavigatorEnums.Authentication_NRIC);
                }
                else if (currentUser.Role == EnumUserRoles.Supervisee)
                {
                    // navigate to SuperviseeParticulars page
                    Trinity.SignalR.Client.Instance.UserLoggedIn(currentUser.UserId);
                    NavigateTo(NavigatorEnums.Supervisee_Particulars);
                }
                else
                {
                    _popupModel.Title = "Login Failed";
                    _popupModel.Message = "You do not have permission to access this page.";
                    _popupModel.IsShowLoading = false;
                    _popupModel.IsShowOK = true;
                    LayerWeb.InvokeScript("showPopupModal", JsonConvert.SerializeObject(_popupModel));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in Fingerprint_OnFingerprintSucceeded. Details:" + ex.Message);
            }
        }

        private void Fingerprint_OnFingerprintFailed()
        {
            // Increase failed counter
            _fingerprintFailed++;

            Trinity.BE.User user = (Trinity.BE.User)Session.Instance[CommonConstants.USER_LOGIN];

            // exceeded max failed
            if (_fingerprintFailed > 3)
            {
                // set message
                string errorMessage = "Unable to read " + user.Name + "'s fingerprint.";

                // Send Notification to duty officer
                Trinity.SignalR.Client.Instance.SendToAppDutyOfficers(user.UserId, "Fingerprint Authentication failed", errorMessage, EnumNotificationTypes.Error);

                //Trinity.BE.PopupModel popupModel = new Trinity.BE.PopupModel();
                //popupModel.Title = "Authorization Failed";
                //popupModel.Message = "Unable to read your fingerprint.\nPlease report to the Duty Officer";
                //popupModel.IsShowLoading = false;
                //popupModel.IsShowOK = true;

                //LayerWeb.InvokeScript("showPopupModal", JsonConvert.SerializeObject(popupModel));

                // Reset failed count
                _fingerprintFailed = 0;

                // Navigate to next page: Facial Authentication
                // Facial SDK has a trouble. Bypass temporarily
                NavigateTo(NavigatorEnums.Authentication_Facial);
                //NavigateTo(NavigatorEnums.Authentication_SmartCard);
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

        #region Facial Authentication Event Handlers
        private void Main_OnFacialRecognitionSucceeded()
        {
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

            Thread.Sleep(200);

            // if role = 0 (duty officer), redirect to NRIC.html
            // else (supervisee), redirect to Supervisee.html
            Trinity.BE.User currentUser = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];
            if (currentUser.Role == EnumUserRoles.DutyOfficer)
            {
                // navigate to Authentication_NRIC
                NavigateTo(NavigatorEnums.Authentication_NRIC);
            }
            else if (currentUser.Role == EnumUserRoles.Supervisee)
            {
                // navigate to SuperviseeParticulars page
                Trinity.SignalR.Client.Instance.UserLoggedIn(currentUser.UserId);
                NavigateTo(NavigatorEnums.Supervisee_Particulars);
            }
            else
            {
                _popupModel.Title = "Login Failed";
                _popupModel.Message = "You do not have permission to access this page.";
                _popupModel.IsShowLoading = false;
                _popupModel.IsShowOK = true;
                LayerWeb.InvokeScript("showPopupModal", JsonConvert.SerializeObject(_popupModel));
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
            Trinity.SignalR.Client.Instance.SendToAppDutyOfficers(user.UserId, "Facial authentication failed", errorMessage, EnumNotificationTypes.Error);

            // show message box to user
            //MessageBox.Show("Facial authentication failed", "Facial Authentication", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Trinity.BE.PopupModel popupModel = new Trinity.BE.PopupModel();
            popupModel.Title = "Authorization Failed";
            popupModel.Message = "Facial Recognition failed.\nPlease report to the Duty Officer";
            popupModel.IsShowLoading = false;
            popupModel.IsShowOK = true;

            LayerWeb.InvokeScript("showPopupModal", JsonConvert.SerializeObject(popupModel));

            // navigate to smartcard login page
            NavigateTo(NavigatorEnums.Authentication_SmartCard);

            // reset counter
            _fingerprintFailed = 0;

        }

        #endregion

        private void EventCenter_OnNewEvent(object sender, EventInfo e)
        {
            if (e.Name == EventNames.LOGIN_SUCCEEDED)
            {

                NavigateTo(NavigatorEnums.Authentication_NRIC);
            }
            else if (e.Name.Equals(EventNames.LOGIN_FAILED))
            {
                //NavigateTo(NavigatorEnums.Authentication_NRIC);
                //MessageBox.Show(e.Message, "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _popupModel.Title = "Login Failed";
                _popupModel.Message = e.Message;
                _popupModel.IsShowLoading = false;
                _popupModel.IsShowOK = true;
                LayerWeb.InvokeScript("showPopupModal", JsonConvert.SerializeObject(_popupModel));
            }
        }

        #region events
        private void JSCallCS_OnNRICFailed(object sender, NRICEventArgs e)
        {
            //MessageBox.Show(e.Message, "Authentication failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //_popupModel.Title = "Authentication Failed";
            //_popupModel.Message = e.Message;
            //_popupModel.IsShowLoading = false;
            //_popupModel.IsShowOK = true;
            //LayerWeb.InvokeScript("showPopupModal", JsonConvert.SerializeObject(_popupModel));

            CSCallJS.ShowMessage(LayerWeb, "Authentication Failed", e.Message);
        }

        private void JSCallCS_ShowMessage(object sender, ShowMessageEventArgs e)
        {
            //MessageBox.Show(e.Message, e.Caption, e.Button, e.Icon);
            //_popupModel.Title = e.Caption;
            //_popupModel.Message = e.Message;
            //_popupModel.IsShowLoading = false;
            //_popupModel.IsShowOK = true;
            //LayerWeb.InvokeScript("showPopupModal", JsonConvert.SerializeObject(_popupModel));
            CSCallJS.ShowMessage(LayerWeb, e.Caption, e.Message);
        }

        private void OnShowMessage(object sender, ShowMessageEventArgs e)
        {
            //MessageBox.Show(e.Message, e.Caption, e.Button, e.Icon);
            //_popupModel.Title = e.Caption;
            //_popupModel.Message = e.Message;
            //_popupModel.IsShowLoading = false;
            //_popupModel.IsShowOK = true;
            //LayerWeb.InvokeScript("showPopupModal", JsonConvert.SerializeObject(_popupModel));

            CSCallJS.ShowMessage(LayerWeb, e.Caption, e.Message);
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
                Session session = Session.Instance;
                Trinity.BE.User user = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];
                if (user == null)
                {
                    MessageBox.Show("Tai sao user lai null");
                }
                LayerWeb.LoadPageHtml("Authentication/FingerPrint.html");
                LayerWeb.RunScript("$('.status-text').css('color','#000').text('Please place your thumb print on the reader.');");
                //if (user.LeftThumbFingerprint == null && user.RightThumbFingerprint == null)
                //{
                //    LayerWeb.RunScript("$('.status-text').css('color','#000').text('Please place your thumb print on the reader.');");
                //}
                //else
                //{
                //    string errMsg = user.Name + "'s Fingerprint could not be found.";
                //    LayerWeb.RunScript("$('.status-text').css('color','#000').text('" + errMsg + "');");
                //}
                try
                {
                    Fingerprint.Instance.Start(new System.Collections.Generic.List<byte[]>() { user.LeftThumbFingerprint, user.RightThumbFingerprint });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else if (navigatorEnum == NavigatorEnums.Authentication_Facial)
            {
                Session session = Session.Instance;
                Trinity.BE.User user = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];
                if ((user.User_Photo1 == null || user.User_Photo1.Length == 0) && (user.User_Photo2 == null || user.User_Photo2.Length == 0))
                {
                    Trinity.BE.PopupModel popupModel = new Trinity.BE.PopupModel();
                    popupModel.Title = "Authorization Failed";
                    popupModel.Message = "User '" + user.Name + "' doesn't have any photos";
                    popupModel.IsShowLoading = false;
                    popupModel.IsShowOK = true;

                    LayerWeb.InvokeScript("showPopupModal", JsonConvert.SerializeObject(popupModel));

                    // navigate to smartcard login page
                    NavigateTo(NavigatorEnums.Authentication_SmartCard);
                    return;
                }
                LayerWeb.LoadPageHtml("Authentication/FacialRecognition.html");
                LayerWeb.RunScript("$('.status-text').css('color','#000').text('Please remain still as Facial Recognition Check takes place.');");
                FacialRecognition.Instance.OnFacialRecognitionFailed += Main_OnFacialRecognitionFailed;
                FacialRecognition.Instance.OnFacialRecognitionSucceeded += Main_OnFacialRecognitionSucceeded;
                FacialRecognition.Instance.OnFacialRecognitionProcessing += Main_OnFacialRecognitionProcessing;

                this.Invoke((MethodInvoker)(() =>
                {
                    Point startLocation = new Point((Screen.PrimaryScreen.Bounds.Size.Width / 2) - 400 / 2, (Screen.PrimaryScreen.Bounds.Size.Height / 2) - 400 / 2);
                    try
                    {
                        FacialRecognition.Instance.StartFacialRecognition(startLocation, new System.Collections.Generic.List<byte[]>() { user.User_Photo1, user.User_Photo2 });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }));
            }
            else if (navigatorEnum == NavigatorEnums.Authentication_NRIC)
            {
                _nric.Start();
            }
            else if (navigatorEnum == NavigatorEnums.Supervisee_Particulars)
            {
                _supperviseeParticulars.Start();
                btnConfirm.Enabled = true;
            }
            else if (navigatorEnum == NavigatorEnums.Supervisee_NRIC)
            {
                _supperviseeParticulars.Start();
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
    }
}
