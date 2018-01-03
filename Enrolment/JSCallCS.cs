
using Enrolment.CodeBehind.Authentication;
using Enrolment.Common;
using Enrolment.Contstants;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Trinity.Common;
using Trinity.Common.Common;
using Trinity.DAL;
using Trinity.DAL.DBContext;
using Trinity.Identity;

namespace Enrolment
{
    [System.Runtime.InteropServices.ComVisibleAttribute(true)]
    public class JSCallCS
    {
        private WebBrowser _web = null;
        private Type _thisType = null;

        public JSCallCS(WebBrowser web)
        {
            this._web = web;
            _thisType = this.GetType();
        }

        public void LoadPage(string file)
        {
            _web.LoadPageHtml(file);
        }

        private void actionThread(object pram)
        {

            var data = (object[])pram;
            var method = data[0].ToString();

            MethodInfo theMethod = _thisType.GetMethod(method);
            var dataReturn = theMethod.Invoke(this, (object[])data[2]);
            if (data[1] != null)
            {
                this._web.InvokeScript("callEventCallBack", data[1], JsonConvert.SerializeObject(dataReturn, Formatting.Indented, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
            }
            _web.SetLoading(false);
        }

        public void ClientCallServer(string method, string guidEvent, params object[] pram)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(actionThread), new object[] { method, guidEvent, pram });
        }

        public void SubmitNRIC(string strNRIC)
        {
            NRIC nric = NRIC.GetInstance(_web);
            nric.NRICAuthentication(strNRIC);
        }

        public void LoadListSupervisee()
        {
            EventCenter eventCenter = EventCenter.Default;
            Session session = Session.Instance;
            var dalUser = new DAL_User();
            var dalUserProfile = new DAL_UserProfile();
            var dbUsers = dalUser.GetAllSupervisees(true);
            var listSupervisee = new List<Trinity.BE.ProfileModel>();
            if (dbUsers != null)
            {
                foreach (var item in dbUsers)
                {
                    var model = new Trinity.BE.ProfileModel()
                    {
                        User = item,
                        UserProfile = dalUserProfile.GetUserProfileByUserId(item.UserId, true),
                        Addresses = null
                    };
                    listSupervisee.Add(model);
                }

                eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Code = 0, Name = EventNames.GET_LIST_SUPERVISEE_SUCCEEDED, Data = listSupervisee, Source = "Supervisee.html" });

