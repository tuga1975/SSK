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
    
    public partial class Notification
    {
        public long ID { get; set; }
        public System.DateTime Date { get; set; }
        public string FromUserId { get; set; }
        public string ToUserId { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public bool IsRead { get; set; }
        public Nullable<bool> IsFromSupervisee { get; set; }
    
        public virtual User User { get; set; }
        public virtual User User1 { get; set; }
    }
}
