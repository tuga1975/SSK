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
using Trinity.Device.Util;
using Trinity.Identity;

namespace SSA
{
    [System.Runtime.InteropServices.ComVisibleAttribute(true)]
    public class JSCallCS : JSCallCSBase
    {

        private CodeBehind.PrintMUBAndTTLabels _printMUBAndTTLabel;

        public event EventHandler<NRICEventArgs> OnNRICFailed;
        public event EventHandler<ShowMessageEventArgs> OnShowMessage;
        public event Action OnLogOutCompleted;
        private static bool _PrintMUBSucceed = false;
        private static bool _PrintTTSucceed = false;
        private Trinity.BE.PopupModel _popupModel;
        private string _currentPrintingStatus = "3";

        public JSCallCS(WebBrowser web)
        {
            this._web = web;
            _thisType = this.GetType();
            _printMUBAndTTLabel = new CodeBehind.PrintMUBAndTTLabels(this);
            _printMUBAndTTLabel.OnPrintMUBLabelsSucceeded += PrintMUBLabels_OnPrintMUBLabelSucceeded;
            _printMUBAndTTLabel.OnPrintMUBLabelsFailed += PrintMUBLabels_OnPrintMUBLabelFailed;
            _printMUBAndTTLabel.OnPrintTTLabelsSucceeded += PrintTTLabels_OnPrintTTLabelSucceeded;
            _printMUBAndTTLabel.OnPrintTTLabelsFailed += PrintTTLabels_OnPrintTTLabelFailed;
            _printMUBAndTTLabel.OnPrintMUBAndTTLabelsException += PrintMUBAndTTLabels_OnPrintTTLabelException;
            _popupModel = new Trinity.BE.PopupModel();
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

        public void CallPrintingMUBAndTT(string printingStatus, string jsonModel, string base64String)
        {
            string base64StringCanvas = base64String.Split(',')[1];
            byte[] bitmapBytes = Convert.FromBase64String(base64StringCanvas);

            var labelInfo = JsonConvert.DeserializeObject<LabelInfo>(jsonModel);
            labelInfo.BitmapLabel = bitmapBytes;
            _printMUBAndTTLabel.Start(labelInfo);
            //MessageBox.Show("Check printing status:" + printingStatus);
            if (printingStatus == "3")
            {
                this._web.RunScript("$('.status-text').css('color','#000').text('Print completed. Please remove the MUB.');");
                this._web.RunScript("$('#ConfirmBtn').html('Confirm to remove the MUB');");
                this._web.RunScript("$('#lblPrintingStatus').text('4');");
            }
        }

        private void PrintMUBLabels_OnPrintMUBLabelSucceeded(object sender, PrintMUBAndTTLabelsEventArgs e)
        {
            try
            {
                _PrintMUBSucceed = true;
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
                dalLabel.UpdateLabel(labelInfo);

                //// Update queue status is finished
                //var dalQueue = new DAL_QueueNumber();
                //dalQueue.UpdateQueueStatusByUserId(labelInfo.UserId, EnumStation.SSA, EnumStation.UHP, EnumQueueStatuses.Finished, "Printer MUB/TT Label");

                //DeleteQRCodeImageFileTemp();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in Trinity.SSA.JSCallCS.PrintMUBLabels_OnPrintMUBLabelSucceeded. Details:" + ex.Message);
            }
        }

        private void PrintMUBLabels_OnPrintMUBLabelFailed(object sender, PrintMUBAndTTLabelsEventArgs e)
        {
            try
            {
                _PrintMUBSucceed = false;
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
                    ReprintReason = e.LabelInfo.ReprintReason,
                    PrintStatus = e.LabelInfo.PrintStatus,
                    Message = e.LabelInfo.Message
                };

                var dalLabel = new DAL_Labels();
                dalLabel.UpdateLabel(labelInfo);

                Trinity.SignalR.Client.Instance.SendToAllDutyOfficers(e.LabelInfo.UserId, "Print MUB Label", "Don't print MUB, Please check !", EnumNotificationTypes.Error);

                //DeleteQRCodeImageFileTemp();
                //LogOut();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Trinity.SSA.JSCallCS.PrintMUBLabels_OnPrintMUBLabelFailed. Details:" + ex.Message);
            }
        }

