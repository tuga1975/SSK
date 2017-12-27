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
        public string SmartCardId { get; set; }

        [DataMember]
        public string Role { get; set; }

        [DataMember]
        public byte[] Fingerprint { get; set; }

        [DataMember]
        public string Status { get; set; }
    }
}
