using Newtonsoft.Json;
using ARK.CodeBehind.Authentication;
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
using Trinity.Device.Util;
using Trinity.Util;
using Trinity.Common.Common;
using Trinity.Common.Utils;

namespace ARK
{
    [System.Runtime.InteropServices.ComVisibleAttribute(true)]
    public class JSCallCS : JSCallCSBase
    {
        public event EventHandler<NRICEventArgs> OnNRICFailed;
        public event EventHandler<ShowMessageEventArgs> OnShowMessage;
        public event Action OnLogOutCompleted;
        private Main main;
        public JSCallCS(WebBrowser web, Main main)
        {
            this._web = web;
            this.main = main;
            _thisType = this.GetType();
        }
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

            List<Trinity.BE.Notification> myNotifications = dalNotification.GetAllNotifications(user.UserId);

            var model = myNotifications;
            _web.LoadPageHtml("Notifications.html", myNotifications);
        }
        public string GetUserName(string userId)
        {
            return new DAL_User().GetUserById(userId).Name;
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

        public void StopSpeakNotification()
        {
            APIUtils.TextToSpeech.Stop();
        }

        #region BookAppointment
        public void BookAppointment()
        {
            Session session = Session.Instance;
            var eventCenter = Trinity.Common.Common.EventCenter.Default;
            Trinity.BE.User currentUser = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];
            Trinity.BE.User supervisee = null;
            if (currentUser.Role == EnumUserRoles.DutyOfficer)
            {
                supervisee = (Trinity.BE.User)session[CommonConstants.SUPERVISEE];
            }
            else
            {
                supervisee = currentUser;
            }



            // check supervisee have appoitment
            Trinity.DAL.DBContext.Appointment appointment = new DAL_Appointments().GetNextAppointment(supervisee.UserId);
            if (appointment != null && (appointment.Status == EnumAppointmentStatuses.Reported || appointment.Status == EnumAppointmentStatuses.Absent))
            {
                appointment = new DAL_Appointments().GetNextAppointmentByStatus(supervisee.UserId, EnumAppointmentStatuses.Pending);
            }

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



            // set workingTimeshift
            Trinity.BE.WorkingTimeshift workingTimeshift = new Trinity.BE.WorkingTimeshift();
            workingTimeshift.Morning = GetWorkingTimeshift(timeslots, appointment.Timeslot_ID, EnumTimeshift.Morning);
            workingTimeshift.Afternoon = GetWorkingTimeshift(timeslots, appointment.Timeslot_ID, EnumTimeshift.Afternoon);
            workingTimeshift.Evening = GetWorkingTimeshift(timeslots, appointment.Timeslot_ID, EnumTimeshift.Evening);

            var appointmentBE = new Trinity.BE.Appointment()
            {
                ID = appointment.ID.ToString(),
                AbsenceReporting_ID = appointment.AbsenceReporting_ID,
                Timeslot_ID = appointment.Timeslot_ID,
                UserId = appointment.UserId,
                AppointmentDate = appointment.Date,
                ChangedCount = appointment.ChangedCount,
                Status = appointment.Status,
                ReportTime = appointment.ReportTime,
                StartTime = appointment.Timeslot != null ? appointment.Timeslot.StartTime : null,
                EndTime = appointment.Timeslot != null ? appointment.Timeslot.EndTime : null
            };

            // redirect
            this._web.LoadPageHtml("BookAppointment.html", new object[] { appointmentBE, workingTimeshift });
        }

