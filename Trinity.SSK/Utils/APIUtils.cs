using SSK.DeviceMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class APIUtils
{
    public static SCardMonitor SCardMonitor { get; set; }
    static APIUtils()
    {
        SCardMonitor = new SCardMonitor();
    }


    public static void Dispose()
    {
        SCardMonitor.Dispose();
    }
}
