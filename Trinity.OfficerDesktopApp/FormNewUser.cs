using Futronic.SDKHelper;
using PCSC;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Trinity.Common.Monitor;

namespace OfficerDesktopApp
{
    public partial class FormNewUser : Form
    {
        private Trinity.BE.User _currentUser = null;

        private FutronicEnrollment _futronicEnrollment = null;
        public FormNewUser()
        {
            InitializeComponent();
        }

        private void FormNewUser_Load(object sender, EventArgs e)
        {
            Trinity.Common.Monitor.DeviceMonitor.Start();
            _currentUser = new Trinity.BE.User();
            _currentUser.UserId = Guid.NewGuid().ToString();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_currentUser.SmartCardId) || _currentUser.Fingerprint == null)
            {
                MessageBox.Show("You have to scan your smart card and fingerprint");
                return;
            }
            //
            // Prepare user information
            //
            _currentUser.Name = txtName.Text;
            _currentUser.NRIC = txtNRIC.Text;
            _currentUser.Role = Convert.ToInt16(cboRoles.SelectedIndex <= 0 ? 0 : 1);
            Trinity.DAL.DAL_User dalUser = new Trinity.DAL.DAL_User();
            if (dalUser.CreateUser(_currentUser, true))
            {
                Trinity.DAL.DAL_UserProfile dalUserProfile = new Trinity.DAL.DAL_UserProfile();
                Trinity.BE.UserProfile userProfile = new Trinity.BE.UserProfile();
                userProfile.Primary_Phone = txtPrimaryPhone.Text;
                userProfile.Primary_Email = txtPrimaryEmail.Text;
                userProfile.Nationality = txtNationality.Text;
                userProfile.DOB = dpDOB.Value;

                dalUserProfile.UpdateUserProfile(userProfile, _currentUser.UserId, true);
                btnSave.Enabled = false;
                MessageBox.Show("Create user successfully!", "Create user", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Could not create user.", "Create user", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Log(string message, Color color)
        {
            this.Invoke((Action)(() => lblNote.Text = message));
            this.Invoke((Action)(() => lblNote.ForeColor = color));
        }

        #region Fingerprint

        private void StartToScanFingerprint()
        {
            this.Invoke((Action)(() => btnScanFingerprint.Enabled = true));
        }
        private void btnScanFingerprint_Click(object sender, EventArgs e)
        {
            Enrollment();
        }

        private void Enrollment()
        {
            _futronicEnrollment = new FutronicEnrollment();

            // Set control properties
            _futronicEnrollment.FakeDetection = true;
            _futronicEnrollment.FFDControl = true;
            _futronicEnrollment.FARN = 200;
            _futronicEnrollment.Version = VersionCompatible.ftr_version_compatible;
            _futronicEnrollment.FastMode = true;
            _futronicEnrollment.MIOTControlOff = false;
            _futronicEnrollment.MaxModels = 5;
            _futronicEnrollment.MinMinuitaeLevel = 3;
            _futronicEnrollment.MinOverlappedLevel = 3;


            // register events
            _futronicEnrollment.OnPutOn += OnPutOn;
            _futronicEnrollment.OnTakeOff += OnTakeOff;
            //_futronicEnrollment.UpdateScreenImage += new UpdateScreenImageHandler(this.UpdateScreenImage);
            _futronicEnrollment.OnFakeSource += OnFakeSource;
            _futronicEnrollment.OnEnrollmentComplete += OnEnrollmentComplete;

            // start enrollment process
            _futronicEnrollment.Enrollment();
        }
        private void OnEnrollmentComplete(bool bSuccess, int nResult)
        {
            StringBuilder szMessage = new StringBuilder();
            if (bSuccess)
            {
                // set status string
                szMessage.Append("Enrollment process finished successfully.");
                szMessage.Append("Quality: ");
                szMessage.Append(_futronicEnrollment.Quality.ToString());
                Console.WriteLine(szMessage);

                _currentUser.Fingerprint = _futronicEnrollment.Template;

                Log("Your fingerprint was scanned successfully!", Color.Blue);
                this.Invoke((Action)(() => btnSave.Enabled = true));
            }
            else
            {
                Log(FutronicSdkBase.SdkRetCode2Message(nResult), Color.Red);
            }

            // unregister events
            _futronicEnrollment.OnPutOn -= OnPutOn;
            _futronicEnrollment.OnTakeOff -= OnTakeOff;
            //m_Operation.UpdateScreenImage -= new UpdateScreenImageHandler(this.UpdateScreenImage);
            _futronicEnrollment.OnFakeSource -= OnFakeSource;
            _futronicEnrollment.OnEnrollmentComplete -= OnEnrollmentComplete;

            _futronicEnrollment = null;
        }

        private bool OnFakeSource(FTR_PROGRESS Progress)
        {
            Log("Fake source detected. Continue ...", Color.Red);
            return false;
        }

        private void OnTakeOff(FTR_PROGRESS Progress)
        {
            Log("Take off finger from device, please ...", Color.Yellow);
        }

        private void OnPutOn(FTR_PROGRESS Progress)
        {
            Log("Put finger into device, please ...", Color.Yellow);
        }

        #endregion

        #region Smart Card
        private void btnScanSmartcard_Click(object sender, EventArgs e)
        {
            btnScanSmartcard.Enabled = false;
            StartCardMonitor();
        }

        private void StartCardMonitor()
        {
            Trinity.Common.Monitor.SCardMonitor.StartCardMonitor(OnCardInitialized, OnCardInserted, OnCardRemoved);
        }

        private void OnCardInitialized(object sender, CardStatusEventArgs e)
        {
            string cardUID = Trinity.Common.Monitor.SCardMonitor.GetCardUID();
            if (string.IsNullOrEmpty(cardUID))
            {
                Log("Please insert your smart card.", Color.Red);
                return;
            } else
            {
                // Scan smart card successfully
                Log("Your smart card was scanned successfully. Please scan your finger print to continue", Color.Blue);

                // Then stop Smart Card and Start to scan finger print
                Trinity.Common.Monitor.SCardMonitor.Stop();
                btnScanSmartcard.Enabled = false;

                _currentUser.SmartCardId = cardUID;
                StartToScanFingerprint();
            }
        }

        private void OnCardInserted(object sender, CardStatusEventArgs e)
        {
            string cardUID = Trinity.Common.Monitor.SCardMonitor.GetCardUID();
            if (!string.IsNullOrEmpty(cardUID))
            {
                // Scan smart card successfully
                Log("Scan smart card successfully. Please scan your finger print to continue", Color.Blue);

                // Then stop Smart Card and Start to scan finger print
                Trinity.Common.Monitor.SCardMonitor.Stop();
                btnScanSmartcard.Enabled = false;

                _currentUser.SmartCardId = cardUID;
                StartToScanFingerprint();
            }
            Log("Could not scan the smart card", Color.Red);
        }

        private void OnCardRemoved(object sender, CardStatusEventArgs e)
        {
            Log("The smart card has been removed", Color.Red);
        }
        #endregion


    }
}
