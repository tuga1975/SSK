using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinFormCharpWebCam;
namespace Enrolment
{
    public partial class PictureCaptureForm : Form
    {
        WebCam webcam;
        public PictureCaptureForm()
        {
            InitializeComponent();
        }

        private void PictureCaptureForm_Load(object sender, EventArgs e)
        {
            webcam = new WebCam();
            webcam.InitializeWebCam(ref pictureBox1);
            webcam.Start();
        }
    }
}
