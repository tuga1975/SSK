using System;
using System.Data;
using System.Linq;
using Newtonsoft.Json;
using System.Windows.Forms;
using Trinity.DAL;
using System.Collections.Generic;

namespace SSK
{
    public partial class FormQueueNumber : Form
    {
        private WebBrowser wbQueueNumber = null;
        private static FormQueueNumber _instance = null;
        private JSCallCS _jsCallCS = null;
        private System.Timers.Timer timer;

        public static FormQueueNumber GetInstance()
        {
            if (_instance == null)
            {
                _instance = new FormQueueNumber();
            }
            return _instance;
        }
        public FormQueueNumber()
        {
            InitializeComponent();

            this.timer = new System.Timers.Timer();
            this.timer.AutoReset = false;
            this.timer.Elapsed += RefreshQueueNumberTimer_Elapsed;
            TimerStart();

        }
        public void TimerStart()
        {
            this.timer.Interval = 1;
            //this.timer.Interval = 30000; 
            this.timer.Enabled = true;
        }
        public void TimerStop()
        {
            this.timer.Enabled = false;
        }

        private void InitializeWebBrowser()
        {
            wbQueueNumber = new WebBrowser();
            this.Controls.Add(wbQueueNumber);
            wbQueueNumber.Dock = DockStyle.Fill;
            wbQueueNumber.DocumentCompleted += wbQueueNumber_DocumentCompleted;

            _jsCallCS = new JSCallCS(wbQueueNumber);
            wbQueueNumber.Url = new Uri(String.Format("file:///{0}/View/html/Layout_QueueNumber.html", CSCallJS.curDir));
            wbQueueNumber.ObjectForScripting = _jsCallCS;
        }

        private void wbQueueNumber_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            wbQueueNumber.InvokeScript("createEvent", JsonConvert.SerializeObject(_jsCallCS.GetType().GetMethods().Where(d => d.IsPublic && !d.IsVirtual && !d.IsSecuritySafeCritical).ToArray().Select(d => d.Name)));
            wbQueueNumber.LoadPageHtml("QueueNumber.html");
            RefreshQueueNumbers();
        }
        public void ShowOnSecondaryScreen()
        {
            if (Screen.AllScreens.Count() > 1)
            {
                if (Screen.AllScreens[0].Primary)
                {

                    this.DesktopLocation = Screen.AllScreens[1].WorkingArea.Location;
                }
                else
                {
                    this.DesktopLocation = Screen.AllScreens[0].WorkingArea.Location;
                }
            }
            InitializeWebBrowser();
        }
        public void RefreshQueueNumbers()
        {
            DAL_QueueNumber dalQueue = new DAL_QueueNumber();
            var allQueue = GetAllQueueToday(dalQueue,EnumStations.SSK);

            var setting = new DAL_Setting().GetTodayEnvironmentSetting();
            var today = DateTime.Now;

            string currentQueueNumber = string.Empty;
            var waitingQueueNumbers = new List<string>();
            for (int i = 0; i < allQueue.Count; i++)
            {
                var appointmentStartTime = new DAL_Appointments().GetAppointmentDetails(allQueue[i].AppointmentId);
                var diffHour = appointmentStartTime.StartTime.Value.Hours - today.Hour;
                var diffStartMin = appointmentStartTime.StartTime.Value.Minutes - today.Minute;
                var diffEndHour = appointmentStartTime.EndTime.Value.Hours - today.Hour;
                var diffEndMin = appointmentStartTime.EndTime.Value.Minutes - today.Minute;
                if (allQueue[i].Status == EnumQueueStatuses.Waiting && diffHour == 0 && diffStartMin <= 0 && ((diffEndHour > 0 && diffEndMin <= 0) || (diffEndHour == 0 && diffEndMin >= 0)))
                {
                    currentQueueNumber += allQueue[i].QueueNumber + "-";
                }
                else if (allQueue[i].Status == EnumQueueStatuses.Waiting && diffHour > 0)
                {
                    waitingQueueNumbers.Add(allQueue[i].QueueNumber);
                }
                else if (allQueue[i].Status == EnumQueueStatuses.Waiting && diffHour == 0 && diffStartMin > 0)
                {
                    waitingQueueNumbers.Add(allQueue[i].QueueNumber);
                }
            }

            wbQueueNumber.RefreshQueueNumbers(currentQueueNumber, waitingQueueNumbers.ToArray());

        }

        private static List<Trinity.BE.Queue> GetAllQueueToday(DAL_QueueNumber dalQueue,string station)
        {
            return dalQueue.GetAllQueueNumberByDate(DateTime.Today,station).Select(d => new Trinity.BE.Queue()
            {
                ID = d.Queue_ID,
                AppointmentId = d.Appointment_ID,
                Status = d.QueueDetails.FirstOrDefault(qd => qd.Queue_ID == d.Queue_ID && qd.Station == EnumStations.SSK).Status,
                QueueNumber = d.QueuedNumber,
                Time = d.CreatedTime
            }).ToList();
        }


        private void RefreshQueueNumberTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var setting = new DAL_Setting().GetTodayEnvironmentSetting();

            //this.timer.Interval = 1000 * 60 * setting.Duration;
            this.timer.Interval = 60000;
            this.timer.Enabled = true;
            this.RefreshQueueNumbers();
        }
    }
}
