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
    
    public partial class Setting
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Setting()
        {
            this.Timeslots = new HashSet<Timeslot>();
        }
    
        public System.Guid Setting_ID { get; set; }
        public Nullable<int> WeekNum { get; set; }
        public Nullable<int> Year { get; set; }
        public string Status { get; set; }
        public Nullable<System.TimeSpan> Mon_Open_Time { get; set; }
        public Nullable<System.TimeSpan> Mon_Close_Time { get; set; }
        public Nullable<int> Mon_Interval { get; set; }
        public Nullable<int> Mon_MaximumNum { get; set; }
        public Nullable<int> Mon_ReservedForSpare { get; set; }
        public Nullable<System.TimeSpan> Tue_Open_Time { get; set; }
        public Nullable<System.TimeSpan> Tue_Close_Time { get; set; }
        public Nullable<int> Tue_Interval { get; set; }
        public Nullable<int> Tue_MaximumNum { get; set; }
        public Nullable<int> Tue_ReservedForSpare { get; set; }
        public Nullable<System.TimeSpan> Wed_Open_Time { get; set; }
        public Nullable<System.TimeSpan> Wed_Close_Time { get; set; }
        public Nullable<int> Wed_Interval { get; set; }
        public Nullable<int> Wed_MaximumNum { get; set; }
        public Nullable<int> Wed_ReservedForSpare { get; set; }
        public Nullable<System.TimeSpan> Thu_Open_Time { get; set; }
        public Nullable<System.TimeSpan> Thu_Close_Time { get; set; }
        public Nullable<int> Thu_MaximumNum { get; set; }
        public Nullable<int> Thu_Interval { get; set; }
        public Nullable<int> Thu_ReservedForSpare { get; set; }
        public Nullable<System.TimeSpan> Fri_Open_Time { get; set; }
        public Nullable<System.TimeSpan> Fri_Close_Time { get; set; }
        public Nullable<int> Fri_Interval { get; set; }
        public Nullable<int> Fri_MaximumNum { get; set; }
        public Nullable<int> Fri_ReservedForSpare { get; set; }
        public Nullable<System.TimeSpan> Sat_Open_Time { get; set; }
        public Nullable<System.TimeSpan> Sat_Close_Time { get; set; }
        public Nullable<int> Sat_Interval { get; set; }
        public Nullable<int> Sat_MaximumNum { get; set; }
        public Nullable<int> Sat_ReservedForSpare { get; set; }
        public Nullable<System.TimeSpan> Sun_Open_Time { get; set; }
        public Nullable<System.TimeSpan> Sun_Close_Time { get; set; }
        public Nullable<int> Sun_Interval { get; set; }
        public Nullable<int> Sun_MaximumNum { get; set; }
        public Nullable<int> Sun_ReservedForSpare { get; set; }
        public string Last_Updated_By { get; set; }
        public System.DateTime Last_Updated_Date { get; set; }
        public string Description { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Timeslot> Timeslots { get; set; }
    }
}
