﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class TrinityCentralizedDBEntities : DbContext
    {
        public TrinityCentralizedDBEntities()
            : base("name=TrinityCentralizedDBEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<AbsenceReporting> AbsenceReportings { get; set; }
        public virtual DbSet<Address> Addresses { get; set; }
        public virtual DbSet<ApplicationDevice_Status> ApplicationDevice_Status { get; set; }
        public virtual DbSet<Appointment> Appointments { get; set; }
        public virtual DbSet<Device> Devices { get; set; }
        public virtual DbSet<Environment> Environments { get; set; }
        public virtual DbSet<Holiday> Holidays { get; set; }
        public virtual DbSet<Notification> Notifications { get; set; }
        public virtual DbSet<QueueNumber> QueueNumbers { get; set; }
        public virtual DbSet<User_Profiles> User_Profiles { get; set; }
        public virtual DbSet<User> Users { get; set; }
    }
}
