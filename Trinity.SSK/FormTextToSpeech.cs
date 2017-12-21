using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SSK
{
    public partial class FormTextToSpeech : Form
    {
        public FormTextToSpeech()
        {
            InitializeComponent();
        }

        private void btnSpeak_Click(object sender, EventArgs e)
        {
            APIUtils.TextToSpeech.Speak(txtTextToSpeech.Text);
            //SpeechSynthesizer synthesizer = new SpeechSynthesizer();
            //synthesizer.Volume = 100;  // 0...100
            //synthesizer.Rate = -2;     // -10...10

            //// Synchronous
            ////synthesizer.Speak(txtTextToSpeech.Text);

            //// Asynchronous
            //synthesizer.SpeakAsync(txtTextToSpeech.Text);
        }
    }
}
