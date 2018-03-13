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
        private string _currentAction = "PrintMUBLabel";
        private LabelInfo _currentMUBLabelInfo = null;

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

                Trinity.SignalR.Client.Instance.SendToAllDutyOfficers(e.LabelInfo.UserId, "Cannot print MUB Label", "User '" + labelInfo.UserId + "' cannot print MUB label.", EnumNotificationTypes.Error);

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
                var update = dalLabel.UpdateLabel(labelInfo);
                if (update != null)
                {
                    var dalAppointment = new DAL_Appointments();
                    var dalQueue = new DAL_QueueNumber();
                    var appointment = dalAppointment.GetTodayAppointmentByUserId(labelInfo.UserId);
                    //var appointment = result.Data;

                    if (appointment != null)
                    {
                        var sskQueue = new DAL_QueueNumber().GetQueueDetailByAppointment(appointment, EnumStation.SSK);

                        dalQueue.UpdateQueueStatus(sskQueue.Queue_ID, EnumQueueStatuses.Finished, EnumStation.SSK);
                        dalQueue.UpdateQueueStatus(sskQueue.Queue_ID, EnumQueueStatuses.Processing, EnumStation.SSA);
                    }
                    //this._web.RunScript("$('#WaitingSection').hide();$('#CompletedSection').show(); ; ");
                    //this._web.RunScript("$('.status-text').css('color','#000').text('Please collect your labels');");

                    //DeleteQRCodeImageFileTemp();
                }
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

                Trinity.SignalR.Client.Instance.SendToAllDutyOfficers(e.LabelInfo.UserId, "Cannot print TT Label", "User '" + labelInfo.UserId + "' cannot print TT label.", EnumNotificationTypes.Error);

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
                dalQueue.UpdateQueueStatusByUserId(supervisee.UserId, EnumStation.SSA, EnumQueueStatuses.Finished, EnumStation.UHP, EnumQueueStatuses.Processing, "", EnumQueueOutcomeText.Processing);
                //this._web.LoadPageHtml("PrintingMUBAndTTLabels.html");
                //this._web.RunScript("$('#WaitingSection').hide();$('#CompletedSection').show(); ; ");
                //this._web.RunScript("$('.status-text').css('color','#000').text('Please collect your labels');");
                //this._web.InvokeScript("countdownLogout");

                DeleteQRCodeImageFileTemp();

                CheckMUBPrintingLabellingProgress();
                this._web.RunScript("$('.status-text').css('color','#000').text('Printing and labelling is in progress...');");
                this._web.RunScript("$('#ConfirmBtn').html('Waiting...');");
                this._web.RunScript("$('#lblNextAction').text('CheckIfMUBIsRemoved');");
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

        public void CallPrintingMUBAndTT(string action, string jsonModel, string base64String)
        {
            string base64StringCanvas = base64String.Split(',')[1];
            byte[] bitmapBytes = Convert.FromBase64String(base64StringCanvas);

            var labelInfo = JsonConvert.DeserializeObject<LabelInfo>(jsonModel);
            labelInfo.BitmapLabel = bitmapBytes;
            _printMUBAndTTLabel.Start(labelInfo);
            //if (action == "PrintMUBLabel")
            //{
            //    CheckMUBApplicatorFinishStatus();
            //    this._web.RunScript("$('.status-text').css('color','#000').text('Print completed. Please remove the MUB.');");
            //    this._web.RunScript("$('#ConfirmBtn').html('Confirm to remove the MUB');");
            //    this._web.RunScript("$('#lblNextAction').text('CheckIfMUBIsRemoved');");
            //}
        }

        public void ConfirmAction(string action, string json)
        {
            _currentAction = action;
            _currentMUBLabelInfo = JsonConvert.DeserializeObject<LabelInfo>(json);
            if (action == "InitializeMUBApplicator")
            {
                // MUB Applicator is not ready
                LEDStatusLightingUtil.Instance.MUBApplicatorStatusUpdated += InitializeMUBApplicator_Handler;
                LEDStatusLightingUtil.Instance.InitializeMUBApplicator();
                //this._web.RunScript("$('.ConfirmBtn').prop('disabled', true);");
            }
            else if (action == "CheckIfMUBIsPresent")
            {
                // MUB Applicator is ready
                // Check if MUB is present or not
                CheckIfMUBIsPresent();
            }
            else if (action == "StartMUBApplicator")
            {
                // MUB is placed
                // Start MUB Applicator
                StartMUBApplicator();
            }
            else if (action == "PrintMUBLabel")
            {
                // Start to print
                StartToPrintMUBLabel(_currentMUBLabelInfo);
            }
            else if (action == "CheckIfMUBIsRemoved")
            {
                this._web.RunScript("$('.status-text').css('color','#000').text('Please remove MUB/TT');");
                LEDStatusLightingUtil.Instance.MUBStatusUpdated += CheckIfMUBIsRemoved_Callback;
                LEDStatusLightingUtil.Instance.VerifyMUBPresence();
            }
            else if (action == "OpenMUBDoor")
            {
                LEDStatusLightingUtil.Instance.MUBDoorStatusChanged += OpenMUBDoor_Callback;
                LEDStatusLightingUtil.Instance.OpenMUBDoor();
            }
        }

        private void CheckIfMUBIsPresent()
        {
            // Check if MUB is present or not
            LEDStatusLightingUtil.Instance.MUBStatusUpdated += CheckIfMUBIsPresent_Callback;
            LEDStatusLightingUtil.Instance.VerifyMUBPresence();
        }

        private void CheckIfMUBIsRemoved()
        {
            // Check if MUB is present or not
            LEDStatusLightingUtil.Instance.MUBStatusUpdated += CheckIfMUBIsRemoved_Callback;
            LEDStatusLightingUtil.Instance.VerifyMUBPresence();
        }

        private void StartMUBApplicator()
        {
            // Start MUB Applicator
            LEDStatusLightingUtil.Instance.MUBApplicatorStatusUpdated += StartMUBApplicator_Callback;
            LEDStatusLightingUtil.Instance.StartMUBApplicator();
        }

        private void CheckIfMUBDoorIsOpen()
        {

        }
        private void StartToPrintMUBLabel(LabelInfo labelInfo)
        {
            _web.LoadPageHtml("PrintingTemplates/MUBLabelTemplate.html", new object[] { "PrintMUBLabel", labelInfo });
        }

        //int _retryCount = 0;
        private void CheckMUBPrintingLabellingProgress()
        {
            // We check MUB Printing and labelling progress by checking if the MUB Door is open or not.
            // If the Door is open, it means the printing and labelling is completed
            LEDStatusLightingUtil.Instance.MUBDoorStatusChanged += CheckMUBPrintingLabellingProgress_Callback;
            LEDStatusLightingUtil.Instance.CheckDoorOpenStatus();
        }

        private void OpenMUBDoor_Callback(object sender, EnumMUBDoorStatus status)
        {
            LEDStatusLightingUtil.Instance.MUBDoorStatusChanged += OpenMUBDoor_Callback;
            if (status == EnumMUBDoorStatus.FullyOpen)
            {
                this._web.RunScript("$('.status-text').css('color','#000').text('MUB Door is open. Please remove MUB.');");
                this._web.RunScript("$('#ConfirmBtn').html('Confirm to remove the MUB');");
                this._web.RunScript("$('#lblNextAction').text('CheckIfMUBIsRemoved');");
                CheckIfMUBIsRemoved();
            }
            else
            {
                this._web.RunScript("$('.status-text').css('color','#000').text('Cannot complete printing and pasting MUB. Please report to Duty Officer');");
                this._web.RunScript("$('#ConfirmBtn').html('Open MUB Door.');");
                this._web.RunScript("$('#lblNextAction').text('OpenMUBDoor');");
            }
        }

        private void CheckMUBPrintingLabellingProgress_Callback(object sender, EnumMUBDoorStatus status)
        {
            LEDStatusLightingUtil.Instance.MUBDoorStatusChanged -= CheckMUBPrintingLabellingProgress_Callback;
            if (status == EnumMUBDoorStatus.FullyOpen)
            {
                this._web.RunScript("$('.status-text').css('color','#000').text('MUB was processed successfully. Please remove MUB.');");
                this._web.RunScript("$('#ConfirmBtn').html('Confirm to remove the MUB');");
                this._web.RunScript("$('#lblNextAction').text('CheckIfMUBIsRemoved');");
                CheckIfMUBIsRemoved();
            }
            else
            {
                this._web.RunScript("$('.status-text').css('color','#000').text('Cannot complete printing and pasting MUB. Please report to Duty Officer');");
                this._web.RunScript("$('#ConfirmBtn').html('Open MUB Door.');");
                this._web.RunScript("$('#lblNextAction').text('OpenMUBDoor');");
            }
        }


        //private void CheckMUBApplicatorFinishStatus()
        //{
        //    LEDStatusLightingUtil.Instance.MUBApplicatorStatusUpdated += CheckMUBApplicatorFinishStatus_Callback;
        //    LEDStatusLightingUtil.Instance.CheckMUBApplicatorFinishStatus();
        //}

        //private void CheckMUBApplicatorFinishStatus_Callback(object sender, EnumMUBApplicatorStatus status)
        //{
        //    LEDStatusLightingUtil.Instance.MUBApplicatorStatusUpdated += CheckMUBApplicatorFinishStatus_Callback;
        //    if (status == EnumMUBApplicatorStatus.Finished)
        //    {
        //        _retryCount = 0;
        //        this._web.RunScript("$('.status-text').css('color','#000').text('Ket thuc roi');");
        //        this._web.RunScript("$('#ConfirmBtn').html('Confirm to remove the MUB');");
        //        this._web.RunScript("$('#lblNextAction').text('CheckIfMUBIsRemoved');");
        //    }
        //    else
        //    {
        //        if (_retryCount == 100)
        //        {
        //            _retryCount = 0;
        //            this._web.RunScript("$('.status-text').css('color','#000').text('Chua ket thuc :(');");
        //            this._web.RunScript("$('#ConfirmBtn').html('Confirm to remove the MUB');");
        //            this._web.RunScript("$('#lblNextAction').text('CheckIfMUBIsRemoved');");
        //        }
        //        else
        //        {
        //            _retryCount++;
        //            Thread.Sleep(200);
        //            CheckMUBApplicatorFinishStatus();
        //        }
        //    }
        //}

        private void InitializeMUBApplicator_Handler(object sender, EnumMUBApplicatorStatus status)
        {
            LEDStatusLightingUtil.Instance.MUBApplicatorStatusUpdated -= InitializeMUBApplicator_Handler;
            if (status == EnumMUBApplicatorStatus.Ready)
            {
                // MUB Applicator is ready
                // Then verify presence of MUB
                this._web.RunScript("$('#ConfirmBtn').html('Verify presence of MUB.');");
                // Set next action to 'CheckIfMUBIsPresent'
                this._web.RunScript("$('#lblNextAction').text('CheckIfMUBIsPresent');");
                CheckIfMUBIsPresent();
            }
            else if (status == EnumMUBApplicatorStatus.NotReady)
            {
                this._web.RunScript("$('.status-text').css('color','#000').text('MUB Applicator is not ready.');");
            }
        }

        private void StartMUBApplicator_Callback(object sender, EnumMUBApplicatorStatus status)
        {
            LEDStatusLightingUtil.Instance.MUBApplicatorStatusUpdated -= StartMUBApplicator_Callback;
            if (status == EnumMUBApplicatorStatus.Started)
            {
                // MUB Applicator is started. Ready to print
                this._web.RunScript("$('.status-text').css('color','#000').text('Ready to print.');");
                this._web.RunScript("$('#ConfirmBtn').html('Start to print MUB/TT Label');");
                // Set next action to 'PrintMUBLabel'
                this._web.RunScript("$('#lblNextAction').text('PrintMUBLabel');");
                if (_currentMUBLabelInfo != null)
                {
                    StartToPrintMUBLabel(_currentMUBLabelInfo);
                }
            }
            else if (status == EnumMUBApplicatorStatus.NotReady)
            {
                this._web.RunScript("$('.status-text').css('color','#000').text('MUB Applicator is not ready.');");
            }
        }

        private void CheckIfMUBIsPresent_Callback(object sender, EnumMUBStatus status)
        {
            LEDStatusLightingUtil.Instance.MUBStatusUpdated -= CheckIfMUBIsPresent_Callback;
            if (status == EnumMUBStatus.Placed)
            {
                // MUB is placed on the holder
                this._web.RunScript("$('.status-text').css('color','#000').text('The MUB has been placed on the holder.');");
                this._web.RunScript("$('#ConfirmBtn').html('Start Applicator.');");
                // Set next action to "StartMUBApplicator"
                this._web.RunScript("$('#lblNextAction').text('StartMUBApplicator');");
                StartMUBApplicator();
            }
            else
            {
                // MUB is not present
                this._web.RunScript("$('.status-text').css('color','#000').text('The MUB is not present.');");
            }
        }


        private void CheckIfMUBIsRemoved_Callback(object sender, EnumMUBStatus status)
        {
            LEDStatusLightingUtil.Instance.MUBStatusUpdated -= CheckIfMUBIsRemoved_Callback;
            if (status == EnumMUBStatus.Removed)
            {
                // If MUB is removed then close the MUB Door
                LEDStatusLightingUtil.Instance.MUBDoorStatusChanged += CloseMUBDoor_Callback;
                LEDStatusLightingUtil.Instance.CloseMUBDoor();
            }
        }

        private void CloseMUBDoor_Callback(object sender, EnumMUBDoorStatus status)
        {
            LEDStatusLightingUtil.Instance.MUBDoorStatusChanged -= CloseMUBDoor_Callback;

            if (status == EnumMUBDoorStatus.FullyClosed)
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

                //lblStatus.Text = "The door is fully close";
                this._web.RunScript("$('.status-text').css('color','#000').text('MUB and TT Labels Printing Completed. Logging out...');");

                //btnConfirm.Text = "Initialize MUB Applicator";
                this._web.RunScript("$('#ConfirmBtn').html('Logout');");

                //btnConfirm.Enabled = true;
                //this._web.RunScript("$('.ConfirmBtn').prop('disabled', false);");

                //btnConfirm.Tag = "0";
                this._web.RunScript("$('#lblNextAction').text('');");

                Thread.Sleep(2000);
                LogOut();
            }
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
