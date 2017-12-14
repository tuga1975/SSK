using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SSK.CodeBehind.Authentication
{
    class NRIC
    {
        WebBrowser _web;
        public NRIC(WebBrowser web)
        {
            _web = web;
        }
        internal void Start()
        {
            _web.LoadPageHtml("Authentication/NRIC.html");
        }
    }
}
