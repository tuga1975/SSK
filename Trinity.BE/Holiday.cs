using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.BE
{
    [Serializable]
    public class Holiday
    {
        [DataMember]
        public DateTime Holiday1 { get; set; }
        [DataMember]
        public bool IsSingHoliday { get; set; }
        [DataMember]
        public Nullable<bool> IsMalayHoliday { get; set; }
        [DataMember]
        public string ShortDesc { get; set; }
        [DataMember]
        public string Notes { get; set; }
    }
}
