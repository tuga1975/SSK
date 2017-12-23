﻿using Newtonsoft.Json;
using SSA.CodeBehind.Authentication;
using SSA.Common;
using SSA.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Trinity.Common;
using Trinity.DAL;
using Trinity.DAL.DBContext;

namespace SSA
{
    [System.Runtime.InteropServices.ComVisibleAttribute(true)]
    public class JSCallCS
    {
        private WebBrowser _web = null;
        private Type _thisType = null;

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

        public void LoadPage(string file)
        {
            _web.LoadPageHtml(file);
        }

        public void LoadNotications()
        {
            Session currentSession = Session.Instance;
            Trinity.BE.User user = (Trinity.BE.User)currentSession[CommonConstants.USER_LOGIN];
            DAL_Notification dalNotification = new DAL_Notification();
            List<Trinity.BE.Notification> myNotifications = dalNotification.GetMyNotifications(user.UserId, false);
            var model = myNotifications;
            _web.LoadPageHtml("Notication.html", myNotifications);
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

            DAL_Appointments DAL_Appointments = new DAL_Appointments();

            DAL_Environment DAL_Environment = new DAL_Environment();

            Trinity.DAL.DBContext.Appointment appointment = DAL_Appointments.GetMyAppointmentCurrent(user.UserId);
            var selectedTimes = new Trinity.BE.EnvironmentTimeDetail();
            if (appointment != null)
            {
                selectedTimes = DAL_Environment.GetEnvironmentTime(appointment.Date);
                SetSelectedTime(appointment, selectedTimes.Morning);
                SetSelectedTime(appointment, selectedTimes.Afternoon);
                SetSelectedTime(appointment, selectedTimes.Evening);
                this._web.LoadPageHtml("BookAppointment.html", new object[] { appointment, selectedTimes });
            }
            else
            {
                MessageBox.Show("You have no appointment");
            }
        }

        private static void SetSelectedTime(Appointment appointment, List<Trinity.BE.EnvironmentTime> selectedTimes)
        {
            var item = selectedTimes.Where(d => appointment.FromTime != null && d.StartTime == appointment.FromTime.Value && d.EndTime == appointment.ToTime.Value).FirstOrDefault();
            if (item != null)
            {
                item.IsSelected = true;
            }
        }

        public string UpdateTimeAppointment(string IDAppointment, string timeStart, string timeEnd)
        {
            DAL_Appointments DAL_Appointments = new DAL_Appointments();
            DAL_Appointments.UpdateBookTime(IDAppointment, timeStart, timeEnd);

            Trinity.BE.Appointment appointment = DAL_Appointments.GetAppointmentDetails(new Guid(IDAppointment));
            APIUtils.Printer.PrintAppointmentDetails("AppointmentDetailsTemplate.html", appointment);

            return timeStart;
        }

