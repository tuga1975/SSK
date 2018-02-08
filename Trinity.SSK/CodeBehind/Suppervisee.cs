using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Trinity.Common;

namespace SSK.CodeBehind
{
    class Suppervisee
    {
        WebBrowser _web;

        public Suppervisee(WebBrowser web)
        {
            _web = web;
        }

        public void Start()
        {
            // load page
            _web.LoadPageHtml("Supervisee.html");



            // get user login info
            Session session = Session.Instance;
            Trinity.BE.User user = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];
            List<Trinity.BE.Notification> myNotifications = new Trinity.DAL.DAL_Notification().GetAllNotifications(user.UserId);
            if (myNotifications != null)
            {
                var unReadCount = myNotifications.Count;
                APIUtils.LayerWeb.Invoke((System.Windows.Forms.MethodInvoker)(() =>
                {
                    APIUtils.LayerWeb.PushNoti(unReadCount);
                }));
            }
            //// if user login is dutyofficer, implement duty officer override
            //if (user.Role == EnumUserRoles.DutyOfficer)
            //{
            //    CSCallJS.DisplayNRICLogin(_web);
            //}
        }
    }
}
