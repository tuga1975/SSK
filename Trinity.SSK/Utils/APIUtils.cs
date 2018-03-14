using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class APIUtils
{
    public static SSK.Utils.Printer Printer { get; set; }
    public static Trinity.Common.TextToSpeech TextToSpeech { get; set; }

    public static SSK.FormQueueNumber FormQueueNumber { get; set; }

    public static void Start()
    {
        Printer = new SSK.Utils.Printer();
        TextToSpeech = new Trinity.Common.TextToSpeech();
    }


    public static void Dispose()
    {
    }
}
