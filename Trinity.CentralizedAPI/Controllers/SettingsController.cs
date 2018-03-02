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
    public class SettingsController : ApiController
    {
        /// <summary>
        /// Get Appointment Time
        /// </summary>
        /// <returns></returns>
        //[HttpGet]
        //[Route("api/Setting/GetCurrentAppointmentTime")]
        //[ResponseType(typeof(Common.ResponseModel))]
        //public IHttpActionResult GetCurrentAppointmentTime()
        //{
        //    var responseModel = new Common.ResponseModel();
           
        //    var result = new DAL.DAL_Setting().GetCurrentApptmtTime();
        //    //responseModel.ResponseCode = result.ResponseCode;
        //    //responseModel.ResponseMessage = result.ResponseMessage;
        //    //responseModel.Data = result.Data;
        //    return Ok(responseModel);
        //}

        //[HttpGet]
        //[Route("api/Setting/GetOperationSettings")]
        //public IHttpActionResult GetOperationSettings()
        //{
        //    return Ok(new DAL.DAL_Setting().GetOperationSettings());
        //}

        //[HttpGet]
        //[Route("api/Setting/GetTimeslots")]
        //public IHttpActionResult GetTimeslots(string date)
        //{
        //    var _date = DateTime.Now;
        //    if (DateTime.TryParse(date,out _date))
        //    {
        //        return Ok(new DAL.DAL_Timeslots().GetTimeSlots(_date));
        //    }
        //    else
        //    {
        //        return null;
        //    }
          
        //}

        //[HttpPost]
        //[Route("api/Setting/UpdateSettingAndTimeSlot")]
        //public IHttpActionResult UpdateSettingAndTimeSlot(BE.SettingUpdate settingUpdate)
        //{
        //    var result = new DAL.DAL_Setting().UpdateSettingAndTimeSlot(settingUpdate);
        //    return Ok(result);
        //}

        //[HttpGet]
        //[Route("api/Setting/CheckWarningSaveSetting")]
        //public IHttpActionResult CheckWarningSaveSetting(string DayOfWeek)
        //{
        //    int dayofWeek = 0;
        //    if (Int32.TryParse(DayOfWeek, out dayofWeek))
        //    {
        //        return Ok(new DAL.DAL_Setting().CheckWarningSaveSetting(dayofWeek));
        //    }
        //    else
        //        return null;
        //}

        //[HttpPost]
        //[Route("api/Setting/AddHoliday")]
        //public IHttpActionResult AddHoliday(string date, string shortDesc, string notes, string updatedByName, string updatedByID)
        //{
        //    DateTime _date = Convert.ToDateTime(date);
        //    var result = new DAL.DAL_Setting().AddHoliday(_date, shortDesc, notes, updatedByName, updatedByID);
        //    return Ok(result);
        //}

        //[HttpPost]
        //[Route("api/Setting/DeleteHoliday")]
        //public IHttpActionResult DeleteHoliday(string date, string updatedBy)
        //{
        //    DateTime _date = Convert.ToDateTime(date);
        //    var result = new DAL.DAL_Setting().DeleteHoliday(_date, updatedBy);
        //    return Ok(result);
        //}
    }
}
