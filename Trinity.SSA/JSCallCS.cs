using Newtonsoft.Json;
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
using Trinity.Common.Common;
using Trinity.DAL;
using Trinity.DAL.DBContext;

namespace SSA
{
    [System.Runtime.InteropServices.ComVisibleAttribute(true)]
    public class JSCallCS
    {
        private WebBrowser _web = null;
        private Type _thisType = null;
        private CodeBehind.PrintMUBAndTTLabels _printTTLabel;

        public event EventHandler<NRICEventArgs> OnNRICFailed;
        public event EventHandler<ShowMessageEventArgs> OnShowMessage;
        public event Action OnLogOutCompleted;


        public JSCallCS(WebBrowser web)
        {
            this._web = web;
            _thisType = this.GetType();
            _printTTLabel = new CodeBehind.PrintMUBAndTTLabels(web);
            _printTTLabel.OnPrintMUBAndTTLabelsSucceeded += PrintMUBAndTTLabels_OnPrintTTLabelSucceeded;
            _printTTLabel.OnPrintMUBAndTTLabelsFailed += PrintMUBAndTTLabels_OnPrintTTLabelFailed;
            _printTTLabel.OnPrintMUBAndTTLabelsException += PrintMUBAndTTLabels_OnPrintTTLabelException;
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

        public void PrintingMUBAndTTLabels()
        {
            _printTTLabel.Start();
        }

        private void PrintMUBAndTTLabels_OnPrintTTLabelSucceeded()
        {
            this._web.RunScript("$('#WaitingSection').hide();$('#CompletedSection').show(); ; ");
            this._web.RunScript("$('.status-text').css('color','#000').text('Please collect your labels');");
        }

        private void PrintMUBAndTTLabels_OnPrintTTLabelFailed(object sender, CodeBehind.PrintMUBAndTTLabelsEventArgs e)
        {
            this._web.RunScript("$('#WaitingSection').hide();$('#CompletedSection').hide(); ; ");
            MessageBox.Show("Unable to print labels\nPlease report to the Duty Officer", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            APIUtils.SignalR.SendNotificationToDutyOfficer("A supervisee can't print label", e.Message);
            this._web.RunScript("$('.status-text').css('color','#000').text('Sent problem to Duty Officer. Please wait to check !');");
        }

        private void PrintMUBAndTTLabels_OnPrintTTLabelException(object sender, ExceptionArgs e)
        {
            this._web.RunScript("$('#WaitingSection').hide();$('#CompletedSection').hide(); ; ");
            this._web.RunScript("$('.status-text').css('color','#000').text('Sent problem to Duty Officer. Please wait to check !');");
            MessageBox.Show(e.ErrorMessage, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            APIUtils.SignalR.SendNotificationToDutyOfficer("A supervisee can't print label", "Please check Supervisee's information!");
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
