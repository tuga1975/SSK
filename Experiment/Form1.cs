using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using Trinity.BE;
using Trinity.Common;
using Trinity.DAL;
using Trinity.Device;
using Trinity.Device.Util;
using Trinity.Util;

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
                CompanyName = "Central Narcotics Bureau",
                Date = new DateTime(2018, 2, 25, 15, 00, 00),
                Name = "Do Duc Tu",
                Venue = "393 New Bridge Road",
                StartTime = new TimeSpan(11, 30, 00)
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
                FrontCardImagePath = @"E:\GitHub\2018\Trinity\Experiment\bin\Debug\Temp\Front.png",
                BackCardImagePath = @"E:\GitHub\2018\Trinity\Experiment\bin\Debug\Temp\Back.png",
                SuperviseeBiodata = new SuperviseeBiodata()
                {
                    Name = "Do Duc Tu",
                    DrugProfile = "ABC AND",
                    NRIC = "S9872509D",
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
            //var status = FingerprintReaderUtil.Instance.GetDeviceStatus();
        }

        private void btnTestSerialComm_Click(object sender, EventArgs e)
        {
            FormLEDLightControl f = new FormLEDLightControl();
            f.Show();
        }

        private void btnStartFlashing_Click(object sender, EventArgs e)
        {
            LEDStatusLightingUtil.Instance.StartBLUELightFlashing();
        }

        private void btnInitFlashing_Click(object sender, EventArgs e)
        {
            string comPort = ConfigurationManager.AppSettings["COMPort"];
            int baudRate = int.Parse(ConfigurationManager.AppSettings["BaudRate"]);
            string parity = ConfigurationManager.AppSettings["Parity"];
            LEDStatusLightingUtil.Instance.OpenPort("SSK", comPort, baudRate, parity);
            LEDStatusLightingUtil.Instance.TurnOffAllLEDs();
            //LEDStatusLightingUtil.Instance.SwitchBLUELightOnOff(true);
            //LEDStatusLightingUtil.Instance.SwitchBLUELightFlashingOnOff(true);
        }

        private void bnStopFlashing_Click(object sender, EventArgs e)
        {
            LEDStatusLightingUtil.Instance.StopBLUELightFlashing();
        }

        private void btnPrintTTLabel_Click(object sender, EventArgs e)
        {
            BarcodePrinterUtil.Instance.PrintTTLabel(new TTLabelInfo()
            {
                ID = "S9872509D",
                Name = "Do Duc Tu",
                MarkingNumber = "CSA18001991"
            });
        }

        private void btnPrintTTLabel_2_Click(object sender, EventArgs e)
        {
            BarcodePrinterUtil.Instance.PrintTTLabel(new TTLabelInfo()
            {
                ID = "S9872509D",
                Name = "Do Duc Tu TVO Long Name",
                MarkingNumber = "CSA18001901"
            });
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MUBLabelInfo mubLabelInfo = new MUBLabelInfo()
            {
                ID = "S9872509D",
                Name = "Do Duc Tu",
                MarkingNumber = "CSA18001991"
            };

            string qrCodeString = string.Format("{0}*{1}*{2}", mubLabelInfo.MarkingNumber, mubLabelInfo.ID, mubLabelInfo.Name).PadRight(91, '*');
            mubLabelInfo.QRCodeString = qrCodeString;

            BarcodePrinterUtil.Instance.PrintMUBLabel(mubLabelInfo);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MUBLabelInfo mubLabelInfo = new MUBLabelInfo()
            {
                ID = "S9872509D",
                Name = "Do Duc Tu TVO Long Name",
                MarkingNumber = "CSA18001991"
            };

            string qrCodeString = string.Format("{0}*{1}*{2}", mubLabelInfo.MarkingNumber, mubLabelInfo.ID, mubLabelInfo.Name).PadRight(91, '*');
            mubLabelInfo.QRCodeString = qrCodeString;

            BarcodePrinterUtil.Instance.PrintMUBLabel(mubLabelInfo);
        }

        private void btnConnectPDIScanner_Click(object sender, EventArgs e)
        {
            DocumentScannerUtil.Instance.StartScanning(DocumentScannerCallback);
            MessageBox.Show("Start OK");
        }

        private void DocumentScannerCallback(string frontPath, string error)
        {
            try
            {
                if (string.IsNullOrEmpty(error))
                {
                    MessageBox.Show(frontPath);
                }
                else
                {
                    MessageBox.Show(error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void btnStopDPIScanner_Click(object sender, EventArgs e)
        {
            DocumentScannerUtil.Instance.StopScanning();
        }
    }
}
