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
        #endregion

        public void LoadPopupBlock(string json)
        {
            var rawData = JsonConvert.DeserializeObject<object>(json);

            this._web.LoadPopupHtml("BlockedPopupDetail.html", rawData);
        }

        public void GetAllAppoinments()
        {
            var dalAppointment = new DAL_Appointments();
            List<Appointment> data = dalAppointment.GetAllAppointments();

            foreach(var item in data)
            {
                item.TimeSlot = GetDurationBetweenTwoTimespan(item.StartTime.Value, item.EndTime.Value);
            }

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

        public void LoadPopupMUBAndTTLabel(string json)
        {
            var rawdata = JsonConvert.DeserializeObject <Object>(json);
            this._web.LoadPopupHtml("MUBAndTTPopup.html", rawdata);
            
        }

        public void LoadPopupUBLabel(string json)
        {
            var rawdata = JsonConvert.DeserializeObject<Object>(json);
            this._web.LoadPopupHtml("UBlabelPopup.html", rawdata);
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
