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
                    Trinity.BE.User user = (Trinity.BE.User)session[CommonConstants.SUPERVISEE];
                    
                    var labelInfo = new LabelInfo
                    {
                        UserId = user.UserId,
                        Name = user.Name, 
                        NRIC = user.NRIC,
                        Label_Type = EnumLabelType.MUB,
                        Date = System.DateTime.Now,
                        CompanyName = CommonConstants.COMPANY_NAME,
                        LastStation = EnumStations.SSA,
                        MarkingNo = CommonUtil.GenerateMarkingNumber()
                    };

                    byte[] byteArrayQRCode = null;
                    byteArrayQRCode = CommonUtil.CreateLabelQRCode(labelInfo);
                    labelInfo.QRCode = byteArrayQRCode;

                    using (var ms = new System.IO.MemoryStream(byteArrayQRCode))
                    {
                        string fileName = String.Format("{0}/Temp/{1}", CSCallJS.curDir, "QRCode_" + user.NRIC + ".png");
                        if (System.IO.File.Exists(fileName))
                            System.IO.File.Delete(fileName);

                        var bitmap = System.Drawing.Image.FromStream(ms);
                        bitmap.Save(fileName, System.Drawing.Imaging.ImageFormat.Png);
                    }

                    //profile model 
                    _web.LoadPageHtml("SuperviseeParticulars.html", labelInfo);
                }
            }
            catch (Exception ex)
            {
                _web.LoadPageHtml("SuperviseeParticulars.html", new Trinity.BE.ProfileModel());
            }
        }
    }
}
