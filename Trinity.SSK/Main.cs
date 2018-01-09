using Newtonsoft.Json;
using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Trinity.BE;
using Trinity.Common;
using Trinity.Common.Common;
using Trinity.DAL;

namespace SSK
{
    public partial class Main : Form
    {
        private JSCallCS _jsCallCS;
        private CodeBehind.Authentication.NRIC _nric;
        private CodeBehind.Suppervisee _suppervisee;
        private NavigatorEnums _currentPage;
        private Trinity.Common.DeviceMonitor.HealthMonitor healthMonitor;
        private EventCenter _eventCenter;
        private int _smartCardFailed;
        private int _fingerprintFailed;
        private int _facedetectFailed = 0;
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
            Trinity.Common.Authentication.SmartCard.Instance.GetCardInfoSucceeded += GetCardInfoSucceeded;
            // Fingerprint
            Trinity.Common.Authentication.Fingerprint.Instance.GetVerification += GetVerificationFingerprint;
            Trinity.Common.Authentication.Fingerprint.Instance.GetHealthMonitor += GetHealthMonitorFingerprint;


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

            //health check
            healthMonitor = Trinity.Common.DeviceMonitor.HealthMonitor.Instance;
            healthMonitor.OnHealthCheck += OnHealthMonitor;

            //for testing
            //var timer = new System.Timers.Timer(30000);

            //15 minutes
            var timer = new System.Timers.Timer(1000 * 60 * 15);

            timer.Elapsed += PeriodCheck;
            timer.Start();
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
            if (e.Name == EventNames.ALERT_NO_APPOINTMENT)
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

        }
        private void GetHealthMonitorFingerprint(bool status)
        {
            if (!status)
            {
                _fingerprintFailed = 3;
                Fingerprint_OnFingerprintFailed("The fingerprint does not work");
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

        private void PeriodCheck(object sender, System.Timers.ElapsedEventArgs e)
        {
            healthMonitor.CheckHealth();
        }

        private void OnHealthMonitor(object sender, Trinity.Common.DeviceMonitor.HealthMonitorEventArgs e)
        {
            var dalDeviceStatus = new DAL_DeviceStatus();
            var listDeviceStatusModel = new System.Collections.Generic.List<DeviceStatus>();
            try
            {

                //entry app name - lenght better be < 10 char
                var entryAppName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
                if (e != null)
                {
                    //Receipt Print Status
                    var deviceId = dalDeviceStatus.GetDeviceId(EnumDeviceTypes.ReceiptPrinter);
                    listDeviceStatusModel.Add(dalDeviceStatus.SetInfo(entryAppName, deviceId, e.PrintStatus));

                    //Smart Cart Reader Status
                    deviceId = dalDeviceStatus.GetDeviceId(EnumDeviceTypes.SmartCardReader);
                    listDeviceStatusModel.Add(dalDeviceStatus.SetInfo(entryAppName, deviceId, e.SCardStatus));

                    //Document Scanner Status
                    deviceId = dalDeviceStatus.GetDeviceId(EnumDeviceTypes.DocumentScanner);
                    listDeviceStatusModel.Add(dalDeviceStatus.SetInfo(entryAppName, deviceId, e.DocStatus));

                    //Fingerprint Scanner Status
                    deviceId = dalDeviceStatus.GetDeviceId(EnumDeviceTypes.FingerprintScanner);
                    listDeviceStatusModel.Add(dalDeviceStatus.SetInfo(entryAppName, deviceId, e.FPrintStatus));

                    dalDeviceStatus.Insert(listDeviceStatusModel);
                }
            }
            catch (Exception ex)
            {

                return;
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
                //NavigateTo(NavigatorEnums.Authentication_SmartCard);

                // For testing purpose
                Session session = Session.Instance;
                ////Supervisee
                Trinity.BE.User user = new DAL_User().GetUserByUserId("bb67863c-c330-41aa-b397-c220428ad16f", true);
                //// Duty Officer
                //Trinity.BE.User user = new DAL_User().GetUserByUserId("ead039f9-b9a1-45bb-8186-0bb7248aafac", true);
                session[CommonConstants.USER_LOGIN] = user;
                session.IsSmartCardAuthenticated = true;
                session.IsFingerprintAuthenticated = true;
                NavigateTo(NavigatorEnums.Authentication_Fingerprint);
                //NavigateTo(NavigatorEnums.Supervisee);
                //NavigateTo(NavigatorEnums.Authentication_NRIC);

                _isFirstTimeLoaded = false;
            }
        }

        private void NRIC_OnNRICSucceeded()
        {
            // navigate to Supervisee page
            NavigateTo(NavigatorEnums.Supervisee);
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
            if (user.Role == EnumUserRoles.DutyOfficer)
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

        private void FaceDetectSucceeded()
        {
            _facedetectFailed = 0;
            FacialRecognition.Instance.FaceDetectFailed -= FaceDetectFailed;
            FacialRecognition.Instance.FaceDetectSucceeded -= FaceDetectSucceeded;
            FacialRecognition.Instance.Dispose();
            Fingerprint_OnFingerprintSucceeded();
        }
        private void FaceDetectFailed()
        {
            Session session = Session.Instance;
            Trinity.BE.User user = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];
            _facedetectFailed++;
            if (_facedetectFailed == 2)
            {
                // Send Notification to duty officer
                APIUtils.SignalR.SendNotificationToDutyOfficer("Fingerprint scans and facial identification failed", "Fingerprint scans and facial identification failed");

                // show message box to user
                MessageBox.Show("Fingerprint scans and facial identification failed", "Authentication failed", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // navigate to smartcard login page
                NavigateTo(NavigatorEnums.Authentication_SmartCard);

                // reset counter
                _fingerprintFailed = 0;
                _facedetectFailed = 0;
                FacialRecognition.Instance.FaceDetectFailed -= FaceDetectFailed;
                FacialRecognition.Instance.FaceDetectSucceeded -= FaceDetectSucceeded;
                FacialRecognition.Instance.Dispose();
            }
            else if (_facedetectFailed < 2 && user.User_Photo1 != null && user.User_Photo2 != null)
            {
                FacialRecognition.Instance.Compare(user.User_Photo2);
            }
            else
            {
                FaceDetectFailed();
            }
        }
        private void Fingerprint_OnFingerprintFailed(string message)
        {
            // increase counter
            _fingerprintFailed++;

            // exceeded max failed
            if (_fingerprintFailed > 3)
            {
                //#region Remove Khi có Face Scan
                //// Send Notification to duty officer
                //APIUtils.SignalR.SendNotificationToDutyOfficer("Fingerprint scans and facial identification failed", "Fingerprint scans and facial identification failed");

                //// show message box to user
                //MessageBox.Show("Fingerprint scans and facial identification failed", "Authentication failed", MessageBoxButtons.OK, MessageBoxIcon.Error);

                //// navigate to smartcard login page
                //NavigateTo(NavigatorEnums.Authentication_SmartCard);

                //// reset counter
                //_fingerprintFailed = 0;
                //#endregion
                Session session = Session.Instance;
                Trinity.BE.User user = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];
                if (user.User_Photo1 == null && user.User_Photo2 == null)
                {
                    _facedetectFailed++;
                    FaceDetectFailed();
                }
                else
                {
                    FacialRecognition.Instance.FaceDetectFailed += FaceDetectFailed;
                    FacialRecognition.Instance.FaceDetectSucceeded += FaceDetectSucceeded;
                    FacialRecognition.Instance.Compare(user.User_Photo1 ?? user.User_Photo2);
                }
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
