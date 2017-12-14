using Newtonsoft.Json;
using SSK.Contstants;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Trinity.Common;
using Trinity.DAL;

namespace SSK
{
    [System.Runtime.InteropServices.ComVisibleAttribute(true)]
    public class JSCallCS
    {
        Trinity.Common.Session session = Trinity.Common.Session.Instance;
        private WebBrowser web = null;
        private Type thisType = null;

        public event EventHandler<NRICEventArgs> OnNRICFailed;
        public event EventHandler<ShowMessageEventArgs> OnShowMessage;

        public JSCallCS(WebBrowser web)
        {
            this.web = web;
            thisType = this.GetType();
        }

        #region virtual events
        // Wrap event invocations inside a protected virtual method
        // to allow derived classes to override the event invocation behavior
        protected virtual void RaiseOnNRICFailedEvent(NRICEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<NRICEventArgs> handler = OnNRICFailed;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Use the () operator to raise the event.
                handler(this, e);
            }
        }
        protected virtual void RaiseOnShowMessageEvent(ShowMessageEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<ShowMessageEventArgs> handler = OnShowMessage;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Use the () operator to raise the event.
                handler(this, e);
            }
        }

        #endregion

        public void LoadPage(string file)
        {
            web.LoadPageHtml(file);
        }

        public void LoadNotications()
        {
            DAL_Notification dalNotification = new DAL_Notification();
            List<Trinity.BE.Notification> myNotifications = dalNotification.GetMyNotifications("supervisee", false);
            var model = myNotifications;
            web.LoadPageHtml("Notication.html", myNotifications);
        }

        public void LoadProfile()
        {
            try
            {

                if (session.IsAuthenticated)
                {
                    Trinity.BE.User user = (Trinity.BE.User)session[Contstants.CommonConstants.USER_LOGIN];



                    var dalUser = new Trinity.DAL.DAL_User();
                    var dalUserprofile = new Trinity.DAL.DAL_UserProfile();
                    var profileModel = new Trinity.BE.ProfileModel
                    {
                        User = dalUser.GetUserByUserId(user.UserId, true),
                        UserProfile = dalUserprofile.GetUserProfileByUserId(user.UserId, true),
                        Addresses = dalUserprofile.GetAddressByUserId(user.UserId, true)

                    };
                    //profile model 

                    web.LoadPageHtml("Profile.html", profileModel);
                }
                //for testing purpose
                else
                {
                    Trinity.BE.User user = new Trinity.BE.User();
                    user.UserId = "a179195a-d502-4069-a7c0-9530613c961f";

                    var dalUser = new Trinity.DAL.DAL_User();
                    var dalUserprofile = new Trinity.DAL.DAL_UserProfile();
                    var profileModel = new Trinity.BE.ProfileModel
                    {
                        User = dalUser.GetUserByUserId(user.UserId, true),
                        UserProfile = dalUserprofile.GetUserProfileByUserId(user.UserId, true),
                        Addresses = dalUserprofile.GetAddressByUserId(user.UserId, true)

                    };
                    web.LoadPageHtml("Profile.html", profileModel);
                }

            }
            catch (Exception ex)
            {
                web.LoadPageHtml("Profile.html", new Trinity.BE.ProfileModel());
            }
        }

        public void SaveProfile(string param, bool primaryInfoChange)
        {
            try
            {
                var rawData = JsonConvert.DeserializeObject<Trinity.BE.ProfileRawMData>(param);
                var data = new Trinity.BE.ProfileRawMData().ToProfileModel(rawData);
                var dalUser = new Trinity.DAL.DAL_User();
                var dalUserprofile = new Trinity.DAL.DAL_UserProfile();
                if (primaryInfoChange)
                {

                    dalUser.UpdateUser(data.User, data.User.UserId, true);

                    dalUserprofile.UpdateUserProfile(data.UserProfile, data.User.UserId, true);
                    //send notifiy to duty officer
                    APIUtils.SignalR.SendNotificationToDutyOfficer("Supervisee's information changed!", "Please check the Supervisee's information!");
                }
                else
                {
                    dalUserprofile.UpdateUserProfile(data.UserProfile, data.User.UserId, true);
                    //send notifiy to case officer
                    APIUtils.SignalR.SendNotificationToDutyOfficer("Supervisee's information changed!", "Please check the Supervisee's information!");
                }

                //load Supervisee page 
                LoadPage("Supervisee.html");
            }
            catch (Exception)
            {
                LoadPage("Supervisee.html");
            }
        }

        public void LoadScanDocument(string jsonData)
        {
            try
            {

                session[Contstants.CommonConstants.PROFILE_DATA] = jsonData;

                LoadPage("Document.html");

            }
            catch (Exception)
            {
                MessageBox.Show("Something wrong happened!");
                LoadProfile();
            }
        }
        public void UpdateProfileAfterScanDoc()
        {
            var jsonData = session[Contstants.CommonConstants.PROFILE_DATA];
            SaveProfile(jsonData.ToString(), true);
        }


        private void actionThread(object pram)
        {

            var data = (object[])pram;
            var method = data[0].ToString();
            MethodInfo theMethod = thisType.GetMethod(method);
            theMethod.Invoke(this, (object[])data[1]);
            web.SetLoading(false);
        }
        public void ClientCallServer(string method, params object[] pram)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(actionThread), new object[] { method, pram });
        }

        public void SubmitNRIC(string nric)
        {
            DAL_User dal_User = new DAL_User();
            var user = dal_User.GetUserByNRIC(nric, true);

            if (user == null)
            {
                // raise failsed event and return false
                RaiseOnNRICFailedEvent(new NRICEventArgs("NRIC " + nric + ": not found. Please check NRIC again."));
                return;
            }


            // Create a session object to store UserLogin information
            Session session = Session.Instance;
            session[CommonConstants.USER_LOGIN] = user;


            web.LoadPageHtml("Supervisee.html");

            return;
        }

        public void GetQueue()
        {
            Session session = Session.Instance;
            Trinity.BE.User user = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];
            string queueNumber = "S" + user.NRIC.Substring(user.NRIC.Length - 5, 5).PadLeft(8, '*');
            RaiseOnShowMessageEvent(new ShowMessageEventArgs("Your queue is: " + queueNumber, "Queue Number", MessageBoxButtons.OK, MessageBoxIcon.Information));

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
    #endregion
}
