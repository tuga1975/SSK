
using Enrolment.CodeBehind.Authentication;
using Enrolment.Common;
using Enrolment.Contstants;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
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
using Trinity.Identity;

namespace Enrolment
{
    [System.Runtime.InteropServices.ComVisibleAttribute(true)]
    public class JSCallCS
    {
        private WebBrowser _web = null;
        private Type _thisType = null;

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

        public void SubmitNRIC(string strNRIC)
        {
            NRIC nric = NRIC.GetInstance(_web);
            nric.NRICAuthentication(strNRIC);
        }



        public void SearchSuperviseeByNRIC(string nric)
        {
            Session session = Session.Instance;
            var dalUser = new DAL_User();
            var dbUser = dalUser.GetSuperviseeByNRIC(nric, true);
            if (dbUser != null)
            {
                session[CommonConstants.SUPERVISEE] = dbUser;
            }
        }

        public void AddNewSupervisee() {
            _web.LoadPageHtml("New-Supervisee.html");
        }

        #region Authentication & Authorization

        public void Login(string username, string password)
        {
            EventCenter eventCenter = EventCenter.CreateEventCenter();

            UserManager<ApplicationUser> userManager = ApplicationIdentityManager.GetUserManager();
            ApplicationUser appUser = userManager.Find(username, password);
            if (appUser != null)
            {
                // Authenticated successfully
                // Check if the current user is an Enrolment Officer or not
                if (userManager.IsInRole(appUser.Id, EnumUserRoles.EnrolmentOfficer))
                {
                    // Authorized successfully
                    Trinity.BE.User user = new Trinity.BE.User()
                    {
                        Fingerprint = appUser.Fingerprint,
                        Name = appUser.Name,
                        NRIC = appUser.NRIC,
                        Role = EnumUserRoles.EnrolmentOfficer,
                        SmartCardId = appUser.SmartCardId,
                        Status = appUser.Status,
                        UserId = appUser.Id
                    };
                    Session session = Session.Instance;
                    session.IsUserNamePasswordAuthenticated = true;
                    session.Role = EnumUserRoles.EnrolmentOfficer;
                    session[CommonConstants.USER_LOGIN] = user;

                    eventCenter.RaiseLogInSucceededEvent();
                }
                else
                {
                    eventCenter.RaiseLogInFailedEvent(new LoginEventArgs(-2, "You do not have permission to access this page."));
                }
            }
            else
            {
                eventCenter.RaiseLogInFailedEvent(new LoginEventArgs(-1, "Your username or password is incorrect."));
            }
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
            EventCenter eventCenter = EventCenter.CreateEventCenter();
            eventCenter.RaiseLogOutCompletedEvent();
        }

        #endregion
    }
}
