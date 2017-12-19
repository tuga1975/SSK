using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity.Common.Monitor;

class APIUtils
{
    public static System.Windows.Forms.WebBrowser LayerWeb { get; set; }
    public static SCardMonitor SCardMonitor { get; set; }
    public static SSK.Utils.SignalR SignalR { get; set; }
    public static SSK.Utils.Printer Printer { get; set; }

    static APIUtils()
    {
        SignalR = new SSK.Utils.SignalR();
        Printer = new SSK.Utils.Printer();
    }


    public static void Dispose()
    {
        #region Đóng lại nếu ko có Smart Card Driver
        //SCardMonitor.Dispose();
        #endregion
    }
}
