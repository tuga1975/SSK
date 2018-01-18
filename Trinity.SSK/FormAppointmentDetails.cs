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
using Trinity.Common;
using Trinity.Common.Authentication;
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
            try
            {
                DAL_Setting dalSetting = new DAL_Setting();
                dalSetting.GenerateTimeslots("dfbb2a6a-9e45-4a76-9f75-af1a7824a947");
                MessageBox.Show("OK");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi", ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
        SuperviseeBiodata _superviseeBiodata = null;

        private void btnWriteDataToSmartCard_Click(object sender, EventArgs e)
        {
            _superviseeBiodata = new SuperviseeBiodata()
            {

                DrugProfile = "",
                Name = "Minh",
                NRIC = "033082000012",
                SupervisionContactNo = "0988482445",
                SupervisionFrom = DateTime.Now,
                SupervisionTo = DateTime.Now.AddYears(2),
                SupervisionOfficer = "Kong",
                UserId = "1234567890"
            };
            bool result = SmartCardUtil.UpdateSuperviseeBiodata(_superviseeBiodata);
            if (result)
            {
                MessageBox.Show("Update data successfully");
            }
            else
            {
                MessageBox.Show("Update data failed");
            }
        }

        private void btnReaderData_Click(object sender, EventArgs e)
        {
            if (SmartCardUtil.ReadData())
            {
                SuperviseeBiodata superviseeBioData = SmartCardUtil.GetSuperviseeBiodata();
                if (superviseeBioData != null && _superviseeBiodata != null)
                {
                    if (superviseeBioData.Name.Equals(_superviseeBiodata.Name) &&
                        superviseeBioData.NRIC.Equals(_superviseeBiodata.NRIC) &&
                        superviseeBioData.SupervisionContactNo.Equals(_superviseeBiodata.SupervisionContactNo) &&
                        superviseeBioData.SupervisionFrom.Equals(_superviseeBiodata.SupervisionFrom) &&
                        superviseeBioData.SupervisionOfficer.Equals(_superviseeBiodata.SupervisionOfficer) &&
                        superviseeBioData.SupervisionTo.Equals(_superviseeBiodata.SupervisionTo) &&
                        superviseeBioData.UserId.Equals(_superviseeBiodata.UserId) &&
                        superviseeBioData.DrugProfile.Equals(_superviseeBiodata.DrugProfile))
                    {
                        MessageBox.Show("Verified");
                        return;
                    }
                }
                MessageBox.Show("failed");
            }
        }
    }
}
