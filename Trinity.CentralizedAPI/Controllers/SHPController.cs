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

    public class SHPDrugResultsModel
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
    public class SHPCompleteModel
    {
        public string NRIC { get; set; }
    }

    [Route("api/SHP/{Action}")]
    public class SHPController : ApiController
    {
        [HttpGet]
        [ResponseType(typeof(SHPDrugResultsModel))]
        public IHttpActionResult SHPGetDrugResults(string markingnumber)
        {
            Trinity.DAL.DBContext.DrugResult result = new DAL.DAL_DrugResults().GetByMarkingNumber(markingnumber);
            if (result == null)
            {
                return Ok();
            }
            else
            {
                return Ok(new SHPDrugResultsModel()
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
        public async System.Threading.Tasks.Task<IHttpActionResult> SHPPostNotification([FromBody] SHPNotificationModel data)
        {
            try
            {
                string IDNoti = new DAL.DAL_Notification().InsertNotification(null, null, null, data.Content, false, data.Datetime.Value, data.notification_code, data.Type, EnumStation.SHP);
                if (string.IsNullOrEmpty(IDNoti))
                {
                    //return Ok(string.Empty);
                    return Ok(false);
                }
                else
                {
                    await System.Threading.Tasks.Task.Run(() => Trinity.SignalR.Client.Instance.SendToAppDutyOfficers(null, null, data.Content, data.Type, EnumStation.SHP, false));
                    //return Ok(IDNoti);
                    return Ok(true);
                }
            }
            catch (Exception)
            {
                return Ok(false);
            }
        }

        [HttpPost]
        [ResponseType(typeof(bool))]
        public async System.Threading.Tasks.Task<IHttpActionResult> SHPComplete([FromBody]SHPCompleteModel model)
        {
            try
            {
                var user = new DAL.DAL_User().GetByNRIC(model.NRIC);
                new DAL.DAL_QueueNumber().UpdateQueueStatusByUserId(user.UserId, EnumStation.SHP, EnumQueueStatuses.Finished, EnumStation.UT, EnumQueueStatuses.Processing, "", EnumQueueOutcomeText.Processing);
                await System.Threading.Tasks.Task.Run(() => Trinity.SignalR.Client.Instance.BackendAPISend(NotificationNames.SHP_COMPLETED, model.NRIC));
                return Ok(true);
            }
            catch (Exception)
            {
                return Ok(false);
            }
        }
    }
}
