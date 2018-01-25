using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.BE
{
    [Serializable]
    public class OperationSettings_ChangeHist
    {
        [DataMember]
        public int ID { get; set; }
        [DataMember]
        public int DayOfWeek { get; set; }
        [DataMember]
        public DateTime LastUpdatedDate { get; set; }
        [DataMember]
        public string LastUpdatedBy { get; set; }
        [DataMember]
        public string ChangeDetails { get; set; }
    }
}
