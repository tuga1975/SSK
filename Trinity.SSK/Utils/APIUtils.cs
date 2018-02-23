using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class APIUtils
{
    public static SSK.Utils.SignalR SignalR { get; set; }
    public static SSK.Utils.Printer Printer { get; set; }
    public static SSK.Utils.TextToSpeech TextToSpeech { get; set; }

    public static SSK.FormQueueNumber FormQueueNumber { get; set; }

    public static void Start()
    {
        SignalR = new SSK.Utils.SignalR();
        Printer = new SSK.Utils.Printer();
        TextToSpeech = new SSK.Utils.TextToSpeech();
    }


    public static void Dispose()
    {
        SignalR.Dispose();
    }
}
