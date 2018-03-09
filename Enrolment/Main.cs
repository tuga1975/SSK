using Futronic.SDKHelper;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Trinity.BE;
using Trinity.Common;
using Trinity.Common.Common;
using Trinity.DAL;
using Trinity.Device.Util;

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
        private CodeBehind.Authentication.Fingerprint _fingerprint;
        private int _smartCardFailed;
        private int _fingerprintFailed;
        private Webcam webcam;
        private string _imgBox;
        private byte[] image1;
        private byte[] image2;
        private bool _displayLoginButtonStatus = false;
        private FutronicEnrollment _futronicEnrollment = null;
        private bool _isFirstTimeLoaded = true;

        public Main()
        {
            InitializeComponent();

            APIUtils.Start();
            //Notification
            Trinity.SignalR.Client signalrClient = Trinity.SignalR.Client.Instance;

            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;

            // setup variables
            _smartCardFailed = 0;
            _fingerprintFailed = 0;
            webcam = new Webcam(pictureBox1);
            _displayLoginButtonStatus = false;
            pictureBox1.Hide();
            //add transparent image to picture box 
            pictureBox1.Controls.Add(pictureBoxHead);
            pictureBoxHead.Location = new Point(30, 0);
            pictureBoxHead.BackColor = Color.Transparent;
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

            Lib.LayerWeb = LayerWeb;
            LayerWeb.Url = new Uri(String.Format("file:///{0}/View/html/Layout.html", CSCallJS.curDir));
            LayerWeb.ObjectForScripting = _jsCallCS;

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
                            if (isRight)
                            {
                                session[CommonConstants.CURRENT_RIGHT_FINGERPRINT_IMAGE] = byteData;
                                LayerWeb.InvokeScript("setBase64FingerprintOnloadServerCall", string.Empty, base64Str);
                            }
                            else
                            {
                                session[CommonConstants.CURRENT_LEFT_FINGERPRINT_IMAGE] = byteData;
                                LayerWeb.InvokeScript("setBase64FingerprintOnloadServerCall", base64Str, string.Empty);
                            }

                        }
                    }


                }

            }
        }

        private void EnrollmentFingerprint()
        {
            Session session = Session.Instance;
            if (session["CountPutOn"] == null)
                session["CountPutOn"] = 0;
            FingerprintReaderUtil.Instance.StartCapture(OnPutOn, OnTakeOff, UpdateScreenImage, OnFakeSource, OnEnrollmentComplete);
            //_futronicEnrollment = new FutronicEnrollment();

            //// Set control properties
            //_futronicEnrollment.FakeDetection = true;
            //_futronicEnrollment.FFDControl = true;
            //_futronicEnrollment.FARN = 200;
            //_futronicEnrollment.Version = VersionCompatible.ftr_version_compatible;
            //_futronicEnrollment.FastMode = true;
            //_futronicEnrollment.MIOTControlOff = false;
            //_futronicEnrollment.MaxModels = 5;
            //_futronicEnrollment.MinMinuitaeLevel = 3;
            //_futronicEnrollment.MinOverlappedLevel = 3;


            //// register events
            //_futronicEnrollment.OnPutOn += OnPutOn;
            //_futronicEnrollment.OnTakeOff += OnTakeOff;
            //_futronicEnrollment.UpdateScreenImage += new UpdateScreenImageHandler(UpdateScreenImage);
            //_futronicEnrollment.OnFakeSource += OnFakeSource;
            //_futronicEnrollment.OnEnrollmentComplete += OnEnrollmentComplete;

            //// start enrollment process
            //_futronicEnrollment.Enrollment();
        }
        private void OnEnrollmentComplete(bool bSuccess, int nResult)
        {
            Session session = Session.Instance;
            var isRight = session[CommonConstants.IS_RIGHT_THUMB] != null ? (bool)session[CommonConstants.IS_RIGHT_THUMB] : (bool)session[CommonConstants.IS_RIGHT_THUMB];
            StringBuilder szMessage = new StringBuilder();
            if (bSuccess && FingerprintReaderUtil.Instance.GetQuality >= 7)
            {


                // set status string
                szMessage.Append("Enrollment process finished successfully.");
                szMessage.Append("Quality: ");
                szMessage.Append(FingerprintReaderUtil.Instance.GetQuality.ToString());
                Console.WriteLine(szMessage);

                //set data for curent edit user
                var profileModel = (Trinity.BE.ProfileModel)session[CommonConstants.CURRENT_EDIT_USER];
                var leftThumbImage = (byte[])session[CommonConstants.CURRENT_LEFT_FINGERPRINT_IMAGE];
                var rightThumbImage = (byte[])session[CommonConstants.CURRENT_RIGHT_FINGERPRINT_IMAGE];
                if (profileModel != null)
                {
                    if (leftThumbImage != null && leftThumbImage.Length > 0)
                    {
                        profileModel.UserProfile.LeftThumbImage = leftThumbImage;
                    }
                    if (rightThumbImage != null && rightThumbImage.Length > 0)
                    {
                        profileModel.UserProfile.RightThumbImage = rightThumbImage;
                    }

                    if (isRight)
                    {
                        profileModel.User.RightThumbFingerprint = FingerprintReaderUtil.Instance.GetTemplate;

                    }
                    else
                    {
                        profileModel.User.LeftThumbFingerprint = FingerprintReaderUtil.Instance.GetTemplate;

                    }
                    session[CommonConstants.CURRENT_FINGERPRINT_DATA] = FingerprintReaderUtil.Instance.GetTemplate;


                    session[CommonConstants.CURRENT_EDIT_USER] = profileModel;

                }
                //_currentUser.RightThumbFingerprint = _futronicEnrollment.Template;
                LayerWeb.InvokeScript("enableClearBtnServerCall", isRight);
                LayerWeb.InvokeScript("changeMessageServerCall", isRight, "Your fingerprint was scanned successfully!", EnumColors.Green);

                session["CountPutOn"] = 0;

            }
            else
            {
                if (bSuccess)
                {
                    LayerWeb.InvokeScript("changeMessageServerCall", isRight, "Please use only one finger for one scan.", EnumColors.Red);
                }
                else
                {
                    LayerWeb.InvokeScript("changeMessageServerCall", isRight, FutronicSdkBase.SdkRetCode2Message(nResult), EnumColors.Red);
                }
                var count = (int)session["CountPutOn"] + 1;
                session["CountPutOn"] = count;
                LayerWeb.InvokeScript("enableClearBtnServerCall", isRight);
                if (count >= 3)
                {
                    session["CountPutOn"] = 0;
                    LayerWeb.InvokeScript("showMessageFingerprintFalse3");
                }
            }

            FingerprintReaderUtil.Instance.DisposeCapture();

            //// unregister events
            //_futronicEnrollment.OnPutOn -= OnPutOn;
            //_futronicEnrollment.OnTakeOff -= OnTakeOff;
            //_futronicEnrollment.UpdateScreenImage -= new UpdateScreenImageHandler(UpdateScreenImage);
            //_futronicEnrollment.OnFakeSource -= OnFakeSource;
            //_futronicEnrollment.OnEnrollmentComplete -= OnEnrollmentComplete;

            //_futronicEnrollment = null;
        }

        private bool OnFakeSource(FTR_PROGRESS Progress)
        {
            Session session = Session.Instance;
            var isRight = session[CommonConstants.IS_RIGHT_THUMB] != null ? (bool)session[CommonConstants.IS_RIGHT_THUMB] : (bool)session[CommonConstants.IS_RIGHT_THUMB];
            LayerWeb.InvokeScript("changeMessageServerCall", isRight, "Fake source detected. Continue ...", EnumColors.Red);
            return false;
        }

        private void OnTakeOff(FTR_PROGRESS Progress)
        {
            Session session = Session.Instance;
            var isRight = session[CommonConstants.IS_RIGHT_THUMB] != null ? (bool)session[CommonConstants.IS_RIGHT_THUMB] : (bool)session[CommonConstants.IS_RIGHT_THUMB];
            LayerWeb.InvokeScript("changeMessageServerCall", isRight, "Take off finger from device, please ...", EnumColors.Yellow);
        }
        private void OnPutOn(FTR_PROGRESS Progress)
        {
            Session session = Session.Instance;
            var isRight = session[CommonConstants.IS_RIGHT_THUMB] != null ? (bool)session[CommonConstants.IS_RIGHT_THUMB] : (bool)session[CommonConstants.IS_RIGHT_THUMB];
            LayerWeb.InvokeScript("changeMessageServerCall", isRight, "Put finger into device, please ...", EnumColors.Yellow);
        }

        #endregion

        private void LayerWeb_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            LayerWeb.InvokeScript("createEvent", JsonConvert.SerializeObject(_jsCallCS.GetType().GetMethods().Where(d => d.IsPublic && !d.IsVirtual && !d.IsSecuritySafeCritical).ToArray().Select(d => d.Name)));

            if (_isFirstTimeLoaded)
            {

                //Session session = Session.Instance;
                //Trinity.BE.User user = new DAL_User().GetUserByUserId("bb67863c-c330-41aa-b397-c220428ad16f", true);
                //session[CommonConstants.USER_LOGIN] = user;

                // Set Start page = Login
                NavigateTo(NavigatorEnums.Login);
                //NavigateTo(NavigatorEnums.Supervisee);
                _isFirstTimeLoaded = false;
            }
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

        public Image SetImageOpacity(Image image, float opacity)
        {
            Bitmap bmp = new Bitmap(image.Width, image.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                ColorMatrix matrix = new ColorMatrix();
                matrix.Matrix33 = opacity;
                ImageAttributes attributes = new ImageAttributes();
                attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default,
                                                  ColorAdjustType.Bitmap);
                g.DrawImage(image, new Rectangle(0, 0, bmp.Width, bmp.Height),
                                   0, 0, image.Width, image.Height,
                                   GraphicsUnit.Pixel, attributes);
            }
            return bmp;
        }

        #region Event Handlers

        private void EventCenter_OnNewEvent(object sender, EventInfo e)
        {

            if (e.Name == EventNames.LOGIN_SUCCEEDED)
            {
                new JSCallCS(this.LayerWeb).LoadListSupervisee();
            }
            else if (e.Name.Equals(EventNames.LOGIN_FAILED))
            {
                LayerWeb.InvokeScript("failAlert", e.Message);
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
                var from = e.Data;
                //pictureBox1.Image.Save(AppDomain.CurrentDomain.BaseDirectory.ToString() + "/image" + _imgBox + ".jpg");
                if (InvokeRequired)
                {
                    try
                    {
                        Invoke(new Action(() =>
                        {
                            webcam.InitializeWebCam();
                            webcam.startWebcam();
                            _imgBox = e.Message;

                            pictureBox1.Show();

                            //pictureBoxHead.BackColor = Color.Transparent;
                            // pictureBoxHead.Image = SetImageOpacity(Resources.background_takephoto, 0.25F);
                            //btnHead.BackgroundImage = bmp;
                            pictureBoxHead.BringToFront();
                            pictureBoxHead.Show();
                        }));
                        LayerWeb.LoadPageHtml("WebcamCapture.html");
                    }
                    catch (Exception ex)
                    {
                        Session session = Session.Instance;
                        var currentEditUser = (Trinity.BE.ProfileModel)session[CommonConstants.CURRENT_EDIT_USER];
                        LayerWeb.InvokeScript("failAlert", "Cant find this device camera!");
                        if (currentEditUser != null && from != null)
                        {

                            if (from.ToString() == "new")
                            {
                                LayerWeb.LoadPageHtml("UpdateSuperviseeBiodata.html", currentEditUser);
                                LayerWeb.InvokeScript("setAvatar", currentEditUser.UserProfile.User_Photo1_Base64, currentEditUser.UserProfile.User_Photo2_Base64);

                                string fingerprintLeft = "../images/leftthumb.png";
                                string fingerprintRight = "../images/rightthumb.png";
                                if (currentEditUser.UserProfile.LeftThumbImage != null)
                                {
                                    fingerprintLeft = string.Concat("data:image/jpg;base64,", Convert.ToBase64String(currentEditUser.UserProfile.LeftThumbImage));
                                }
                                if (currentEditUser.UserProfile.RightThumbImage != null)
                                {
                                    fingerprintRight = string.Concat("data:image/jpg;base64,", Convert.ToBase64String(currentEditUser.UserProfile.RightThumbImage));
                                }
                                LayerWeb.InvokeScript("setFingerprintServerCall", fingerprintLeft, fingerprintRight);
                            }
                            else if (from.ToString() == "edit")
                            {
                                string photo1 = currentEditUser.UserProfile.User_Photo1_Base64 ?? string.Empty;
                                string photo2 = currentEditUser.UserProfile.User_Photo2_Base64 ?? string.Empty;
                                //LoadEditSupervisee(currentEditUser, photo1, photo2);
                                LayerWeb.LoadPageHtml("Edit-Supervisee.html", currentEditUser);
                                LayerWeb.InvokeScript("setAvatar", photo1, photo2);

                                string fingerprintLeft = "../images/leftthumb.png";
                                string fingerprintRight = "../images/rightthumb.png";
                                if (currentEditUser.UserProfile.LeftThumbImage != null)
                                {
                                    fingerprintLeft = string.Concat("data:image/jpg;base64,", Convert.ToBase64String(currentEditUser.UserProfile.LeftThumbImage));
                                }
                                if (currentEditUser.UserProfile.RightThumbImage != null)
                                {
                                    fingerprintRight = string.Concat("data:image/jpg;base64,", Convert.ToBase64String(currentEditUser.UserProfile.RightThumbImage));
                                }
                                LayerWeb.InvokeScript("setFingerprintServerCall", fingerprintLeft, fingerprintRight);
                            }
                        }
                    }

                }
            }
            else if (e.Name.Equals(EventNames.CAPTURE_PICTURE))
            {
                if (InvokeRequired)
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
                var currentPhotosSession = session[CommonConstants.CURRENT_PHOTOS];
                if (InvokeRequired)
                {

                    Invoke(new Action(() =>
                    {
                        pictureBox1.Hide();
                        webcam.stopWebcam();
                        using (MemoryStream mStream = new MemoryStream())
                        {
                            pictureBox1.Image.Save(mStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                            if (_imgBox == "1") { image1 = mStream.ToArray(); }
                            if (_imgBox == "2") { image2 = mStream.ToArray(); }
                        }
                    }));
                    var currentPage = session[CommonConstants.CURRENT_PAGE];
                    var currentEditUser = (Trinity.BE.ProfileModel)session[CommonConstants.CURRENT_EDIT_USER];
                    var isPrimaryPhoto = session[CommonConstants.IS_PRIMARY_PHOTO];
                    //for testing purpose

                    string base64Str1 = "";
                    string base64Str2 = "";

                    Tuple<string, string> currentOldPhotos = new Tuple<string, string>(string.Empty, string.Empty);
                    Tuple<string, string> currentNewPhotos = new Tuple<string, string>(string.Empty, string.Empty);

                    if (currentEditUser != null)
                    {
                        //if (currentPhotosSession != null)
                        //{
                        //    currentOldPhotos = (Tuple<string, string>)currentPhotosSession;
                        //}
                        //else
                        //{
                        //    if (currentEditUser.UserProfile.User_Photo1 != null && currentEditUser.UserProfile.User_Photo2 != null)
                        //    {
                        //        var uPhoto1 = Convert.ToBase64String(currentEditUser.UserProfile.User_Photo1);
                        //        var uPhoto2 = Convert.ToBase64String(currentEditUser.UserProfile.User_Photo2);
                        //        currentOldPhotos = new Tuple<string, string>(uPhoto1, uPhoto2);

                        //    }
                        //    else if (currentEditUser.UserProfile.User_Photo1 == null && currentEditUser.UserProfile.User_Photo2 != null)
                        //    {
                        //        var uPhoto2 = Convert.ToBase64String(currentEditUser.UserProfile.User_Photo2);
                        //        currentOldPhotos = new Tuple<string, string>(string.Empty, uPhoto2);
                        //    }
                        //    else if (currentEditUser.UserProfile.User_Photo1 != null && currentEditUser.UserProfile.User_Photo2 == null)
                        //    {
                        //        var uPhoto1 = Convert.ToBase64String(currentEditUser.UserProfile.User_Photo1);
                        //        currentOldPhotos = new Tuple<string, string>(uPhoto1, string.Empty);
                        //    }
                        //}

                        if (currentEditUser.UserProfile.User_Photo1 != null && currentEditUser.UserProfile.User_Photo2 != null)
                        {
                            var uPhoto1 = Convert.ToBase64String(currentEditUser.UserProfile.User_Photo1);
                            var uPhoto2 = Convert.ToBase64String(currentEditUser.UserProfile.User_Photo2);
                            currentOldPhotos = new Tuple<string, string>(uPhoto1, uPhoto2);

                        }
                        else if (currentEditUser.UserProfile.User_Photo1 == null && currentEditUser.UserProfile.User_Photo2 != null)
                        {
                            var uPhoto2 = Convert.ToBase64String(currentEditUser.UserProfile.User_Photo2);
                            currentOldPhotos = new Tuple<string, string>(string.Empty, uPhoto2);
                        }
                        else if (currentEditUser.UserProfile.User_Photo1 != null && currentEditUser.UserProfile.User_Photo2 == null)
                        {
                            var uPhoto1 = Convert.ToBase64String(currentEditUser.UserProfile.User_Photo1);
                            currentOldPhotos = new Tuple<string, string>(uPhoto1, string.Empty);
                        }

                        if (image1 != null)
                        {
                            base64Str1 = Convert.ToBase64String(image1);

                            currentEditUser.UserProfile.User_Photo1 = image1;

                            currentNewPhotos = new Tuple<string, string>(base64Str1, currentOldPhotos.Item2);

                        }

                        if (image2 != null)
                        {
                            base64Str2 = Convert.ToBase64String(image2);

                            currentEditUser.UserProfile.User_Photo2 = image2;

                            currentNewPhotos = new Tuple<string, string>(currentOldPhotos.Item1, base64Str2);
                        }
                    }

                    session[CommonConstants.CURRENT_PHOTOS] = currentNewPhotos;

                    if (currentPage != null && currentPage.ToString() == "EditSupervisee" && currentEditUser != null)
                    {
                        session[CommonConstants.CURRENT_EDIT_USER] = currentEditUser;

                        CSCallJS.LoadPageHtml(this.LayerWeb, "UpdateSuperviseeBiodata.html", currentEditUser);
                        LayerWeb.InvokeScript("setAvatar", currentNewPhotos.Item1, currentNewPhotos.Item2);

                    }
                    else if (currentPage.ToString() == "UpdateSupervisee")
                    {
                        LayerWeb.LoadPageHtml("Edit-Supervisee.html", currentEditUser);
                        //LayerWeb.InvokeScript("setPopUpPhotoServerCall", currentNewPhotos.Item1, currentNewPhotos.Item2);
                        //LayerWeb.InvokeScript("showPopUp", "pageUpdatePhotos");

                        LayerWeb.InvokeScript("setAvatar", currentNewPhotos.Item1, currentNewPhotos.Item2);

                        string fingerprintLeft = "../images/fingerprint.png";
                        string fingerprintRight = "../images/fingerprint.png";
                        if (currentEditUser.UserProfile.LeftThumbImage != null)
                        {
                            fingerprintLeft = string.Concat("data:image/jpg;base64,", Convert.ToBase64String(currentEditUser.UserProfile.LeftThumbImage));
                        }
                        if (currentEditUser.UserProfile.RightThumbImage != null)
                        {
                            fingerprintRight = string.Concat("data:image/jpg;base64,", Convert.ToBase64String(currentEditUser.UserProfile.RightThumbImage));
                        }
                        LayerWeb.InvokeScript("setFingerprintServerCall", fingerprintLeft, fingerprintRight);
                    }
                    else if (currentPage.ToString() == "UpdateSuperviseePhoto")
                    {
                        LayerWeb.LoadPageHtml("UpdateSuperviseePhoto.html", currentEditUser);
                        LayerWeb.InvokeScript("setAvatar", currentNewPhotos.Item1, currentNewPhotos.Item2);
                    }
                    else if (currentPage.ToString() == "UpdateSuperviseeFinger")
                    {
                        LayerWeb.LoadPageHtml("UpdateSuperviseeFingerprint.html", currentEditUser);
                    }
                }
                image1 = null;
                image2 = null;
                _imgBox = "";
            }
            else if (e.Name.Equals(EventNames.LOAD_UPDATE_PHOTOS))
            {
                string photo1 = string.Empty;
                string photo2 = string.Empty;
                Session session = Session.Instance;
                var currentEditUser = (Trinity.BE.ProfileModel)session[CommonConstants.CURRENT_EDIT_USER];
                if (currentEditUser.UserProfile.User_Photo1 != null)
                {
                    photo1 = Convert.ToBase64String(currentEditUser.UserProfile.User_Photo1);

                }
                if (currentEditUser.UserProfile.User_Photo2 != null)
                {
                    photo2 = Convert.ToBase64String(currentEditUser.UserProfile.User_Photo2);
                }
                session[CommonConstants.CURRENT_PAGE] = "UpdateSuperviseePhoto";
                LayerWeb.LoadPageHtml("UpdateSuperviseePhoto.html", currentEditUser);
                LayerWeb.InvokeScript("setAvatar", photo1, photo2);
            }
            else if (e.Name.Equals(EventNames.CANCEL_UPDATE_PICTURE))
            {
                Session session = Session.Instance;
                var currentPage = session[CommonConstants.CURRENT_PAGE];
                StopCamera();
                if (webcam.videoSource == null)
                {
                    if (session[CommonConstants.CURRENT_EDIT_USER] != null && currentPage != null)
                    {
                        var currentEditUser = (Trinity.BE.ProfileModel)session[CommonConstants.CURRENT_EDIT_USER];
                        var oldPhotos = session["TempPhotos"];
                        string photo1 = string.Empty;
                        string photo2 = string.Empty;
                        /*if (currentEditUser.UserProfile.User_Photo1 != null)
                        {
                            photo1 = Convert.ToBase64String(currentEditUser.UserProfile.User_Photo1);
                        }
                        if (currentEditUser.UserProfile.User_Photo2 != null)
                        {
                            photo2 = Convert.ToBase64String(currentEditUser.UserProfile.User_Photo2);
                        }*/
                        var photos = (Tuple<string, string>)session["TempPhotos"];
                        if (photos != null && photos.Item1 != null)
                        {
                            photo1 = photos.Item1;
                        }
                        if (photos != null && photos.Item2 != null)
                        {
                            photo2 = photos.Item2;
                        }
                        session[CommonConstants.CURRENT_PHOTOS] = null;

                        if (photos != null)
                        {
                            currentEditUser.UserProfile.User_Photo1 = photos.Item1 != null ? Convert.FromBase64String(photo1) : null;
                            currentEditUser.UserProfile.User_Photo2 = photos.Item2 != null ? Convert.FromBase64String(photo2) : null;
                        }

                        session[CommonConstants.CURRENT_EDIT_USER] = currentEditUser;
                        if (currentPage.ToString() == "EditSupervisee")
                        {
                            LayerWeb.LoadPageHtml("UpdateSuperviseeBiodata.html", currentEditUser);
                            LayerWeb.InvokeScript("setAvatar", currentEditUser.UserProfile.User_Photo1_Base64, currentEditUser.UserProfile.User_Photo2_Base64);

                            string fingerprintLeft = "../images/leftthumb.png";
                            string fingerprintRight = "../images/rightthumb.png";
                            if (currentEditUser.UserProfile.LeftThumbImage != null)
                            {
                                fingerprintLeft = string.Concat("data:image/jpg;base64,", Convert.ToBase64String(currentEditUser.UserProfile.LeftThumbImage));
                            }
                            if (currentEditUser.UserProfile.RightThumbImage != null)
                            {
                                fingerprintRight = string.Concat("data:image/jpg;base64,", Convert.ToBase64String(currentEditUser.UserProfile.RightThumbImage));
                            }
                            LayerWeb.InvokeScript("setFingerprintServerCall", fingerprintLeft, fingerprintRight);
                        }
                        else if (currentPage.ToString() == "UpdateSupervisee")
                        {
                            LoadEditSupervisee(currentEditUser, photo1, photo2);
                        }
                        else if (currentPage.ToString() == "UpdateSuperviseePhoto")
                        {
                            photo1 = currentEditUser.UserProfile.User_Photo1 != null ? Convert.ToBase64String(currentEditUser.UserProfile.User_Photo1) : string.Empty;
                            photo2 = currentEditUser.UserProfile.User_Photo1 != null ? Convert.ToBase64String(currentEditUser.UserProfile.User_Photo2) : string.Empty;
                            LoadEditSupervisee(currentEditUser, photo1, photo2);
                        }
                    }
                }
            }
            else if (e.Name.Equals(EventNames.CANCEL_CAPTURE_PICTURE))
            {
                Session session = Session.Instance;
                var currentPage = session[CommonConstants.CURRENT_PAGE];
                if (InvokeRequired)
                {
                    Invoke(new Action(() =>
                    {
                        webcam.stopWebcam();
                        pictureBox1.Hide();
                    }));
                    // LayerWeb.LoadPageHtml("New-Supervisee.html");
                    if (session[CommonConstants.CURRENT_EDIT_USER] != null && currentPage != null)
                    {
                        var currentEditUser = (Trinity.BE.ProfileModel)session[CommonConstants.CURRENT_EDIT_USER];
                        var oldPhotos = session["TempPhotos"];
                        string photo1 = string.Empty;
                        string photo2 = string.Empty;
                        /*if (currentEditUser.UserProfile.User_Photo1 != null)
                        {
                            photo1 = Convert.ToBase64String(currentEditUser.UserProfile.User_Photo1);
                        }
                        if (currentEditUser.UserProfile.User_Photo2 != null)
                        {
                            photo2 = Convert.ToBase64String(currentEditUser.UserProfile.User_Photo2);
                        }*/
                        var photos = (Tuple<string, string>)session["TempPhotos"];
                        if (photos != null && photos.Item1 != null)
                        {
                            photo1 = photos.Item1;
                        }
                        if (photos != null && photos.Item2 != null)
                        {
                            photo2 = photos.Item2;
                        }
                        session[CommonConstants.CURRENT_PHOTOS] = null;

                        if (photos != null)
                        {
                            currentEditUser.UserProfile.User_Photo1 = photos.Item1 != null ? Convert.FromBase64String(photo1) : null;
                            currentEditUser.UserProfile.User_Photo2 = photos.Item2 != null ? Convert.FromBase64String(photo2) : null;
                        }

                        session[CommonConstants.CURRENT_EDIT_USER] = currentEditUser;
                        if (currentPage.ToString() == "EditSupervisee")
                        {
                            LayerWeb.LoadPageHtml("UpdateSuperviseeBiodata.html", currentEditUser);
                            LayerWeb.InvokeScript("setAvatar", currentEditUser.UserProfile.User_Photo1_Base64, currentEditUser.UserProfile.User_Photo2_Base64);

                            string fingerprintLeft = "../images/fingerprint.png";
                            string fingerprintRight = "../images/fingerprint.png";
                            if (currentEditUser.UserProfile.LeftThumbImage != null)
                            {
                                fingerprintLeft = string.Concat("data:image/jpg;base64,", Convert.ToBase64String(currentEditUser.UserProfile.LeftThumbImage));
                            }
                            if (currentEditUser.UserProfile.RightThumbImage != null)
                            {
                                fingerprintRight = string.Concat("data:image/jpg;base64,", Convert.ToBase64String(currentEditUser.UserProfile.RightThumbImage));
                            }
                            LayerWeb.InvokeScript("setFingerprintServerCall", fingerprintLeft, fingerprintRight);
                        }
                        else if (currentPage.ToString() == "UpdateSupervisee")
                        {
                            LayerWeb.LoadPageHtml("Edit-Supervisee.html", currentEditUser);
                            LayerWeb.InvokeScript("setAvatar", photo1, photo2);

                            string fingerprintLeft = "../images/fingerprint.png";
                            string fingerprintRight = "../images/fingerprint.png";
                            if (currentEditUser.UserProfile.LeftThumbImage != null)
                            {
                                fingerprintLeft = string.Concat("data:image/jpg;base64,", Convert.ToBase64String(currentEditUser.UserProfile.LeftThumbImage));
                            }
                            if (currentEditUser.UserProfile.RightThumbImage != null)
                            {
                                fingerprintRight = string.Concat("data:image/jpg;base64,", Convert.ToBase64String(currentEditUser.UserProfile.RightThumbImage));
                            }
                            LayerWeb.InvokeScript("setFingerprintServerCall", fingerprintLeft, fingerprintRight);
                        }
                        else if (currentPage.ToString() == "UpdateSuperviseePhoto")
                        {
                            if (currentEditUser.UserProfile.User_Photo1 != null)
                            {
                                photo1 = Convert.ToBase64String(currentEditUser.UserProfile.User_Photo1);

                            }
                            if (currentEditUser.UserProfile.User_Photo2 != null)
                            {
                                photo2 = Convert.ToBase64String(currentEditUser.UserProfile.User_Photo2);
                            }
                            session[CommonConstants.CURRENT_PAGE] = "UpdateSuperviseePhoto";
                            LayerWeb.LoadPageHtml("UpdateSuperviseePhoto.html", currentEditUser);
                            LayerWeb.InvokeScript("setAvatar", photo1, photo2);
                        }
                        CaptureAttempt(CommonConstants.CAPTURE_PHOTO_ATTEMPT);
                    }

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
            else if (e.Name == EventNames.LOAD_UPDATE_SUPERVISEE_BIODATA_SUCCEEDED)
            {
                var profileModel = (Trinity.BE.ProfileModel)e.Data;
                CSCallJS.LoadPageHtml(this.LayerWeb, "UpdateSuperviseeBiodata.html", profileModel);


                if (profileModel.UserProfile.LeftThumbImage != null && profileModel.UserProfile.RightThumbImage != null)
                {
                    var leftFingerprint = profileModel.UserProfile.LeftThumbImage;
                    var rightFingerprint = profileModel.UserProfile.RightThumbImage;

                    LayerWeb.InvokeScript("setBase64FingerprintOnloadServerCall", Convert.ToBase64String(leftFingerprint), Convert.ToBase64String(rightFingerprint));

                }
                string photo1 = string.Empty;
                string photo2 = string.Empty;
                if (profileModel.UserProfile.User_Photo1 != null)
                {
                    photo1 = Convert.ToBase64String(profileModel.UserProfile.User_Photo1);
                }
                if (profileModel.UserProfile.User_Photo2 != null)
                {
                    photo2 = Convert.ToBase64String(profileModel.UserProfile.User_Photo2);
                }
                LayerWeb.InvokeScript("setAvatar", photo1, photo2);
            }
            else if (e.Name == EventNames.UPDATE_SUPERVISEE_BIODATA)
            {
                Session session = Session.Instance;
                object[] data = (object[])e.Data;
                var profileModel = (Trinity.BE.ProfileModel)data[0];
                string frontBase64 = (string)data[1];
                string backBase64 = (string)data[2];
                var dalUser = new DAL_User();
                var dalUserProfile = new DAL_UserProfile();

                new DAL_User().Update(profileModel.User);
                var userProfileModel = profileModel.UserProfile;
                userProfileModel.UserId = profileModel.User.UserId;
                var updateUProfileResult = new DAL_UserProfile().UpdateProfile(userProfileModel);
                dalUser.ChangeUserStatus(profileModel.User.UserId, EnumUserStatuses.New);

                new DAL_Membership_Users().UpdateFingerprint(profileModel.User.UserId, profileModel.User.LeftThumbFingerprint, profileModel.User.RightThumbFingerprint);
                new DAL_UserProfile().UpdateFingerprintImg(profileModel.User.UserId, profileModel.UserProfile.LeftThumbImage, profileModel.UserProfile.RightThumbImage);

                //create to issue card 
                //var dalIssueCard = new DAL_IssueCard();
                //var issueCardModel = dalIssueCard.GetIssueCardById(profileModel.User.SmartCardId);
                //var userLogin = new Trinity.BE.User();
                //if (session[CommonConstants.USER_LOGIN]!=null)
                //{
                //    userLogin=(Trinity.BE.User)session[CommonConstants.USER_LOGIN];
                //    issueCardModel.CreatedBy = userLogin.Name;//officer name 
                //    issueCardModel.Status = EnumUserStatuses.ReEnrolled;
                //    dalIssueCard.Update(profileModel.User.SmartCardId, profileModel.User.UserId, issueCardModel);
                //}
                session[CommonConstants.CURRENT_EDIT_USER] = profileModel;
            }
            else if (e.Name == EventNames.LOAD_EDIT_SUPERVISEE_SUCCEEDED)
            {
                var profileModel = (Trinity.BE.ProfileModel)e.Data;

                Session session = Session.Instance;
                session[CommonConstants.CURRENT_EDIT_USER] = profileModel;
                CSCallJS.LoadPageHtml(this.LayerWeb, "Edit-Supervisee.html", profileModel);
                // convert photo to base64 and add to html
                string photo1 = string.Empty;
                string photo2 = string.Empty;
                if (profileModel.UserProfile.User_Photo1 != null)
                {
                    photo1 = Convert.ToBase64String(profileModel.UserProfile.User_Photo1);
                }
                if (profileModel.UserProfile.User_Photo2 != null)
                {
                    photo2 = Convert.ToBase64String(profileModel.UserProfile.User_Photo2);
                }
                if (!string.IsNullOrEmpty(photo1) || !string.IsNullOrEmpty(photo2))
                {
                    LayerWeb.InvokeScript("setAvatar", photo1, photo2);
                    //LayerWeb.InvokeScript("setPopUpPhotoServerCall", photo1, photo2);
                }

                // convert fingerprint to base64 and add to html
                string fingerprintLeft = "../images/leftthumb.png";
                string fingerprintRight = "../images/rightthumb.png";
                if (profileModel.UserProfile.LeftThumbImage != null)
                {
                    fingerprintLeft = string.Concat("data:image/jpg;base64,", Convert.ToBase64String(profileModel.UserProfile.LeftThumbImage));
                }
                if (profileModel.UserProfile.RightThumbImage != null)
                {
                    fingerprintRight = string.Concat("data:image/jpg;base64,", Convert.ToBase64String(profileModel.UserProfile.RightThumbImage));
                }
                LayerWeb.InvokeScript("setFingerprintServerCall", fingerprintLeft, fingerprintRight);
            }
            else if (e.Name == EventNames.SUPERVISEE_DATA_UPDATE_CANCELED)
            {

                NavigateTo(NavigatorEnums.Supervisee);
            }
        }

        private void StopCamera()
        {
            try
            {
                if (InvokeRequired && webcam.videoSource != null)
                {
                    Invoke(new Action(() =>
                    {
                        webcam.stopWebcam();
                        pictureBox1.Hide();
                    }));
                }
            }
            catch (Exception ex)
            {

                LayerWeb.InvokeScript("failAlert","Something wrong with camera!");
            }
        }

            private void LoadEditSupervisee(ProfileModel currentEditUser, string photo1, string photo2)
            {
                LayerWeb.LoadPageHtml("Edit-Supervisee.html", currentEditUser);
                LayerWeb.InvokeScript("setAvatar", photo1, photo2);

                string fingerprintLeft = "../images/leftthumb.png";
                string fingerprintRight = "../images/rightthumb.png";
                if (currentEditUser.UserProfile.LeftThumbImage != null)
                {
                    fingerprintLeft = string.Concat("data:image/jpg;base64,", Convert.ToBase64String(currentEditUser.UserProfile.LeftThumbImage));
                }
                if (currentEditUser.UserProfile.RightThumbImage != null)
                {
                    fingerprintRight = string.Concat("data:image/jpg;base64,", Convert.ToBase64String(currentEditUser.UserProfile.RightThumbImage));
                }
                LayerWeb.InvokeScript("setFingerprintServerCall", fingerprintLeft, fingerprintRight);
            }

            private void CaptureAttempt(string sessionAttemptName)
            {
                Session session = Session.Instance;
                var firstAttemp = 1;
                if (session[sessionAttemptName] != null)
                {
                    firstAttemp = (int)session[sessionAttemptName];
                    if (sessionAttemptName == CommonConstants.CAPTURE_PHOTO_ATTEMPT)
                    {
                        _jsCallCS.PreviewSuperviseePhoto(firstAttemp);
                    }
                    else
                    {
                        _jsCallCS.PreviewSuperviseeFingerprint(firstAttemp);
                    }
                }
                else
                {
                    session[sessionAttemptName] = firstAttemp;
                    if (sessionAttemptName == CommonConstants.CAPTURE_PHOTO_ATTEMPT)
                    {
                        _jsCallCS.PreviewSuperviseePhoto(firstAttemp);
                    }
                    else
                    {
                        _jsCallCS.PreviewSuperviseeFingerprint(firstAttemp);
                    }

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
                    this.LayerWeb.LoadPageHtml("Supervisee.html");
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

            private void Main_FormClosed(object sender, FormClosedEventArgs e)
            {
                Environment.Exit(0);
            }
        }
    }

