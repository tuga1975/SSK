using System;
using System.Collections.Generic;
using System.Configuration;
using System.Speech.Synthesis;

namespace Trinity.Common
{
    public class TextToSpeech
    {
        private SpeechSynthesizer _synthesizer;
        public TextToSpeech()
        {
            _synthesizer = new SpeechSynthesizer();
            _synthesizer.Volume = Convert.ToInt32(ConfigurationManager.AppSettings["SpeechSynthesizer_Volume"]);
            _synthesizer.Rate = Convert.ToInt32(ConfigurationManager.AppSettings["SpeechSynthesizer_Rate"]);
            string strVoiceGender = ConfigurationManager.AppSettings["SpeechSynthesizer_VoiceGender"];
            string strVoiceAge = ConfigurationManager.AppSettings["SpeechSynthesizer_VoiceAge"];
            VoiceGender voiceGender = VoiceGender.Neutral;
            VoiceAge voiceAge = VoiceAge.Senior;

            // Set voice gender
            if (strVoiceGender.Equals("Male", StringComparison.InvariantCultureIgnoreCase))
            {
                voiceGender = VoiceGender.Male;
            }
            else if (strVoiceGender.Equals("Female", StringComparison.InvariantCultureIgnoreCase))
            {
                voiceGender = VoiceGender.Female;
            }
            else if (strVoiceGender.Equals("Neutral", StringComparison.InvariantCultureIgnoreCase))
            {
                voiceGender = VoiceGender.Neutral;
            }
            else if (strVoiceGender.Equals("Default", StringComparison.InvariantCultureIgnoreCase))
            {
                voiceGender = VoiceGender.NotSet;
            }

            // Set voice age
            if (strVoiceAge.Equals("Adult", StringComparison.InvariantCultureIgnoreCase))
            {
                voiceAge = VoiceAge.Adult;
            }
            else if (strVoiceAge.Equals("Child", StringComparison.InvariantCultureIgnoreCase))
            {
                voiceAge = VoiceAge.Child;
            }
            else if (strVoiceAge.Equals("Default", StringComparison.InvariantCultureIgnoreCase))
            {
                voiceAge = VoiceAge.NotSet;
            }
            else if (strVoiceAge.Equals("Senior", StringComparison.InvariantCultureIgnoreCase))
            {
                voiceAge = VoiceAge.Senior;
            }
            else if (strVoiceAge.Equals("Teen", StringComparison.InvariantCultureIgnoreCase))
            {
                voiceAge = VoiceAge.Teen;
            }
            _synthesizer.SelectVoiceByHints(voiceGender, voiceAge);
        }

        public void Speak(string text)
        {
            _synthesizer.SpeakAsync(text);
        }
        public void Stop()
        {
            _synthesizer.SpeakAsyncCancelAll();
        }
    }
}
