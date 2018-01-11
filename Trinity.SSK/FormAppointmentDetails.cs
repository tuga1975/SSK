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
using Trinity.DAL;

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
            printerUtil.Test();
            return;
            Trinity.Common.LabelInfo labelInfo = new Trinity.Common.LabelInfo()
            {
                Date = "07-04-1982",
                NRIC = "022234343",
                Name = "minhdq"
            };
            //printerUtil.PrintBarcodeUserInfo(labelInfo);
            //if (documentLoaded)
            //{
            //    webBrowserAppointmentDetails.Print();
            //    //webBrowserAppointmentDetails.ShowPrintPreviewDialog();
            //}
        }

        private void btnGenerateTimeslots_Click(object sender, EventArgs e)
        {
            DAL_Setting dalSetting = new DAL_Setting();
            dalSetting.GenerateTimeslots("dfbb2a6a-9e45-4a76-9f75-af1a7824a947");
        }

        private void btnGenerateAppointments_Click(object sender, EventArgs e)
        {
            try
            {
                DAL_Appointments dalAppointment = new DAL_Appointments();
                dalAppointment.CreateAppointmentsForAllUsers(dateTimePicker1.Value);
                MessageBox.Show("OK");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi", ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
