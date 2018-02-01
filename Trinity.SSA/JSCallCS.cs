using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using SSA.CodeBehind.Authentication;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Trinity.Common;
using Trinity.Common.Common;
using Trinity.DAL;
using Trinity.Identity;

namespace SSA
{
    [System.Runtime.InteropServices.ComVisibleAttribute(true)]
    public class JSCallCS: JSCallCSBase
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
        
        public void SubmitNRIC(string strNRIC)
        {
            NRIC nric = NRIC.GetInstance(_web);
            nric.NRICAuthentication(strNRIC);
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

        public void MappingTemplateMUBAndTTLabels(string json)
        {
            var labelInfo = JsonConvert.DeserializeObject<LabelInfo>(json);
            _web.LoadPageHtml("PrintingTemplates/MUBLabelTemplate.html", labelInfo);
        }

        public void CallPrintingMUBAndTT(string jsonModel, string base64String)
        {
            string base64StringCanvas = base64String.Split(',')[1];
            byte[] bitmapBytes = Convert.FromBase64String(base64StringCanvas);

            var labelInfo = JsonConvert.DeserializeObject<LabelInfo>(jsonModel);
            labelInfo.BitmapLabel = bitmapBytes;
            _printMUBAndTTLabel.Start(labelInfo);
        }

        private void PrintMUBLabels_OnPrintMUBLabelSucceeded(object sender, PrintMUBAndTTLabelsSucceedEventArgs e)
        {
            var labelInfo = new Trinity.BE.Label
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
                LastStation = e.LabelInfo.LastStation,
                PrintCount = e.LabelInfo.PrintCount,
                ReprintReason = e.LabelInfo.ReprintReason
            };

            var dalLabel = new DAL_Labels();
            dalLabel.UpdateLabel(labelInfo, labelInfo.UserId, EnumLabelType.MUB);

            // Update queue status is finished
            var dalQueue = new DAL_QueueNumber();
            dalQueue.UpdateQueueStatusByUserId(labelInfo.UserId, EnumStations.SSA, EnumStations.UHP, EnumQueueStatuses.Finished, "Printer MUB/TT Label");

            this._web.RunScript("$('#WaitingSection').hide();$('#CompletedSection').show(); ; ");
            this._web.RunScript("$('.status-text').css('color','#000').text('Please collect your labels');");
            this._web.InvokeScript("countdownLogout");

            DeleteQRCodeImageFileTemp();
        }

        private void PrintMUBLabels_OnPrintMUBLabelFailed(object sender, CodeBehind.PrintMUBAndTTLabelsEventArgs e)
        {
            this._web.RunScript("$('#WaitingSection').hide();$('#CompletedSection').hide(); ; ");
            this._web.RunScript("$('.status-text').css('color','#000').text('Sent problem to Duty Officer. Please wait to check !');");
            MessageBox.Show("Unable to print MUB labels\nPlease report to the Duty Officer", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            APIUtils.SignalR.SendNotificationToDutyOfficer("Print MUB Label", "Don't print MUB, Please check !", NotificationType.Error, EnumStations.SSA);

            DeleteQRCodeImageFileTemp();
            LogOut();
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
            var update = dalLabel.UpdateLabel(labelInfo, labelInfo.UserId, EnumLabelType.TT);
            if (update)
            {
                var dalAppointment = new DAL_Appointments();
                var dalQueue = new DAL_QueueNumber();
                var appointment = dalAppointment.GetTodayAppointment(labelInfo.UserId);

                var sskQueue = new DAL_QueueNumber().GetQueueDetailByAppointent(appointment, EnumStations.SSK);

                dalQueue.UpdateQueueStatus(sskQueue.Queue_ID, EnumQueueStatuses.Finished, EnumStations.SSK);
                dalQueue.UpdateQueueStatus(sskQueue.Queue_ID, EnumQueueStatuses.Processing, EnumStations.SSA);
            }
            this._web.RunScript("$('#WaitingSection').hide();$('#CompletedSection').show(); ; ");
            this._web.RunScript("$('.status-text').css('color','#000').text('Please collect your labels');");

            DeleteQRCodeImageFileTemp();
            
        }

        private void PrintTTLabels_OnPrintTTLabelFailed(object sender, CodeBehind.PrintMUBAndTTLabelsEventArgs e)
        {
            this._web.RunScript("$('#WaitingSection').hide();$('#CompletedSection').hide(); ; ");
            this._web.RunScript("$('.status-text').css('color','#000').text('Sent problem to Duty Officer. Please wait to check !');");
            MessageBox.Show("Unable to print TT labels\nPlease report to the Duty Officer", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            APIUtils.SignalR.SendNotificationToDutyOfficer("Print TT Label", "Don't print MUB, Please check !", NotificationType.Error, EnumStations.SSA);

            DeleteQRCodeImageFileTemp();
            LogOut();
        }

        private void PrintMUBAndTTLabels_OnPrintTTLabelException(object sender, ExceptionArgs e)
        {
            this._web.RunScript("$('#WaitingSection').hide();$('#CompletedSection').hide(); ; ");
            this._web.RunScript("$('.status-text').css('color','#000').text('Sent problem to Duty Officer. Please wait to check !');");
            MessageBox.Show(e.ErrorMessage, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            APIUtils.SignalR.SendNotificationToDutyOfficer("MUB & TT", "Don't print MUB & TT, Please check !", NotificationType.Error, EnumStations.SSA);

            DeleteQRCodeImageFileTemp();
            LogOut();
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

        // Delete file image QRCode after print Supervisee particulars to avoid over memory. The image QRCode is auto generate to show on view SuperviseeParticulars
        public void DeleteQRCodeImageFileTemp()
        {
            Session session = Session.Instance;
            Trinity.BE.User user = (Trinity.BE.User)session[CommonConstants.SUPERVISEE];
            string fileName = String.Format("{0}/Temp/{1}", CSCallJS.curDir, "QRCode_" + user.NRIC + ".png");
            if (System.IO.File.Exists(fileName))
                System.IO.File.Delete(fileName);
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
