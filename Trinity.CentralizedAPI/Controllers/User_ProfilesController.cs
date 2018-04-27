using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using Trinity.Common;

namespace Trinity.BackendAPI.Controllers
{
    public class User_ProfilesModel
    {
        public string UserId { get; set; }
    }
    public class User_ProfilesController : ApiController
    {
        [HttpPost]
        [ResponseType(typeof(bool))]
        public IHttpActionResult Approve([FromBody] User_ProfilesModel model)
        {
            try
            {
                new DAL.DAL_UpdateProfile().Approve(model.UserId);
                return Ok(true);
            }
            catch (Exception)
            {
                return Ok(false);
            }
        }

        [HttpPost]
        [ResponseType(typeof(bool))]
        public IHttpActionResult Reject([FromBody] User_ProfilesModel model)
        {
            try
            {
                new DAL.DAL_UpdateProfile().Reject(model.UserId);
                return Ok(true);
            }
            catch (Exception)
            {
                return Ok(false);
            }
        }
    }
}
