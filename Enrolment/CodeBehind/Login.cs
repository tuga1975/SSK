using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Enrolment.CodeBehind
{
    class Login
    {
        WebBrowser _web;

        public Login(WebBrowser web)
        {
            _web = web;
        }

        public void Start()
        {
            _web.LoadPageHtml("Login.html");
        }
    }
}
