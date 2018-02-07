﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using Newtonsoft.Json;

namespace Trinity.CentralizedAPI.Controllers
{
    public class SettingsController : ApiController
    {

        [HttpGet]
        [Route("api/Settings/GetCardInfo")]
        public IHttpActionResult GetCardInfo()
        {
            return Ok( new DAL.DAL_GetCardInfo().GetCardInfo());
        }

        /// <summary>
        /// Get Appointment Time
        /// </summary>
        /// <param name="userID">user id</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/Setting/GetAppointmentTime")]
        [ResponseType(typeof(Common.ResponseModel))]
        public IHttpActionResult GetAppointmentTime(string date)
        {
            var responseModel = new Common.ResponseModel();
            var _date = Convert.ToDateTime(date);
            
            var result = new DAL.DAL_Setting().GetApptmtTime(_date);
            //responseModel.ResponseCode = result.ResponseCode;
            //responseModel.ResponseMessage = result.ResponseMessage;
            //responseModel.Data = result.Data;
            return Ok(responseModel);
        }

        /// <summary>
        /// Get Appointment Time
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/Setting/GetCurrentAppointmentTime")]
        [ResponseType(typeof(Common.ResponseModel))]
        public IHttpActionResult GetCurrentAppointmentTime()
        {
            var responseModel = new Common.ResponseModel();
           
            var result = new DAL.DAL_Setting().GetCurrentApptmtTime();
            //responseModel.ResponseCode = result.ResponseCode;
            //responseModel.ResponseMessage = result.ResponseMessage;
            //responseModel.Data = result.Data;
            return Ok(responseModel);
        }

        [HttpGet]
        [Route("api/Setting/GetOperationSettings")]
        public IHttpActionResult GetOperationSettings()
        {
            return Ok(new DAL.DAL_Setting().GetOperationSettings());
        }

    }
}
