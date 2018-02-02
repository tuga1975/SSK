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
        [Route("api/Notification/GetByUserId")]
        [ResponseType(typeof(ResponseModel))]
        public IHttpActionResult GetMyNotifications(string userId)
        {
            var responseModel = new ResponseModel();
            var result = new DAL.DAL_Notification().GetMyNotifications(userId);
            responseModel.ResponseCode = result.ResponseCode;
            responseModel.ResponseMessage = result.ResponseMessage;
            responseModel.Data = result.Data;
            return Ok(responseModel);
        }
    }
}
