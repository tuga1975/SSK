using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.BE
{
    

    public class Membership_Users
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PasswordHash { get; set; }
        public string SecurityStamp { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public Nullable<System.DateTime> LockoutEndDateUtc { get; set; }
        public Nullable<bool> LockoutEnabled { get; set; }
        public int AccessFailedCount { get; set; }
        public string UserName { get; set; }
        public string Discriminator { get; set; }
        public string NRIC { get; set; }
        public string Name { get; set; }
        public string SmartCardId { get; set; }
        public byte[] RightThumbFingerprint { get; set; }
        public byte[] LeftThumbFingerprint { get; set; }
        public string Status { get; set; }
        public Nullable<bool> IsFirstAttempt { get; set; }
        public string Note { get; set; }
    }

}
