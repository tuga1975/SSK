//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Trinity.DAL.DBContext
{
    using System;
    using System.Collections.Generic;
    
    public partial class User_Profiles
    {
        public string UserId { get; set; }
        public string Primary_Phone { get; set; }
        public string Secondary_Phone { get; set; }
        public string Primary_Email { get; set; }
        public string Secondary_Email { get; set; }
        public Nullable<System.DateTime> DOB { get; set; }
        public string Nationality { get; set; }
        public string Maritial_Status { get; set; }
        public Nullable<int> Residential_Addess_ID { get; set; }
        public Nullable<int> Other_Address_ID { get; set; }
        public string NextOfKin_Name { get; set; }
        public string NextOfKin_Contact_Number { get; set; }
        public string NextOfKin_Relationship { get; set; }
        public string NextOfKin_BlkHouse_Number { get; set; }
        public string NextOfKin_FlrUnit_Number { get; set; }
        public string NextOfKin_Street_Name { get; set; }
        public string NextOfKin_Country { get; set; }
        public string NextOfKin_PostalCode { get; set; }
        public string Employment_Name { get; set; }
        public string Employment_Contact_Number { get; set; }
        public string Employment_Company_Name { get; set; }
        public string Employment_Job_Title { get; set; }
        public Nullable<System.DateTime> Employment_Start_Date { get; set; }
        public Nullable<System.DateTime> Employment_End_Date { get; set; }
        public string Employment_Remarks { get; set; }
        public byte[] User_Photo1 { get; set; }
        public byte[] User_Photo2 { get; set; }
        public string Serial_Number { get; set; }
        public Nullable<System.DateTime> Date_of_Issue { get; set; }
        public string Gender { get; set; }
        public string Race { get; set; }
        public byte[] RightThumbImage { get; set; }
        public byte[] LeftThumbImage { get; set; }
    
        public virtual Address Address { get; set; }
        public virtual Address Address1 { get; set; }
        public virtual Membership_Users Membership_Users { get; set; }
    }
}
