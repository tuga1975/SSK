using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Trinity.Common;
using Trinity.DAL;

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
                    Trinity.BE.User currentUser = (Trinity.BE.User)session[CommonConstants.USER_LOGIN];

                    Trinity.BE.User supervisee = null;
                    if (currentUser.Role == EnumUserRoles.DutyOfficer)
                    {
                        supervisee = (Trinity.BE.User)session[CommonConstants.SUPERVISEE];
                    }
                    else
                    {
                        supervisee = currentUser;
                    }

                    // Update Queue SSA Processing
                    var dalQueue = new DAL_QueueNumber();
                    Trinity.DAL.DBContext.Queue dbQueue = dalQueue.UpdateQueueStatusByUserId(supervisee.UserId, EnumStation.SSK, EnumQueueStatuses.Processing, EnumStation.SSA, EnumQueueStatuses.Processing, "Printing MUB/TT labels", EnumQueueOutcomeText.Processing);

                    var lable = new DAL_Labels().GetByUserID(supervisee.UserId, EnumLabelType.MUB, DateTime.Today);

                    string markingNo = lable.MarkingNo;
                    var labelInfo = new LabelInfo
                    {
                        UserId = lable.UserId,
                        Name = lable.Name,
                        NRIC = lable.NRIC,
                        Label_Type = EnumLabelType.MUB,
                        Date = DateTime.Now.ToString("dd/MM/yyyy"),
                        CompanyName = lable.CompanyName,
                        LastStation = EnumStation.SSA,
                        MarkingNo = markingNo,
                        DrugType = "NA",
                        QRCode = lable.QRCode
                    };
                    using (var ms = new System.IO.MemoryStream(labelInfo.QRCode))
                    {
                        System.IO.Directory.CreateDirectory(String.Format("{0}/Temp", CSCallJS.curDir));
                        string fileName = String.Format("{0}/Temp/{1}", CSCallJS.curDir, "QRCode_" + supervisee.NRIC + ".png");
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
