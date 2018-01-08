using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Speech.Synthesis;
using Trinity.BE;
using Trinity.Common;
using Trinity.DAL;

namespace SSA.Utils
{
    public class TextToSpeech
    {
        private SpeechSynthesizer _synthesizer;
        public TextToSpeech()
        {
            _synthesizer = new SpeechSynthesizer();
            _synthesizer.Volume = Convert.ToInt32(ConfigurationManager.AppSettings["SpeechSynthesizer_Volume"]);
            _synthesizer.Rate = Convert.ToInt32(ConfigurationManager.AppSettings["SpeechSynthesizer_Rate"]);
        }

        public void Speak(string text)
        {
            _synthesizer.SpeakAsync(text);
        }
    }
}
