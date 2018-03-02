using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

 class APIUtils
{
    public static SSA.Utils.Printer Printer { get; set; }
    public static SSA.Utils.TextToSpeech TextToSpeech { get; set; }

    public static void Start()
    {
        Printer = new SSA.Utils.Printer();
        TextToSpeech = new SSA.Utils.TextToSpeech();
    }


    public static void Dispose()
    {
    }
}
