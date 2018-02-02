using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using Trinity.BE;
using Trinity.Common;
using Trinity.DAL;
using Trinity.Device;

namespace Experiment
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnPrintAppointmentDetails_Click(object sender, EventArgs e)
        {
            AppointmentDetails appointmentDetails = new AppointmentDetails()
            {
                Date = new DateTime(2018, 2, 25, 15, 00, 00),
                Name = "Do Duc Tu",
                Venue = "CNB ENF A"
            };

            bool result = ReceiptPrinterUtil.Instance.PrintAppointmentDetails(appointmentDetails);

            AlertResult(result);
        }

        private void AlertResult(bool result)
        {
            if (result)
            {
                MessageBox.Show("Successfully", "Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Failed", "Result", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnPrintSuperviseeCard_Click(object sender, EventArgs e)
        {
            SmartCardPrinterUtil smartCardPrinterUtils = SmartCardPrinterUtil.Instance;
            PrintAndWriteSmartCardInfo superviseeCardInfo = new PrintAndWriteSmartCardInfo()
            {
                FrontCardImagePath = @"E:\GitHub\SSK\DocumentScannerTest\bin\x64\Debug\Front.bmp",
                BackCardImagePath = @"E:\GitHub\SSK\DocumentScannerTest\bin\x64\Debug\Back.bmp",
                SuperviseeBiodata = new SuperviseeBiodata()
                {
                    Name = "Do Duc Tu",
                    DrugProfile = "ABC AND",
                    NRIC = "S999999G",
                    SupervisionContactNo = "123123123",
                    SupervisionFrom = new DateTime(2018, 1, 1),
                    SupervisionTo = new DateTime(2020, 1, 1),
                    SupervisionOfficer = "ABC123312",
                    UserId = "bvjknliaedlk"
                }
            };

            smartCardPrinterUtils.PrintAndWriteSmartCard(superviseeCardInfo, OnCompleted);
        }

        private void OnCompleted(PrintAndWriteCardResult result)
        {
            Alert(result.Success.ToString());
        }

        private void IdentificationCompleted(bool result)
        {
            Alert("IdentificationCompleted: " + result);
        }

        private void Alert(string mesage)
        {
            MessageBox.Show(mesage, "Alert", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnIdentifyFingerprint_Click(object sender, EventArgs e)
        {
            FingerprintReaderUtil fingerprintReaderUtils = FingerprintReaderUtil.Instance;

            DAL_User dAL_User = new DAL_User();
            List<User> users = dAL_User.GetAllSupervisees().Data;
            List<byte[]> templates = new List<byte[]>();
            templates.AddRange(users?.Where(s => s.LeftThumbFingerprint != null).Select(s => s.LeftThumbFingerprint).ToList());
            templates.AddRange(users?.Where(s => s.RightThumbFingerprint != null).Select(s => s.RightThumbFingerprint).ToList());

            fingerprintReaderUtils.StartIdentification(templates, IdentificationCompleted);
            //Task task = new Task(() => fingerprintReaderUtils.StartIdentification(templates, IdentificationCompleted));
            //task.Start();

            AlertResult(true);
        }

        private void btnReadSmartCardData_Click(object sender, EventArgs e)
        {
            bool result = SmartCardUtil.ReadData();

            AlertResult(result);
        }

        private void btnStartHealthChecker_Click(object sender, EventArgs e)
        {
            //SCardMonitor.Instance.Start();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var status = FingerprintReaderUtil.Instance.GetDeviceStatus();
        }
    }
}
