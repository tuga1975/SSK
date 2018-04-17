using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity.BE;
using Trinity.DAL.DBContext;
using Trinity.DAL.Repository;
using Trinity.Common;
using System.Reflection;

namespace Trinity.DAL
{
    public class DAL_UserProfile
    {
        Local_UnitOfWork _localUnitOfWork = new Local_UnitOfWork();
        Centralized_UnitOfWork _centralizedUnitOfWork = new Centralized_UnitOfWork();

        #region 2018

        public void CreateUserProfileIfNotExit(string UserID)
        {
            DAL.DBContext.User_Profiles userProfile = _localUnitOfWork.DataContext.User_Profiles.FirstOrDefault(d => d.UserId.Equals(UserID));
            if (userProfile == null)
            {
                userProfile = new User_Profiles()
                {
                    UserId = UserID
                };
                _localUnitOfWork.GetRepository<DAL.DBContext.User_Profiles>().Add(userProfile);
                _localUnitOfWork.Save();
            }
        }

        public bool ARKUpdateProfile(string UserID, Dictionary<string, Dictionary<string, object>> dataUpdate, List<string> arrayScanDocument)
        {
            bool isUpdateUser_Profiles = false;
            bool isSaveDataBase = false;
            CreateUserProfileIfNotExit(UserID);
            DAL.DBContext.User_Profiles userProfile = _localUnitOfWork.DataContext.User_Profiles.FirstOrDefault(d => d.UserId.Equals(UserID));
            DAL.DBContext.Address Alternate_Addresses = null;
            if (dataUpdate.ContainsKey("Alternate_Addresses"))
            {
                if (!string.IsNullOrEmpty(userProfile.Other_Address_ID))
                {
                    Alternate_Addresses = _localUnitOfWork.DataContext.Addresses.FirstOrDefault(d => d.Address_ID.Equals(userProfile.Other_Address_ID));
                }
                if (Alternate_Addresses == null)
                {
                    Alternate_Addresses = new DBContext.Address()
                    {
                        Address_ID = Guid.NewGuid().ToString().Trim()
                    };
                    foreach (var item in dataUpdate["Alternate_Addresses"])
                    {
                        PropertyInfo propertyInfo = Alternate_Addresses.GetType().GetProperty(item.Key);
                        propertyInfo.SetValue(Alternate_Addresses, Convert.ChangeType(item.Value, propertyInfo.PropertyType), null);
                    }
                    userProfile.Other_Address_ID = Alternate_Addresses.Address_ID;
                    _localUnitOfWork.GetRepository<DAL.DBContext.Address>().Add(Alternate_Addresses);
                    isUpdateUser_Profiles = true;
                    isSaveDataBase = true;
                }
                else
                {
                    bool isUpdateAddress = false;
                    foreach (var item in dataUpdate["Alternate_Addresses"])
                    {
                        PropertyInfo propertyInfo = Alternate_Addresses.GetType().GetProperty(item.Key);
                        propertyInfo.SetValue(Alternate_Addresses, Convert.ChangeType(item.Value, propertyInfo.PropertyType), null);
                        isSaveDataBase = true;
                        isUpdateAddress = true;
                    }
                    if (isUpdateAddress)
                    {
                        _localUnitOfWork.GetRepository<DAL.DBContext.Address>().Update(Alternate_Addresses);
                    }
                }
            }

            if (dataUpdate.ContainsKey("User_Profiles"))
            {
                foreach (var item in dataUpdate["User_Profiles"])
                {
                    PropertyInfo propertyInfo = userProfile.GetType().GetProperty(item.Key);
                    var underlyingType = Nullable.GetUnderlyingType(propertyInfo.PropertyType);
                    propertyInfo.SetValue(userProfile, Convert.ChangeType(item.Value, underlyingType ?? propertyInfo.PropertyType), null);
                    isUpdateUser_Profiles = true;
                    isSaveDataBase = true;
                }
            }

            if (arrayScanDocument.Count > 0)
            {
                Guid IDDocuemnt = new DAL_UploadedDocuments().Insert(arrayScanDocument, userProfile.UserId);
                userProfile.Document_ID = IDDocuemnt;
                isUpdateUser_Profiles = true;
                isSaveDataBase = true;
            }


            if ((!userProfile.Employment_Start_Date.HasValue && userProfile.Employment_End_Date.HasValue) || (userProfile.Employment_Start_Date.HasValue && userProfile.Employment_End_Date.HasValue && userProfile.Employment_Start_Date.Value >= userProfile.Employment_End_Date.Value))
            {
                Lib.LayerWeb.ShowMessage("<b>Employment Start Date</b> must be less than <b>Employment End Date</b>");
                return false;
            }
            else
            {
                if (isUpdateUser_Profiles)
                {
                    _localUnitOfWork.GetRepository<DAL.DBContext.User_Profiles>().Update(userProfile);
                }
                if (isSaveDataBase)
                {
                    _localUnitOfWork.Save();
                }
            }
            return true;
        }
        public void UploadDocumentScan(Guid IDDocument, string UserID)
        {
            var use = _localUnitOfWork.DataContext.User_Profiles.FirstOrDefault(d => d.UserId == UserID);
            use.Document_ID = IDDocument;
            _localUnitOfWork.GetRepository<DBContext.User_Profiles>().Update(use);
            _localUnitOfWork.Save();
        }
        public void UpdateCardInfo(string UserId, string CardNumber, DateTime Date_of_Issue, DateTime Expired_Date)
        {
            if (EnumAppConfig.IsLocal)
            {
                bool statusCentralized;
                CallCentralized.Post("User_Profiles", "UpdateCardInfo", out statusCentralized, "UserId=" + UserId, "CardNumber=" + CardNumber, "Date_of_Issue=" + Date_of_Issue.ToString(), "Expired_Date=" + Expired_Date.ToString());
                if (!statusCentralized)
                {
                    throw new Exception(EnumMessage.NotConnectCentralized);
                }
                else
                {
                    DBContext.User_Profiles user = _localUnitOfWork.DataContext.User_Profiles.FirstOrDefault(d => d.UserId == UserId);
                    user.Serial_Number = CardNumber;
                    user.Date_of_Issue = Date_of_Issue;
                    user.Expired_Date = Expired_Date;
                    _localUnitOfWork.GetRepository<DBContext.User_Profiles>().Update(user);
                    _localUnitOfWork.Save();
                }


            }
            else
            {
                DBContext.User_Profiles user = _centralizedUnitOfWork.DataContext.User_Profiles.FirstOrDefault(d => d.UserId == UserId);
                user.Serial_Number = CardNumber;
                user.Date_of_Issue = Date_of_Issue;
                user.Expired_Date = Expired_Date;
                _centralizedUnitOfWork.GetRepository<DBContext.User_Profiles>().Update(user);
                _centralizedUnitOfWork.Save();
            }


        }

