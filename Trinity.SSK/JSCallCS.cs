using Newtonsoft.Json;
using SSK.CodeBehind.Authentication;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using Trinity.Common;
using Trinity.DAL;
using Trinity.DAL.DBContext;
using Trinity.Device;
using Trinity.BE;

namespace SSK
{
    [System.Runtime.InteropServices.ComVisibleAttribute(true)]
    public class JSCallCS : JSCallCSBase
    {
        public event EventHandler<NRICEventArgs> OnNRICFailed;
        public event EventHandler<ShowMessageEventArgs> OnShowMessage;
        public event Action OnLogOutCompleted;

        public JSCallCS(WebBrowser web)
        {
            this._web = web;
            _thisType = this.GetType();
        }

        #region virtual events
        // Wrap event invocations inside a protected virtual method
        // to allow derived classes to override the event invocation behavior
        protected virtual void RaiseOnNRICFailedEvent(NRICEventArgs e)
        {
            OnNRICFailed?.Invoke(this, e);
        }

        protected virtual void RaiseOnShowMessageEvent(ShowMessageEventArgs e)
        {
            OnShowMessage?.Invoke(this, e);
        }

        protected virtual void RaiseLogOutCompletedEvent()
        {
            OnLogOutCompleted?.Invoke();
        }
        #endregion

        public void PopupMessage(string title, string content)
        {
            this._web.LoadPopupHtml("PopupMessage.html", new object[] { title, content });
        }
        public void LoadNotications()
        {

            Session currentSession = Session.Instance;
            Trinity.BE.User user = (Trinity.BE.User)currentSession[CommonConstants.USER_LOGIN];

            DAL_Notification dalNotification = new DAL_Notification();

            List<Trinity.BE.Notification> myNotifications = dalNotification.GetAllNotifications(user.UserId);

            var model = myNotifications;
            _web.LoadPageHtml("Notifications.html", myNotifications);
        }

        public void ChangeReadStatus(string notificationId)
        {
            try
            {

                var dalNotify = new DAL_Notification();
                dalNotify.ChangeReadStatus(notificationId);
            }
            catch (Exception ex)
            {

                return;
            }
        }
        public void SpeakNotification(string notificationId)
        {
            DAL_Notification dalNotify = new DAL_Notification();
            string content = dalNotify.GetNotification(notificationId);
            APIUtils.TextToSpeech.Speak(content);
        }

        #region BookAppointment
        public void BookAppointment()
        {
            Session session = Session.Instance;
            Trinity.BE.User user = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];
            var eventCenter = Trinity.Common.Common.EventCenter.Default;

            // check supervisee have appoitment
            Trinity.DAL.DBContext.Appointment appointment = new DAL_Appointments().GetNextAppointment(user.UserId);

