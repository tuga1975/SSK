
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
using AForge.Video;
using AForge.Video.DirectShow;
using System.Drawing;
using System.IO;
using Futronic.SDKHelper;
using System.Text;

namespace Enrolment
{
    public partial class Main : Form
    {
        private JSCallCS _jsCallCS;
        private EventCenter _eventCenter;
        private CodeBehind.Authentication.NRIC _nric;
        private CodeBehind.Login _login;
        private CodeBehind.WebcamCapture _webcamCapture;
        private CodeBehind.Suppervisee _suppervisee;
        private NavigatorEnums _currentPage;
        private Trinity.Common.DeviceMonitor.HealthMonitor healthMonitor;
        private CodeBehind.Authentication.Fingerprint _fingerprint;
        private int _smartCardFailed;
        private int _fingerprintFailed;
        private Webcam webcam;
        private string _imgBox;
        private byte[] image1;
        private byte[] image2;
        private bool _displayLoginButtonStatus = false;
        private FutronicEnrollment _futronicEnrollment = null;
        public Main()
        {
            InitializeComponent();
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            // setup variables
            _smartCardFailed = 0;
            _fingerprintFailed = 0;
            webcam = new Webcam(pictureBox1);
            _displayLoginButtonStatus = false;
            pictureBox1.Hide();
            #region Initialize and register events
            // _jsCallCS
            _jsCallCS = new JSCallCS(this.LayerWeb);
            _eventCenter = EventCenter.Default;

            _eventCenter.OnNewEvent += EventCenter_OnNewEvent;

            //login
            _login = new CodeBehind.Login(LayerWeb);
            // Supervisee
            _suppervisee = new CodeBehind.Suppervisee(LayerWeb);
            //webcam capture
            _webcamCapture = new CodeBehind.WebcamCapture(LayerWeb);

            // Fingerprint


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
        //private void Fingerprint_OnFingerprintSucceeded(object sender, CodeBehind.Authentication.FingerprintEventArgs e)
        //{
        //    Session session = Session.Instance;

        //    if (session[CommonConstants.CURRENT_FINGERPRINT_DATA] != null)
        //    {
        //        var fingerprintData = (byte[])session[CommonConstants.CURRENT_FINGERPRINT_DATA];
        //        CSCallJS.LoadPageHtml(LayerWeb, "FingerprintCapture.html", fingerprintData);
        //    }
        //}

        //private void Fingerprint_OnFingerprintFailed(object sender, CodeBehind.Authentication.FingerprintEventArgs e)
        //{

        //}

        #region Fingerprint

        private void StartToScanFingerprint()
        {
            Session session = Session.Instance;
            session[CommonConstants.CURRENT_FINGERPRINT_DATA] = true;
            EnrollmentFingerprint();
        }
        private bool m_bExit;
        delegate void SetImageCallback(Bitmap hBitmap);
        private void UpdateScreenImage(Bitmap hBitmap)
        {
            Session session = Session.Instance;
            // Do not change the state control during application closing.
            if (m_bExit)
                return;

            if (this.InvokeRequired)
            {
                SetImageCallback d = new SetImageCallback(this.UpdateScreenImage);
                this.Invoke(d, new object[] { hBitmap });
            }
            else
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    hBitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                    var byteData = ms.ToArray();

                    var base64Str = Convert.ToBase64String(byteData);


                    if (session[CommonConstants.CURRENT_FINGERPRINT_DATA] != null)
                    {

                        if (session[CommonConstants.IS_RIGHT_THUMB] != null)
                        {
                            var isRight = (bool)session[CommonConstants.IS_RIGHT_THUMB];
                            LayerWeb.InvokeScript("setBase64FingerprintOnloadServerCall", isRight, base64Str);
                        }
                    }


                }

            }
        }