        private void PrintTTLabels_OnPrintTTLabelSucceeded(object sender, PrintMUBAndTTLabelsEventArgs e)
        {
            try
            {
                _PrintTTSucceed = true;
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
                dalLabel.UpdateLabel(labelInfo);
            }
            catch (Exception ex)
            {

            }
        }

        private void PrintTTLabels_OnPrintTTLabelFailed(object sender, PrintMUBAndTTLabelsEventArgs e)
        {
            try
            {
                _PrintTTSucceed = false;
                var labelInfo = new Trinity.BE.Label
                {
                    UserId = e.LabelInfo.UserId,
                    Label_Type = EnumLabelType.TT,
                    CompanyName = e.LabelInfo.CompanyName,
                    MarkingNo = e.LabelInfo.MarkingNo,
                    DrugType = e.LabelInfo.DrugType,
                    NRIC = e.LabelInfo.NRIC,
                    Name = e.LabelInfo.Name,
                    Date = DateTime.Now,
                    QRCode = e.LabelInfo.QRCode,
                    LastStation = e.LabelInfo.LastStation,
                    PrintCount = e.LabelInfo.PrintCount,
                    ReprintReason = e.LabelInfo.ReprintReason,
                    PrintStatus = e.LabelInfo.PrintStatus,
                    Message = e.LabelInfo.Message
                };

                var dalLabel = new DAL_Labels();
                dalLabel.UpdateLabel(labelInfo);

                Trinity.SignalR.Client.Instance.SendToAllDutyOfficers(e.LabelInfo.UserId, "Print TT Label", "Don't print TT, Please check !", EnumNotificationTypes.Error);

                //DeleteQRCodeImageFileTemp();
                //LogOut();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in Trinity.SSA.JSCallCS.PrintTTLabels_OnPrintTTLabelFailed. Details:" + ex.Message);
            }
        }

        private void PrintMUBAndTTLabels_OnPrintTTLabelException(object sender, ExceptionArgs e)
        {
            _PrintMUBSucceed = false;
            _PrintTTSucceed = false;
            //this._web.RunScript("$('#WaitingSection').hide();$('#CompletedSection').hide(); ; ");
            //this._web.RunScript("$('.status-text').css('color','#000').text('Sent problem to Duty Officer. Please wait to check !');");
            //MessageBox.Show(e.ErrorMessage, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Trinity.SignalR.Client.Instance.SendToAllDutyOfficers(((Trinity.BE.User)Session.Instance[CommonConstants.USER_LOGIN]).UserId, "MUB & TT", "Don't print MUB & TT, Please check !", EnumNotificationTypes.Error);

            //DeleteQRCodeImageFileTemp();
            //LogOut();
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
            Trinity.BE.User currentUser = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];
            if (currentUser != null)
            {
                Trinity.BE.User supervisee = null;
                if (currentUser.Role == EnumUserRoles.DutyOfficer)
                {
                    supervisee = (Trinity.BE.User)session[CommonConstants.SUPERVISEE];
                }
                else
                {
                    supervisee = currentUser;
                }
                if (supervisee != null)
                {
                    string fileName = String.Format("{0}/Temp/{1}", CSCallJS.curDir, "QRCode_" + supervisee.NRIC + ".png");
                    if (System.IO.File.Exists(fileName))
                        System.IO.File.Delete(fileName);
                }
            }
        }

        public void popupLoading(string content)
        {
            this._web.LoadPopupHtml("LoadingPopup.html", content);
        }

