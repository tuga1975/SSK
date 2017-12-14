using Futronic.SDKHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OfficerDesktopApp
{
    public partial class FormNewUser : Form
    {
        private FutronicEnrollment _futronicEnrollment = null;
        public FormNewUser()
        {
            InitializeComponent();
        }

        private void btnSave_Click(object sender, EventArgs e)
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

                Trinity.BE.User user = new Trinity.BE.User();
                user.UserId = Guid.NewGuid().ToString();
                user.Fingerprint = _futronicEnrollment.Template;

                Trinity.DAL.DAL_User dalUser = new Trinity.DAL.DAL_User();
                
                if (dalUser.CreateUser(user, true))
                {
                    Console.WriteLine("Saved: " + user.UserId);
                    Console.WriteLine("- Data: " + user.Fingerprint.ToString());
                }
                else
                {
                    Console.WriteLine("Save unsuccessful");
                }
            }
            else
            {
                this.Invoke((Action)(() => lblNote.Text = FutronicSdkBase.SdkRetCode2Message(nResult)));
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
            Log("Fake source detected. Continue ...");
            return false;
        }

        private void OnTakeOff(FTR_PROGRESS Progress)
        {
            Log("Take off finger from device, please ...");
        }

        private void OnPutOn(FTR_PROGRESS Progress)
        {
            Log("Put finger into device, please ...");
        }

        private void Log(string message)
        {
            this.Invoke((Action)(() => lblNote.Text = message));
        }
    }
}
