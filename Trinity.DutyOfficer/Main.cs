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
using Trinity.Common.Utils;
using Trinity.DAL;
using Trinity.Device;
using Trinity.Device.Authentication;
using Trinity.Device.Util;

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
        private bool _isFirstTimeLoaded = true;
        private bool _isSmartCardToLogin = false;

        public Main()
        {
            InitializeComponent();
            //Notification
            Trinity.SignalR.Client.Instance.OnDeviceStatusChanged += OnDeviceStatusChanged_Handler;
            Trinity.SignalR.Client.Instance.OnNewNotification += OnNewNotification_Handler;
            Trinity.SignalR.Client.Instance.OnAppDisconnected += OnAppDisconnected_Handler;
            Trinity.SignalR.Client.Instance.OnQueueCompleted += OnQueueCompleted_Handler;
            Trinity.SignalR.Client.Instance.OnQueueInserted += OnQueueInserted_Handler;
            Trinity.SignalR.Client.Instance.OnAppointmentBooked += OnAppointmentBooked_Handler;
            Trinity.SignalR.Client.Instance.OnAppointmentReported += OnAppointmentReported_Handler;
            Trinity.SignalR.Client.Instance.OnSSACompleted += OnSSACompleted_Handler;
            Trinity.SignalR.Client.Instance.OnSSAPrintingLabel += OnSSAPrintingLabel_Handler;
            Trinity.SignalR.Client.Instance.OnBackendAPISend += OnBackendAPISend_Handler;

            // setup variables
            _smartCardFailed = 0;
            _fingerprintFailed = 0;
            _displayLoginButtonStatus = false;

            #region Initialize and register events
            // _jsCallCS
            _jsCallCS = new JSCallCS(this.LayerWeb);
            _jsCallCS.OnLogOutCompleted += JSCallCS_OnLogOutCompleted;

            SmartCard.Instance.GetCardInfoSucceeded += GetCardInfoSucceeded;
            SmartCard.Instance.Start();

            // SmartCard
            //SmartCard.Instance.GetCardInfoSucceeded += GetCardInfoSucceeded;
            // Fingerprint
            //Fingerprint.Instance.OnIdentificationCompleted += Fingerprint_OnIdentificationCompleted;
            //Fingerprint.Instance.OnDeviceDisconnected += Fingerprint_OnDeviceDisconnected;

            _eventCenter = EventCenter.Default;
            _eventCenter.OnNewEvent += EventCenter_OnNewEvent;
            #endregion


            Lib.LayerWeb = LayerWeb;
            LayerWeb.Url = new Uri(String.Format("file:///{0}/View/html/Layout.html", CSCallJS.curDir));
            LayerWeb.ObjectForScripting = _jsCallCS;
        }
        private void GetCardInfoSucceeded(string cardUID)
        {
            if (!_isFirstTimeLoaded)
            {
                Lib.LayerWeb.Invoke((MethodInvoker)(() =>
                {
                    if (GetCurrentTab() == EnumDOTabName.Queue)
                    {
                        DAL_User dAL_User = new DAL_User();
                        Trinity.BE.User user = dAL_User.GetUserBySmartCardId(cardUID);
                        if (user != null)
                        {
                            var queue = new DAL_QueueNumber().GetMyQueueToday(user.UserId);
                            if (queue != null && queue.QueueDetails.Any(d => d.Station == EnumStation.DUTYOFFICER && d.Status == EnumQueueStatuses.TabSmartCard))
                            {
                                if (_jsCallCS.GetResultUT(user.NRIC, DateTime.Now.Date) == EnumUTResult.NEG && queue.QueueDetails.Any(d => d.Station == EnumStation.SSP && d.Status == EnumQueueStatuses.NotRequired))
                                {
                                    new DAL_QueueNumber().UpdateQueueStatusByUserId(user.UserId, EnumStation.DUTYOFFICER, EnumQueueStatuses.Finished, EnumStation.DUTYOFFICER, EnumQueueStatuses.Finished, string.Empty, EnumQueueOutcomeText.UnconditionalRelease);
                                    this.LayerWeb.InvokeScript("reloadDataQueues");
                                }
                                else
                                {
                                    this.LayerWeb.LoadPopupHtml("QueuePopupOutcome.html", new {
                                        UserId = user.UserId,
                                        Name = user.Name
                                    });
                                }
                            }
                        }
                    }
                }));
            }
        }

        private void OnBackendAPISend_Handler(object sender, NotificationInfo e)
        {
            // khi ở tab Queue nhận đc cái này sẽ tải lại
            RefreshCurrentTab(EnumDOTabName.Queue);
            if (e.Name == NotificationNames.SSP_COMPLETED)
            {
                RefreshCurrentTab(EnumDOTabName.UBLabel);
            }
        }
        private void OnBackendApiSendDO_Handler(object sender, NotificationInfo e)
        {
            // khi ở tab Alter nhận đc cái này sẽ tải lại
            RefreshCurrentTab(EnumDOTabName.Alerts);
        }

        private void OnSSACompleted_Handler(object sender, NotificationInfo e)
        {
            //string userId = e.UserID;
            // Refresh data queue or MUB/TT Labels
            RefreshCurrentTab(EnumDOTabName.Queue);
            RefreshCurrentTab(EnumDOTabName.MUBLabel);
        }
        private void OnSSAPrintingLabel_Handler(object sender, NotificationInfo e)
        {
            RefreshCurrentTab(EnumDOTabName.MUBLabel);
        }
        private void OnAppointmentBooked_Handler(object sender, NotificationInfo e)
        {
            //string AppointmentID = e.AppointmentID;
            //string Status = e.Status;
            RefreshCurrentTab(EnumDOTabName.Appointments);
            RefreshCurrentTab(EnumDOTabName.Statistics);
        }
        private void OnAppointmentReported_Handler(object sender, NotificationInfo e)
        {
            //string AppointmentID = e.AppointmentID;
            //string Status = e.Status;
            RefreshCurrentTab(EnumDOTabName.Appointments);
            RefreshCurrentTab(EnumDOTabName.Statistics);
        }
        private void OnQueueInserted_Handler(object sender, NotificationInfo e)
        {
            //string QueueID = e.QueueID;
            RefreshCurrentTab(EnumDOTabName.Queue);
            RefreshCurrentTab(EnumDOTabName.MUBLabel);
        }


        private void OnAppDisconnected_Handler(object sender, EventInfo e)
        {
            string station = (string)e.Source;
            //new DAL_DeviceStatus().RemoveDevice(station);

            LayerWeb.InvokeScript("SetStationColorDevice", JsonConvert.SerializeObject(_jsCallCS.GetStationClolorDevice()));

        }

        private void OnNewNotification_Handler(object sender, NotificationInfo e)
        {
            var user = ((Trinity.BE.User)Session.Instance[CommonConstants.USER_LOGIN]);
            if (user != null)
            {
                string dutyOfficerId = user.UserId;
                string toDutyOfficerId = e.ToUserIds != null ? e.ToUserIds[0] : dutyOfficerId;
                if (!string.IsNullOrEmpty(dutyOfficerId) && e.Name == NotificationNames.ALERT_MESSAGE)
                {
                    Trinity.BE.Notification notification = new Trinity.BE.Notification();
                    notification.Subject = e.Subject;
                    notification.ToUserId = toDutyOfficerId;
                    notification.Content = e.Content;
                    notification.Source = (string)e.Source;
                    notification.Type = e.Type;
                    notification.Datetime = DateTime.Now;
                    notification.NotificationID = e.NotificationID;

                    object result = JsonConvert.SerializeObject(notification, Formatting.Indented,
                    new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                    RefreshCurrentTab(EnumDOTabName.Alerts, result);
                }
            }

        }

        private void OnDeviceStatusChanged_Handler(object sender, EventInfo e)
        {
            string station = (string)e.Source;
            if (!string.IsNullOrEmpty(station))
            {
                //DAL_DeviceStatus device = new DAL_DeviceStatus();

                //if (station == EnumStation.ALK)
                //{
                //    JSCallCS._StationColorDevice.SSAColor = device.CheckStatusDevicesStation(station);
                //}
                //if (station == EnumStation.ARK)
                //{
                //    JSCallCS._StationColorDevice.SSKColor = device.CheckStatusDevicesStation(station);
                //}
                //if (station == EnumStation.SSP)
                //{
                //    JSCallCS._StationColorDevice.ESPColor = device.CheckStatusDevicesStation(station);
                //}
                //if (station == EnumStation.SHP)
                //{
                //    JSCallCS._StationColorDevice.UHPColor = device.CheckStatusDevicesStation(station);
                //}
                LogManager.Info("OnDeviceStatusChanged_Handler: " + station);

                LayerWeb.InvokeScript("SetStationColorDevice",JsonConvert.SerializeObject(_jsCallCS.GetStationClolorDevice()));
            }
            else
            {
                LogManager.Info("OnDeviceStatusChanged_Handler: Emty");
            }
        }

        private void OnQueueCompleted_Handler(object sender, EventInfo e)
        {
            // Refresh data queue
            RefreshCurrentTab(EnumDOTabName.Queue);
        }

        private void Fingerprint_OnDeviceDisconnected()
        {
            LayerWeb.RunScript("alert('The fingerprint does not work');");
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

        //private void GetCardInfoSucceeded(string cardUID)
        //{
        //    if (_isSmartCardToLogin)
        //    {
        //        // get local user info
        //        DAL_User dAL_User = new DAL_User();
        //        var user = dAL_User.GetUserBySmartCardId(cardUID);

        //        if (user != null)
        //        {
        //            if (user.Role == EnumUserRoles.DutyOfficer)
        //            {
        //                Session session = Session.Instance;
        //                session.IsSmartCardAuthenticated = true;
        //                Session.Instance[CommonConstants.USER_LOGIN] = user;
        //                this.LayerWeb.RunScript("$('.status-text').css('color','#000').text('Your smart card is authenticated.');");
        //                // Stop SCardMonitor
        //                SmartCardReaderUtil.Instance.StopSmartCardMonitor();
        //                // raise succeeded event
        //                SmartCard_OnSmartCardSucceeded();
        //            }
        //            else
        //            {
        //                SmartCard_OnSmartCardFailed("You do not have permission to access this page");
        //            }
        //        }
        //        else
        //        {
        //            // raise failed event
        //            SmartCard_OnSmartCardFailed("Unable to read your smart card");
        //        }
        //    }
        //}

        private void Fingerprint_OnFingerprintSucceeded()
        {
            _fingerprintFailed = 0;
            //
            // Login successfully
            //
            // Create a session object to store UserLogin information
            Session session = Session.Instance;
            session.IsFingerprintAuthenticated = true;

            Trinity.BE.User user = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];
            if (user.Role == EnumUserRoles.DutyOfficer)
            {
                LayerWeb.RunScript("$('.status-text').css('color','#000').text('Fingerprint authentication is successful.');");
                //APIUtils.SignalR.GetLatestNotifications();

                Trinity.SignalR.Client.Instance.UserLoggedIn(((Trinity.BE.User)Session.Instance[CommonConstants.USER_LOGIN]).UserId);
                Thread.Sleep(400);
                NavigateTo(NavigatorEnums.Queue);
            }
            else
            {
                MessageBox.Show("You do not have permission to access this page.");
                //Logout();
                session.IsSmartCardAuthenticated = false;
                session.IsFingerprintAuthenticated = false;
                session[CommonConstants.USER_LOGIN] = null;
                session[CommonConstants.PROFILE_DATA] = null;
                NavigateTo(NavigatorEnums.Authentication_SmartCard);
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
                //APIUtils.SignalR.SendNotificationToDutyOfficer(message, message);

                // show message box to user
                //MessageBox.Show(message, "Authentication failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                CSCallJS.ShowMessage(LayerWeb, "Authentication Failed", message);

                // reset counter
                _smartCardFailed = 0;

                // display failed on UI
                LayerWeb.RunScript("$('.status-text').css('color','#000').text('Please place your smart card on the reader.');");

                return;
            }

            // display failed on UI
            LayerWeb.RunScript("$('.status-text').css('color','#000').text('Please place your smart card on the reader. Failed: " + _smartCardFailed + "');");
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
                //string message = "Unable to read your fingerprint. Please report to the Duty Officer";
                string message = "Unable to read " + user.Name + "'s fingerprint.";
                // Send Notification to duty officer
                //APIUtils.SignalR.SendNotificationToDutyOfficer(message, message);

                // show message box to user
                //MessageBox.Show(message, "Authentication failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                CSCallJS.ShowMessage(LayerWeb, "Authentication Failed", message);

                // navigate to smartcard login page
                NavigateTo(NavigatorEnums.Authentication_Facial);

                // reset counter
                _fingerprintFailed = 0;

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
                session[CommonConstants.SUPERVISEE] = user;
                session[CommonConstants.USER_LOGIN] = null;
                // navigate to SuperviseeParticulars page
                Trinity.SignalR.Client.Instance.UserLoggedIn(user.UserId);
                NavigateTo(NavigatorEnums.Supervisee_Particulars);
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
            MessageBox.Show("Facial Recognition failed.\nPlease report to the Duty Officer", "Authentication Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);

            // navigate to smartcard login page
            NavigateTo(NavigatorEnums.Authentication_SmartCard);

            // reset counter
            _fingerprintFailed = 0;

        }

        #endregion

        private void NavigateTo(NavigatorEnums navigatorEnum)
        {
            // navigate
            if (navigatorEnum == NavigatorEnums.Login)
            {
                LayerWeb.LoadPageHtml("Login.html");
            }
            else if (navigatorEnum == NavigatorEnums.Authentication_SmartCard)
            {
                _fingerprintFailed = 0;
                _smartCardFailed = 0;
                _isSmartCardToLogin = true;
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
                LayerWeb.LoadPageHtml("Authentication/FacialRecognition.html");
                LayerWeb.RunScript("$('.status-text').css('color','#000').text('Please remain still as Facial Recognition Check takes place.');");
                FacialRecognition.Instance.OnFacialRecognitionFailed += Main_OnFacialRecognitionFailed;
                FacialRecognition.Instance.OnFacialRecognitionSucceeded += Main_OnFacialRecognitionSucceeded;
                FacialRecognition.Instance.OnFacialRecognitionProcessing += Main_OnFacialRecognitionProcessing;
                if (user.User_Photo1 == null || user.User_Photo1.Length == 0)
                {
                    MessageBox.Show("User photo is null");
                    return;
                }
                this.Invoke((MethodInvoker)(() =>
                {
                    Point startLocation = new Point((Screen.PrimaryScreen.Bounds.Size.Width / 2) - 800 / 2, (Screen.PrimaryScreen.Bounds.Size.Height / 2) - 450 / 2);
                    FacialRecognition.Instance.StartFacialRecognition(startLocation, new System.Collections.Generic.List<byte[]>() { user.User_Photo1, user.User_Photo2 });
                }));
            }
            else if (navigatorEnum == NavigatorEnums.Queue)
            {
                _isSmartCardToLogin = false;
                LayerWeb.LoadPageHtml("Queue.html");
            }


            // set current page
            _currentPage = navigatorEnum;

            // display options in Authentication_SmartCard page
            if (_currentPage == NavigatorEnums.Authentication_SmartCard)
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
        }

        private void LayerWeb_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            LogManager.Debug("Document Completed");
            if (_isFirstTimeLoaded)
            {
                LayerWeb.InvokeScript("createEvent", JsonConvert.SerializeObject(_jsCallCS.GetType().GetMethods().Where(d => (d.IsPublic && !d.IsVirtual && !d.IsSecuritySafeCritical) || (d.IsPublic && d.IsSecurityCritical)).ToArray().Select(d => d.Name)));
                NavigateTo(NavigatorEnums.Login);
                _isFirstTimeLoaded = false;
            }

            // For testing purpose
            //Session session = Session.Instance;
            //// Duty Officer
            //Trinity.BE.User user = new DAL_User().GetUserByUserId("dfbb2a6a-9e45-4a76-9f75-af1a7824a947").Data;
            //session[CommonConstants.USER_LOGIN] = user;
            //session.IsSmartCardAuthenticated = true;
            //session.IsFingerprintAuthenticated = true;

            //NavigateTo(NavigatorEnums.Queue);
            //Trinity.SignalR.Client.Instance.UserLoggedIn(((Trinity.BE.User)Session.Instance[CommonConstants.USER_LOGIN]).UserId);
        }

        private void EventCenter_OnNewEvent(object sender, EventInfo e)
        {
            if (e.Name == EventNames.LOGIN_SUCCEEDED)
            {
                Trinity.SignalR.Client.Instance.UserLoggedIn(((Trinity.BE.User)Session.Instance[CommonConstants.USER_LOGIN]).UserId);
                NavigateTo(NavigatorEnums.Queue);
                LayerWeb.InvokeScript("getRealtimeNotificationServer");
            }
            else if (e.Name.Equals(EventNames.LOGIN_FAILED))
            {
                //NavigateTo(NavigatorEnums.Authentication_NRIC);
                MessageBox.Show(e.Message, "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void JSCallCS_OnLogOutCompleted()
        {
            NavigateTo(NavigatorEnums.Login);
        }

        private void RefreshCurrentTab(string tabName, object dataForAlert = null)
        {
            Lib.LayerWeb.Invoke((MethodInvoker)(() =>
            {
                if (GetCurrentTab() == tabName)
                {
                    switch (tabName)
                    {
                        case EnumDOTabName.Queue:
                            Lib.LayerWeb.InvokeScript("reloadDataQueues");
                            break;
                        case EnumDOTabName.Appointments:
                            Lib.LayerWeb.InvokeScript("reloadDataAppts");
                            break;
                        case EnumDOTabName.Statistics:
                            Lib.LayerWeb.InvokeScript("reloadDataStatistics");
                            break;
                        case EnumDOTabName.Blocked:
                            Lib.LayerWeb.InvokeScript("reloadDataBlocked");
                            break;
                        case EnumDOTabName.MUBLabel:
                            Lib.LayerWeb.InvokeScript("reloadDataMUBAndTT");
                            break;
                        case EnumDOTabName.Alerts:
                            Lib.LayerWeb.InvokeScript("getNotificationInCurrentTab", dataForAlert);
                            break;
                        case EnumDOTabName.UBLabel:
                            Lib.LayerWeb.InvokeScript("getDataAllUBlabels");
                            break;
                    }
                }
                else if (tabName == EnumDOTabName.Alerts)
                {
                    Lib.LayerWeb.InvokeScript("getRealtimeNotificationServer", dataForAlert);
                }
            }));
        }
        private string GetCurrentTab()
        {
            return Lib.LayerWeb.Document.InvokeScript("getActiveTab").ToString();
        }
    }
}
