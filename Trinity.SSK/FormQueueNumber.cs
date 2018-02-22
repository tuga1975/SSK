﻿using System;
using System.Data;
using System.Linq;
using Newtonsoft.Json;
using System.Windows.Forms;
using Trinity.DAL;
using System.Collections.Generic;
using System.Diagnostics;
using Trinity.DAL.DBContext;

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
            //holding-blocked user: user.block
            List<string> holdingList = new DAL_QueueNumber().GetHoldingListByDate(DateTime.Now);
            List<string> todayHoldingList = new List<string>();
            if (holdingList != null && holdingList.Count > 0)
            {
                for (int i = 0; i < holdingList.Count; i++)
                {
                    todayHoldingList.Add(Trinity.Common.CommonUtil.GetQueueNumber(holdingList[i]));
                }
            }

            // Get list timeslot today
            List<Timeslot> timeslots = new DAL_Timeslots().GetTimeSlots(DateTime.Now);

            // get currentTimeslot
            Timeslot currentTimeslot = timeslots.FirstOrDefault(d => d.StartTime.Value <= DateTime.Now.TimeOfDay && d.EndTime.Value > DateTime.Now.TimeOfDay);

            // get nextTimeslot
            Timeslot nextTimeslot;
            if (currentTimeslot != null)
            {
                // update queue status waiting to processing
                new DAL_QueueNumber().UpdateQueueStatus_SSK(currentTimeslot.Timeslot_ID);
                nextTimeslot = timeslots.FirstOrDefault(d => d.StartTime.Value >= currentTimeslot.EndTime.Value);
            }
            else
            {
                nextTimeslot = timeslots.FirstOrDefault(d => d.StartTime.Value >= DateTime.Now.TimeOfDay);
            }

           

            // Get list queue todaynew DAL_QueueNumber();
            List<Trinity.BE.QueueDetail> allQueue = new DAL_QueueNumber().GetAllQueue_SSK(DateTime.Now);
            // remove blocked users
            foreach (var queuedNumber in todayHoldingList)
            {
                allQueue.RemoveAll(q => q.QueuedNumber == queuedNumber);
            }

            //serving: SSK status = processing
            //current: SSK status = waiting, timeslot = current
            //next: SSK status = waiting, timeslot = next
            string servingQueuedNumber = string.Empty;
            string currentQueuedNumber = string.Empty;
            List<string> nextQueuedNumber = new List<string>();
            foreach (var queue in allQueue)
            {
                if (queue.Status == EnumQueueStatuses.Processing)
                {
                    //serving
                    servingQueuedNumber += queue.QueuedNumber + "-";
                }
                else if (queue.Status == EnumQueueStatuses.Waiting && queue.Timeslot_ID == currentTimeslot?.Timeslot_ID)
                {
                    //current
                    currentQueuedNumber += queue.QueuedNumber + "-";
                }
                else if (queue.Status == EnumQueueStatuses.Waiting && queue.Timeslot_ID == nextTimeslot?.Timeslot_ID)
                {
                    //next
                    nextQueuedNumber.Add(queue.QueuedNumber);
                }
            }

            // display
            wbQueueNumber.RefreshQueueNumbers(servingQueuedNumber, currentQueuedNumber, nextQueuedNumber.ToArray(), todayHoldingList.ToArray());
            wbQueueNumber.InvokeScript("setTimeslot", currentTimeslot?.FromTimeTxt + " - " + currentTimeslot?.ToTimeTxt, nextTimeslot?.FromTimeTxt + " - " + nextTimeslot?.ToTimeTxt);

            //Stopwatch stopWatch = new Stopwatch();
            //stopWatch.Start();
            //DAL_QueueNumber dalQueue = new DAL_QueueNumber();
            //List<Trinity.BE.Queue> allQueue = dalQueue.GetAllQueueToday(EnumStations.SSK);

            //var setting = new DAL_Setting().GetCurrentApptmtTime();
            //var today = DateTime.Now;
            //List<Trinity.BE.Queue> currentTimeslotQueue = new List<Trinity.BE.Queue>();
            //List<Trinity.BE.Queue> nextTimesloteQueue = new List<Trinity.BE.Queue>();
            //string servingQueueNumber = string.Empty;
            //string currentQueueNumber = string.Empty;
            //var waitingQueueNumbers = new List<string>();
            ////var holdingList = new List<string>();

            //Trinity.DAL.DBContext.Timeslot _currentTs = null;
            //Trinity.DAL.DBContext.Timeslot _nextTs = null;

            //var currentTimeslot = string.Empty;
            //var nextTimeslot = string.Empty;

            //var listTodayTimeslot = new DAL_Setting().GetListTodayTimeslot();
            //_currentTs = listTodayTimeslot.FirstOrDefault(d => d.StartTime.Value <= DateTime.Now.TimeOfDay && d.EndTime.Value > DateTime.Now.TimeOfDay);
            //if (_currentTs != null)
            //{
            //    currentTimeslot = _currentTs.FromTimeTxt + " - " + _currentTs.ToTimeTxt;
            //    _nextTs = listTodayTimeslot.FirstOrDefault(d => d.StartTime.Value >= _currentTs.EndTime.Value);
            //    if (_nextTs != null)
            //    {
            //        nextTimeslot = _nextTs.FromTimeTxt + " - " + _nextTs.ToTimeTxt;
            //    }
            //}
            //else
            //{
            //    _nextTs = listTodayTimeslot.FirstOrDefault(d => d.StartTime.Value >= DateTime.Now.TimeOfDay);
            //    if (_nextTs != null)
            //    {
            //        nextTimeslot = _nextTs.FromTimeTxt + " - " + _nextTs.ToTimeTxt;
            //    }

            //}
            //for (int i = 0; i < listTodayTimeslot.Count; i++)
            //{
            //    var timeSlot = listTodayTimeslot[i];
            //    var diffHour = timeSlot.StartTime.Value.Hours - today.Hour;
            //    var diffStartMin = timeSlot.StartTime.Value.Minutes - today.Minute;
            //    var diffEndHour = timeSlot.EndTime.Value.Hours - today.Hour;
            //    var diffEndMin = timeSlot.EndTime.Value.Minutes - today.Minute;

            //    if (diffHour == 0 && diffStartMin <= 0 && ((diffEndHour > 0 && diffEndMin <= 0) || (diffEndHour == 0 && diffEndMin >= 0)))
            //    {
            //        _currentTs = timeSlot;
            //        currentTimeslot = timeSlot.FromTimeTxt + " - " + timeSlot.ToTimeTxt;
            //        _nextTs = new DAL_Setting().GetNextTimeslotToday(timeSlot.StartTime.Value);
            //        if (_nextTs != null)
            //        {
            //            nextTimeslot = _nextTs.FromTimeTxt + " - " + _nextTs.ToTimeTxt;
            //        }
            //    }
            //}

            //for (int i = 0; i < allQueue.Count; i++)
            //{
            //    var result= new DAL_Appointments().GetAppmtDetails(allQueue[i].AppointmentId);
            //    var appointmentStartTime = result;
            //    var diffHour = appointmentStartTime.StartTime.Value.Hours - today.Hour;
            //    var diffStartMin = appointmentStartTime.StartTime.Value.Minutes - today.Minute;
            //    var diffEndHour = appointmentStartTime.EndTime.Value.Hours - today.Hour;
            //    var diffEndMin = appointmentStartTime.EndTime.Value.Minutes - today.Minute;
            //    //queue - serving
            //    if (allQueue[i].Status == EnumQueueStatuses.Processing)
            //    {
            //        servingQueueNumber += allQueue[i].QueueNumber + "-";
            //    }
            //    //queue - current
            //    if (allQueue[i].Status == EnumQueueStatuses.Waiting && diffHour == 0 && diffStartMin <= 0 && ((diffEndHour > 0 && diffEndMin <= 0) || (diffEndHour == 0 && diffEndMin >= 0)))
            //    {
            //        currentTimeslotQueue.Add(allQueue[i]);
            //        servingQueueNumber += allQueue[i].QueueNumber + "-";

            //    }
            //    //waiting - next
            //    else if (allQueue[i].Status == EnumQueueStatuses.Waiting && diffHour > 0)
            //    {
            //        if (_nextTs != null && (appointmentStartTime.StartTime.Value.Hours - _nextTs.StartTime.Value.Hours) == 0 && (appointmentStartTime.StartTime.Value.Minutes - _nextTs.StartTime.Value.Minutes) == 0)
            //        {
            //            nextTimesloteQueue.Add(allQueue[i]);
            //        }
            //    }
            //    //waiting -next
            //    else if (allQueue[i].Status == EnumQueueStatuses.Waiting && diffHour == 0 && (diffStartMin > 0 || (diffEndHour > 0 && diffEndMin <= 0)))
            //    {
            //        if (_nextTs != null && (appointmentStartTime.StartTime.Value.Hours - _nextTs.StartTime.Value.Hours) == 0 && (appointmentStartTime.StartTime.Value.Minutes - _nextTs.StartTime.Value.Minutes) == 0)
            //        {
            //            nextTimesloteQueue.Add(allQueue[i]);
            //        }
            //    }

            //}
            //if (currentTimeslotQueue.Count > 0)
            //{
            //    for (int i = 0; i < currentTimeslotQueue.Count; i++)
            //    {
            //        var currentTs = currentTimeslotQueue[i];

            //       // var appointment = new DAL_Appointments().GetMyAppointmentByID(currentTs.AppointmentId);
            //        //new DAL_QueueNumber().UpdateQueueStatus(currentTs.ID, EnumQueueStatuses.Processing, EnumStations.SSK);

            //            currentQueueNumber += currentTs.QueueNumber + "-";


            //    }

            //}
            //get NRIC for booked current timeslot and have not queued
            //if (_currentTs != null)
            //{
            //    var result= new DAL_Appointments().GetAllCurrentTimeslotApptmt(_currentTs.StartTime.Value);
            //    var todayAppointment = result;
            //    foreach (var item in todayAppointment)
            //    {

            //        var userNRIC = new DAL_User().GetUserByUserId(item.UserId).Data.NRIC;
            //        var qNumber = userNRIC.GetLast(5);
            //        currentQueueNumber += qNumber + "-";
            //    }
            //}

            //if (nextTimesloteQueue.Count > 0)
            //{
            //    List<Trinity.DAL.DBContext.Queue> allNextQueue = GetAllNextQueue(nextTimesloteQueue);
            //    foreach (var item in allNextQueue)
            //    {
            //        waitingQueueNumbers.Add(item.QueuedNumber);
            //    }
            //}

            //List<string> holdingList = new DAL_QueueNumber().GetHoldingListByDate(DateTime.Now);
            //List<string> todayHoldingList = new List<string>();
            //if (holdingList != null && holdingList.Count > 0)
            //{
            //    for (int i = 0; i < holdingList.Count; i++)
            //    {
            //        todayHoldingList.Add(Trinity.Common.CommonUtil.GetQueueNumber(holdingList[i]));
            //    }
            //}


            //serving: SSK status = processing
            //current: SSK status = waiting, timeslot = current
            //next: SSK status = waiting, timeslot = next
            //holding-blocked user: user.block

            //serving  //current //next  //holding-blocked user
            //wbQueueNumber.RefreshQueueNumbers(servingQueueNumber, currentQueueNumber, waitingQueueNumbers.Distinct().ToArray(), todayHoldingList.ToArray());
            //wbQueueNumber.InvokeScript("setTimeslot", currentTimeslot, nextTimeslot);
            //stopWatch.Stop();
            //Console.WriteLine(stopWatch.Elapsed.TotalSeconds+"--"+ stopWatch.Elapsed.TotalSeconds);
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
            var appointment = new DAL_Appointments().GetAppointmentByID(allQueue[0].AppointmentId);
            //var nextTimeslot = new DAL_Setting().GetNextTimeslotToday(appointment.Timeslot.StartTime.Value);
            var allNextQueue = new DAL_QueueNumber().GetAllQueueByNextimeslot(appointment.Timeslot.StartTime.Value, EnumStations.SSK);
            return allNextQueue;
        }

        //private static List<Trinity.BE.Queue> GetAllQueueToday(DAL_QueueNumber dalQueue, string station)
        //{
        //    return dalQueue.GetAllQueueNumberByDate(DateTime.Today, station).Select(d => new Trinity.BE.Queue()
        //    {
        //        ID = d.Queue_ID,
        //        AppointmentId = d.Appointment_ID,
        //        Status = d.QueueDetails.FirstOrDefault(qd => qd.Queue_ID == d.Queue_ID && qd.Station == station).Status,
        //        QueueNumber = d.QueuedNumber,
        //        Time = d.CreatedTime
        //    }).ToList();
        //}


        private void RefreshQueueNumberTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {

            var setting = new DAL_Setting().GetCurrentApptmtTime();

            //this.timer.Interval = 1000 * 60 * setting.Duration;
            this.timer.Interval = 30000;
            this.timer.Enabled = true;
            this.RefreshQueueNumbers();
        }
    }
}
