using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using Newtonsoft.Json;
using Trinity.Common;

namespace Trinity.BackendAPI.Controllers
{
    public class SSPNotificationModel
    {
        public string Type { get; set; }
        public string Content { get; set; }
        public DateTime Datetime { get; set; }
        public string notification_code { get; set; }
        public string NRIC { get; set; }
    }

    public class SSPTransactionModel
    {
        public string Type { get; set; }
        public string Content { get; set; }
        public DateTime Datetime { get; set; }
        public string transaction_code { get; set; }
        public string NRIC { get; set; }
    }
    public class SSPCompleteModel
    {
        public string NRIC { get; set; }
    }

    public class SSPDrugResultsModel
    {
        public string NRIC { get; set; }
        public DateTime? Datetime { get; set; }
        public string MarkingNumber { get; set; }
        public bool AMPH { get; set; }
        public bool OPI { get; set; }
        public bool COCA { get; set; }
        public bool LSD { get; set; }
        public bool MTQL { get; set; }
        public bool KET { get; set; }
        public bool CAT { get; set; }
        public bool NPS { get; set; }
        public bool BENZ { get; set; }
        public bool THC { get; set; }
        public bool BARB { get; set; }
        public bool METH { get; set; }
        public bool PCP { get; set; }
        public bool BUPRE { get; set; }
        public bool PPZ { get; set; }
        public bool? IsSealed { get; set; }
        public string SealedOrDiscardedBy { get; set; }
        public DateTime? SealedOrDiscardedDate { get; set; }
    }

    public class SSPCaseOfficerModel
    {
        public string Name { get; set; }

    }

    public class SSPAuthenticateModel
    {
        public string Name { get; set; }
        public bool Found { get; set; }
        public byte[] Right { get; set; }
        public byte[] Left { get; set; }
        public string Message { get; set; }
    }
    public class SSPVerifyDutyOfficerModel
    {
        public string ID { get; set; }
        public string Password { get; set; }
    }

    [Route("api/SSP/{Action}")]
    public class SSPController : ApiController
    {

        [HttpPost]
        [ResponseType(typeof(bool))]
        public IHttpActionResult SSPVerifyDutyOfficer([FromBody]SSPVerifyDutyOfficerModel data)
        {
            try
            {
                var dalUser = new DAL.DAL_User();
                ApplicationUser appUser = dalUser.Login(data.ID, data.Password);
                if (appUser != null)
                {
                    if (dalUser.IsInRole(appUser.Id, EnumUserRoles.DutyOfficer))
                    {
                        return Ok(true);
                    }
                }
            }
            catch (Exception)
            {

            }
            return Ok(false);
        }

        [HttpPost]
        [ResponseType(typeof(bool))]
        public async System.Threading.Tasks.Task<IHttpActionResult> SSPPostNotification([FromBody]SSPNotificationModel data)
        {
            try
            {
                Trinity.DAL.DBContext.Membership_Users user = null;
                if (!string.IsNullOrEmpty(data.NRIC))
                {
                    user = new DAL.DAL_User().GetByNRIC(data.NRIC);
                }
                string Subject = data.Content;
                if (Subject.Length > 100)
                {
                    Subject = Subject.Substring(0, 96) + " ...";
                }
                string IDNoti = new DAL.DAL_Notification().InsertNotification(user != null ? user.UserId : null, null, Subject, data.Content, false, data.Datetime, data.notification_code, data.Type, EnumStation.SSP);
                if (!string.IsNullOrEmpty(IDNoti))
                {
                    if (data.Type == EnumNotificationTypes.Error && user != null)
                    {
                        new DAL.DAL_QueueNumber().UpdateQueueStatusByUserId(user.UserId, EnumStation.SSP, EnumQueueStatuses.Errors, EnumStation.DUTYOFFICER, EnumQueueStatuses.Finished, EnumMessage.LeakageDeletected, EnumQueueOutcomeText.Processing);

                        await System.Threading.Tasks.Task.Run(() => Trinity.SignalR.Client.Instance.BackendAPISend(NotificationNames.SSP_ERROR, data.NRIC));
                    }
                    await System.Threading.Tasks.Task.Run(() => Trinity.SignalR.Client.Instance.SendToAppDutyOfficers((user != null ? user.UserId : null), Subject, data.Content, data.Type, EnumStation.SSP, false, IDNoti));
                    //return Ok(IDNoti);
                    return Ok(true);
                }
                else
                {
                    //return Ok(string.Empty);
                    return Ok(false);
                }
            }
            catch (Exception)
            {
                return Ok(false);
            }
        }

