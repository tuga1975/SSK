using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AForge.Video.DirectShow;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Trinity.Common.Common;
using Trinity.Common;

namespace Enrolment
{
    class Webcam
    {
        VideoCaptureDevice videoSource;
        PictureBox pictureBox;
        public Webcam(PictureBox pic)
        {
            pictureBox = pic;
        }
        public void InitializeWebCam()
        {
            //List all available video sources. (That can be webcams as well as tv cards, etc)
            FilterInfoCollection videosources = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            //Check if atleast one video source is available
            if (videosources != null)
            {
                //For example use first video device. You may check if this is your webcam.
                try
                {
                    videoSource = new VideoCaptureDevice(videosources[0].MonikerString);
                }
                catch {
                    EventCenter eventCenter = EventCenter.Default;
                    eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Name = EventNames.OPEN_PICTURE_CAPTURE_FORM_FAILED });
                    return;
                }
                try
                {
                    //Check if the video device provides a list of supported resolutions
                    if (videoSource.VideoCapabilities.Length > 0)
                    {
                        string highestSolution = "0;0";
                        //Search for the highest resolution
                        for (int i = 0; i < videoSource.VideoCapabilities.Length; i++)
                        {
                            if (videoSource.VideoCapabilities[i].FrameSize.Width > Convert.ToInt32(highestSolution.Split(';')[0]))
                                highestSolution = videoSource.VideoCapabilities[i].FrameSize.Width.ToString() + ";" + i.ToString();
                        }
                        //Set the highest resolution as active
                        videoSource.VideoResolution = videoSource.VideoCapabilities[Convert.ToInt32(highestSolution.Split(';')[1])];
                    }
                }
                catch { }
            }
        }

        public void startWebcam()
        {
            //Create NewFrame event handler
            //(This one triggers every time a new frame/image is captured
            if (videoSource != null)
            {
                videoSource.NewFrame += new AForge.Video.NewFrameEventHandler(videoSource_NewFrame);
                //Start recording
                videoSource.Start();
            }
        }

        void videoSource_NewFrame(object sender, AForge.Video.NewFrameEventArgs eventArgs)
        {
            //Cast the frame as Bitmap object and don't forget to use ".Clone()" otherwise
            //you'll probably get access violation exceptions
            pictureBox.Image = (Bitmap)eventArgs.Frame.Clone();
        }

        public void stopWebcam()
        {
            try
            {
                if (videoSource != null && videoSource.IsRunning)
                {
                    videoSource.SignalToStop();
                    videoSource.NewFrame -= new AForge.Video.NewFrameEventHandler(videoSource_NewFrame);
                    videoSource = null;
                }
            }
            catch
            { return; }
                
        }
    }
}
