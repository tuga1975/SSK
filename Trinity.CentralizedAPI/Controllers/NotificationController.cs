using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using Trinity.Common;

namespace Trinity.BackendAPI.Controllers
{
    public class NotificationController : ApiController
    {
        //[HttpGet]
        //[Route("api/Notification/GetAllNotifications")]
        ////[ResponseType(typeof(ResponseModel))]
        //public IHttpActionResult GetAllNotifications(string userId)
        //{
        //    return Ok(new DAL.DAL_Notification().GetAllNotifications(userId));
        //}
        //[HttpPost]
        //[Route("api/Notification/SendToDutyOfficer")]
        //public IHttpActionResult SendToDutyOfficer(string UserId, string DutyOfficerID, string Subject, string Content, string Type, string Station)
        //{
        //    string MessageID = Guid.NewGuid().ToString().Trim();
        //    new DAL.DAL_Notification().SendToDutyOfficer(MessageID, UserId, DutyOfficerID, Subject, Content, Type, Station);
        //    return Ok(MessageID);
        //}

        //[HttpPost]
        //[Route("api/Notification/SendToAllDutyOfficers")]
        //public IHttpActionResult SendToAllDutyOfficers(string UserId, string Subject, string Content, string Type, string Station)
        //{
        //    return Ok(new DAL.DAL_Notification().SendToAllDutyOfficers(UserId, Subject, Content, Type, Station));
        //}

        //[HttpGet]
        //[Route("api/Notification/GetNotificationsSentToDutyOfficer")]
        ////[ResponseType(typeof(ResponseModel))]
        //public IHttpActionResult GetNotificationsSentToDutyOfficer()
        //{
        //    return Ok(new DAL.DAL_Notification().GetNotificationsSentToDutyOfficer());
        //}
    }
}
