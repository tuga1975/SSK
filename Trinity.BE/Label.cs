using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.BE
{
    

    public class Label
    {
        [DataMember]
        public string Label_Type { get; set; }

        [DataMember]
        public string CompanyName { get; set; }

        [DataMember]
        public string MarkingNo { get; set; }

        [DataMember]
        public string DrugType { get; set; }

        [DataMember]
        public string UserId { get; set; }
        
        [DataMember]
        public string NRIC { get; set; }
        
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public DateTime? Date { get; set; }

        [DataMember]
        public byte[] QRCode { get; set; }

        [DataMember]
        public string LastStation { get; set; }

        [DataMember]
        public int PrintCount { get; set; }

        [DataMember]
        public string ReprintReason { get; set; }
        public bool IsMUB { get; set; }
        public bool IsTT { get; set; }
        public string OfficerNRIC { get; set; }
        public string OfficerName { get; set; }
        [DataMember]
        public string PrintStatus { get; set; }
        [DataMember]
        public string Message { get; set; }
        public string TimeSlot_ID { get; set; }
        public Guid Queue_ID { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
    }
}
