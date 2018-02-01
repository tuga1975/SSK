using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

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
        [ResponseType(typeof(BE.ResponseTypeModel<Trinity.BE.Appointment>))]
        public IHttpActionResult GetById(string appointmentId)
        {
            var responseModel = new BE.ResponseModel();


            var guid = Guid.Empty;
            if (Guid.TryParse(appointmentId, out guid))
            {
                var result = new DAL.DAL_Appointments().GetMyAppointmentByID(guid);
                responseModel.ResponseCode = result.ResponseCode;
                responseModel.ResponseMessage = result.ResponseMessage;
                responseModel.Data = result.Data;
                return Ok(responseModel);
            }
            else
            {
                return Ok(responseModel);
            }

        }

        /// <summary>
        /// Get  appointment details by Id
        /// </summary>
        /// <param name="appointmentId">appointment id</param>
        /// <returns>Appointment</returns>
        [HttpGet]
        [Route("api/Appointment/GetDetailsById")]
        [ResponseType(typeof(BE.ResponseTypeModel<Trinity.BE.Appointment>))]
        public IHttpActionResult GetDetailsById(string appointmentId)
        {
            var responseModel = new BE.ResponseModel();
            var guid = Guid.Empty;
            if (Guid.TryParse(appointmentId, out guid))
            {
                var result= new DAL.DAL_Appointments().GetAppointmentDetails(guid);
                responseModel.ResponseCode = result.ResponseCode;
                responseModel.ResponseMessage = result.ResponseMessage;
                responseModel.Data = result.Data;
                return Ok(responseModel);
            }
            else
            {
                return Ok(responseModel);
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
        [ResponseType(typeof(BE.ResponseTypeModel<Trinity.BE.Appointment>))]
        public IHttpActionResult GetByUserIdAndDate(string UserId, string date)
        {
            var responseModel = new BE.ResponseModel();
            DateTime _date = Convert.ToDateTime(date);
            var result = new DAL.DAL_Appointments().GetMyAppointmentByDate(UserId, _date);
            responseModel.ResponseCode = result.ResponseCode;
            responseModel.ResponseMessage = result.ResponseMessage;
            responseModel.Data = result.Data;
            return Ok(responseModel);

        }

        /// <summary>
        /// Get list appointment by user Id
        /// </summary>
        /// <param name="userId">user id</param>
        /// <returns>Appointment</returns>
        [HttpGet]
        [Route("api/Appointment/GetListByUserId")]
        [ResponseType(typeof(BE.ResponseTypeModel<List<Trinity.BE.Appointment>>))]
        public IHttpActionResult GetListByUserId(string userId)
        {
            var responseModel = new BE.ResponseModel();
            var result = new DAL.DAL_Appointments().GetMyAppointmentBy(userId);
            responseModel.ResponseCode = result.ResponseCode;
            responseModel.ResponseMessage = result.ResponseMessage;
            responseModel.Data = result.Data;
            return Ok(responseModel);
            
        }

        /// <summary>
        /// Get appointment by today
        /// </summary>
        /// <param name="userId">user id</param>
        /// <returns>Appointment</returns>
        [HttpGet]
        [Route("api/Appointment/GetByToday")]
        [ResponseType(typeof(BE.ResponseTypeModel<Trinity.BE.Appointment>))]
        public IHttpActionResult GetByToday(string userId)
        {
            var responseModel = new BE.ResponseModel();
            var result = new DAL.DAL_Appointments().GetTodayAppointment(userId);
            responseModel.ResponseCode = result.ResponseCode;
            responseModel.ResponseMessage = result.ResponseMessage;
            responseModel.Data = result.Data;
            return Ok(responseModel);
           
        }

        /// <summary>
        /// Get list current timeslot by specific timespan
        /// </summary>
        /// <param name="currentTime">current time</param>
        /// <returns>List Appointment</returns>
        [HttpGet]
        [Route("api/Appointment/GetListCurrentTimeslot")]
        [ResponseType(typeof(BE.ResponseTypeModel<List<Trinity.BE.Appointment>>))]
        public IHttpActionResult GetListCurrentTimeslot(string currentTime)
        {
            var responseModel = new BE.ResponseModel();
            TimeSpan current = new TimeSpan();
            if (TimeSpan.TryParse(currentTime, out current))
            {
               
                var result = new DAL.DAL_Appointments().GetAllCurrentTimeslotAppointment(current);
                responseModel.ResponseCode = result.ResponseCode;
                responseModel.ResponseMessage = result.ResponseMessage;
                responseModel.Data = result.Data;
                return Ok(responseModel);
            }
            return Ok(responseModel);
        }

        /// <summary>
        /// Get the nearest appointment of user 
        /// </summary>
        /// <param name="userId"> user id </param>
        /// <returns>Appointment</returns>
        [HttpGet]
        [Route("api/Appointment/GetNearest")]
        [ResponseType(typeof(BE.ResponseTypeModel<Trinity.BE.Appointment>))]
        public IHttpActionResult GetNearest(string userId)
        {
            var responseModel = new BE.ResponseModel();
            var result = new DAL.DAL_Appointments().GetNearestAppointment(userId);
            responseModel.ResponseCode = result.ResponseCode;
            responseModel.ResponseMessage = result.ResponseMessage;
            responseModel.Data = result.Data;
            return Ok(responseModel);
          
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
        [ResponseType(typeof(BE.ResponseTypeModel<Trinity.BE.Appointment>))]
        public IHttpActionResult UpdateBooktime(string appointmentId, string timeStart, string timeEnd)
        {
            var responseModel = new BE.ResponseModel();
            var result = new DAL.DAL_Appointments().UpdateBookTime(appointmentId, timeStart, timeEnd);
            responseModel.ResponseCode = result.ResponseCode;
            responseModel.ResponseMessage = result.ResponseMessage;
            responseModel.Data = result.Data;
            return Ok(responseModel);
        }

        /// <summary>
        /// Get count absence appointment of user
        /// </summary>
        /// <param name="userID">user id</param>
        /// <returns>int</returns>
        [HttpGet]
        [Route("api/Appointment/CountAbsenceByUserId")]
        [ResponseType(typeof(BE.ResponseModel))]
        public IHttpActionResult CountAbsenceByUserId(string userID)
        {
            var responseModel = new BE.ResponseModel();
            var result = new DAL.DAL_Appointments().CountMyAbsence(userID);
            responseModel.ResponseCode = result.ResponseCode;
            responseModel.ResponseMessage = result.ResponseMessage;
            responseModel.Data = result.Data;
            return Ok(responseModel);
        }

        /// <summary>
        /// Get list absence appointment of user
        /// </summary>
        /// <param name="userID">user id</param>
        /// <returns>List Appointment</returns>
        [HttpGet]
        [Route("api/Appointment/GetAbsenceByUserId")]
        [ResponseType(typeof(BE.ResponseTypeModel<List<Trinity.BE.Appointment>>))]
        public IHttpActionResult GetAbsenceByUserId(string userID)
        {
            var responseModel = new BE.ResponseModel();
            var result = new DAL.DAL_Appointments().GetMyAbsentAppointments(userID);
            responseModel.ResponseCode = result.ResponseCode;
            responseModel.ResponseMessage = result.ResponseMessage;
            responseModel.Data = result.Data;
            return Ok(responseModel);
        }

        /// <summary>
        /// Update absence reason of appointment
        /// </summary>
        /// <param name="appointmentId">appointment id</param>
        /// <param name="asbsenceId">absence id</param>
        /// <returns>Appointment</returns>
        [HttpPost]
        [Route("api/Appointment/UpdateReason")]
        [ResponseType(typeof(BE.ResponseTypeModel<Trinity.BE.Appointment>))]
        public IHttpActionResult UpdateReason(string appointmentId, string asbsenceId)
        {
            var responseModel = new BE.ResponseModel();
            var appointmentGuid = Guid.Empty;
            var absenceGuid = Guid.Empty;
            if (Guid.TryParse(appointmentId, out appointmentGuid) && Guid.TryParse(asbsenceId, out absenceGuid))
            {

                var result = new DAL.DAL_Appointments().UpdateReason(appointmentGuid, absenceGuid);
                responseModel.ResponseCode = result.ResponseCode;
                responseModel.ResponseMessage = result.ResponseMessage;
                responseModel.Data = result.Data;
                return Ok(responseModel);
            }
            return Ok(responseModel);


        }

        /// <summary>
        /// Get list appointment from selected date
        /// </summary>
        /// <param name="listAppointmentId">appointment id</param>
        /// <returns>List Appointment</returns>
        [HttpGet]
        [Route("api/Appointment/GetListFromSelectedDate")]
        [ResponseType(typeof(BE.ResponseTypeModel<List<Trinity.BE.Appointment>>))]
        public IHttpActionResult GetListFromSelectedDate(string[] listAppointmentId)
        {
            var responseModel = new BE.ResponseModel();
            var _listAppointmentId = listAppointmentId.ToList();
            var result = new DAL.DAL_Appointments().GetListAppointmentFromSelectedDate(_listAppointmentId);
            responseModel.ResponseCode = result.ResponseCode;
            responseModel.ResponseMessage = result.ResponseMessage;
            responseModel.Data = result.Data;
            return Ok(responseModel);
        }

        /// <summary>
        /// Count appointment by time slot
        /// </summary>
        /// <param name="appointmentId">appointment id</param>
        /// <returns>int</returns>
        [HttpGet]
        [Route("api/Appointment/CountByTimeslot")]
        [ResponseType(typeof(BE.ResponseModel))]
        public IHttpActionResult CountByTimeslot(string appointmentId)
        {
            var responseModel = new BE.ResponseModel();
            var guid = Guid.Empty;
            if (Guid.TryParse(appointmentId, out guid))
            {
                var appointment = new DAL.DAL_Appointments().GetMyAppointmentByID(guid);
                responseModel.Data = new DAL.DAL_Appointments().CountListAppointmentByTimeslot(appointment.Data);
                responseModel.ResponseCode = (int)EnumResponseStatuses.Success;
                responseModel.ResponseMessage = EnumResponseMessage.Success;
                return Ok(responseModel);
            }
            return Ok(responseModel);


        }

        /// <summary>
        /// Get all appointment
        /// </summary>
        /// <returns>List Appointment</returns>
        [HttpGet]
        [Route("api/Appointment/GetAll")]
        [ResponseType(typeof(BE.ResponseTypeModel<List<Trinity.BE.Appointment>>))]
        public IHttpActionResult GetAllAppointments()
        {
            var responseModel = new BE.ResponseModel();
            var result = new DAL.DAL_Appointments().GetAllAppointments();
            responseModel.ResponseCode = result.ResponseCode;
            responseModel.ResponseMessage = result.ResponseMessage;
            responseModel.Data = result.Data;
            return Ok(responseModel);
        }

        /// <summary>
        /// Get all statistics
        /// </summary>
        /// <returns>List Statistics</returns>
        [HttpGet]
        [Route("api/Appointment/GetAllStatistics")]
        [ResponseType(typeof(BE.ResponseTypeModel<List<Trinity.BE.Statistics>>))]
        public IHttpActionResult GetAllStatistics()
        {
            var responseModel = new BE.ResponseModel();
            var result = new DAL.DAL_Appointments().GetAllStatistics();
            responseModel.ResponseCode = result.ResponseCode;
            responseModel.ResponseMessage = result.ResponseMessage;
            responseModel.Data = result.Data;
            return Ok(responseModel);
        }

        /// <summary>
        /// Count booked appointment by timeslot
        /// </summary>
        /// <param name="timeslotId">time slot id</param>
        /// <returns>int</returns>
        [HttpGet]
        [Route("api/Appointment/CountBookedByTimeslot")]
        [ResponseType(typeof(BE.ResponseModel))]
        public IHttpActionResult CountBookedByTimeslot(string timeslotId)
        {
            var responseModel = new BE.ResponseModel();
            var result = new DAL.DAL_Appointments().CountAppointmentBookedByTimeslot(timeslotId);
            responseModel.ResponseCode = result.ResponseCode;
            responseModel.ResponseMessage = result.ResponseMessage;
            responseModel.Data = result.Data;
            return Ok(responseModel);
        }

        /// <summary>
        /// Count reported appointment by time slot
        /// </summary>
        /// <param name="timeslotId">time slot id</param>
        /// <returns>int</returns>
        [HttpGet]
        [Route("api/Appointment/CountReportedByTimeslot")]
        [ResponseType(typeof(BE.ResponseModel))]
        public IHttpActionResult CountReportedByTimeslot(string timeslotId)
        {
            var responseModel = new BE.ResponseModel();
            var result = new DAL.DAL_Appointments().CountAppointmentReportedByTimeslot(timeslotId);
            responseModel.ResponseCode = result.ResponseCode;
            responseModel.ResponseMessage = result.ResponseMessage;
            responseModel.Data = result.Data;
            return Ok(responseModel);
        }

        /// <summary>
        /// Count no show appointment by time slot
        /// </summary>
        /// <param name="timeslotId">time slot id</param>
        /// <returns>int</returns>
        [HttpGet]
        [Route("api/Appointment/CountNoShowdByTimeslot")]
        [ResponseType(typeof(BE.ResponseModel))]
        public IHttpActionResult CountNoShowdByTimeslot(string timeslotId)
        {
            var responseModel = new BE.ResponseModel();
            var result = new DAL.DAL_Appointments().CountAppointmentNoShowByTimeslot(timeslotId);
            responseModel.ResponseCode = result.ResponseCode;
            responseModel.ResponseMessage = result.ResponseMessage;
            responseModel.Data = result.Data;
            return Ok(responseModel);
        }

        /// <summary>
        /// Get maximum queue number of time slot
        /// </summary>
        /// <param name="timeslotId">time slot id </param>
        /// <returns>int</returns>
        [HttpGet]
        [Route("api/Appointment/GetMaximumNumberOfTimeslot")]
        [ResponseType(typeof(BE.ResponseModel))]
        public IHttpActionResult GetMaximumNumberOfTimeslot(string timeslotId)
        {
            var responseModel = new BE.ResponseModel();
            var result = new DAL.DAL_Appointments().GetMaximumNumberOfTimeslot(timeslotId);
            responseModel.ResponseCode = result.ResponseCode;
            responseModel.ResponseMessage = result.ResponseMessage;
            responseModel.Data = result.Data;
            return Ok(responseModel);
        }

        /// <summary>
        /// Create appointment for all user
        /// </summary>
        /// <param name="specificDate">specific date</param>
        [HttpPost]
        [Route("api/Appointment/CreateForAllUsers")]
        [ResponseType(typeof(BE.ResponseModel))]
        public IHttpActionResult CreateForAllUsers(string specificDate)
        {
            var responseModel = new BE.ResponseModel();
            DateTime date = Convert.ToDateTime(specificDate);
            var result = new DAL.DAL_Appointments().CreateAppointmentsForAllUsers(date); 
            responseModel.ResponseCode = result.ResponseCode;
            responseModel.ResponseMessage = result.ResponseMessage;
            responseModel.Data = result.Data;
            return Ok(responseModel);
          
            
        }


        /// <summary>
        /// Get the nearest time slot
        /// </summary>
        /// <returns>Timeslot</returns>
        [HttpGet]
        [Route("api/Appointment/GetNearestTimeslot")]
        [ResponseType(typeof(BE.ResponseTypeModel<Trinity.BE.TimeslotDetails>))]
        public IHttpActionResult GetNearestTimeslot()
        {
            var responseModel = new BE.ResponseModel();
            var result = new DAL.DAL_Appointments().GetTimeslotNearest();
            responseModel.ResponseCode = result.ResponseCode;
            responseModel.ResponseMessage = result.ResponseMessage;
            responseModel.Data = result.Data;
            return Ok(responseModel);
        }


        /// <summary>
        /// Update time slot for appointment
        /// </summary>
        /// <param name="appointmentId">appointment id</param>
        /// <param name="timeslotID"> timeslot id</param>
        /// <returns>Appointment</returns>
        [HttpPost]
        [Route("api/Appointment/UpdateTimeslot")]
        [ResponseType(typeof(BE.ResponseTypeModel<Trinity.BE.Appointment>))]
        public IHttpActionResult UpdateTimeslot(string appointmentId, string timeslotID)
        {
            var responseModel = new BE.ResponseModel();
            Guid appointmentGuid = Guid.Empty;

            if (Guid.TryParse(appointmentId, out appointmentGuid))
            {
                
                var result = new DAL.DAL_Appointments().UpdateTimeslotForAppointment(appointmentGuid, timeslotID);
                responseModel.ResponseCode = result.ResponseCode;
                responseModel.ResponseMessage = result.ResponseMessage;
                responseModel.Data = result.Data;
                return Ok(responseModel);
            }
            return Ok(responseModel);

        }
    }
}