        [HttpPost]
        [ResponseType(typeof(bool))]
        public IHttpActionResult SSPPostTransaction([FromBody]SSPTransactionModel data)
        {
            try
            {
                if (new DAL.DAL_Transactions().Insert(data.NRIC, EnumStation.SSP, data.Type, data.Content, data.Datetime, data.transaction_code) != Guid.Empty)
                {
                    return Ok(true);
                }
                else
                {
                    return Ok(false);
                }
            }
            catch (Exception)
            {
                return Ok(false);
            }
        }

        [HttpGet]
        [ResponseType(typeof(SSPAuthenticateModel))]
        public IHttpActionResult SSPAuthenticate(string NRIC, string SmartCardUID)
        {
            Trinity.DAL.DBContext.Membership_Users user = new DAL.DAL_User().GetByNRIC(NRIC);
            if (user == null)
            {
                return Ok(new SSPAuthenticateModel()
                {
                    Found = false,
                    Message = "NRIC " + NRIC + " is not registered"
                });
            }
            if (string.IsNullOrEmpty(SmartCardUID))
            {
                return Ok(new SSPAuthenticateModel()
                {
                    Name = user.Name,
                    Found = true,
                    Right = user.RightThumbFingerprint,
                    Left = user.LeftThumbFingerprint,
                    Message = "Success"
                });
            }
            var smartCard = new DAL.DAL_IssueCard().GetIssueCardBySmartCardId(SmartCardUID);
            if (smartCard == null)
            {
                return Ok(new SSPAuthenticateModel()
                {
                    Found = false,
                    Message = "Your smart card does not exist"
                });
            }
            else if (smartCard.Status == EnumIssuedCards.Inactive)
            {
                return Ok(new SSPAuthenticateModel()
                {
                    Found = false,
                    Message = "Your smart card does not work"
                });
            }
            else if (smartCard.Status == EnumIssuedCards.Active && smartCard.Expired_Date < DateTime.Today)
            {
                return Ok(new SSPAuthenticateModel()
                {
                    Found = false,
                    Message = "Your smart card has expired"
                });
            }

            if (user.UserId != smartCard.UserId)
            {
                return Ok(new SSPAuthenticateModel()
                {
                    Found = false,
                    Message = "Smart cards are not yours"
                });
            }
            else
            {
                return Ok(new SSPAuthenticateModel()
                {
                    Name = user.Name,
                    Found = true,
                    Right = user.RightThumbFingerprint,
                    Left = user.LeftThumbFingerprint,
                    Message = "Success"
                });
            }
        }

        [HttpGet]
        [ResponseType(typeof(SSPCaseOfficerModel))]
        public IHttpActionResult SSPGetOfficerName(string NRIC)
        {
            Trinity.DAL.DBContext.Membership_Users user = new DAL.DAL_User().GetByNRIC(NRIC);
            if (user != null && user.Membership_UserRoles != null && user.Membership_UserRoles.Any(d => d.Membership_Roles != null &&
            (d.Membership_Roles.Name == EnumUserRoles.CaseOfficer || d.Membership_Roles.Name == EnumUserRoles.DutyOfficer || d.Membership_Roles.Name == EnumUserRoles.EnrolmentOfficer)))
            {
                return Ok(new SSPCaseOfficerModel() { Name = user.Name });
            }
            else
            {
                return Ok();
            }
        }

