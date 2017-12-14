﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity.BE;
using Trinity.DAL.DBContext;
using Trinity.DAL.Repository;

namespace Trinity.DAL
{
    public class DAL_UserProfile
    {
        Local_UnitOfWork _localUnitOfWork = new Local_UnitOfWork();
        Centralized_UnitOfWork _centralizedUnitOfWork = new Centralized_UnitOfWork();
        public void SaveUserProfile(UserProfile userProfile, bool isLocal)
        {
            IRepository<User_Profiles> userProfileRepo = null;
            if (isLocal)
            {
                userProfileRepo = _localUnitOfWork.GetRepository<User_Profiles>();
            }
            else
            {
                userProfileRepo = _centralizedUnitOfWork.GetRepository<User_Profiles>();
            }
            User_Profiles userProfileEntity = new User_Profiles()
            {
                //particulars optional
                Primary_Phone = userProfile.Primary_Phone,
                Secondary_Phone = userProfile.Secondary_Phone,
                Primary_Email = userProfile.Primary_Email,
                Secondary_Email = userProfile.Secondary_Email,

                //next of kin
                NextOfKin_Name = userProfile.NextOfKin_Name,
                NextOfKin_Contact_Number = userProfile.NextOfKin_Contact_Number,
                NextOfKin_Relationship = userProfile.NextOfKin_Relationship,
                NextOfKin_BlkHouse_Number = userProfile.NextOfKin_BlkHouse_Number,
                NextOfKin_FlrUnit_Number = userProfile.NextOfKin_FlrUnit_Number,
                NextOfKin_Street_Name = userProfile.NextOfKin_Street_Name,
                NextOfKin_Country = userProfile.NextOfKin_Country,
                NextOfKin_PostalCode = userProfile.NextOfKin_PostalCode,

                //Employment details
                Employment_Name = userProfile.Employment_Name,
                Employment_Contact_Number = userProfile.Employment_Contact_Number,
                Employment_Company_Name = userProfile.Employment_Company_Name,
                Employment_Job_Title = userProfile.Employment_Job_Title,
                Employment_Start_Date = userProfile.Employment_Start_Date,
                Employment_Remarks = userProfile.Employment_Remarks
            };
            userProfileRepo.Update(userProfileEntity);

            if (isLocal)
            {
                _localUnitOfWork.Save();
            }
            else
            {
                _centralizedUnitOfWork.Save();
            }
        }

        public Trinity.BE.UserProfile GetUserProfileByUserId(string userId,bool isLocal)
        {
            User_Profiles dbUserProfile = null;
            if (isLocal)
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
                    UserId = dbUserProfile.UserId
                };
                return userProfile;
            }
            return new UserProfile();
        }

        public bool UpdateUserProfile(BE.UserProfile model, string userId,bool isLocal)
        {
            try
            {
                User_Profiles dbUserProfile = null;
                if (isLocal)
                {
                    var localUserProfileRepo = _localUnitOfWork.GetRepository<User_Profiles>();
                    dbUserProfile = localUserProfileRepo.GetById(userId);
                    SetInfo(dbUserProfile, model);
                    localUserProfileRepo.Update(dbUserProfile);
                    _localUnitOfWork.Save();
                    return true;
                }
                else
                {
                    var centralUserProfileRepo = _centralizedUnitOfWork.GetRepository<User_Profiles>();
                    dbUserProfile = centralUserProfileRepo.GetById(userId);
                    SetInfo(dbUserProfile, model);
                    centralUserProfileRepo.Update(dbUserProfile);
                    _centralizedUnitOfWork.Save();
                    return true;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        protected void SetInfo(User_Profiles dbUserProfile, BE.UserProfile model)
        {
            if (dbUserProfile != null)
            {
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
                dbUserProfile.Other_Address_ID = model.Other_Address_ID;
                dbUserProfile.Primary_Email = model.Primary_Email;
                dbUserProfile.Primary_Phone = model.Primary_Phone;
                dbUserProfile.Residential_Addess_ID = model.Residential_Addess_ID;
                dbUserProfile.Secondary_Email = model.Secondary_Email;
                dbUserProfile.Secondary_Phone = model.Secondary_Phone;
                dbUserProfile.UserId = model.UserId;

            }
        }
        public Trinity.BE.Address GetAddressByUserId(string userId,bool isLocal)
        {
            User_Profiles dbUserProfile = null;
            DBContext.Address dbAddress = null;
            if (isLocal)
            {
                dbUserProfile = _localUnitOfWork.DataContext.User_Profiles.FirstOrDefault(u => u.UserId == userId);
                if (dbUserProfile!=null)
                {
                    dbAddress = _localUnitOfWork.DataContext.Addresses.FirstOrDefault(a => a.Address_ID == dbUserProfile.Residential_Addess_ID);
                }
               
            }
            else
            { dbUserProfile = _centralizedUnitOfWork.DataContext.User_Profiles.FirstOrDefault(u => u.UserId == userId);
                if (dbUserProfile != null)
                {

                    dbAddress = _centralizedUnitOfWork.DataContext.Addresses.FirstOrDefault(a => a.Address_ID == dbUserProfile.Residential_Addess_ID);
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
                        Name = dbAddress.Name,
                        Postal_Code = dbAddress.Postal_Code,
                        Street_Name = dbAddress.Street_Name
                    };
                    return address;
                }
            }

            return new BE.Address();
        }
    }
}
