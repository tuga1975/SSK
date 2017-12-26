using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity.Common.DeviceMonitor;

namespace Trinity.Common.Monitor
{
    public class DeviceMonitor
    {
        public static void Start()
        {
            HealthMonitor healthMonitor = HealthMonitor.Instance;
            healthMonitor.Start();

            SCardMonitor sCardMonitor = SCardMonitor.Instance;
            sCardMonitor.Start();
            FingerprintMonitor fingerprintMonitor = FingerprintMonitor.Instance;
            fingerprintMonitor.Start();
            
        }
    }
}
