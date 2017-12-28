using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Trinity.Enrolment.Utils;

namespace Enrolment
{
    public partial class Main : Form
    {
        private JSCallCS _jsCallCS;

        public Main()
        {
            InitializeComponent();

            // _jsCallCS
            _jsCallCS = new JSCallCS(this.LayerWeb);

            APIUtils.LayerWeb = LayerWeb;
            LayerWeb.Url = new Uri(String.Format("file:///{0}/View/html/Login.html", CSCallJS.curDir));
            LayerWeb.ObjectForScripting = _jsCallCS;

        }

        private void LayerWeb_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            LayerWeb.InvokeScript("createEvent", JsonConvert.SerializeObject(_jsCallCS.GetType().GetMethods().Where(d => d.IsPublic && !d.IsVirtual && !d.IsSecuritySafeCritical).ToArray().Select(d => d.Name)));

            // Start page
            //NavigateTo(NavigatorEnums.Authentication_SmartCard);
        }
    }
}
