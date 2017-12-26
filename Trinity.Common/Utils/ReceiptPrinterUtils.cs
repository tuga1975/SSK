using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity.Common.Common;

namespace Trinity.Common.Utils
{
    public class ReceiptPrinterUtils : DeviceUtils
    {
        // for testing purpose
        public override EnumDeviceStatuses[] GetDeviceStatus()
        {
            return new EnumDeviceStatuses[]{ EnumDeviceStatuses.Disconnected };
        }
    }
}
