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
        public string type { get; set; }
        public string content { get; set; }
        public DateTime datetime { get; set; }
        public string notification_code { get; set; }
        public string NRIC { get; set; }
    }
    public class SSPTransactionModel
    {
        public string type { get; set; }
        public string content { get; set; }
        public DateTime datetime { get; set; }
        public string transaction_code { get; set; }
        public string NRIC { get; set; }
    }

    [Route("api/SSP/{Action}")]
    public class SSPController : ApiController
    {

        [HttpPost]
        public async System.Threading.Tasks.Task<IHttpActionResult> SSPPostNotification([FromBody]SSPNotificationModel data)
        {
            if (!string.IsNullOrEmpty(new DAL.DAL_Notification().InsertNotification(null, null, null, data.content, false, data.datetime, data.notification_code, data.type, EnumStation.ESP)))
            {
                if (data.type== EnumNotificationTypes.Error && !string.IsNullOrEmpty(data.NRIC))
                {
                    var user = new DAL.DAL_User().GetByNRIC(data.NRIC);
                    new DAL.DAL_QueueNumber().UpdateQueueStatusByUserId(user.UserId, EnumStation.ESP, EnumQueueStatuses.Errors, EnumStation.DUTYOFFICER, EnumQueueStatuses.Finished, EnumMessage.LeakageDeletected, EnumQueueOutcomeText.Processing);
                    await System.Threading.Tasks.Task.Run(() => Trinity.SignalR.Client.Instance.BackendAPICompleted(NotificationNames.SSP_COMPLETED, data.NRIC));
                }
                await System.Threading.Tasks.Task.Run(() => Trinity.SignalR.Client.Instance.SendToAppDutyOfficers(EnumStation.ESP, data.type, data.content, data.notification_code));
                return Ok(true);
            }
            else
                return Ok(false);
        }
        [HttpPost]
        public IHttpActionResult SSPPostTransaction([FromBody]SSPTransactionModel data)
        {
            if (new DAL.DAL_Transactions().Insert(data.NRIC,EnumStation.ESP, data.type, data.content, data.datetime, data.transaction_code)!= Guid.Empty)
                return Ok(true);
            else
                return Ok(false);
        }

        [HttpGet]
        public IHttpActionResult SSPAuthenticate(string NRIC)
        {
            Trinity.DAL.DBContext.Membership_Users user = new DAL.DAL_User().GetByNRIC(NRIC);
            if (user == null)
            {
                return Ok(new
                {
                    Found = false
                });
            }
            else
            {
                return Ok(new
                {
                    Found = true,
                    Right = user.RightThumbFingerprint,
                    Left = user.LeftThumbFingerprint
                });
            }
        }
        [HttpGet]
        public IHttpActionResult SSPGetCaseOfficer(string NRIC)
        {
            Trinity.DAL.DBContext.Membership_Users user = new DAL.DAL_User().GetByNRIC(NRIC);
            if (user == null)
            {
                return Ok(string.Empty);
            }
            else
            {
                return Ok(user.Name);
            }
        }
        [HttpGet]
        public IHttpActionResult SSPGetDrugResults(string NRIC,DateTime DateCreate)
        {
            Trinity.DAL.DBContext.DrugResult result = new DAL.DAL_DrugResults().GetByNRICAndDate(NRIC, DateCreate);
            if (result == null)
            {
                return Ok();
            }
            else
            {
                return Ok(new
                {
                    result.NRIC,
                    result.timestamp,
                    result.markingnumber,
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
                    PPZ = result.PPZ.GetValueOrDefault(false)
                });
            }
        }

        [HttpGet]
        public async System.Threading.Tasks.Task<IHttpActionResult> SSPComplete(string NRIC)
        {
            var user = new DAL.DAL_User().GetByNRIC(NRIC);
            new DAL.DAL_QueueNumber().UpdateQueueStatusByUserId(user.UserId, EnumStation.ESP, EnumQueueStatuses.Finished, EnumStation.DUTYOFFICER, EnumQueueStatuses.TabSmartCard, EnumMessage.SelectOutCome, EnumQueueOutcomeText.TapSmartCardToContinue);
            await System.Threading.Tasks.Task.Run(() => Trinity.SignalR.Client.Instance.BackendAPICompleted(NotificationNames.SSP_COMPLETED, NRIC));
            return Ok(true);
        }
    }
}
