﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Trinity.Common;
using Trinity.Common.Common;
using Trinity.DAL;
using Trinity.Device;
using Trinity.Device.Authentication;

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

            APIUtils.Start();
            //Notification
            Trinity.SignalR.Client.SignalR.Instance.Event_DeviceStatusUpdate += Instance_Event_DeviceStatusUpdate;
            Trinity.SignalR.Client.SignalR.Instance.Event_MessageTo += Instance_Event_MessageTo;
            Trinity.SignalR.Client.SignalR.Instance.Event_AppDisconnect += Instance_Event_AppDisconnect;

            // setup variables
            _smartCardFailed = 0;
            _fingerprintFailed = 0;
            _displayLoginButtonStatus = false;

            #region Initialize and register events
            // _jsCallCS
            _jsCallCS = new JSCallCS(this.LayerWeb);
            _jsCallCS.OnLogOutCompleted += JSCallCS_OnLogOutCompleted;

            // SmartCard
            SmartCard.Instance.GetCardInfoSucceeded += GetCardInfoSucceeded;
            // Fingerprint
            Fingerprint.Instance.OnIdentificationCompleted += Fingerprint_OnIdentificationCompleted;
            Fingerprint.Instance.OnDeviceDisconnected += Fingerprint_OnDeviceDisconnected;

            _eventCenter = EventCenter.Default;
            _eventCenter.OnNewEvent += EventCenter_OnNewEvent;
            #endregion


            Lib.LayerWeb = LayerWeb;
            LayerWeb.Url = new Uri(String.Format("file:///{0}/View/html/Layout.html", CSCallJS.curDir));
            LayerWeb.ObjectForScripting = _jsCallCS;

        }

        private void Instance_Event_AppDisconnect(string Station)
        {
            new DAL_DeviceStatus().RemoveDevice(Station);
        }

        private void Instance_Event_MessageTo(string NotificationID, string UserId, string Subject, string Content, string notificationType, string Station)
        {
            string dutiOfficerId = ((Trinity.BE.User)Session.Instance[CommonConstants.USER_LOGIN]).UserId;
            if (dutiOfficerId != null && dutiOfficerId != "")
            {
                //Trinity.BE.Notification notification = new Trinity.BE.Notification();
                //notification.Subject = Subject;
                //notification.ToUserId = UserId;
                //notification.Content = Content;
                //notification.Source = Station;
                //notification.Type = notificationType;
                //notification.Datetime = DateTime.Now;
                //object result = JsonConvert.SerializeObject(notification, Formatting.Indented,
                //new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                //Lib.LayerWeb.Invoke((MethodInvoker)(() =>
                //{
                //    string activeTab = Lib.LayerWeb.Document.InvokeScript("getActiveTab").ToString();
                //    if (activeTab != "Alerts")
                //    {
                //        Lib.LayerWeb.InvokeScript("getRealtimeNotificationServer", result);
                //    }
                //    else
                //    {
                //        Lib.LayerWeb.InvokeScript("getNotificationInCurrentTab", result);
                //    }
                //}));
                Lib.LayerWeb.InvokeScript("ShowMessageBox", Subject);
            }
        }

        private void Instance_Event_DeviceStatusUpdate(int deviceId, EnumDeviceStatuses[] deviceStatuses, string station)
        {
            DAL_DeviceStatus device = new DAL_DeviceStatus();

            if (station == EnumStations.SSA)
            {
                JSCallCS._StationColorDevice.SSAColor = device.CheckStatusDevicesStation(station) ? EnumColors.Green : EnumColors.Red;
            }
            if (station == EnumStations.SSK)
            {
                JSCallCS._StationColorDevice.SSKColor = device.CheckStatusDevicesStation(station) ? EnumColors.Green : EnumColors.Red;
            }
            if (station == EnumStations.ESP)
            {
                JSCallCS._StationColorDevice.ESPColor = device.CheckStatusDevicesStation(station) ? EnumColors.Green : EnumColors.Red;
            }
            if (station == EnumStations.UHP)
            {
                JSCallCS._StationColorDevice.UHPColor = device.CheckStatusDevicesStation(station) ? EnumColors.Green : EnumColors.Red;
            }

            JSCallCS jSCall = new JSCallCS(Lib.LayerWeb);
            jSCall.LoadStationColorDevice();
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

        private void GetCardInfoSucceeded(string cardUID)
        {
            if (_isSmartCardToLogin)
            {
                // get local user info
                DAL_User dAL_User = new DAL_User();
                var user = dAL_User.GetUserBySmartCardId(cardUID);

                // if local user is null, get user from centralized, and sync db
                if (user == null)
                {
                    user = dAL_User.GetUserBySmartCardId(cardUID);
                    if (user != null && user.Role != EnumUserRoles.DutyOfficer)
                        user = null;
                }

                if (user != null)
                {
                    Session session = Session.Instance;
                    session.IsSmartCardAuthenticated = true;
                    Session.Instance[CommonConstants.USER_LOGIN] = user;
                    this.LayerWeb.RunScript("$('.status-text').css('color','#000').text('Your smart card is authenticated.');");
                    // Stop SCardMonitor
                    SmartCardReaderUtil.Instance.StopSmartCardMonitor();
                    // raise succeeded event
                    SmartCard_OnSmartCardSucceeded();
                }
                else
                {
                    // raise failed event
                    SmartCard_OnSmartCardFailed("Unable to read your smart card. Please report to the Duty Officer");
                }
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

            Trinity.BE.User user = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];
            if (user.Role == EnumUserRoles.DutyOfficer)
            {
                LayerWeb.RunScript("$('.status-text').css('color','#000').text('Fingerprint authentication is successful.');");
                //APIUtils.SignalR.GetLatestNotifications();

                Thread.Sleep(1000);

                Trinity.SignalR.Client.SignalR.Instance.UserLogined(((Trinity.BE.User)Session.Instance[CommonConstants.USER_LOGIN]).UserId);
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
                MessageBox.Show(message, "Authentication failed", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // navigate to smartcard login page
                NavigateTo(NavigatorEnums.Authentication_SmartCard);

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

        private void NavigateTo(NavigatorEnums navigatorEnum)
        {
            // navigate
            if (navigatorEnum == NavigatorEnums.Authentication_SmartCard)
            {
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
            }else if (navigatorEnum == NavigatorEnums.Queue)
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
            APIUtils.Dispose();
        }

        private void LayerWeb_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            LayerWeb.InvokeScript("createEvent", JsonConvert.SerializeObject(_jsCallCS.GetType().GetMethods().Where(d => (d.IsPublic && !d.IsVirtual && !d.IsSecuritySafeCritical) || (d.IsPublic && d.IsSecurityCritical)).ToArray().Select(d => d.Name)));

            //if (_isFirstTimeLoaded)
            //{
            //    NavigateTo(NavigatorEnums.Authentication_SmartCard);

            //    _isFirstTimeLoaded = false;
            //}

            //// For testing purpose
            Session session = Session.Instance;
            //// Duty Officer
            Trinity.BE.User user = new DAL_User().GetUserByUserId("dfbb2a6a-9e45-4a76-9f75-af1a7824a947").Data;
            session[CommonConstants.USER_LOGIN] = user;
            session.IsSmartCardAuthenticated = true;
            session.IsFingerprintAuthenticated = true;

            NavigateTo(NavigatorEnums.Queue);
            Trinity.SignalR.Client.SignalR.Instance.UserLogined(((Trinity.BE.User)Session.Instance[CommonConstants.USER_LOGIN]).UserId);
        }

        private void EventCenter_OnNewEvent(object sender, EventInfo e)
        {
            if (e.Name == EventNames.LOGIN_SUCCEEDED)
            {
                Trinity.SignalR.Client.SignalR.Instance.UserLogined(((Trinity.BE.User)Session.Instance[CommonConstants.USER_LOGIN]).UserId);
                NavigateTo(NavigatorEnums.Queue);
            }
            else if (e.Name.Equals(EventNames.LOGIN_FAILED))
            {
                //NavigateTo(NavigatorEnums.Authentication_NRIC);
                MessageBox.Show(e.Message, "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void JSCallCS_OnLogOutCompleted()
        {
            NavigateTo(NavigatorEnums.Authentication_SmartCard);
        }
    }
}
