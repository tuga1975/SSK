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
    
    public partial class UpdateProfile_Requests
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public UpdateProfile_Requests()
        {
            this.User_Profiles = new HashSet<User_Profiles>();
        }
    
        public string VersionId { get; set; }
        public string UserId { get; set; }
        public System.DateTime UpdatedTime { get; set; }
        public string Status { get; set; }
        public string ApprovedOrRejectedBy { get; set; }
        public Nullable<System.DateTime> ApprovedOrRejectedTime { get; set; }
        public string Current_Content_JSON { get; set; }
        public string Note { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<User_Profiles> User_Profiles { get; set; }
    }
}