﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.BE
{
    

    public class User
    {
        [DataMember]
        public string UserId { get; set; }

        [DataMember]
        public string NRIC { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string SmartCardId { get; set; }

        [DataMember]
        public string Role { get; set; }

        [DataMember]
        public byte[] RightThumbFingerprint { get; set; }

        [DataMember]
        public byte[] LeftThumbFingerprint { get; set; }

        [DataMember]
        public string Status { get; set; }

        [DataMember]
        public Nullable<bool> IsFirstAttempt { get; set; }

        [DataMember]
        public int AccessFailedCount { get; set; }

        public byte[] User_Photo1 { get; set; }
        public byte[] User_Photo2 { get; set; }

        [DataMember]
        public string Note { get; set; }

        [DataMember]
        public Nullable<DateTime> Expired_Date { get; set; }
    }

    public class UserBlockedModel
    {
        public string UserId { get; set; }
        public string NRIC { get; set; }
        public string Name { get; set; }
        public string Reason { get; set; }
        public string OfficerNRIC { get; set; }
        public string OfficerName { get; set; }
    }
}
