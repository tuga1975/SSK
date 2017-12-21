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
        public override DeviceStatus GetDeviceStatus()
        {
            return DeviceStatus.Disconnected;
        }
    }
}
