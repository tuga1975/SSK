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
            var allQueue = GetAllQueueToday(dalQueue, EnumStations.SSK);

            var setting = new DAL_Setting().GetCurrentAppointmentTime();
            var today = DateTime.Now;
            List<Trinity.BE.Queue> currentTimeslotQueue = new List<Trinity.BE.Queue>();
            List<Trinity.BE.Queue> nextTimesloteQueue = new List<Trinity.BE.Queue>();
            string servingQueueNumber = string.Empty;
            string currentQueueNumber = string.Empty;
            var waitingQueueNumbers = new List<string>();
            var holdingList = new List<string>();

            Trinity.DAL.DBContext.Timeslot _currentTs = null;
            Trinity.DAL.DBContext.Timeslot _nextTs = null;

            var currentTimeslot = string.Empty;
            var nextTimeslot = string.Empty;

            var listTodayTimeslot = new DAL_Setting().GetListTodayTimeslot();
            for (int i = 0; i < listTodayTimeslot.Count; i++)
            {
                var timeSlot = listTodayTimeslot[i];
                var diffHour = timeSlot.StartTime.Value.Hours - today.Hour;
                var diffStartMin = timeSlot.StartTime.Value.Minutes - today.Minute;
                var diffEndHour = timeSlot.EndTime.Value.Hours - today.Hour;
                var diffEndMin = timeSlot.EndTime.Value.Minutes - today.Minute;

                if (diffHour == 0 && diffStartMin <= 0 && ((diffEndHour > 0 && diffEndMin <= 0) || (diffEndHour == 0 && diffEndMin >= 0)))
                {
                    _currentTs = timeSlot;
                    currentTimeslot = timeSlot.FromTimeTxt + " - " + timeSlot.ToTimeTxt;
                    _nextTs = new DAL_Setting().GetNextTimeslotToday(timeSlot.StartTime.Value);
                    if (_nextTs != null)
                    {
                        nextTimeslot = _nextTs.FromTimeTxt + " - " + _nextTs.ToTimeTxt;
                    }
                }
            }

            for (int i = 0; i < allQueue.Count; i++)
            {
                var appointmentStartTime = new DAL_Appointments().GetAppointmentDetails(allQueue[i].AppointmentId);
                var diffHour = appointmentStartTime.StartTime.Value.Hours - today.Hour;
                var diffStartMin = appointmentStartTime.StartTime.Value.Minutes - today.Minute;
                var diffEndHour = appointmentStartTime.EndTime.Value.Hours - today.Hour;
                var diffEndMin = appointmentStartTime.EndTime.Value.Minutes - today.Minute;
                //queue - serving
                if (allQueue[i].Status == EnumQueueStatuses.Processing)
                {
                    servingQueueNumber += allQueue[i].QueueNumber + "-";
                }
                //queue - current
                if ((allQueue[i].Status == EnumQueueStatuses.Waiting|| allQueue[i].Status == EnumQueueStatuses.Processing) && diffHour == 0 && diffStartMin <= 0 && ((diffEndHour > 0 && diffEndMin <= 0) || (diffEndHour == 0 && diffEndMin >= 0)))
                {
                    currentTimeslotQueue.Add(allQueue[i]);
                    servingQueueNumber += allQueue[i].QueueNumber + "-";

                }
                //waiting - next
                else if (allQueue[i].Status == EnumQueueStatuses.Waiting && diffHour > 0)
                {
                    if (_nextTs != null && (appointmentStartTime.StartTime.Value.Hours - _nextTs.StartTime.Value.Hours) == 0 && (appointmentStartTime.StartTime.Value.Minutes - _nextTs.StartTime.Value.Minutes) == 0)
                    {
                        nextTimesloteQueue.Add(allQueue[i]);
                    }
                }
                //waiting -next
                else if (allQueue[i].Status == EnumQueueStatuses.Waiting && diffHour == 0 && (diffStartMin > 0 || (diffEndHour > 0 && diffEndMin <= 0)))
                {
                    if (_nextTs != null && (appointmentStartTime.StartTime.Value.Hours - _nextTs.StartTime.Value.Hours) == 0 && (appointmentStartTime.StartTime.Value.Minutes - _nextTs.StartTime.Value.Minutes) == 0)
                    {
                        nextTimesloteQueue.Add(allQueue[i]);
                    }
                }

            }
            if (currentTimeslotQueue.Count > 0)
            {
                for (int i = 0; i < currentTimeslotQueue.Count; i++)
                {
                    var currentTs = currentTimeslotQueue[i];

                    var appointment = new DAL_Appointments().GetMyAppointmentByID(currentTs.AppointmentId);
                    new DAL_QueueNumber().UpdateQueueStatus(currentTs.ID, EnumQueueStatuses.Processing, EnumStations.SSK);

                        currentQueueNumber += currentTs.QueueNumber + "-";


                }

            }
            //get NRIC for booked current timeslot and have not queued
            if (_currentTs != null)
            {
                var todayAppointment = new DAL_Appointments().GetAllCurrentTimeslotAppointment(_currentTs.StartTime.Value);
                foreach (var item in todayAppointment)
                {
                    var userNRIC = new DAL_User().GetUserByUserId(item.UserId, true).NRIC;
                    var qNumber = userNRIC;
                    currentQueueNumber += qNumber + "-";
                }
            }

            if (nextTimesloteQueue.Count > 0)
            {
                List<Trinity.DAL.DBContext.Queue> allNextQueue = GetAllNextQueue(nextTimesloteQueue);
                foreach (var item in allNextQueue)
                {
                    waitingQueueNumbers.Add(item.QueuedNumber);
                }

            }

            var blockedUser = new DAL_User().GetAllSuperviseeBlocked(true);

            foreach (var item in blockedUser)
            {
                holdingList.Add(item.NRIC);
            }
            //serving  //current //next  //holding-blocked user
            wbQueueNumber.RefreshQueueNumbers(servingQueueNumber, currentQueueNumber, waitingQueueNumbers.Distinct().ToArray(), holdingList.ToArray());
            wbQueueNumber.InvokeScript("setTimeslot", currentTimeslot, nextTimeslot);
        }

        //private static string SetNextTimeslotTxt(List<Trinity.BE.Queue> allQueue, string nextTimeslot, int i)
        //{
        //    var qDetail = new DAL_Appointments().GetAppointmentDetails(allQueue[i].AppointmentId);
        //    if (qDetail.StartTime.HasValue)
        //    {
        //        var nextTs = new DAL_Setting().GetNextTimeslotToday(qDetail.StartTime.Value);
        //        if (nextTs != null)
        //        {
        //            nextTimeslot = nextTs.FromTimeTxt + " - " + nextTs.ToTimeTxt;
        //        }

        //    }

        //    return nextTimeslot;
        //}

        private static List<Trinity.DAL.DBContext.Queue> GetAllNextQueue(List<Trinity.BE.Queue> allQueue)
        {
            var appointment = new DAL_Appointments().GetMyAppointmentByID(allQueue[0].AppointmentId);
            //var nextTimeslot = new DAL_Setting().GetNextTimeslotToday(appointment.Timeslot.StartTime.Value);
            var allNextQueue = new DAL_QueueNumber().GetAllQueueByNextimeslot(appointment.Timeslot.StartTime.Value, EnumStations.SSK);
            return allNextQueue;
        }

        private static List<Trinity.BE.Queue> GetAllQueueToday(DAL_QueueNumber dalQueue, string station)
        {
            return dalQueue.GetAllQueueNumberByDate(DateTime.Today, station).Select(d => new Trinity.BE.Queue()
            {
                ID = d.Queue_ID,
                AppointmentId = d.Appointment_ID,
                Status = d.QueueDetails.FirstOrDefault(qd => qd.Queue_ID == d.Queue_ID && qd.Station == station).Status,
                QueueNumber = d.QueuedNumber,
                Time = d.CreatedTime
            }).ToList();
        }


        private void RefreshQueueNumberTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var setting = new DAL_Setting().GetCurrentAppointmentTime();

            //this.timer.Interval = 1000 * 60 * setting.Duration;
            this.timer.Interval = 30000;
            this.timer.Enabled = true;
            this.RefreshQueueNumbers();
        }
    }
}
