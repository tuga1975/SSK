using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class APIUtils
{
    //public static SCardMonitor SCardMonitor { get; set; }
    public static DutyOfficer.Utils.SignalR SignalR { get; set; }

    public static void Start()
    {
        SignalR = new DutyOfficer.Utils.SignalR();
    }


    public static void Dispose()
    {
        SignalR.Dispose();
    }
}
