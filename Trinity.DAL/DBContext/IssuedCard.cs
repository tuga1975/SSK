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
    
    public partial class IssuedCard
    {
        public string UserId { get; set; }
        public string SmartCardId { get; set; }
        public string NRIC { get; set; }
        public string Name { get; set; }
        public string Serial_Number { get; set; }
        public Nullable<System.DateTime> Date_Of_Issue { get; set; }
        public Nullable<System.DateTime> Expired_Date { get; set; }
        public string Status { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string Reprint_Reason { get; set; }
    
        public virtual Membership_Users Membership_Users { get; set; }
    }
}