﻿using System;
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
            return Ok(new DAL.DAL_Notification().GetAllNotifications(userId));
        }
        [HttpPost]
        [Route("api/Notification/SendToDutyOfficer")]
        public IHttpActionResult SendToDutyOfficer(string UserId, string DutyOfficerID, string Subject, string Content, string Type, string Station)
        {
            string MessageID = Guid.NewGuid().ToString().Trim();
            new DAL.DAL_Notification().SendToDutyOfficer(MessageID, UserId, DutyOfficerID, Subject, Content, Type, Station);
            return Ok(MessageID);
        }

        [HttpPost]
        [Route("api/Notification/SendAllDutyOfficer")]
        public IHttpActionResult SendAllDutyOfficer(string UserId, string Subject, string Content, string Type, string Station)
        {
            return Ok(new DAL.DAL_Notification().SendAllDutyOfficer(UserId, Subject, Content, Type, Station));
        }
    }
}
