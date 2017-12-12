using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.BE
{
    [Serializable]

    public class User
    {
        [DataMember]
        public string UserId { get; set; }

        [DataMember]
        public string NRIC { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public Nullable<System.DateTime> LastLoginTime { get; set; }

        [DataMember]
        public Nullable<short> SmartCardFailedCount { get; set; }

        [DataMember]
        public Nullable<short> FingerprintFailedCount { get; set; }

        [DataMember]
        public string SmartCard_Id { get; set; }

        [DataMember]
        public Nullable<System.DateTime> EnrolledDate { get; set; }

        [DataMember]
        public Nullable<short> Type { get; set; }

        [DataMember]
        public string DutyOfficer_Id { get; set; }
    }
}
