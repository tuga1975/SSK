using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.BE
{
    

    public class UserProfile
    {
        [DataMember]
        public string UserId { get; set; }

        [DataMember]
        public string Primary_Phone { get; set; }

        [DataMember]
        public string Secondary_Phone { get; set; }

        [DataMember]
        public string Primary_Email { get; set; }

        [DataMember]
        public string Secondary_Email { get; set; }

        [DataMember]
        public Nullable<System.DateTime> DOB { get; set; }


        public string DOBAsString
        {
            get
            {
                return this.DOB.HasValue ? DOB.Value.ToString("dd/MM/yyyy") : string.Empty;
            }
        }

        [DataMember]
        public string Nationality { get; set; }

        [DataMember]
        public string Maritial_Status { get; set; }

        [DataMember]
        public string Residential_Addess_ID { get; set; }

        [DataMember]
        public string Other_Address_ID { get; set; }

        [DataMember]
        public string NextOfKin_Name { get; set; }

        [DataMember]
        public string NextOfKin_Contact_Number { get; set; }

        [DataMember]
        public string NextOfKin_Relationship { get; set; }

        [DataMember]
        public string NextOfKin_BlkHouse_Number { get; set; }

        [DataMember]
        public string NextOfKin_FlrUnit_Number { get; set; }

        [DataMember]
        public string NextOfKin_Street_Name { get; set; }

        [DataMember]
        public string NextOfKin_Country { get; set; }

        [DataMember]
        public string NextOfKin_PostalCode { get; set; }

        [DataMember]
        public string Employment_Name { get; set; }

        [DataMember]
        public string Employment_Contact_Number { get; set; }

        [DataMember]
        public string Employment_Company_Name { get; set; }

        [DataMember]
        public string Employment_Job_Title { get; set; }

        [DataMember]
        public Nullable<System.DateTime> Employment_Start_Date { get; set; }

        public string Employment_Start_Date_Text
        {
            get
            {
                if (Employment_Start_Date != null)
                {
                    return Employment_Start_Date.Value.ToString("dd/MM/yyyy");
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        [DataMember]
        public Nullable<System.DateTime> Employment_End_Date { get; set; }

        public string Employment_End_Date_Text
        {
            get
            {
                if (Employment_End_Date != null)
                {
                    return Employment_End_Date.Value.ToString("dd/MM/yyyy");
                }
                else
                {
                    return string.Empty;
                }
            }
        }


        [DataMember]
        public string Employment_Remarks { get; set; }


        [DataMember]
        public string Gender { get; set; }


        [DataMember]
        public string SerialNumber { get; set; }

        [DataMember]
        public string Race { get; set; }

        [DataMember]
        public DateTime? DateOfIssue { get; set; }
        public string DateOfIssueTxt
        {
            get
            {
                return this.DateOfIssue.HasValue ? DateOfIssue.Value.ToString("dd/MM/yyyy") : string.Empty;
            }
        }

        [DataMember]
        public byte[] User_Photo1 { get; set; }

        [DataMember]
        public string User_Photo1_Base64
        {
            get
            {
                if (User_Photo1 == null)
                    return "../images/usr-default.jpg";
                return Convert.ToBase64String(User_Photo1);
            }
        }

        [DataMember]
        public byte[] User_Photo2 { get; set; }

        [DataMember]
        public string User_Photo2_Base64
        {
            get
            {
                if (User_Photo2 == null)
                    return "../images/usr-default.jpg";
                return Convert.ToBase64String(User_Photo2);
            }
        }

        [DataMember]
        public byte[] RightThumbImage { get; set; }

        [DataMember]
        public byte[] LeftThumbImage { get; set; }

        [DataMember]
        public Nullable<DateTime> Expired_Date { get; set; }
        public string Expired_DateTxt
        {
            get
            {
                return this.Expired_Date.HasValue ? Expired_Date.Value.ToString("dd/MM/yyyy") : string.Empty;
            }
        }
    }
}
