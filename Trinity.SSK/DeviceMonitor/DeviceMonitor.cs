using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSK.DeviceMonitor
{
    class DeviceMonitor
    {
        public static void Start()
        {
            SCardMonitor.Start();
            FingerprintMonitor.Start();
        }
    }
}
