﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SSK.DriverScan
{
    public class FingerPrint
    {
        private WebBrowser web = null;
        private SmartCard smartCard = null;
        private int CountScan = 0;
        private Random ran = new Random();

        public FingerPrint(SmartCard smartCard,WebBrowser web)
        {
            this.web = web;
            this.smartCard = smartCard;
        }

        public void Scanning()
        {
            Thread thread = new Thread(ThreadScan);
            thread.IsBackground = true;
            thread.Start();
        }
        private void ThreadScan()
        {
            web.LoadPageHtml("FingerPrint.html");
            bool status = Event_Scan();
            while (!status && CountScan < 3)
            {
                CountScan++;
                web.RunScript("$('.status-text').text('Fingerprint Scanning " + CountScan + "/3');");
                status = Event_Scan();

            }
            if (!status)
            {
                web.RunScript("$('.status-text').text('Fingerprint Scanning  Failure');");
                Thread.Sleep(1000);
                CountScan = 0;
                smartCard.Scanning();
            }
            else
            {
                web.RunScript("$('.status-text').text('Fingerprint Scanning Success');");
                Thread.Sleep(1000);
                web.LoadPageHtml("Supervisee.html");
            }
        }

        private bool Event_Scan()
        {
            Thread.Sleep(3000);
            return (ran.Next(0, 10)) % 2 == 0 ? true : false;
        }


    }
}
