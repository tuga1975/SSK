using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using Newtonsoft.Json;

namespace Trinity.CentralizedAPI.Controllers
{
    public class IssueCardController : ApiController
    {

        [HttpGet]
        [Route("api/IssueCard/GetMyIssueCards")]
        public IHttpActionResult GetMyIssueCards(string UserId)
        {
            return Ok( new DAL.DAL_IssueCard().GetMyIssueCards(UserId));
        }

        [HttpPost]
        [Route("api/IssueCard/Insert")]
        public void Insert([FromBody] Trinity.BE.IssueCard model)
        {
            new DAL.DAL_IssueCard().Insert(model);
        }

        [HttpPost]
        [Route("api/IssueCard/UpdateStatusByUserId")]
        public void UpdateStatusByUserId(string userId, string Status)
        {
            new DAL.DAL_IssueCard().UpdateStatusByUserId(userId, Status);
        }



    }
}
