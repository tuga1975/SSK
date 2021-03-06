﻿using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Trinity.BE;
using Trinity.Common;
using Trinity.Common.Common;
using Trinity.DAL;
using Trinity.Device.Authentication;
using Trinity.Device.Util;
using Trinity.Identity;

namespace DutyOfficer
{
    [System.Runtime.InteropServices.ComVisibleAttribute(true)]
    public class JSCallCS : JSCallCSBase
    {
        private CodeBehind.PrintMUBAndTTLabels _printMUBAndTTLabel;

        //public event EventHandler<NRICEventArgs> OnNRICFailed;
        //public event EventHandler<ShowMessageEventArgs> OnShowMessage;
        public event Action OnLogOutCompleted;
        private bool _isPrintFailMUB = false;
        private bool _isPrintFailTT = false;
        private bool _isPrintFailUB = false;
        private bool _isFocusQueue = false;

        public JSCallCS(WebBrowser web)
        {
            this._web = web;
            _thisType = this.GetType();

            _printMUBAndTTLabel = new CodeBehind.PrintMUBAndTTLabels(web);
            _printMUBAndTTLabel.OnPrintMUBLabelsSucceeded += PrintMUBLabels_OnPrintMUBLabelSucceeded;
            _printMUBAndTTLabel.OnPrintMUBLabelsFailed += PrintMUBLabels_OnPrintMUBLabelFailed;
            _printMUBAndTTLabel.OnPrintTTLabelsSucceeded += PrintTTLabels_OnPrintTTLabelSucceeded;
            _printMUBAndTTLabel.OnPrintTTLabelsFailed += PrintTTLabels_OnPrintTTLabelFailed;
            _printMUBAndTTLabel.OnPrintMUBAndTTLabelsException += PrintMUBAndTTLabels_OnPrintTTLabelException;
            _printMUBAndTTLabel.OnPrintUBLabelsFailed += PrintUBLabels_OnPrintUBLabelFailed;
        }

        public StationColorDevice GetStationClolorDevice()
        {
            var dalDeviceStatus = new DAL_DeviceStatus();
            StationColorDevice _StationColorDevice = new StationColorDevice();
            _StationColorDevice.SSAColor = dalDeviceStatus.CheckStatusDevicesStation(EnumStation.ALK);
            _StationColorDevice.SSKColor = dalDeviceStatus.CheckStatusDevicesStation(EnumStation.ARK);
            _StationColorDevice.ESPColor = dalDeviceStatus.CheckStatusDevicesStation(EnumStation.SSP);
            _StationColorDevice.UHPColor = dalDeviceStatus.CheckStatusDevicesStation(EnumStation.SHP);

            Trinity.Common.Utils.LogManager.Info("GetStationClolorDevice: " + JsonConvert.SerializeObject(_StationColorDevice));
            return _StationColorDevice;
        }

        #region Queue
        public void LoadDrugsTest(string userId)
        {
            var user = new Trinity.DAL.DAL_User().GetUserById(userId);
            Trinity.DAL.DBContext.DrugResult dbDrugResult = new Trinity.DAL.DAL_DrugResults().GetByNRICAndUploadedDate(user.NRIC, DateTime.Today);
            if (dbDrugResult != null)
            {
                this._web.LoadPopupHtml("QueuePopupDrugs.html", new
                {
                    Date = DateTime.Today.ToString("dd MMM yyyy"),
                    Name = user.Name,
                    TestResults = new
                    {
                        AMPH = dbDrugResult.AMPH,
                        BENZ = dbDrugResult.BENZ,
                        OPI = dbDrugResult.OPI,
                        THC = dbDrugResult.THC
                    },
                    Seal = new
                    {
                        BARB = dbDrugResult.BARB,
                        BUPRE = dbDrugResult.BUPRE,
                        CAT = dbDrugResult.CAT,
                        COCA = dbDrugResult.COCA,
                        KET = dbDrugResult.KET,
                        LSD = dbDrugResult.LSD,
                        METH = dbDrugResult.METH,
                        MTQL = dbDrugResult.MTQL,
                        NPS = dbDrugResult.NPS,
                        PCP = dbDrugResult.PCP,
                        PPZ = dbDrugResult.PPZ
                    },
                    UserId = userId
                });
            }
        }
        public void SaveDrugTest(string UserId, bool COCA, bool BARB, bool LSD, bool METH, bool MTQL, bool PCP, bool KET, bool BUPRE, bool CAT, bool PPZ, bool NPS)
        {
            Session session = Session.Instance;
            Trinity.BE.User dutyOfficer = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];
            DAL_DrugResults dalDrug = new DAL_DrugResults();
            dalDrug.UpdateDrugSeal(UserId, COCA, BARB, LSD, METH, MTQL, PCP, KET, BUPRE, CAT, PPZ, NPS, dutyOfficer.UserId);
            var dalQueue = new DAL_QueueNumber();
            dalQueue.UpdateQueueStatusByUserId(UserId, EnumStation.UT, EnumQueueStatuses.Finished, EnumStation.SSP, EnumQueueStatuses.Processing, "Waiting for SSP", EnumQueueOutcomeText.Processing);


