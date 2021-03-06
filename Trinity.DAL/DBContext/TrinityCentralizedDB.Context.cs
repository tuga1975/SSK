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
        public virtual DbSet<ActionLog> ActionLogs { get; set; }
        public virtual DbSet<Address> Addresses { get; set; }
        public virtual DbSet<ApplicationDevice_Status> ApplicationDevice_Status { get; set; }
        public virtual DbSet<Appointment> Appointments { get; set; }
        public virtual DbSet<APS_USER_ACT_LOG> APS_USER_ACT_LOG { get; set; }
        public virtual DbSet<Device> Devices { get; set; }
        public virtual DbSet<DrugResult> DrugResults { get; set; }
        public virtual DbSet<EMAIL_LOG> EMAIL_LOG { get; set; }
        public virtual DbSet<Holiday> Holidays { get; set; }
        public virtual DbSet<IssuedCard> IssuedCards { get; set; }
        public virtual DbSet<Label> Labels { get; set; }
        public virtual DbSet<Membership_RoleClaims> Membership_RoleClaims { get; set; }
        public virtual DbSet<Membership_Roles> Membership_Roles { get; set; }
        public virtual DbSet<Membership_UserClaims> Membership_UserClaims { get; set; }
        public virtual DbSet<Membership_UserDevices> Membership_UserDevices { get; set; }
        public virtual DbSet<Membership_UserLogins> Membership_UserLogins { get; set; }
        public virtual DbSet<Membership_UserRoles> Membership_UserRoles { get; set; }
        public virtual DbSet<Membership_Users> Membership_Users { get; set; }
        public virtual DbSet<Message> Messages { get; set; }
        public virtual DbSet<Notification> Notifications { get; set; }
        public virtual DbSet<OperationSetting> OperationSettings { get; set; }
        public virtual DbSet<OperationSettings_ChangeHist> OperationSettings_ChangeHist { get; set; }
        public virtual DbSet<QueueDetail> QueueDetails { get; set; }
        public virtual DbSet<Queue> Queues { get; set; }
        public virtual DbSet<Recipient> Recipients { get; set; }
        public virtual DbSet<Security_QA> Security_QA { get; set; }
        public virtual DbSet<Setting> Settings { get; set; }
        public virtual DbSet<SMS_LOG> SMS_LOG { get; set; }
        public virtual DbSet<Timeslot> Timeslots { get; set; }
        public virtual DbSet<Transaction> Transactions { get; set; }
        public virtual DbSet<UploadedDocumentDetail> UploadedDocumentDetails { get; set; }
        public virtual DbSet<UploadedDocument> UploadedDocuments { get; set; }
        public virtual DbSet<User_Profiles> User_Profiles { get; set; }
    }
}
