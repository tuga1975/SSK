using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Trinity.DAL.DBContext;
using Trinity.DAL.Repository;


namespace Trinity.DAL
{
    public class DAL_QueueNumber
    {
        Local_UnitOfWork _localUnitOfWork = new Local_UnitOfWork();
        Centralized_UnitOfWork _centralizedUnitOfWork = new Centralized_UnitOfWork();

        public Trinity.DAL.DBContext.Queue InsertQueueNumber(Guid appointmentID, string userId, string station)
        {
            var generateQNo = GenerateQueueNumber(_localUnitOfWork.DataContext.Membership_Users.Find(userId).NRIC);
            Trinity.DAL.DBContext.Queue dataInsert = new Trinity.DAL.DBContext.Queue()
            {
                Appointment_ID = appointmentID,
                Queue_ID = Guid.NewGuid(),
                CreatedTime = DateTime.Now,
                QueuedNumber = generateQNo,
                CurrentStation = station,
                Outcome = EnumOutcome.Processing
            };
            _localUnitOfWork.GetRepository<Trinity.DAL.DBContext.Queue>().Add(dataInsert);
            _localUnitOfWork.Save();
            //insert to queue details
            var listStation = EnumStations.GetListStation();
            var today = DateTime.Now;
            var appointmentDetails = new DAL_Appointments().GetAppointmentDetails(appointmentID);
            var diffHour = appointmentDetails.StartTime.Value.Hours - today.Hour;
            var diffStartMin = appointmentDetails.StartTime.Value.Minutes - today.Minute;
            var diffEndHour = appointmentDetails.EndTime.Value.Hours - today.Hour;
            var diffEndMin = appointmentDetails.EndTime.Value.Minutes - today.Minute;
            foreach (var item in listStation)
            {
                var queueDetails = new QueueDetail { Queue_ID = dataInsert.Queue_ID, Station = item, Status = EnumQueueStatuses.Waiting };

                _localUnitOfWork.GetRepository<Trinity.DAL.DBContext.QueueDetail>().Add(queueDetails);
                _localUnitOfWork.Save();
            }
            var queueDetailsRepo = _localUnitOfWork.GetRepository<QueueDetail>();

            if (diffHour == 0 && diffStartMin <= 0 && ((diffEndHour > 0 && diffEndMin <= 0) || (diffEndHour == 0 && diffEndMin >= 0)))
            {
                var currentStationQueue = queueDetailsRepo.Get(s => s.Station == station && s.Queue_ID == dataInsert.Queue_ID);
                if (currentStationQueue != null)
                {
                    currentStationQueue.Status = EnumQueueStatuses.Processing;
                }
                queueDetailsRepo.Update(currentStationQueue);

            }
            _localUnitOfWork.Save();

            return dataInsert;
        }


        public string GenerateQueueNumber(string baseOnNRIC)
        {
            string queueNumber = "";
            if (!string.IsNullOrEmpty(baseOnNRIC) && baseOnNRIC.Length > 6)
            {
                queueNumber += baseOnNRIC.Substring(0, 1) + baseOnNRIC.Substring(baseOnNRIC.Length - 5, 5).PadLeft(8, '*');
            }
            else if (!string.IsNullOrEmpty(baseOnNRIC) && baseOnNRIC.Length <= 6)
            {
                queueNumber += baseOnNRIC.Substring(0, 1) + baseOnNRIC.PadLeft(8, '*');
            }
            else
            {
                queueNumber += null;
            }

            return queueNumber;

        }
        public List<Trinity.DAL.DBContext.Queue> GetAllQueueNumberByDate(DateTime date, string station)
        {
            date = date.Date;

            var listDbQueue = (from apm in _localUnitOfWork.DataContext.Appointments
                               join usr in _localUnitOfWork.DataContext.Membership_Users
                                 on apm.UserId equals usr.UserId
                               join q in _localUnitOfWork.DataContext.Queues
                                on apm.ID equals q.Appointment_ID
                               join qd in _localUnitOfWork.DataContext.QueueDetails
                               on q.Queue_ID equals qd.Queue_ID
                               join ts in _localUnitOfWork.DataContext.Timeslots
                               on apm.Timeslot_ID equals ts.Timeslot_ID
                               where DbFunctions.TruncateTime(q.CreatedTime).Value == date && qd.Station == station && (qd.Status.Equals(EnumQueueStatuses.Waiting, StringComparison.InvariantCultureIgnoreCase) || qd.Status.Equals(EnumQueueStatuses.Processing, StringComparison.InvariantCultureIgnoreCase))
                               select q).ToList();

            return listDbQueue;
        }

