using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Trinity.DAL;

namespace SSK
{
    [System.Runtime.InteropServices.ComVisibleAttribute(true)]
    public class JSCallCS
    {
        private WebBrowser web = null;
        private Type thisType = null;

        public JSCallCS(WebBrowser web)
        {
            this.web = web;
            thisType = this.GetType();
        }

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

        public void LoadProfile(string userId)
        {
            try
            {
                var dalUser = new Trinity.DAL.DAL_User();
                var dalUserprofile = new Trinity.DAL.DAL_UserProfile();
                var profileModel = new Trinity.BE.ProfileModel
                {
                    User = dalUser.GetUserByUserId(userId, true),
                    UserProfile = dalUserprofile.GetUserProfileByUserId(userId, true),
                    Addresses = dalUserprofile.GetAddressByUserId(userId, true)

                };
                //profile model 

                web.LoadPageHtml("Profile.html", profileModel);
            }
            catch (Exception ex)
            {
                LoadPage("Supervisee.html");
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

                }
                else
                {
                    dalUserprofile.UpdateUserProfile(data.UserProfile, data.User.UserId, true);
                }

                //send notify to case officer
                //load Supervisee page 
                LoadPage("Supervisee.html");
            }
            catch (Exception)
            {
                LoadPage("Supervisee.html");
            }
        }

        public void ScanDocument()
        {
            //
            APIUtils.SignalR.SendNotificationToDutyOfficer("Hello Mr. Duty Officer!", "Hello Mr. Duty Officer! I'm a Supervisee");
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
    }
}
