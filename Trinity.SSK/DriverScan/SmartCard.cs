﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SSK.DriverScan
{
    public class SmartCard
    {
        private WebBrowser web = null;
        private FingerPrint fingerPrint = null;
        private int CountScan = 0;
        private Random ran = new Random();
        public SmartCard(WebBrowser web)
        {
            this.web = web;
            this.fingerPrint = new FingerPrint(this, web);
        }

        public void Scanning()
        {
            Thread thread = new Thread(ThreadScan);
            thread.IsBackground = true;
            thread.Start();
        }
        private void ThreadScan()
        {
            web.LoadPageHtml("Authentication/SmartCard.html");
            web.RunScript("$('.status-text').css('color','#000').text('Please place your smart card on the reader.');");
            Thread.Sleep(1000);
            bool status = Event_Scan();
            while (!status && CountScan < 3)
            {
                CountScan++;
                status = Event_Scan();

            }
            if (!status)
            {
                web.RunScript("$('.status-text').text('Smart Card Scanning Failure. Please try again.');");
                Thread.Sleep(1000);
                CountScan = 0;
                ThreadScan();
            }
            else
            {
                web.RunScript("$('.status-text').css('color','blue').text('Authentication Successful');");
                Thread.Sleep(1000);
                fingerPrint.Scanning();
            }
        }

        private bool Event_Scan()
        {
            Thread.Sleep(5000);
            
            return (ran.Next(0, 10))%2==0 ? true : false;
        }
    }
}
