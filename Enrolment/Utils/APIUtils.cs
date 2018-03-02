using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class APIUtils
{
    public static Enrolment.Utils.Printer Printer { get; set; }
    public static Enrolment.Utils.TextToSpeech TextToSpeech { get; set; }

    public static void Start()
    {
        Printer = new Enrolment.Utils.Printer();
        TextToSpeech = new Enrolment.Utils.TextToSpeech();
    }


    public static void Dispose()
    {
    }
}
