using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.BE
{
    [Serializable]
    public class DeviceStatusInfo
    {

    }

    [Serializable]
    public class DeviceStatus
    {
        [DataMember]
        public int ID { get; set; }
        [DataMember]
        public string ApplicationType { get; set; }
        [DataMember]
        public Nullable<int> DeviceID { get; set; }
        [DataMember]
        public EnumDeviceStatuses[] StatusCode { get; set; }
    }
}
