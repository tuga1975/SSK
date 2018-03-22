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
   
    public class SHPNotificationModel
    {
        public string Type { get; set; }
        public string Content { get; set; }
        public Nullable<System.DateTime> Datetime { get; set; }
        public string notification_code { get; set; }
    }


    [Route("api/SHP/{Action}")]
    public class SHPController : ApiController
    {
        [HttpGet]
        public IHttpActionResult SHPGetDrugResults(string markingnumber)
        {
            Trinity.DAL.DBContext.DrugResult result = new DAL.DAL_DrugResults().GetByMarkingNumber(markingnumber);
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

        [HttpPost]
        public async System.Threading.Tasks.Task<IHttpActionResult> SHPPostNotification([FromBody] SHPNotificationModel data)
        {
            string IDNoti = new DAL.DAL_Notification().InsertNotification(null, null, null, data.Content, false, data.Datetime.Value, data.notification_code, data.Type, EnumStation.UHP);
            if (string.IsNullOrEmpty(IDNoti))
            {
                return Ok(string.Empty);
            }
            else
            {
                await System.Threading.Tasks.Task.Run(() => Trinity.SignalR.Client.Instance.SendToAppDutyOfficers(EnumStation.UHP, data.Type, data.Content, data.notification_code));
                return Ok(IDNoti);
            }
        }

        [HttpGet]
        public async System.Threading.Tasks.Task<IHttpActionResult> SHPComplete(string NRIC)
        {
            var user = new DAL.DAL_User().GetByNRIC(NRIC);
            new DAL.DAL_QueueNumber().UpdateQueueStatusByUserId(user.UserId, EnumStation.UHP, EnumQueueStatuses.Finished, EnumStation.HSA, EnumQueueStatuses.Processing, "", EnumQueueOutcomeText.Processing);
            await System.Threading.Tasks.Task.Run(() => Trinity.SignalR.Client.Instance.BackendAPICompleted(NotificationNames.SHP_COMPLETED, NRIC));
            return Ok(true);
        }

    }
}
