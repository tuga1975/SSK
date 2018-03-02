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
        #region 2018
        /// <summary>
        /// Get ApplicationUser by username and password
        /// </summary>
        /// <param name="username">username</param>
        /// <param name="password">password</param>
        /// <returns>ApplicationUser</returns>
        [HttpGet]
        [Route("api/User/Login")]
        //[ResponseType(typeof(ResponseModel))]
        public IHttpActionResult Login(string username, string password)
        {
            return Ok(new DAL.DAL_User().Login_Centralized(username, password));
        }

        /// <summary>
        /// Get User by smartcardId
        /// </summary>
        /// <param name="smartcardId">smart card UID</param>
        /// <returns>Trinity.BE.User</returns>
        [HttpGet]
        [Route("api/User/GetUserBySmartCardId")]
        //[ResponseType(typeof(ResponseModel))]
        public IHttpActionResult GetUserBySmartCardId(string smartcardId)
        {
            var responseModel = new ResponseModel();
            var result = new DAL.DAL_User().GetUserBySmartCardId(smartcardId);
            return Ok(result);
        }

        /// <summary>
        /// Get Supervisee by supervisee's NRIC (Duty officer Override)
        /// </summary>
        /// <param name="nric">supervisee's NRIC</param>
        /// <returns>Trinity.BE.User</returns>
        [HttpGet]
        [Route("api/User/GetSuperviseeByNRIC")]
        public IHttpActionResult GetSuperviseeByNRIC(string nric)
        {
            return Ok(new DAL.DAL_User().GetSuperviseeByNRIC(nric));
        }

        /// <summary>
        /// Get all supervicees
        /// </summary>
        /// <returns>List<BE.User></returns>
        [HttpGet]
        [Route("api/User/GetListAllSupervisees")]
        public IHttpActionResult GetListAllSupervisees()
        {
            return Ok(new DAL.DAL_User().GetListAllSupervisees());
        }

        #endregion


        #region unuse

        //[HttpGet]
        //[Route("api/User/GetUserById")]
        //public IHttpActionResult GetUserById(string userId)
        //{
        //    return Ok(new DAL.DAL_User().GetUserById(userId));
        //}

        //[HttpPost]
        //[Route("api/User/ChangeAccessFailedCount")]
        //public void ChangeAccessFailedCount(string userId, int count)
        //{
        //    new DAL.DAL_User().ChangeAccessFailedCount(userId, count);
        //}

        //[HttpGet]
        //[Route("api/User/IsInRole")]
        //public IHttpActionResult IsInRole(string Id, string Role)
        //{
        //    return Ok(new DAL.DAL_User().IsInRole(Id, Role));
        //}
        //[HttpGet]
        //[Route("api/User/FindByName")]
        //public IHttpActionResult FindByName(string username)
        //{
        //    return Ok(new DAL.DAL_User().FindByName(username));
        //}

        //[HttpGet]
        //[Route("api/User/GetProfileByUserId")]
        //public IHttpActionResult GetProfileByUserId(string userId)
        //{
        //    return Ok(new DAL.DAL_UserProfile().GetProfile(userId));
        //}
        //[HttpGet]
        //[Route("api/User/GetAddByUserId")]
        //public IHttpActionResult GetAddByUserId(string userId, bool isOther)
        //{
        //    return Ok(new DAL.DAL_UserProfile().GetAddByUserId(userId, isOther));
        //}
        //[HttpGet]
        //[Route("api/User/GetByUserId")]
        //public IHttpActionResult GetByUserId(string userId)
        //{
        //    return Ok(new DAL.DAL_Membership_Users().GetByUserId(userId));
        //}

        //[HttpPost]
        //[Route("api/User/Update")]
        //public IHttpActionResult Update([FromBody] BE.User model) {
        //    return Ok(new DAL.DAL_User().Update(model));
        //}
        //[HttpPost]
        //[Route("api/User/ChangeUserStatus")]
        //public void ChangeUserStatus(string userId, string status)
        //{
        //    new DAL.DAL_User().ChangeUserStatus(userId, status);
        //}
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="model">object[]{string userid,byte[] left,byte[] right}</param>
        //[HttpPost]
        //[Route("api/User/UpdateFingerprint")]
        //public void UpdateFingerprint([FromBody] object[] model)
        //{
        //    new DAL.DAL_Membership_Users().UpdateFingerprint(model[0].ToString(), (byte[])model[1], (byte[])model[2]);
        //}

        //[HttpPost]
        //[Route("api/User/UpdateSmartCardId")]
        //public void UpdateSmartCardId(string UserId, string SmartCardId)
        //{
        //    new DAL.DAL_Membership_Users().UpdateSmartCardId(UserId, SmartCardId);
        //}

        //[HttpPost]
        //[Route("api/User/UpdateCardInfo")]
        //public void UpdateCardInfo(string UserId, string CardNumber, DateTime Date_of_Issue, DateTime Expired_Date)
        //{
        //    new DAL.DAL_UserProfile().UpdateCardInfo(UserId, CardNumber, Date_of_Issue, Expired_Date);
        //}

        //[HttpPost]
        //[Route("api/User/UnblockSuperviseeById")]
        //public void UnblockSuperviseeById(string userId, string reason)
        //{
        //    new DAL.DAL_User().UnblockSuperviseeById(userId, reason);
        //}


        //[HttpGet]
        //[Route("api/User/GetUserByUserId")]
        ////[ResponseType(typeof(ResponseModel))]
        //public IHttpActionResult GetUserByUserId(string userId)
        //{
        //    var responseModel = new ResponseModel();
        //    var result = new DAL.DAL_User().GetUserById(userId);
        //    //responseModel.ResponseCode = result.ResponseCode;
        //    //responseModel.ResponseMessage = result.ResponseMessage;
        //    //responseModel.Data = result.Data;
        //    return Ok(result);
        //}







        //[HttpPost]
        //[Route("api/User/UpdateUser")]
        ////[ResponseType(typeof(ResponseModel))]
        //public IHttpActionResult UpdateUser(BE.User model)
        //{
        //    var responseModel = new ResponseModel();
        //    var result = new DAL.DAL_User().Update(model);
        //    //responseModel.ResponseCode = result.ResponseCode;
        //    //responseModel.ResponseMessage = result.ResponseMessage;
        //    //responseModel.Data = result.Data;
        //    return Ok(result);
        ////}

        //[HttpPost]
        //[Route("api/User/UpdateUserProfile")]
        ////[ResponseType(typeof(ResponseModel))]
        //public IHttpActionResult UpdateUserProfile(BE.UserProfile model)
        //{
        //    var responseModel = new ResponseModel();
        //    var result = new DAL.DAL_UserProfile().UpdateProfile(model);
        //    //responseModel.ResponseCode = result.ResponseCode;
        //    //responseModel.ResponseMessage = result.ResponseMessage;
        //    //responseModel.Data = result.Data;
        //    return Ok(result);
        //}
        #endregion

    }
}
