using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

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
        public Trinity.DAL.DBContext.Appointment GetById(string appointmentId)
        {
            var guid = Guid.Empty;
            if (Guid.TryParse(appointmentId, out guid))
            {
                return new DAL.DAL_Appointments().GetMyAppointmentByID(guid);
            }
            else
            {
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
        public Trinity.DAL.DBContext.Appointment GetByUserIdAndDate(string UserId, string date)
        {
            DateTime _date = Convert.ToDateTime(date);
            return new DAL.DAL_Appointments().GetMyAppointmentByDate(UserId, _date);
        }

        /// <summary>
        /// Get list appointment by user Id
        /// </summary>
        /// <param name="userId">user id</param>
        /// <returns>Appointment</returns>
        [HttpGet]
        [Route("api/Appointment/GetListByUserId")]
        public List<Trinity.DAL.DBContext.Appointment> GetListByUserId(string userId)
        {
            return new DAL.DAL_Appointments().GetMyAppointmentBy(userId);
        }

        /// <summary>
        /// Get appointment by today
        /// </summary>
        /// <param name="userId">user id</param>
        /// <returns>Appointment</returns>
        [HttpGet]
        [Route("api/Appointment/GetByToday")]
        public Trinity.DAL.DBContext.Appointment GetByToday(string userId)
        {
            return new DAL.DAL_Appointments().GetTodayAppointment(userId);
        }

        /// <summary>
        /// Get list current timeslot by specific timespan
        /// </summary>
        /// <param name="currentTime">current time</param>
        /// <returns>List Appointment</returns>
        [HttpGet]
        [Route("api/Appointment/GetListCurrentTimeslot")]
        public List<Trinity.DAL.DBContext.Appointment> GetListCurrentTimeslot(string currentTime)
        {
            TimeSpan current = new TimeSpan();
            if (TimeSpan.TryParse(currentTime, out current))
            {
                return new DAL.DAL_Appointments().GetAllCurrentTimeslotAppointment(current);
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
        public Trinity.DAL.DBContext.Appointment GetNearest(string userId)
        {
            return new DAL.DAL_Appointments().GetNearestAppointment(userId);
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
        public Trinity.DAL.DBContext.Appointment UpdateBooktime(string appointmentId, string timeStart, string timeEnd)
        {
            return new DAL.DAL_Appointments().UpdateBookTime(appointmentId, timeStart, timeEnd);
        }

        /// <summary>
        /// Get count absence appointment of user
        /// </summary>
        /// <param name="userID">user id</param>
        /// <returns>int</returns>
        [HttpGet]
        [Route("api/Appointment/CountAbsenceByUserId")]
        public int CountAbsenceByUserId(string userID)
        {
            return new DAL.DAL_Appointments().CountMyAbsence(userID);
        }

        /// <summary>
        /// Get list absence appointment of user
        /// </summary>
        /// <param name="userID">user id</param>
        /// <returns>List Appointment</returns>
        [HttpGet]
        [Route("api/Appointment/GetAbsenceByUserId")]
        public List<Trinity.DAL.DBContext.Appointment> GetAbsenceByUserId(string userID)
        {
            return new DAL.DAL_Appointments().GetMyAbsentAppointments(userID);
        }

        /// <summary>
        /// Update absence reason of appointment
        /// </summary>
        /// <param name="appointmentId">appointment id</param>
        /// <param name="asbsenceId">absence id</param>
        /// <returns>Appointment</returns>
        [HttpPost]
        [Route("api/Appointment/UpdateReason")]
        public Trinity.DAL.DBContext.Appointment UpdateReason(string appointmentId, string asbsenceId)
        {
            var appointmentGuid = Guid.Empty;
            var absenceGuid = Guid.Empty;
            if (Guid.TryParse(appointmentId, out appointmentGuid) && Guid.TryParse(asbsenceId, out absenceGuid))
            {
                return new DAL.DAL_Appointments().UpdateReason(appointmentGuid, absenceGuid);
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
        public List<Trinity.DAL.DBContext.Appointment> GetListFromSelectedDate(List<string> listAppointmentId)
        {
            return new DAL.DAL_Appointments().GetListAppointmentFromSelectedDate(listAppointmentId);
        }

        /// <summary>
        /// Count appointment by time slot
        /// </summary>
        /// <param name="appointmentId">appointment id</param>
        /// <returns>int</returns>
        [HttpGet]
        [Route("api/Appointment/CountByTimeslot")]
        public int CountByTimeslot(string appointmentId)
        {
            var guid = Guid.Empty;
            if (Guid.TryParse(appointmentId, out guid))
            {
                var appointment = new DAL.DAL_Appointments().GetMyAppointmentByID(guid);
                return new DAL.DAL_Appointments().CountListAppointmentByTimeslot(appointment);
            }
            return 0;


        }

        /// <summary>
        /// Get all appointment
        /// </summary>
        /// <returns>List Appointment</returns>
        [HttpGet]
        [Route("api/Appointment/GetAll")]
        public List<Trinity.BE.Appointment> GetAllAppointments()
        {
            return new DAL.DAL_Appointments().GetAllAppointments();
        }

        /// <summary>
        /// Get all statistics
        /// </summary>
        /// <returns>List Statistics</returns>
        [HttpGet]
        [Route("api/Appointment/GetAllStatistics")]
        public List<Trinity.BE.Statistics> GetAllStatistics()
        {
            return new DAL.DAL_Appointments().GetAllStatistics();
        }

        /// <summary>
        /// Count booked appointment by timeslot
        /// </summary>
        /// <param name="timeslotId">time slot id</param>
        /// <returns>int</returns>
        [HttpGet]
        [Route("api/Appointment/CountBookedByTimeslot")]
        public int CountBookedByTimeslot(string timeslotId)
        {
            return new DAL.DAL_Appointments().CountAppointmentBookedByTimeslot(timeslotId);
        }

        /// <summary>
        /// Count reported appointment by time slot
        /// </summary>
        /// <param name="timeslotId">time slot id</param>
        /// <returns>int</returns>
        [HttpGet]
        [Route("api/Appointment/CountReportedByTimeslot")]
        public int CountReportedByTimeslot(string timeslotId)
        {
            return new DAL.DAL_Appointments().CountAppointmentReportedByTimeslot(timeslotId);
        }

        /// <summary>
        /// Count no show appointment by time slot
        /// </summary>
        /// <param name="timeslotId">time slot id</param>
        /// <returns>int</returns>
        [HttpGet]
        [Route("api/Appointment/CountNoShowdByTimeslot")]
        public int CountNoShowdByTimeslot(string timeslotId)
        {
            return new DAL.DAL_Appointments().CountAppointmentNoShowByTimeslot(timeslotId);
        }

        /// <summary>
        /// Get maximum queue number of time slot
        /// </summary>
        /// <param name="timeslotId">time slot id </param>
        /// <returns>int</returns>
        [HttpGet]
        [Route("api/Appointment/GetMaximumNumberOfTimeslot")]
        public int GetMaximumNumberOfTimeslot(string timeslotId)
        {
            return new DAL.DAL_Appointments().GetMaximumNumberOfTimeslot(timeslotId);
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
        public Trinity.DAL.DBContext.Appointment UpdateTimeslot(string appointmentId, string timeslotID)
        {
            Guid appointmentGuid = Guid.Empty;

            if (Guid.TryParse(appointmentId, out appointmentGuid))
            {
                return new DAL.DAL_Appointments().UpdateTimeslotForAppointment(appointmentGuid, timeslotID);
            }
            return null;

        }
    }
}