        public void UpdateFingerprintImg(string userId, byte[] left, byte[] right)
        {
            if (EnumAppConfig.IsLocal)
            {
                bool statusCentralized;
                CallCentralized.Post<bool>("User_Profiles", "UpdateFingerprintImg", out statusCentralized, new object[] { userId, left, right });
                if (!statusCentralized)
                {
                    throw new Exception(EnumMessage.NotConnectCentralized);
                }
                else
                {
                    User_Profiles user = this._localUnitOfWork.DataContext.User_Profiles.FirstOrDefault(d => d.UserId == userId);
                    if (left != null && left.Length > 0)
                        user.LeftThumb_Photo = left;
                    if (right != null && right.Length > 0)
                        user.RightThumb_Photo = right;
                    if ((left != null && left.Length > 0) || (right != null && right.Length > 0))
                    {
                        this._localUnitOfWork.GetRepository<User_Profiles>().Update(user);
                        this._localUnitOfWork.Save();
                    }
                }


            }
            else
            {
                User_Profiles user = this._centralizedUnitOfWork.DataContext.User_Profiles.FirstOrDefault(d => d.UserId == userId);
                if (left != null && left.Length > 0)
                    user.LeftThumb_Photo = left;
                if (right != null && right.Length > 0)
                    user.RightThumb_Photo = right;
                if ((left != null && left.Length > 0) || (right != null && right.Length > 0))
                {
                    this._centralizedUnitOfWork.GetRepository<User_Profiles>().Update(user);
                    this._centralizedUnitOfWork.Save();
                }
            }

        }
        public bool UpdateProfile(BE.UserProfile model)
        {
            if (EnumAppConfig.IsLocal)
            {
                bool statusCentralized;
                CallCentralized.Post<bool>("User_Profiles", "UpdateProfile", out statusCentralized, model);
                if (!statusCentralized)
                {
                    throw new Exception(EnumMessage.NotConnectCentralized);
                }
                else
                {
                    User_Profiles dbUserProfile;
                    var localUserProfileRepo = _localUnitOfWork.GetRepository<User_Profiles>();
                    dbUserProfile = localUserProfileRepo.GetById(model.UserId);
                    if (dbUserProfile == null)
                    {
                        dbUserProfile = new User_Profiles();
                        SetInfo(dbUserProfile, model);
                        localUserProfileRepo.Add(dbUserProfile);
                    }
                    else
                    {
                        SetInfo(dbUserProfile, model);
                        localUserProfileRepo.Update(dbUserProfile);
                    }
                    return _localUnitOfWork.Save() > 0;
                }
            }
            else
            {
                var centralUserProfileRepo = _centralizedUnitOfWork.GetRepository<User_Profiles>();
                User_Profiles dbUserProfile = centralUserProfileRepo.GetById(model.UserId);
                if (dbUserProfile == null)
                {
                    dbUserProfile = new User_Profiles();
                    SetInfo(dbUserProfile, model);
                    centralUserProfileRepo.Add(dbUserProfile);
                }
                else
                {
                    SetInfo(dbUserProfile, model);
                    centralUserProfileRepo.Update(dbUserProfile);
                }
                return _centralizedUnitOfWork.Save() > 0;
            }
        }
        public BE.Address GetAddByUserId(string userId, bool isOther = false)
        {
            UserProfile user = GetProfile(userId);
            if (user == null)
                return null;
            string AddressID = isOther ? user.Other_Address_ID : user.Residential_Addess_ID;
            if (string.IsNullOrEmpty(AddressID))
                return null;

            DBContext.Address addressData = _localUnitOfWork.DataContext.Addresses.FirstOrDefault(a => a.Address_ID == AddressID);

            if (addressData != null)
            {
                var address = new BE.Address
                {
                    Address_ID = addressData.Address_ID,
                    BlkHouse_Number = addressData.BlkHouse_Number,
                    Country = addressData.Country,
                    FlrUnit_Number = addressData.FlrUnit_Number,
                    Postal_Code = addressData.Postal_Code,
                    Street_Name = addressData.Street_Name
                };
                return address;
            }
            return null;
        }
        public UserProfile GetProfile(string userId)
        {
            User_Profiles dbUserProfile = null;
            if (EnumAppConfig.IsLocal)
            {
                dbUserProfile = _localUnitOfWork.DataContext.User_Profiles.FirstOrDefault(u => u.UserId == userId);
                if (dbUserProfile == null && !EnumAppConfig.ByPassCentralizedDB)
                {
                    UserProfile data = CallCentralized.Get<UserProfile>(EnumAPIParam.User, "GetProfileByUserId", "userId=" + userId);
                    return data;
                }
            }
            else
            {
                dbUserProfile = _centralizedUnitOfWork.DataContext.User_Profiles.FirstOrDefault(u => u.UserId == userId);
            }

            if (dbUserProfile != null)
            {
                var userProfile = new BE.UserProfile
                {
                    DOB = dbUserProfile.DOB,
                    Employment_Company_Name = dbUserProfile.Employment_Company_Name,
                    Employment_Contact_Number = dbUserProfile.Employment_Contact_Number,
                    Employment_End_Date = dbUserProfile.Employment_End_Date,
                    Employment_Job_Title = dbUserProfile.Employment_Job_Title,
                    Employment_Name = dbUserProfile.Employment_Name,
                    Employment_Remarks = dbUserProfile.Employment_Remarks,
                    Employment_Start_Date = dbUserProfile.Employment_Start_Date,
                    Maritial_Status = dbUserProfile.Maritial_Status,
                    Nationality = dbUserProfile.Nationality,
                    NextOfKin_BlkHouse_Number = dbUserProfile.NextOfKin_BlkHouse_Number,
                    NextOfKin_Contact_Number = dbUserProfile.NextOfKin_Contact_Number,
                    NextOfKin_Country = dbUserProfile.NextOfKin_Country,
                    NextOfKin_FlrUnit_Number = dbUserProfile.NextOfKin_FlrUnit_Number,
                    NextOfKin_Name = dbUserProfile.NextOfKin_Name,
                    NextOfKin_PostalCode = dbUserProfile.NextOfKin_PostalCode,
                    NextOfKin_Relationship = dbUserProfile.NextOfKin_Relationship,
                    NextOfKin_Street_Name = dbUserProfile.NextOfKin_Street_Name,
                    Other_Address_ID = dbUserProfile.Other_Address_ID,
                    Primary_Email = dbUserProfile.Primary_Email,
                    Primary_Phone = dbUserProfile.Primary_Phone,
                    Residential_Addess_ID = dbUserProfile.Residential_Addess_ID,
                    Secondary_Email = dbUserProfile.Secondary_Email,
                    Secondary_Phone = dbUserProfile.Secondary_Phone,
                    UserId = dbUserProfile.UserId,
                    User_Photo1 = dbUserProfile.User_Photo1,
                    User_Photo2 = dbUserProfile.User_Photo2,
                    DateOfIssue = dbUserProfile.Date_of_Issue,
                    Gender = dbUserProfile.Gender,
                    Race = dbUserProfile.Race,
                    SerialNumber = dbUserProfile.Serial_Number,
                    LeftThumbImage = dbUserProfile.LeftThumb_Photo,
                    RightThumbImage = dbUserProfile.RightThumb_Photo,
                    Expired_Date = dbUserProfile.Expired_Date,
                };
                return userProfile;
            }
            return null;
        }
        #endregion
        public Response<UserProfile> GetUserProfileByUserId(string userId)
        {
            User_Profiles dbUserProfile = null;
            if (EnumAppConfig.IsLocal)
            {
                dbUserProfile = _localUnitOfWork.DataContext.User_Profiles.FirstOrDefault(u => u.UserId == userId);
            }
            else
            {
                dbUserProfile = _centralizedUnitOfWork.DataContext.User_Profiles.FirstOrDefault(u => u.UserId == userId);
            }
            if (dbUserProfile != null)
            {
                var userProfile = new BE.UserProfile
                {
                    DOB = dbUserProfile.DOB,
                    Employment_Company_Name = dbUserProfile.Employment_Company_Name,
                    Employment_Contact_Number = dbUserProfile.Employment_Contact_Number,
                    Employment_End_Date = dbUserProfile.Employment_End_Date,
                    Employment_Job_Title = dbUserProfile.Employment_Job_Title,
                    Employment_Name = dbUserProfile.Employment_Name,
                    Employment_Remarks = dbUserProfile.Employment_Remarks,
                    Employment_Start_Date = dbUserProfile.Employment_Start_Date,
                    Maritial_Status = dbUserProfile.Maritial_Status,
                    Nationality = dbUserProfile.Nationality,
                    NextOfKin_BlkHouse_Number = dbUserProfile.NextOfKin_BlkHouse_Number,
                    NextOfKin_Contact_Number = dbUserProfile.NextOfKin_Contact_Number,
                    NextOfKin_Country = dbUserProfile.NextOfKin_Country,
                    NextOfKin_FlrUnit_Number = dbUserProfile.NextOfKin_FlrUnit_Number,
                    NextOfKin_Name = dbUserProfile.NextOfKin_Name,
                    NextOfKin_PostalCode = dbUserProfile.NextOfKin_PostalCode,
                    NextOfKin_Relationship = dbUserProfile.NextOfKin_Relationship,
                    NextOfKin_Street_Name = dbUserProfile.NextOfKin_Street_Name,
                    Other_Address_ID = dbUserProfile.Other_Address_ID,
                    Primary_Email = dbUserProfile.Primary_Email,
                    Primary_Phone = dbUserProfile.Primary_Phone,
                    Residential_Addess_ID = dbUserProfile.Residential_Addess_ID,
                    Secondary_Email = dbUserProfile.Secondary_Email,
                    Secondary_Phone = dbUserProfile.Secondary_Phone,
                    UserId = dbUserProfile.UserId,
                    User_Photo1 = dbUserProfile.User_Photo1,
                    User_Photo2 = dbUserProfile.User_Photo2,
                    DateOfIssue = dbUserProfile.Date_of_Issue,
                    Gender = dbUserProfile.Gender,
                    Race = dbUserProfile.Race,
                    SerialNumber = dbUserProfile.Serial_Number,
                    LeftThumbImage = dbUserProfile.LeftThumb_Photo,
                    RightThumbImage = dbUserProfile.RightThumb_Photo,
                    Expired_Date = dbUserProfile.Expired_Date,
                };
                return new Response<UserProfile>((int)EnumResponseStatuses.Success, EnumResponseMessage.Success, userProfile);
            }
            return new Response<UserProfile>((int)EnumResponseStatuses.ErrorSystem, EnumResponseMessage.ErrorSystem, null);
        }
        public Response<bool> UpdateUserProfile(BE.UserProfile model)
        {
            try
            {
                User_Profiles dbUserProfile = null;
                if (EnumAppConfig.IsLocal)
                {
                    dbUserProfile = UpdateLocal(model, model.UserId);
                    dbUserProfile = UpdateCentral(model, model.UserId);
                    return new Response<bool>((int)EnumResponseStatuses.Success, EnumResponseMessage.Success, true);
                }
                else
                {
                    dbUserProfile = UpdateCentral(model, model.UserId);
                    return new Response<bool>((int)EnumResponseStatuses.Success, EnumResponseMessage.Success, true);
                }
            }
            catch (Exception ex)
            {
                return new Response<bool>((int)EnumResponseStatuses.ErrorSystem, EnumResponseMessage.ErrorSystem, false);
            }
        }

