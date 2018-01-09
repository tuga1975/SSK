using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.BE
{
    public class ProfileModel
    {
        public User User { get; set; }
        public UserProfile UserProfile { get; set; }
        public Address Addresses { get; set; }

        public ProfileModel()
        {

        }
    }

    public class ProfileRawMData
    {
        public string UserId { get; set; }
        public DateTime? LastLoginTime { get; set; }
        public short? SmartCardFailedCount { get; set; }
        public short? FingerprintFailedCount { get; set; }
        public DateTime? EnrolledDate { get; set; }
        public string Role { get; set; }
        public string ParticularsName { get; set; }
        public string NRIC { get; set; }
        public DateTime? DOB { get; set; }
        public string Nationality { get; set; }
        public string MaritalStatus { get; set; }
        public string PrimaryContact { get; set; }
        public string SecondaryContact { get; set; }
        public string PrimaryEmail { get; set; }
        public string SecondaryEmail { get; set; }
        public int? Residential_Addess_ID { get; set; }
        public int? Other_Address_ID { get; set; }
        public string Postal_Code { get; set; }
        public string NextOfKinDetailsName { get; set; }
        public string ContactNumber { get; set; }   // NextOfKinDetailsContactNumber
        public string Relationship { get; set; }
        public string NextOfKinDetailsHouseNumber { get; set; }
        public string NextOfKinDetailsUnitNumber { get; set; }
        public string NextOfKinDetailsStreetName { get; set; }
        public string NextOfKinDetailsPostalCode { get; set; }
        public string NextOfKinDetailsCountry { get; set; }
        public string EmployerName { get; set; }
        public string EmployerContact { get; set; }
        public string CompanyName { get; set; }
        public string JobTitle { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Remarks { get; set; }
        public string SmartCardId { get; set; }
        public byte[] RightThumbFingerprint { get; set; }
        public byte[] LeftThumbFingerprint { get; set; }
        public bool? IsFirstAttempt { get; set; }
        public byte[] User_Photo1 { get; set; }
        public byte[] User_Photo2 { get; set; }


        public ProfileModel ToProfileModel(ProfileRawMData rawData)
        {
            var profileModel = new ProfileModel
            {
                User = new User
                {
                    RightThumbFingerprint = rawData.RightThumbFingerprint,
                    LeftThumbFingerprint = rawData.LeftThumbFingerprint,
                    Name = rawData.ParticularsName,
                    NRIC = rawData.NRIC,
                    SmartCardId = rawData.SmartCardId,
                    //Role = rawData.Role,
                    UserId = rawData.UserId,
                    IsFirstAttempt = rawData.IsFirstAttempt
                },
                UserProfile = new UserProfile
                {
                    UserId = rawData.UserId,
                    DOB = rawData.DOB,
                    Employment_Company_Name = rawData.CompanyName,
                    Employment_Contact_Number = rawData.EmployerContact,
                    Employment_End_Date = rawData.EndDate,
                    Employment_Job_Title = rawData.JobTitle,
                    Employment_Name = rawData.EmployerName,
                    Employment_Remarks = rawData.Remarks,
                    Employment_Start_Date = rawData.StartDate,
                    Maritial_Status = rawData.MaritalStatus,
                    Nationality = rawData.Nationality,
                    NextOfKin_BlkHouse_Number = rawData.NextOfKinDetailsHouseNumber,
                    NextOfKin_Contact_Number = rawData.ContactNumber,
                    NextOfKin_Country = rawData.NextOfKinDetailsCountry,
                    NextOfKin_FlrUnit_Number = rawData.NextOfKinDetailsUnitNumber,
                    NextOfKin_Name = rawData.NextOfKinDetailsName,
                    NextOfKin_PostalCode = rawData.NextOfKinDetailsPostalCode,
                    NextOfKin_Relationship = rawData.Relationship,
                    NextOfKin_Street_Name = rawData.NextOfKinDetailsStreetName,
                    Primary_Email = rawData.PrimaryEmail,
                    Primary_Phone = rawData.PrimaryContact,
                    Secondary_Email = rawData.SecondaryEmail,
                    Secondary_Phone = rawData.SecondaryContact,
                    Residential_Addess_ID = rawData.Residential_Addess_ID,
                    Other_Address_ID = rawData.Other_Address_ID,
                    User_Photo1 = rawData.User_Photo1,
                    User_Photo2 = rawData.User_Photo2
                },
                Addresses = new Address
                {
                }
            };
            return profileModel;
        }
    }

    [Serializable]
    public class Address
    {
        [DataMember]
        public int Address_ID { get; set; }
        [DataMember]
        public string BlkHouse_Number { get; set; }
        [DataMember]
        public string FlrUnit_Number { get; set; }
        [DataMember]
        public string Street_Name { get; set; }
        [DataMember]
        public string Country { get; set; }
        [DataMember]
        public string Postal_Code { get; set; }
    }
}
