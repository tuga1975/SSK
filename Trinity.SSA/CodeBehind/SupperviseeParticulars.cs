using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Trinity.Common;

namespace SSA.CodeBehind
{
    class SupperviseeParticulars
    {
        WebBrowser _web;

        public SupperviseeParticulars(WebBrowser web)
        {
            _web = web;
        }

        public void Start()
        {
            try
            {
                Session session = Session.Instance;
                if (session.IsAuthenticated)
                {
                    Trinity.BE.User user = (Trinity.BE.User)session[Constants.CommonConstants.USER_LOGIN];

                    var dalUser = new Trinity.DAL.DAL_User();
                    var dalUserprofile = new Trinity.DAL.DAL_UserProfile();
                    var profileModel = new Trinity.BE.ProfileModel
                    {
                        User = dalUser.GetUserByUserId(user.UserId, true),
                        UserProfile = dalUserprofile.GetUserProfileByUserId(user.UserId, true)
                    };
                    
                    //profile model 
                    _web.LoadPageHtml("SuperviseeParticulars.html", profileModel);
                }
            }
            catch (Exception ex)
            {
                _web.LoadPageHtml("SuperviseeParticulars.html", new Trinity.BE.ProfileModel());
            }
        }
    }
}