        //public bool UpdateProfile(BE.UserProfile model)
        //{
        //    try
        //    {
        //        User_Profiles dbUserProfile = null;
        //        if (EnumAppConfig.IsLocal)
        //        {
        //            dbUserProfile = UpdateLocal(model, model.UserId);
        //            dbUserProfile = UpdateCentral(model, model.UserId);
        //            return true;
        //        }
        //        else
        //        {
        //            dbUserProfile = UpdateCentral(model, model.UserId);
        //            return true;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return false;
        //    }
        //}
        private User_Profiles UpdateCentral(UserProfile model, string userId)
        {
            User_Profiles dbUserProfile = new User_Profiles();
            try
            {
                var centralUserProfileRepo = _centralizedUnitOfWork.GetRepository<User_Profiles>();
                dbUserProfile = centralUserProfileRepo.GetById(userId);
                if (dbUserProfile == null)
                {
                    dbUserProfile = new User_Profiles();
                    SetInfo(dbUserProfile, model);
                    centralUserProfileRepo.Add(dbUserProfile);
                }
                else
                {
                    SetInfo(dbUserProfile, model);
                    centralUserProfileRepo.Update(dbUserProfile);
                }
                _centralizedUnitOfWork.Save();
            }
            catch (Exception ex)
            {

                return dbUserProfile;
            }
            return dbUserProfile;
        }

