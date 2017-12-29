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
                    Trinity.BE.User user = (Trinity.BE.User)session[Constants.CommonConstants.SUPERVISEE];

                    var dalUser = new Trinity.DAL.DAL_User();
                    var dalUserprofile = new Trinity.DAL.DAL_UserProfile();
                    
                    var userInfo = new Trinity.Common.UserInfo
                    {
                        UserName = user.Name,
                        NRIC = user.NRIC,
                        DOB = dalUserprofile.GetUserProfileByUserId(user.UserId, true).DOB.HasValue ? dalUserprofile.GetUserProfileByUserId(user.UserId, true).DOB.Value.ToString("dd/MM/yyyy") : "",
                        Status = user.Status, //dalUserprofile.GetUserProfileByUserId(user.UserId, true).Maritial_Status,
                        MarkingNumber = CommonUtil.GenerateMarkingNumber()
                    };
                    
                    var bitmap = CommonUtil.CreateQRCode(userInfo, "AESKey");
                    // save to stream as PNG  
                    // test qr code bang file image, chua goi printer
                    string fileName = String.Format("{0}/View/img/{1}",CSCallJS.curDir, "QRCode.png");
                    if (System.IO.File.Exists(fileName))
                        System.IO.File.Delete(fileName);
                    bitmap.Save(fileName, System.Drawing.Imaging.ImageFormat.Png);


                    //profile model 
                    _web.LoadPageHtml("SuperviseeParticulars.html", userInfo);
                }
            }
            catch (Exception ex)
            {
                _web.LoadPageHtml("SuperviseeParticulars.html", new Trinity.BE.ProfileModel());
            }
        }
    }
}
