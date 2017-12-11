using System;
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
            web.LoadPageHtml("SmartCard.html");
            bool status = Event_Scan();
            while (!status && CountScan < 3)
            {
                CountScan++;
                web.RunScript("$('.status-text').text('Smart Card Scanning " + CountScan + "/3');");
                status = Event_Scan();

            }
            if (!status)
            {
                web.RunScript("$('.status-text').text('Smart Card Scanning  Failure');");
                Thread.Sleep(1000);
                CountScan = 0;
                ThreadScan();
            }
            else
            {
                web.RunScript("$('.status-text').text('Smart Card Scanning Success');");
                Thread.Sleep(1000);
                fingerPrint.Scanning();
            }
        }

        private bool Event_Scan()
        {
            Thread.Sleep(3000);
            
            return (ran.Next(0, 10))%2==0 ? true : false;
        }
    }
}
