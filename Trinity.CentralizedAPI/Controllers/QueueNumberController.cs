using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Trinity.CentralizedAPI.Controllers
{
    public class QueueNumberController : ApiController
    {
        [HttpPost]
        [Route("api/QueueNumber/InsertNewQueue")]
        public IHttpActionResult InsertNewQueue(string appointmentId, string userId, string station)
        {
            var guid = Guid.Empty;
            if (Guid.TryParse(appointmentId, out guid))
            {

                var result = new DAL.DAL_QueueNumber().InsertQueueNumber(guid, userId, station);
                return Ok(result);
            }
            return null;

        }

        [HttpGet]
        [Route("api/QueueNumber/GetQueueDetailByAppointment")]
        public IHttpActionResult GetQueueDetailByAppointment(string appointmentId, string station)
        {
            var guid = Guid.Empty;
            if (Guid.TryParse(appointmentId, out guid))
            {
                var appointment = new DAL.DAL_Appointments().GetAppointmentByID(guid);
                var result = new DAL.DAL_QueueNumber().GetQueueDetailByAppointment(appointment, station);
                return Ok(result);
            }
            return null;

        }

        //[HttpGet]
        //[Route("api/QueueNumber/GetAllQueueNumberByDate")]
        //public IHttpActionResult GetAllQueueNumberByDate(string date, string station)
        //{
        //    var _date = DateTime.Now;
        //    if (DateTime.TryParse(date, out _date))
        //    {
        //        var result = new DAL.DAL_QueueNumber().GetAllQueueNumberByDate(_date, station);
        //        return Ok(result);
        //    }
        //    return null;

        //}

        [HttpGet]
        [Route("api/QueueNumber/GetAllQueueByNextimeslot")]
        public IHttpActionResult GetAllQueueByNextimeslot(string timeSlot, string station)
        {
            var _timeSlot = new TimeSpan();
            if (TimeSpan.TryParse(timeSlot, out _timeSlot))
            {
                var result = new DAL.DAL_QueueNumber().GetAllQueueByNextimeslot(_timeSlot, station);
                return Ok(result);
            }
            return null;

        }

        [HttpGet]
        [Route("api/QueueNumber/GetQueueStatusByStation")]
        public IHttpActionResult GetQueueStatusByStation(string queueId, string station)
        {
            var _queueId = Guid.Empty;
            if (Guid.TryParse(queueId, out _queueId))
            {
                var result = new DAL.DAL_QueueNumber().GetQueueStatusByStation(_queueId, station);
                return Ok(result);
            }
            return null;

        }

        [HttpPost]
        [Route("api/QueueNumber/UpdateQueueStatus")]
        public IHttpActionResult UpdateQueueStatus(string queueId, string status, string station)
        {
            var _queueId = Guid.Empty;
            if (Guid.TryParse(queueId, out _queueId))
            {
                var result = new DAL.DAL_QueueNumber().UpdateQueueStatus(_queueId, status, station);
                return Ok(result);
            }
            return null;

        }

        [HttpPost]
        [Route("api/QueueNumber/UpdateQueueCurrentStation")]
        public IHttpActionResult UpdateQueueCurrentStation(string queueId, string station)
        {
            var _queueId = Guid.Empty;
            if (Guid.TryParse(queueId, out _queueId))
            {
                var result = new DAL.DAL_QueueNumber().UpdateQueueCurrentStation(_queueId, station);
                return Ok(result);
            }
            return null;

        }

        [HttpPost]
        [Route("api/QueueNumber/UpdateQueueDetailMessage")]
        public IHttpActionResult UpdateQueueDetailMessage(string queueId, string message, string station)
        {
            var _queueId = Guid.Empty;
            if (Guid.TryParse(queueId, out _queueId))
            {
                var result = new DAL.DAL_QueueNumber().UpdateQueueDetailMessage(_queueId, message, station);
                return Ok(result);
            }
            return null;

        }
        [HttpGet]
        [Route("api/QueueNumber/GetHoldingListByDate")]
        public IHttpActionResult GetHoldingListByDate(string date)
        {
            var _date = DateTime.Now;
            if (DateTime.TryParse(date, out _date))
            {
                var result = new DAL.DAL_QueueNumber().GetHoldingListByDate(_date);
                return Ok(result);
            }
            return null;

        }

        [HttpGet]
        [Route("api/QueueNumber/GetQueueInfoByQueueID")]
        public IHttpActionResult GetQueueInfoByQueueID(string queueId)
        {
            var _queueId = Guid.Empty;
            if (Guid.TryParse(queueId, out _queueId))
            {
                var result = new DAL.DAL_QueueNumber().GetQueueInfoByQueueID(_queueId);
                return Ok(result);
            }
            return null;

        }

        [HttpPost]
        [Route("api/QueueNumber/UpdateQueueStatusByUserId")]
        public IHttpActionResult UpdateQueueStatusByUserId(string userId, string currentStation, string nextStation, string status, string outcome)
        {
            var result = new DAL.DAL_QueueNumber().UpdateQueueStatusByUserId(userId, currentStation, nextStation, status, outcome);
            return Ok(result);
        }

    }
}

