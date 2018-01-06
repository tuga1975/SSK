using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using Trinity.Common;
using Trinity.DAL;

namespace Enrolment.CodeBehind
{
    class Document
    {
        WebBrowser _web;
        object _scanDocument;
        public Document(WebBrowser web,object scanDocument)
        {
            _web = web;
            _scanDocument = scanDocument;
        }
    }
}
