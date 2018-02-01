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
        [ResponseType(typeof(Trinity.BE.Appointment))]
        public Trinity.BE.Appointment GetDetailsById(string appointmentId)
        {
            var guid = Guid.Empty;
            if (Guid.TryParse(appointmentId, out guid))
            {
                return new DAL.DAL_Appointments().GetAppointmentDetails(guid);
            }
            else
            {
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
        [ResponseType(typeof(Trinity.BE.Appointment))]
        public IHttpActionResult GetByUserIdAndDate(string UserId, string date)
        {
            DateTime _date = Convert.ToDateTime(date);
            return Ok(new DAL.DAL_Appointments().GetMyAppointmentByDate(UserId, _date));
        }

        /// <summary>
        /// Get list appointment by user Id
        /// </summary>
        /// <param name="userId">user id</param>
        /// <returns>Appointment</returns>
        [HttpGet]
        [Route("api/Appointment/GetListByUserId")]
        [ResponseType(typeof(List<Trinity.BE.Appointment>))]
        public IHttpActionResult GetListByUserId(string userId)
        {
            return Ok(new DAL.DAL_Appointments().GetMyAppointmentBy(userId));
        }

        /// <summary>
        /// Get appointment by today
        /// </summary>
        /// <param name="userId">user id</param>
        /// <returns>Appointment</returns>
        [HttpGet]
        [Route("api/Appointment/GetByToday")]
        [ResponseType(typeof(Trinity.BE.Appointment))]
        public IHttpActionResult GetByToday(string userId)
        {
            return Ok(new DAL.DAL_Appointments().GetTodayAppointment(userId));
        }

        /// <summary>
        /// Get list current timeslot by specific timespan
        /// </summary>
        /// <param name="currentTime">current time</param>
        /// <returns>List Appointment</returns>
        [HttpGet]
        [Route("api/Appointment/GetListCurrentTimeslot")]
        [ResponseType(typeof(List<Trinity.BE.Appointment>))]
        public IHttpActionResult GetListCurrentTimeslot(string currentTime)
        {
            TimeSpan current = new TimeSpan();
            if (TimeSpan.TryParse(currentTime, out current))
            {
                return Ok(new DAL.DAL_Appointments().GetAllCurrentTimeslotAppointment(current));
            }
            return null;
        }

        /// <summary>
        /// Get the nearest appointment of user 
        /// </summary>
        /// <param name="userId"> user id </param>
        /// <returns>Appointment</returns>
        [HttpGet]
        [Route("api/Appointment/GetNearest")]
        [ResponseType(typeof(Trinity.BE.Appointment))]
        public IHttpActionResult GetNearest(string userId)
        {
            return Ok(new DAL.DAL_Appointments().GetNearestAppointment(userId));
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
        [ResponseType(typeof(Trinity.BE.Appointment))]
        public IHttpActionResult UpdateBooktime(string appointmentId, string timeStart, string timeEnd)
        {
            return Ok(new DAL.DAL_Appointments().UpdateBookTime(appointmentId, timeStart, timeEnd));
        }

        /// <summary>
        /// Get count absence appointment of user
        /// </summary>
        /// <param name="userID">user id</param>
        /// <returns>int</returns>
        [HttpGet]
        [Route("api/Appointment/CountAbsenceByUserId")]
        [ResponseType(typeof(int))]
        public IHttpActionResult CountAbsenceByUserId(string userID)
        {
            return Ok(new DAL.DAL_Appointments().CountMyAbsence(userID));
        }

        /// <summary>
        /// Get list absence appointment of user
        /// </summary>
        /// <param name="userID">user id</param>
        /// <returns>List Appointment</returns>
        [HttpGet]
        [Route("api/Appointment/GetAbsenceByUserId")]
        [ResponseType(typeof(List<Trinity.BE.Appointment>))]
        public IHttpActionResult GetAbsenceByUserId(string userID)
        {
            return Ok(new DAL.DAL_Appointments().GetMyAbsentAppointments(userID));
        }

        /// <summary>
        /// Update absence reason of appointment
        /// </summary>
        /// <param name="appointmentId">appointment id</param>
        /// <param name="asbsenceId">absence id</param>
        /// <returns>Appointment</returns>
        [HttpPost]
        [Route("api/Appointment/UpdateReason")]
        [ResponseType(typeof(Trinity.BE.Appointment))]
        public IHttpActionResult UpdateReason(string appointmentId, string asbsenceId)
        {
            var appointmentGuid = Guid.Empty;
            var absenceGuid = Guid.Empty;
            if (Guid.TryParse(appointmentId, out appointmentGuid) && Guid.TryParse(asbsenceId, out absenceGuid))
            {
                return Ok(new DAL.DAL_Appointments().UpdateReason(appointmentGuid, absenceGuid));
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
        [ResponseType(typeof(Trinity.BE.Appointment))]
        public IHttpActionResult GetListFromSelectedDate(string[] listAppointmentId)
        {
            var _listAppointmentId = listAppointmentId.ToList();
            return Ok(new DAL.DAL_Appointments().GetListAppointmentFromSelectedDate(_listAppointmentId));
        }

        /// <summary>
        /// Count appointment by time slot
        /// </summary>
        /// <param name="appointmentId">appointment id</param>
        /// <returns>int</returns>
        [HttpGet]
        [Route("api/Appointment/CountByTimeslot")]
        [ResponseType(typeof(int))]
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
        [ResponseType(typeof(List<Trinity.BE.Appointment>))]
        public IHttpActionResult GetAllAppointments()
        {
            return Ok(new DAL.DAL_Appointments().GetAllAppointments());
        }

        /// <summary>
        /// Get all statistics
        /// </summary>
        /// <returns>List Statistics</returns>
        [HttpGet]
        [Route("api/Appointment/GetAllStatistics")]
        [ResponseType(typeof(List<Trinity.BE.Statistics>))]
        public IHttpActionResult GetAllStatistics()
        {
            return Ok(new DAL.DAL_Appointments().GetAllStatistics());
        }

        /// <summary>
        /// Count booked appointment by timeslot
        /// </summary>
        /// <param name="timeslotId">time slot id</param>
        /// <returns>int</returns>
        [HttpGet]
        [Route("api/Appointment/CountBookedByTimeslot")]
        [ResponseType(typeof(int))]
        public IHttpActionResult CountBookedByTimeslot(string timeslotId)
        {
            return Ok(new DAL.DAL_Appointments().CountAppointmentBookedByTimeslot(timeslotId));
        }

        /// <summary>
        /// Count reported appointment by time slot
        /// </summary>
        /// <param name="timeslotId">time slot id</param>
        /// <returns>int</returns>
        [HttpGet]
        [Route("api/Appointment/CountReportedByTimeslot")]
        [ResponseType(typeof(int))]
        public IHttpActionResult CountReportedByTimeslot(string timeslotId)
        {
            return Ok(new DAL.DAL_Appointments().CountAppointmentReportedByTimeslot(timeslotId));
        }

        /// <summary>
        /// Count no show appointment by time slot
        /// </summary>
        /// <param name="timeslotId">time slot id</param>
        /// <returns>int</returns>
        [HttpGet]
        [Route("api/Appointment/CountNoShowdByTimeslot")]
        [ResponseType(typeof(int))]
        public IHttpActionResult CountNoShowdByTimeslot(string timeslotId)
        {
            return Ok(new DAL.DAL_Appointments().CountAppointmentNoShowByTimeslot(timeslotId));
        }

        /// <summary>
        /// Get maximum queue number of time slot
        /// </summary>
        /// <param name="timeslotId">time slot id </param>
        /// <returns>int</returns>
        [HttpGet]
        [Route("api/Appointment/GetMaximumNumberOfTimeslot")]
        [ResponseType(typeof(int))]
        public IHttpActionResult GetMaximumNumberOfTimeslot(string timeslotId)
        {
            return Ok(new DAL.DAL_Appointments().GetMaximumNumberOfTimeslot(timeslotId));
        }

        /// <summary>
        /// Create appointment for all user
        /// </summary>
        /// <param name="specificDate">specific date</param>
        [HttpPost]
        [Route("api/Appointment/CreateForAllUsers")]
        public void CreateForAllUsers(string specificDate)
        {
            DateTime date = Convert.ToDateTime(specificDate);
            new DAL.DAL_Appointments().CreateAppointmentsForAllUsers(date);
        }


        /// <summary>
        /// Get the nearest time slot
        /// </summary>
        /// <returns>Timeslot</returns>
        [HttpGet]
        [Route("api/Appointment/GetNearestTimeslot")]
        [ResponseType(typeof(Trinity.BE.TimeslotDetails))]
        public Trinity.DAL.DBContext.Timeslot GetNearestTimeslot()
        {
            return new DAL.DAL_Appointments().GetTimeslotNearest();
        }


        /// <summary>
        /// Update time slot for appointment
        /// </summary>
        /// <param name="appointmentId">appointment id</param>
        /// <param name="timeslotID"> timeslot id</param>
        /// <returns>Appointment</returns>
        [HttpPost]
        [Route("api/Appointment/UpdateTimeslot")]
        [ResponseType(typeof(Trinity.BE.Appointment))]
        public IHttpActionResult UpdateTimeslot(string appointmentId, string timeslotID)
        {
            Guid appointmentGuid = Guid.Empty;

            if (Guid.TryParse(appointmentId, out appointmentGuid))
            {
                return Ok(new DAL.DAL_Appointments().UpdateTimeslotForAppointment(appointmentGuid, timeslotID));
            }
            return null;

        }
    }
}
