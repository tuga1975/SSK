
using Enrolment.CodeBehind.Authentication;
using Enrolment.Common;
using Enrolment.Contstants;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
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

        #region Authentication & Authorization
        public void Login(string userName, string password)
        {
            UserManager<ApplicationUser> userManager = ApplicationIdentityManager.GetUserManager();
            ApplicationUser appUser = userManager.Find(userName, password);
            if (appUser != null)
            {
                // Authenticated successfully
                // Check if the logged-in user is a Enrolment Officer or not
                RoleManager<IdentityRole> roleManager = ApplicationIdentityManager.GetRoleManager();
                if (userManager.IsInRole(appUser.Id, EnumUserRoles.EnrolmentOfficer))
                {
                    Trinity.BE.User user = new Trinity.BE.User()
                    {
                        Fingerprint = appUser.Fingerprint,
                        Name = appUser.Name,
                        NRIC = appUser.NRIC,
                        Role = EnumUserRoles.EnrolmentOfficer,
                        UserId = appUser.Id,
                        SmartCardId = appUser.SmartCardId,
                        Status = appUser.Status
                    };
                    Session session = Session.Instance;
                    session.Role = EnumUserRoles.EnrolmentOfficer;
                    session.IsUserNamePasswordAuthenticated = true;
                    session[CommonConstants.USER_LOGIN] = user;

                    // Raise LoginSucceededEvent
                    EventCenter eventCenter = EventCenter.CreateEventCenter();
                    eventCenter.RaiseLogInSucceededEvent();
                }
                else
                {
                    // You do not have permission to access this page
                    // Raise LoginSucceededEvent
                    EventCenter eventCenter = EventCenter.CreateEventCenter();
                    eventCenter.RaiseLogInFailedEvent(new LoginEventArgs(-2, "You do not have permission to access this page."));
                }
            }
            else
            {
                // You do not have permission to access this page
                // Raise LoginSucceededEvent
                EventCenter eventCenter = EventCenter.CreateEventCenter();
                eventCenter.RaiseLogInFailedEvent(new LoginEventArgs(-1, "Your username or password is not correct."));
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
