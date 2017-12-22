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
    public static SSA.Utils.SignalR SignalR { get; set; }
    public static SSA.Utils.Printer Printer { get; set; }
    public static SSA.Utils.TextToSpeech TextToSpeech { get; set; }

    static APIUtils()
    {
        SignalR = new SSA.Utils.SignalR();
        Printer = new SSA.Utils.Printer();
        TextToSpeech = new SSA.Utils.TextToSpeech();
    }


    public static void Dispose()
    {
        #region Đóng lại nếu ko có Smart Card Driver
        //SCardMonitor.Dispose();
        #endregion
    }
}
