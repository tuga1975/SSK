using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using Newtonsoft.Json;




namespace Trinity.BackendAPI.Controllers
{
    public class SSPModel
    {
        public string source { get; set; }
        public string type { get; set; }
        public string content { get; set; }
        public DateTime datetime { get; set; }
        public string notification_code { get; set; }
        public string transaction_code { get; set; }
        public string NRIC { get; set; }
    }

    [Route("api/SSP/{Action}")]
    public class SSPController : ApiController
    {
        [HttpPost]
        [Custom(IgnoreParameter = "NRIC,transaction_code")]
        public async System.Threading.Tasks.Task<IHttpActionResult> Notification([FromBody]SSPModel data)
        {
            if (!string.IsNullOrEmpty(new DAL.DAL_Notification().SSPInsert(data.source, data.type, data.content, data.datetime, data.notification_code)))
            {
                await System.Threading.Tasks.Task.Run(() => Trinity.SignalR.Client.Instance.SendToAllDutyOfficers(null,"SSP Insert Noti", data.content, data.type));
                return Ok(true);
            }
            else
                return Ok(false);
        }
        [HttpPost]
        [Custom(IgnoreParameter = "NRIC,notification_code,type")]
        public IHttpActionResult Transaction([FromBody]SSPModel data)
        {
            if (new DAL.DAL_Transactions().Insert(data.NRIC,data.source, data.type, data.content, data.datetime, data.transaction_code)!= Guid.Empty)
                return Ok(true);
            else
                return Ok(false);
        }

        [HttpGet]
        public IHttpActionResult Authentication(string source,string NRIC)
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
        public IHttpActionResult GetCaseOfficer(string NRIC)
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
        public IHttpActionResult DrugResult(string NRIC)
        {
            Trinity.DAL.DBContext.DrugResult result = new DAL.DAL_DrugResults().GetByNRIC(NRIC);
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
        public async System.Threading.Tasks.Task<IHttpActionResult> Completion(string NRIC)
        {
            var user = new DAL.DAL_User().GetByNRIC(NRIC);
            new DAL.DAL_QueueNumber().UpdateQueueStatusByUserId(user.UserId, EnumStation.ESP, EnumQueueStatuses.Finished, EnumStation.DUTYOFFICER, EnumQueueStatuses.Processing, "Select outcome", EnumQueueOutcomeText.TapSmartCardToContinue);
            await System.Threading.Tasks.Task.Run(() => Trinity.SignalR.Client.Instance.SSPCompleted(NRIC));
            return Ok(true);
        }
    }
}