        public string GetNextAppointmentDate()
        {
            Session session = Session.Instance;
            Trinity.BE.User user = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];
            if (user != null)
            {
                var _appointment = new DAL_Appointments().GetNextAppointment(user.UserId);
                if (_appointment != null)
                {
                    return _appointment.Date.ToLongDateString();
                }
            }
            return null;

        }

        public bool CheckBookingTime(string timeslotId, string date)
        {
            return new DAL_Timeslots().CheckTimeslot(timeslotId, Convert.ToDateTime(date));
        }

        private List<WorkingShiftDetails> GetWorkingTimeshift(List<Timeslot> timeslots, string selected_Timeslot_ID, string timeshift)
        {
            List<WorkingShiftDetails> returnValue = timeslots.Where(item => item.Category == timeshift)
                    .Select(item => new WorkingShiftDetails()
                    {
                        Timeslot_ID = item.Timeslot_ID,
                        StartTime = item.StartTime.Value,
                        EndTime = item.EndTime.Value,
                        IsAvailble = item.MaximumSupervisee.HasValue ? new DAL_Appointments().CountApptmtHasUseByTimeslot(item.Timeslot_ID) < item.MaximumSupervisee.Value : false,
                        IsSelected = selected_Timeslot_ID == item.Timeslot_ID,
                        Category = item.Category
                    }).OrderBy(item => item.StartTime).ToList();

            return returnValue;
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

        public void UpdateTimeAppointment(string appointment_ID, string timeslot_ID)
        {
            if (new DAL_QueueNumber().IsInQueue(appointment_ID, EnumStation.ARK))
            {
                this._web.ShowMessage("You already registered a queue number!");
            }
            else
            {
                Session session = Session.Instance;
                Trinity.BE.User currentUser = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];
                Trinity.BE.User supervisee = null;
                if (currentUser.Role == EnumUserRoles.DutyOfficer)
                {
                    supervisee = (Trinity.BE.User)session[CommonConstants.SUPERVISEE];
                }
                else
                {
                    supervisee = currentUser;
                }


                bool updateResult = new DAL_Appointments().UpdateTimeslot_ID(appointment_ID, timeslot_ID);

                if (updateResult)
                {
                    Trinity.SignalR.Client.Instance.AppointmentBooked(supervisee.UserId, appointment_ID, timeslot_ID);
                    AppointmentDetails appointmentdetails = new DAL_Appointments().GetAppointmentDetails(appointment_ID);
                    if (BarcodePrinterUtil.Instance.GetDeviceStatus(EnumDeviceNames.ReceiptPrinter).Contains(EnumDeviceStatus.Connected))
                    {
                        ReceiptPrinterUtil.Instance.PrintAppointmentDetails(appointmentdetails);
                    }
                    else
                    {
                        Trinity.SignalR.Client.Instance.SendToAppDutyOfficers(supervisee.UserId, "Lost connection to printer", "Lost connection to printer", EnumNotificationTypes.Error);
                        _web.ShowMessage("Lost connection to printer");
                    }
                    LoadPageSupervisee();
                }
                else
                {
                    this._web.ShowMessage("Book Appointment failed.<br/>Please try again!");
                }
            }
        }

        public void PrintAppointmentDetails(string appointmentId)
        {

            AppointmentDetails appointmentdetails = new DAL_Appointments().GetAppointmentDetails(appointmentId);
            ReceiptPrinterUtil.Instance.PrintAppointmentDetails(appointmentdetails);
        }
        #endregion

        private object[] data_old_update_profile = null;
        public void LoadProfile()
        {
            Session session = Session.Instance;
            Trinity.BE.User user = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];
            var dataReturn = new
            {
                Membership_Users = user,
                User_Profiles = new DAL_UserProfile().GetProfile(user.UserId),
                Primary_Addresses = new DAL_UserProfile().GetAddByUserId(user.UserId, true),
                Alternate_Addresses = new DAL_UserProfile().GetAddByUserId(user.UserId, true)
            };
            data_old_update_profile = new object[] { dataReturn.User_Profiles, dataReturn.Alternate_Addresses };
            _web.LoadPageHtml("Profile.html", dataReturn);
        }

        public void SaveProfile(string param, string dataChangeAll, string arrayDocumentScan, bool isSendNotiIfDontScanDocument)
        {
            Session session = Session.Instance;
            Trinity.BE.User user = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];
            var data = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(param);
            List<string> arrayScan = JsonConvert.DeserializeObject<List<string>>(arrayDocumentScan);

            Trinity.BE.UserProfile User_Profiles_New = null;
            Trinity.BE.Address Alternate_Addresses_New = null;
            Nullable<Guid> IDDocuemnt = null;
            if (new DAL_UserProfile().ARKUpdateProfile(user.UserId, data, arrayScan, out User_Profiles_New, out Alternate_Addresses_New, out IDDocuemnt))
            {
                new DAL_UpdateProfile_Requests().CreateRequest(
                    (Trinity.BE.UserProfile)data_old_update_profile[0],
                    (Trinity.BE.Address)data_old_update_profile[1],
                    User_Profiles_New,
                    Alternate_Addresses_New,
                    IDDocuemnt,
                    isSendNotiIfDontScanDocument
                );
                if (isSendNotiIfDontScanDocument)
                {
                    Trinity.SignalR.Client.Instance.SendToAppDutyOfficers(user.UserId, "Supervisee " + user.Name + " has updated profile.", "Please check Supervisee " + user.Name + "'s information!", EnumNotificationTypes.Notification);
                }
                else
                {
                    Trinity.SignalR.Client.Instance.SendToAppDutyOfficers(user.UserId, "Supervisee " + user.Name + " has updated profile and scan documents.", "Please check Supervisee " + user.Name + "'s information!", EnumNotificationTypes.Notification);
                }
                LoadPageSupervisee();
            }
        }

        public void LoadScanDocument(string jsonData)
        {
            try
            {
                Session session = Session.Instance;
                session[CommonConstants.PROFILE_DATA] = jsonData;
                var user = (Trinity.BE.User)Session.Instance[CommonConstants.USER_LOGIN];
                Trinity.SignalR.Client.Instance.SendToAppDutyOfficers(user.UserId, "Supervisee's information changed!", "Please check the Supervisee " + user.Name + "'s information!", EnumNotificationTypes.Notification);
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

            //SaveProfile(jsonData.ToString(), true);
        }

        public void ManualLogin()
        {
            _web.LoadPageHtml("Authentication/ManualLogin.html");

            // Enable scanner
            if (BarcodeScannerUtil.Instance.GetDeviceStatus().Contains(EnumDeviceStatus.Connected))
            {
                System.Threading.Tasks.Task.Factory.StartNew(() => BarcodeScannerUtil.Instance.StartScanning(BarcodeScannerCallback));
            }
        }

        public void RestartBarcodeScanning()
        {
            // Enable scanner
            if (BarcodeScannerUtil.Instance.GetDeviceStatus().Contains(EnumDeviceStatus.Connected))
            {
                System.Threading.Tasks.Task.Factory.StartNew(() => BarcodeScannerUtil.Instance.StartScanning(BarcodeScannerCallback));
            }
        }

        private void BarcodeScannerCallback(string value, string error)
        {
            try
            {
                if (string.IsNullOrEmpty(error))
                {
                    // Fill value to the textbox
                    //CSCallJS.ShowMessage(_web, value);
                    CSCallJS.InvokeScript(_web, "updateLoginNRICTextValue", value.Trim());
                    //CSCallJS.UpdateLoginNRICTextValue(_web, value);
                }
                else
                {
                    LogManager.Error("BarcodeScannerCallback ERROR: " + error);
                    //CSCallJS.ShowMessageAsync(_web, "Manual Login ERROR", error);
                }
            }
            finally
            {
                BarcodeScannerUtil.Instance.Disconnect();
            }
        }

        public void ProcessManualLogin(string username, string password)
        {
            try
            {
                BarcodeScannerUtil.Instance.Disconnect();

                EventCenter eventCenter = EventCenter.Default;

                //UserManager<ApplicationUser> userManager = ApplicationIdentityManager.GetUserManager();
                //ApplicationUser appUser = userManager.Find(username, password);
                var dalUser = new DAL_User();
                ApplicationUser appUser = dalUser.Login(username, password);
                if (appUser != null)
                {
                    // Authenticated successfully
                    // Check if the current user is an Duty Officer or not
                    if (dalUser.IsInRole(appUser.Id, EnumUserRoles.DutyOfficer))
                    {
                        // Authorized successfully
                        Trinity.BE.User user = new Trinity.BE.User()
                        {
                            RightThumbFingerprint = appUser.RightThumbFingerprint,
                            LeftThumbFingerprint = appUser.LeftThumbFingerprint,
                            IsFirstAttempt = appUser.IsFirstAttempt,
                            Name = appUser.Name,
                            NRIC = appUser.NRIC,
                            Role = EnumUserRoles.DutyOfficer,
                            SmartCardId = appUser.SmartCardId,
                            Status = appUser.Status,
                            UserId = appUser.Id
                        };
                        Session session = Session.Instance;
                        session.IsUserNamePasswordAuthenticated = true;
                        session.Role = EnumUserRoles.DutyOfficer;
                        session[CommonConstants.USER_LOGIN] = user;

                        eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Code = 0, Name = EventNames.LOGIN_SUCCEEDED });

                        // Set application status is busy
                        ApplicationStatusManager.Instance.IsBusy = true;
                    }
                    else
                    {
                        eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Code = -2, Name = EventNames.LOGIN_FAILED, Message = "You do not have permission to access this page." });
                    }
                }
                else
                {
                    // Enable scanner
                    if (BarcodeScannerUtil.Instance.GetDeviceStatus().Contains(EnumDeviceStatus.Connected))
                    {
                        System.Threading.Tasks.Task.Factory.StartNew(() => BarcodeScannerUtil.Instance.StartScanning(BarcodeScannerCallback));
                    }

                    eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Code = -1, Name = EventNames.LOGIN_FAILED, Message = "Your username or password is incorrect." });
                }
            }
            finally
            {
                BarcodeScannerUtil.Instance.Disconnect();
            }
        }

        public void SubmitNRIC(string strNRIC)
        {
            NRIC nric = NRIC.GetInstance(_web);
            nric.NRICAuthentication(strNRIC);
        }

        public int GetMyAbsencesCount()
        {
            Session session = Session.Instance;
            Trinity.BE.User user = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];

            Trinity.BE.User supervisee = null;
            // Check if the current user is a duty officcer
            if (user.Role == EnumUserRoles.DutyOfficer)
            {
                supervisee = (Trinity.BE.User)session[CommonConstants.SUPERVISEE];
            }
            if (supervisee != null)
            {
                return new DAL_Appointments().CountAbsenceReport(supervisee.UserId);
            }
            else
            {
                return 0;
            }
        }

        // Reporting for Queue Number
        public void ReportingForQueueNumber()
        {
            // Get current user
            Session session = Session.Instance;
            Trinity.BE.User currentUser = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];
            Trinity.BE.User supervisee = currentUser;
            // Check if the current user is a duty officcer 
            if (currentUser.Role == EnumUserRoles.DutyOfficer)
            {
                supervisee = (Trinity.BE.User)session[CommonConstants.SUPERVISEE];
            }
            else
            {
                supervisee = currentUser;
            }
            if (supervisee.Status == EnumUserStatuses.Blocked)
            {

            }
            int absenceCount = 0;

            if (supervisee != null)
            {
                absenceCount = new DAL_Appointments().CountAbsenceReport(supervisee.UserId);
            }

            if (absenceCount == 0)
            {
                GetMyQueueNumber();
            }
            //else if (absenceCount >= 3 || supervisee.Status==EnumUserStatuses.Blocked)
            //{
            //    var eventCenter = Trinity.Common.Common.EventCenter.Default;
            //    //eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Name = EventNames.ABSENCE_MORE_THAN_3, Message = "You have been blocked for 3 or more absences \n Please report to the Duty Officer" });
            //    this._web.ShowMessage("You have been blocked for 3 or more absences <br/> Please report to the Duty Officer");
            //    //for testing purpose
            //    //notify to officer
            //    Trinity.SignalR.Client.Instance.SendToAllDutyOfficers(supervisee.UserId, "Supervisee got blocked for 3 or more absences", "Please check the Supervisee's information!", EnumNotificationTypes.Caution);
            //    //var dalUser = new DAL_User();

            //    //// Create absence reporting
            //    //var listAppointment = new DAL_Appointments().GetAbsentAppointments(supervisee.UserId);
            //    //session[CommonConstants.LIST_APPOINTMENT] = listAppointment;
            //    //_web.LoadPageHtml("ReasonsForQueue.html", listAppointment.Select(d => new
            //    //{
            //    //    ID = d.ID,
            //    //    GetDateTxt = d.GetDateTxt
            //    //}));
            //}
            else
            {
                var listAppointment = new DAL_Appointments().GetAbsentAppointments(supervisee.UserId);

                this._web.ShowMessage("You have been absent for " + absenceCount + " time(s).<br/>Please provide reason(s) and supporting document(s).");
                _web.LoadPageHtml("ReasonsForQueue.html", listAppointment.Select(d => new
                {
                    ID = d.ID,
                    GetDateTxt = d.GetDateTxt
                }));
            }
        }

        private void GetMyQueueNumber()
        {
            // Get current user
            Session session = Session.Instance;
            Trinity.BE.User currentUser = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];
            Trinity.BE.User supervisee = null;
            if (currentUser.Role == EnumUserRoles.DutyOfficer)
            {
                supervisee = (Trinity.BE.User)session[CommonConstants.SUPERVISEE];
            }
            else
            {
                supervisee = currentUser;
            }
            DAL_Appointments _Appointment = new DAL_Appointments();
            Trinity.DAL.DBContext.Appointment appointment = new DAL_Appointments().GetAppointmentByDate(supervisee.UserId, DateTime.Today);
            var _dalQueue = new DAL_QueueNumber();
            Trinity.DAL.DBContext.Queue queueNumber = null;

            if (!_dalQueue.IsUserAlreadyQueue(supervisee.UserId, DateTime.Today))
            {
                if (appointment != null)
                {
                    if (string.IsNullOrEmpty(appointment.Timeslot_ID) && currentUser.Role == EnumUserRoles.Supervisee)
                    {
                        _web.ShowMessage("You have not selected a timeslot!");
                        BookAppointment();
                    }
                    else
                    {
                        queueNumber = _dalQueue.InsertQueueNumber(appointment.ID, appointment.UserId, EnumStation.ARK, currentUser.UserId);
                        var dalLabel = new DAL_Labels();
                        string MarkingNumber = dalLabel.GetMarkingNumber(supervisee.UserId, DateTime.Today);
                        if (string.IsNullOrEmpty(MarkingNumber))
                            MarkingNumber = new DAL_SettingSystem().GenerateMarkingNumber();
                        dalLabel.Insert(new Trinity.BE.Label
                        {
                            UserId = supervisee.UserId,
                            Label_Type = EnumLabelType.TT,
                            CompanyName = CommonConstants.COMPANY_NAME,
                            MarkingNo = MarkingNumber,
                            NRIC = supervisee.NRIC,
                            Name = supervisee.Name,
                            LastStation = EnumStation.ARK,
                            Queue_ID = queueNumber.Queue_ID
                        });
                        dalLabel.Insert(new Trinity.BE.Label
                        {
                            UserId = supervisee.UserId,
                            Label_Type = EnumLabelType.MUB,
                            CompanyName = CommonConstants.COMPANY_NAME,
                            MarkingNo = MarkingNumber,
                            NRIC = supervisee.NRIC,
                            Name = supervisee.Name,
                            LastStation = EnumStation.ARK,
                            Queue_ID = queueNumber.Queue_ID
                        });


                        Trinity.SignalR.Client.Instance.AppointmentReported(queueNumber.Queue_ID.ToString().Trim(), queueNumber.Appointment_ID.ToString().Trim());
                        Trinity.SignalR.Client.Instance.QueueInserted(queueNumber.Queue_ID.ToString().Trim());
                        APIUtils.FormQueueNumber.RefreshQueueNumbers();
                        this._web.ShowMessage("Your queue number is:" + queueNumber.QueuedNumber);
                    }
                }
                else
                {
                    queueNumber = _dalQueue.InsertQueueNumberFromDO(supervisee.UserId, EnumStation.ARK, currentUser.UserId);
                    var dalLabel = new DAL_Labels();
                    string MarkingNumber = dalLabel.GetMarkingNumber(supervisee.UserId, DateTime.Today);
                    if (string.IsNullOrEmpty(MarkingNumber))
                        MarkingNumber = new DAL_SettingSystem().GenerateMarkingNumber();

                    dalLabel.Insert(new Trinity.BE.Label
                    {
                        UserId = supervisee.UserId,
                        Label_Type = EnumLabelType.TT,
                        CompanyName = CommonConstants.COMPANY_NAME,
                        MarkingNo = MarkingNumber,
                        NRIC = supervisee.NRIC,
                        Name = supervisee.Name,
                        LastStation = EnumStation.ARK,
                        Queue_ID = queueNumber.Queue_ID
                    });
                    dalLabel.Insert(new Trinity.BE.Label
                    {
                        UserId = supervisee.UserId,
                        Label_Type = EnumLabelType.MUB,
                        CompanyName = CommonConstants.COMPANY_NAME,
                        MarkingNo = MarkingNumber,
                        NRIC = supervisee.NRIC,
                        Name = supervisee.Name,
                        LastStation = EnumStation.ARK,
                        Queue_ID = queueNumber.Queue_ID
                    });
                    Trinity.SignalR.Client.Instance.QueueInserted(queueNumber.Queue_ID.ToString().Trim());
                    APIUtils.FormQueueNumber.RefreshQueueNumbers();
                    this._web.ShowMessage("Your queue number is:" + queueNumber.QueuedNumber);
                }

                //if (appointment != null && string.IsNullOrEmpty(appointment.Timeslot_ID))
                //{
                //    var eventCenter = Trinity.Common.Common.EventCenter.Default;
                //    eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Name = EventNames.ALERT_MESSAGE, Message = "You have not selected the timeslot!\n Please go to Book Appointment page to select a timeslot." });
                //}
                //else if (appointment != null && !string.IsNullOrEmpty(appointment.Timeslot_ID))
                //{
                //    queueNumber = _dalQueue.InsertQueueNumber(appointment.ID, appointment.UserId, EnumStation.ARK, currentUser.UserId);
                //    if (queueNumber != null)
                //    {
                //        Trinity.SignalR.Client.Instance.AppointmentBookedOrReported(appointment.ID.ToString().Trim(), EnumAppointmentStatuses.Reported);
                //        Trinity.SignalR.Client.Instance.QueueInserted(queueNumber.Queue_ID.ToString().Trim());
                //        APIUtils.FormQueueNumber.RefreshQueueNumbers();
                //        var eventCenter = Trinity.Common.Common.EventCenter.Default;
                //        eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Name = EventNames.ALERT_MESSAGE, Message = "Your queue number is:" + queueNumber.QueuedNumber });
                //    }
                //    else
                //    {
                //        this._web.ShowMessage("Sorry all timeslots are fully booked!");
                //    }

                //}
                //else
                //{

                //}
            }
            else
            {
                this._web.ShowMessage("You already registered a queue number!\n Please wait for your turn.");
            }
            //if (appointment == null && currentUser.Role == EnumUserRoles.Supervisee)
            //{
            //    var eventCenter = Trinity.Common.Common.EventCenter.Default;
            //    eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Name = EventNames.ALERT_MESSAGE, Message = "You have no appointment today" });
            //}
            //else
            //{

            //}
        }

        public void SaveReasonForQueue(string dataTxt, string arrayDocumentScan)
        {
            List<string> arrayScan = JsonConvert.DeserializeObject<List<string>>(arrayDocumentScan);
            if (arrayScan.Count == 0)
            {
                _SaveReasonForQueue(dataTxt, null);
            }
            else
            {
                Guid IDDocuemnt = new DAL_UploadedDocuments().Insert(arrayScan, ((Trinity.BE.User)Session.Instance[CommonConstants.USER_LOGIN]).UserId);
                _SaveReasonForQueue(dataTxt, IDDocuemnt);
            }

        }

        private void _SaveReasonForQueue(string dataTxt, Nullable<Guid> IdDocument)
        {
            List<Dictionary<string, string>> data = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(dataTxt);
            new DAL_AbsenceReporting().InsertAbsentReason(data, IdDocument);
            LoadPageSupervisee();
        }

        //public void SaveReasonForQueue(/*string data,*/ string reason, string selectedID)
        //{
        //    Session currentSession = Session.Instance;
        //    Trinity.BE.User user = (Trinity.BE.User)currentSession[CommonConstants.USER_LOGIN];
        //    //send message to case office if no support document
        //    if (reason == "No Supporting Document")
        //    {
        //        APIUtils.SignalR.SendToAllDutyOfficers(((Trinity.BE.User)Session.Instance[CommonConstants.USER_LOGIN]).UserId, "Supervisee get queue without supporting document", "Please check the Supervisee's information!", NotificationType.Notification);
        //    }
        //    var charSeparators = new char[] { ',' };
        //    var listSplitID = selectedID.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);

        //    if (listSplitID.Count()<=0)
        //    {
        //        CSCallJS.InvokeScript(_web, "showMessage", "You have select a date to report!");
        //        return;
        //    }
        //    //var listAppointment = JsonConvert.DeserializeObject<List<Appointment>>(data);
        //    Trinity.BE.Reason reasonModel = JsonConvert.DeserializeObject<Trinity.BE.Reason>(reason);
        //    if (reasonModel == null)
        //    {
        //        reasonModel = new Trinity.BE.Reason()
        //        {
        //            Detail = "",
        //            Value = (int)EnumAbsenceReasons.No_Valid_Reason
        //        };

        //    }
        //    //create absence report 
        //    var dalAbsence = new DAL_AbsenceReporting();


        //    var dalAppointment = new DAL_Appointments();

        //    var listSelectedDate = new DAL_Appointments().GetListAppointmentFromListSelectedDate(selectedID);
        //    //var listSelectedDate = dalAppointment.GetListAppointmentFromSelectedDate(listSplitID);
        //    foreach (var item in listSelectedDate)
        //    {
        //        var result = new DAL_AbsenceReporting().SetInfo(reasonModel);
        //        //var absenceModel = dalAbsence.SetInfo(reasonModel);
        //        var absenceModel = result;
        //        var create = dalAbsence.CreateAbsenceReporting(absenceModel, true);
        //        if (create)
        //        {
        //            var absenceId = absenceModel.ID;
        //            dalAppointment.UpdateAbsenceReason(item.ID, absenceModel.ID);

        //            //dalAppointment.UpdateReason(item.ID, absenceModel.ID);
        //        }

        //    }

        //    //send notify to case officer
        //    if (reasonModel.Value == (int)EnumAbsenceReasons.No_Valid_Reason)
        //    {
        //        APIUtils.SignalR.SendToAllDutyOfficers(user.UserId, user.Name + " has not provided any valid reason", " Please check the Supervisee's information!", NotificationType.Notification);
        //        LoadPageSupervisee();;
        //        return;
        //    }

        //    APIUtils.SignalR.SendToAllDutyOfficers(user.UserId, user.Name + " has provided absent reason", user.Name + " has provided absent reason.", NotificationType.Notification);
        //    ReportingForQueueNumber();
        //    LoadPageSupervisee();;
        //}

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
            LoadPageSupervisee();

        }

        public void LoadPageSupervisee()
        {
            main.NavigateTo(NavigatorEnums.Supervisee);
        }

        public void LoadAuthentication_SmartCard()
        {
            BarcodeScannerUtil.Instance.Disconnect();
            main.NavigateTo(NavigatorEnums.Authentication_SmartCard);
        }

        #region Scan Document
        private void ScanDocumentCallBack(string[] frontPath, string error)
        {
            _web.InvokeScript("showImgScanDocument", JsonConvert.SerializeObject(frontPath));
        }

        public void StartScanDocument()
        {
            Trinity.Util.DocumentScannerUtil.Instance.StartScanning(ScanDocumentCallBack);
        }

        public void StopScanDocument()
        {
            Trinity.Util.DocumentScannerUtil.Instance.StopScanning();
        }

        #endregion Scan Document

        public void LogOut()
        {
            // reset session value
            Session session = Session.Instance;
            var user = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];
            if (user != null && !string.IsNullOrEmpty(user.UserId))
            {
                Trinity.SignalR.Client.Instance.UserLoggedOut(user.UserId);
            }
            session.IsSmartCardAuthenticated = false;
            session.IsFingerprintAuthenticated = false;
            session[CommonConstants.USER_LOGIN] = null;
            session[CommonConstants.PROFILE_DATA] = null;
            RaiseLogOutCompletedEvent();

            BarcodeScannerUtil.Instance.Disconnect();
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
