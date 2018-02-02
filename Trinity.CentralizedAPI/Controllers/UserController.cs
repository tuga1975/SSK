using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using Trinity.Common;

namespace Trinity.CentralizedAPI.Controllers
{
    public class UserController : ApiController
    {

        [HttpGet]
        [Route("api/User/GetUserByUserId")]
        //[ResponseType(typeof(ResponseModel))]
        public IHttpActionResult GetUserByUserId(string userId)
        {
            var responseModel = new ResponseModel();
            var result = new DAL.DAL_User().GetUserByUserId(userId);
            responseModel.ResponseCode = result.ResponseCode;
            responseModel.ResponseMessage = result.ResponseMessage;
            responseModel.Data = result.Data;
            return Ok(responseModel);
        }

        [HttpGet]
        [Route("api/User/GetUserProfileByUserId")]
        //[ResponseType(typeof(ResponseModel))]
        public IHttpActionResult GetUserProfileByUserId(string userId)
        {
            var responseModel = new ResponseModel();
            var result = new DAL.DAL_UserProfile().GetUserProfileByUserId(userId);
            responseModel.ResponseCode = result.ResponseCode;
            responseModel.ResponseMessage = result.ResponseMessage;
            responseModel.Data = result.Data;
            return Ok(responseModel);
        }

        [HttpGet]
        [Route("api/User/GetAddressByUserId")]
        //[ResponseType(typeof(ResponseModel))]
        public IHttpActionResult GetAddressByUserId(string userId,string isOther)
        {
            var responseModel = new ResponseModel();
            bool other = Convert.ToBoolean(isOther);
            var result = new DAL.DAL_UserProfile().GetAddressByUserId(userId, other);
            responseModel.ResponseCode = result.ResponseCode;
            responseModel.ResponseMessage = result.ResponseMessage;
            responseModel.Data = result.Data;
            return Ok(responseModel);
        }

[HttpGet]
        [Route("api/User/GetAllSupervisees")]
        //[ResponseType(typeof(ResponseModel))]
        public IHttpActionResult GetAllSupervisees(string userId,string isOther)
        {
            var responseModel = new ResponseModel();
            bool other = Convert.ToBoolean(isOther);
            var result = new DAL.DAL_User().GetAllSupervisees();
            responseModel.ResponseCode = result.ResponseCode;
            responseModel.ResponseMessage = result.ResponseMessage;
            responseModel.Data = result.Data;
            return Ok(responseModel);
        }

        [HttpPost]
        [Route("api/User/UpdateUser")]
        //[ResponseType(typeof(ResponseModel))]
        public IHttpActionResult UpdateUser(BE.User model)
        {
            var responseModel = new ResponseModel();
            var result = new DAL.DAL_User().UpdateUser(model);
            responseModel.ResponseCode = result.ResponseCode;
            responseModel.ResponseMessage = result.ResponseMessage;
            responseModel.Data = result.Data;
            return Ok(responseModel);
        }

        [HttpPost]
        [Route("api/User/UpdateUserProfile")]
        //[ResponseType(typeof(ResponseModel))]
        public IHttpActionResult UpdateUserProfile(BE.UserProfile model)
        {
            var responseModel = new ResponseModel();
            var result = new DAL.DAL_UserProfile().UpdateUserProfile(model);
            responseModel.ResponseCode = result.ResponseCode;
            responseModel.ResponseMessage = result.ResponseMessage;
            responseModel.Data = result.Data;
            return Ok(responseModel);
        }
    }
}
