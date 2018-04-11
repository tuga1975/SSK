using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Trinity.BE;

namespace ARK.Utils
{
    public class Printer
    {
        WebBrowser webBrowserForPrinting;
        public Printer()
        {
            webBrowserForPrinting = new WebBrowser();
            webBrowserForPrinting.ScriptErrorsSuppressed = true;
            webBrowserForPrinting.DocumentCompleted +=
            new WebBrowserDocumentCompletedEventHandler(PrintDocument);
        }

        public void PrintAppointmentDetails(string filetemplate, Appointment appointmentInfo)
        {
            string documentText = File.ReadAllText(String.Format("{0}/View/html/PrintingTemplates/" + filetemplate, CSCallJS.curDir), Encoding.UTF8);
            //var properties = appointmentInfo.GetType().GetProperties();
            //foreach (var p in properties)
            //{
            //    var value = p.GetValue(appointmentInfo);
            //    documentText = documentText.Replace("{%" + p.Name + "%}", value != null ? value.ToString() : string.Empty);
            //}
            documentText = documentText.Replace("{%Name%}", appointmentInfo.Name);
            documentText = documentText.Replace("{%NRIC%}", appointmentInfo.NRIC);
            documentText = documentText.Replace("{%AppointmentDate%}", appointmentInfo.AppointmentDate.Value.ToString("dd/MM/yyyy"));
            documentText = documentText.Replace("{%FromTime%}", appointmentInfo.StartTime.Value.ToString());
            documentText = documentText.Replace("{%ToTime%}", appointmentInfo.EndTime.Value.ToString());
            File.WriteAllText(String.Format("{0}/View/html/temp.html", CSCallJS.curDir), documentText, Encoding.UTF8);
            Uri uri = new Uri(String.Format("file:///{0}/View/html/temp.html", CSCallJS.curDir));
            webBrowserForPrinting.Navigate(uri);
        }

        private void PrintDocument(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            // Print the document now that it is fully loaded.
            webBrowserForPrinting.Invoke((MethodInvoker)(() =>
            {
                webBrowserForPrinting.Print();
            }));
            File.Delete(String.Format("{0}/View/html/temp.html", CSCallJS.curDir));
        }
    }
}
