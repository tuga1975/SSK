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

namespace DutyOfficer
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

        

        public void LoadPage(string file)
        {
            _web.LoadPageHtml(file);
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


        public void getAlertsSendToDutyOfficer()
        {
            var dalNotify = new DAL_Notification();

            List<Notification> data = dalNotify.GetNotificationsSentToDutyOfficer(true);
            //TODO : Should be remove dummy data
            if (data == null)
            {
                IFormatProvider culture = new CultureInfo("en-US", true);
                DateTime dateVal = DateTime.ParseExact("11/01/2018 14:31", "dd/MM/yyyy HH:mm", culture);

                data = new List<Notification>();
                Notification notification = new Notification();
                notification.Type = NotificationType.Error;
                notification.Content = "Out of wrapper";
                notification.Date = dateVal;
                notification.Subject = "EPS";

                Notification notification2 = new Notification();
                notification2.Type = NotificationType.Notification;
                notification2.Content = "Supervisee tested postive";
                notification2.Date = dateVal;
                notification2.Subject = "UHP";


                Notification notification3 = new Notification();
                notification3.Type = NotificationType.Caution;
                notification3.Content = "Out of plastic wrapper";
                notification3.Date = dateVal;
                notification3.Subject = "EPS";


                Notification notification4 = new Notification();
                notification4.Type = NotificationType.Notification;
                notification4.Content = "Rack A is ready for collection";
                notification4.Date = dateVal;
                notification4.Subject = "Result";

                Notification notification5 = new Notification();
                notification5.Type = NotificationType.Error;
                notification5.Content = "Leakage detected on UB1";
                notification5.Date = dateVal;
                notification5.Subject = "UHP";

                data.Add(notification);
                data.Add(notification2);
                data.Add(notification3);
                data.Add(notification4);
                data.Add(notification5);
            }

            object result = JsonConvert.SerializeObject(data, Formatting.Indented,
                new JsonSerializerSettings {ReferenceLoopHandling = ReferenceLoopHandling.Ignore});
            _web.InvokeScript("getDataCallback", result);
        }


        #region Queue
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
        #endregion

        public void LoadPopupBlock(string json)
        {
            var rawData = JsonConvert.DeserializeObject<object>(json);

            this._web.LoadPopupHtml("BlockedPopupDetail.html", rawData);
        }

        public void LoadPopupMUBAndTTLabel()
        {
            this._web.LoadPopupHtml("MUBAndTTPopup.html");
        }

        public void LoadPopupUBLabel()
        {
            this._web.LoadPopupHtml("UBPopup.html");
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
