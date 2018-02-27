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
        public Address OtherAddress { get; set; }

        public Membership_Users Membership_Users { get; set; }


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
        public string Residential_Addess_ID { get; set; }
        public string Other_Address_ID { get; set; }
        public string Postal_Code { get; set; }
        public string NextOfKin_Name { get; set; }
        public string NextOfKin_Contact_Number { get; set; }   // NextOfKinDetailsContactNumber
        public string NextOfKin_Relationship { get; set; }
        public string NextOfKin_BlkHouse_Number { get; set; }
        public string NextOfKin_FlrUnit_Number { get; set; }
        public string NextOfKin_Street_Name { get; set; }
        public string NextOfKin_PostalCode { get; set; }
        public string NextOfKin_Country { get; set; }
        public string Employment_Name { get; set; }
        public string Employment_Contact_Number { get; set; }
        public string Employment_Company_Name { get; set; }
        public string Employment_Job_Title { get; set; }
        public DateTime? Employment_Start_Date { get; set; }
        public DateTime? Employment_End_Date { get; set; }
        public string Employment_Remarks { get; set; }
        public string SmartCardId { get; set; }
        public byte[] RightThumbFingerprint { get; set; }
        public byte[] LeftThumbFingerprint { get; set; }
        public bool? IsFirstAttempt { get; set; }
        public byte[] User_Photo1 { get; set; }
        public byte[] User_Photo2 { get; set; }
        public byte[] RightThumbImage { get; set; }
        public byte[] LeftThumbImage { get; set; }
        public string Gender { get; set; }
        public string SerialNumber { get; set; }
        public string Race { get; set; }
        public DateTime? DateOfIssue { get; set; }
        public string UserStatus { get; set; }


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
                    IsFirstAttempt = rawData.IsFirstAttempt,
                    Status = rawData.UserStatus
                },
                UserProfile = new UserProfile
                {
                    UserId = rawData.UserId,
                    DOB = rawData.DOB,
                    Employment_Company_Name = rawData.Employment_Company_Name,
                    Employment_Contact_Number = rawData.Employment_Contact_Number,
                    Employment_End_Date = rawData.Employment_End_Date,
                    Employment_Job_Title = rawData.Employment_Job_Title,
                    Employment_Name = rawData.Employment_Name,
                    Employment_Remarks = rawData.Employment_Remarks,
                    Employment_Start_Date = rawData.Employment_Start_Date,
                    Maritial_Status = rawData.MaritalStatus,
                    Nationality = rawData.Nationality,
                    NextOfKin_BlkHouse_Number = rawData.NextOfKin_BlkHouse_Number,
                    NextOfKin_Contact_Number = rawData.NextOfKin_Contact_Number,
                    NextOfKin_Country = rawData.NextOfKin_Country,
                    NextOfKin_FlrUnit_Number = rawData.NextOfKin_FlrUnit_Number,
                    NextOfKin_Name = rawData.NextOfKin_Name,
                    NextOfKin_PostalCode = rawData.NextOfKin_PostalCode,
                    NextOfKin_Relationship = rawData.NextOfKin_Relationship,
                    NextOfKin_Street_Name = rawData.NextOfKin_Street_Name,
                    Primary_Email = rawData.PrimaryEmail,
                    Primary_Phone = rawData.PrimaryContact,
                    Secondary_Email = rawData.SecondaryEmail,
                    Secondary_Phone = rawData.SecondaryContact,
                    Residential_Addess_ID = rawData.Residential_Addess_ID,
                    Other_Address_ID = rawData.Other_Address_ID,
                    User_Photo1 = rawData.User_Photo1,
                    User_Photo2 = rawData.User_Photo2,
                    LeftThumbImage = rawData.LeftThumbImage,
                    RightThumbImage = rawData.RightThumbImage,
                    DateOfIssue = rawData.DateOfIssue,
                    Gender = rawData.Gender,
                    Race = rawData.Race,
                    SerialNumber = rawData.SerialNumber

                },
                Addresses = new Address
                {
                },
                OtherAddress = new Address {
                }
            };
            return profileModel;
        }
    }

    
    public class Address
    {
        [DataMember]
        public string Address_ID { get; set; }
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

    public class OtherAddress
    {
        [DataMember]
        public string OAddress_ID { get; set; }
        [DataMember]
        public string OBlkHouse_Number { get; set; }
        [DataMember]
        public string OFlrUnit_Number { get; set; }
        [DataMember]
        public string OStreet_Name { get; set; }
        [DataMember]
        public string OCountry { get; set; }
        [DataMember]
        public string OPostal_Code { get; set; }
    }
}