            var user = new DAL_User().GetUserById(UserId);
            var dalLabel = new DAL_Labels();
            string MarkingNumber = dalLabel.GetMarkingNumber(user.UserId, DateTime.Today);
            if (string.IsNullOrEmpty(MarkingNumber))
                MarkingNumber = new DAL_SettingSystem().GenerateMarkingNumber();
            var myQueue = dalQueue.GetMyQueueToday(user.UserId);
            dalLabel.Insert(new Trinity.BE.Label
            {
                UserId = UserId,
                Label_Type = EnumLabelType.UB,
                CompanyName = CommonConstants.COMPANY_NAME,
                MarkingNo = MarkingNumber,
                NRIC = user.NRIC,
                Name = user.Name,
                DrugType = dalDrug.GetResultUTByNRIC(user.NRIC, DateTime.Today),
                Queue_ID = myQueue.Queue_ID,
                LastStation = EnumStation.DUTYOFFICER
            });

        }
        public object getDataQueue()
        {
            if (!_isFocusQueue)
            {
                SmartCard.Instance.Start();
                _isFocusQueue = true;
            }
            List<object> arrayData = new List<object>();
            new DAL_DrugResults().CheckDrugResult();
            arrayData.AddRange(new DAL_Appointments().GetByDate(DateTime.Today).Select(app => new
            {
                Queue_ID = app.Queue == null ? null : app.Queue.Queue_ID.ToString().Trim(),
                Date = app.Date,
                UserId = app.UserId,
                NRIC = app.Membership_Users.NRIC,
                Name = app.Membership_Users.Name,
                APS = app.Color(EnumStation.APS),
                ARK = app.Color(EnumStation.ARK),
                ALK = app.Color(EnumStation.ALK),
                SHP = app.Color(EnumStation.SHP),
                UT = new
                {
                    Status = app.Queue == null ? string.Empty : app.Queue.QueueDetails.FirstOrDefault(c => c.Station == EnumStation.UT).Status,
                    Text = app.Queue == null ? string.Empty : app.Queue.QueueDetails.Any(c => c.Station == EnumStation.UT && (c.Status == EnumQueueStatuses.SelectSealOrDiscard || c.Status == EnumQueueStatuses.Finished)) ? GetResultUT(app.Membership_Users.NRIC, app.Date) : string.Empty
                },
                SSP = app.Color(EnumStation.SSP),
                Outcome = app.Queue == null ? string.Empty : app.Queue.Outcome,
                Message = new
                {
                    content = (app.Queue == null ? string.Empty : app.Queue.QueueDetails.Where(c => c.Station == app.Queue.CurrentStation).FirstOrDefault().Message) ?? string.Empty
                }
            }).ToList());
            arrayData.AddRange(new DAL_QueueNumber().GetQueueWalkInByDate(DateTime.Today).Select(queue => new
            {
                Queue_ID = queue.Queue_ID,
                Date = queue.CreatedTime.Date,
                UserId = queue.UserId,
                NRIC = queue.Membership_Users1.NRIC,
                Name = queue.Membership_Users1.Name,
                APS = queue.Color(EnumStation.APS),
                ARK = queue.Color(EnumStation.ARK),
                ALK = queue.QueueDetails.FirstOrDefault(c => c.Station == EnumStation.ALK).Color,
                SHP = queue.QueueDetails.FirstOrDefault(c => c.Station == EnumStation.SHP).Color,
                UT = new
                {
                    Status = queue.QueueDetails.FirstOrDefault(c => c.Station == EnumStation.UT).Status,
                    Text = queue.QueueDetails.Any(c => c.Station == EnumStation.UT && (c.Status == EnumQueueStatuses.SelectSealOrDiscard || c.Status == EnumQueueStatuses.Finished)) ? GetResultUT(queue.Membership_Users1.NRIC, queue.CreatedTime.Date) : string.Empty
                },
                SSP = queue.QueueDetails.FirstOrDefault(c => c.Station == EnumStation.SSP).Color,
                Outcome = queue.Outcome,
                Message = new
                {
                    content = queue.QueueDetails.Where(c => c.Station == queue.CurrentStation).FirstOrDefault().Message == null ? "" : queue.QueueDetails.Where(c => c.Station == queue.CurrentStation).FirstOrDefault().Message
                }
            }).ToList());
            return arrayData;
        }
        public string GetResultUT(string NRIC, DateTime date)
        {
            DAL_DrugResults dalDrug = new DAL_DrugResults();
            return dalDrug.GetResultUTByNRIC(NRIC, date);
        }
        public void LoadPopupSeal(string date, string userId, string queueID, string resultUT)
        {
            this._web.LoadPopupHtml("QueuePopupSeal.html", new
            {
                Date = date,
                UserId = userId,
                Queue_ID = queueID,
                UTResult = resultUT // NEG or POS
            });
        }
        public void UpdateDisCard(string UserId, string UTResult)
        {
            Session session = Session.Instance;
            Trinity.BE.User dutyOfficer = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];
            DAL_DrugResults dalDrug = new DAL_DrugResults();


