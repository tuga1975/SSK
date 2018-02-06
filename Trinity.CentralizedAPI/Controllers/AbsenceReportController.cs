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

    public class AbsenceReportController : ApiController
    {
        [HttpPost]
        [Route("api/AbsenceReport/SetReasonInfo")]
        public IHttpActionResult SetReasonInfo(Trinity.BE.Reason model)
        {
            var result = new DAL.DAL_AbsenceReporting().SetInfo(model);
            return Ok(result);
        }
    }
}
