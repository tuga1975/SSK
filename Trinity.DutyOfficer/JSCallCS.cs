using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Trinity.BE;
using Trinity.Common;
using Trinity.Common.Common;
using Trinity.DAL;
using Trinity.Identity;
using System.Linq;

namespace DutyOfficer
{
    [System.Runtime.InteropServices.ComVisibleAttribute(true)]
    public class JSCallCS : JSCallCSBase
    {
        private CodeBehind.PrintMUBAndTTLabels _printMUBAndTTLabel;

        public event EventHandler<NRICEventArgs> OnNRICFailed;
        public event EventHandler<ShowMessageEventArgs> OnShowMessage;
        public event Action OnLogOutCompleted;
        private bool _isPrintFailMUB = false;
        private bool _isPrintFailTT = false;
        private bool _isPrintFailUB = false;


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

        public List<Notification> getAlertsSendToDutyOfficer()
        {
            var dalNotify = new DAL_Notification();
            //Receive alerts and notifications from APS, SSK, SSA, UHP and ESP 
            List<string> modules = new List<string>() { "APS", "SSK", "SSA", "UHP", "ESP" };
            return dalNotify.GetNotificationsSentToDutyOfficer(true, modules);
        }

        public StationColorDevice GetStationClolorDevice()
        {
            var dalDeviceStatus = new DAL_DeviceStatus();
            StationColorDevice stationColorDevice = new StationColorDevice();
            stationColorDevice.SSAColor = dalDeviceStatus.CheckStatusDevicesStation(EnumStations.SSA) ? EnumColors.Green : EnumColors.Red;
            stationColorDevice.SSKColor = dalDeviceStatus.CheckStatusDevicesStation(EnumStations.SSK) ? EnumColors.Green : EnumColors.Red;
            stationColorDevice.ESPColor = dalDeviceStatus.CheckStatusDevicesStation(EnumStations.ESP) ? EnumColors.Green : EnumColors.Red;
            stationColorDevice.UHPColor = dalDeviceStatus.CheckStatusDevicesStation(EnumStations.UHP) ? EnumColors.Green : EnumColors.Red;
            return stationColorDevice;
        }

        #region Queue
        public void LoadDrugsTest(string Date, string UserId)
        {
            Trinity.DAL.DBContext.Label dbLabel = new Trinity.DAL.DAL_Labels().GetByDateAndUserId(Convert.ToDateTime(Date).Date, UserId);
            if (dbLabel != null)
            {
                Trinity.DAL.DBContext.DrugResult dbDrugResult = new Trinity.DAL.DAL_DrugResults().GetByMarkingNumber(dbLabel.MarkingNo);
                if (dbDrugResult != null)
                {
                    this._web.LoadPopupHtml("QueuePopupDrugs.html", new
                    {
                        Date = dbLabel.Date.ToString("dd MMM yyyy"),
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
                        }
                    });
                }
            }
            this._web.LoadPopupHtml("QueuePopupDrugs.html", null);
        }
        public object getDataQueue()
        {
            var data = new DAL_QueueNumber().GetAllQueueByDateIncludeDetail(DateTime.Now.Date)
                .Select(queue => new
                {
                    Queue_ID = queue.Queue_ID,
                    Date = queue.Appointment.Date.Date,
                    UserId = queue.Appointment.UserId,
                    NRIC = queue.Appointment.Membership_Users.NRIC,
                    Name = queue.Appointment.Membership_Users.Name,
                    APS = queue.QueueDetails.Where(c => c.Station == EnumStations.APS).FirstOrDefault().Color,
                    SSK = queue.QueueDetails.Where(c => c.Station == EnumStations.SSK).FirstOrDefault().Color,
                    SSA = queue.QueueDetails.Where(c => c.Station == EnumStations.SSA).FirstOrDefault().Color,
                    UHP = queue.QueueDetails.Where(c => c.Station == EnumStations.UHP).FirstOrDefault().Color,
                    HSA = queue.QueueDetails.Where(c => c.Station == EnumStations.HSA).FirstOrDefault().Status == EnumQueueStatuses.Finished ? "Drugs" : string.Empty,
                    ESP = queue.QueueDetails.Where(c => c.Station == EnumStations.ESP).FirstOrDefault().Color,
                    Outcome = queue.Outcome,
                    Message = new
                    {
                        content = queue.QueueDetails.Where(c => c.Station == queue.CurrentStation).FirstOrDefault().Message
                    }
                });
            return data;
        }
        public void LoadPopupQueue(string queue_ID)
        {
            var dalQueue = new DAL_QueueNumber();
            Trinity.BE.QueueInfo queueInfo = dalQueue.GetQueueInfoByQueueID(new Guid(queue_ID));
            this._web.LoadPopupHtml("QueuePopupDetail.html", queueInfo);
        }
        #endregion

