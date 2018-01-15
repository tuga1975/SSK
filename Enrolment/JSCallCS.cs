﻿using Enrolment.CodeBehind.Authentication;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

                //eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Code = 0, Name = EventNames.GET_LIST_SUPERVISEE_SUCCEEDED, Data = listSupervisee, Source = "Supervisee.html" });
                _web.LoadPageHtml("Supervisee.html", listSupervisee);
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
                //  _web.LoadPageHtml("Supervisee.html", listSupervisee);
                eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Code = 0, Name = EventNames.GET_LIST_SUPERVISEE_SUCCEEDED, Data = listSupervisee, Source = "Supervisee.html" });
            }
            else
            {
                eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Code = -1, Name = EventNames.GET_LIST_SUPERVISEE_SUCCEEDED, Data = listSupervisee, Source = "Supervisee.html" });
                // LoadListSupervisee();
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
                //eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Code = 0, Name = EventNames.OPEN_FINGERPRINT_CAPTURE_FORM });
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

        public void OpenFingerprintCaptureForm(string number)
        {
            EventCenter eventCenter = EventCenter.Default;
            Session session = Session.Instance;
            if (number.Equals("1"))
            {
                session[CommonConstants.IS_RIGHT_THUMB] = true;
            }
            eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Name = EventNames.OPEN_FINGERPRINT_CAPTURE_FORM, Message = number });
        }
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
        public void CaptureFingerPrint(bool isRight)
        {
            EventCenter eventCenter = EventCenter.Default;
            Session session = Session.Instance;
            if (isRight)
            {
                session[CommonConstants.IS_RIGHT_THUMB] = true;
            }
            else
            {
                session[CommonConstants.IS_RIGHT_THUMB] = false;
            }

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
            Session session = Session.Instance;

            var dalUser = new DAL_User();
            var dalUserProfile = new DAL_UserProfile();
            var dalUserMembership = new DAL_Membership_Users();

            var dbUser = dalUser.GetUserByUserId(userId, true);


            Trinity.BE.ProfileModel profileModel = null;
            if (session[CommonConstants.CURRENT_EDIT_USER] != null && ((Trinity.BE.ProfileModel)session[CommonConstants.CURRENT_EDIT_USER]).UserProfile.UserId != userId)
            {
                session[CommonConstants.CURRENT_EDIT_USER] = null;
            }

            //session passing from other event like confirm capture photo
            if (session[CommonConstants.CURRENT_EDIT_USER] != null)
            {
                profileModel = (Trinity.BE.ProfileModel)session[CommonConstants.CURRENT_EDIT_USER];
            }
            else
            {
                profileModel = new Trinity.BE.ProfileModel
                {
                    User = dbUser,
                    UserProfile = dalUserProfile.GetUserProfileByUserId(userId, true),
                    Addresses = dalUserProfile.GetAddressByUserId(userId, true),
                    OtherAddress = dalUserProfile.GetAddressByUserId(userId, true, true),
                    Membership_Users = dalUserMembership.GetByUserId(userId)
                };
                //first load set model to session 
                session[CommonConstants.CURRENT_EDIT_USER] = profileModel;
                session["TEMP_USER"] = profileModel;
            }

            if (dbUser.Status.Equals(EnumUserStatuses.New, StringComparison.InvariantCultureIgnoreCase))
            {
                session[CommonConstants.CURRENT_PAGE] = "EditSupervisee";
                EventCenter eventCenter = EventCenter.Default;
                eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Name = EventNames.LOAD_UPDATE_SUPERVISEE_BIODATA_SUCCEEDED, Data = profileModel });
            }
            else
            {
                if (profileModel.Addresses == null)
                {
                    profileModel.Addresses = new Trinity.BE.Address();
                    // if address == null then set Residential_Addess_ID and Other_Address_ID = 0 to insert new record
                    profileModel.UserProfile.Residential_Addess_ID = 0;
                }
                if (profileModel.OtherAddress == null)
                {
                    profileModel.OtherAddress = new Trinity.BE.Address();
                    profileModel.UserProfile.Other_Address_ID = 0;
                }
                session[CommonConstants.CURRENT_PAGE] = "UpdateSupervisee";
                // _web.LoadPageHtml("Edit-Supervisee.html", profileModel);
                EventCenter eventCenter = EventCenter.Default;
                eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Name = EventNames.LOAD_EDIT_SUPERVISEE_SUCCEEDED, Data = profileModel });
            }
        }

        public void SaveSupervisee(string param)
        {
            try
            {
                Session session = Session.Instance;
                var rawData = JsonConvert.DeserializeObject<Trinity.BE.ProfileRawMData>(param);
                var rawDataAddress = JsonConvert.DeserializeObject<Trinity.BE.Address>(param);
                var rawDataOtherAddress = JsonConvert.DeserializeObject<Trinity.BE.OtherAddress>(param);

                var data = new Trinity.BE.ProfileRawMData().ToProfileModel(rawData);
                var dalUser = new Trinity.DAL.DAL_User();

                var dalUserprofile = new Trinity.DAL.DAL_UserProfile();
                var profileModel = (Trinity.BE.ProfileModel)session[CommonConstants.CURRENT_EDIT_USER];
                var tempUser = (Trinity.BE.ProfileModel)session["TEMP_USER"];
                var address = new DAL_Address();

                // get address_ID insert or update
                var residential_Addess_ID = address.SaveAddress(rawDataAddress, true);
                var blkHouse_Number = rawDataAddress.BlkHouse_Number;
                var flrUnit_Number = rawDataAddress.FlrUnit_Number;
                var street_Name = rawDataAddress.Street_Name;
                var country = rawDataAddress.Country;
                var Postal_Code = rawDataAddress.Postal_Code;

                // get Other address ID
                rawDataAddress.Address_ID = rawDataOtherAddress.OAddress_ID;
                rawDataAddress.BlkHouse_Number = rawDataOtherAddress.OBlkHouse_Number;
                rawDataAddress.FlrUnit_Number = rawDataOtherAddress.OFlrUnit_Number;
                rawDataAddress.Street_Name = rawDataOtherAddress.OStreet_Name;
                rawDataAddress.Country = rawDataOtherAddress.OCountry;
                rawDataAddress.Postal_Code = rawDataOtherAddress.OPostal_Code;

                var other_Address_ID = address.SaveAddress(rawDataAddress, true);
                data.OtherAddress = rawDataAddress;
                // set address again to reload page
                data.Addresses.Address_ID = residential_Addess_ID;
                data.Addresses.BlkHouse_Number = blkHouse_Number;
                data.Addresses.FlrUnit_Number = flrUnit_Number;
                data.Addresses.Street_Name = street_Name;
                data.Addresses.Country = country;
                data.Addresses.Postal_Code = Postal_Code;
                //
                data.UserProfile.Residential_Addess_ID = residential_Addess_ID;
                data.UserProfile.Other_Address_ID = other_Address_ID;
                data.UserProfile.User_Photo1 = profileModel.UserProfile.User_Photo1;
                data.UserProfile.User_Photo2 = profileModel.UserProfile.User_Photo2;

                data.User.LeftThumbFingerprint = profileModel.User.LeftThumbFingerprint;
                data.User.RightThumbFingerprint = profileModel.User.RightThumbFingerprint;

                // add some some old data not change in form
                data.UserProfile.SerialNumber = tempUser.UserProfile.SerialNumber;
                data.UserProfile.DateOfIssue = tempUser.UserProfile.DateOfIssue;
                data.UserProfile.Gender = tempUser.UserProfile.Gender;
                data.UserProfile.Race = tempUser.UserProfile.Race;
                data.UserProfile.RightThumbImage = tempUser.UserProfile.RightThumbImage;
                data.UserProfile.LeftThumbImage = tempUser.UserProfile.LeftThumbImage;
                data.UserProfile.Primary_Phone = tempUser.UserProfile.Primary_Phone;
                data.UserProfile.Secondary_Phone = tempUser.UserProfile.Secondary_Phone;
                data.UserProfile.Primary_Email = tempUser.UserProfile.Primary_Email;
                data.UserProfile.Secondary_Email = tempUser.UserProfile.Secondary_Email;
                data.UserProfile.DOB = tempUser.UserProfile.DOB;
                data.UserProfile.Nationality = tempUser.UserProfile.Nationality;
                data.UserProfile.Maritial_Status = tempUser.UserProfile.Maritial_Status;

                data.User.NRIC = tempUser.User.NRIC;
                data.User.SmartCardId = tempUser.User.SmartCardId;
                data.User.IsFirstAttempt = tempUser.User.IsFirstAttempt;
                //////
                data.User.Name = tempUser.User.Name;
                data.User.Status = tempUser.User.Status;

                dalUser.UpdateUser(data.User, profileModel.User.UserId, true);

                dalUserprofile.UpdateUserProfile(data.UserProfile, profileModel.User.UserId, true);
                ////send notifiy to case officer
                APIUtils.SignalR.SendNotificationToDutyOfficer("A supervisee has updated profile.", "Please check Supervisee's information!");


                //session[CommonConstants.CURRENT_EDIT_USER] = data;
                session[CommonConstants.CURRENT_EDIT_USER] = null;
                session["TEMP_USER"] = null;
                //load Supervisee page 
                LoadListSupervisee();
            }
            catch (Exception)
            {
                LoadPage("Login.html");
            }
        }

        public void saveNewDataToSession(string param)
        {
            try
            {
                Session session = Session.Instance;
                var rawData = JsonConvert.DeserializeObject<Trinity.BE.ProfileRawMData>(param);
                var rawDataAddress = JsonConvert.DeserializeObject<Trinity.BE.Address>(param);
                var rawDataOtherAddress = JsonConvert.DeserializeObject<Trinity.BE.OtherAddress>(param);

                var data = new Trinity.BE.ProfileRawMData().ToProfileModel(rawData);
                data.Addresses = rawDataAddress;
                data.OtherAddress.Address_ID = rawDataOtherAddress.OAddress_ID;
                data.OtherAddress.BlkHouse_Number = rawDataOtherAddress.OBlkHouse_Number;
                data.OtherAddress.FlrUnit_Number = rawDataOtherAddress.OFlrUnit_Number;
                data.OtherAddress.Street_Name = rawDataOtherAddress.OStreet_Name;
                data.OtherAddress.Country = rawDataOtherAddress.OCountry;
                data.OtherAddress.Postal_Code = rawDataOtherAddress.OPostal_Code;

                var profileModel = (Trinity.BE.ProfileModel)session[CommonConstants.CURRENT_EDIT_USER];
                var tempProfileModel = (Trinity.BE.ProfileModel)session["TEMP_USER"];

                var photo1 = tempProfileModel.UserProfile.User_Photo1 != null ? Convert.ToBase64String(tempProfileModel.UserProfile.User_Photo1) : null;
                var photo2 = tempProfileModel.UserProfile.User_Photo2 != null ? Convert.ToBase64String(tempProfileModel.UserProfile.User_Photo2) : null;
                ////////
                session["TempPhotos"] = new Tuple<string, string>(photo1, photo2);
                ////////
                data.UserProfile.User_Photo1 = profileModel.UserProfile.User_Photo1;
                data.UserProfile.User_Photo2 = profileModel.UserProfile.User_Photo2;
                data.UserProfile.Residential_Addess_ID = profileModel.UserProfile.Residential_Addess_ID;
                data.UserProfile.Other_Address_ID = profileModel.UserProfile.Other_Address_ID;
                session[CommonConstants.CURRENT_EDIT_USER] = data;

            }
            catch
            {

            }
        }

        public void ReplaceOldPhotos()
        {
            Session session = Session.Instance;
            var profileModel = (Trinity.BE.ProfileModel)session[CommonConstants.CURRENT_EDIT_USER];
            var tempUser = (Trinity.BE.ProfileModel)session["TEMP_USER"];
            tempUser.UserProfile.User_Photo1 = profileModel.UserProfile.User_Photo1;
            tempUser.UserProfile.User_Photo2 = profileModel.UserProfile.User_Photo2;
            session["TEMP_USER"] = tempUser;
        }

        public void UpdateSuperviseePhoto(string param)
        {
            var rawData = JsonConvert.DeserializeObject<Trinity.BE.ProfileRawMData>(param);
            var data = new Trinity.BE.ProfileRawMData().ToProfileModel(rawData);
            Session session = Session.Instance;
            session[CommonConstants.CURRENT_EDIT_USER] = data;
            session[CommonConstants.CURRENT_PAGE] = "UpdateSuperviseePhoto";
            _web.LoadPageHtml("UpdateSuperviseePhoto.html");
        }

        public void UpdateSuperviseeFingerprint()
        {
            Session session = Session.Instance;
            session[CommonConstants.CURRENT_PAGE] = "UpdateSuperviseeFinger";
            _web.LoadPageHtml("UpdateSuperviseeFingerprint.html");
        }

        public void UpdateSuperviseeBiodata(string frontBase64, string backBase64)
        {
            Session session = Session.Instance;
            var dalUser = new Trinity.DAL.DAL_User();
            var dalUserprofile = new Trinity.DAL.DAL_UserProfile();
            var profileModel = (Trinity.BE.ProfileModel)session[CommonConstants.CURRENT_EDIT_USER];
            if (profileModel != null)
            {
                EventCenter eventCenter = EventCenter.Default;

                eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Name = EventNames.UPDATE_SUPERVISEE_BIODATA, Data = new object[] { profileModel, frontBase64, backBase64 } });
            }
        }
        public object loadDataVerify()
        {
            Session session = Session.Instance;
            return session[CommonConstants.CURRENT_EDIT_USER];
        }
        public void PrintSmartCart(string frontBase64, string backBase64)
        {
            this._web.InvokeScript("showPrintMessage", null, "Printing card please wait ...");
            frontBase64 = frontBase64.Replace("data:image/png;base64,", string.Empty);
            backBase64 = backBase64.Replace("data:image/png;base64,", string.Empty);
            Session session = Session.Instance;
            var userLogin = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];
            var profileModel = (Trinity.BE.ProfileModel)session[CommonConstants.CURRENT_EDIT_USER];
            var ImgFront = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".png";
            var ImgBack = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".png";
            new System.Drawing.Bitmap(new System.IO.MemoryStream(Convert.FromBase64String(frontBase64))).Save(ImgFront);
            new System.Drawing.Bitmap(new System.IO.MemoryStream(Convert.FromBase64String(backBase64))).Save(ImgBack);

            PrintAndWriteSmartcardInfo infoPrinter = new PrintAndWriteSmartcardInfo()
            {
                BackCardImagePath = ImgBack,
                FrontCardImagePath = ImgFront,
                SmartCardData = new SmartCardData()
                {
                    CardHolderInfo = new CardHolderInfo()
                    {
                        DOB = profileModel.UserProfile.DOB,
                        Name = profileModel.User.Name,
                        NRIC = profileModel.User.NRIC,
                        UserId = profileModel.User.UserId,
                    },
                    CardInfo = new CardInfo()
                    {
                        CreatedBy = userLogin.UserId,
                        CreatedDate = DateTime.Now
                    }
                }
            };

            Trinity.Common.Utils.SmartCardPrinterUtils.Instance.PrintAndWriteSmartcardData(infoPrinter, OnNewCardPrintedSuccessfully);
        }
        private void OnNewCardPrintedSuccessfully(PrintAndWriteSmartcardResult result)
        {
            if (result.Success)
            {
                Session session = Session.Instance;
                var userLogin = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];
                var currentEditUser = (Trinity.BE.ProfileModel)session[CommonConstants.CURRENT_EDIT_USER];
                DAL_IssueCard dalIssueCard = new Trinity.DAL.DAL_IssueCard();
                string SmartID = result.SmartCardData.CardInfo.UID;
                Trinity.BE.IssueCard IssueCard = new Trinity.BE.IssueCard()
                {
                    CreatedBy = userLogin.UserId,
                    CreatedDate = DateTime.Now,
                    Date_Of_Issue = currentEditUser.UserProfile.DateOfIssue,
                    Name = currentEditUser.Membership_Users.Name,
                    NRIC = currentEditUser.Membership_Users.NRIC,
                    Reprint_Reason = string.Empty,
                    Serial_Number = currentEditUser.UserProfile.SerialNumber,
                    Expired_Date = currentEditUser.UserProfile.Expired_Date,
                    Status = EnumIssuedCards.Active,
                    SmartCardId = SmartID,
                    UserId = currentEditUser.UserProfile.UserId
                };
                dalIssueCard.UpdateStatusByUserId(currentEditUser.UserProfile.UserId, EnumIssuedCards.Deactivate);
                new DAL_Membership_Users().UpdateSmartCardId(currentEditUser.User.UserId, SmartID);
                new DAL_User().ChangeUserStatus(currentEditUser.User.UserId, EnumUserStatuses.Enrolled);
                dalIssueCard.Insert(IssueCard);
                currentEditUser.Membership_Users.SmartCardId = SmartID;
                this._web.InvokeScript("showPrintMessage", true, "Smart Card was printed successfully! Please collect the smart card from printer and place on the reader to verify.");
            }
            else
            {
                this._web.InvokeScript("showPrintMessage", false, result.Description);
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
            Session session = Session.Instance;
            if (number.Equals("1"))
            {
                session[CommonConstants.IS_PRIMARY_PHOTO] = true;
            }
            else
            {
                session[CommonConstants.IS_PRIMARY_PHOTO] = false;
            }

            eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Name = EventNames.OPEN_PICTURE_CAPTURE_FORM, Message = number });
        }

        public void CancelEditSupervisee()
        {
            Session session = Session.Instance;
            session[CommonConstants.CURRENT_EDIT_USER] = null;
            session[CommonConstants.CURRENT_FINGERPRINT_DATA] = null;
            session[CommonConstants.CURRENT_LEFT_FINGERPRINT_IMAGE] = null;
            session[CommonConstants.CURRENT_RIGHT_FINGERPRINT_IMAGE] = null;
            session[CommonConstants.CURRENT_PHOTO_DATA] = null;

            EventCenter eventCenter = EventCenter.Default;
            eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Name = EventNames.SUPERVISEE_DATA_UPDATE_CANCELED });
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
            var dalUser = new DAL_User();
            UserManager<ApplicationUser> userManager = ApplicationIdentityManager.GetUserManager();
            ApplicationUser appUser = userManager.Find(username, password);
            if (appUser != null)
            {
                var userInfo = dalUser.GetUserByUserId(appUser.Id, true);
                if (userInfo.AccessFailedCount >= 3)
                {
                    eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Code = -1, Name = EventNames.LOGIN_FAILED, Message = "You have exceeded the maximum amount of tries to Login. Please go to Forget Password to reset your password" });
                    return;
                }
                // Authenticated successfully
                // Reset AccessFailedCount
                dalUser.ChangeAccessFailedCount(appUser.Id, 0);
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
                ApplicationUser user = userManager.FindByName(username);
                if (user != null)
                {
                    var userInfo = dalUser.GetUserByUserId(user.Id, true);
                    dalUser.ChangeAccessFailedCount(user.Id, userInfo.AccessFailedCount + 1);
                }
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

        #region Update Finger Prints
        private int FingerprintLeftRight = 0;
        private int FingerprintNumber = 0;

        public void SubmitUpdateFingerprints(string left, string leftImg, string right, string rightImg)
        {
            Session session = Session.Instance;
            var currentEditUser = (Trinity.BE.ProfileModel)session[CommonConstants.CURRENT_EDIT_USER];
            byte[] _left = Convert.FromBase64String(left);
            byte[] _right = Convert.FromBase64String(right);

            byte[] _leftImg = Convert.FromBase64String(leftImg);
            byte[] _rightImg = Convert.FromBase64String(rightImg);
            new DAL_Membership_Users().UpdateFingerprint(currentEditUser.UserProfile.UserId, _left, _right);
            new DAL_UserProfile().UpdateFingerprintImg(currentEditUser.UserProfile.UserId, _leftImg, _rightImg);
            if (_left.Length > 0)
            {
                currentEditUser.User.LeftThumbFingerprint = _left;
                currentEditUser.UserProfile.LeftThumbImage = _leftImg;
            }
            if (_right.Length > 0)
            {
                currentEditUser.User.RightThumbFingerprint = _right;
                currentEditUser.UserProfile.RightThumbImage = _rightImg;
            }
            EditSupervisee(currentEditUser.UserProfile.UserId);
        }
        public void UpdateFingerprints()
        {
            FingerprintNumber = 0;
            Session session = Session.Instance;
            var currentEditUser = (Trinity.BE.ProfileModel)session[CommonConstants.CURRENT_EDIT_USER];
            this._web.LoadPageHtml("UpdateSuperviseeFingerprint.html", new object[] { currentEditUser.UserProfile.LeftThumbImage == null ? null : Convert.ToBase64String(currentEditUser.UserProfile.LeftThumbImage), currentEditUser.UserProfile.RightThumbImage == null ? null : Convert.ToBase64String(currentEditUser.UserProfile.RightThumbImage) });
        }
        public void CancelUpdateFingerprints()
        {
            FingerprintReaderUtils.Instance.DisposeCapture();
            Session session = Session.Instance;
            EditSupervisee(((Trinity.BE.ProfileModel)session[CommonConstants.CURRENT_EDIT_USER]).User.UserId);
        }
        public void CaptureFingerprint(int LeftOrRight)
        {
            FingerprintLeftRight = LeftOrRight;
            FingerprintReaderUtils.Instance.StartCapture(OnPutOn, OnTakeOff, UpdateScreenImage, OnFakeSource, OnEnrollmentComplete);
        }

        #region Event Capture Fingerprint
        private void UpdateScreenImage(System.Drawing.Bitmap hBitmap)
        {
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                hBitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                var byteData = ms.ToArray();
                var base64Str = Convert.ToBase64String(byteData);
                _web.InvokeScript("setImageFingerprint", FingerprintLeftRight, base64Str);
            }
        }
        private void OnEnrollmentComplete(bool bSuccess, int nResult)
        {
            if (bSuccess)
            {
                _web.InvokeScript("setDataFingerprint", FingerprintLeftRight, Convert.ToBase64String(FingerprintReaderUtils.Instance.GetTemplate));
                _web.InvokeScript("captureFingerprintMessage", FingerprintLeftRight, "Your fingerprint was scanned successfully!", EnumColors.Green);
                FingerprintNumber = 0;
            }
            else
            {
                _web.InvokeScript("captureFingerprintMessage", FingerprintLeftRight, Futronic.SDKHelper.FutronicSdkBase.SdkRetCode2Message(nResult), EnumColors.Red);
                FingerprintNumber++;
                if (FingerprintNumber >= 3)
                    _web.InvokeScript("moreThan3Fingerprint");
            }
            try
            {
                FingerprintReaderUtils.Instance.DisposeCapture();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error in Trinity.Enrolment.JSCallCS.OnEnrollmentComplete. Details: " + ex.Message);
            }
        }
        private bool OnFakeSource(Futronic.SDKHelper.FTR_PROGRESS Progress)
        {
            _web.InvokeScript("captureFingerprintMessage", FingerprintLeftRight, "Fake source detected. Continue ...", EnumColors.Red);
            return false;
        }
        private void OnTakeOff(Futronic.SDKHelper.FTR_PROGRESS Progress)
        {
            _web.InvokeScript("captureFingerprintMessage", FingerprintLeftRight, "Take off finger from device, please ...", EnumColors.Yellow);
        }
        private void OnPutOn(Futronic.SDKHelper.FTR_PROGRESS Progress)
        {
            _web.InvokeScript("captureFingerprintMessage", FingerprintLeftRight, "Put finger into device, please ...", EnumColors.Yellow);
        }
        #endregion


        #endregion

        #region Issued Cards
        private string reprintTxt = string.Empty;
        public object[] GetDataIssuedCards()
        {
            Session session = Session.Instance;
            var currentEditUser = (Trinity.BE.ProfileModel)session[CommonConstants.CURRENT_EDIT_USER];
            List<Trinity.BE.IssueCard> array = new Trinity.DAL.DAL_IssueCard().GetMyIssueCard(currentEditUser.UserProfile.UserId);
            ////Mở ra nếu IssueCard ko có dữ liệu (Do dữ liệu test, tao ko đúng luồng)
            //array.Insert(0, new Trinity.BE.IssueCard() {
            //    CreatedDate = currentEditUser.UserProfile.DateOfIssue,
            //    Date_Of_Issue = currentEditUser.UserProfile.DateOfIssue,
            //    Name = currentEditUser.Membership_Users.Name,
            //    NRIC = currentEditUser.Membership_Users.NRIC,
            //    Serial_Number = currentEditUser.UserProfile.SerialNumber,
            //    SmartCardId = currentEditUser.Membership_Users.SmartCardId,
            //    Status = EnumIssuedCards.Active,
            //    UserId = currentEditUser.Membership_Users.UserId
            //});
            return new object[] { array, currentEditUser.UserProfile.UserId };
        }
        public void PriterIssuedCard(string reprint)
        {
            reprintTxt = reprint;
            Session session = Session.Instance;
            var userLogin = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];
            var currentEditUser = (Trinity.BE.ProfileModel)session[CommonConstants.CURRENT_EDIT_USER];
            PrintAndWriteSmartcardInfo infoPrinter = new PrintAndWriteSmartcardInfo()
            {
                SmartCardData = new SmartCardData()
                {
                    CardHolderInfo = new CardHolderInfo()
                    {
                        DOB = currentEditUser.UserProfile.DOB,
                        Name = currentEditUser.User.Name,
                        NRIC = currentEditUser.User.NRIC,
                        UserId = currentEditUser.User.UserId,
                    },
                    CardInfo = new CardInfo()
                    {
                        CreatedBy = userLogin.UserId,
                        CreatedDate = DateTime.Now
                    }
                }
            };

            Trinity.Common.Utils.SmartCardPrinterUtils.Instance.PrintAndWriteSmartcardData(null, PriterIssuedCardOnCompleted);
        }
        private void PriterIssuedCardOnCompleted(PrintAndWriteSmartcardResult result)
        {
            if (result.Success)
            {
                Session session = Session.Instance;
                var userLogin = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];
                var currentEditUser = (Trinity.BE.ProfileModel)session[CommonConstants.CURRENT_EDIT_USER];
                DAL_IssueCard dalIssueCard = new Trinity.DAL.DAL_IssueCard();
                string SmartID = result.SmartCardData.CardInfo.UID;
                Trinity.BE.IssueCard IssueCard = new Trinity.BE.IssueCard()
                {
                    CreatedBy = userLogin.UserId,
                    CreatedDate = DateTime.Now,
                    Date_Of_Issue = currentEditUser.UserProfile.DateOfIssue,
                    Name = currentEditUser.Membership_Users.Name,
                    NRIC = currentEditUser.Membership_Users.NRIC,
                    Reprint_Reason = reprintTxt,
                    Serial_Number = currentEditUser.UserProfile.SerialNumber,
                    Expired_Date = currentEditUser.UserProfile.Expired_Date,
                    Status = EnumIssuedCards.Active,
                    SmartCardId = SmartID,
                    UserId = currentEditUser.UserProfile.UserId
                };
                dalIssueCard.UpdateStatusByUserId(currentEditUser.UserProfile.UserId, EnumIssuedCards.Deactivate);
                dalIssueCard.Insert(IssueCard);
                new DAL_Membership_Users().UpdateSmartCardId(currentEditUser.UserProfile.UserId, SmartID);
                currentEditUser.Membership_Users.SmartCardId = SmartID;
                _web.InvokeScript("OnPriterIssuedCardCompleted", true, IssueCard.JsonString());
            }
            else
            {
                _web.InvokeScript("OnPriterIssuedCardCompleted", false, null);
            }
        }
        #endregion
    }
}
