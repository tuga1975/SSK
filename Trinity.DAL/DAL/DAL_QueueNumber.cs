﻿using System;
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
            var generateQNo = Trinity.Common.CommonUtil.GetQueueNumber(_localUnitOfWork.DataContext.Membership_Users.Find(userId).NRIC);
            var listStation = EnumStations.GetListStation();
            var today = DateTime.Now;
            var result = new DAL_Appointments().GetAppointmentDetails(appointmentID);
            var appointmentDetails = result.Data;
            var diffHour = appointmentDetails.StartTime.Value.Hours - today.Hour;
            var diffStartMin = appointmentDetails.StartTime.Value.Minutes - today.Minute;
            var diffEndHour = appointmentDetails.EndTime.Value.Hours - today.Hour;
            var diffEndMin = appointmentDetails.EndTime.Value.Minutes - today.Minute;

            Trinity.DAL.DBContext.Queue dataInsert = new Trinity.DAL.DBContext.Queue()
            {
                Appointment_ID = appointmentID,
                Queue_ID = Guid.NewGuid(),
                CreatedTime = DateTime.Now,
                QueuedNumber = generateQNo,
                CurrentStation = station,
                Outcome = EnumOutcome.Processing
            };
            //insert to queue details
            List<QueueDetail> arrayQueueDetail = new List<QueueDetail>();
            foreach (var item in listStation)
            {
                var queueDetails = new QueueDetail { Queue_ID = dataInsert.Queue_ID, Station = item, Status = EnumQueueStatuses.Waiting };
                if(queueDetails.Station == station && diffHour == 0 && diffStartMin <= 0 && ((diffEndHour > 0 && diffEndMin <= 0) || (diffEndHour == 0 && diffEndMin >= 0)))
                {
                    queueDetails.Status = EnumQueueStatuses.Processing;
                }
                arrayQueueDetail.Add(queueDetails);

                //_localUnitOfWork.GetRepository<Trinity.DAL.DBContext.QueueDetail>().Add(queueDetails);
                //_localUnitOfWork.Save();
            }
            _localUnitOfWork.GetRepository<Trinity.DAL.DBContext.Queue>().Add(dataInsert);
            _localUnitOfWork.GetRepository<Trinity.DAL.DBContext.QueueDetail>().AddRange(arrayQueueDetail);

            //var queueDetailsRepo = _localUnitOfWork.GetRepository<QueueDetail>();

            //if (diffHour == 0 && diffStartMin <= 0 && ((diffEndHour > 0 && diffEndMin <= 0) || (diffEndHour == 0 && diffEndMin >= 0)))
            //{
            //    var currentStationQueue = queueDetailsRepo.Get(s => s.Station == station && s.Queue_ID == dataInsert.Queue_ID);
            //    if (currentStationQueue != null)
            //    {
            //        currentStationQueue.Status = EnumQueueStatuses.Processing;
            //    }
            //    queueDetailsRepo.Update(currentStationQueue);

            //}
            _localUnitOfWork.Save();

            return dataInsert;
        }

        public Trinity.DAL.DBContext.QueueDetail GetQueueDetailByAppointent(Appointment appointment,string station)
        {
            var today = DateTime.Now;
            var queueDetail= _localUnitOfWork.DataContext.Queues.Include("QueueDetails").Where(q => q.Appointment_ID == appointment.ID && DbFunctions.TruncateTime(appointment.Date) == today.Date).Select(qd=>qd.QueueDetails.FirstOrDefault(s=>s.Station==station)).FirstOrDefault();
            return queueDetail;
        }

        public List<Trinity.DAL.DBContext.Queue> GetAllQueueNumberByDate(DateTime date, string station)
        {
            date = date.Date;
            var listDbQueue = _localUnitOfWork.DataContext.QueueDetails.Include("Queue").Where(d => d.Station == station && (d.Status.Equals(EnumQueueStatuses.Waiting, StringComparison.InvariantCultureIgnoreCase) || d.Status.Equals(EnumQueueStatuses.Processing, StringComparison.InvariantCultureIgnoreCase)) && DbFunctions.TruncateTime(d.Queue.CreatedTime).Value == date).ToList().Select(d=>d.Queue).ToList();
            //var listDbQueue = (from apm in _localUnitOfWork.DataContext.Appointments
            //                   join usr in _localUnitOfWork.DataContext.Membership_Users
            //                     on apm.UserId equals usr.UserId
            //                   join q in _localUnitOfWork.DataContext.Queues
            //                    on apm.ID equals q.Appointment_ID
            //                   join qd in _localUnitOfWork.DataContext.QueueDetails
            //                   on q.Queue_ID equals qd.Queue_ID
            //                   join ts in _localUnitOfWork.DataContext.Timeslots
            //                   on apm.Timeslot_ID equals ts.Timeslot_ID
            //                   where DbFunctions.TruncateTime(q.CreatedTime).Value == date && qd.Station == station && 
            //                   select q).ToList();

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

        /// <summary>
        ///      Return the list of supervisees those who have to report by today but are being blocked because of absence more than 3 times
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public List<string> GetHoldingListByDate(DateTime date)
        {
            date = date.Date;

            //var listDbQueue = (from apm in _localUnitOfWork.DataContext.Appointments
            //                   join usr in _localUnitOfWork.DataContext.Membership_Users
            //                     on apm.UserId equals usr.UserId
            //                   join q in _localUnitOfWork.DataContext.Queues
            //                    on apm.ID equals q.Appointment_ID
            //                   join qd in _localUnitOfWork.DataContext.QueueDetails
            //                   on q.Queue_ID equals qd.Queue_ID
            //                   join ts in _localUnitOfWork.DataContext.Timeslots
            //                   on apm.Timeslot_ID equals ts.Timeslot_ID
            //                   where DbFunctions.TruncateTime(q.CreatedTime).Value == date && qd.Station == station && usr.Status.Equals(EnumUserStatuses.Blocked, StringComparison.InvariantCultureIgnoreCase)
            //                   select q).Distinct().ToList();

            List<string> holdingList = _localUnitOfWork.DataContext.Appointments.Include("Membership_Users").Where(d => DbFunctions.TruncateTime(d.Date).Value == date && d.Membership_Users.Status.Equals(EnumUserStatuses.Blocked, StringComparison.InvariantCultureIgnoreCase)).Select(d => d.Membership_Users.NRIC).Distinct().ToList();
            //List<string> holdingList = (from apm in _localUnitOfWork.DataContext.Appointments
            //                            join usr in _localUnitOfWork.DataContext.Membership_Users
            //                            on apm.UserId equals usr.UserId
            //                            where DbFunctions.TruncateTime(apm.Date).Value == date && usr.Status.Equals(EnumUserStatuses.Blocked, StringComparison.InvariantCultureIgnoreCase)
            //                            select usr.NRIC).Distinct().ToList();

            return holdingList;
        }

        public BE.QueueInfo GetQueueInfoByQueueID(Guid queue_ID)
        {
            var dbQueue = _localUnitOfWork.DataContext.Queues.Include("Appointment").Include("Appointment.Membership_Users").FirstOrDefault(q => q.Queue_ID == queue_ID);
            var queueDetails = _localUnitOfWork.DataContext.QueueDetails.Where(qd => qd.Queue_ID == queue_ID).ToList().Select(d => d.Map<BE.QueueDetail>()).ToList();

            BE.QueueInfo queueInfo = new BE.QueueInfo();
            if (dbQueue != null)
            {
                queueInfo.Queue_ID = dbQueue.Queue_ID;
                queueInfo.NRIC = dbQueue.Appointment.Membership_Users.NRIC;
                queueInfo.Name = dbQueue.Appointment.Membership_Users.Name;
                queueInfo.CurrentStation = dbQueue.CurrentStation;
                queueInfo.Status = queueDetails.FirstOrDefault(qd => qd.Station.Equals(dbQueue.CurrentStation)).Status;
                queueInfo.QueueDetail = queueDetails.Where(qd => qd.Message != null && qd.Message != "").ToList();
            }

            return queueInfo;
        }

        public void UpdateQueueStatusByUserId(string userId, string currentStation, string nextStation, string status, string outcome)
        {
            DBContext.Queue dbQueue = _localUnitOfWork.DataContext.Queues.Include("Appointment").FirstOrDefault(d => d.Appointment.UserId == userId);
            if (dbQueue != null)
            {
                dbQueue.CurrentStation = nextStation;
                dbQueue.Outcome = outcome;
                DBContext.QueueDetail dbQueueDetail = _localUnitOfWork.DataContext.QueueDetails.FirstOrDefault(d => d.Queue_ID == dbQueue.Queue_ID && d.Station == currentStation);
                if (dbQueueDetail != null)
                {
                    dbQueueDetail.Status = status;
                    _localUnitOfWork.GetRepository<DBContext.QueueDetail>().Update(dbQueueDetail);
                }
                _localUnitOfWork.GetRepository<DBContext.Queue>().Update(dbQueue);
            }
        }
    }
}
