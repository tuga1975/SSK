using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using Trinity.Common;

namespace Trinity.CentralizedAPI.Controllers
{
    public class NotificationController : ApiController
    {
        [HttpGet]
        [Route("api/Notification/GetMyNotifications")]
        //[ResponseType(typeof(ResponseModel))]
        public IHttpActionResult GetMyNotifications(string userId)
        {
            return Ok(new DAL.DAL_Notification().GetMyNotifications(userId));
        }
        [Route("api/Notification/SendToDutyOfficer")]
        public void SendToDutyOfficer(string UserId, string DutyOfficerID, string Subject, string Content,string Station)
        {
            new DAL.DAL_Notification().SendToDutyOfficer(UserId,DutyOfficerID,Subject,Content,Station);
        }
        [Route("api/Notification/SendAllDutyOfficer")]
        public void SendAllDutyOfficer(string UserId, string Subject, string Content, string Station)
        {
            new DAL.DAL_Notification().SendAllDutyOfficer(UserId, Subject, Content, Station);
        }
    }
}
