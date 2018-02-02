using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

namespace Trinity.CentralizedAPI.Controllers
{

    public class AbsenceReportController : ApiController
    {
        [HttpPost]
        [Route("api/AbsenceReport/SetReasonInfo")]
        [ResponseType(typeof(BE.ResponseModel))]
        public IHttpActionResult SetReasonInfo(Trinity.BE.Reason model)
        {
            var responseModel = new BE.ResponseModel();

            var result = new DAL.DAL_AbsenceReporting().SetInfo(model);
            responseModel.ResponseCode = result.ResponseCode;
            responseModel.ResponseMessage = result.ResponseMessage;
            responseModel.Data = result.Data;
            return Ok(responseModel);
        }
    }
}
