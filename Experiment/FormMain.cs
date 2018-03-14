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
    public partial class FormMain : Form
    {
        public FormMain()
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

        private void Alert()
        {
            MessageBox.Show("OK", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private void btnSendBTCommand_Click(object sender, EventArgs e)
        {
            try
            {
                Alert(TSCLIB_DLL.openport(EnumDeviceNames.MUBLabelPrinter).ToString());
                TSCLIB_DLL.clearbuffer();
                List<string> cmdLines = new List<string>();
                for (int i = 0; i < rtbBarTenderCommand.Lines.Length; i++)
                {
                    cmdLines.Add(rtbBarTenderCommand.Lines[i]);
                    Alert(TSCLIB_DLL.sendcommand(rtbBarTenderCommand.Lines[i]).ToString());
                }

                //int result = TSCLIB_DLL.openport(EnumDeviceNames.MUBLabelPrinter);
                ////TSCLIB_DLL.clearbuffer();

                //result = TSCLIB_DLL.sendcommand("DMATRIX 120,8,400,400");
                //TSCLIB_DLL.printlabel("1", "1");

                TSCLIB_DLL.closeport();
            }
            catch (Exception ex)
            {
                Alert(ex.ToString());
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            BarcodePrinterUtil.Instance.ResetPagePossition(EnumDeviceNames.TTLabelPrinter);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            BarcodePrinterUtil.Instance.ResetPagePossition(EnumDeviceNames.MUBLabelPrinter);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (DocumentScannerUtil.Instance.Connect())
            {
                Alert();
            }
            else
            {
                Alert("failed");
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (DocumentScannerUtil.Instance.Disconnect())
            {
                Alert();
            }
            else
            {
                Alert("Disconnect failed");
            }
        }

        private void btnTestSignalR_Click(object sender, EventArgs e)
        {
            FormTestSignalR f = new FormTestSignalR();
            f.Show();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            FormTestSignalR formTestSignalR = new FormTestSignalR();
            formTestSignalR.ShowDialog();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            FormLEDLightControl formLEDLightControl = new FormLEDLightControl();
            formLEDLightControl.ShowDialog();
        }

        private void btnTestTextSpeech_Click(object sender, EventArgs e)
        {
            TextToSpeech textToSpeech = new TextToSpeech();
            textToSpeech.Speak("US airline industry primed for springtime boost: trade group");
        }

        private void button9_Click(object sender, EventArgs e)
        {
            Guid IDDocuemnt = new DAL_UploadedDocuments().Insert(Lib.ReadAllBytes(@"E:\GitHub\2018\Trinity\Trinity.SSK\bin\Debug\Temp\document_front.bmp"), "9043d88e-94d1-4c01-982a-02d41965a621");
            Alert(IDDocuemnt.ToString());
            //_SaveReasonForQueue(dataAbsenceReporting, IDDocuemnt);
        }
    }
}
