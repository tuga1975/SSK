using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.BE
{
    [Serializable]

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

        [DataMember]
        public string Nationality { get; set; }

        [DataMember]
        public string Maritial_Status { get; set; }

        [DataMember]
        public Nullable<int> Residential_Addess_ID { get; set; }

        [DataMember]
        public Nullable<int> Other_Address_ID { get; set; }

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

        [DataMember]
        public Nullable<System.DateTime> Employment_End_Date { get; set; }

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

        [DataMember]
        public byte[] User_Photo1 { get; set; }

        [DataMember]
        public byte[] User_Photo2 { get; set; }

        [DataMember]
        public string RightThumbImage { get; set; }

        [DataMember]
        public string LeftThumbImage { get; set; }

    }
}
