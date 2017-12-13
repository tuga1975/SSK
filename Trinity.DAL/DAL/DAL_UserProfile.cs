using System;
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
    }
}
