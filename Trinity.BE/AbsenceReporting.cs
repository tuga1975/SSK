using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.BE
{
    [Serializable]
    public class AbsenceReporting
    {
        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public DateTime ReportingDate { get; set; }

        [DataMember]
        public short AbsenceReason { get; set; }

        [DataMember]
        public string ReasonDetails { get; set; }

        [DataMember]
        public byte[] ScannedDocument { get; set; }
        

    }

    public class Reason
    {
        public short Value { get; set; }
        public string Detail { get; set; }
    }
}
