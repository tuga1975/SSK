using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using ALK.CodeBehind.Authentication;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Trinity.Common;
using Trinity.Common.Common;
using Trinity.DAL;
using Trinity.Device.Util;
using Trinity.Identity;
using Trinity.Device;
using Trinity.Common.Utils;

namespace ALK
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
        //private string _currentAction = "PrintMUBAndTTLabel";
        //private string _nextAction = string.Empty;
        private LabelInfo _currentLabelInfo = null;
        private bool _mubApplicatorReady = false;
        private bool _ttApplicatorReady = false;
        private bool _mubApplicatorStarted = false;
        private bool _ttApplicatorStarted = false;
        private bool _mubIsPresent = false;
        private bool _ttIsPresent = false;
        private bool _mubIsRemoved = false;
        private bool _ttIsRemoved = false;
        private bool _mubDoorIsFullyClosed = false;
        private bool _ttDoorIsFullyClosed = false;
        private List<string> _errorLogs = new List<string>();
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

            //LEDStatusLightingUtil.Instance.OnNewEvent += Instance_OnNewEvent;
        }

        /// <summary>
        /// Write printing logs
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Instance_OnNewEvent(object sender, string e)
        {
            _errorLogs.Insert(0, e);
            while (_errorLogs.Count > 10)
            {
                _errorLogs.RemoveAt(10);
            }
            string errorLogs = string.Join("<br/>", _errorLogs.ToArray());
            this._web.RunScript("$('#printLogs').val('" + errorLogs + "');");
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
            session.IsFacialAuthenticated = false;
            session[CommonConstants.USER_LOGIN] = null;
            session[CommonConstants.PROFILE_DATA] = null;
            session[CommonConstants.SUPERVISEE] = null;

            RaiseLogOutCompletedEvent();
        }

        public void MappingTemplateMUBAndTTLabels(string json)
        {
            var labelInfo = JsonConvert.DeserializeObject<LabelInfo>(json);
            _web.LoadPageHtml("PrintingTemplates/MUBLabelTemplate.html", labelInfo);
        }

        private void PrintMUBLabels_OnPrintMUBLabelSucceeded(object sender, PrintMUBAndTTLabelsEventArgs e)
        {
            new DAL_Labels().UpdatePrinting(e.LabelInfo.UserId, EnumLabelType.MUB, EnumPrintStatus.Successful, EnumStation.ALK, DateTime.Today);
            _PrintMUBSucceed = true;
            Trinity.SignalR.Client.Instance.SSALabelPrinted(e.LabelInfo.UserId);
        }

        private void PrintMUBLabels_OnPrintMUBLabelFailed(object sender, PrintMUBAndTTLabelsEventArgs e)
        {
            try
            {

                new DAL_Labels().UpdatePrinting(e.LabelInfo.UserId, EnumLabelType.MUB, e.LabelInfo.PrintStatus, EnumStation.ALK, DateTime.Today);
                _PrintMUBSucceed = false;
                Trinity.SignalR.Client.Instance.SSALabelPrinted(e.LabelInfo.UserId);
                Trinity.SignalR.Client.Instance.SendToAppDutyOfficers(e.LabelInfo.UserId, "Cannot print MUB Label", "User '" + e.LabelInfo.Name + "' cannot print MUB label.", EnumNotificationTypes.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Trinity.ALK.JSCallCS.PrintMUBLabels_OnPrintMUBLabelFailed. Details:" + ex.Message);
            }
        }

        private void PrintTTLabels_OnPrintTTLabelSucceeded(object sender, PrintMUBAndTTLabelsEventArgs e)
        {
            new DAL_Labels().UpdatePrinting(e.LabelInfo.UserId, EnumLabelType.TT, EnumPrintStatus.Successful, EnumStation.ALK, DateTime.Today);
            Trinity.SignalR.Client.Instance.SSALabelPrinted(e.LabelInfo.UserId);
            _PrintTTSucceed = true;
        }

        private void PrintTTLabels_OnPrintTTLabelFailed(object sender, PrintMUBAndTTLabelsEventArgs e)
        {
            new DAL_Labels().UpdatePrinting(e.LabelInfo.UserId, EnumLabelType.TT, e.LabelInfo.PrintStatus, EnumStation.ALK, DateTime.Today);
            _PrintTTSucceed = false;
            Trinity.SignalR.Client.Instance.SSALabelPrinted(e.LabelInfo.UserId);
            Trinity.SignalR.Client.Instance.SendToAppDutyOfficers(e.LabelInfo.UserId, "Cannot print TT Label", "User '" + e.LabelInfo.Name + "' cannot print TT label.", EnumNotificationTypes.Error);
        }

        private void PrintMUBAndTTLabels_OnPrintTTLabelException(object sender, ExceptionArgs e)
        {
            _PrintMUBSucceed = false;
            _PrintTTSucceed = false;
            //this._web.RunScript("$('#WaitingSection').hide();$('#CompletedSection').hide(); ; ");
            //this._web.RunScript("$('#ttStatus').css('color','#000').text('Sent problem to Duty Officer. Please wait to check !');");
            //MessageBox.Show(e.ErrorMessage, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            var user = (Trinity.BE.User)Session.Instance[CommonConstants.USER_LOGIN];
            Trinity.SignalR.Client.Instance.SendToAppDutyOfficers(user.UserId, "Can not print MUB & TT Labels", "User " + user.Name + " cannot print MUB & TT labels, please check!", EnumNotificationTypes.Error);

            //DeleteQRCodeImageFileTemp();
            //LogOut();
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
                    try
                    {
                        string fileName = String.Format("{0}/Temp/{1}", CSCallJS.curDir, "QRCode_" + supervisee.NRIC + ".png");
                        if (System.IO.File.Exists(fileName))
                            System.IO.File.Delete(fileName);
                    }
                    catch
                    {
                    }
                }
            }
        }

        public void popupLoading(string content)
        {
            this._web.LoadPopupHtml("LoadingPopup.html", content);
        }

        public void OnPrintingAndLabellingCompleted()
        {
            if (_PrintMUBSucceed && _PrintTTSucceed)
            {
                // Update queue status is finished
                Session session = Session.Instance;
                Trinity.BE.User currentUser = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];
                if (currentUser == null)
                {
                    // Check why current user is null
                    this._web.RunScript("$('#mubStatus').css('color','#000').text('The current user is null');");
                    return;
                }
                //Trinity.BE.User supervisee = null;
                //if (currentUser.Role == EnumUserRoles.DutyOfficer)
                //{
                //    supervisee = (Trinity.BE.User)session[CommonConstants.SUPERVISEE];
                //}
                //else
                //{
                //    supervisee = currentUser;
                //}

                DeleteQRCodeImageFileTemp();

                CheckMUBPrintingLabellingProgress();
                CheckTTPrintingLabellingProgress();
                this._web.RunScript("$('#mubStatus').css('color','#000').text('MUB label is being printed and labelled...');");
                this._web.RunScript("$('#ttStatus').css('color','#000').text('TT label is being printed and labelled...');");
                this._web.RunScript("$('#ConfirmBtn').html('Waiting...');");
                this._web.RunScript("$('#lblNextAction').text('CheckIfMUBAndTTIsRemoved');");
            }
            else
            {
                _popupModel.Title = "Printing Failed";
                _popupModel.Message = "Unable to print labels.\nPlease report to the Duty Officer";
                _popupModel.IsShowLoading = false;
                _popupModel.IsShowOK = true;

                if (!_PrintMUBSucceed)
                {
                    _popupModel.Message = "Unable to print MUB label.\nPlease report to the Duty Officer";
                }
                if (!_PrintTTSucceed)
                {
                    _popupModel.Message = "Unable to print TT label.\nPlease report to the Duty Officer";
                }
                this._web.InvokeScript("showPopupModal", JsonConvert.SerializeObject(_popupModel));
            }
        }

        public void CallPrintingMUBAndTT(string action, string jsonModel, string base64String)
        {
            string base64StringCanvas = base64String.Split(',')[1];
            byte[] bitmapBytes = Convert.FromBase64String(base64StringCanvas);

            var labelInfo = JsonConvert.DeserializeObject<LabelInfo>(jsonModel);
            labelInfo.BitmapLabel = bitmapBytes;
            _printMUBAndTTLabel.Start(labelInfo);
        }

        public void ConfirmAction(string action, string json)
        {

            bool isPrinterConnected = true;
            if (!BarcodePrinterUtil.Instance.GetDeviceStatus(EnumDeviceNames.MUBLabelPrinter).Contains(EnumDeviceStatus.Connected))
            {
                isPrinterConnected = false;
                this._web.RunScript("$('#mubStatus').css('color','#000').text('Problem communicating with the MUB Printer.');");
            }
            if (!BarcodePrinterUtil.Instance.GetDeviceStatus(EnumDeviceNames.TTLabelPrinter).Contains(EnumDeviceStatus.Connected))
            {
                isPrinterConnected = false;
                this._web.RunScript("$('#ttStatus').css('color','#000').text('Problem communicating with the TT Printer.');");
            }

            if (isPrinterConnected)
            {
                this._web.RunScript("$('#divSave').hide();");
                //_currentAction = action;
                _currentLabelInfo = JsonConvert.DeserializeObject<LabelInfo>(json);
                if (action == "InitializeMUBAndTTApplicator")
                {
                    // Initialize MUB Applicator
                    InitializeMUBApplicator();
                    InitializeTTApplicator();
                }
                else if (action == "CheckIfMUBAndTTIsPresent")
                {
                    // MUB Applicator is ready
                    // Check if MUB is present or not
                    CheckIfMUBIsPresent();
                    CheckIfTTIsPresent();
                }
                else if (action == "StartMUBAndTTApplicator")
                {
                    CheckIfMUBIsPresent();
                    CheckIfTTIsPresent();
                }
                else if (action == "PrintMUBAndTTLabel")
                {
                    if (_mubApplicatorStarted && _ttApplicatorStarted && _currentLabelInfo != null)
                    {
                        // Start to print
                        StartToPrintMUBAndTTLabel(_currentLabelInfo);
                    }
                }
                else if (action == "CheckIfMUBAndTTIsRemoved")
                {
                    this._web.RunScript("$('#mubStatus').css('color','#000').text('Please remove MUB');");
                    this._web.RunScript("$('#ttStatus').css('color','#000').text('Please remove TT');");
                    CheckIfMUBIsRemoved();
                    CheckIfTTIsRemoved();
                }
                else if (action == "OpenMUBAndTTDoor")
                {
                    this._web.RunScript("$('#divSave').hide();");
                    OpenMUBDoor();
                    OpenTTDoor();
                }
            }
        }

        private void ShowTutorialVideos(bool placeOrRemove)
        {
            if (placeOrRemove)
            {
                this._web.RunScript("$('#placeMUBAndTT').show();");
                this._web.RunScript("$('#removeMUBAndTT').hide();");
            }
            else
            {
                this._web.RunScript("$('#placeMUBAndTT').hide();");
                this._web.RunScript("$('#removeMUBAndTT').show();");
            }
        }

        private void HideTutorialVideos()
        {
            this._web.RunScript("$('#placeMUBAndTT').hide();");
            this._web.RunScript("$('#removeMUBAndTT').hide();");
        }

        private void StartToPrintMUBAndTTLabel(LabelInfo labelInfo)
        {
            _web.LoadPageHtml("PrintingTemplates/MUBLabelTemplate.html", new object[] { "PrintMUBAndTTLabel", labelInfo });
        }

        #region MUB printing & labelling sample process

        private void InitializeMUBApplicator()
        {
            _mubApplicatorReady = false;
            LEDStatusLightingUtil.Instance.InitializeMUBApplicator_Async();
            LEDStatusLightingUtil.Instance.SendCommand_Async(EnumCommands.CheckIfMUBApplicatorIsReady, CheckIfMUBApplicatorIsReady_Callback);
        }

        private void InitializeTTApplicator()
        {
            _ttApplicatorReady = false;
            LEDStatusLightingUtil.Instance.InitializeTTApplicator_Async();
            LEDStatusLightingUtil.Instance.SendCommand_Async(EnumCommands.CheckIfTTApplicatorIsReady, CheckIfTTApplicatorIsReady_Callback);
        }

        private void CheckIfMUBIsPresent()
        {
            _mubIsPresent = false;
            //this._web.RunScript("$('#mubStatus').css('color','#000').text('Checking if the MUB Applicator is present...');");
            this._web.RunScript("$('#mubStatus').css('color','#000').text('Please place the MUB on the holder');");

            // Check if MUB is present or not
            LEDStatusLightingUtil.Instance.SendCommand_Async(EnumCommands.CheckIfMUBIsPresent, CheckIfMUBIsPresent_Callback);
        }

        private void CheckIfTTIsPresent()
        {
            _ttIsPresent = false;
            this._web.RunScript("$('#ttStatus').css('color','#000').text('Checking if the TT is present...');");

            // Check if MUB is present or not
            LEDStatusLightingUtil.Instance.SendCommand_Async(EnumCommands.CheckIfTTIsPresent, CheckIfTTIsPresent_Callback);
        }

        private void CheckIfMUBIsRemoved()
        {
            // Check if MUB is present or not
            LEDStatusLightingUtil.Instance.SendCommand_Async(EnumCommands.CheckIfMUBIsRemoved, CheckIfMUBIsRemoved_Callback);
        }

        private void CheckIfTTIsRemoved()
        {
            // Check if MUB is present or not
            LEDStatusLightingUtil.Instance.SendCommand_Async(EnumCommands.CheckIfTTIsRemoved, CheckIfTTIsRemoved_Callback);
        }

        private void StartMUBApplicator()
        {
            try
            {
                _mubApplicatorStarted = false;
                this._web.RunScript("$('#mubStatus').css('color','#000').text('The MUB Applicator is starting...');");
                this._web.RunScript("$('#ConfirmBtn').html('Starting Applicator...');");
                // Start MUB Applicator
                LEDStatusLightingUtil.Instance.StartMUBApplicator_Async();

                // Wait for 200 miliseconds and then check MUB Applicator status
                Thread.Sleep(200);
                LEDStatusLightingUtil.Instance.SendCommand_Async(EnumCommands.CheckIfMUBApplicatorIsStarted, CheckIfMUBApplicatorIsStarted_Callback);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in StartMUBApplicator. Details:" + ex.Message);
            }
        }

        private void OpenMUBDoor()
        {
            // Open MUB Door
            LEDStatusLightingUtil.Instance.OpenMUBDoor_Async();
            // Also open MUB holder
            LEDStatusLightingUtil.Instance.OpenMUBHolder_Async();
            // Wait for 200 miliseconds and then check if MUB Door is fully open
            Thread.Sleep(200);
            LEDStatusLightingUtil.Instance.SendCommand_Async(EnumCommands.CheckIfMUBDoorIsFullyOpen, CheckIfMUBDoorIsFullyOpen_Callback);
        }

        private void CloseMUBDoor()
        {
            LEDStatusLightingUtil.Instance.CloseMUBDoor_Async();

            // Wait for 200 miliseconds and then check MUB Door is fully closed
            Thread.Sleep(200);
            LEDStatusLightingUtil.Instance.SendCommand_Async(EnumCommands.CheckIfMUBDoorIsFullyClosed, CheckIfMUBDoorIsFullyClosed_Callback);
            //CheckIfMUBDoorIsFullyClosed();
        }

        private void CheckMUBPrintingLabellingProgress()
        {
            // We check MUB Printing and labelling progress by checking if the MUB Door is open or not.
            // If the Door is open, it means the printing and labelling is completed
            LEDStatusLightingUtil.Instance.SendCommand_Async(EnumCommands.CheckIfMUBDoorIsFullyOpen, CheckIfMUBDoorIsFullyOpen_Callback);
        }

        private void CheckIfMUBDoorIsFullyOpen_Callback(bool isFullyOpen)
        {
            if (isFullyOpen)
            {
                // Show tutorial videos to guide user how to remove MUB
                ShowTutorialVideos(false);

                this._web.RunScript("$('#mubStatus').css('color','#000').text('MUB was processed successfully. Please remove MUB.');");
                this._web.RunScript("$('#ConfirmBtn').html('Confirm to remove the MUB and TT');");
                this._web.RunScript("$('#lblNextAction').text('CheckIfMUBAndTTIsRemoved');");
                CheckIfMUBIsRemoved();
            }
            else
            {
                this._web.RunScript("$('#divSave').show();");
                this._web.RunScript("$('#CancelBtn').hide();");
                this._web.RunScript("$('#mubStatus').css('color','#000').text('Cannot complete printing and pasting MUB. Please report to Duty Officer');");
                this._web.RunScript("$('#ConfirmBtn').html('Open MUB and TT Door.');");
                this._web.RunScript("$('#lblNextAction').text('OpenMUBAndTTDoor');");
            }
        }

        private void CheckIfMUBApplicatorIsReady_Callback(bool isReady)
        {
            _mubApplicatorReady = isReady;
            if (isReady)
            {
                this._web.RunScript("$('#mubStatus').css('color','#000').text('MUB Applicator is ready.');");

                if (_ttApplicatorReady)
                {
                    // Show tutorial videos to guide user how to place MUB and TT
                    ShowTutorialVideos(true);

                    // MUB Applicator is ready
                    // Then verify presence of MUB
                    this._web.RunScript("$('#ConfirmBtn').html('Verify presence of MUB and TT.');");
                    // Set next action to 'CheckIfMUBIsPresent'
                    this._web.RunScript("$('#lblNextAction').text('CheckIfMUBAndTTIsPresent');");
                    CheckIfMUBIsPresent();
                    CheckIfTTIsPresent();
                    //CheckIfMUBAndTTArePresent();
                }
            }
            else
            {
                this._web.RunScript("$('#mubStatus').css('color','#000').text('MUB Applicator is not ready.');");
            }
        }

        private void CheckIfMUBApplicatorIsStarted_Callback(bool isStarted)
        {
            _mubApplicatorStarted = isStarted;
            if (isStarted)
            {
                // MUB Applicator is started. Ready to print
                this._web.RunScript("$('#mubStatus').css('color','#000').text('MUB label is ready to print.');");
                // Set next action to 'PrintMUBAndTTLabel'
                if (_currentLabelInfo != null & _ttApplicatorStarted)
                {
                    this._web.RunScript("$('#ConfirmBtn').html('Start to print MUB/TT Label');");
                    this._web.RunScript("$('#lblNextAction').text('PrintMUBAndTTLabel');");
                    //_nextAction = "PrintMUBAndTTLabel";
                    StartToPrintMUBAndTTLabel(_currentLabelInfo);
                }
            }
            else
            {
                this._web.RunScript("$('#mubStatus').css('color','#000').text('MUB Applicator cannot be started. Checking if the MUB Door is closed or not');");
                // InitializeMUBApplicator();

                // Follow suggestion from supplier
                // Check if MUB Door is fully closed
                LEDStatusLightingUtil.Instance.SendCommand_Async(EnumCommands.CheckIfMUBDoorIsFullyClosed, CheckIfMUBDoorIsFullyClosed_Callback2);
            }
        }

        private void CheckIfMUBDoorIsFullyClosed_Callback2(bool isFullyClosed)
        {
            _mubDoorIsFullyClosed = isFullyClosed;
            if (!_mubDoorIsFullyClosed)
            {
                this._web.RunScript("$('#mubStatus').css('color','#000').text('The MUB Door is not closed. Start sending command to close...');");

                // If the MUB Door is not fully closed
                // Then send command to close MUB Door
                LEDStatusLightingUtil.Instance.CloseMUBDoor_Async();

                // Wait for 200 miliseconds
                Thread.Sleep(200);

                this._web.RunScript("$('#mubStatus').css('color','#000').text('Start checking if the MUB Door is fully closed...');");
                LEDStatusLightingUtil.Instance.SendCommand_Async(EnumCommands.CheckIfMUBDoorIsFullyClosed, CheckIfMUBDoorIsFullyClosed_Callback2);
            }
            else
            {
                // If the MUB Door is fully closed, start MUB Applicator again
                this._web.RunScript("$('#mubStatus').css('color','#000').text('The MUB Door is fully closed. Trying to start MUB Applicator again...');");
                StartMUBApplicator();
            }
        }

        private void CheckIfTTApplicatorIsStarted_Callback(bool isStarted)
        {
            _ttApplicatorStarted = isStarted;
            if (isStarted)
            {
                // TT Applicator is started. Ready to print
                this._web.RunScript("$('#ttStatus').css('color','#000').text('TT label is ready to print.');");

                // Set next action to 'PrintMUBAndTTLabel'
                if (_currentLabelInfo != null && _mubApplicatorStarted)
                {
                    this._web.RunScript("$('#ConfirmBtn').html('Start to print MUB/TT Label');");
                    this._web.RunScript("$('#lblNextAction').text('PrintMUBAndTTLabel');");
                    StartToPrintMUBAndTTLabel(_currentLabelInfo);
                }
            }
            else
            {
                this._web.RunScript("$('#ttStatus').css('color','#000').text('TT Applicator cannot be started. Checking if the TT Door is closed or not');");
                // Try to re-initialize TT
                //InitializeTTApplicator();
                // Follow suggestion from supplier
                // Check if MUB Door is fully closed
                LEDStatusLightingUtil.Instance.SendCommand_Async(EnumCommands.CheckIfTTDoorIsFullyClosed, CheckIfTTDoorIsFullyClosed_Callback2);
            }
        }

        private void CheckIfTTDoorIsFullyClosed_Callback2(bool isFullyClosed)
        {
            _ttDoorIsFullyClosed = isFullyClosed;
            if (!_ttDoorIsFullyClosed)
            {
                this._web.RunScript("$('#ttStatus').css('color','#000').text('The TT Door is not closed. Start sending command to close...');");

                // If the TT Door is not fully closed
                // Then send command to close TT Door
                LEDStatusLightingUtil.Instance.CloseTTDoor_Async();

                // Wait for 200  milliseconds
                Thread.Sleep(200);

                this._web.RunScript("$('#ttStatus').css('color','#000').text('Start checking if the TT Door is fully closed...');");
                LEDStatusLightingUtil.Instance.SendCommand_Async(EnumCommands.CheckIfTTDoorIsFullyClosed, CheckIfTTDoorIsFullyClosed_Callback2);
            }
            else
            {
                // If the TT Door is fully closed, start TT Applicator again
                this._web.RunScript("$('#ttStatus').css('color','#000').text('The TT Door is fully closed. Trying to start TT Applicator again...');");
                StartTTApplicator();
            }
        }

        private void CheckIfMUBIsPresent_Callback(bool isPresent)
        {
            _mubIsPresent = isPresent;
            if (isPresent)
            {
                // MUB is placed on the holder
                this._web.RunScript("$('#mubStatus').css('color','#000').text('The MUB has been placed on the holder.');");

                // Set next action to "StartMUBApplicator"
                if (_ttIsPresent)
                {
                    // MUB and TT are present on the hold
                    // Hide tutorial videos
                    HideTutorialVideos();

                    this._web.RunScript("$('#ConfirmBtn').html('Starting Applicator...');");
                    this._web.RunScript("$('#lblNextAction').text('StartMUBAndTTApplicator');");
                    StartMUBApplicator();
                    StartTTApplicator();
                }
            }
            else
            {
                // Update on April 5th 2018, 15:28
                Thread.Sleep(500);
                // MUB is not present
                this._web.RunScript("$('#mubStatus').css('color','#000').text('The MUB is not present.');");
                // Sleep 500 miliseconds and then check MUB presence again
                Thread.Sleep(500);
                CheckIfMUBIsPresent();
            }
        }

        private void CheckIfMUBIsRemoved_Callback(bool isRemoved)
        {
            _mubIsRemoved = isRemoved;
            if (_mubIsRemoved)
            {
                this._web.RunScript("$('#mubStatus').css('color','#000').text('The MUB has been removed.');");

                if (_ttIsRemoved)
                {
                    CloseMUBDoor();
                    CloseTTDoor();
                }
            }
            else
            {
                // Update on April 5th 2018, 15:28
                Thread.Sleep(500);
                CheckIfMUBIsRemoved();
            }
        }

        private void CheckIfMUBDoorIsFullyClosed_Callback(bool isFullyClosed)
        {
            _mubDoorIsFullyClosed = isFullyClosed;
            if (isFullyClosed)
            {
                if (_ttDoorIsFullyClosed)
                {
                    CompletePrinting();
                }
            }
        }

        private void CompletePrinting()
        {
            // Hide all tutorial videos
            HideTutorialVideos();

            // Complete test. Remove queue number from Queue Monitor
            Session session = Session.Instance;
            Trinity.BE.User currentUser = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];
            if (currentUser == null)
            {
                // Check why current user is null
                this._web.RunScript("$('#mubStatus').css('color','#000').text('The current user is null');");
                this._web.RunScript("$('#ttStatus').css('color','#000').text('');");
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

            // Remove queue number and inform others
            var dalQueue = new DAL_QueueNumber();

            dalQueue.UpdateQueueStatusByUserId(supervisee.UserId, EnumStation.ARK, EnumQueueStatuses.Finished, EnumStation.ALK, EnumQueueStatuses.Processing, "Printing MUB/TT labels", EnumQueueOutcomeText.Processing);
            dalQueue.UpdateQueueStatusByUserId(supervisee.UserId, EnumStation.ALK, EnumQueueStatuses.Finished, EnumStation.SHP, EnumQueueStatuses.Processing, "Waiting for SHP", EnumQueueOutcomeText.Processing);

            Trinity.SignalR.Client.Instance.QueueCompleted(supervisee.UserId);
            Trinity.SignalR.Client.Instance.SSACompleted(supervisee.UserId);

            //lblStatus.Text = "The door is fully close";
            this._web.RunScript("$('#mubStatus').css('color','#000').text('MUB and TT Labels Printing Completed. Logging out...');");
            this._web.RunScript("$('#ttStatus').css('color','#000').text('');");

            //btnConfirm.Text = "Initialize MUB Applicator";
            this._web.RunScript("$('#ConfirmBtn').html('Logout');");

            //btnConfirm.Enabled = true;
            //this._web.RunScript("$('.ConfirmBtn').prop('disabled', false);");

            //btnConfirm.Tag = "0";
            this._web.RunScript("$('#lblNextAction').text('');");

            _mubApplicatorReady = false;
            _ttApplicatorReady = false;
            _mubApplicatorStarted = false;
            _ttApplicatorStarted = false;
            _mubIsPresent = false;
            _ttIsPresent = false;
            _mubIsRemoved = false;
            _ttIsRemoved = false;
            _mubDoorIsFullyClosed = false;
            _ttDoorIsFullyClosed = false;

            Thread.Sleep(2000);
            LogOut();
        }
        #endregion

        #region TT printing & labelling sample process

        private void StartTTApplicator()
        {
            _ttApplicatorStarted = false;
            this._web.RunScript("$('#ttStatus').css('color','#000').text('The TT Applicator is starting...');");
            this._web.RunScript("$('#ConfirmBtn').html('Starting Applicator...');");
            // Start MUB Applicator
            LEDStatusLightingUtil.Instance.StartTTApplicator_Async();
            // Wait for 200 miliseconds and then check TT Applicator status
            Thread.Sleep(200);
            LEDStatusLightingUtil.Instance.SendCommand_Async(EnumCommands.CheckIfTTApplicatorIsStarted, CheckIfTTApplicatorIsStarted_Callback);
        }

        private void OpenTTDoor()
        {
            LEDStatusLightingUtil.Instance.OpenTTDoor_Async();
            // Wait for 200 miliseconds and then check if TT Door is fully open
            Thread.Sleep(200);
            LEDStatusLightingUtil.Instance.SendCommand_Async(EnumCommands.CheckIfTTDoorIsFullyOpen, CheckIfTTDoorIsFullyOpen_Callback);
        }

        private void CloseTTDoor()
        {
            LEDStatusLightingUtil.Instance.CloseTTDoor_Async();
            // Wait for 200 miliseconds and then check if TT Door is fully closed
            Thread.Sleep(200);
            LEDStatusLightingUtil.Instance.SendCommand_Async(EnumCommands.CheckIfTTDoorIsFullyClosed, CheckIfTTDoorIsFullyClosed_Callback);
        }

        //int _retryCount = 0;
        private void CheckTTPrintingLabellingProgress()
        {
            // We check MUB Printing and labelling progress by checking if the MUB Door is open or not.
            // If the Door is open, it means the printing and labelling is completed
            LEDStatusLightingUtil.Instance.SendCommand_Async(EnumCommands.CheckIfTTDoorIsFullyOpen, CheckIfTTDoorIsFullyOpen_Callback);
        }

        private void CheckIfTTDoorIsFullyOpen_Callback(bool isFullyOpen)
        {
            if (isFullyOpen)
            {
                // Show tutorial videos to guide user how to remove TT
                ShowTutorialVideos(false);

                this._web.RunScript("$('#ttStatus').css('color','#000').text('TT was processed successfully. Please remove TT.');");
                this._web.RunScript("$('#ConfirmBtn').html('Confirm to remove the MUB and TT');");
                this._web.RunScript("$('#lblNextAction').text('CheckIfMUBAndTTIsRemoved');");
                CheckIfTTIsRemoved();
            }
            else
            {
                this._web.RunScript("$('#divSave').show();");
                this._web.RunScript("$('#CancelBtn').hide();");
                this._web.RunScript("$('#ttStatus').css('color','#000').text('Cannot complete printing and pasting TT. Please report to Duty Officer');");
                this._web.RunScript("$('#ConfirmBtn').html('Open MUB and TT Door.');");
                this._web.RunScript("$('#lblNextAction').text('OpenMUBAndTTDoor');");
            }
        }

        private void CheckIfTTApplicatorIsReady_Callback(bool isReady)
        {
            _ttApplicatorReady = isReady;
            if (_ttApplicatorReady)
            {
                this._web.RunScript("$('#ttStatus').css('color','#000').text('TT Applicator is ready.');");

                if (_mubApplicatorReady)
                {
                    // MUB Applicator is ready
                    // Then verify presence of MUB
                    this._web.RunScript("$('#ConfirmBtn').html('Verify presence of MUB and TT.');");
                    // Set next action to 'CheckIfMUBIsPresent'
                    this._web.RunScript("$('#lblNextAction').text('CheckIfMUBAndTTIsPresent');");

                    CheckIfMUBIsPresent();
                    CheckIfTTIsPresent();
                    //CheckIfMUBAndTTArePresent();
                }
            }
            else
            {
                this._web.RunScript("$('#ttStatus').css('color','#000').text('TT Applicator is not ready.');");
            }
        }


        private void CheckIfTTIsPresent_Callback(bool isPresent)
        {
            _ttIsPresent = isPresent;
            if (_ttIsPresent)
            {
                // TT is placed on the holder
                this._web.RunScript("$('#ttStatus').css('color','#000').text('The TT has been placed on the holder.');");

                // Set next action to "StartTTApplicator"
                if (_mubIsPresent)
                {
                    this._web.RunScript("$('#ConfirmBtn').html('Starting Applicator...');");
                    this._web.RunScript("$('#lblNextAction').text('StartMUBAndTTApplicator');");
                    StartMUBApplicator();
                    StartTTApplicator();
                }
            }
            else
            {
                // Update on April 5th 2018, 15:28
                Thread.Sleep(500);
                // TT is not present
                this._web.RunScript("$('#ttStatus').css('color','#000').text('The TT is not present.');");
                // Sleep 500 miliseconds and then check MUB presence again
                Thread.Sleep(500);
                CheckIfTTIsPresent();
            }
        }

        private void CheckIfTTIsRemoved_Callback(bool isRemoved)
        {
            _ttIsRemoved = isRemoved;
            if (_ttIsRemoved)
            {
                this._web.RunScript("$('#ttStatus').css('color','#000').text('The TT has been removed.');");

                if (_mubIsRemoved)
                {
                    CloseMUBDoor();
                    CloseTTDoor();
                }
            }
            else
            {
                // Update on April 5th 2018, 15:28
                Thread.Sleep(500);
                CheckIfTTIsRemoved();
            }
        }

        private void CheckIfTTDoorIsFullyClosed_Callback(bool isFullyClosed)
        {
            _ttDoorIsFullyClosed = isFullyClosed;
            if (_ttDoorIsFullyClosed)
            {
                if (_mubDoorIsFullyClosed)
                {
                    CompletePrinting();
                }
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