        private void EnrollmentFingerprint()
        {
            _futronicEnrollment = new FutronicEnrollment();

            // Set control properties
            _futronicEnrollment.FakeDetection = true;
            _futronicEnrollment.FFDControl = true;
            _futronicEnrollment.FARN = 200;
            _futronicEnrollment.Version = VersionCompatible.ftr_version_compatible;
            _futronicEnrollment.FastMode = true;
            _futronicEnrollment.MIOTControlOff = false;
            _futronicEnrollment.MaxModels = 5;
            _futronicEnrollment.MinMinuitaeLevel = 3;
            _futronicEnrollment.MinOverlappedLevel = 3;


            // register events
            _futronicEnrollment.OnPutOn += OnPutOn;
            _futronicEnrollment.OnTakeOff += OnTakeOff;
            _futronicEnrollment.UpdateScreenImage += new UpdateScreenImageHandler(UpdateScreenImage);
            _futronicEnrollment.OnFakeSource += OnFakeSource;
            _futronicEnrollment.OnEnrollmentComplete += OnEnrollmentComplete;

            // start enrollment process
            _futronicEnrollment.Enrollment();
        }
        private void OnEnrollmentComplete(bool bSuccess, int nResult)
        {
            Session session = Session.Instance;
            var isRight = session[CommonConstants.IS_RIGHT_THUMB] != null ? (bool)session[CommonConstants.IS_RIGHT_THUMB] : (bool)session[CommonConstants.IS_RIGHT_THUMB];
            StringBuilder szMessage = new StringBuilder();
            if (bSuccess)
            {
                // set status string
                szMessage.Append("Enrollment process finished successfully.");
                szMessage.Append("Quality: ");
                szMessage.Append(_futronicEnrollment.Quality.ToString());
                Console.WriteLine(szMessage);

                //set data for curent edit user
                var profileModel = (Trinity.BE.ProfileModel)session[CommonConstants.CURRENT_EDIT_USER];
                if (profileModel != null)
                {
                    if (isRight)
                    {
                        profileModel.User.RightThumbFingerprint = _futronicEnrollment.Template;

                    }
                    else
                    {
                        profileModel.User.LeftThumbFingerprint = _futronicEnrollment.Template;
                    }
                    session[CommonConstants.CURRENT_FINGERPRINT_DATA] = _futronicEnrollment.Template;


                    session[CommonConstants.CURRENT_EDIT_USER] = profileModel;

                }
                //_currentUser.RightThumbFingerprint = _futronicEnrollment.Template;
                LayerWeb.InvokeScript("enableClearBtnServerCall", isRight);
                LayerWeb.InvokeScript("changeMessageServerCall", isRight, "Your fingerprint was scanned successfully!", EnumColors.Green);

                session["CountPutOn"] = null;

            }
            else
            {
                LayerWeb.InvokeScript("changeMessageServerCall", isRight, FutronicSdkBase.SdkRetCode2Message(nResult), EnumColors.Red);
            }

            // unregister events
            _futronicEnrollment.OnPutOn -= OnPutOn;
            _futronicEnrollment.OnTakeOff -= OnTakeOff;
            _futronicEnrollment.UpdateScreenImage -= new UpdateScreenImageHandler(UpdateScreenImage);
            _futronicEnrollment.OnFakeSource -= OnFakeSource;
            _futronicEnrollment.OnEnrollmentComplete -= OnEnrollmentComplete;

            _futronicEnrollment = null;
        }

        private bool OnFakeSource(FTR_PROGRESS Progress)
        {
            Session session = Session.Instance;
            var isRight = session[CommonConstants.IS_RIGHT_THUMB] != null ? (bool)session[CommonConstants.IS_RIGHT_THUMB] : (bool)session[CommonConstants.IS_RIGHT_THUMB];
            //if (session["CountPutOn"] != null)
            //{
            //    var count = (int)session["CountPutOn"];
            //    count--;
            //    session["CountPutOn"] = count;
            //}
            LayerWeb.InvokeScript("changeMessageServerCall", isRight, "Fake source detected. Continue ...", EnumColors.Red);
            return false;
        }

        private void OnTakeOff(FTR_PROGRESS Progress)
        {
            Session session = Session.Instance;
            var isRight = session[CommonConstants.IS_RIGHT_THUMB] != null ? (bool)session[CommonConstants.IS_RIGHT_THUMB] : (bool)session[CommonConstants.IS_RIGHT_THUMB];
            if (session["CountPutOn"] != null)
            {
                var count = (int)session["CountPutOn"];
                count++;
                session["CountPutOn"] = count;
            }
            LayerWeb.InvokeScript("changeMessageServerCall", isRight, "Take off finger from device, please ...", EnumColors.Yellow);
        }
        int countPutOn = 1;
        private void OnPutOn(FTR_PROGRESS Progress)
        {
            Session session = Session.Instance;
            var isRight = session[CommonConstants.IS_RIGHT_THUMB] != null ? (bool)session[CommonConstants.IS_RIGHT_THUMB] : (bool)session[CommonConstants.IS_RIGHT_THUMB];

            if (session["CountPutOn"] != null)
            {
                LayerWeb.InvokeScript("changeMessageServerCall", isRight, "Put finger into device, please ...(" + (int)session["CountPutOn"] + ")", EnumColors.Yellow);
            }
            else
            {
                session["CountPutOn"] = countPutOn;
                LayerWeb.InvokeScript("changeMessageServerCall", isRight, "Put finger into device, please ...(" + countPutOn + ")", EnumColors.Yellow);
            }

        }

