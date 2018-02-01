using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

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


    }
}
