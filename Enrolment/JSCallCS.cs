using Enrolment.CodeBehind.Authentication;
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
using Trinity.Device;
using Trinity.Device.Util;
using System.IO;
using System.Drawing;

namespace Enrolment
{
    [System.Runtime.InteropServices.ComVisibleAttribute(true)]
    public class JSCallCS : JSCallCSBase
    {

        public JSCallCS(WebBrowser web)
        {
            this._web = web;
            _thisType = this.GetType();
        }
        public void LoadPopupQueue()
        {
            this._web.LoadPopupHtml("QueuePopupDetail.html");
        }
        public void SubmitNRIC(string strNRIC)
        {
            NRIC nric = NRIC.GetInstance(_web);
            nric.NRICAuthentication(strNRIC);
        }

        public void LoadListSupervisee()
        {
            EventCenter eventCenter = EventCenter.Default;

            eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Code = 0, Name = EventNames.GET_LIST_SUPERVISEE_SUCCEEDED });
        }

        public object SearchSuperviseeByNRIC(string nric)
        {
            Session session = Session.Instance;
            var dalUser = new DAL_User();
            var dalUserProfile = new DAL_UserProfile();
            var listSupervisee = new List<Trinity.BE.ProfileModel>();
            if (string.IsNullOrEmpty(nric))
            {
                foreach (var item in new DAL_User().GetListAllSupervisees())
                {
                    var model = new Trinity.BE.ProfileModel()
                    {
                        User = item,
                        UserProfile = dalUserProfile.GetProfile(item.UserId),
                        Addresses = null
                    };
                    listSupervisee.Add(model);
                }
            }
            else
            {
                foreach (var item in dalUser.SearchSuperviseeByNRIC(nric))
                {
                    var model = new Trinity.BE.ProfileModel()
                    {
                        User = item,
                        UserProfile = dalUserProfile.GetProfile(item.UserId),
                        Addresses = null
                    };
                    listSupervisee.Add(model);
                }
            }
            return listSupervisee.Select(d => new
            {
                d.User.Name,
                d.User.NRIC,
                DOB = d.UserProfile == null ? string.Empty : d.UserProfile.DOBAsString,
                d.User.Status,
                d.User.UserId
            });
            //var dbUsers = dalUser.SearchSuperviseeByNRIC(nric);
            //var listSupervisee = new List<Trinity.BE.ProfileModel>();
            //if (dbUsers != null && dbUsers.Count > 0)
            //{
            //    foreach (var item in dbUsers)
            //    {
            //        var model = new Trinity.BE.ProfileModel()
            //        {
            //            User = item,
            //            UserProfile = dalUserProfile.GetProfile(item.UserId),
            //            Addresses = null
            //        };
            //        listSupervisee.Add(model);
            //    }

            //    //  session[CommonConstants.SUPERVISEE] = dbUser;

            //    //  _web.LoadPageHtml("Supervisee.html", listSupervisee);
            //    eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Code = 0, Name = EventNames.GET_LIST_SUPERVISEE_SUCCEEDED, Data = listSupervisee, Source = nric });
            //}
            //else
            //{
            //    eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Code = -1, Name = EventNames.GET_LIST_SUPERVISEE_SUCCEEDED, Data = listSupervisee, Source = nric, Message = "Cant find any record with your input's information." });
            //    // LoadListSupervisee();
            //}
        }

        public void PreviewSuperviseePhoto(int attempt)
        {
            EventCenter eventCenter = EventCenter.Default;
            Session session = Session.Instance;
            var sessionAttempt = (int)session[CommonConstants.CAPTURE_PHOTO_ATTEMPT];
            if (attempt > 3)
            {
                session[CommonConstants.CAPTURE_PHOTO_ATTEMPT] = null;
                Trinity.SignalR.Client.Instance.SendToAppDutyOfficers(null, "Supervisee failed to capture photo!", "Supervisee failed to capture photo!\n Please check the status", EnumNotificationTypes.Error);
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
                Trinity.SignalR.Client.Instance.SendToAppDutyOfficers(null, "Supervisee failed to capture photo!", "Supervisee failed to capture fingerprint!\n Please check the status", EnumNotificationTypes.Error);
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
                Trinity.SignalR.Client.Instance.SendToAppDutyOfficers(null, "Supervisee failed to capture photo!", "Supervisee failed to print smart card!\n Please check the status", EnumNotificationTypes.Error);
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
                Trinity.SignalR.Client.Instance.SendToAppDutyOfficers(null, "Unable to scan supervisee's NRIC", "Unable to scan supervisee's NRIC! Please check the manually input information!", EnumNotificationTypes.Caution);
                LoadListSupervisee();

            }
        }

        public void EditSupervisee(string userId, string Action = null)
        {

            Session session = Session.Instance;
            var dalUser = new DAL_User();
            var dalUserProfile = new DAL_UserProfile();
            var dalUserMembership = new DAL_Membership_Users();
            var result = dalUser.GetUserById(userId);
            var dbUser = result;
            Trinity.BE.ProfileModel profileModel = null;
            if (session[CommonConstants.CURRENT_EDIT_USER] != null && ((Trinity.BE.ProfileModel)session[CommonConstants.CURRENT_EDIT_USER]).User.UserId != userId)
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
                    UserProfile = new DAL_UserProfile().GetProfile(dbUser.UserId),
                    Addresses = new DAL_UserProfile().GetAddByUserId(dbUser.UserId),
                    OtherAddress = new DAL_UserProfile().GetAddByUserId(dbUser.UserId, true),
                    Membership_Users = dalUserMembership.GetByUserId(userId)
                };
                //first load set model to session 
                profileModel.UserProfile = profileModel.UserProfile != null ? profileModel.UserProfile : new Trinity.BE.UserProfile() { UserId = dbUser.UserId };
                session[CommonConstants.CURRENT_EDIT_USER] = profileModel;
                session["TEMP_USER"] = profileModel;
            }
            if (Action != null)
            {
                if (Action == "UpdateSupervisee")
                {
                    if (profileModel.Addresses == null)
                    {
                        profileModel.Addresses = new Trinity.BE.Address();
                        // if address == null then set Residential_Addess_ID and Other_Address_ID = 0 to insert new record
                        //profileModel.UserProfile.Residential_Addess_ID = 0;
                    }
                    if (profileModel.OtherAddress == null)
                    {
                        profileModel.OtherAddress = new Trinity.BE.Address();
                        //profileModel.UserProfile.Other_Address_ID = 0;
                    }
                    session[CommonConstants.CURRENT_PAGE] = "UpdateSupervisee";
                    // _web.LoadPageHtml("Edit-Supervisee.html", profileModel);
                    EventCenter eventCenter = EventCenter.Default;
                    eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Name = EventNames.LOAD_EDIT_SUPERVISEE_SUCCEEDED, Data = profileModel });
                }
            }
            else
            {
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
                        //profileModel.UserProfile.Residential_Addess_ID = 0;
                    }
                    if (profileModel.OtherAddress == null)
                    {
                        profileModel.OtherAddress = new Trinity.BE.Address();
                        //profileModel.UserProfile.Other_Address_ID = 0;
                    }
                    session[CommonConstants.CURRENT_PAGE] = "UpdateSupervisee";
                    // _web.LoadPageHtml("Edit-Supervisee.html", profileModel);
                    EventCenter eventCenter = EventCenter.Default;
                    eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Name = EventNames.LOAD_EDIT_SUPERVISEE_SUCCEEDED, Data = profileModel });
                }
            }
        }

        public void SaveSupervisee(string param)
        {
            Session session = Session.Instance;
            var rawData = JsonConvert.DeserializeObject<Trinity.BE.ProfileRawMData>(param);


            if ((!rawData.Employment_Start_Date.HasValue && rawData.Employment_End_Date.HasValue) || (rawData.Employment_Start_Date.HasValue && rawData.Employment_End_Date.HasValue && rawData.Employment_Start_Date.Value >= rawData.Employment_End_Date.Value))
            {
                _web.ShowMessage("<b>Employment Start Date</b> must be less than <b>Employment End Date</b>");
            }
            else
            {
                var rawDataAddress = JsonConvert.DeserializeObject<Trinity.BE.Address>(param);
                var rawDataOtherAddress = JsonConvert.DeserializeObject<Trinity.BE.OtherAddress>(param);

                var data = new Trinity.BE.ProfileRawMData().ToProfileModel(rawData);
                var dalUser = new Trinity.DAL.DAL_User();

                var dalUserprofile = new Trinity.DAL.DAL_UserProfile();
                var profileModel = (Trinity.BE.ProfileModel)session[CommonConstants.CURRENT_EDIT_USER];
                var tempUser = (Trinity.BE.ProfileModel)session["TEMP_USER"];
                var address = new DAL_Address();

                // get address_ID insert or update
                var residential_Addess_ID = address.SaveAddress(rawDataAddress);
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

                var other_Address_ID = address.SaveAddress(rawDataAddress);
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

                byte[] photo1 = VerifyImage(profileModel.UserProfile.User_Photo1);
                byte[] photo2 = VerifyImage(profileModel.UserProfile.User_Photo2);

                data.UserProfile.User_Photo1 = photo1;
                data.UserProfile.User_Photo2 = photo2;

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
                data.User.UserId = profileModel.User.UserId;

                var updateUserResult = dalUser.Update(data.User);
                // dalUser.UpdateUser(data.User, data.User.UserId, true);
                var userProfileModel = data.UserProfile;
                userProfileModel.UserId = data.User.UserId;
                var updateUProfileResult = dalUserprofile.UpdateProfile(userProfileModel);

                ////send notifiy to case officer
                var user = (Trinity.BE.User)Session.Instance[CommonConstants.USER_LOGIN];
                Trinity.SignalR.Client.Instance.SendToAppDutyOfficers(user.UserId, "Supervisee " + user.Name + " has updated profile.", "Please check Supervisee " + user.Name + "'s information!", EnumNotificationTypes.Notification);

                //session[CommonConstants.CURRENT_EDIT_USER] = data;
                session[CommonConstants.CURRENT_EDIT_USER] = null;
                session["TEMP_USER"] = null;
                //load Supervisee page 
                LoadListSupervisee();
            }
        }

        private byte[] VerifyImage(byte[] user_Photo)
        {
            try
            {
                // Get image from byte[]
                MemoryStream ms = new MemoryStream(user_Photo);
                Image image = Image.FromStream(ms);

                int maxHeight = 1366;
                int maxWidth = 768;
                if (image.Height > maxHeight || image.Width > maxWidth)
                {
                    // Scale image
                    Image newImage = CommonUtil.ScaleImage(image, maxHeight, maxWidth);

                    // Convert image to byte[]
                    MemoryStream newMS = new MemoryStream();
                    newImage.Save(newMS, image.RawFormat);
                    return newMS.ToArray();
                }

                return user_Photo;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public void saveNewDataToSession(string param)
        {
            var rawData = JsonConvert.DeserializeObject<Trinity.BE.ProfileRawMData>(param);

            var rawDataAddress = JsonConvert.DeserializeObject<Trinity.BE.Address>(param);
            var rawDataOtherAddress = JsonConvert.DeserializeObject<Trinity.BE.OtherAddress>(param);


            Session session = Session.Instance;
            var profileModel = (Trinity.BE.ProfileModel)session[CommonConstants.CURRENT_EDIT_USER];
            profileModel.Addresses = rawDataAddress;
            profileModel.OtherAddress.Address_ID = rawDataOtherAddress.OAddress_ID;
            profileModel.OtherAddress.BlkHouse_Number = rawDataOtherAddress.OBlkHouse_Number;
            profileModel.OtherAddress.FlrUnit_Number = rawDataOtherAddress.OFlrUnit_Number;
            profileModel.OtherAddress.Street_Name = rawDataOtherAddress.OStreet_Name;
            profileModel.OtherAddress.Country = rawDataOtherAddress.OCountry;
            profileModel.OtherAddress.Postal_Code = rawDataOtherAddress.OPostal_Code;




            //var data = new Trinity.BE.ProfileRawMData().ToProfileModel(rawData);
            //data.Addresses = rawDataAddress;
            //data.OtherAddress.Address_ID = rawDataOtherAddress.OAddress_ID;
            //data.OtherAddress.BlkHouse_Number = rawDataOtherAddress.OBlkHouse_Number;
            //data.OtherAddress.FlrUnit_Number = rawDataOtherAddress.OFlrUnit_Number;
            //data.OtherAddress.Street_Name = rawDataOtherAddress.OStreet_Name;
            //data.OtherAddress.Country = rawDataOtherAddress.OCountry;
            //data.OtherAddress.Postal_Code = rawDataOtherAddress.OPostal_Code;



            //var profileModel = (Trinity.BE.ProfileModel)session[CommonConstants.CURRENT_EDIT_USER];

            var tempProfileModel = (Trinity.BE.ProfileModel)session["TEMP_USER"];

            //data.UserProfile.LeftThumbImage = profileModel.UserProfile.LeftThumbImage;
            //data.UserProfile.RightThumbImage = profileModel.UserProfile.RightThumbImage;


            var photo1 = tempProfileModel.UserProfile.User_Photo1 != null ? Convert.ToBase64String(tempProfileModel.UserProfile.User_Photo1) : null;
            var photo2 = tempProfileModel.UserProfile.User_Photo2 != null ? Convert.ToBase64String(tempProfileModel.UserProfile.User_Photo2) : null;
            ////////
            session["TempPhotos"] = new Tuple<string, string>(photo1, photo2);
            ////////
            //data.UserProfile.User_Photo1 = profileModel.UserProfile.User_Photo1;
            //data.UserProfile.User_Photo2 = profileModel.UserProfile.User_Photo2;
            //data.UserProfile.Residential_Addess_ID = profileModel.UserProfile.Residential_Addess_ID;
            //data.UserProfile.Other_Address_ID = profileModel.UserProfile.Other_Address_ID;
            //session[CommonConstants.CURRENT_EDIT_USER] = data;
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
        private int UpdateSuperviseeBio_CountPriterSmartCard = 0;
        public void UpdateSuperviseeBiodata(string frontBase64, string backBase64, string cardInfo)
        {
            UpdateSuperviseeBio_CountPriterSmartCard = 0;
            Session session = Session.Instance;
            var dalUser = new Trinity.DAL.DAL_User();
            var dalUserprofile = new Trinity.DAL.DAL_UserProfile();
            var profileModel = (Trinity.BE.ProfileModel)session[CommonConstants.CURRENT_EDIT_USER];
            if (profileModel != null)
            {
                EventCenter eventCenter = EventCenter.Default;

                eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Name = EventNames.UPDATE_SUPERVISEE_BIODATA, Data = new object[] { profileModel, frontBase64, backBase64 } });
                PrintSmartCard(frontBase64, backBase64, cardInfo);
            }
        }
        public object loadDataVerify()
        {
            Session session = Session.Instance;
            return new object[] { session[CommonConstants.CURRENT_EDIT_USER], new DAL_GetCardInfo().GetCardInfo() };
        }
        public void PrintSmartCard(string frontBase64, string backBase64, string cardInfo)
        {
            _CardInfo = JsonConvert.DeserializeObject<Trinity.BE.CardInfo>(cardInfo);
            this._web.InvokeScript("showPrintMessage", null, @"<p>Printing card in progress.Please wait.....</p>");
            frontBase64 = frontBase64.Replace("data:image/png;base64,", string.Empty);
            backBase64 = backBase64.Replace("data:image/png;base64,", string.Empty);
            Session session = Session.Instance;
            var userLogin = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];
            var profileModel = (Trinity.BE.ProfileModel)session[CommonConstants.CURRENT_EDIT_USER];

            string ImgFront = null;
            string ImgBack = null;
            if (!string.IsNullOrEmpty(frontBase64))
            {
                System.IO.Directory.CreateDirectory(String.Format("{0}/Temp", CSCallJS.curDir));
                ImgFront = String.Format("{0}/Temp/{1}", CSCallJS.curDir, "frontcard.bmp");
                if (System.IO.File.Exists(ImgFront))
                    System.IO.File.Delete(ImgFront);

                //ImgFront = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".png";
                new System.Drawing.Bitmap(new System.IO.MemoryStream(Convert.FromBase64String(frontBase64))).Save(ImgFront, System.Drawing.Imaging.ImageFormat.Bmp);
            }
            if (!string.IsNullOrEmpty(backBase64))
            {
                System.IO.Directory.CreateDirectory(String.Format("{0}/Temp", CSCallJS.curDir));
                ImgBack = String.Format("{0}/Temp/{1}", CSCallJS.curDir, "backcard.bmp");
                if (System.IO.File.Exists(ImgBack))
                    System.IO.File.Delete(ImgBack);

                //ImgBack = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".png";
                new System.Drawing.Bitmap(new System.IO.MemoryStream(Convert.FromBase64String(backBase64))).Save(ImgBack, System.Drawing.Imaging.ImageFormat.Bmp);
            }

            PrintAndWriteSmartCardInfo infoPrinter = new PrintAndWriteSmartCardInfo()
            {
                BackCardImagePath = ImgBack,
                FrontCardImagePath = ImgFront,
                SuperviseeBiodata = new SuperviseeBiodata()
                {
                    Name = profileModel.User.Name,
                    NRIC = profileModel.User.NRIC,
                    UserId = profileModel.User.UserId,
                    DOB = profileModel.UserProfile == null ? string.Empty : profileModel.UserProfile.DOBAsString
                }
            };
            SmartCardPrinterUtil.Instance.PrintAndWriteSmartCard(infoPrinter, OnNewCardPrinted);
        }
        private void OnNewCardPrinted(PrintAndWriteCardResult result)
        {
            UpdateSuperviseeBio_CountPriterSmartCard++;
            Session session = Session.Instance;
            var userLogin = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];
            var currentEditUser = (Trinity.BE.ProfileModel)session[CommonConstants.CURRENT_EDIT_USER];
            if (result.Success)
            {
                try
                {
                    DAL_IssueCard dalIssueCard = new Trinity.DAL.DAL_IssueCard();
                    string SmartID = result.CardUID;
                    Trinity.BE.IssueCard IssueCard = new Trinity.BE.IssueCard()
                    {
                        CreatedBy = userLogin.UserId,
                        CreatedDate = DateTime.Now,
                        Date_Of_Issue = _CardInfo.Date_Of_Issue,
                        Name = currentEditUser.Membership_Users.Name,
                        NRIC = currentEditUser.Membership_Users.NRIC,
                        Reprint_Reason = string.Empty,
                        Serial_Number = _CardInfo.CardNumberFull,
                        Expired_Date = _CardInfo.Expired_Date,
                        Status = EnumIssuedCards.Inactive,
                        SmartCardId = SmartID,
                        UserId = currentEditUser.User.UserId
                    };
                    dalIssueCard.Insert(IssueCard);
                    //new DAL_Membership_Users().UpdateSmartCardId(currentEditUser.User.UserId, SmartID);
                    ////new DAL_User().ChangeUserStatus(currentEditUser.User.UserId, EnumUserStatuses.Enrolled);
                    //new DAL_UserProfile().UpdateCardInfo(currentEditUser.User.UserId, _CardInfo.CardNumberFull, _CardInfo.Date_Of_Issue, _CardInfo.Expired_Date);
                    //currentEditUser.UserProfile.Expired_Date = _CardInfo.Expired_Date;
                    //currentEditUser.UserProfile.DateOfIssue = _CardInfo.Date_Of_Issue;
                    //currentEditUser.UserProfile.SerialNumber = _CardInfo.CardNumberFull;
                    //currentEditUser.Membership_Users.SmartCardId = SmartID;
                    this._web.InvokeScript("showPrintMessage", true, "Smart Card was printed successfully! Please collect the smart card from printer and place on the reader to verify.");

                }
                catch (Exception ex)
                {
                    this._web.InvokeScript("showPrintMessage", false, EnumMessage.SmartCardIsAlreadyInUse);
                    if (UpdateSuperviseeBio_CountPriterSmartCard > 3)
                    {
                        Trinity.SignalR.Client.Instance.SendToAppDutyOfficers(userLogin.UserId, "Smart card printing too 3 times unsuccessful", "Smart card printing too 3 times unsuccessful", EnumNotificationTypes.Error);
                        UpdateSuperviseeBio_CountPriterSmartCard = 0;
                    }
                }

            }
            else
            {
                if (UpdateSuperviseeBio_CountPriterSmartCard > 3)
                {
                    Trinity.SignalR.Client.Instance.SendToAppDutyOfficers(userLogin.UserId, "Smart card printing too 3 times unsuccessful", "Smart card printing too 3 times unsuccessful", EnumNotificationTypes.Error);
                    UpdateSuperviseeBio_CountPriterSmartCard = 0;
                }
                this._web.InvokeScript("showPrintMessage", false, result.Description);
            }
        }

        public void CheckVerfyCard()
        {
            SmartCardReaderUtil smartCardReaderUtil = SmartCardReaderUtil.Instance;
            SmartCardData_Original smartCardData_Original = null;
            bool readDataResult = smartCardReaderUtil.ReadAllData_MifareClassic(ref smartCardData_Original);
            if (!readDataResult)
            {
                this._web.InvokeScript("showCheckVerfyCard", false, "Can not read information smart card.");
            }
            else
            {
                this._web.InvokeScript("showPrintMessage", true, "");
                this._web.InvokeScript("showCheckVerfyCard", true, JsonConvert.SerializeObject(new
                {
                    Name = smartCardData_Original.SuperviseeBiodata.Name,
                    NRIC = smartCardData_Original.SuperviseeBiodata.NRIC,
                    DOB = smartCardData_Original.SuperviseeBiodata.DOB
                }));
            }
        }
        public void DoneEnrolSupervisee()
        {
            Session session = Session.Instance;
            var currentEditUser = (Trinity.BE.ProfileModel)session[CommonConstants.CURRENT_EDIT_USER];
            var data = new DAL_User().DoneEnrolSupervisee(currentEditUser.User.UserId);

            currentEditUser.UserProfile.Expired_Date = data.Expired_Date;
            currentEditUser.UserProfile.DateOfIssue = data.Date_Of_Issue;
            currentEditUser.UserProfile.SerialNumber = data.Serial_Number;
            currentEditUser.Membership_Users.SmartCardId = data.SmartCardId;
            _web.LoadPageHtml("Supervisee.html");
        }
        public void AddNewSupervisee()
        {
            _web.LoadPageHtml("New-Supervisee.html");
        }

        #region Webcam event
        public void OpenPictureCaptureForm(string number, string from = null)
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

            // _web.LoadPageHtml("WebcamCapture.html");
            eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Name = EventNames.OPEN_PICTURE_CAPTURE_FORM, Message = number, Data = from });
        }

        public void CancelEditSupervisee()
        {
            var a = Lib.LayerWeb;
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
        public void ConfirmCapturePicture(string base64Img)
        {

            Bitmap Img;
            using (var ms = new MemoryStream(Convert.FromBase64String(base64Img)))
            {
                Img = new Bitmap(ms);
            }

            int height = Img.Height;
            if (height > ImageAttr.Height)
            {
                height = ImageAttr.Height;
            }
            int width = Convert.ToInt32(height / ImageAttr.Ratio);
            base64Img = Convert.ToBase64String(Img.ResizeImage(width, height).ImageToByte());
            EventCenter eventCenter = EventCenter.Default;
            eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Name = EventNames.CONFIRM_CAPTURE_PICTURE, Data = base64Img });
        }
        public void CancelConfirmCapturePicture()
        {
            EventCenter eventCenter = EventCenter.Default;
            eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Name = EventNames.CANCEL_CONFIRM_CAPTURE_PICTURE });
        }
        #endregion

        #region Authentication & Authorization

        private static int _count = 0;
        public void Login(string username, string password)
        {
            _count++;
            EventCenter eventCenter = EventCenter.Default;
            var dalUser = new DAL_User();
            ApplicationUser appUser = dalUser.Login(username, password);
            if (appUser != null)
            {
                var userInfo = dalUser.GetUserById(appUser.Id);
                if (userInfo.AccessFailedCount >= 3)
                {
                    eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Code = -1, Name = EventNames.LOGIN_FAILED, Message = "You have exceeded the maximum amount of tries to Login. Please goto APS and select \"Forgot Password\" to reset your password." });
                    return;
                }
                // Authenticated successfully
                // Reset AccessFailedCount
                dalUser.ChangeAccessFailedCount(appUser.Id, 0);
                // Check if the current user is an Enrolment Officer or not
                if (dalUser.IsInRole(appUser.Id, EnumUserRoles.EnrolmentOfficer))
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

                    Trinity.SignalR.Client.Instance.UserLoggedIn(user.UserId);

                    eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Code = 0, Name = EventNames.LOGIN_SUCCEEDED });
                }
                else
                {
                    _web.ShowMessage("You do not have permission to access this page.");
                    //eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Code = -2, Name = EventNames.LOGIN_FAILED, Message = "You do not have permission to access this page." });
                }
            }
            else
            {
                ApplicationUser user = dalUser.FindByName(username);
                if (user != null)
                {
                    var userInfo = dalUser.GetUserByUserId(user.Id).Data;
                    dalUser.ChangeAccessFailedCount(user.Id, userInfo.AccessFailedCount + 1);
                }
                _web.ShowMessage("Your username or password is incorrect.");
                //eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Code = -1, Name = EventNames.LOGIN_FAILED, Message = "Your username or password is incorrect." });
            }
        }

        public void LogOut()
        {
            // reset session value
            Session session = Session.Instance;

            Trinity.SignalR.Client.Instance.UserLoggedOut(((Trinity.BE.User)session[CommonConstants.USER_LOGIN]).UserId);

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
            byte[] _left = new byte[] { };
            byte[] _right = new byte[] { };


            if (!string.IsNullOrEmpty(left))
                _left = Convert.FromBase64String(left);
            if (!string.IsNullOrEmpty(right))
                _right = Convert.FromBase64String(right);

            byte[] _leftImg = new byte[] { };
            byte[] _rightImg = new byte[] { };

            if (!string.IsNullOrEmpty(leftImg))
                _leftImg = Convert.FromBase64String(leftImg);
            if (!string.IsNullOrEmpty(rightImg))
                _rightImg = Convert.FromBase64String(rightImg);

            new DAL_Membership_Users().UpdateFingerprint(currentEditUser.User.UserId, _left, _right);
            new DAL_UserProfile().UpdateFingerprintImg(currentEditUser.User.UserId, _leftImg, _rightImg);
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
            EditSupervisee(currentEditUser.User.UserId);
        }

        public void CancelUpdatePicture()
        {
            EventCenter eventCenter = EventCenter.Default;
            eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Name = EventNames.CANCEL_UPDATE_PICTURE });
        }
        public void UpdatePhotos()
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
            this._web.LoadPageHtml("UpdateSuperviseePhoto.html", currentEditUser);
            this._web.InvokeScript("setAvatar", photo1, photo2);
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
            FingerprintReaderUtil.Instance.DisposeCapture();
            Session session = Session.Instance;
            EditSupervisee(((Trinity.BE.ProfileModel)session[CommonConstants.CURRENT_EDIT_USER]).User.UserId);
        }
        public void CaptureFingerprint(int LeftOrRight)
        {
            FingerprintLeftRight = LeftOrRight;
            FingerprintReaderUtil.Instance.StartCapture(OnPutOn, OnTakeOff, UpdateScreenImage, OnFakeSource, OnEnrollmentComplete);
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
            if (bSuccess && FingerprintReaderUtil.Instance.GetQuality >= 7)
            {
                _web.InvokeScript("setDataFingerprint", FingerprintLeftRight, Convert.ToBase64String(FingerprintReaderUtil.Instance.GetTemplate));
                _web.InvokeScript("captureFingerprintMessage", FingerprintLeftRight, "Your fingerprint was scanned successfully!", EnumColors.Green);
                FingerprintNumber = 0;
            }
            else
            {
                if (bSuccess)
                {
                    _web.InvokeScript("captureFingerprintMessage", FingerprintLeftRight, "Please use the same thumb for verification.", EnumColors.Red);
                }
                else
                {
                    _web.InvokeScript("captureFingerprintMessage", FingerprintLeftRight, Futronic.SDKHelper.FutronicSdkBase.SdkRetCode2Message(nResult), EnumColors.Red);
                }
                FingerprintNumber++;
                if (FingerprintNumber >= 3)
                    _web.InvokeScript("moreThan3Fingerprint");
            }
            _web.InvokeScript("disBtnFingerprint", false);
            try
            {
                FingerprintReaderUtil.Instance.DisposeCapture();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error in Trinity.Enrolment.JSCallCS.OnEnrollmentComplete. Details: " + ex.Message);
            }
        }
        private bool OnFakeSource(Futronic.SDKHelper.FTR_PROGRESS Progress)
        {
            _web.InvokeScript("captureFingerprintMessage", FingerprintLeftRight, "No thumb detected.", EnumColors.Red);
            return false;
        }
        private void OnTakeOff(Futronic.SDKHelper.FTR_PROGRESS Progress)
        {
            _web.InvokeScript("captureFingerprintMessage", FingerprintLeftRight, "Remove thumb from fingerprint scanner.", EnumColors.Yellow);
        }
        private void OnPutOn(Futronic.SDKHelper.FTR_PROGRESS Progress)
        {
            _web.InvokeScript("captureFingerprintMessage", FingerprintLeftRight, "Place thumb on the fingerprint reader.", EnumColors.Yellow);
        }
        #endregion


        #endregion

        #region Issued Cards
        private string reprintTxt = string.Empty;
        private Trinity.BE.CardInfo _CardInfo = null;
        public object[] GetIssuedCards()
        {
            Session session = Session.Instance;
            var currentEditUser = (Trinity.BE.ProfileModel)session[CommonConstants.CURRENT_EDIT_USER];
            List<Trinity.BE.IssueCard> array = new Trinity.DAL.DAL_IssueCard().GetMyIssueCards(currentEditUser.User.UserId);
            return new object[] { array, currentEditUser.User.UserId };
        }
        public void ReprintIssuedCard(string reprintReason, string cardInfo, string frontBase64, string backBase64)
        {
            _CardInfo = JsonConvert.DeserializeObject<Trinity.BE.CardInfo>(cardInfo);
            frontBase64 = frontBase64.Replace("data:image/png;base64,", string.Empty);
            backBase64 = backBase64.Replace("data:image/png;base64,", string.Empty);
            string ImgFront = null;
            string ImgBack = null;
            if (!string.IsNullOrEmpty(frontBase64))
            {
                System.IO.Directory.CreateDirectory(String.Format("{0}/Temp", CSCallJS.curDir));
                ImgFront = String.Format("{0}/Temp/{1}", CSCallJS.curDir, "frontcard.bmp");
                new System.Drawing.Bitmap(new System.IO.MemoryStream(Convert.FromBase64String(frontBase64))).Save(ImgFront, System.Drawing.Imaging.ImageFormat.Bmp);
            }
            if (!string.IsNullOrEmpty(backBase64))
            {
                System.IO.Directory.CreateDirectory(String.Format("{0}/Temp", CSCallJS.curDir));
                ImgBack = String.Format("{0}/Temp/{1}", CSCallJS.curDir, "backcard.bmp");
                new System.Drawing.Bitmap(new System.IO.MemoryStream(Convert.FromBase64String(backBase64))).Save(ImgBack, System.Drawing.Imaging.ImageFormat.Bmp);
            }

            reprintTxt = reprintReason;
            Session session = Session.Instance;
            var userLogin = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];
            var currentEditUser = (Trinity.BE.ProfileModel)session[CommonConstants.CURRENT_EDIT_USER];


            SmartCardPrinterUtil.Instance.PrintAndWriteSmartCard(new PrintAndWriteSmartCardInfo()
            {
                BackCardImagePath = ImgBack,
                FrontCardImagePath = ImgFront,
                SuperviseeBiodata = new SuperviseeBiodata()
                {
                    Name = currentEditUser.User.Name,
                    NRIC = currentEditUser.User.NRIC,
                    UserId = currentEditUser.User.UserId,
                    DOB = currentEditUser.UserProfile == null ? string.Empty : currentEditUser.UserProfile.DOBAsString
                }
            }, OnIssuedCardReprinted);
        }
        private void OnIssuedCardReprinted(PrintAndWriteCardResult result)
        {
            if (result.Success)
            {
                Session session = Session.Instance;
                var userLogin = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];
                var currentEditUser = (Trinity.BE.ProfileModel)session[CommonConstants.CURRENT_EDIT_USER];
                DAL_IssueCard dalIssueCard = new Trinity.DAL.DAL_IssueCard();
                string SmartID = result.CardUID;
                Trinity.BE.IssueCard IssueCard = new Trinity.BE.IssueCard()
                {
                    CreatedBy = userLogin.UserId,
                    CreatedDate = DateTime.Now,
                    Name = currentEditUser.Membership_Users.Name,
                    NRIC = currentEditUser.Membership_Users.NRIC,
                    Reprint_Reason = reprintTxt,
                    Serial_Number = _CardInfo.CardNumberFull,
                    Date_Of_Issue = _CardInfo.Date_Of_Issue,
                    Expired_Date = _CardInfo.Expired_Date,
                    Status = EnumIssuedCards.Active,
                    SmartCardId = SmartID,
                    UserId = currentEditUser.User.UserId
                };
                dalIssueCard.Insert(IssueCard);
                new DAL_Membership_Users().UpdateSmartCardId(currentEditUser.User.UserId, SmartID);
                new DAL_UserProfile().UpdateCardInfo(currentEditUser.User.UserId, _CardInfo.CardNumberFull, _CardInfo.Date_Of_Issue, _CardInfo.Expired_Date);
                currentEditUser.UserProfile.Expired_Date = _CardInfo.Expired_Date;
                currentEditUser.UserProfile.DateOfIssue = _CardInfo.Date_Of_Issue;
                currentEditUser.UserProfile.SerialNumber = _CardInfo.CardNumberFull;
                currentEditUser.Membership_Users.SmartCardId = SmartID;
                _web.InvokeScript("OnIssuedCardReprinted", true, string.Empty);
            }
            else
            {
                _web.InvokeScript("OnIssuedCardReprinted", false, result.Description);
            }
        }
        public bool checkStepNew(int step)
        {
            Session session = Session.Instance;
            var currentEditUser = (Trinity.BE.ProfileModel)session[CommonConstants.CURRENT_EDIT_USER];

            if (currentEditUser != null && currentEditUser.UserProfile != null)
            {
                if (step == 0)
                {
                    if (currentEditUser.UserProfile.User_Photo1 != null && currentEditUser.UserProfile.User_Photo2 != null)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (step == 1)
                {
                    if (currentEditUser.UserProfile.LeftThumbImage != null && currentEditUser.UserProfile.RightThumbImage != null)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return false;
        }
        #endregion
    }
}