        public void PrintAppointmentDetails(string appointmentId)
        {
            var dalAppointment = new DAL_Appointments();
            Trinity.BE.Appointment appointment = dalAppointment.GetAppointmentDetails(Guid.Parse(appointmentId));
            APIUtils.Printer.PrintAppointmentDetails("AppointmentDetailsTemplate.html", appointment);
        }
        #endregion
        public void LoadProfile()
        {
            try
            {
                Session session = Session.Instance;
                if (session.IsAuthenticated)
                {
                    Trinity.BE.User user = (Trinity.BE.User)session[Constants.CommonConstants.USER_LOGIN];

                    var dalUser = new Trinity.DAL.DAL_User();
                    var dalUserprofile = new Trinity.DAL.DAL_UserProfile();
                    var profileModel = new Trinity.BE.ProfileModel
                    {
                        User = dalUser.GetUserByUserId(user.UserId, true),
                        UserProfile = dalUserprofile.GetUserProfileByUserId(user.UserId, true),
                        Addresses = dalUserprofile.GetAddressByUserId(user.UserId, true)
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
                var dalUser = new Trinity.DAL.DAL_User();
                var dalUserprofile = new Trinity.DAL.DAL_UserProfile();
                if (primaryInfoChange)
                {

                    dalUser.UpdateUser(data.User, data.User.UserId, true);

                    dalUserprofile.UpdateUserProfile(data.UserProfile, data.User.UserId, true);
                    //send notifiy to duty officer
                    APIUtils.SignalR.SendNotificationToDutyOfficer("A supervisee has updated profile.", "Please check Supervisee's information!");
                }
                else
                {
                    dalUserprofile.UpdateUserProfile(data.UserProfile, data.User.UserId, true);
                    //send notifiy to case officer
                    APIUtils.SignalR.SendNotificationToDutyOfficer("A supervisee has updated profile.", "Please check Supervisee's information!");
                }

                //load Supervisee page 
                LoadPage("Supervisee.html");
            }
            catch (Exception)
            {
                LoadPage("Supervisee.html");
            }
        }

        public void LoadScanDocument(string jsonData)
        {
            try
            {
                Session session = Session.Instance;
                session[Constants.CommonConstants.PROFILE_DATA] = jsonData;
                APIUtils.SignalR.SendNotificationToDutyOfficer("Supervisee's information changed!", "Please check the Supervisee's information!");
                LoadPage("Document.html");

            }
            catch (Exception)
            {
                MessageBox.Show("Something wrong happened!");
                LoadProfile();
            }
        }
        public void LoadScanDocumentForAbsence(string jsonData, string reason)
        {
            try
            {
                Session session = Session.Instance;

                var dalAbsence = new DAL_AbsenceReporting();
                var reasonModel = JsonConvert.DeserializeObject<Trinity.BE.Reason>(reason);
                var absenceModel = dalAbsence.SetInfo(reasonModel);
                session[Constants.CommonConstants.ABSENCE_REPORTING_DATA] = absenceModel;

                LoadPage("DocumentFromQueue.html");

            }
            catch (Exception)
            {
                MessageBox.Show("Something wrong happened!");
                LoadProfile();
            }
        }
        public void UpdateProfileAfterScanDoc()
        {
            Session session = Session.Instance;
            var jsonData = session[Constants.CommonConstants.PROFILE_DATA];

            SaveProfile(jsonData.ToString(), true);
        }

        private void actionThread(object pram)
        {

            var data = (object[])pram;
            var method = data[0].ToString();

            MethodInfo theMethod = _thisType.GetMethod(method);
            var dataReturn = theMethod.Invoke(this, (object[])data[2]);
            if (data[1] != null)
            {
                this._web.InvokeScript("callEventCallBack", data[1], JsonConvert.SerializeObject(dataReturn, Formatting.Indented, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
            }
            _web.SetLoading(false);
        }

        public void ClientCallServer(string method, string guidEvent, params object[] pram)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(actionThread), new object[] { method, guidEvent, pram });
        }

        public void SubmitNRIC(string strNRIC)
        {
            NRIC nric = NRIC.GetInstance(_web);
            nric.NRICAuthentication(strNRIC);
        }

        // Reporting for Queue Number
        public void ReportingForQueueNumber()
        {
            Session session = Session.Instance;
            Trinity.BE.User user = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];

            var dalAbsence = new DAL_AbsenceReporting();

            var dalAppointment = new DAL_Appointments();

            int countAbsence = 0;
            countAbsence = dalAppointment.CountMyAbsence(user.UserId);
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
                MessageBox.Show("You have been blocked for 3 or more absences \n Please report to the Duty Officer");

                //for testing purpose
                //notify to officer
                APIUtils.SignalR.SendNotificationToDutyOfficer("Supervisee got blocked for 3 or more absences", "Please check the Supervisee's information!");
                var dalUser = new DAL_User();
                //active the user
                dalUser.ChangeUserStatus(user.UserId, EnumUserStatuses.Active);

                //create absence reporting

                var listAppointment = dalAppointment.GetMyAbsentAppointments(user.UserId);
                session[CommonConstants.LIST_APPOINTMENT] = listAppointment;
                _web.LoadPageHtml("ReasonsForQueue.html", listAppointment);
            }
            else if (countAbsence > 0 && countAbsence < 3)
            {
                var listAppointment = dalAppointment.GetMyAbsentAppointments(user.UserId);

                MessageBox.Show("You have been absent for " + countAbsence + " times.\nPlease provide reasons and the supporting documents.");

                this._web.LoadPageHtml("ReasonsForQueue.html", listAppointment);
            }
        }

        private void GetMyQueueNumber()
        {
            Session session = Session.Instance;
            Trinity.BE.User user = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];

            DAL_Appointments _Appointment = new DAL_Appointments();
            Trinity.DAL.DBContext.Appointment appointment = _Appointment.GetMyAppointmentByDate(user.UserId, DateTime.Today);
            if (appointment == null)
            {
                MessageBox.Show("You have no appointment today");
            }
            else
            {
                var _dalQueue = new DAL_QueueNumber();
                Trinity.DAL.DBContext.QueueNumber QueueNumber = null;
                //check queue exist
                if (!_dalQueue.CheckQueueExistToday(appointment.UserId))
                {
                    QueueNumber = _dalQueue.InsertQueueNumber(appointment.ID, appointment.UserId);
                    RaiseOnShowMessageEvent(new ShowMessageEventArgs("Your queue number is: " + QueueNumber.QueuedNumber, "Queue Number", MessageBoxButtons.OK, MessageBoxIcon.Information));
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
            var listSplitID = selectedID.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries).ToList();


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
            var absenceModel = dalAbsence.SetInfo(reasonModel);
            var create = dalAbsence.CreateAbsenceReporting(absenceModel, true);
            if (create)
            {
                var dalAppointment = new DAL_Appointments();

                var listSelectedDate = dalAppointment.GetListAppointmentFromSelectedDate(listSplitID);

                foreach (var item in listSelectedDate)
                {
                    dalAppointment.UpdateReason(item.ID, absenceModel.ID);
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
                    dalAppointment.UpdateReason(item.ID, absenceData.ID);
                }
            }

            LoadPage("Supervisee.html");

        }
        public void LogOut()
        {
            // reset session value
            Session session = Session.Instance;
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