            if (UTResult.Equals(EnumUTResult.NEG))
            {
                dalDrug.UpdateDiscardDrugResult(UserId, dutyOfficer.UserId);
                new Trinity.DAL.DAL_QueueNumber().UpdateQueueStatusByUserId(UserId, EnumStation.UT, EnumQueueStatuses.Finished, EnumStation.SSP, EnumQueueStatuses.NotRequired, string.Empty, EnumQueueOutcomeText.Processing);
                new Trinity.DAL.DAL_QueueNumber().UpdateQueueStatusByUserId(UserId, EnumStation.SSP, EnumQueueStatuses.NotRequired, EnumStation.DUTYOFFICER, EnumQueueStatuses.TabSmartCard, string.Empty, EnumQueueOutcomeText.TapSmartCardToContinue);
            }
            else
            {
                dalDrug.UpdateDiscardDrugResult(UserId, dutyOfficer.UserId);
                new Trinity.DAL.DAL_QueueNumber().UpdateQueueStatusByUserId(UserId, EnumStation.UT, EnumQueueStatuses.Finished, EnumStation.SSP, EnumQueueStatuses.NotRequired, string.Empty, EnumQueueOutcomeText.Processing);
                new Trinity.DAL.DAL_QueueNumber().UpdateQueueStatusByUserId(UserId, EnumStation.SSP, EnumQueueStatuses.NotRequired, EnumStation.DUTYOFFICER, EnumQueueStatuses.TabSmartCard, EnumMessage.SelectOutCome, EnumQueueOutcomeText.TapSmartCardToContinue);
            }
        }
        public void LoadPopupQueue(string queue_ID)
        {
            var dalQueue = new DAL_QueueNumber();
            Trinity.BE.QueueInfo queueInfo = dalQueue.GetQueueInfoByQueueID(new Guid(queue_ID));
            if (queueInfo.CurrentStation == EnumStation.UT)
            {
                if (queueInfo.Status == EnumQueueStatuses.Finished || queueInfo.Status == EnumQueueStatuses.SelectSealOrDiscard)
                {
                    queueInfo.Status = GetResultUT(queueInfo.NRIC, queueInfo.Date.Date);
                }
                else if (queueInfo.Status != EnumQueueStatuses.Errors)
                {
                    queueInfo.Status = string.Empty;
                }
            }
            this._web.LoadPopupHtml("QueuePopupDetail.html", queueInfo);
        }

        public void LoadPopupOutcome(string userId)
        {
            Trinity.BE.User user = new DAL_User().GetUserById(userId);
            this._web.LoadPopupHtml("QueuePopupOutcome.html", new
            {
                UserId = user.UserId,
                Name = user.Name
            });
        }

        public void startInstanceSmartcard()
        {
            _isFocusQueue = true;
            //SmartCard.Instance.Start();
        }

        public void TapSmartCardOnQueueSucceed(string queueId)
        {
            DAL_QueueNumber dalQueue = new DAL_QueueNumber();
            var queueDetail = dalQueue.GetQueueInfoByQueueID(new Guid(queueId));
            if (queueDetail != null)
            {
                string resultUT = GetResultUT(queueDetail.NRIC, queueDetail.Date);

                if (resultUT == EnumUTResult.NEG)
                {
                    var drugResult = new DAL_DrugResults().GetByNRICAndUploadedDate(queueDetail.NRIC, queueDetail.Date);
                    if (drugResult != null && drugResult.IsSealed.Value)
                    {
                        this._web.InvokeScript("openPopupOutcome", queueId);
                    }
                    else
                    {
                        dalQueue.UpdateQueueStatusByUserId(queueDetail.UserId, EnumStation.DUTYOFFICER, EnumQueueStatuses.Finished, EnumStation.DUTYOFFICER, EnumQueueStatuses.Finished, "", EnumQueueOutcomeText.UnconditionalRelease);

                        // Re-load queue
                        this._web.InvokeScript("reloadDataQueues");
                    }
                }
                else
                {
                    this._web.InvokeScript("openPopupOutcome", queueId);
                }
            }
        }

        public void SaveOutcome(string UserID, string outcome)
        {
            DAL_QueueNumber dalQueue = new DAL_QueueNumber();

            dalQueue.UpdateQueueStatusByUserId(UserID, EnumStation.DUTYOFFICER, EnumQueueStatuses.Finished, EnumStation.DUTYOFFICER, EnumQueueStatuses.Finished, "", outcome);

            // Re-load queue
            this._web.InvokeScript("reloadDataQueues");
        }
        #endregion

        #region Alert & Notification Popup Detail

        public List<Notification> getAlertsSendToDutyOfficer()
        {
            if (_isFocusQueue)
            {
                SmartCardReaderUtil.Instance.StopSmartCardMonitor();
                _isFocusQueue = false;
            }

            var dalNotify = new DAL_Notification();
            string userID = ((Trinity.BE.User)Session.Instance[CommonConstants.USER_LOGIN]).UserId;
            if (userID != null || userID != "")
            {
                //Receive alerts and notifications from APS, ARK, ALK, SHP and SSP 
                List<string> modules = new List<string>() { EnumStation.APS, EnumStation.ARK, EnumStation.ALK, EnumStation.SHP, EnumStation.SSP, EnumStation.ENROLMENT };
                return dalNotify.GetAllNotifications(userID, modules, true);
            }
            return null;
        }
        public void GetCountNotificationsUnread()
        {
            string userID = ((Trinity.BE.User)Session.Instance[CommonConstants.USER_LOGIN]).UserId;
            List<string> modules = new List<string>() { EnumStation.APS, EnumStation.ARK, EnumStation.ALK, EnumStation.SHP, EnumStation.SSP, EnumStation.ENROLMENT };
            int count =  new DAL_Notification().GetCountNotificationsUnread(userID,modules,true);
            this._web.InvokeScript("setCountMessage", count);
        }
        public bool updateReadedStatus(string NotificationID)
        {
            var dalNotify = new DAL_Notification();
            string userID = ((Trinity.BE.User)Session.Instance[CommonConstants.USER_LOGIN]).UserId;
            if (userID != null || userID != "")
            {
                Response<bool> response = dalNotify.updateReadStatus(NotificationID, true);
                Trinity.SignalR.Client.Instance.DOReadMessage(NotificationID);
                return response.ResponseCode == (int)EnumResponseStatuses.Success;
            }
            return false;
        }

        public void LoadPopupAlert(string jsonData)
        {
            this._web.LoadPopupHtml("AlertPopupDetail.html", jsonData);
        }

        #endregion

        #region Settings
        public void PopupAddHoliday()
        {
            this._web.LoadPopupHtml("PopupAddHoliday.html");
        }

        public void PopupEditOperationalDate(string dayOfWeek)
        {
            this._web.LoadPopupHtml("PopupEditOperationalDate.html", dayOfWeek);
        }

        public SettingModel GetSettings()
        {
            if (_isFocusQueue)
            {
                SmartCardReaderUtil.Instance.StopSmartCardMonitor();
                _isFocusQueue = false;
            }

            DAL_Setting dalSetting = new DAL_Setting();
            Session session = Session.Instance;
            Trinity.BE.User dutyOfficer = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];
            return dalSetting.GetOperationSettings(dutyOfficer.UserId);
        }

        public void UpdateOperationSetting(string json)
        {
            var model = JsonConvert.DeserializeObject<Trinity.BE.SettingDetails>(json);
            Session session = Session.Instance;
            Trinity.BE.User dutyOfficer = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];
            model.Last_Updated_By = dutyOfficer.UserId;
            model.Last_Updated_Date = DateTime.Now;

            var dalSetting = new DAL_Setting();
            CheckWarningSaveSetting checkWarningSaveSetting = dalSetting.CheckWarningSaveSetting(model.DayOfWeek);
            if (checkWarningSaveSetting != null && checkWarningSaveSetting.arrayDetail.Count > 0)
            {
                session[CommonConstants.SETTING_DETAIL] = model;
                // Show popup confirm with list Supervisee have appointment
                this._web.LoadPopupHtml("PopupConfirmDeleteAppointment.html", checkWarningSaveSetting);
                this._web.InvokeScript("showModal");
            }
            else
            {
                SettingUpdate settingUpdate = new SettingUpdate()
                {
                    CheckWarningSaveSetting = checkWarningSaveSetting,
                    SettingDetails = model
                };
                dalSetting.UpdateSettingAndTimeSlot(settingUpdate);
            }
        }

        public void UpdateSettingAndTimeslot(string jsonCheckWarningSaveSetting, string jsonModel)
        {
            //var settingDetail = JsonConvert.DeserializeObject<Trinity.BE.SettingDetails>(jsonModel);
            Session session = Session.Instance;
            var settingDetail = (SettingDetails)session[CommonConstants.SETTING_DETAIL];
            var checkWarningSaveSetting = JsonConvert.DeserializeObject<Trinity.BE.CheckWarningSaveSetting>(jsonCheckWarningSaveSetting);
            var dalSetting = new DAL_Setting();
            SettingUpdate settingUpdate = new SettingUpdate()
            {
                CheckWarningSaveSetting = checkWarningSaveSetting,
                SettingDetails = settingDetail
            };
            dalSetting.UpdateSettingAndTimeSlot(settingUpdate);
        }

        public bool CheckWarningSaveSetting(int dayOfWeek)
        {
            var dalSetting = new DAL_Setting();
            CheckWarningSaveSetting checkWarningSaveSetting = dalSetting.CheckWarningSaveSetting(dayOfWeek);
            return (checkWarningSaveSetting.arrayDetail.Count > 0);

        }

        public bool AddHoliday(string json)
        {
            var holiday = JsonConvert.DeserializeObject<Trinity.DAL.DBContext.Holiday>(json);
            var dalSetting = new DAL_Setting();

            Session session = Session.Instance;
            Trinity.BE.User dutyOfficer = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];

            dalSetting.AddHoliday(holiday.Holiday1, holiday.ShortDesc, holiday.Notes, dutyOfficer.Name, dutyOfficer.UserId);
            return true;
        }

        public void DeleteHoliday(string json)
        {
            var data = JsonConvert.DeserializeObject<List<Trinity.BE.Holiday>>(json);
            //DateTime dateHoliday = Convert.ToDateTime(date);
            var dalSetting = new DAL_Setting();

            Session session = Session.Instance;
            Trinity.BE.User dutyOfficer = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];

            dalSetting.DeleteHoliday(data, dutyOfficer.Name);
        }

        #endregion

        #region Blocked
        public List<User> GetAllSuperviseesBlocked()
        {
            if (_isFocusQueue)
            {
                Trinity.Device.Util.SmartCardReaderUtil.Instance.StopSmartCardMonitor();
                _isFocusQueue = false;
            }

            var dalUser = new DAL_User();
            return dalUser.GetAllSuperviseeBlocked();
        }

        public void LoadPopupBlock(string userId)
        {
            UserBlockedModel rawData = new UserBlockedModel();
            Session session = Session.Instance;
            Trinity.BE.User dutyOfficer = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];
            rawData.OfficerNRIC = dutyOfficer.NRIC;
            rawData.OfficerName = dutyOfficer.Name;
            rawData.UserId = userId;

            this._web.LoadPopupHtml("BlockedPopupDetail.html", rawData);
        }

        public void UnblockSuperviseeByUserId(string userId, string reason)
        {
            var dalUser = new DAL_User();
            dalUser.UnblockSuperviseeById(userId, reason);
            //GetQueueForSupervisee(userId);
        }

        private void GetQueueForSupervisee(string userId)
        {
            Session session = Session.Instance;
            Trinity.BE.User dutyOfficer = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];
            DAL_Appointments dalAppointment = new DAL_Appointments();
            Trinity.DAL.DBContext.Appointment appointment = dalAppointment.GetAppointmentByDate(userId, DateTime.Today);
            if (appointment != null)
            {
                //var responseResult= _Appointment.GetTimeslotNearestAppointment();
                DAL_QueueNumber dalQueue = new DAL_QueueNumber();
                Trinity.DAL.DBContext.Timeslot timeslot = dalQueue.GetTimeSlotEmpty();
                var response = dalAppointment.UpdateTimeslotForApptmt(appointment.ID, timeslot.Timeslot_ID);
                appointment = response;
                Trinity.DAL.DBContext.Queue queueNumber = dalQueue.InsertQueueNumber(appointment.ID, appointment.UserId, EnumStation.ARK, dutyOfficer.UserId);
            }
        }
        #endregion

        #region Appointment
        public object getTimeSlot(string date, string category)
        {
            var dataRe = new DAL_Timeslots().GetTimeSlots(DateTime.ParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture));
            if (!string.IsNullOrEmpty(category))
            {
                dataRe = dataRe.Where(d => d.Category.Equals(category)).ToList();
            }
            return dataRe.OrderBy(d => d.StartTime).Select(d => new
            {
                ID = d.Timeslot_ID,
                Time = string.Format("{0:D2}:{1:D2}", d.StartTime.Value.Hours, d.StartTime.Value.Minutes) + " - " + string.Format("{0:D2}:{1:D2}", d.EndTime.Value.Hours, d.EndTime.Value.Minutes)
            });

        }
        public List<Appointment> GetAllAppoinments(string date, string category, string timeslot)
        {
            if (_isFocusQueue)
            {
                Trinity.Device.Util.SmartCardReaderUtil.Instance.StopSmartCardMonitor();
                _isFocusQueue = false;
            }

            var dalAppointment = new DAL_Appointments();
            var result = dalAppointment.GetAppointmentsByDate(DateTime.ParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture), category, timeslot);
            return result;
        }

        #endregion

        #region Statistics
        public List<Statistics> GetStatistics()
        {
            if (_isFocusQueue)
            {
                SmartCardReaderUtil.Instance.StopSmartCardMonitor();
                _isFocusQueue = false;
            }

            var dalAppointment = new DAL_Appointments();
            var result = dalAppointment.GetAllStats();
            List<Statistics> data = result;

            foreach (var item in data)
            {
                var maxResult = dalAppointment.GetMaxNumberOfTimeslot(item.Timeslot_ID);
                item.Max = maxResult;
                var bookedResult = dalAppointment.CountApptmtBookedByTimeslot(item.Timeslot_ID);
                item.Booked = bookedResult;
                var reportedResult = dalAppointment.CountApptmtReportedByTimeslot(item.Timeslot_ID);
                item.Reported = reportedResult;
                var absentResult = dalAppointment.CountApptmtAbsentByTimeslot(item.Timeslot_ID);
                item.Absent = absentResult;
                item.Available = item.Max - item.Booked - item.Reported - item.Absent;
            }

            return data;
        }

        public void LoadPageAppointment(string dateSearch, string startTime, string endTime)
        {
            _web.LoadPageHtml("Appointments.html", dateSearch + ";" + startTime + ";" + endTime);
        }
        #endregion

        #region Print UB
        public List<Trinity.BE.Label> GetAllUBlabels()
        {
            return new DAL_Labels().GetAllLabelsForUBToday();
        }

        //Load Popup of UB label
        public void LoadPopupUBLabel(string json)
        {
            var data = JsonConvert.DeserializeObject<List<Trinity.BE.Label>>(json);

            Session session = Session.Instance;
            Trinity.BE.User dutyOfficer = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];
            data[0].OfficerNRIC = dutyOfficer.NRIC;
            data[0].OfficerName = dutyOfficer.Name;

            this._web.LoadPopupHtml("UBlabelPopup.html", data);

        }

        //Mapping Template UB labels
        public void MappingTemplateUBLabels(string json, string reason)
        {
            try
            {
                List<Trinity.BE.Label> lstLabel = JsonConvert.DeserializeObject<List<Trinity.BE.Label>>(json);

                foreach (var item in lstLabel)
                {
                    if (_isPrintFailUB)
                    {
                        break;
                    }

                    DAL_User dalUser = new DAL_User();
                    string userID = dalUser.GetSuperviseeByNRIC(item.NRIC).UserId;
                    LabelInfo labelInfo = new LabelInfo
                    {
                        UserId = userID,
                        Name = item.Name,
                        NRIC = item.NRIC,
                        Label_Type = EnumLabelType.UB,
                        Date = DateTime.Now.ToString("dd/MM/yyyy"),
                        CompanyName = CommonConstants.COMPANY_NAME,
                        LastStation = EnumStation.DUTYOFFICER,
                        MarkingNo = item.MarkingNo,
                        DrugType = item.DrugType,
                        ReprintReason = reason,
                        IsMUB = false
                    };
                    if (string.IsNullOrEmpty(item.DrugType) || item.DrugType != "NA")
                    {
                        labelInfo.DrugType = new DAL_DrugResults().GetDrugTypeByNRIC(item.NRIC);
                    }
                    byte[] byteArrayQRCode = null;
                    byteArrayQRCode = CommonUtil.CreateLabelQRCode(labelInfo, "AESKey", true);
                    labelInfo.QRCode = byteArrayQRCode;

                    using (var ms = new System.IO.MemoryStream(byteArrayQRCode))
                    {
                        if (!Directory.Exists(CSCallJS.curDir + "\\Temp"))
                        {
                            Directory.CreateDirectory(CSCallJS.curDir + "\\Temp");
                        }
                        string fileName = String.Format("{0}/Temp/{1}", CSCallJS.curDir, "QRCode.png");
                        if (System.IO.File.Exists(fileName))
                            System.IO.File.Delete(fileName);

                        if (System.IO.File.Exists(fileName))
                            System.IO.File.Delete(fileName);

                        System.Drawing.Image bitmap = System.Drawing.Image.FromStream(ms);
                        bitmap.Save(fileName, System.Drawing.Imaging.ImageFormat.Png);
                    }

                    _web.LoadPageHtml("PrintingTemplates/UBLabelTemplate .html", labelInfo);

                    Thread.Sleep(1500);
                }

                if (_isPrintFailUB)
                {
                    MessageBox.Show("Unable to print UB labels", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show("Print all labels successfully", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                _isPrintFailUB = false;
            }
            catch (Exception e)
            {

            }
        }

        public void CallPrintingUB(string jsonModel, string base64String)
        {
            string base64StringCanvas = base64String.Split(',')[1];
            byte[] bitmapBytes = Convert.FromBase64String(base64StringCanvas);

            var labelInfo = JsonConvert.DeserializeObject<LabelInfo>(jsonModel);
            labelInfo.BitmapLabel = bitmapBytes;

            _printMUBAndTTLabel.StartPrintMUB(labelInfo);
        }

        private void PrintUBLabels_OnPrintUBLabelFailed(object sender, CodeBehind.PrintMUBAndTTLabelsEventArgs e)
        {
            if (!_isPrintFailUB)
            {
                _isPrintFailUB = true;
                //MessageBox.Show("Unable to print UB labels\nPlease report to the Duty Officer", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            DeleteQRCodeImageFileTemp();
        }
        #endregion

        #region Print MUB And TT
        public List<Trinity.BE.Label> GetAllMUBAndTTlabels()
        {
            if (_isFocusQueue)
            {
                Trinity.Device.Util.SmartCardReaderUtil.Instance.StopSmartCardMonitor();
                _isFocusQueue = false;
            }

            var dalMUBAndTTlabels = new DAL_Labels();
            return dalMUBAndTTlabels.GetAllLabelsForMUBAndTT();
        }

        //Load Popup of MUBAndTT Label
        public void LoadPopupMUBAndTTLabel(string json)
        {
            var data = JsonConvert.DeserializeObject<List<Trinity.BE.Label>>(json);

            if (data.Count() > 0)
            {
                Session session = Session.Instance;
                Trinity.BE.User dutyOfficer = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];
                data[0].OfficerNRIC = dutyOfficer.NRIC;
                data[0].OfficerName = dutyOfficer.Name;

                this._web.LoadPopupHtml("MUBAndTTPopup.html", data);
            }
        }

        //Mapping Template MUB and TT labels
        public void MappingTemplateMUBAndTTLabels(string json, string reason)
        {
            try
            {
                List<Trinity.BE.Label> lstLabel = JsonConvert.DeserializeObject<List<Trinity.BE.Label>>(json);

                foreach (var item in lstLabel)
                {
                    if (_isPrintFailMUB && _isPrintFailTT)
                    {
                        break;
                    }

                    DAL_User dalUser = new DAL_User();
                    string userID = dalUser.GetSuperviseeByNRIC(item.NRIC).UserId;
                    string markingNo = new DAL_Labels().GetMarkingNoByUserId(userID);
                    LabelInfo labelInfo = new LabelInfo
                    {
                        UserId = userID,
                        Name = item.Name,
                        NRIC = item.NRIC,
                        Label_Type = EnumLabelType.MUB,
                        Date = DateTime.Now.ToString("dd/MM/yyyy"),
                        CompanyName = CommonConstants.COMPANY_NAME,
                        LastStation = EnumStation.DUTYOFFICER,
                        MarkingNo = markingNo,//new DAL_SettingSystem().GenerateMarkingNumber(),
                        //DrugType = "NA",
                        ReprintReason = reason,
                        IsMUB = item.IsMUB,
                        IsTT = item.IsTT
                    };

                    if (item.IsMUB)
                    {
                        byte[] byteArrayQRCode = null;
                        byteArrayQRCode = CommonUtil.CreateLabelQRCode(labelInfo, "AESKey");
                        labelInfo.QRCode = byteArrayQRCode;

                        using (var ms = new System.IO.MemoryStream(byteArrayQRCode))
                        {
                            if (!Directory.Exists(CSCallJS.curDir + "\\Temp"))
                            {
                                Directory.CreateDirectory(CSCallJS.curDir + "\\Temp");
                            }
                            string fileName = String.Format("{0}/Temp/{1}", CSCallJS.curDir, "QRCode.png");
                            if (System.IO.File.Exists(fileName))
                                System.IO.File.Delete(fileName);

                            System.Drawing.Image bitmap = System.Drawing.Image.FromStream(ms);
                            bitmap.Save(fileName, System.Drawing.Imaging.ImageFormat.Png);
                        }
                        _web.LoadPageHtml("PrintingTemplates/MUBLabelTemplate.html", labelInfo);
                        Thread.Sleep(1000);
                    }
                    if (item.IsTT)
                    {
                        _printMUBAndTTLabel.StartPrintTT(labelInfo);
                        Thread.Sleep(500);
                    }

                }

                if (_isPrintFailMUB && _isPrintFailTT)
                {
                    MessageBox.Show("Unable to print MUB and TT labels", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (_isPrintFailMUB)
                {
                    MessageBox.Show("Unable to print MUB labels", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (_isPrintFailTT)
                {
                    MessageBox.Show("Unable to print TT labels", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show("Print all labels successfully", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                _isPrintFailMUB = false;
                _isPrintFailTT = false;
            }
            catch (Exception e)
            {

            }
        }

        //Call Printing MUB and TT labels
        public void CallPrintingMUBAndTT(string jsonModel, string base64String)
        {
            string base64StringCanvas = base64String.Split(',')[1];
            byte[] bitmapBytes = Convert.FromBase64String(base64StringCanvas);

            var labelInfo = JsonConvert.DeserializeObject<LabelInfo>(jsonModel);
            labelInfo.BitmapLabel = bitmapBytes;

            // Set MarkingNo for demo purpose
            //labelInfo.MarkingNo = "CSA18001991";
            if (labelInfo.IsMUB == true)
            {
                _printMUBAndTTLabel.StartPrintMUB(labelInfo);
            }

        }

        private void PrintMUBLabels_OnPrintMUBLabelSucceeded(object sender, PrintMUBAndTTLabelsEventArgs e)
        {
            Trinity.BE.Label labelInfo = new Trinity.BE.Label
            {
                UserId = e.LabelInfo.UserId,
                Label_Type = EnumLabelType.MUB,
                CompanyName = e.LabelInfo.CompanyName,
                MarkingNo = e.LabelInfo.MarkingNo,
                DrugType = e.LabelInfo.DrugType,
                NRIC = e.LabelInfo.NRIC,
                Name = e.LabelInfo.Name,
                Date = DateTime.Now,
                QRCode = e.LabelInfo.QRCode,
                LastStation = EnumStation.DUTYOFFICER,
                PrintCount = e.LabelInfo.PrintCount,
                ReprintReason = e.LabelInfo.ReprintReason,
                PrintStatus = EnumPrintStatus.Successful
            };

            // IsMUB = false: Print UB
            DAL_Labels dalLabel = new DAL_Labels();
            if (!e.LabelInfo.IsMUB)
            {
                //labelInfo.Label_Type = EnumLabelType.UB;
                //labelInfo.DrugType = e.LabelInfo.DrugType;
                //if (dalLabel.UpdateLabel(labelInfo) != null)
                //{
                //    string message = "Print MUB for " + e.LabelInfo.Name + " successful.";
                //    //MessageBox.Show(message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //}
            }
            else
            {
                dalLabel.UpdatePrinting(e.LabelInfo.UserId, EnumLabelType.MUB, EnumPrintStatus.Successful, EnumStation.DUTYOFFICER, DateTime.Today);
            }



            DeleteQRCodeImageFileTemp();
        }

        private void PrintMUBLabels_OnPrintMUBLabelFailed(object sender, CodeBehind.PrintMUBAndTTLabelsEventArgs e)
        {
            if (!_isPrintFailMUB)
            {
                _isPrintFailMUB = true;
                //MessageBox.Show("Unable to print MUB labels\nPlease report to the Duty Officer", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            DeleteQRCodeImageFileTemp();
        }

        private void PrintTTLabels_OnPrintTTLabelSucceeded(object sender, PrintMUBAndTTLabelsEventArgs e)
        {
            var dalLabel = new DAL_Labels();
            dalLabel.UpdatePrinting(e.LabelInfo.UserId, EnumLabelType.TT, EnumPrintStatus.Successful, EnumStation.DUTYOFFICER, DateTime.Today);
            DeleteQRCodeImageFileTemp();
        }

        private void PrintTTLabels_OnPrintTTLabelFailed(object sender, CodeBehind.PrintMUBAndTTLabelsEventArgs e)
        {
            if (!_isPrintFailTT)
            {
                _isPrintFailTT = true;
                //MessageBox.Show("Unable to print TT labels\nPlease report to the Duty Officer", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            DeleteQRCodeImageFileTemp();
        }

        private void PrintMUBAndTTLabels_OnPrintTTLabelException(object sender, ExceptionArgs e)
        {
            //MessageBox.Show(e.ErrorMessage, "", MessageBoxButtons.OK, MessageBoxIcon.Error);

            DeleteQRCodeImageFileTemp();
        }

        #endregion

        public void DeleteQRCodeImageFileTemp()
        {
            string fileName = String.Format("{0}/Temp/{1}", CSCallJS.curDir, "QRCode.png");
            if (System.IO.File.Exists(fileName))
                System.IO.File.Delete(fileName);
        }

        public void LogOut()
        {
            // reset session value
            string userID = ((Trinity.BE.User)Session.Instance[CommonConstants.USER_LOGIN]).UserId;
            Session session = Session.Instance;
            session.IsSmartCardAuthenticated = false;
            session.IsFingerprintAuthenticated = false;
            session[CommonConstants.USER_LOGIN] = null;
            session[CommonConstants.PROFILE_DATA] = null;

            //
            // RaiseLogOutCompletedEvent
            RaiseLogOutCompletedEvent();

            Trinity.SignalR.Client.Instance.UserLoggedOut(userID);
        }

        public void ManualLogin(string username, string password)
        {
            EventCenter eventCenter = EventCenter.Default;

            UserManager<ApplicationUser> userManager = ApplicationIdentityManager.GetUserManager();
            ApplicationUser appUser = userManager.Find(username, password);
            if (appUser != null)
            {
                // Authenticated successfully
                // Check if the current user is an Duty Officer or not
                if (userManager.IsInRole(appUser.Id, EnumUserRoles.DutyOfficer))
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
                }
                else
                {
                    eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Code = -2, Name = EventNames.LOGIN_FAILED, Message = "You do not have permission to access this page." });
                }
            }
            else
            {
                eventCenter.RaiseEvent(new Trinity.Common.EventInfo() { Code = -1, Name = EventNames.LOGIN_FAILED, Message = "Your username or password is incorrect." });
            }
        }

        public void Login(string username, string password)
        {
            var dalUser = new DAL_User();
            ApplicationUser appUser = dalUser.Login(username, password);
            if (appUser != null)
            {
                if (dalUser.IsInRole(appUser.Id, EnumUserRoles.DutyOfficer))
                {

                    Trinity.BE.User user = new Trinity.BE.User()
                    {
                        UserId = appUser.Id,
                        Status = appUser.Status,
                        SmartCardId = appUser.SmartCardId,
                        RightThumbFingerprint = appUser.RightThumbFingerprint,
                        LeftThumbFingerprint = appUser.LeftThumbFingerprint,
                        Name = appUser.Name,
                        NRIC = appUser.NRIC,
                        IsFirstAttempt = appUser.IsFirstAttempt
                    };
                    user.Role = EnumUserRoles.DutyOfficer;
                    Session session = Session.Instance;
                    session.IsUserNamePasswordAuthenticated = true;
                    session.Role = EnumUserRoles.EnrolmentOfficer;
                    session[CommonConstants.USER_LOGIN] = user;
                    Trinity.SignalR.Client.Instance.UserLoggedIn(user.UserId);
                    EventCenter.Default.RaiseEvent(new Trinity.Common.EventInfo() { Code = 0, Name = EventNames.LOGIN_SUCCEEDED });
                }
                else
                {
                    this._web.ShowMessage("You do not have permission to access this page.");
                }
            }
            else
            {
                this._web.ShowMessage("Your username or password is incorrect.");
            }
        }
        protected virtual void RaiseLogOutCompletedEvent()
        {
            OnLogOutCompleted?.Invoke();
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