        public List<Trinity.DAL.DBContext.Queue> GetAllQueueByNextimeslot(TimeSpan timeSlot, string station)
        {

            var listDbQueue = (from apm in _localUnitOfWork.DataContext.Appointments
                               join usr in _localUnitOfWork.DataContext.Membership_Users
                                 on apm.UserId equals usr.UserId
                               join q in _localUnitOfWork.DataContext.Queues
                                on apm.ID equals q.Appointment_ID
                               join qd in _localUnitOfWork.DataContext.QueueDetails
                               on q.Queue_ID equals qd.Queue_ID
                               join ts in _localUnitOfWork.DataContext.Timeslots
                               on apm.Timeslot_ID equals ts.Timeslot_ID
                               where ts.StartTime.Value == timeSlot && qd.Station == station && qd.Status.Equals(EnumQueueStatuses.Waiting, StringComparison.InvariantCultureIgnoreCase)
                               select q).ToList();

            return listDbQueue;
        }

        public string GetQueueStatusByStation(Guid queueId, string station)
        {
            var dbQueueDetail = _localUnitOfWork.DataContext.QueueDetails.FirstOrDefault(qd => qd.Queue_ID == queueId && qd.Station == station);
            if (dbQueueDetail != null)
            {
                return dbQueueDetail.Status;
            }
            return null;
        }


        public List<Trinity.DAL.DBContext.Queue> GetAllQueueByDateIncludeDetail(DateTime today)
        {
            today = today.Date;
            return _localUnitOfWork.DataContext.Queues.Include("QueueDetails").Include("Appointment").Include("Appointment.Membership_Users").Where(d => DbFunctions.TruncateTime(d.CreatedTime).Value == today).OrderByDescending(d => d.CreatedTime).ToList();
        }

        public bool CheckQueueExistToday(string userId, string station)
        {
            if (CountQueueByStatus(userId, EnumQueueStatuses.Waiting, station) > 0 || CountQueueByStatus(userId, EnumQueueStatuses.Processing, station) > 0)
            {
                return true;
            }
            if (CountQueueByStatus(userId, EnumQueueStatuses.Missed, station) > 0)
            {
                return false;
            }
            return false;


        }

        public int CountQueueByStatus(string userId, string status, string station)
        {
            var today = DateTime.Now.Date;
            return _localUnitOfWork.DataContext.Queues.Include(u => u.Appointment).Include(q => q.QueueDetails).Count(d => DbFunctions.TruncateTime(d.CreatedTime).Value == today && d.Appointment.UserId == userId && d.QueueDetails.FirstOrDefault(qd => qd.Station == station) != null && d.QueueDetails.FirstOrDefault(qd => qd.Station == station).Status == status);
        }

        public void UpdateQueueStatus(Guid queueId, string status, string station)
        {
            var queueDetailRepo = _localUnitOfWork.GetRepository<Trinity.DAL.DBContext.QueueDetail>();
            var dbqueueDetail = queueDetailRepo.Get(q => q.Queue_ID == queueId && q.Station == station);
            dbqueueDetail.Status = status;
            queueDetailRepo.Update(dbqueueDetail);
            _localUnitOfWork.Save();
        }

        public void UpdateQueueDetailMessage(Guid queueId, string message, string station)
        {
            var queueDetailRepo = _localUnitOfWork.GetRepository<Trinity.DAL.DBContext.QueueDetail>();
            var dbqueueDetail = queueDetailRepo.Get(q => q.Queue_ID == queueId && q.Station == station);
            dbqueueDetail.Message = message;
            queueDetailRepo.Update(dbqueueDetail);
            _localUnitOfWork.Save();
        }
        public void UpdateQueueCurrentStation(Guid queueId, string station)
        {
            var queueRepo = _localUnitOfWork.GetRepository<Trinity.DAL.DBContext.Queue>();
            var dbQueue = queueRepo.Get(q => q.Queue_ID == queueId);
            dbQueue.CurrentStation = station;
            queueRepo.Update(dbQueue);
            _localUnitOfWork.Save();
        }

        public List<Trinity.DAL.DBContext.Queue> GetAllQueueNumberByBlockedUser(DateTime date, string station)
        {
            date = date.Date;

            var listDbQueue = (from apm in _localUnitOfWork.DataContext.Appointments
                               join usr in _localUnitOfWork.DataContext.Membership_Users
                                 on apm.UserId equals usr.UserId
                               join q in _localUnitOfWork.DataContext.Queues
                                on apm.ID equals q.Appointment_ID
                               join qd in _localUnitOfWork.DataContext.QueueDetails
                               on q.Queue_ID equals qd.Queue_ID
                               join ts in _localUnitOfWork.DataContext.Timeslots
                               on apm.Timeslot_ID equals ts.Timeslot_ID
                               where DbFunctions.TruncateTime(q.CreatedTime).Value == date && qd.Station == station && usr.Status.Equals(EnumUserStatuses.Blocked, StringComparison.InvariantCultureIgnoreCase)
                               select q).Distinct().ToList();

            return listDbQueue;
        }
    }
}
