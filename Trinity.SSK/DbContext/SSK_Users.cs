//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SSK.DbContext
{
    using System;
    using System.Collections.Generic;
    
    public partial class SSK_Users
    {
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public Nullable<System.DateTime> LastLoginTime { get; set; }
        public Nullable<short> SmartCardFailedCount { get; set; }
        public Nullable<short> FingerprintFailedCount { get; set; }
        public string NRIC { get; set; }
        public string SmartCardId { get; set; }
        public Nullable<System.DateTime> EnrolledDate { get; set; }
    }
}
