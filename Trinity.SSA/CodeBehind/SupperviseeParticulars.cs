﻿using System;
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

                    // Update Queue finish at SSK and start for SSA
                    var dalQueue = new DAL_QueueNumber();
                    Trinity.DAL.DBContext.Queue dbQueue = dalQueue.UpdateQueueStatusByUserId(supervisee.UserId, EnumStation.SSK, EnumQueueStatuses.Finished, EnumStation.SSA, EnumQueueStatuses.Processing, "Printing MUB/TT labels", EnumQueueOutcomeText.Processing);
                    if (dbQueue != null)
                    {
                        Trinity.SignalR.Client.Instance.QueueInserted(dbQueue.Queue_ID.ToString());
                    }

                    string markingNo = new DAL_Labels().GetMarkingNoByUserId(supervisee.UserId);
                    if(string.IsNullOrEmpty(markingNo))
                    {
                        markingNo = new DAL_SettingSystem().GenerateMarkingNumber();
                    }

                    var labelInfo = new LabelInfo
                    {
                        UserId = supervisee.UserId,
                        Name = supervisee.Name,
                        NRIC = supervisee.NRIC,
                        Label_Type = EnumLabelType.MUB,
                        Date = DateTime.Now.ToString("dd/MM/yyyy"),
                        CompanyName = CommonConstants.COMPANY_NAME,
                        LastStation = EnumStation.SSA,
                        MarkingNo = markingNo,
                        DrugType = "NA"
                    };

                    byte[] byteArrayQRCode = null;
                    byteArrayQRCode = CommonUtil.CreateLabelQRCode(labelInfo, "AESKey");
                    labelInfo.QRCode = byteArrayQRCode;

                    using (var ms = new System.IO.MemoryStream(byteArrayQRCode))
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
