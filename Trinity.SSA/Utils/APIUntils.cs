using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

 class APIUtils
{
    public static ALK.Utils.Printer Printer { get; set; }
    public static ALK.Utils.TextToSpeech TextToSpeech { get; set; }

    public static void Start()
    {
        Printer = new ALK.Utils.Printer();
        TextToSpeech = new ALK.Utils.TextToSpeech();
    }


    public static void Dispose()
    {
    }
}