                //_web.LoadPageHtml("Supervisee.html", listSupervisee);
            }
            else
            {
                eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Code = -1, Name = EventNames.GET_LIST_SUPERVISEE_FAILED, Source = "Login.html" });
            }
        }

        public void SearchSuperviseeByNRIC(string nric)
        {
            EventCenter eventCenter = EventCenter.Default;
            Session session = Session.Instance;
            var dalUser = new DAL_User();
            var dalUserProfile = new DAL_UserProfile();
            var dbUser = dalUser.GetSuperviseeByNRIC(nric, true);
            var listSupervisee = new List<Trinity.BE.ProfileModel>();
            if (dbUser != null)
            {
                var model = new Trinity.BE.ProfileModel()
                {
                    User = dbUser,
                    UserProfile = dalUserProfile.GetUserProfileByUserId(dbUser.UserId, true),
                    Addresses = null
                };
                session[CommonConstants.SUPERVISEE] = dbUser;
                listSupervisee.Add(model);
                _web.LoadPageHtml("Supervisee.html", listSupervisee);
                eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Code = 0, Name = EventNames.GET_LIST_SUPERVISEE_SUCCEEDED, Data = listSupervisee, Source = "Supervisee.html" });
            }
            else
            {
                LoadListSupervisee();
            }
        }

        public void PreviewSuperviseePhoto(int attempt)
        {
            EventCenter eventCenter = EventCenter.Default;
            Session session = Session.Instance;
            var sessionAttempt = (int)session[CommonConstants.CAPTURE_PHOTO_ATTEMPT];
            if (attempt > 3)
            {
                session[CommonConstants.CAPTURE_PHOTO_ATTEMPT] = null;
                APIUtils.SignalR.SendNotificationToDutyOfficer("Supervisee failed to capture photo!", "Supervisee failed to capture photo!\n Please check the status");
                eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Code = -1, Name = EventNames.PHOTO_CAPTURE_FAILED, Message = "Unable to capture photo", Source = "FailToCapture.html" });
            }
            else
            {
                sessionAttempt++;
                session[CommonConstants.CAPTURE_PHOTO_ATTEMPT] = sessionAttempt;
                eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Code = 0, Name = EventNames.OPEN_PICTURE_CAPTURE_FORM });
            }
        }

        public void PreviewSuperviseeFingerprint(int attempt)
        {
            EventCenter eventCenter = EventCenter.Default;
            Session session = Session.Instance;
            var sessionAttempt = (int)session[CommonConstants.CAPTURE_FINGERPRINT_ATTEMPT];
            if (attempt > 3)
            {
                session[CommonConstants.CAPTURE_FINGERPRINT_ATTEMPT] = null;
                APIUtils.SignalR.SendNotificationToDutyOfficer("Supervisee failed to capture photo!", "Supervisee failed to capture fingerprint!\n Please check the status");
                eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Code = -1, Name = EventNames.PHOTO_CAPTURE_FAILED, Message = "Unable to capture fingerprint", Source = "FailToCapture.html" });
            }
            else
            {
                sessionAttempt++;
                session[CommonConstants.CAPTURE_FINGERPRINT_ATTEMPT] = sessionAttempt;
                eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Code = 0, Name = EventNames.OPEN_FINGERPRINT_CAPTURE_FORM });
            }
        }

        public void AbleToPrintSCard(int attempt)
        {
            EventCenter eventCenter = EventCenter.Default;
            Session session = Session.Instance;
            var sessionAttempt = (int)session[CommonConstants.PRINT_SMARTCARD_ATTEMPT];
            if (attempt > 3)
            {
                session[CommonConstants.PRINT_SMARTCARD_ATTEMPT] = null;
                APIUtils.SignalR.SendNotificationToDutyOfficer("Supervisee failed to capture photo!", "Supervisee failed to print smart card!\n Please check the status");
                eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Code = -1, Name = EventNames.ABLE_TO_PRINT_FAILED, Message = "Unable to print smart card", Source = "FailToCapture.html" });
            }
            else
            {
                sessionAttempt++;
                session[CommonConstants.PRINT_SMARTCARD_ATTEMPT] = sessionAttempt;
            }
        }

        #region Fingerprint Capture Event
        public void CancelCaptureFingerprint()
        {
            EventCenter eventCenter = EventCenter.Default;
            eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Name = EventNames.CANCEL_CAPTURE_FINGERPRINT });
        }

        public void ConfirmFingerprint()
        {
            EventCenter eventCenter = EventCenter.Default;

            eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Name = EventNames.CONFIRM_CAPTURE_FINGERPRINT });


        }
        #endregion




        public void ScanNRIC()
        {
            Session session = Session.Instance;
            var codeScanned = (string)session[CommonConstants.SCANNED_BARCODE];
            if (!string.IsNullOrEmpty(codeScanned))
            {
                SearchSuperviseeByNRIC(codeScanned);
            }
            else
            {
                APIUtils.SignalR.SendNotificationToDutyOfficer("Unable to scan supervisee's NRIC", "Unable to scan supervisee's NRIC! Please check the manually input information!");
                LoadListSupervisee();

            }
        }

        public void EditSupervisee(string userId)
        {
            var dalUser = new DAL_User();
            var dalUserProfile = new DAL_UserProfile();

            var dbUser = dalUser.GetUserByUserId(userId, true);

            var profileModel = new Trinity.BE.ProfileModel
            {
                User = dbUser,
                UserProfile = dalUserProfile.GetUserProfileByUserId(userId, true),
                Addresses = dalUserProfile.GetAddressByUserId(userId, true)
            };

            _web.LoadPageHtml("Edit-Supervisee.html", profileModel);
        }

        public void SaveSupervisee(string param, bool primaryInfoChange) {
            try
            {
                var rawData = JsonConvert.DeserializeObject<Trinity.BE.ProfileRawMData>(param);
                var data = new Trinity.BE.ProfileRawMData().ToProfileModel(rawData);
                var dalUser = new Trinity.DAL.DAL_User();
                var dalUserprofile = new Trinity.DAL.DAL_UserProfile();
                if (primaryInfoChange)
                {

                    dalUser.UpdateUser(data.User, data.User.UserId, true);

                    dalUserprofile.UpdateUserProfile(data.UserProfile, data.User.UserId, true);
                    //send notifiy to duty officer
                    APIUtils.SignalR.SendNotificationToDutyOfficer("A supervisee has updated profile.", "Please check Supervisee's information!");
                }
                else
                {
                    dalUserprofile.UpdateUserProfile(data.UserProfile, data.User.UserId, true);
                    //send notifiy to case officer
                    APIUtils.SignalR.SendNotificationToDutyOfficer("A supervisee has updated profile.", "Please check Supervisee's information!");
                }

                //load Supervisee page 
                LoadPage("Supervisee.html");
            }
            catch (Exception)
            {
                LoadPage("Supervisee.html");
            }
        }

        public void AddNewSupervisee()
        {
            _web.LoadPageHtml("New-Supervisee.html");
        }

        #region Webcam event
        public void OpenPictureCaptureForm(string number)
        {
            EventCenter eventCenter = EventCenter.Default;
            eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Name = EventNames.OPEN_PICTURE_CAPTURE_FORM, Message = number });
        }
        public void CancelCapturePicture()
        {
            EventCenter eventCenter = EventCenter.Default;
            eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Name = EventNames.CANCEL_CAPTURE_PICTURE });
        }
        public void CapturePicture()
        {
            EventCenter eventCenter = EventCenter.Default;
            eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Name = EventNames.CAPTURE_PICTURE });
        }
        public void ConfirmCapturePicture()
        {
            EventCenter eventCenter = EventCenter.Default;
            eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Name = EventNames.CONFIRM_CAPTURE_PICTURE });
        }
        public void CancelConfirmCapturePicture()
        {
            EventCenter eventCenter = EventCenter.Default;
            eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Name = EventNames.CANCEL_CONFIRM_CAPTURE_PICTURE });
        }
        #endregion

        #region Authentication & Authorization

        public void Login(string username, string password)
        {
            EventCenter eventCenter = EventCenter.Default;

            UserManager<ApplicationUser> userManager = ApplicationIdentityManager.GetUserManager();
            ApplicationUser appUser = userManager.Find(username, password);
            if (appUser != null)
            {
                // Authenticated successfully
                // Check if the current user is an Enrolment Officer or not
                if (userManager.IsInRole(appUser.Id, EnumUserRoles.EnrolmentOfficer))
                {
                    // Authorized successfully
                    Trinity.BE.User user = new Trinity.BE.User()
                    {
                        RightThumbFingerprint = appUser.RightThumbFingerprint,
                        LeftThumbFingerprint = appUser.LeftThumbFingerprint,
                        IsFirstAttempt = appUser.IsFirstAttempt,
                        Name = appUser.Name,
                        NRIC = appUser.NRIC,
                        Role = EnumUserRoles.EnrolmentOfficer,
                        SmartCardId = appUser.SmartCardId,
                        Status = appUser.Status,
                        UserId = appUser.Id
                    };
                    Session session = Session.Instance;
                    session.IsUserNamePasswordAuthenticated = true;
                    session.Role = EnumUserRoles.EnrolmentOfficer;
                    session[CommonConstants.USER_LOGIN] = user;

                    eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Code = 0, Name = EventNames.LOGIN_SUCCEEDED });
                }
                else
                {
                    eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Code = -2, Name = EventNames.LOGIN_FAILED, Message = "You do not have permission to access this page." });
                }
            }
            else
            {
                eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Code = -1, Name = EventNames.LOGIN_FAILED, Message = "Your username or password is incorrect." });
            }
        }

        public void LogOut()
        {
            // reset session value
            Session session = Session.Instance;
            session.IsSmartCardAuthenticated = false;
            session.IsFingerprintAuthenticated = false;
            session[CommonConstants.USER_LOGIN] = null;
            session[CommonConstants.PROFILE_DATA] = null;

            //
            // RaiseLogOutCompletedEvent
            EventCenter eventCenter = EventCenter.Default;
            eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Code = 0, Name = EventNames.LOGOUT_SUCCEEDED });
        }

        #endregion
    }
}
