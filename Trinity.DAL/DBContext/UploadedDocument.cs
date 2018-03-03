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
    
    public partial class UploadedDocument
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public UploadedDocument()
        {
            this.AbsenceReportings = new HashSet<AbsenceReporting>();
            this.User_Profiles = new HashSet<User_Profiles>();
        }
    
        public System.Guid Document_ID { get; set; }
        public string DocumentName { get; set; }
        public string UploadedBy { get; set; }
        public Nullable<System.DateTime> UploadedDate { get; set; }
        public string Extension { get; set; }
        public byte[] DocumentContent { get; set; }
        public string Note { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AbsenceReporting> AbsenceReportings { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<User_Profiles> User_Profiles { get; set; }
    }
}
