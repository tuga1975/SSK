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
        [ResponseType(typeof(BE.ResponseModel))]
        public IHttpActionResult GetUserByUserId(string userId)
        {
            var responseModel = new BE.ResponseModel();
            var result = new DAL.DAL_User().GetUserByUserId(userId);
            responseModel.ResponseCode = result.ResponseCode;
            responseModel.ResponseMessage = result.ResponseMessage;
            responseModel.Data = result.Data;
            return Ok(responseModel);
        }

        [HttpGet]
        [Route("api/User/GetUserProfileByUserId")]
        [ResponseType(typeof(BE.ResponseModel))]
        public IHttpActionResult GetUserProfileByUserId(string userId)
        {
            var responseModel = new BE.ResponseModel();
            var result = new DAL.DAL_UserProfile().GetUserProfileByUserId(userId);
            responseModel.ResponseCode = result.ResponseCode;
            responseModel.ResponseMessage = result.ResponseMessage;
            responseModel.Data = result.Data;
            return Ok(responseModel);
        }

        [HttpGet]
        [Route("api/User/GetAddressByUserId")]
        [ResponseType(typeof(BE.ResponseTypeModel<BE.UserProfile>))]
        public IHttpActionResult GetAddressByUserId(string userId,string isOther)
        {
            var responseModel = new BE.ResponseModel();
            bool other = Convert.ToBoolean(isOther);
            var result = new DAL.DAL_UserProfile().GetAddressByUserId(userId, other);
            responseModel.ResponseCode = result.ResponseCode;
            responseModel.ResponseMessage = result.ResponseMessage;
            responseModel.Data = result.Data;
            return Ok(responseModel);
        }

[HttpGet]
        [Route("api/User/GetAllSupervisees")]
        [ResponseType(typeof(BE.ResponseTypeModel<List<BE.User>>))]
        public IHttpActionResult GetAllSupervisees(string userId,string isOther)
        {
            var responseModel = new BE.ResponseModel();
            bool other = Convert.ToBoolean(isOther);
            var result = new DAL.DAL_User().GetAllSupervisees();
            responseModel.ResponseCode = result.ResponseCode;
            responseModel.ResponseMessage = result.ResponseMessage;
            responseModel.Data = result.Data;
            return Ok(responseModel);
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
