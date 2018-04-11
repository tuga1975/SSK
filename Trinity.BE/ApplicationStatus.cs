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
        public DeviceStatus(EnumDeviceId deviceID)
        {
            this.DeviceID = deviceID;
            this.DeviceName = string.Empty;
            this.Status = null;
            this.Summary = EnumDeviceStatusSumary.Error;
        }

        public EnumDeviceId DeviceID { get; set; }
        public string DeviceName { get; set; }
        public EnumDeviceStatus[] Status { get; set; }
        public EnumDeviceStatusSumary Summary { get; set; }
    }

    public class StationColorDevice
    {
        public string SSAColor { get; set; }
        public string SSKColor { get; set; }
        public string UHPColor { get; set; }
        public string ESPColor { get; set; }
    }
}
