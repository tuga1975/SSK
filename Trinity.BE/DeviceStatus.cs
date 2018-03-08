using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.BE
{
    public class DeviceStatus
    {
        public string Station { get; set; }
        public int DeviceID { get; set; }
        public EnumDeviceStatus[] StatusCode { get; set; }
    }

    public class StationColorDevice
    {
        public string SSAColor { get; set; }
        public string SSKColor { get; set; }
        public string UHPColor { get; set; }
        public string ESPColor { get; set; }
    }
}
