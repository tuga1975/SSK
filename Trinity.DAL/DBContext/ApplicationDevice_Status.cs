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
    
    public partial class ApplicationDevice_Status
    {
        public System.Guid ID { get; set; }
        public string Station { get; set; }
        public Nullable<int> DeviceID { get; set; }
        public int StatusCode { get; set; }
        public string StatusMessage { get; set; }
    }
}
