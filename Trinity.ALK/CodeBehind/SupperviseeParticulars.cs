using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Trinity.Common;
using Trinity.DAL;

namespace ALK.CodeBehind
{
    class SupperviseeParticulars
    {
        WebBrowser _web;
        private JSCallCS _jsCallCS;
        private Main _main;
        public SupperviseeParticulars(WebBrowser web, JSCallCS _jsCallCS, Main _main)
        {
            this._web = web;
            this._jsCallCS = _jsCallCS;
            this._main = _main;
        }

        public void Start()
        {
            Trinity.BE.User supervisee = _jsCallCS.getSuperviseeLogin();
            // Update Queue ALK Processing
            var dalQueue = new DAL_QueueNumber();

            var myQueue = dalQueue.GetMyQueueToday(supervisee.UserId);
            if (myQueue == null)
            {
                _web.ShowMessage("Please register for a queue number at " + EnumStation.ARK + " first.");
                _jsCallCS.LogOut();
            }
            else if (myQueue != null && myQueue.QueueDetails.Any(d => d.Station == EnumStation.ARK && d.Status == EnumQueueStatuses.Waiting))
            {
                _web.ShowMessage("Please wait for your queue number to appear under the \"Now Serving\" list.");
                _jsCallCS.LogOut();
            }
            else
            {
                dalQueue.UpdateQueueStatusByUserId(supervisee.UserId, EnumStation.ARK, EnumQueueStatuses.Processing, EnumStation.ALK, EnumQueueStatuses.Processing, "Printing MUB/TT labels", EnumQueueOutcomeText.Processing);
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
                    LastStation = EnumStation.ALK,
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
                _main._isPrintingMUBTT = false;
                if (_main._timerCheckLogout.Enabled)
                    _main._timerCheckLogout.Stop();
                _main._timerCheckLogout.Start();
            }
        }
    }
}
