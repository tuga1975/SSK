using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class APIUtils
{
    public static System.Windows.Forms.WebBrowser LayerWeb { get; set; }
    public static Enrolment.Utils.SignalR SignalR { get; set; }
    public static Enrolment.Utils.Printer Printer { get; set; }
    public static Enrolment.Utils.TextToSpeech TextToSpeech { get; set; }

    static APIUtils()
    {
        SignalR = new Enrolment.Utils.SignalR();
        Printer = new Enrolment.Utils.Printer();
        TextToSpeech = new Enrolment.Utils.TextToSpeech();
    }


    public static void Dispose()
    {
        SignalR.Dispose();
    }
}
