using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Enrolment.CodeBehind
{
    class Suppervisee
    {
        WebBrowser _web;

        public Suppervisee(WebBrowser web)
        {
            _web = web;
        }

        public void Start()
        {
            _web.LoadPageHtml("Supervisee.html");
        }
    }
}
