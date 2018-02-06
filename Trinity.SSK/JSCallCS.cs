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


        public void LoadNotications()
        {
            
            Session currentSession = Session.Instance;
            Trinity.BE.User user = (Trinity.BE.User)currentSession[CommonConstants.USER_LOGIN];

            DAL_Notification dalNotification = new DAL_Notification();

            List<Trinity.BE.Notification> myNotifications = CallCentralized.Get<List<Trinity.BE.Notification>>("Notification", "GetByUserId", "userId="+user.UserId);

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
            var dalNotify = new DAL_Notification();
            var content = dalNotify.GetNotificationContentById(Guid.Parse(notificationId), false);
            APIUtils.TextToSpeech.Speak(content);
        }

        #region BookAppointment
        public void BookAppointment()
        {

            Session session = Session.Instance;
            Trinity.BE.User user = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];

            
            Trinity.DAL.DBContext.Appointment appointment;
            Trinity.DAL.DBContext.Appointment nearestAppointment;
            DateTime today = DateTime.Now.Date;
            var selectedTimes = new Trinity.BE.WorkingTimeshift();

            appointment = CallCentralized.Get<Trinity.DAL.DBContext.Appointment>(EnumAPIParam.Appointment, EnumAPIParam.GetByToday, "userId=" + user.UserId);

            if (appointment != null)
            {

                selectedTimes = SetSelectedTimes( appointment);

            }
            else
            {
                nearestAppointment = CallCentralized.Get<Trinity.DAL.DBContext.Appointment>(EnumAPIParam.Appointment, EnumAPIParam.GetNearest, "userId=" + user.UserId);
                //nearestAppointment = DAL_Appointments.GetNearestAppointment(user.UserId);
                if (nearestAppointment != null)
                {
                    selectedTimes = SetSelectedTimes( nearestAppointment);
                }
                else
                {
                    var eventCenter = Trinity.Common.Common.EventCenter.Default;
                    eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Name = EventNames.ALERT_MESSAGE, Message = "You have no appointment" });
                }

            }

        }

        private Trinity.BE.WorkingTimeshift SetSelectedTimes( Appointment appointment)
        {
            var date = appointment.Date;
            
            Trinity.BE.WorkingTimeshift selectedTimes = CallCentralized.Get<Trinity.BE.WorkingTimeshift>("Setting", "GetAppointmentTime", "date=" + date.ToString());
            SetSelectedTime(appointment, selectedTimes.Morning);
            SetSelectedTime(appointment, selectedTimes.Afternoon);
            SetSelectedTime(appointment, selectedTimes.Evening);
            this._web.LoadPageHtml("BookAppointment.html", new object[] { appointment, selectedTimes });
            return selectedTimes;
        }

        private static void SetSelectedTime(Appointment appointment, List<Trinity.BE.WorkingShiftDetails> selectedTimes)
        {
            

            var item = selectedTimes.Where(d => appointment.Timeslot != null && appointment.Timeslot.StartTime != null && d.StartTime == appointment.Timeslot.StartTime.Value && d.EndTime == appointment.Timeslot.EndTime.Value).FirstOrDefault();
            if (item != null)
            {
                item.IsSelected = true;
            }

            if (!string.IsNullOrEmpty(appointment.Timeslot_ID))
            {


                var maxAppPerTimeslot = CallCentralized.Get<int>(EnumAPIParam.Appointment, EnumAPIParam.GetMaximumNumberOfTimeslot, "timeslotId=" + appointment.Timeslot_ID);
                foreach (var selectedItem in selectedTimes)
                {
                    var count = CallCentralized.Get<int>(EnumAPIParam.Appointment, EnumAPIParam.CountByTimeslot, "appointmentId=" + appointment.ID.ToString());
                    if (count >= maxAppPerTimeslot)
                    {
                        selectedItem.IsAvailble = false;
                    }
                }
            }
        }

        public bool UpdateTimeAppointment(string IDAppointment, string timeStart, string timeEnd)
        {
            
            var dbAppointment = CallCentralized.Get<Appointment>(EnumAPIParam.Appointment, EnumAPIParam.GetById, "appointmentId=" + IDAppointment);
            //check exist queue
            var dalQueue = new DAL_QueueNumber();
            if (dbAppointment != null)
            {
                if (!dalQueue.CheckQueueExistToday(dbAppointment.UserId, EnumStations.SSK))
                {
                    //var data = JsonConvert.SerializeObject(new { IDAppointment, timeStart, timeEnd });
                    CallCentralized.Post<Appointment>(EnumAPIParam.Appointment, EnumAPIParam.UpdateBooktime, "appointmentId=" + IDAppointment, "timeStart=" + timeStart, "timeEnd=" + timeEnd);
                    // dalAppointments.UpdateBookTime(IDAppointment, timeStart, timeEnd);

                    Trinity.BE.Appointment appointment = CallCentralized.Get<Trinity.BE.Appointment>(EnumAPIParam.Appointment, EnumAPIParam.GetDetailsById, "appointmentId=" + IDAppointment);
                    //Trinity.BE.Appointment appointment = dalAppointments.GetAppointmentDetails(new Guid(IDAppointment));

                    APIUtils.Printer.PrintAppointmentDetails("AppointmentDetailsTemplate.html", appointment);
                    FormQueueNumber f = FormQueueNumber.GetInstance();
                    f.RefreshQueueNumbers();
                    return true;
                }
                else
                {
                    return false;
                }

            }
            return false;
        }

        public void PrintAppointmentDetails(string appointmentId)
        {
            
            var dalAppointment = new DAL_Appointments();
            Trinity.BE.Appointment appointment = CallCentralized.Get<Trinity.BE.Appointment>(EnumAPIParam.Appointment, EnumAPIParam.GetDetailsById, "appointmentId=" + appointmentId);
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
                        User = CallCentralized.Get<Trinity.BE.User>("User", "GetUserByUserId", "userId=" + user.UserId),
                        UserProfile = CallCentralized.Get<Trinity.BE.UserProfile>("User", "GetUserProfileByUserId", "userId=" + user.UserId),
                        Addresses = CallCentralized.Get<Trinity.BE.Address>("User", "GetAddressByUserId", "userId=" + user.UserId, "isOther=" + false),
                        OtherAddress = CallCentralized.Get<Trinity.BE.Address>("User", "GetAddressByUserId", "userId=" + user.UserId,"isOther="+true),
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

        public void SaveProfile(string param, bool primaryInfoChange)
        {
            try
            {
                
                var rawData = JsonConvert.DeserializeObject<Trinity.BE.ProfileRawMData>(param);
                var data = new Trinity.BE.ProfileRawMData().ToProfileModel(rawData);
               
                if (primaryInfoChange)
                {
                    var updateUserResult = CallCentralized.Post<bool>("User", "UpdateUser", data.User);
                    // dalUser.UpdateUser(data.User, data.User.UserId, true);
                    var userProfileModel = data.UserProfile;
                    userProfileModel.UserId = data.User.UserId;
                    var updateUProfileResult = CallCentralized.Post<bool>("User", "UpdateUserProfile", userProfileModel);
                    // dalUserprofile.UpdateUserProfile(data.UserProfile,data.User.UserId , true);
                    //send notifiy to duty officer
                    APIUtils.SignalR.SendNotificationToDutyOfficer("A supervisee has updated profile.", "Please check Supervisee's information!");
                }
                else
                {
                    var userProfileModel = data.UserProfile;
                    userProfileModel.UserId = data.User.UserId;
                    var updateUProfileResult = CallCentralized.Post<bool>("User", "UpdateUserProfile", userProfileModel);
                   // dalUserprofile.UpdateUserProfile(data.UserProfile, data.User.UserId, true);
                    //send notifiy to case officer
                    APIUtils.SignalR.SendNotificationToDutyOfficer("A supervisee has updated profile.", "Please check Supervisee's information!");
                }

                //load Supervisee page 
                LoadPage("Supervisee.html");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("JSCallCS.SaveProfile exception: " + ex.ToString());
                LoadPage("Supervisee.html");
            }
        }

        public void LoadScanDocument(string jsonData)
        {
            try
            {
                Session session = Session.Instance;
                session[CommonConstants.PROFILE_DATA] = jsonData;
                APIUtils.SignalR.SendNotificationToDutyOfficer("Supervisee's information changed!", "Please check the Supervisee's information!");
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
                var result = CallCentralized.Post<Trinity.BE.AbsenceReporting>("AbsenceReport", "SetReasonInfo", reasonModel);
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
            return CallCentralized.Get<int>(EnumAPIParam.Appointment, EnumAPIParam.CountAbsenceByUserId, "userId=" + user.UserId);
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

            countAbsence = CallCentralized.Get<int>(EnumAPIParam.Appointment, EnumAPIParam.CountAbsenceByUserId, "userId=" + user.UserId);
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
                APIUtils.SignalR.SendNotificationToDutyOfficer("Supervisee got blocked for 3 or more absences", "Please check the Supervisee's information!");
                var dalUser = new DAL_User();
                //active the user
                //dalUser.ChangeUserStatus(user.UserId, EnumUserStatuses.New);

                //create absence reporting
                var listAppointment = CallCentralized.Get<int>(EnumAPIParam.Appointment, EnumAPIParam.GetAbsenceByUserId, "userId=" + user.UserId);
                // var listAppointment = dalAppointment.GetMyAbsentAppointments(user.UserId);
                session[CommonConstants.LIST_APPOINTMENT] = listAppointment;
                _web.LoadPageHtml("ReasonsForQueue.html", listAppointment);
            }
            else if (countAbsence > 0 && countAbsence < 3)
            {
                
                var result = CallCentralized.Get<List<Trinity.BE.Appointment>>(EnumAPIParam.Appointment, EnumAPIParam.GetAbsenceByUserId, "userId=" + user.UserId);
                var listAppointment = result;

                var eventCenter = Trinity.Common.Common.EventCenter.Default;
                eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Name = EventNames.ABSENCE_LESS_THAN_3, Message = "You have been absent for " + countAbsence + " times.\nPlease provide reasons and the supporting documents." });

                this._web.LoadPageHtml("ReasonsForQueue.html", listAppointment);
            }
        }

        private void GetMyQueueNumber()
        {
            
            Session session = Session.Instance;
            Trinity.BE.User user = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];

            DAL_Appointments _Appointment = new DAL_Appointments();
            Trinity.DAL.DBContext.Appointment appointment = CallCentralized.Get<Appointment>(EnumAPIParam.Appointment, EnumAPIParam.GetByUserIdAndDate, "UserId=" + user.UserId, "date=" + DateTime.Today.ToString());
            //Trinity.DAL.DBContext.Appointment appointment = _Appointment.GetMyAppointmentByDate(user.UserId, DateTime.Today);
            if (appointment == null)
            {
                var eventCenter = Trinity.Common.Common.EventCenter.Default;
                eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Name = EventNames.ALERT_MESSAGE, Message = "You have no appointment today" });
            }
            else
            {
                var _dalQueue = new DAL_QueueNumber();
                Trinity.DAL.DBContext.Queue queueNumber = null;
                //check queue exist
                if (!_dalQueue.CheckQueueExistToday(appointment.UserId, EnumStations.SSK))
                {
                    if (appointment.Timeslot_ID != null)
                    {
                        queueNumber = _dalQueue.InsertQueueNumber(appointment.ID, appointment.UserId, EnumStations.SSK);

                        var eventCenter = Trinity.Common.Common.EventCenter.Default;
                        eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Name = EventNames.ALERT_MESSAGE, Message = "Your queue number is:" + queueNumber.QueuedNumber });
                    }
                    else
                    {
                        var eventCenter = Trinity.Common.Common.EventCenter.Default;
                        eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Name = EventNames.ALERT_MESSAGE, Message = "You have not selected the timeslot!\n Please go to Book Appointment page to select a timeslot." });
                        //BookAppointment();
                    }

                    //RaiseOnShowMessageEvent(new ShowMessageEventArgs("Your queue number is: " + queueNumber.QueuedNumber, "Queue Number", MessageBoxButtons.OK, MessageBoxIcon.Information));
                }
                else
                {
                    var eventCenter = Trinity.Common.Common.EventCenter.Default;
                    eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Name = EventNames.ALERT_MESSAGE, Message = "You have already queued!\n Please wait for your turn." });
                }
                //var model = _dalQueue.GetAllQueueNumberByDate(DateTime.Today).Select(d => new Trinity.BE.Queue()
                //{
                //    Status = d.Status,
                //    QueueNumber = d.QueuedNumber
                //}).ToArray();
                FormQueueNumber f = FormQueueNumber.GetInstance();
                f.RefreshQueueNumbers();
            }
        }


        public void SaveReasonForQueue(/*string data,*/ string reason, string selectedID)
        {
            
            //send message to case office if no support document
            if (reason == "No Supporting Document")
            {
                APIUtils.SignalR.SendNotificationToDutyOfficer("Supervisee get queue without supporting document", "Please check the Supervisee's information!");
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

            var listSelectedDate = CallCentralized.Get<List<Appointment>>(EnumAPIParam.Appointment, EnumAPIParam.GetListFromSelectedDate, "listAppointmentId=" + selectedID);
            //var listSelectedDate = dalAppointment.GetListAppointmentFromSelectedDate(listSplitID);
            foreach (var item in listSelectedDate)
            {
                var result = CallCentralized.Post<Trinity.BE.AbsenceReporting>("AbsenceReport", "SetReasonInfo", reasonModel);
                //var absenceModel = dalAbsence.SetInfo(reasonModel);
                var absenceModel = result;
                var create = dalAbsence.CreateAbsenceReporting(absenceModel, true);
                if (create)
                {
                    var absenceId = absenceModel.ID;
                    CallCentralized.Post<Appointment>(EnumAPIParam.Appointment, EnumAPIParam.UpdateReason, "appointmentId=" + item.ID, "absenceId=" + absenceId);
                    //dalAppointment.UpdateReason(item.ID, absenceModel.ID);
                }

            }

            //send notify to case officer
            Session currentSession = Session.Instance;
            Trinity.BE.User user = (Trinity.BE.User)currentSession[CommonConstants.USER_LOGIN];
            APIUtils.SignalR.SendNotificationToDutyOfficer(user.Name + " has provided absent reason", user.Name + " has provided absent reason.");
            ReportingForQueueNumber();
            LoadPage("Supervisee.html");
        }

        public void UpdateAbsenceAfterScanDoc()
        {
            
            Session session = Session.Instance;
            Trinity.BE.AbsenceReporting absenceData = (Trinity.BE.AbsenceReporting)session[CommonConstants.ABSENCE_REPORTING_DATA];
            //get scanned data from session
            var scannedDoc = (byte[])session[CommonConstants.SCANNED_DOCUMENT];
            var listAppointment = (List<Appointment>)session[CommonConstants.LIST_APPOINTMENT];
            absenceData.ScannedDocument = scannedDoc;
            var dalAbsence = new DAL_AbsenceReporting();
            var create = dalAbsence.CreateAbsenceReporting(absenceData, true);
            if (create)
            {
                var dalAppointment = new DAL_Appointments();
                foreach (var item in listAppointment)
                {
                    var absenceId = absenceData.ID;
                    CallCentralized.Post<Appointment>(EnumAPIParam.Appointment, EnumAPIParam.UpdateReason, "appointmentId=" + item.ID, "absenceId=" + absenceId);
                    // dalAppointment.UpdateReason(item.ID, absenceData.ID);
                }
            }

            LoadPage("Supervisee.html");

        }
        public void LogOut()
        {
            // reset session value
            Session session = Session.Instance;

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