            if (appointment == null)
            {
                eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Name = EventNames.ALERT_MESSAGE, Message = "You have no appointment" });
                return;
            }

            // get timeslots of appoitment day
            List<Timeslot> timeslots = new DAL_Timeslots().GetTimeSlots(appointment.Date);

            if (timeslots == null)
            {
                eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Name = EventNames.ALERT_MESSAGE, Message = "Have no timeslot to select. \nPlease contact Duty officer." });
                return;
            }

            // get GetOperationSetting of appointment day
            var operationSettings = new DAL_Setting().GetOperationSettings();
            Trinity.BE.SettingDetails settingDetail = null;
            switch (appointment.Date.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    settingDetail = operationSettings.Sunday;
                    break;
                case DayOfWeek.Monday:
                    settingDetail = operationSettings.Monday;
                    break;
                case DayOfWeek.Tuesday:
                    settingDetail = operationSettings.Tuesday;
                    break;
                case DayOfWeek.Wednesday:
                    settingDetail = operationSettings.Wednesday;
                    break;
                case DayOfWeek.Thursday:
                    settingDetail = operationSettings.Thursday;
                    break;
                case DayOfWeek.Friday:
                    settingDetail = operationSettings.Friday;
                    break;
                case DayOfWeek.Saturday:
                    settingDetail = operationSettings.Saturday;
                    break;
                default:
                    break;
            }

            if (settingDetail == null)
            {
                eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Name = EventNames.ALERT_MESSAGE, Message = "Can not get timeslots. \nPlease contact Duty officer." });
                return;
            }

            // set workingTimeshift
            Trinity.BE.WorkingTimeshift workingTimeshift = new Trinity.BE.WorkingTimeshift();
            workingTimeshift.Morning = GetWorkingTimeshift(timeslots, appointment.Timeslot_ID, EnumTimeshift.Morning);
            workingTimeshift.Afternoon = GetWorkingTimeshift(timeslots, appointment.Timeslot_ID, EnumTimeshift.Afternoon);
            workingTimeshift.Evening = GetWorkingTimeshift(timeslots, appointment.Timeslot_ID, EnumTimeshift.Evening);

            // redirect
            this._web.LoadPageHtml("BookAppointment.html", new object[] { appointment, workingTimeshift });
        }

        public bool CheckBookingTime(string timeslotId)
        {
            return new DAL_Timeslots().CheckTimeslot(timeslotId);
        }

        private List<WorkingShiftDetails> GetWorkingTimeshift(List<Timeslot> timeslots, string selected_Timeslot_ID, string timeshift)
        {
            try
            {

                List<WorkingShiftDetails> returnValue = timeslots.Where(item => item.Category == timeshift)
                    .Select(item => new WorkingShiftDetails()
                    {
                        Timeslot_ID = item.Timeslot_ID,
                        StartTime = item.StartTime.Value,
                        EndTime = item.EndTime.Value,
                        IsAvailble = new DAL_Timeslots().CheckAvailableTimeslot(item),
                        IsSelected = selected_Timeslot_ID == item.Timeslot_ID,
                        Category = item.Category
                    }).OrderBy(item => item.StartTime).ToList();

                return returnValue;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        //private Trinity.BE.WorkingTimeshift SetSelectedTimes( Appointment appointment)
        //{
        //    var date = appointment.Date;

        //    Trinity.BE.WorkingTimeshift selectedTimes = new DAL_Setting().GetApptmtTime(date); 
        //    SetSelectedTime(appointment, selectedTimes.Morning);
        //    SetSelectedTime(appointment, selectedTimes.Afternoon);
        //    SetSelectedTime(appointment, selectedTimes.Evening);
        //    this._web.LoadPageHtml("BookAppointment.html", new object[] { appointment, selectedTimes });
        //    return selectedTimes;
        //}

        //private static void SetSelectedTime(Appointment appointment, List<Trinity.BE.WorkingShiftDetails> selectedTimes)
        //{


        //    var item = selectedTimes.Where(d => appointment.Timeslot != null && appointment.Timeslot.StartTime != null && d.StartTime == appointment.Timeslot.StartTime.Value && d.EndTime == appointment.Timeslot.EndTime.Value).FirstOrDefault();
        //    if (item != null)
        //    {
        //        item.IsSelected = true;
        //    }

        //    if (!string.IsNullOrEmpty(appointment.Timeslot_ID))
        //    {


        //        var maxAppPerTimeslot = new DAL_Appointments().GetMaxNumberOfTimeslot(appointment.Timeslot_ID);
        //        foreach (var selectedItem in selectedTimes)
        //        {
        //            var count = new DAL_Appointments().CountListApptmtByTimeslot(appointment); 
        //            if (count >= maxAppPerTimeslot)
        //            {
        //                selectedItem.IsAvailble = false;
        //            }
        //        }
        //    }
        //}

        public bool UpdateTimeAppointment(string appointment_ID, string timeslot_ID)
        {
            try
            {
                // if appointment is in queue, return false
                bool inQueue = new DAL_QueueNumber().IsInQueue(appointment_ID, EnumStations.SSK);
                if (inQueue)
                {
                    return false;
                }

                // if not, update Appointments.timeslotId
                bool updateResult = new DAL_Appointments().UpdateTimeslot_ID(appointment_ID, timeslot_ID);

                if (updateResult)
                {
                    Trinity.BE.Appointment appointment = new DAL_Appointments().GetAppointment(appointment_ID);

                    //APIUtils.Printer.PrintAppointmentDetails("AppointmentDetailsTemplate.html", appointment);
                    ReceiptPrinterUtil.Instance.PrintAppointmentDetails(new AppointmentDetails()
                    {
                        Date = appointment.AppointmentDate.Value,
                        Name = appointment.Name,
                        Venue = appointment.NRIC
                    });
                    FormQueueNumber f = FormQueueNumber.GetInstance();
                    f.RefreshQueueNumbers();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                return false;
            }


            //var dbAppointment = new DAL_Appointments().GetAppointmentByID(Guid.Parse(IDAppointment));
            ////check exist queue
            //var dalQueue = new DAL_QueueNumber();
            //if (dbAppointment != null)
            //{
            //if (!dalQueue.CheckQueueExistToday(dbAppointment.UserId, EnumStations.SSK))
            //    {
            //        //var data = JsonConvert.SerializeObject(new { IDAppointment, timeStart, timeEnd });
            //        new DAL_Appointments().UpdateBookingTime(IDAppointment, timeStart, timeEnd);

            //        // dalAppointments.UpdateBookTime(IDAppointment, timeStart, timeEnd);

            //        Trinity.BE.Appointment appointment = new DAL_Appointments().GetAppmtDetails(Guid.Parse(IDAppointment)); 
            //        //Trinity.BE.Appointment appointment = dalAppointments.GetAppointmentDetails(new Guid(IDAppointment));

            //        APIUtils.Printer.PrintAppointmentDetails("AppointmentDetailsTemplate.html", appointment);
            //        FormQueueNumber f = FormQueueNumber.GetInstance();
            //        f.RefreshQueueNumbers();
            //        return true;
            //    }
            //    else
            //    {
            //        return false;
            //    }

            //}
            //return false;
        }

        public void PrintAppointmentDetails(string appointmentId)
        {

            var dalAppointment = new DAL_Appointments();
            Trinity.BE.Appointment appointment = new DAL_Appointments().GetAppmtDetails(Guid.Parse(appointmentId));
            //Trinity.BE.Appointment appointment = dalAppointment.GetAppointmentDetails(Guid.Parse(appointmentId));
            ReceiptPrinterUtil.Instance.PrintAppointmentDetails(new AppointmentDetails()
            {
                Date = appointment.AppointmentDate.Value,
                Name = appointment.Name,
                Venue = appointment.NRIC
            });
            //APIUtils.Printer.PrintAppointmentDetails("AppointmentDetailsTemplate.html", appointment);
        }
        #endregion
        public void LoadProfile()
        {
            try
            {

                Session session = Session.Instance;
                if (session.IsAuthenticated)
                {
                    Trinity.BE.User user = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];

                    var dalUser = new Trinity.DAL.DAL_User();
                    var dalUserprofile = new Trinity.DAL.DAL_UserProfile();
                    var profileModel = new Trinity.BE.ProfileModel
                    {
                        User = user,
                        UserProfile = new DAL_UserProfile().GetProfile(user.UserId),
                        Addresses = new DAL_UserProfile().GetAddByUserId(user.UserId),
                        OtherAddress = new DAL_UserProfile().GetAddByUserId(user.UserId, true),
                    };

                    //profile model 
                    _web.LoadPageHtml("Profile.html", profileModel);
                }
            }
            catch (Exception ex)
            {
                _web.LoadPageHtml("Profile.html", new Trinity.BE.ProfileModel());
            }
        }

        public void LoadSupervisee()
        {
            var _suppervisee = new CodeBehind.Suppervisee(_web);
            _suppervisee.Start();
        }
        public void SaveProfile(string param, bool primaryInfoChange)
        {
            try
            {

                var rawData = JsonConvert.DeserializeObject<Trinity.BE.ProfileRawMData>(param);
                var data = new Trinity.BE.ProfileRawMData().ToProfileModel(rawData);
                Session session = Session.Instance;
                if (primaryInfoChange)
                {
                    var updateUserResult = new DAL_User().Update(data.User);
                    // dalUser.UpdateUser(data.User, data.User.UserId, true);
                    var userProfileModel = data.UserProfile;
                    userProfileModel.UserId = data.User.UserId;
                    var updateUProfileResult = new DAL_UserProfile().UpdateProfile(userProfileModel);
                    // dalUserprofile.UpdateUserProfile(data.UserProfile,data.User.UserId , true);
                    //send notifiy to duty officer
                    APIUtils.SignalR.SendAllDutyOfficer(data.User.UserId, "A supervisee has updated profile.", "Please check Supervisee's information!", NotificationType.Notification);

                    session[CommonConstants.USER_LOGIN] = data.User;

                }
                else
                {
                    var userProfileModel = data.UserProfile;
                    userProfileModel.UserId = data.User.UserId;
                    var updateUProfileResult = new DAL_UserProfile().UpdateProfile(userProfileModel);
                    // dalUserprofile.UpdateUserProfile(data.UserProfile, data.User.UserId, true);
                    //send notifiy to case officer
                    APIUtils.SignalR.SendAllDutyOfficer(data.User.UserId, "A supervisee has updated profile.", "Please check Supervisee's information!", NotificationType.Notification);
                }

                //load Supervisee page 
                LoadPage("Supervisee.html");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("JSCallCS.SaveProfile exception: " + ex.ToString());
                CSCallJS.InvokeScript(_web, "showMessage", "Update failed!!!\n Please check the input information.");
                return;
            }
        }

        public void LoadScanDocument(string jsonData)
        {
            try
            {
                Session session = Session.Instance;
                session[CommonConstants.PROFILE_DATA] = jsonData;
                APIUtils.SignalR.SendAllDutyOfficer(null, "Supervisee's information changed!", "Please check the Supervisee's information!", NotificationType.Notification);
                LoadPage("Document.html");

            }
            catch (Exception)
            {
                var eventCenter = Trinity.Common.Common.EventCenter.Default;
                eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Name = EventNames.SOMETHING_WENT_WRONG, Message = "Something wrong happened!" });
                LoadProfile();
            }
        }
        public void LoadScanDocumentForAbsence(string jsonData, string reason)
        {

            try
            {
                Session session = Session.Instance;


                var reasonModel = JsonConvert.DeserializeObject<Trinity.BE.Reason>(reason);
                var result = new DAL_AbsenceReporting().SetInfo(reasonModel);
                //var absenceModel = dalAbsence.SetInfo(reasonModel);
                session[CommonConstants.ABSENCE_REPORTING_DATA] = result;

                LoadPage("DocumentFromQueue.html");

            }
            catch (Exception)
            {
                var eventCenter = Trinity.Common.Common.EventCenter.Default;
                eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Name = EventNames.SOMETHING_WENT_WRONG, Message = "Something wrong happened!" });
                LoadProfile();
            }
        }
        public void UpdateProfileAfterScanDoc()
        {
            Session session = Session.Instance;
            var jsonData = session[CommonConstants.PROFILE_DATA];

            SaveProfile(jsonData.ToString(), true);
        }

        public void SubmitNRIC(string strNRIC)
        {
            NRIC nric = NRIC.GetInstance(_web);
            nric.NRICAuthentication(strNRIC);
        }

        public int GetCountMyAbsence()
        {

            Session session = Session.Instance;
            Trinity.BE.User user = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];

            // if duty officcer override, set supervisee info into user
            if (user.Role == EnumUserRoles.DutyOfficer)
            {
                user = (Trinity.BE.User)session[CommonConstants.SUPERVISEE];
            }
            return new DAL_Appointments().CountAbsenceReport(user.UserId);
            //return new DAL_Appointments().CountMyAbsence(user.UserId);
        }

        // Reporting for Queue Number
        public void ReportingForQueueNumber()
        {

            // get user info
            Session session = Session.Instance;
            Trinity.BE.User user = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];

            // if duty officcer override, set supervisee info into user
            if (user.Role == EnumUserRoles.DutyOfficer)
            {
                user = (Trinity.BE.User)session[CommonConstants.SUPERVISEE];
            }

            int countAbsence = 0;

            countAbsence = new DAL_Appointments().CountAbsenceReport(user.UserId);
            //countAbsence = dalAppointment.CountMyAbsence(user.UserId);
            if (countAbsence == 0)
            {
                DAL_Notification noti = new DAL_Notification();
                if (noti.CountGetMyNotifications(user.UserId, true) > 0)
                {
                    LoadNotications();
                }
                else
                {
                    GetMyQueueNumber();
                }
            }
            else if (countAbsence >= 3)
            {
                var eventCenter = Trinity.Common.Common.EventCenter.Default;
                eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Name = EventNames.ABSENCE_MORE_THAN_3, Message = "You have been blocked for 3 or more absences \n Please report to the Duty Officer" });
                //for testing purpose
                //notify to officer
                APIUtils.SignalR.SendAllDutyOfficer(user.UserId, "Supervisee got blocked for 3 or more absences", "Please check the Supervisee's information!", NotificationType.Caution);
                var dalUser = new DAL_User();
                //active the user
                //dalUser.ChangeUserStatus(user.UserId, EnumUserStatuses.New);

                //create absence reporting
                var listAppointment = new DAL_Appointments().GetAbsentAppointments(user.UserId);
                // var listAppointment = dalAppointment.GetMyAbsentAppointments(user.UserId);
                session[CommonConstants.LIST_APPOINTMENT] = listAppointment;
                _web.LoadPageHtml("ReasonsForQueue.html", listAppointment);
            }
            else if (countAbsence > 0 && countAbsence < 3)
            {

                var listAppointment = new DAL_Appointments().GetAbsentAppointments(user.UserId);
                var eventCenter = Trinity.Common.Common.EventCenter.Default;
                eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Name = EventNames.ABSENCE_LESS_THAN_3, Message = "You have been absent for " + countAbsence + " times.\nPlease provide reasons and the supporting documents." });

                this._web.LoadPageHtml("ReasonsForQueue.html", listAppointment);
            }
        }

        private void GetMyQueueNumber()
        {

            // get user info
            Session session = Session.Instance;
            Trinity.BE.User user = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];
            Trinity.BE.User userSupervise = null;
            if (user.Role == EnumUserRoles.DutyOfficer)
            {
                userSupervise = (Trinity.BE.User)session[CommonConstants.SUPERVISEE];
            }
            else
            {
                userSupervise = user;
            }
            DAL_Appointments _Appointment = new DAL_Appointments();
            Trinity.DAL.DBContext.Appointment appointment = new DAL_Appointments().GetAppointmentByDate(userSupervise.UserId, DateTime.Today);
            //Trinity.DAL.DBContext.Appointment appointment = _Appointment.GetMyAppointmentByDate(user.UserId, DateTime.Today);
            if (appointment == null && user.Role == EnumUserRoles.Supervisee)
            {
                var eventCenter = Trinity.Common.Common.EventCenter.Default;
                eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Name = EventNames.ALERT_MESSAGE, Message = "You have no appointment today" });
            }
            else
            {
                var _dalQueue = new DAL_QueueNumber();
                Trinity.DAL.DBContext.Queue queueNumber = null;

                if (!_dalQueue.IsUserAlreadyQueue(userSupervise.UserId, DateTime.Today))
                {
                    if (appointment != null && string.IsNullOrEmpty(appointment.Timeslot_ID))
                    {
                        var eventCenter = Trinity.Common.Common.EventCenter.Default;
                        eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Name = EventNames.ALERT_MESSAGE, Message = "You have not selected the timeslot!\n Please go to Book Appointment page to select a timeslot." });
                    }
                    else if (appointment != null && !string.IsNullOrEmpty(appointment.Timeslot_ID))
                    {
                        queueNumber = _dalQueue.InsertQueueNumber(appointment.ID, appointment.UserId, EnumStations.SSK, user.UserId);
                        if (queueNumber!=null)
                        {
                            APIUtils.FormQueueNumber.RefreshQueueNumbers();
                            var eventCenter = Trinity.Common.Common.EventCenter.Default;
                            eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Name = EventNames.ALERT_MESSAGE, Message = "Your queue number is:" + queueNumber.QueuedNumber });
                        }
                        else
                        {
                            this._web.InvokeScript("ShowMessageBox", "Sorry all timeslots are fully booked!");
                        }
                        
                    }
                    else
                    {
                        queueNumber = _dalQueue.InsertQueueNumberFromDO(appointment.UserId, EnumStations.SSK, user.UserId);
                        if (queueNumber != null)
                        {
                            APIUtils.FormQueueNumber.RefreshQueueNumbers();
                            var eventCenter = Trinity.Common.Common.EventCenter.Default;
                            eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Name = EventNames.ALERT_MESSAGE, Message = "Your queue number is:" + queueNumber.QueuedNumber });
                        }
                        else
                        {
                            this._web.InvokeScript("ShowMessageBox", "Sorry all timeslots are fully booked!");
                        }
                    }
                }
                else
                {
                    var eventCenter = Trinity.Common.Common.EventCenter.Default;
                    eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Name = EventNames.ALERT_MESSAGE, Message = "You have already queued!\n Please wait for your turn." });
                }

                ////check queue exist
                //if (!_dalQueue.IsUserAlreadyQueue(userSupervise.UserId, DateTime.Today))
                //{

                //    if (appointment.Timeslot_ID != null)
                //    {
                //        queueNumber = _dalQueue.InsertQueueNumber(appointment.ID, appointment.UserId, EnumStations.SSK);

                //        var eventCenter = Trinity.Common.Common.EventCenter.Default;
                //        eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Name = EventNames.ALERT_MESSAGE, Message = "Your queue number is:" + queueNumber.QueuedNumber });
                //    }
                //    else
                //    {
                //        var eventCenter = Trinity.Common.Common.EventCenter.Default;
                //        eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Name = EventNames.ALERT_MESSAGE, Message = "You have not selected the timeslot!\n Please go to Book Appointment page to select a timeslot." });
                //        //BookAppointment();
                //    }

                //    //RaiseOnShowMessageEvent(new ShowMessageEventArgs("Your queue number is: " + queueNumber.QueuedNumber, "Queue Number", MessageBoxButtons.OK, MessageBoxIcon.Information));
                //}
                //else
                //{
                //    var eventCenter = Trinity.Common.Common.EventCenter.Default;
                //    eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Name = EventNames.ALERT_MESSAGE, Message = "You have already queued!\n Please wait for your turn." });
                //}
                ////var model = _dalQueue.GetAllQueueNumberByDate(DateTime.Today).Select(d => new Trinity.BE.Queue()
                ////{
                ////    Status = d.Status,
                ////    QueueNumber = d.QueuedNumber
                ////}).ToArray();
            }
        }


        public void SaveReasonForQueue(/*string data,*/ string reason, string selectedID)
        {

            //send message to case office if no support document
            if (reason == "No Supporting Document")
            {
                APIUtils.SignalR.SendAllDutyOfficer(null, "Supervisee get queue without supporting document", "Please check the Supervisee's information!", NotificationType.Notification);
            }
            var charSeparators = new char[] { ',' };
            var listSplitID = selectedID.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);


            //var listAppointment = JsonConvert.DeserializeObject<List<Appointment>>(data);
            Trinity.BE.Reason reasonModel = JsonConvert.DeserializeObject<Trinity.BE.Reason>(reason);
            if (reasonModel == null)
            {
                reasonModel = new Trinity.BE.Reason()
                {
                    Detail = "",
                    Value = (int)EnumAbsenceReasons.No_Valid_Reason
                };
            }
            //create absence report 
            var dalAbsence = new DAL_AbsenceReporting();


            var dalAppointment = new DAL_Appointments();

            var listSelectedDate = new DAL_Appointments().GetListAppointmentFromListSelectedDate(selectedID);
            //var listSelectedDate = dalAppointment.GetListAppointmentFromSelectedDate(listSplitID);
            foreach (var item in listSelectedDate)
            {
                var result = new DAL_AbsenceReporting().SetInfo(reasonModel);
                //var absenceModel = dalAbsence.SetInfo(reasonModel);
                var absenceModel = result;
                var create = dalAbsence.CreateAbsenceReporting(absenceModel, true);
                if (create)
                {
                    var absenceId = absenceModel.ID;
                    dalAppointment.UpdateAbsenceReason(item.ID, absenceModel.ID);

                    //dalAppointment.UpdateReason(item.ID, absenceModel.ID);
                }

            }

            //send notify to case officer
            Session currentSession = Session.Instance;
            Trinity.BE.User user = (Trinity.BE.User)currentSession[CommonConstants.USER_LOGIN];
            APIUtils.SignalR.SendAllDutyOfficer(user.UserId, user.Name + " has provided absent reason", user.Name + " has provided absent reason.", NotificationType.Notification);
            ReportingForQueueNumber();
            LoadPage("Supervisee.html");
        }

        public void UpdateAbsenceAfterScanDoc()
        {

            Session session = Session.Instance;
            Trinity.BE.AbsenceReporting absenceData = (Trinity.BE.AbsenceReporting)session[CommonConstants.ABSENCE_REPORTING_DATA];
            //get scanned data from session
            var scannedDoc = (byte[])session[CommonConstants.SCANNED_DOCUMENT];
            var listAppointment = (List<Trinity.DAL.DBContext.Appointment>)session[CommonConstants.LIST_APPOINTMENT];
            absenceData.ScannedDocument = scannedDoc;
            var dalAbsence = new DAL_AbsenceReporting();
            var create = dalAbsence.CreateAbsenceReporting(absenceData, true);
            if (create)
            {
                var dalAppointment = new DAL_Appointments();
                foreach (var item in listAppointment)
                {
                    var absenceId = absenceData.ID;
                    dalAppointment.UpdateAbsenceReason(item.ID, absenceData.ID);
                    // dalAppointment.UpdateReason(item.ID, absenceData.ID);
                }
            }

            LoadPage("Supervisee.html");

        }
        public void LogOut()
        {
            // reset session value
            Session session = Session.Instance;
            var user = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];
            if (user != null)
            {
                APIUtils.SignalR.UserLogout(((Trinity.BE.User)session[CommonConstants.USER_LOGIN]).UserId);

                session.IsSmartCardAuthenticated = false;
                session.IsFingerprintAuthenticated = false;
                session[CommonConstants.USER_LOGIN] = null;
                session[CommonConstants.PROFILE_DATA] = null;

                //
                // RaiseLogOutCompletedEvent
                RaiseLogOutCompletedEvent();
            }
        }
    }

    #region Custom Events
    public class NRICEventArgs
    {
        private string _message;
        public NRICEventArgs(string message)
        {
            _message = message;
        }
        public string Message
        {
            get
            {
                return _message;
            }
            set
            {
                _message = value;
            }
        }
    }

    public class ShowMessageEventArgs
    {
        private string _message;
        private string _caption;
        private MessageBoxButtons _button;
        private MessageBoxIcon _icon;

        public ShowMessageEventArgs(string message, string caption, MessageBoxButtons button, MessageBoxIcon icon)
        {
            _message = message;
            _caption = caption;
            _button = button;
            _icon = icon;
        }

        public string Message { get => _message; set => _message = value; }
        public string Caption { get => _caption; set => _caption = value; }
        public MessageBoxButtons Button { get => _button; set => _button = value; }
        public MessageBoxIcon Icon { get => _icon; set => _icon = value; }
    }
    public class NavigateEventArgs
    {
        private NavigatorEnums _navigatorEnum;
        public NavigateEventArgs(NavigatorEnums navigatorEnum)
        {
            _navigatorEnum = navigatorEnum;
        }
        public NavigatorEnums navigatorEnum
        {
            get
            {
                return _navigatorEnum;
            }
            set
            {
                _navigatorEnum = value;
            }
        }
    }
    #endregion
}