        #endregion

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

        private void EventCenter_OnNewEvent(object sender, EventInfo e)
        {
            if (e.Name == EventNames.LOGIN_SUCCEEDED)
            {
                NavigateTo(NavigatorEnums.Supervisee);
            }
            else if (e.Name.Equals(EventNames.LOGIN_FAILED))
            {
                LayerWeb.InvokeScript("Alert", e.Message);
                //MessageBox.Show(e.Message, "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (e.Name == EventNames.LOGOUT_SUCCEEDED)
            {
                NavigateTo(NavigatorEnums.Login);
            }
            else if (e.Name == EventNames.GET_LIST_SUPERVISEE_SUCCEEDED)
            {
                var model = (System.Collections.Generic.List<Trinity.BE.ProfileModel>)e.Data;
                CSCallJS.LoadPageHtml(this.LayerWeb, e.Source.ToString(), model);
            }
            else if (e.Name == EventNames.GET_LIST_SUPERVISEE_FAILED)
            {
                NavigateTo(NavigatorEnums.Login);
            }
            else if (e.Name.Equals(EventNames.OPEN_PICTURE_CAPTURE_FORM))
            {
                //pictureBox1.Image.Save(AppDomain.CurrentDomain.BaseDirectory.ToString() + "/image" + _imgBox + ".jpg");
                if (InvokeRequired)
                {
                    Invoke(new Action(() =>
                    {
                        try
                        {
                            webcam.InitializeWebCam();
                            webcam.startWebcam();
                            _imgBox = e.Message;
                            pictureBox1.Show();
                            NavigateTo(NavigatorEnums.WebcamCapture);
                        }
                        catch
                        {
                            LayerWeb.InvokeScript("Alert", "Cant find this device camera!");
                        }
                    }));
                    return;
                }
            }
            else if (e.Name.Equals(EventNames.CAPTURE_PICTURE))
            {
                if(InvokeRequired)
                {
                    Invoke(new Action(() =>
                    {
                        webcam.stopWebcam();
                        LayerWeb.InvokeScript("confirmMode");
                    }));
                }
            }
            else if (e.Name.Equals(EventNames.CONFIRM_CAPTURE_PICTURE))
            {
                Session session = Session.Instance;
                if (InvokeRequired)
                {

                    Invoke(new Action(() =>
                    {
                        pictureBox1.Hide();
                        webcam.stopWebcam();
                        using (MemoryStream mStream = new MemoryStream())
                        {
                            pictureBox1.Image.Save(mStream, pictureBox1.Image.RawFormat);
                            if (_imgBox == "1") { image1 = mStream.ToArray(); }
                            if (_imgBox == "2") { image2 = mStream.ToArray(); }
                        }
                        var currentPage = session[CommonConstants.CURRENT_PAGE];
                        var currentEditUser = (Trinity.BE.ProfileModel)session[CommonConstants.CURRENT_EDIT_USER];
                        var isPrimaryPhoto = session[CommonConstants.IS_PRIMARY_PHOTO];
                        //for testing purpose
                        var tempBase64String = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAIAAACQd1PeAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAMSURBVBhXY/j//z8ABf4C/qc1gYQAAAAASUVORK5CYII=";
                        var converToByte = Convert.FromBase64String(tempBase64String);
                        var captureImage = session[CommonConstants.CURRENT_PHOTO_DATA];

                        if (currentPage != null && currentPage.ToString() == "EditSupervise" && currentEditUser != null)
                        {
                            if (captureImage != null)
                            {
                                if (isPrimaryPhoto != null && (bool)isPrimaryPhoto)
                                {
                                    // currentEditUser.UserProfile.User_Photo1 = (byte[])captureImage;
                                    currentEditUser.UserProfile.User_Photo1 = image1;
                                }
                                else
                                {
                                    currentEditUser.UserProfile.User_Photo2 = image2;
                                }

                            }
                            session[CommonConstants.CURRENT_EDIT_USER] = currentEditUser;

                            CSCallJS.LoadPageHtml(this.LayerWeb, "UpdateSuperviseeBiodata.html", currentEditUser);
                        }
                        else
                        {
                            LayerWeb.LoadPageHtml("New-Supervisee.html");
                        }

                    }));
                    return;
                }
            }
            else if (e.Name.Equals(EventNames.CANCEL_CAPTURE_PICTURE))
            {
                if (InvokeRequired)
                {
                    CaptureAttempt(CommonConstants.CAPTURE_PHOTO_ATTEMPT);
                    Invoke(new Action(() =>
                    {
                        webcam.stopWebcam();
                        pictureBox1.Hide();
                        LayerWeb.LoadPageHtml("New-Supervisee.html");
                    }));
                    return;
                }
            }
            else if (e.Name.Equals(EventNames.CANCEL_CONFIRM_CAPTURE_PICTURE))
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() =>
                    {
                        webcam.InitializeWebCam();
                        webcam.startWebcam();
                        LayerWeb.InvokeScript("captureMode");
                    }));
                }
            }
            else if (e.Name == EventNames.PHOTO_CAPTURE_FAILED)
            {
                CSCallJS.LoadPageHtml(this.LayerWeb, "FailToCapture.html", e.Message);
            }
            else if (e.Name.Equals(EventNames.OPEN_FINGERPRINT_CAPTURE_FORM))
            {
                CSCallJS.LoadPageHtml(this.LayerWeb, "FingerprintCapture.html");

            }
            else if (e.Name == EventNames.FINGERPRINT_CAPTURE_FAILED)
            {
                CSCallJS.LoadPageHtml(this.LayerWeb, "FailToCapture.html", e.Message);
            }
            else if (e.Name == EventNames.ABLE_TO_PRINT_FAILED)
            {
                CSCallJS.LoadPageHtml(this.LayerWeb, "FailToCapture.html", e.Message);
            }
            else if (e.Name == EventNames.CANCEL_CAPTURE_FINGERPRINT)
            {
                CaptureAttempt(CommonConstants.CAPTURE_FINGERPRINT_ATTEMPT);
            }
            else if (e.Name == EventNames.CONFIRM_CAPTURE_FINGERPRINT)
            {
                StartToScanFingerprint();

            }
            else if (e.Name == EventNames.LOAD_UPDATE_SUPERVISEE_BIODATA)
            {
                var profileModel = (Trinity.BE.ProfileModel)e.Data;
                CSCallJS.LoadPageHtml(this.LayerWeb, "UpdateSuperviseeBiodata.html", profileModel);
                if (profileModel.User.LeftThumbFingerprint != null && profileModel.User.RightThumbFingerprint != null)
                {
                    var leftFingerprint = Convert.ToBase64String(profileModel.User.LeftThumbFingerprint);
                    var rightFingerprint = Convert.ToBase64String(profileModel.User.RightThumbFingerprint);

                    LayerWeb.InvokeScript("setBase64FingerprintOnloadServerCall", leftFingerprint, rightFingerprint);

                }


            }
            else if (e.Name == EventNames.UPDATE_SUPERVISEE_BIODATA)
            {
                var profileModel = (Trinity.BE.ProfileModel)e.Data;
                var dalUser = new DAL_User();
                dalUser.UpdateUser(profileModel.User, profileModel.User.UserId, true);

                new DAL_UserProfile().UpdateUserProfile(profileModel.UserProfile, profileModel.User.UserId, true);
                dalUser.ChangeUserStatus(profileModel.User.UserId, EnumUserStatuses.Enrolled);

            }

        }

        private void CaptureAttempt(string sessionAttemptName)
        {
            Session session = Session.Instance;
            var firstAttemp = 1;
            if (session[sessionAttemptName] != null)
            {
                var attempt = (int)session[sessionAttemptName];
                _jsCallCS.PreviewSuperviseeFingerprint(attempt);
            }
            else
            {
                session[sessionAttemptName] = firstAttemp;
                _jsCallCS.PreviewSuperviseeFingerprint(firstAttemp);
            }
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
            else if (navigatorEnum == NavigatorEnums.WebcamCapture)
            {
                _webcamCapture.Start();
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

        private void Main_Load(object sender, EventArgs e)
        {

        }
    }
}

