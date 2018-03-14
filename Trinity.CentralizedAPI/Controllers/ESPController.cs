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
    public class ESPModel
    {
        public string markingnumber { get; set; }
        public string NotificationID { get; set; }
        public string Source { get; set; }
        public string Type { get; set; }
        public string Content { get; set; }
        public Nullable<System.DateTime> Datetime { get; set; }
        public string notification_code { get; set; }
        public Nullable<System.DateTime> requestDate { get; set; }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }

    [Route("api/ESP/{Action}")]
    public class ESPController : ApiController
    {
        [HttpPost]
        public IHttpActionResult GetDrugResults([FromBody]ESPModel data)
        {
            Trinity.DAL.DBContext.DrugResult result = new DAL.DAL_DrugResults().GetByMarkingNumber(data.markingnumber);
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
        public IHttpActionResult InsertNotification([FromBody] ESPModel data)
        {
            string IDNoti = new DAL.DAL_Notification().SSPInsert(data.Source, data.Type, data.Content, data.Datetime.Value, data.NotificationID);
            return Ok(IDNoti);
        }

        [HttpPost]
        public IHttpActionResult GetNotificationByDate([FromBody] ESPModel data)
        {
            var result = new DAL.DAL_Notification().GetByDate(data.requestDate.Value).Select(d=>new {
                d.NotificationID,
                d.Source,
                d.Type,
                d.Content,
                d.Datetime,
                d.notification_code
            });
            return Ok(result);
        }

    }
}