        public void OnEventPrintFinished()
        {
            if (_PrintMUBSucceed && _PrintTTSucceed)
            {
                // Update queue status is finished
                Session session = Session.Instance;
                Trinity.BE.User currentUser = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];
                if (currentUser == null)
                {
                    // Check why current user is null
                    this._web.RunScript("$('.status-text').css('color','#000').text('The current user is null');");
                    return;
                }
                Trinity.BE.User supervisee = null;
                if (currentUser.Role == EnumUserRoles.DutyOfficer)
                {
                    supervisee = (Trinity.BE.User)session[CommonConstants.SUPERVISEE];
                }
                else
                {
                    supervisee = currentUser;
                }
                var dalQueue = new DAL_QueueNumber();
                dalQueue.UpdateQueueStatusByUserId(supervisee.UserId, EnumStation.SSA, EnumQueueStatuses.Finished, EnumStation.UHP, EnumQueueStatuses.Waiting, "", EnumQueueOutcomeText.Processing);
                //Trinity.SignalR.Client.Instance.QueueCompleted(currentUser.UserId);

                //this._web.LoadPageHtml("PrintingMUBAndTTLabels.html");
                //this._web.RunScript("$('#WaitingSection').hide();$('#CompletedSection').show(); ; ");
                //this._web.RunScript("$('.status-text').css('color','#000').text('Please collect your labels');");
                //this._web.InvokeScript("countdownLogout");

                DeleteQRCodeImageFileTemp();
                
                //LogOut();
            }
            else
            {
                _popupModel.Title = "Printing Failed";
                _popupModel.Message = "Unable to print labels.\nPlease report to the Duty Officer";
                _popupModel.IsShowLoading = false;
                _popupModel.IsShowOK = true;

                if (_PrintTTSucceed)
                {
                    _popupModel.Message = "Unable to print MUB labels.\nPlease report to the Duty Officer";
                }
                if (_PrintMUBSucceed)
                {
                    _popupModel.Message = "Unable to print TT labels.\nPlease report to the Duty Officer";
                }
                this._web.InvokeScript("showPopupModal", JsonConvert.SerializeObject(_popupModel));
            }
        }

        #region MUB printing & labelling sample process

        public void ConfirmAction(string printingStatus, string json)
        {
            _currentPrintingStatus = printingStatus;
            if (printingStatus == "0")
            {
                LEDStatusLightingUtil.Instance.MUBAutoFlagApplicatorReadyOK += Instance_MUBAutoFlagApplicatorReadyOK;
                LEDStatusLightingUtil.Instance.InitializeMUBApplicator();

                //btnConfirm.Enabled = false;
                //this._web.RunScript("$('.ConfirmBtn').prop('disabled', true);");
            }
            else if (printingStatus == "1")
            {
                LEDStatusLightingUtil.Instance.MUBStatusUpdated += Instance_MUBStatusUpdated;
                LEDStatusLightingUtil.Instance.VerifyMUBPresence();

                //btnConfirm.Enabled = false;
                this._web.RunScript("$('.ConfirmBtn').prop('disabled', true);");
            }
            else if (printingStatus == "2")
            {
                LEDStatusLightingUtil.Instance.MUBReadyToPrint += Instance_MUBReadyToPrint;
                LEDStatusLightingUtil.Instance.StartMUBApplicator();

                //btnConfirm.Enabled = false;
                this._web.RunScript("$('.ConfirmBtn').prop('disabled', true);");
            }
            else if (printingStatus == "3")
            {
                // Start to print
                var labelInfo = JsonConvert.DeserializeObject<LabelInfo>(json);
                _web.LoadPageHtml("PrintingTemplates/MUBLabelTemplate.html", new object[] { printingStatus, labelInfo });

                LEDStatusLightingUtil.Instance.MUBReadyToRemove += Instance_MUBReadyToRemove;
                LEDStatusLightingUtil.Instance.CheckMUBApplicatorFinishStatus();
                ////btnConfirm.Enabled = false;
                //this._web.RunScript("$('.ConfirmBtn').prop('disabled', true);");
                ////btnConfirm.Text = "Check printing status";
                //this._web.RunScript("$('.ConfirmBtn').text('Check printing status.');");
            }
            else if (printingStatus == "4")
            {
                // Verify Supervisee remove the MUB before close the door.
                //lblStatus.Text = "You haven't removed the MUB. Please remove it";
                this._web.RunScript("$('.status-text').css('color','#000').text('Please remove MUB/TT');");
                LEDStatusLightingUtil.Instance.MUBDoorFullyClosed += Instance_MUBDoorFullyClosed;
                LEDStatusLightingUtil.Instance.CheckIfMUBRemoved();

                //btnConfirm.Enabled = false;
                //this._web.RunScript("$('.ConfirmBtn').prop('disabled', true);");
            }
        }

        private void Instance_MUBAutoFlagApplicatorReadyOK(object sender, string e)
        {
            LEDStatusLightingUtil.Instance.MUBAutoFlagApplicatorReadyOK -= Instance_MUBAutoFlagApplicatorReadyOK;
            this._web.RunScript("$('.status-text').css('color','#000').text('Please place the MUB on the holder.');");
            //btnConfirm.Enabled = true;
            //this._web.RunScript("$('.ConfirmBtn').prop('disabled', false);");
            //btnConfirm.Text = "Verify presence of MUB";
            this._web.RunScript("$('#ConfirmBtn').html('Verify presence of MUB.');");

            this._web.RunScript("$('#lblPrintingStatus').text('1');");
            //btnConfirm.Tag = "1";
        }

        private void Instance_MUBStatusUpdated(object sender, string e)
        {
            LEDStatusLightingUtil.Instance.MUBStatusUpdated -= Instance_MUBStatusUpdated;
            if (e == "1")
            {
                // MUB is placed on the holder
                //lblStatus.Text = "Supervisee has placed the MUB on the holder";
                this._web.RunScript("$('.status-text').css('color','#000').text('The MUB has been placed on the holder.');");

                //btnConfirm.Enabled = true;
                //this._web.RunScript("$('#ConfirmBtn').prop('disabled', false);");

                //btnConfirm.Text = "Start Applicator";
                this._web.RunScript("$('#ConfirmBtn').html('Start Applicator.');");

                this._web.RunScript("$('#lblPrintingStatus').text('2');");
                //btnConfirm.Tag = "2";
            }
            else if (e == "0")
            {
                // MUB is not present
                this._web.RunScript("$('.status-text').css('color','#000').text('The MUB is not present.');");

                //btnConfirm.Enabled = true;
                //this._web.RunScript("$('#ConfirmBtn').prop('disabled', false);");
            }
        }

        private void Instance_MUBReadyToPrint(object sender, string e)
        {
            LEDStatusLightingUtil.Instance.MUBReadyToPrint -= Instance_MUBReadyToPrint;
            //lblStatus.Text = "Ready to print";
            this._web.RunScript("$('.status-text').css('color','#000').text('Ready to print.');");

            //btnConfirm.Enabled = true;
            //this._web.RunScript("$('#ConfirmBtn').prop('disabled', false);");

            //btnConfirm.Text = "Start to print MUB/TT Label";
            this._web.RunScript("$('#ConfirmBtn').html('Start to print MUB/TT Label');");

            this._web.RunScript("$('#lblPrintingStatus').text('3');");
            //btnConfirm.Tag = "3";
        }

        private void Instance_MUBReadyToRemove(object sender, string e)
        {
            LEDStatusLightingUtil.Instance.MUBReadyToRemove -= Instance_MUBReadyToRemove;
            //lblStatus.Text = "Print completed. Please remove the MUB";
            this._web.RunScript("$('.status-text').css('color','#000').text('Print completed. Please remove the MUB.');");

            //btnConfirm.Text = "Confirm to remove the MUB";
            this._web.RunScript("$('#ConfirmBtn').html('Confirm to remove the MUB');");

            //btnConfirm.Enabled = true;
            //this._web.RunScript("$('#ConfirmBtn').prop('disabled', false);");

            this._web.RunScript("$('#lblPrintingStatus').text('4');");
            //btnConfirm.Tag = "4";
        }

        private void Instance_MUBDoorFullyClosed(object sender, string e)
        {
            // Complete test. Remove queue number from Queue Monitor
            Session session = Session.Instance;
            Trinity.BE.User currentUser = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];
            if (currentUser == null)
            {
                // Check why current user is null
                this._web.RunScript("$('.status-text').css('color','#000').text('The current user is null');");
                return;
            }
            // Remove queue number and inform others
            //new DAL_QueueDetails().RemoveQueueFromSSK(currentUser.UserId);
            Trinity.SignalR.Client.Instance.QueueCompleted(currentUser.UserId);

            LEDStatusLightingUtil.Instance.MUBDoorFullyClosed -= Instance_MUBDoorFullyClosed;
            //lblStatus.Text = "The door is fully close";
            this._web.RunScript("$('.status-text').css('color','#000').text('MUB and TT Labels Printing Completed.');");

            //btnConfirm.Text = "Initialize MUB Applicator";
            this._web.RunScript("$('#ConfirmBtn').html('Logout');");

            //btnConfirm.Enabled = true;
            //this._web.RunScript("$('.ConfirmBtn').prop('disabled', false);");

            //btnConfirm.Tag = "0";
            this._web.RunScript("$('#lblPrintingStatus').text('-1');");

            Thread.Sleep(300);
            LogOut();
        }

        #endregion
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
