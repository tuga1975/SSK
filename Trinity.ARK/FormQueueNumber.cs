﻿using System;
using System.Data;
using System.Linq;
using Newtonsoft.Json;
using System.Windows.Forms;
using Trinity.DAL;
using System.Collections.Generic;
using System.Diagnostics;
using Trinity.DAL.DBContext;

namespace ARK
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
        private void ClockStart()
        {
            System.Timers.Timer clock = new System.Timers.Timer();
            clock.AutoReset = true;
            clock.Interval = 1000;
            clock.Elapsed += RefreshClock;
            clock.Enabled = true;
        }
        private void RefreshClock(object sender, System.Timers.ElapsedEventArgs e)
        {
            wbQueueNumber.InvokeScript("RefreshClock", String.Format("{0:dd MMM, dddd}", DateTime.Now), String.Format("{0:hh:mm tt}", DateTime.Now));
        }
        public void TimerStart()
        {
            //this.timer.Interval = 1;
            this.timer.Interval = 30000;
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
            ClockStart();
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
            try
            {
                DAL_QueueNumber _DAL_QueueNumber = new DAL_QueueNumber();

                // Get list timeslot today
                List<Timeslot> timeslots = new DAL_Timeslots().GetTimeSlots(DateTime.Now).OrderBy(d => d.SortCategory).ThenBy(d => d.StartTime).ToList();

                List<Trinity.DAL.DBContext.Queue> allQueue = _DAL_QueueNumber.SSKGetQueueToDay();

                List<Trinity.DAL.DBContext.Queue> queueNowServing = allQueue.Where(d => d.QueueDetails.Any(c => c.Station == EnumStation.ARK && c.Status == EnumQueueStatuses.Processing)).OrderBy(d => d.QueueDetails.FirstOrDefault(c => c.Station == EnumStation.ARK).LastUpdatedDate).ToList();

                List<Trinity.DAL.DBContext.Queue> arrQueue = allQueue.Where(d => d.QueueDetails.Any(c => c.Station == EnumStation.ARK && c.Status == EnumQueueStatuses.Waiting)).OrderBy(d => d.Timeslot.SortCategory).ThenBy(d => d.Timeslot.StartTime).ToList();

                // get currentTimeslot
                Trinity.DAL.DBContext.Timeslot currentTimeslot = timeslots.FirstOrDefault(d => d.StartTime.Value <= DateTime.Now.TimeOfDay && d.EndTime.Value > DateTime.Now.TimeOfDay);
                // lấy ra timeslot nhỏ nhất vẫn còn người chờ
                Trinity.DAL.DBContext.Timeslot minTimeSlot = arrQueue.Count > 0 ? arrQueue[0].Timeslot : null;
                Trinity.DAL.DBContext.Timeslot nextTimeSlot = null;
                string textTimeSlot = string.Empty;
                string textBreakTime = string.Empty;

                //Nếu ko có currentTimeslot  hoặc time slot nhỏ hơn currentTimeslot vẫn còn queue chưa xong
                List<Trinity.DAL.DBContext.Queue> queueCurrent = new List<Queue>();
                List<Trinity.DAL.DBContext.Queue> queueOther = new List<Queue>();
                if (currentTimeslot != null && minTimeSlot != null && minTimeSlot.EndTime.Value < currentTimeslot.StartTime)
                {
                    // Neu time slot nho hon hien tai chua het nguoi
                    currentTimeslot = minTimeSlot;
                }
                if (currentTimeslot != null)
                {
                    queueCurrent = arrQueue.Where(d => d.Timeslot_ID == currentTimeslot.Timeslot_ID).OrderBy(d => d.InTimeSlot).ThenBy(d => d.Priority).ToList();
                    textTimeSlot = (DateTime.Today + currentTimeslot.StartTime.Value).ToString("hh:mm tt") + " - " + (DateTime.Today + currentTimeslot.EndTime.Value).ToString("hh:mm tt");
                }
                //// Nếu Timeslot trên đã hết ==> lấy tiếp time slot
                //if (queueCurrent.Count == 0 && minTimeSlot != null)
                //{
                //    currentTimeslot = minTimeSlot;
                //    queueCurrent = arrQueue.Where(d => d.Timeslot_ID == currentTimeslot.Timeslot_ID).OrderBy(d => d.InTimeSlot).ThenBy(d => d.Priority).ToList();
                //    textTimeSlot = (DateTime.Today + currentTimeslot.StartTime.Value).ToString("hh:mm tt") + " - " + (DateTime.Today + currentTimeslot.EndTime.Value).ToString("hh:mm tt");
                //}


                // Show List Other
                if (currentTimeslot != null)
                {
                    nextTimeSlot = timeslots.FirstOrDefault(d => d.StartTime.Value >= currentTimeslot.EndTime.Value);
                    if (nextTimeSlot != null)
                    {
                        getQueueOther(queueOther,arrQueue,timeslots,nextTimeSlot);
                    }
                }
                else
                {
                    // get nextTimeSlot
                    nextTimeSlot = timeslots.FirstOrDefault(d => d.StartTime.Value >= DateTime.Now.TimeOfDay);
                    if (nextTimeSlot != null)
                    {
                        getQueueOther(queueOther, arrQueue, timeslots, nextTimeSlot);
                        textBreakTime = "BREAK TIME (Next operation time: " + (DateTime.Today + nextTimeSlot.StartTime.Value).ToString("hh:mm tt") + ")";
                    }
                    else
                    {
                        // if nextTimeSlot == null, get first timeslot of next day
                        Timeslot nextDayTimeSlot = new DAL_Timeslots().GetNextTimeSlotFromDB();
                        if (nextDayTimeSlot != null)
                        {
                            textBreakTime = "BREAK TIME (Next operation time: " + (DateTime.Today + nextDayTimeSlot.StartTime.Value).ToString("hh:mm tt") + ")";
                        }
                    }

                }

                //Nếu queueOther <8 lấy thêm những người bị block mà có lịch hôm nay
                if (queueOther.Count < 8)
                {
                    List<Trinity.DAL.DBContext.Membership_Users> arrayUserBlocked = new DAL_User().GetSuperviseeBlockedAppointmentsAvailable(DateTime.Today, 8 - queueOther.Count);
                    foreach (var item in arrayUserBlocked)
                    {
                        queueOther.Add(new Queue()
                        {
                            QueuedNumber = Trinity.Common.CommonUtil.GetQueueNumber(item.NRIC)
                        });
                    }
                }

                if (queueNowServing.Count < 5 && queueCurrent.Count > 0)
                {
                    int maxAdd = 5 - queueNowServing.Count;
                    maxAdd = queueCurrent.Count < maxAdd ? queueCurrent.Count : maxAdd;
                    for (int i = 0; i < maxAdd; i++)
                    {
                        Queue addNowServing = queueCurrent[0];
                        queueNowServing.Add(addNowServing);
                        queueCurrent.RemoveAt(0);
                        new DAL_QueueDetails().UpdateStatusQueueDetail(addNowServing.Queue_ID, EnumStation.ARK, EnumQueueStatuses.Processing);
                    }
                }

                //if (string.IsNullOrEmpty(textTimeSlot))
                //{
                //    var time = timeslots.FirstOrDefault(d => d.StartTime >= DateTime.Now.TimeOfDay);
                //    if (time != null)
                //        textTimeSlot = (DateTime.Today + time.StartTime.Value).ToString("hh:mm tt") + " - " + (DateTime.Today + time.EndTime.Value).ToString("hh:mm tt");
                //}

                wbQueueNumber.InvokeScript("ShowTimeSlot",
                    string.IsNullOrEmpty(textTimeSlot) ? textBreakTime : textTimeSlot,
                    JsonConvert.SerializeObject(queueNowServing.Select(d => new { d.QueuedNumber, d.Type })),
                    JsonConvert.SerializeObject(queueCurrent.Select(d => new { d.QueuedNumber, d.Type }).Take(12)),
                    JsonConvert.SerializeObject(queueOther.Select(d => new { d.QueuedNumber, d.Type }).Take(8))
                    );

            }
            catch (Exception ex)
            {
                Trinity.Common.Utils.LogManager.Error("RefreshQueueNumbers: " + ex.ToString());
            }
        }

        private void getQueueOther(List<Trinity.DAL.DBContext.Queue> ortherQueue, List<Trinity.DAL.DBContext.Queue> arrayQueue, List<Timeslot> timeslots, Timeslot nextTimeSlot)
        {
            if (ortherQueue.Count < 8 && nextTimeSlot != null)
            {
                ortherQueue.AddRange(arrayQueue.Where(d => d.Timeslot_ID == nextTimeSlot.Timeslot_ID).OrderBy(d => d.InTimeSlot).ThenBy(d => d.Priority));
                if (ortherQueue.Count > 8)
                {
                    ortherQueue = ortherQueue.Take(8).ToList();
                }
                else if (ortherQueue.Count < 8)
                {
                    nextTimeSlot = timeslots.FirstOrDefault(d => d.StartTime.Value >= nextTimeSlot.EndTime.Value);
                    if (nextTimeSlot!=null)
                    {
                        getQueueOther(ortherQueue, arrayQueue, timeslots, nextTimeSlot);
                    }
                }
            }
        }
        private static List<Trinity.DAL.DBContext.Queue> GetAllNextQueue(List<Trinity.BE.Queue> allQueue)
        {
            var appointment = new DAL_Appointments().GetAppointmentByID(allQueue[0].AppointmentId);
            //var nextTimeslot = new DAL_Setting().GetNextTimeslotToday(appointment.Timeslot.StartTime.Value);
            var allNextQueue = new DAL_QueueNumber().GetAllQueueByNextimeslot(appointment.Timeslot.StartTime.Value, EnumStation.ARK);
            return allNextQueue;
        }
        private void RefreshQueueNumberTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.RefreshQueueNumbers();
            this.timer.Interval = 30000;
            this.timer.Enabled = true;
        }
    }
}
