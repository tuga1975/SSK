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

        public void LoadProfile()
        {
            try
            {
                Trinity.Common.Session session = Trinity.Common.Session.Instance;
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
                    user.UserId = "supervisee";

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
                    //send notifiy to duty officer
                    APIUtils.SignalR.SendNotificationToDutyOfficer("Supervisee's information changed!", "Please check the Supervisee's information!");
                }
                else
                {
                    dalUserprofile.UpdateUserProfile(data.UserProfile, data.User.UserId, true);
                    //send notifiy to case officer
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

        public void LoadScanDocument()
        {
            try
            {
                LoadPage("Document.html");

            }
            catch (Exception)
            {

                LoadProfile();
            }
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

        public bool SubmitNRIC(string nric)
        {
            DAL_User dal_User = new DAL_User();
            var user = dal_User.GetUserByNRIC(nric, true);

            if (user == null)
            {
                // raise failsed event and return false
                return false;
            }

            return true;
        }
    }
}
