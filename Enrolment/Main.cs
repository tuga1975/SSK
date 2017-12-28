
using Newtonsoft.Json;
using Enrolment.Common;
using Enrolment.Contstants;
using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Trinity.BE;
using Trinity.Common;
using Trinity.Common.Common;
using Trinity.Common.Monitor;
using Trinity.DAL;

namespace Enrolment
{
    public partial class Main : Form
    {
        private JSCallCS _jsCallCS;
        private EventCenter _eventCenter;
        private CodeBehind.Authentication.NRIC _nric;
        private CodeBehind.Login _login;
        private CodeBehind.Suppervisee _suppervisee;
        private NavigatorEnums _currentPage;
        private Trinity.Common.DeviceMonitor.HealthMonitor healthMonitor;

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
            _eventCenter = EventCenter.CreateEventCenter();

            _eventCenter.OnNRICFailed += EventCenter_OnNRICFailed;
            _eventCenter.OnShowMessage += EventCenter_OnShowMessage;
            _eventCenter.OnLogOutCompleted += EventCenter_OnLogOutCompleted;
            _eventCenter.OnLogInSucceeded += EventCenter_OnLogInSucceeded;
            _eventCenter.OnLogInFailed += EventCenter_OnLogInFailed;


            //login
            _login = new CodeBehind.Login(LayerWeb);
            // Supervisee
            _suppervisee = new CodeBehind.Suppervisee(LayerWeb);
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

            timer.Elapsed += PeriodCheck; ;
            timer.Start();
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

        private void LayerWeb_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            LayerWeb.InvokeScript("createEvent", JsonConvert.SerializeObject(_jsCallCS.GetType().GetMethods().Where(d => d.IsPublic && !d.IsVirtual && !d.IsSecuritySafeCritical).ToArray().Select(d => d.Name)));

            // Start page
            NavigateTo(NavigatorEnums.Login);
            //NavigateTo(NavigatorEnums.Supervisee);

        }

        private void NRIC_OnNRICSucceeded()
        {
            // navigate to Supervisee page
            NavigateTo(NavigatorEnums.Supervisee);
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.ExitThread();
            APIUtils.Dispose();
        }

        #region Event Handlers

        private void EventCenter_OnLogOutCompleted()
        {
            NavigateTo(NavigatorEnums.Login);
        }

        private void EventCenter_OnLogInSucceeded()
        {
            NavigateTo(NavigatorEnums.Supervisee);
        }

        private void EventCenter_OnLogInFailed(object sender, LoginEventArgs e)
        {
            MessageBox.Show(e.ErrorMsg, "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void EventCenter_OnNRICFailed(object sender, Trinity.Common.NRICEventArgs e)
        {
            MessageBox.Show(e.Message, "Authentication failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void EventCenter_OnShowMessage(object sender, ShowMessageEventArgs e)
        {
            MessageBox.Show(e.Message, e.Caption, e.Button, e.Icon);
        }

        #endregion

        private void NavigateTo(NavigatorEnums navigatorEnum)
        {
            // navigate
            if (navigatorEnum == NavigatorEnums.Authentication_NRIC)
            {
                _nric.Start();
            }
            else if (navigatorEnum == NavigatorEnums.Supervisee)
            {
                _suppervisee.Start();
            }
            else if (navigatorEnum == NavigatorEnums.Login)
            {
                _login.Start();
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
    }
}

