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
            var result = new DAL.DAL_User().GetUserById(userId);
            //responseModel.ResponseCode = result.ResponseCode;
            //responseModel.ResponseMessage = result.ResponseMessage;
            //responseModel.Data = result.Data;
            return Ok(result);
        }

        [HttpGet]
        [Route("api/User/GetUserProfileByUserId")]
        //[ResponseType(typeof(ResponseModel))]
        public IHttpActionResult GetUserProfileByUserId(string userId)
        {
            var responseModel = new ResponseModel();
            var result = new DAL.DAL_UserProfile().GetProfileByUserId(userId);
            //responseModel.ResponseCode = result.ResponseCode;
            //responseModel.ResponseMessage = result.ResponseMessage;
            //responseModel.Data = result.Data;
            return Ok(result);
        }

        [HttpGet]
        [Route("api/User/GetAddressByUserId")]
        //[ResponseType(typeof(ResponseModel))]
        public IHttpActionResult GetAddressByUserId(string userId, string isOther)
        {
            var responseModel = new ResponseModel();
            bool other = Convert.ToBoolean(isOther);
            var result = new DAL.DAL_UserProfile().GetAddByUserId(userId, other);
            //responseModel.ResponseCode = result.ResponseCode;
            //responseModel.ResponseMessage = result.ResponseMessage;
            //responseModel.Data = result.Data;
            return Ok(result);
        }

        [HttpGet]
        [Route("api/User/GetAllSupervisees")]
        //[ResponseType(typeof(ResponseModel))]
        public IHttpActionResult GetAllSupervisees()
        {
            var responseModel = new ResponseModel();
            var result = new DAL.DAL_User().GetListAllSupervisees();
            //responseModel.ResponseCode = result.ResponseCode;
            //responseModel.ResponseMessage = result.ResponseMessage;
            //responseModel.Data = result.Data;
            return Ok(result);
        }

        [HttpPost]
        [Route("api/User/UpdateUser")]
        //[ResponseType(typeof(ResponseModel))]
        public IHttpActionResult UpdateUser(BE.User model)
        {
            var responseModel = new ResponseModel();
            var result = new DAL.DAL_User().Update(model);
            //responseModel.ResponseCode = result.ResponseCode;
            //responseModel.ResponseMessage = result.ResponseMessage;
            //responseModel.Data = result.Data;
            return Ok(result);
        }

        [HttpPost]
        [Route("api/User/UpdateUserProfile")]
        //[ResponseType(typeof(ResponseModel))]
        public IHttpActionResult UpdateUserProfile(BE.UserProfile model)
        {
            var responseModel = new ResponseModel();
            var result = new DAL.DAL_UserProfile().UpdateProfile(model);
            //responseModel.ResponseCode = result.ResponseCode;
            //responseModel.ResponseMessage = result.ResponseMessage;
            //responseModel.Data = result.Data;
            return Ok(result);
        }

        [HttpGet]
        [Route("api/User/GetMembershipByUserId")]
        //[ResponseType(typeof(ResponseModel))]
        public IHttpActionResult GetMembershipByUserId(string userId)
        {
            var responseModel = new ResponseModel();
            var result = new DAL.DAL_Membership_Users().GetByUserId(userId);
            //responseModel.ResponseCode = result.ResponseCode;
            //responseModel.ResponseMessage = result.ResponseMessage;
            //responseModel.Data = result.Data;
            return Ok(result);
        }

        [HttpGet]
        [Route("api/User/GetUserBySmartCardId")]
        //[ResponseType(typeof(ResponseModel))]
        public IHttpActionResult GetUserBySmartCardId(string smartcardId)
        {
            var responseModel = new ResponseModel();
            var result = new DAL.DAL_User().GetUserBySmartCardId(smartcardId);
            //responseModel.ResponseCode = result.ResponseCode;
            //responseModel.ResponseMessage = result.ResponseMessage;
            //responseModel.Data = result.Data;
            return Ok(result);
        }
    }
}
