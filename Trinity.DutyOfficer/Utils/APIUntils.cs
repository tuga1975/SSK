using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class APIUtils
{
    public static System.Windows.Forms.WebBrowser LayerWeb { get; set; }
    //public static SCardMonitor SCardMonitor { get; set; }
    public static DutyOfficer.Utils.SignalR SignalR { get; set; }

    static APIUtils()
    {
        SignalR = new DutyOfficer.Utils.SignalR(EnumStations.SSK);
    }


    public static void Dispose()
    {
        SignalR.Dispose();
    }
}
