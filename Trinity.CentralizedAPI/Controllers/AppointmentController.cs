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
    public class AppointmentController : ApiController
    {
        /// <summary>
        /// Get appointment by Id
        /// </summary>
        /// <param name="appointmentId">appointment id</param>
        /// <returns>Appointment</returns>
        [HttpGet]
        [Route("api/Appointment/GetById")]
        ////[ResponseType(typeof(ResponseModel))]
        public IHttpActionResult GetById(string appointmentId)
        {
            //var responseModel = new ResponseModel();
            var guid = Guid.Empty;
            if (Guid.TryParse(appointmentId, out guid))
            {
                var result = new DAL.DAL_Appointments().GetAppointmentByID(guid);
                //responseModel.ResponseCode = result.ResponseCode;
                //responseModel.ResponseMessage = result.ResponseMessage;
                //responseModel.Data = result.Data;
                //return Ok(responseModel);
                return Ok(result);
            }
            else
            {
                //return Ok(responseModel);
                return null;
            }

        }

        /// <summary>
        /// Get  appointment details by Id
        /// </summary>
        /// <param name="appointmentId">appointment id</param>
        /// <returns>Appointment</returns>
        [HttpGet]
        [Route("api/Appointment/GetDetailsById")]
        ////[ResponseType(typeof(ResponseModel))]
        public IHttpActionResult GetDetailsById(string appointmentId)
        {
            //var responseModel = new ResponseModel();
            var guid = Guid.Empty;
            if (Guid.TryParse(appointmentId, out guid))
            {
                var result= new DAL.DAL_Appointments().GetAppmtDetails(guid);
                //responseModel.ResponseCode = result.ResponseCode;
                //responseModel.ResponseMessage = result.ResponseMessage;
                //responseModel.Data = result.Data;
                //return Ok(responseModel);
                return Ok(result);
            }
            else
            {
                //return Ok(responseModel);
                return null;
            }
        }

        /// <summary>
        /// Get appointment by userId and specific date
        /// </summary>
        /// <param name="UserId">user id</param>
        /// <param name="date"> date </param>
        /// <returns>Appointment</returns>
        [HttpGet]
        [Route("api/Appointment/GetByUserIdAndDate")]
        ////[ResponseType(typeof(ResponseModel))]
        public IHttpActionResult GetByUserIdAndDate(string UserId, string date)
        {
            //var responseModel = new ResponseModel();
            DateTime _date = Convert.ToDateTime(date);
            var result = new DAL.DAL_Appointments().GetAppointmentByDate(UserId, _date);
            //responseModel.ResponseCode = result.ResponseCode;
            //responseModel.ResponseMessage = result.ResponseMessage;
            //responseModel.Data = result.Data;
            //return Ok(responseModel);
            return Ok(result);

        }

        /// <summary>
        /// Get list appointment by user Id
        /// </summary>
        /// <param name="userId">user id</param>
        /// <returns>Appointment</returns>
        [HttpGet]
        [Route("api/Appointment/GetListByUserId")]
        ////[ResponseType(typeof(ResponseModel))]
        public IHttpActionResult GetListByUserId(string userId)
        {
           // var responseModel = new ResponseModel();
            var result = new DAL.DAL_Appointments().GetAppointmentByUserId(userId);
            //responseModel.ResponseCode = result.ResponseCode;
            //responseModel.ResponseMessage = result.ResponseMessage;
            //responseModel.Data = result.Data;
            //return Ok(responseModel);
            return Ok(result);
            
        }

        /// <summary>
        /// Get appointment by today
        /// </summary>
        /// <param name="userId">user id</param>
        /// <returns>Appointment</returns>
        [HttpGet]
        [Route("api/Appointment/GetByToday")]
        ////[ResponseType(typeof(ResponseModel))]
        public IHttpActionResult GetByToday(string userId)
        {
            //var responseModel = new ResponseModel();
            var result = new DAL.DAL_Appointments().GetTodayAppointmentByUserId(userId);
            //responseModel.ResponseCode = result.ResponseCode;
            //responseModel.ResponseMessage = result.ResponseMessage;
            //responseModel.Data = result.Data;
            //return Ok(responseModel);
            return Ok(result);
           
        }

        /// <summary>
        /// Get list current timeslot by specific timespan
        /// </summary>
        /// <param name="currentTime">current time</param>
        /// <returns>List Appointment</returns>
        [HttpGet]
        [Route("api/Appointment/GetListCurrentTimeslot")]
        ////[ResponseType(typeof(ResponseModel))]
        public IHttpActionResult GetListCurrentTimeslot(string currentTime)
        {
            //var responseModel = new ResponseModel();
            TimeSpan current = new TimeSpan();
            if (TimeSpan.TryParse(currentTime, out current))
            {
               
                var result = new DAL.DAL_Appointments().GetAllCurrentTimeslotApptmt(current);
                //responseModel.ResponseCode = result.ResponseCode;
                //responseModel.ResponseMessage = result.ResponseMessage;
                //responseModel.Data = result.Data;
                //return Ok(responseModel);
                return Ok(result);
            }
            //return Ok(responseModel);
            return null;
        }

        /// <summary>
        /// Get the nearest appointment of user 
        /// </summary>
        /// <param name="userId"> user id </param>
        /// <returns>Appointment</returns>
        [HttpGet]
        [Route("api/Appointment/GetNearest")]
        public IHttpActionResult GetNearest(string userId)
        {
           // var responseModel = new ResponseModel();
            var result = new DAL.DAL_Appointments().GetNearestApptmt(userId);
            //responseModel.ResponseCode = result.ResponseCode;
            //responseModel.ResponseMessage = result.ResponseMessage;
            //responseModel.Data = result.Data;
            return Ok(result);
          
        }

        /// <summary>
        /// Update appointment booking time
        /// </summary>
        /// <param name="appointmentId">appointment id</param>
        /// <param name="timeStart">start time</param>
        /// <param name="timeEnd"> end time</param>
        /// <returns>Appointment</returns>
        [HttpPost]
        [Route("api/Appointment/UpdateBooktime")]
        ////[ResponseType(typeof(ResponseModel))]
        public IHttpActionResult UpdateBooktime(string appointmentId, string timeStart, string timeEnd)
        {
            //var responseModel = new ResponseModel();
            var result = new DAL.DAL_Appointments().UpdateBookingTime(appointmentId, timeStart, timeEnd);
            //responseModel.ResponseCode = result.ResponseCode;
            //responseModel.ResponseMessage = result.ResponseMessage;
            //responseModel.Data = result.Data;
            //return Ok(responseModel);
            return Ok(result);
        }

        /// <summary>
        /// Get count absence appointment of user
        /// </summary>
        /// <param name="userID">user id</param>
        /// <returns>int</returns>
        [HttpGet]
        [Route("api/Appointment/CountAbsenceByUserId")]
        //[ResponseType(typeof(ResponseModel))]
        public IHttpActionResult CountAbsenceByUserId(string userID)
        {
           // var responseModel = new ResponseModel();
            var result = new DAL.DAL_Appointments().CountAbsenceReport(userID);
            //responseModel.ResponseCode = result.ResponseCode;
            //responseModel.ResponseMessage = result.ResponseMessage;
            //responseModel.Data = result.Data;
            return Ok(result);
        }

        /// <summary>
        /// Get list absence appointment of user
        /// </summary>
        /// <param name="userID">user id</param>
        /// <returns>List Appointment</returns>
        [HttpGet]
        [Route("api/Appointment/GetAbsenceByUserId")]
        //[ResponseType(typeof(ResponseModel))]
        public IHttpActionResult GetAbsenceByUserId(string userID)
        {
            //var responseModel = new ResponseModel();
            var result = new DAL.DAL_Appointments().GetAbsentAppointments(userID);
            //responseModel.ResponseCode = result.ResponseCode;
            //responseModel.ResponseMessage = result.ResponseMessage;
            //responseModel.Data = result.Data;
            return Ok(result);
        }

        /// <summary>
        /// Update absence reason of appointment
        /// </summary>
        /// <param name="appointmentId">appointment id</param>
        /// <param name="asbsenceId">absence id</param>
        /// <returns>Appointment</returns>
        [HttpPost]
        [Route("api/Appointment/UpdateReason")]
        //[ResponseType(typeof(ResponseModel))]
        public IHttpActionResult UpdateReason(string appointmentId, string asbsenceId)
        {
            //var responseModel = new ResponseModel();
            var appointmentGuid = Guid.Empty;
            var absenceGuid = Guid.Empty;
            if (Guid.TryParse(appointmentId, out appointmentGuid) && Guid.TryParse(asbsenceId, out absenceGuid))
            {

                var result = new DAL.DAL_Appointments().UpdateAbsenceReason(appointmentGuid, absenceGuid);
                //responseModel.ResponseCode = result.ResponseCode;
                //responseModel.ResponseMessage = result.ResponseMessage;
                //responseModel.Data = result.Data;
                //return Ok(responseModel);
                return Ok(result);
            }
            return null;


        }

        /// <summary>
        /// Get list appointment from selected date
        /// </summary>
        /// <param name="listAppointmentId">appointment id</param>
        /// <returns>List Appointment</returns>
        [HttpGet]
        [Route("api/Appointment/GetListFromSelectedDate")]
        //[ResponseType(typeof(ResponseModel))]
        public IHttpActionResult GetListFromSelectedDate(string listAppointmentId)
        {
           // var responseModel = new ResponseModel();
            var _listAppointmentId = listAppointmentId.Split(',').ToList();
            var result = new DAL.DAL_Appointments().GetListAppointmentFromListSelectedDate(listAppointmentId);
            //responseModel.ResponseCode = result.ResponseCode;
            //responseModel.ResponseMessage = result.ResponseMessage;
            //responseModel.Data = result.Data;
            return Ok(result);
        }

        /// <summary>
        /// Count appointment by time slot
        /// </summary>
        /// <param name="appointmentId">appointment id</param>
        /// <returns>int</returns>
        [HttpGet]
        [Route("api/Appointment/CountByTimeslot")]
        //[ResponseType(typeof(ResponseModel))]
        public IHttpActionResult CountByTimeslot(string appointmentId)
        {
           // var responseModel = new ResponseModel();
            var guid = Guid.Empty;
            if (Guid.TryParse(appointmentId, out guid))
            {
                var appointment = new DAL.DAL_Appointments().GetAppointmentByID(guid);
                var result = new DAL.DAL_Appointments().CountListApptmtByTimeslot(appointment);
                //responseModel.ResponseCode = (int)EnumResponseStatuses.Success;
                //responseModel.ResponseMessage = EnumResponseMessage.Success;
                //return Ok(responseModel);
                return Ok(result);
            }
            return null;


        }

        /// <summary>
        /// Get all appointment
        /// </summary>
        /// <returns>List Appointment</returns>
        [HttpGet]
        [Route("api/Appointment/GetAll")]
        //[ResponseType(typeof(ResponseModel))]
        public IHttpActionResult GetAllAppointments()
        {
            //var responseModel = new ResponseModel();
            var result = new DAL.DAL_Appointments().GetAllApptmts();
            //responseModel.ResponseCode = result.ResponseCode;
            //responseModel.ResponseMessage = result.ResponseMessage;
            //responseModel.Data = result.Data;
            //return Ok(responseModel);
            return Ok(result);
        }

        /// <summary>
        /// Get all statistics
        /// </summary>
        /// <returns>List Statistics</returns>
        [HttpGet]
        [Route("api/Appointment/GetAllStatistics")]
        //[ResponseType(typeof(ResponseModel))]
        public IHttpActionResult GetAllStatistics()
        {
           // var responseModel = new ResponseModel();
            var result = new DAL.DAL_Appointments().GetAllStats();
            //responseModel.ResponseCode = result.ResponseCode;
            //responseModel.ResponseMessage = result.ResponseMessage;
            //responseModel.Data = result.Data;
            //return Ok(responseModel);
            return Ok(result);
        }

        /// <summary>
        /// Count booked appointment by timeslot
        /// </summary>
        /// <param name="timeslotId">time slot id</param>
        /// <returns>int</returns>
        [HttpGet]
        [Route("api/Appointment/CountBookedByTimeslot")]
        //[ResponseType(typeof(ResponseModel))]
        public IHttpActionResult CountBookedByTimeslot(string timeslotId)
        {
            var responseModel = new ResponseModel();
            var result = new DAL.DAL_Appointments().CountApptmtBookedByTimeslot(timeslotId);
            //responseModel.ResponseCode = result.ResponseCode;
            //responseModel.ResponseMessage = result.ResponseMessage;
            //responseModel.Data = result.Data;
            //return Ok(responseModel);
            return Ok(result);
        }

        /// <summary>
        /// Count reported appointment by time slot
        /// </summary>
        /// <param name="timeslotId">time slot id</param>
        /// <returns>int</returns>
        [HttpGet]
        [Route("api/Appointment/CountReportedByTimeslot")]
        //[ResponseType(typeof(ResponseModel))]
        public IHttpActionResult CountReportedByTimeslot(string timeslotId)
        {
            var responseModel = new ResponseModel();
            var result = new DAL.DAL_Appointments().CountApptmtReportedByTimeslot(timeslotId);
            //responseModel.ResponseCode = result.ResponseCode;
            //responseModel.ResponseMessage = result.ResponseMessage;
            //responseModel.Data = result.Data;
            //return Ok(responseModel);
            return Ok(result);
        }

        /// <summary>
        /// Count no show appointment by time slot
        /// </summary>
        /// <param name="timeslotId">time slot id</param>
        /// <returns>int</returns>
        [HttpGet]
        [Route("api/Appointment/CountNoShowdByTimeslot")]
        //[ResponseType(typeof(ResponseModel))]
        public IHttpActionResult CountNoShowdByTimeslot(string timeslotId)
        {
            var responseModel = new ResponseModel();
            var result = new DAL.DAL_Appointments().CountApptmtNoShowByTimeslot(timeslotId);
            //responseModel.ResponseCode = result.ResponseCode;
            //responseModel.ResponseMessage = result.ResponseMessage;
            //responseModel.Data = result.Data;
            //return Ok(responseModel);
            return Ok(result);
        }

        /// <summary>
        /// Get maximum queue number of time slot
        /// </summary>
        /// <param name="timeslotId">time slot id </param>
        /// <returns>int</returns>
        [HttpGet]
        [Route("api/Appointment/GetMaximumNumberOfTimeslot")]
        //[ResponseType(typeof(ResponseModel))]
        public IHttpActionResult GetMaximumNumberOfTimeslot(string timeslotId)
        {
            var responseModel = new ResponseModel();
            var result = new DAL.DAL_Appointments().GetMaxNumberOfTimeslot(timeslotId);
            //responseModel.ResponseCode = result.ResponseCode;
            //responseModel.ResponseMessage = result.ResponseMessage;
            //responseModel.Data = result.Data;
            //return Ok(responseModel);
            return Ok(result);
        }

        /// <summary>
        /// Create appointment for all user
        /// </summary>
        /// <param name="specificDate">specific date</param>
        [HttpPost]
        [Route("api/Appointment/CreateForAllUsers")]
        //[ResponseType(typeof(ResponseModel))]
        public IHttpActionResult CreateForAllUsers(string specificDate)
        {
            var responseModel = new ResponseModel();
            DateTime date = Convert.ToDateTime(specificDate);
            var result = new DAL.DAL_Appointments().CreateApptmtsForAllUsers(date);
            //responseModel.ResponseCode = result.ResponseCode;
            //responseModel.ResponseMessage = result.ResponseMessage;
            //responseModel.Data = result.Data;
            //return Ok(responseModel);
            return Ok(result);
          
            
        }


        /// <summary>
        /// Get the nearest time slot
        /// </summary>
        /// <returns>Timeslot</returns>
        [HttpGet]
        [Route("api/Appointment/GetNearestTimeslot")]
        //[ResponseType(typeof(ResponseModel))]
        public IHttpActionResult GetNearestTimeslot()
        {
            var responseModel = new ResponseModel();
            var result = new DAL.DAL_Appointments().GetTimeslotNearestAppointment();
            //responseModel.ResponseCode = result.ResponseCode;
            //responseModel.ResponseMessage = result.ResponseMessage;
            //responseModel.Data = result.Data;
            return Ok(result);
        }

        //[HttpGet]
        //[Route("api/Appointment/GetTimeslotbyAppointment")]
      
        //public IHttpActionResult GetTimeslotbyAppointment(string appointmentId)
        //{
        //    var responseModel = new ResponseModel();
        //    var result = new DAL.DAL_Appointments().GetTimeslotNearest();
        //    //responseModel.ResponseCode = result.ResponseCode;
        //    //responseModel.ResponseMessage = result.ResponseMessage;
        //    //responseModel.Data = result.Data;
        //    return Ok(responseModel);
        //}

        /// <summary>
        /// Update time slot for appointment
        /// </summary>
        /// <param name="appointmentId">appointment id</param>
        /// <param name="timeslotID"> timeslot id</param>
        /// <returns>Appointment</returns>
        [HttpPost]
        [Route("api/Appointment/UpdateTimeslot")]
        //[ResponseType(typeof(ResponseModel))]
        public IHttpActionResult UpdateTimeslot(string appointmentId, string timeslotID)
        {
            var responseModel = new ResponseModel();
            Guid appointmentGuid = Guid.Empty;

            if (Guid.TryParse(appointmentId, out appointmentGuid))
            {
                
                var result = new DAL.DAL_Appointments().UpdateTimeslotForApptmt(appointmentGuid, timeslotID);
                //responseModel.ResponseCode = result.ResponseCode;
                //responseModel.ResponseMessage = result.ResponseMessage;
                //responseModel.Data = result.Data;
                return Ok(result);
            }
            return null;

        }


    }
}
