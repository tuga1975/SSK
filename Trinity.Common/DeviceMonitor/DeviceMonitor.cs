using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.Common.Monitor
{
    public class DeviceMonitor
    {
        public static void Start()
        {
            SCardMonitor.Start();
            FingerprintMonitor.Start();
        }
    }
}
