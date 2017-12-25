using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Trinity.Common.Utils;

namespace SSK
{
    public partial class FormAppointmentDetails : Form
    {
        bool documentLoaded = false;
        public FormAppointmentDetails()
        {
            InitializeComponent();
        }

        private void FormLabelPrint_Load(object sender, EventArgs e)
        {
            webBrowserAppointmentDetails.DocumentCompleted += webBrowserAppointmentDetails_DocumentCompleted;
            string documentText = File.ReadAllText(String.Format("{0}/View/html/PrintingTemplates/AppointmentDetailsTemplate.html", CSCallJS.curDir), Encoding.UTF8);

            // Query Appointment Details from DB and fill values into html template
            documentText = documentText.Replace("{%Name%}", "Dao Quang Minh");
            //documentText = documentText.Replace("{%NRIC%}", "Get NRIC value and fill here");
            //documentText = documentText.Replace("{%DOB%}", "Get Supervisee'DOB value and fill here");

            // Write to a temp file
            File.WriteAllText(String.Format("{0}/View/html/temp.html", CSCallJS.curDir), documentText, Encoding.UTF8);

            // Nagivate webbrowser to the temp file
            Uri uri = new Uri(String.Format("file:///{0}/View/html/temp.html", CSCallJS.curDir));
            webBrowserAppointmentDetails.Navigate(uri);
        }

        private void webBrowserAppointmentDetails_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            documentLoaded = true;
            File.Delete(String.Format("{0}/View/html/temp.html", CSCallJS.curDir));
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            BarcodePrinterUtils printerUtil = BarcodePrinterUtils.Instance;

            Trinity.Common.Common.UserInfo userInfo = new Trinity.Common.Common.UserInfo()
            {
                DOB = "07-04-1982",
                NRIC = "022234343",
                UserName = "minhdq"
            };
            printerUtil.PrintUserInfo(userInfo);
            //if (documentLoaded)
            //{
            //    webBrowserAppointmentDetails.Print();
            //    //webBrowserAppointmentDetails.ShowPrintPreviewDialog();
            //}
        }
    }
}
