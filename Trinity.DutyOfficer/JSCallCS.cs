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
        }

        public void getAlertsSendToDutyOfficer()
        {
            var dalNotify = new DAL_Notification();
            //Receive alerts and notifications from APS, SSK, SSA, UHP and ESP 
            List<string> modules = new List<string>() { "APS", "SSK", "SSA", "UHP", "ESP" };
            List<Notification> data = dalNotify.GetNotificationsSentToDutyOfficer(true, modules);
            object result = null;
            if (data != null)
            {
                result = JsonConvert.SerializeObject(data, Formatting.Indented,
                    new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            }
            _web.InvokeScript("getDataCallback", result);
        }


        #region Queue

        public object getDataQueue()
        {
            MemberInfo[] members = typeof(EnumQueueStatuses).GetMembers(
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Static |
                BindingFlags.Instance |
                BindingFlags.DeclaredOnly);

            var data = new DAL_QueueNumber().GetAllQueueByDateIncludeDetail(DateTime.Now.Date)
                .Select(queue => new
                {
                    NRIC = queue.Appointment.Membership_Users.NRIC,
                    Name = queue.Appointment.Membership_Users.Name,
                    APS = queue.QueueDetails.Where(c => c.Station == EnumStations.APS).FirstOrDefault().Color,
                    SSK = queue.QueueDetails.Where(c => c.Station == EnumStations.SSK).FirstOrDefault().Color,
                    SSA = queue.QueueDetails.Where(c => c.Station == EnumStations.SSA).FirstOrDefault().Color,
                    UHP = queue.QueueDetails.Where(c => c.Station == EnumStations.UHP).FirstOrDefault().Color,
                    HSA = queue.QueueDetails.Where(c => c.Station == EnumStations.UHP).FirstOrDefault().Message,
                    ESP = queue.QueueDetails.Where(c => c.Station == EnumStations.ESP).FirstOrDefault().Color,
                    Outcome = queue.Outcome,
                    Message = new
                    {
                        content = queue.QueueDetails.Where(c => c.Station == queue.CurrentStation).FirstOrDefault().Message
                    }
                });
            return data;
        }
        public void LoadPopupQueue()
        {
            this._web.LoadPopupHtml("QueuePopupDetail.html");
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

            //object result = null;
            //if (data != null)
            //{
            //    result = JsonConvert.SerializeObject(data, Formatting.Indented,
            //        new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            //}
            //_web.InvokeScript("getDataCallback", result);
        }

        public void UpdateOperationSetting(string json)
        {
            var model = JsonConvert.DeserializeObject<Trinity.BE.SettingDetails>(json);
            Session session = Session.Instance;
            Trinity.BE.User dutyOfficer = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];
            model.Last_Updated_By = dutyOfficer.UserId;
            model.Last_Updated_Date = DateTime.Now;

            var dalSetting = new DAL_Setting();
            if (dalSetting.SaveOperationSetting(model))
            {
                //_web.InvokeScript("showMessageBox", "Update successful!");
            }
            //else
            //{
            //    _web.InvokeScript("showMessageBox", "Update faied!");
            //}
        }

        public void AddHoliday(string json)
        {
            var holiday = JsonConvert.DeserializeObject<Trinity.DAL.DBContext.Holiday>(json);
            var dalSetting = new DAL_Setting();
            dalSetting.AddHoliday(holiday);
        }

        public void DeleteHoliday(string date)
        {
            DateTime dateHoliday = Convert.ToDateTime(date);
            var dalSetting = new DAL_Setting();
            dalSetting.DeleteHoliday(dateHoliday);
        }

        #endregion

        #region Blocked
        public void GetAllSuperviseesBlocked()
        {
            var dalUser = new DAL_User();
            List<User> data = dalUser.GetAllSuperviseeBlocked(true);

            object result = null;
            if (data != null)
            {
                result = JsonConvert.SerializeObject(data, Formatting.Indented,
                    new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            }
            _web.InvokeScript("getDataCallback", result);
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

        }
        #endregion

        #region Appointment
        public void GetAllAppoinments()
        {
            var dalAppointment = new DAL_Appointments();
            List<Appointment> data = dalAppointment.GetAllAppointments();

            //foreach(var item in data)
            //{
            //    item.TimeSlot = GetDurationBetweenTwoTimespan(item.StartTime.Value, item.EndTime.Value);
            //}

            object result = null;
            if (data != null)
            {
                result = JsonConvert.SerializeObject(data, Formatting.Indented,
                    new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            }
            _web.InvokeScript("getDataCallback", result);
        }

        private TimeSpan GetDurationBetweenTwoTimespan(TimeSpan startTime, TimeSpan endTime)
        {
            TimeSpan duration = new TimeSpan(endTime.Ticks - startTime.Ticks);
            return duration;
        }

        #endregion

        #region Statistics
        public void GetStatistics()
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

            object result = null;
            if (data != null)
            {
                result = JsonConvert.SerializeObject(data, Formatting.Indented,
                    new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            }
            _web.InvokeScript("getDataCallback", result);
        }
        #endregion

        #region Print UB
        public void GetAllUBlabels()
        {
            var dalUBlabels = new DAL_Labels();
            List<Trinity.BE.Label> data = dalUBlabels.GetAllLabelsForUB();

            object result = null;
            if (data.Count != 0)
            {
                result = JsonConvert.SerializeObject(data, Formatting.Indented,
                    new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            }
            else
            {
                getAlertsSendToDutyOfficer();
            }
            _web.InvokeScript("getDataCallback", result);
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
                        DrugType = "NA",
                        ReprintReason = reason,
                        IsMUB = false
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

                    _web.LoadPageHtml("PrintingTemplates/UBLabelTemplate .html", labelInfo);

                    Thread.Sleep(1500);
                }
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
        #endregion

        #region Print MUB And TT
        public void GetAllMUBAndTTlabels()
        {
            var dalMUBAndTTlabels = new DAL_Labels();
            List<Trinity.BE.Label> data = dalMUBAndTTlabels.GetAllLabelsForMUBAndTT();

            object result = null;
            if (data.Count != 0)
            {
                result = JsonConvert.SerializeObject(data, Formatting.Indented,
                    new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            }
            else
            {
                getAlertsSendToDutyOfficer();
            }
            _web.InvokeScript("getDataCallback", result);
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

        private void PrintMUBLabels_OnPrintMUBLabelSucceeded(object sender, PrintMUBAndTTLabelsSucceedEventArgs e)
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
                labelInfo.DrugType = ""; // Can dien thong tin DrugType khi print UB (chua biet drugtype la gi?)
            }

            DAL_Labels dalLabel = new DAL_Labels();
            if(dalLabel.UpdateLabel(labelInfo, labelInfo.UserId, labelInfo.Label_Type))
            {
                string message = "Print MUB for " + e.LabelInfo.Name + " successful.";
                MessageBox.Show(message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }            

            DeleteQRCodeImageFileTemp();
        }

        private void PrintMUBLabels_OnPrintMUBLabelFailed(object sender, CodeBehind.PrintMUBAndTTLabelsEventArgs e)
        {
            MessageBox.Show("Unable to print MUB/UB labels\nPlease report to the Duty Officer", "", MessageBoxButtons.OK, MessageBoxIcon.Error);

            DeleteQRCodeImageFileTemp();
        }

        private void PrintTTLabels_OnPrintTTLabelSucceeded(object sender, PrintMUBAndTTLabelsSucceedEventArgs e)
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
            MessageBox.Show("Unable to print TT labels\nPlease report to the Duty Officer", "", MessageBoxButtons.OK, MessageBoxIcon.Error);

            DeleteQRCodeImageFileTemp();
        }

        private void PrintMUBAndTTLabels_OnPrintTTLabelException(object sender, ExceptionArgs e)
        {
            MessageBox.Show(e.ErrorMessage, "", MessageBoxButtons.OK, MessageBoxIcon.Error);

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
