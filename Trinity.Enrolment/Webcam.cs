﻿using System;
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
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Enrolment
{
    class Webcam
    {
       public VideoCaptureDevice videoSource;
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
                videoSource = new VideoCaptureDevice(videosources[0].MonikerString);
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
                System.IO.Directory.CreateDirectory(String.Format("{0}/Temp", CSCallJS.curDir));

                videoSource.NewFrame += videoSource_NewFrame;
                //Start recording
                videoSource.Start();
                Lib.LayerWeb.InvokeScript("StartCamera");
            }
        }
        int count = 0;
        void videoSource_NewFrame(object sender, AForge.Video.NewFrameEventArgs eventArgs)
        {
            if (count == 0)
            {
                //Cast the frame as Bitmap object and don't forget to use ".Clone()" otherwise
                //you'll probably get access violation exceptions
                //pictureBox.Invoke((MethodInvoker)(() =>
                //{
                //    pictureBox.Image = (Bitmap)eventArgs.Frame.Clone();
                //}));
                //var imgSave = String.Format("{0}/Temp/{1}", CSCallJS.curDir, "webcam.jpg");
                var Img = (Bitmap)eventArgs.Frame.Clone();

                int width = Img.Width;
                int height = Img.Height;
                float ratio = (float)height / (float)width;

                if (height > ImageAttr.Height)
                {
                    height = ImageAttr.Height;
                    width = Convert.ToInt32(height / ratio);
                }

                //ResizeImage(Img, width, height).Save(imgSave, System.Drawing.Imaging.ImageFormat.Jpeg);

                //Lib.LayerWeb.InvokeScript("showCamera",DateTime.Now.Ticks.ToString());
                Lib.LayerWeb.InvokeScript("showCamera", Convert.ToBase64String(Img.ResizeImage(width, height).ImageToByte()));

                //Lib.LayerWeb.BeginInvoke(new System.Threading.ThreadStart(delegate
                //{
                //    Lib.LayerWeb.InvokeScript("showCamera", Convert.ToBase64String(ImageToByte2(Img)));
                //}));
            }
            else
            {
                count++;
                if (count == 10)
                    count = 0;
            }
            

        }
        
        public void stopWebcam()
        {
            if (videoSource != null && videoSource.IsRunning)
            {
                videoSource.SignalToStop();
                videoSource.NewFrame -= videoSource_NewFrame;
                videoSource = null;
            }
                
        }
    }
}
