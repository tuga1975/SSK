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
        JSCallCS jSCallCS;
        public Suppervisee(WebBrowser web)
        {
            _web = web;
            jSCallCS = new JSCallCS(web);
        }

        public void Start()
        {
            jSCallCS.LoadListSupervisee();
        }
    }
}
