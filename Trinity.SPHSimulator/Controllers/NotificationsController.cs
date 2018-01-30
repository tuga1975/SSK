using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using uhpSim;
using uhpSim.Models;

namespace uhpSim.Controllers
{
    public class NotificationsController : ApiController
    {
        private UHPSimulatorIIEntities db = new UHPSimulatorIIEntities();

        [HttpPost]
        [Route("api/uhp-sim/notification")]
        public IEnumerable<string> Post([FromBody] Notification note)
        {
            string[] lines = { note.Source, note.Type, note.Content, note.Datetime.ToString(), note.notification_code, DateTime.Now.ToString(), "==================" };
            Utils.Logs.SaveLog(System.Web.Hosting.HostingEnvironment.MapPath("~/Logs") + @"\Notifications.log", lines);
            return db.USP_Notification_Insert(note.Source, note.Type, note.Content, note.Datetime, note.notification_code).AsEnumerable();
        }

        [HttpPost]
        [Route("api/uhp-sim/getNotices")]
        public IEnumerable<USP_Notification_Select_By_Date_Result> Get([FromBody] noticeRequest requestDate)
        {
            return db.USP_Notification_Select_By_Date(requestDate.requestDate).AsEnumerable();
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool NotificationExists(string id)
        {
            return db.Notifications.Any(e => e.NotificationID == id);
        }
    }
}