        private User_Profiles UpdateLocal(UserProfile model, string userId)
        {
            User_Profiles dbUserProfile;
            var localUserProfileRepo = _localUnitOfWork.GetRepository<User_Profiles>();
            dbUserProfile = localUserProfileRepo.GetById(userId);
            if (dbUserProfile == null)
            {
                dbUserProfile = new User_Profiles();
                SetInfo(dbUserProfile, model);
                localUserProfileRepo.Add(dbUserProfile);
            }
            else
            {
                SetInfo(dbUserProfile, model);
                localUserProfileRepo.Update(dbUserProfile);
            }
            _localUnitOfWork.Save();
            return dbUserProfile;
        }

        protected void SetInfo(User_Profiles dbUserProfile, BE.UserProfile model)
        {
            dbUserProfile.UserId = model.UserId;
            dbUserProfile.DOB = model.DOB;
            dbUserProfile.Employment_Company_Name = model.Employment_Company_Name;
            dbUserProfile.Employment_Contact_Number = model.Employment_Contact_Number;
            dbUserProfile.Employment_End_Date = model.Employment_End_Date;
            dbUserProfile.Employment_Job_Title = model.Employment_Job_Title;
            dbUserProfile.Employment_Name = model.Employment_Name;
            dbUserProfile.Employment_Remarks = model.Employment_Remarks;
            dbUserProfile.Employment_Start_Date = model.Employment_Start_Date;
            dbUserProfile.Maritial_Status = model.Maritial_Status;
            dbUserProfile.Nationality = model.Nationality;
            dbUserProfile.NextOfKin_BlkHouse_Number = model.NextOfKin_BlkHouse_Number;
            dbUserProfile.NextOfKin_Contact_Number = model.NextOfKin_Contact_Number;
            dbUserProfile.NextOfKin_Country = model.NextOfKin_Country;
            dbUserProfile.NextOfKin_FlrUnit_Number = model.NextOfKin_FlrUnit_Number;
            dbUserProfile.NextOfKin_Name = model.NextOfKin_Name;
            dbUserProfile.NextOfKin_PostalCode = model.NextOfKin_PostalCode;
            dbUserProfile.NextOfKin_Relationship = model.NextOfKin_Relationship;
            dbUserProfile.NextOfKin_Street_Name = model.NextOfKin_Street_Name;
            dbUserProfile.Primary_Email = model.Primary_Email;
            dbUserProfile.Primary_Phone = model.Primary_Phone;
            dbUserProfile.Other_Address_ID = model.Other_Address_ID != "" ? model.Other_Address_ID : null;
            dbUserProfile.Residential_Addess_ID = model.Residential_Addess_ID != "" ? model.Residential_Addess_ID : null;
            dbUserProfile.Secondary_Email = model.Secondary_Email;
            dbUserProfile.Secondary_Phone = model.Secondary_Phone;
            dbUserProfile.User_Photo1 = model.User_Photo1;
            dbUserProfile.User_Photo2 = model.User_Photo2;
            dbUserProfile.Date_of_Issue = model.DateOfIssue;
            dbUserProfile.Gender = model.Gender;
            dbUserProfile.Race = model.Race;
            dbUserProfile.Serial_Number = model.SerialNumber;


            var dalUser = new Trinity.DAL.DAL_User();
            var result = dalUser.GetUserByUserId(model.UserId);
            var user = result.Data;
            var userInfo = new Trinity.Common.UserInfo
            {
                NRIC = user.NRIC,
                UserName = user.Name//,
                //Date = model.DOB.ToString()
            };

        }
        public Response<BE.Address> GetAddressByUserId(string userId, bool isOther = false)
        {
            User_Profiles dbUserProfile = null;
            DBContext.Address dbAddress = null;
            if (EnumAppConfig.IsLocal)
            {
                dbUserProfile = _localUnitOfWork.DataContext.User_Profiles.FirstOrDefault(u => u.UserId == userId);
                if (dbUserProfile != null)
                {
                    var addressId = dbUserProfile.Residential_Addess_ID;
                    if (isOther)
                    {
                        addressId = dbUserProfile.Other_Address_ID;
                    }
                    dbAddress = _localUnitOfWork.DataContext.Addresses.FirstOrDefault(a => a.Address_ID == addressId);
                }

            }
            else
            {
                dbUserProfile = _centralizedUnitOfWork.DataContext.User_Profiles.FirstOrDefault(u => u.UserId == userId);
                if (dbUserProfile != null)
                {
                    var addressId = dbUserProfile.Residential_Addess_ID;
                    if (isOther)
                    {
                        addressId = dbUserProfile.Other_Address_ID;
                    }
                    dbAddress = _centralizedUnitOfWork.DataContext.Addresses.FirstOrDefault(a => a.Address_ID == addressId);
                }
            }

            if (dbUserProfile != null)
            {

                if (dbAddress != null)
                {
                    var address = new BE.Address
                    {
                        Address_ID = dbAddress.Address_ID,
                        BlkHouse_Number = dbAddress.BlkHouse_Number,
                        Country = dbAddress.Country,
                        FlrUnit_Number = dbAddress.FlrUnit_Number,
                        Postal_Code = dbAddress.Postal_Code,
                        Street_Name = dbAddress.Street_Name
                    };
                    return new Response<BE.Address>((int)EnumResponseStatuses.Success, EnumResponseMessage.Success, address);
                }
            }

            return new Response<BE.Address>((int)EnumResponseStatuses.ErrorSystem, EnumResponseMessage.ErrorSystem, null);
        }





    }
}
