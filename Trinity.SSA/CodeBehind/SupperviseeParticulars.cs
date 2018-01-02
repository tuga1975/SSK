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

                    var dalUserprofile = new Trinity.DAL.DAL_UserProfile();
                    var userProfile = dalUserprofile.GetUserProfileByUserId(user.UserId, true);

                    var userInfo = new Trinity.Common.UserInfo
                    {
                        UserName = user.Name,
                        NRIC = user.NRIC,
                        DOB = userProfile.DOB.HasValue ? userProfile.DOB.Value.ToString("dd/MM/yyyy") : "",
                        Status = user.Status,
                        MarkingNumber = CommonUtil.GenerateMarkingNumber()
                    };

                    byte[] byteArrayQRCode = null;
                    if (userProfile.QRCode != null)
                        byteArrayQRCode = userProfile.QRCode;
                    else
                        byteArrayQRCode = CommonUtil.CreateQRCode(userInfo);
                    
                    // test qr code bang file image, chua goi printer
                    using (var ms = new System.IO.MemoryStream(byteArrayQRCode))
                    {
                        string fileName = String.Format("{0}/View/img/{1}", CSCallJS.curDir, "QRCode_" + user.NRIC + ".png");
                        if (System.IO.File.Exists(fileName))
                            System.IO.File.Delete(fileName);

                        var bitmap = System.Drawing.Image.FromStream(ms);
                        bitmap.Save(fileName, System.Drawing.Imaging.ImageFormat.Png);
                    }

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
