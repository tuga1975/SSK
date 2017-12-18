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

namespace SSK
{
    public partial class FormAnnouncementDetails : Form
    {
        bool documentLoaded = false;
        public FormAnnouncementDetails()
        {
            InitializeComponent();
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            if (documentLoaded)
            {
                webBrowser1.Print();
                //webBrowser1.ShowPrintPreviewDialog();
            }
        }

        private void FormLabelPrint_Load(object sender, EventArgs e)
        {
            webBrowser1.DocumentCompleted += WebBrowser1_DocumentCompleted;
            string documentText = File.ReadAllText(String.Format("{0}/View/html/AppointmentDetailsTemplate.html", CSCallJS.curDir), Encoding.UTF8);
            documentText = documentText.Replace("{%Name%}", "Dao Quang Minh");
            File.WriteAllText(String.Format("{0}/View/html/temp.html", CSCallJS.curDir), documentText, Encoding.UTF8);
            Uri uri = new Uri(String.Format("file:///{0}/View/html/temp.html", CSCallJS.curDir));
            webBrowser1.Navigate(uri);
        }

        private void WebBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            documentLoaded = true;
            File.Delete(String.Format("{0}/View/html/temp.html", CSCallJS.curDir));
        }
    }
}
