using Futronic.SDKHelper;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using PCSC;
using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Trinity.DAL;
using Trinity.Identity;

namespace OfficerDesktopApp
{
    public partial class FormNewUser : Form
    {
        private Form _frmMain = null;
        private Trinity.BE.User _currentUser = null;

        private FutronicEnrollment _futronicEnrollment = null;
        public FormNewUser()
        {
            InitializeComponent();
        }

        public Form MainForm
        {
            get
            {
                return _frmMain;
            }
            set
            {
                _frmMain = value;
            }
        }

        private void FormNewUser_Load(object sender, EventArgs e)
        {
            Trinity.Common.Monitor.DeviceMonitor.Start();
            _currentUser = new Trinity.BE.User();
            _currentUser.UserId = Guid.NewGuid().ToString();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            CreateUserAsync();
        }

        private async void CreateUserAsync()
        {
            //if (string.IsNullOrEmpty(_currentUser.SmartCardId) || _currentUser.Fingerprint == null)
            //{
            //    MessageBox.Show("You have to scan your smart card and fingerprint");
            //    return;
            //}
            //
            // Prepare user information
            //

            _currentUser.Name = txtName.Text;
            _currentUser.NRIC = txtNRIC.Text;
            _currentUser.Role = String.IsNullOrEmpty(cboRoles.Text) ? EnumUserRoles.Supervisee : cboRoles.Text;

            ApplicationUser user = new ApplicationUser();
            user.UserName = _currentUser.NRIC;
            user.Name = _currentUser.Name;
            user.Email = txtPrimaryEmail.Text;
            user.Fingerprint = _currentUser.Fingerprint;
            user.NRIC = _currentUser.NRIC;
            user.PhoneNumber = txtPrimaryPhone.Text;
            user.SmartCardId = _currentUser.SmartCardId;
            user.Status = EnumUserStatuses.Active;

            UserManager<ApplicationUser> userManager = ApplicationIdentityManager.GetUserManager();
            Trinity.DAL.DAL_User dalUser = new Trinity.DAL.DAL_User();
            IdentityResult result = await userManager.CreateAsync(user, txtPassword.Text.Trim());
            if (result.Succeeded)
            {
                RoleManager<IdentityRole> roleManager = ApplicationIdentityManager.GetRoleManager();
                userManager.AddToRole(user.Id, _currentUser.Role);
                // Save to the Centralized DB also
                //dalUser.CreateUser(_currentUser, false);

                Trinity.DAL.DAL_UserProfile dalUserProfile = new Trinity.DAL.DAL_UserProfile();
                Trinity.BE.UserProfile userProfile = new Trinity.BE.UserProfile();
                userProfile.UserId = _currentUser.UserId;
                userProfile.Primary_Phone = txtPrimaryPhone.Text;
                userProfile.Primary_Email = txtPrimaryEmail.Text;
                userProfile.Nationality = txtNationality.Text;
                userProfile.DOB = dpDOB.Value;

                dalUserProfile.UpdateUserProfile(userProfile, _currentUser.UserId, true);

                // Save to the Centralized DB also
                dalUserProfile.UpdateUserProfile(userProfile, _currentUser.UserId, false);

                btnSave.Enabled = false;
                MessageBox.Show("Create user successfully!", "Create user", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Form frmMain = (Form)this.MainForm;
                frmMain.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Could not create user.", "Create user", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        delegate void UpdateNoteDelegate(string message, Color color);
        private void UpdateNote(string message, Color color)
        {
            if (this.lblNote.InvokeRequired)
            {
                UpdateNoteDelegate updateNoteDelegate = new UpdateNoteDelegate(UpdateNote);
                this.Invoke(updateNoteDelegate, new object[] { message, color });
            }
            else
            {
                lblNote.Text = message;
                lblNote.ForeColor = color;
            }
        }

        #region Fingerprint

        private void StartToScanFingerprint()
        {
            this.Invoke((Action)(() => btnScanFingerprint.Enabled = true));
        }
        private void btnScanFingerprint_Click(object sender, EventArgs e)
        {
            btnScanFingerprint.Enabled = false;
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

                UpdateNote("Your fingerprint was scanned successfully!", Color.Blue);
                this.Invoke((Action)(() => btnSave.Enabled = true));
            }
            else
            {
                UpdateNote(FutronicSdkBase.SdkRetCode2Message(nResult), Color.Red);
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
            UpdateNote("Fake source detected. Continue ...", Color.Red);
            return false;
        }

        private void OnTakeOff(FTR_PROGRESS Progress)
        {
            UpdateNote("Take off finger from device, please ...", Color.Yellow);
        }

        private void OnPutOn(FTR_PROGRESS Progress)
        {
            UpdateNote("Put finger into device, please ...", Color.Yellow);
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
            Trinity.Common.Monitor.SCardMonitor sCardMonitor = Trinity.Common.Monitor.SCardMonitor.Instance;
            sCardMonitor.StartCardMonitor(OnCardInitialized, OnCardInserted, OnCardRemoved);
        }

        private void OnCardInitialized(object sender, CardStatusEventArgs e)
        {
            Trinity.Common.Monitor.SCardMonitor sCardMonitor = Trinity.Common.Monitor.SCardMonitor.Instance;
            string cardUID = sCardMonitor.GetCardUID();
            if (string.IsNullOrEmpty(cardUID))
            {
                UpdateNote("Please insert your smart card.", Color.Red);
                return;
            }
            else
            {
                DAL_User dalUser = new DAL_User();
                Trinity.BE.User user = dalUser.GetUserBySmartCardId(cardUID, true);
                if (user != null)
                {
                    MessageBox.Show("This smart card is already in used by another person. Please user another card.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                // Scan smart card successfully
                UpdateNote("Your smart card was scanned successfully. Please scan your finger print to continue", Color.Blue);

                // Then stop Smart Card and Start to scan finger print
                sCardMonitor.Stop();
                btnScanSmartcard.Enabled = false;

                _currentUser.SmartCardId = cardUID;
                StartToScanFingerprint();
            }
        }

        private void OnCardInserted(object sender, CardStatusEventArgs e)
        {
            Trinity.Common.Monitor.SCardMonitor sCardMonitor = Trinity.Common.Monitor.SCardMonitor.Instance;
            string cardUID = sCardMonitor.GetCardUID();
            if (!string.IsNullOrEmpty(cardUID))
            {
                // Scan smart card successfully                
                DAL_User dalUser = new DAL_User();
                Trinity.BE.User user = dalUser.GetUserBySmartCardId(cardUID, true);
                if (user != null)
                {
                    MessageBox.Show("This smart card is already in used by another person. Please user another card.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Then stop Smart Card and Start to scan finger print
                sCardMonitor.Stop();
                btnScanSmartcard.Enabled = false;

                _currentUser.SmartCardId = cardUID;
                StartToScanFingerprint();

                UpdateNote("Scan smart card successfully. Please scan your finger print to continue", Color.Blue);
                return;
            }
            UpdateNote("Could not scan the smart card", Color.Red);
        }

        private void OnCardRemoved(object sender, CardStatusEventArgs e)
        {
            UpdateNote("The smart card has been removed", Color.Red);
        }

        #endregion

        private void FormNewUser_FormClosing(object sender, FormClosingEventArgs e)
        {
            Form frmMain = this.MainForm;
            frmMain.Show();
        }
    }
}