        [HttpGet]
        [ResponseType(typeof(SSPDrugResultsModel))]
        [Route("api/SSP/SSPGetDrugResults")]
        public IHttpActionResult SSPGetDrugResultsByUploadedDate(string NRIC, DateTime uploadedDate)
        {
            Trinity.DAL.DBContext.DrugResult result = new DAL.DAL_DrugResults().GetByNRICAndUploadedDate(NRIC, uploadedDate);
            if (result == null)
            {
                return Ok();
            }
            else
            {
                return Ok(new SSPDrugResultsModel()
                {
                    NRIC = result.NRIC,
                    Datetime = result.timestamp,
                    MarkingNumber = result.markingnumber,
                    AMPH = result.AMPH.GetValueOrDefault(false),
                    OPI = result.OPI.GetValueOrDefault(false),
                    COCA = result.COCA.GetValueOrDefault(false),
                    LSD = result.LSD.GetValueOrDefault(false),
                    MTQL = result.MTQL.GetValueOrDefault(false),
                    KET = result.KET.GetValueOrDefault(false),
                    CAT = result.CAT.GetValueOrDefault(false),
                    NPS = result.NPS.GetValueOrDefault(false),
                    BENZ = result.BENZ.GetValueOrDefault(false),
                    THC = result.THC.GetValueOrDefault(false),
                    BARB = result.BARB.GetValueOrDefault(false),
                    METH = result.METH.GetValueOrDefault(false),
                    PCP = result.PCP.GetValueOrDefault(false),
                    BUPRE = result.BUPRE.GetValueOrDefault(false),
                    PPZ = result.PPZ.GetValueOrDefault(false),
                    IsSealed = result.IsSealed,
                    SealedOrDiscardedBy = result.SealedOrDiscardedBy,
                    SealedOrDiscardedDate = result.SealedOrDiscardedDate
                });
            }
        }

        [HttpGet]
        [ResponseType(typeof(SSPDrugResultsModel))]
        [Route("api/SSP/SSPGetDrugResults")]
        public IHttpActionResult SSPGetDrugResultsByTimestamp(string NRIC, DateTime timestamp)
        {
            Trinity.DAL.DBContext.DrugResult result = new DAL.DAL_DrugResults().GetByNRICAndTimestamp(NRIC, timestamp);
            if (result == null)
            {
                return Ok();
            }
            else
            {
                return Ok(new SSPDrugResultsModel()
                {
                    NRIC = result.NRIC,
                    Datetime = result.timestamp,
                    MarkingNumber = result.markingnumber,
                    AMPH = result.AMPH.GetValueOrDefault(false),
                    OPI = result.OPI.GetValueOrDefault(false),
                    COCA = result.COCA.GetValueOrDefault(false),
                    LSD = result.LSD.GetValueOrDefault(false),
                    MTQL = result.MTQL.GetValueOrDefault(false),
                    KET = result.KET.GetValueOrDefault(false),
                    CAT = result.CAT.GetValueOrDefault(false),
                    NPS = result.NPS.GetValueOrDefault(false),
                    BENZ = result.BENZ.GetValueOrDefault(false),
                    THC = result.THC.GetValueOrDefault(false),
                    BARB = result.BARB.GetValueOrDefault(false),
                    METH = result.METH.GetValueOrDefault(false),
                    PCP = result.PCP.GetValueOrDefault(false),
                    BUPRE = result.BUPRE.GetValueOrDefault(false),
                    PPZ = result.PPZ.GetValueOrDefault(false),
                    IsSealed = result.IsSealed,
                    SealedOrDiscardedBy = result.SealedOrDiscardedBy,
                    SealedOrDiscardedDate = result.SealedOrDiscardedDate
                });
            }
        }

        [HttpPost]
        [ResponseType(typeof(bool))]
        public async System.Threading.Tasks.Task<IHttpActionResult> SSPComplete([FromBody] SSPCompleteModel model)
        {
            try
            {
                var user = new DAL.DAL_User().GetByNRIC(model.NRIC);
                new DAL.DAL_QueueNumber().UpdateQueueStatusByUserId(user.UserId, EnumStation.SSP, EnumQueueStatuses.Finished, EnumStation.DUTYOFFICER, EnumQueueStatuses.TabSmartCard, EnumMessage.SelectOutCome, EnumQueueOutcomeText.TapSmartCardToContinue);
                new DAL.DAL_Labels().DeleteLabel(user.UserId, DateTime.Today, EnumLabelType.UB);
                await System.Threading.Tasks.Task.Run(() => Trinity.SignalR.Client.Instance.BackendAPISend(NotificationNames.SSP_COMPLETED, model.NRIC));
                return Ok(true);
            }
            catch (Exception)
            {
                return Ok(false);
            }
        }
    }
}