        #region Alert & Notification Popup Detail

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
            DAL_Setting dalSetting = new DAL_Setting();
            return dalSetting.GetOperationSettings();
        }

        public void UpdateOperationSetting(string json)
        {
            var model = JsonConvert.DeserializeObject<Trinity.BE.SettingDetails>(json);
            Session session = Session.Instance;
            Trinity.BE.User dutyOfficer = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];
            model.Last_Updated_By = dutyOfficer.UserId;
            model.Last_Updated_Date = DateTime.Now;

            var dalSetting = new DAL_Setting();
            CheckWarningSaveSetting checkWarningSaveSetting = dalSetting.CheckWarningSaveSetting((EnumDayOfWeek)model.DayOfWeek);
            if (checkWarningSaveSetting != null && checkWarningSaveSetting.arrayDetail.Count > 0)
            {
                session[CommonConstants.SETTING_DETAIL] = model;
                // Show popup confirm with list Supervisee have appointment
                this._web.LoadPopupHtml("PopupConfirmDeleteAppointment.html", checkWarningSaveSetting);
                this._web.InvokeScript("showModal");
            }
            else
            {
                dalSetting.UpdateSettingAndTimeSlot(checkWarningSaveSetting, model);
            }
        }

        public void UpdateSettingAndTimeslot(string jsonCheckWarningSaveSetting, string jsonModel)
        {
            //var settingDetail = JsonConvert.DeserializeObject<Trinity.BE.SettingDetails>(jsonModel);
            Session session = Session.Instance;
            var settingDetail = (SettingDetails)session[CommonConstants.SETTING_DETAIL];
            var checkWarningSaveSetting = JsonConvert.DeserializeObject<Trinity.BE.CheckWarningSaveSetting>(jsonCheckWarningSaveSetting);
            var dalSetting = new DAL_Setting();
            dalSetting.UpdateSettingAndTimeSlot(checkWarningSaveSetting, settingDetail);
        }

        public bool CheckWarningSaveSetting(int dayOfWeek)
        {
            var dalSetting = new DAL_Setting();
            CheckWarningSaveSetting checkWarningSaveSetting = dalSetting.CheckWarningSaveSetting((EnumDayOfWeek)dayOfWeek);
            return (checkWarningSaveSetting.arrayDetail.Count > 0);

        }

        public void AddHoliday(string json)
        {
            var holiday = JsonConvert.DeserializeObject<Trinity.DAL.DBContext.Holiday>(json);
            var dalSetting = new DAL_Setting();

            Session session = Session.Instance;
            Trinity.BE.User dutyOfficer = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];

            dalSetting.AddHoliday(holiday, dutyOfficer.Name, dutyOfficer.UserId);
        }

        public void DeleteHoliday(string date)
        {
            DateTime dateHoliday = Convert.ToDateTime(date);
            var dalSetting = new DAL_Setting();

            Session session = Session.Instance;
            Trinity.BE.User dutyOfficer = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];

            dalSetting.DeleteHoliday(dateHoliday, dutyOfficer.Name);
        }

        #endregion

        #region Blocked
        public List<User> GetAllSuperviseesBlocked()
        {
            var dalUser = new DAL_User();
            return dalUser.GetAllSuperviseeBlocked(true);
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
            GetQueueForSupervisee(userId);
        }

        private void GetQueueForSupervisee(string userId)
        {
            DAL_Appointments _Appointment = new DAL_Appointments();
            Trinity.DAL.DBContext.Appointment appointment = _Appointment.GetMyAppointmentByDate(userId, DateTime.Today);
            if (appointment != null)
            {
                Trinity.DAL.DBContext.Timeslot timeslot = _Appointment.GetTimeslotNearest();
                appointment = _Appointment.UpdateTimeslotForAppointment(appointment.ID, timeslot.Timeslot_ID);
                var _dalQueue = new DAL_QueueNumber();
                Trinity.DAL.DBContext.Queue queueNumber = _dalQueue.InsertQueueNumber(appointment.ID, appointment.UserId, EnumStations.SSK);
            }
        }
        #endregion

        #region Appointment
        public List<Appointment> GetAllAppoinments()
        {
            var dalAppointment = new DAL_Appointments();
             return dalAppointment.GetAllAppointments();            
        }

        #endregion

        #region Statistics
        public List<Statistics> GetStatistics()
        {
            var dalAppointment = new DAL_Appointments();
            List<Statistics> data = dalAppointment.GetAllStatistics();

            foreach (var item in data)
            {
                item.Max = dalAppointment.GetMaximumNumberOfTimeslot(item.Timeslot_ID);
                item.Booked = dalAppointment.CountAppointmentBookedByTimeslot(item.Timeslot_ID);
                item.Reported = dalAppointment.CountAppointmentReportedByTimeslot(item.Timeslot_ID);
                item.No_Show = dalAppointment.CountAppointmentNoShowByTimeslot(item.Timeslot_ID);
                item.Available = item.Max - item.Booked - item.Reported - item.No_Show;
            }

            return data;
        }
        #endregion

        #region Print UB
        public List<Trinity.BE.Label> GetAllUBlabels()
        {
            var dalUBlabels = new DAL_Labels();
            return dalUBlabels.GetAllLabelsForUB();
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
                        break;

                    LabelInfo labelInfo = new LabelInfo
                    {
                        UserId = item.UserId,
                        Name = item.Name,
                        NRIC = item.NRIC,
                        Label_Type = EnumLabelType.MUB,
                        Date = DateTime.Now.ToString("dd/MM/yyyy"),
                        CompanyName = CommonConstants.COMPANY_NAME,
                        LastStation = EnumStations.DUTYOFFICER,
                        MarkingNo = CommonUtil.GenerateMarkingNumber(),
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
                MessageBox.Show("Unable to print UB labels\nPlease report to the Duty Officer", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            DeleteQRCodeImageFileTemp();
        }
        #endregion

        #region Print MUB And TT
        public List<Trinity.BE.Label> GetAllMUBAndTTlabels()
        {
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
                        break;

                    LabelInfo labelInfo = new LabelInfo
                    {
                        UserId = item.UserId,
                        Name = item.Name,
                        NRIC = item.NRIC,
                        Label_Type = EnumLabelType.MUB,
                        Date = DateTime.Now.ToString("dd/MM/yyyy"),
                        CompanyName = CommonConstants.COMPANY_NAME,
                        LastStation = EnumStations.DUTYOFFICER,
                        MarkingNo = CommonUtil.GenerateMarkingNumber(),
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

                            if (System.IO.File.Exists(fileName))
                                System.IO.File.Delete(fileName);

                            System.Drawing.Image bitmap = System.Drawing.Image.FromStream(ms);
                            bitmap.Save(fileName, System.Drawing.Imaging.ImageFormat.Png);
                        }
                    }

                    _web.LoadPageHtml("PrintingTemplates/MUBLabelTemplate.html", labelInfo);

                    Thread.Sleep(1500);
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

            if (labelInfo.IsMUB == true)
            {
                _printMUBAndTTLabel.StartPrintMUB(labelInfo);
            }
            if (labelInfo.IsTT == true)
            {
                _printMUBAndTTLabel.StartPrintTT(labelInfo);
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
                LastStation = EnumStations.DUTYOFFICER,
                PrintCount = e.LabelInfo.PrintCount,
                ReprintReason = e.LabelInfo.ReprintReason
            };

            // IsMUB = false: Print UB
            if (!e.LabelInfo.IsMUB)
            {
                labelInfo.Label_Type = EnumLabelType.UB;
                labelInfo.DrugType = e.LabelInfo.DrugType; 
            }

            DAL_Labels dalLabel = new DAL_Labels();
            if (dalLabel.UpdateLabel(labelInfo, labelInfo.UserId, labelInfo.Label_Type))
            {
                string message = "Print MUB for " + e.LabelInfo.Name + " successful.";
                //MessageBox.Show(message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            DeleteQRCodeImageFileTemp();
        }

        private void PrintMUBLabels_OnPrintMUBLabelFailed(object sender, CodeBehind.PrintMUBAndTTLabelsEventArgs e)
        {
            if (!_isPrintFailMUB)
            {
                _isPrintFailMUB = true;
                MessageBox.Show("Unable to print MUB labels\nPlease report to the Duty Officer", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            DeleteQRCodeImageFileTemp();
        }

        private void PrintTTLabels_OnPrintTTLabelSucceeded(object sender, PrintMUBAndTTLabelsEventArgs e)
        {
            var labelInfo = new Trinity.BE.Label
            {
                UserId = e.LabelInfo.UserId,
                Label_Type = EnumLabelType.TT,
                CompanyName = e.LabelInfo.CompanyName,
                MarkingNo = e.LabelInfo.MarkingNo,
                //DrugType = e.LabelInfo.DrugType,
                NRIC = e.LabelInfo.NRIC,
                Name = e.LabelInfo.Name,
                Date = DateTime.Now,
                LastStation = e.LabelInfo.LastStation,
                PrintCount = e.LabelInfo.PrintCount,
                ReprintReason = e.LabelInfo.ReprintReason
            };

            var dalLabel = new DAL_Labels();
            if (dalLabel.UpdateLabel(labelInfo, labelInfo.UserId, EnumLabelType.TT))
            {
                string message = "Print TT for " + e.LabelInfo.Name + " successful.";
                MessageBox.Show(message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            DeleteQRCodeImageFileTemp();
        }

        private void PrintTTLabels_OnPrintTTLabelFailed(object sender, CodeBehind.PrintMUBAndTTLabelsEventArgs e)
        {
            if (!_isPrintFailTT)
            {
                _isPrintFailTT = true;
                MessageBox.Show("Unable to print TT labels\nPlease report to the Duty Officer", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            Session session = Session.Instance;
            session.IsSmartCardAuthenticated = false;
            session.IsFingerprintAuthenticated = false;
            session[CommonConstants.USER_LOGIN] = null;
            session[CommonConstants.PROFILE_DATA] = null;

            //
            // RaiseLogOutCompletedEvent
            RaiseLogOutCompletedEvent();
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
