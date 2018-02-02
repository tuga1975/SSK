using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;

namespace Trinity.CentralizedAPI.Controllers
{
    public class UserController : ApiController
    {
        
        [HttpGet]
        [Route("api/User/GetUserByUserId")]
        public Trinity.BE.User GetUserByUserId(string userId)
        {
            return new DAL.DAL_User().GetUserByUserId(userId,false);
        }

        [HttpPost]
        [Route("api/User/UpdateUser")]
        [ResponseType(typeof(BE.ResponseModel))]
        public IHttpActionResult UpdateUser(BE.User model)
        {
            var responseModel = new BE.ResponseModel();
            var result = new DAL.DAL_User().UpdateUser(model);
            responseModel.ResponseCode = result.ResponseCode;
            responseModel.ResponseMessage = result.ResponseMessage;
            responseModel.Data = result.Data;
            return Ok(responseModel);
        }

        [HttpPost]
        [Route("api/User/UpdateUserProfile")]
        [ResponseType(typeof(BE.ResponseModel))]
        public IHttpActionResult UpdateUserProfile(BE.UserProfile model)
        {
            var responseModel = new BE.ResponseModel();
            var result = new DAL.DAL_UserProfile().UpdateUserProfile(model);
            responseModel.ResponseCode = result.ResponseCode;
            responseModel.ResponseMessage = result.ResponseMessage;
            responseModel.Data = result.Data;
            return Ok(responseModel);
        }
    }
}
