using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Trinity.BE;
using Trinity.Common;
using Trinity.DAL.Repository;


namespace Trinity.DAL
{
    public class DAL_QueueNumber
    {
        Local_UnitOfWork _localUnitOfWork = new Local_UnitOfWork();
        Centralized_UnitOfWork _centralizedUnitOfWork = new Centralized_UnitOfWork();

        #region refactor 2018


        public DBContext.Timeslot GetTimeSlotEmpty()
        {
            List<DBContext.Timeslot> arrayTimeslot = _localUnitOfWork.DataContext.Timeslots.Where(d => d.Date == DateTime.Today).ToList().OrderBy(d => d.SortCategory).ThenBy(d => d.StartTime).ToList();
            DBContext.Timeslot timeslotReturn = null;
            while (timeslotReturn == null && arrayTimeslot.Count > 0)
            {
                DBContext.Timeslot time = arrayTimeslot[0];
                arrayTimeslot.RemoveAt(0);
                if (time.EndTime > DateTime.Now.TimeOfDay)
                {
                    int totalSlot = _localUnitOfWork.DataContext.Appointments.Count(d => !string.IsNullOrEmpty(d.Timeslot_ID) && d.Timeslot_ID == time.Timeslot_ID) + _localUnitOfWork.DataContext.Queues.Count(d => d.Timeslot_ID == time.Timeslot_ID && (!d.Appointment_ID.HasValue || (d.Appointment_ID.HasValue && !string.IsNullOrEmpty(d.Appointment.Timeslot_ID) && d.Appointment.Timeslot_ID != time.Timeslot_ID)));
                    if (totalSlot < time.MaximumSupervisee)
                    {
                        timeslotReturn = time;
                    }
                }
            }
            return timeslotReturn;

        }
        public List<Trinity.DAL.DBContext.Queue> SSKGetQueueToDay()
        {

            return _localUnitOfWork.DataContext.Queues.Where(item => DbFunctions.TruncateTime(item.CreatedTime).Value == DateTime.Today).ToList();
        }

        public Queue GetMyQueueToday(string UserId)
        {
            DateTime today = DateTime.Today;
            return _localUnitOfWork.DataContext.Queues.Include("Appointment").FirstOrDefault(d => d.Appointment.UserId == UserId && DbFunctions.TruncateTime(d.Appointment.Date).Value == today).Map<Queue>();
        }
        public bool IsInQueue(string appointment_ID, string station)
        {
            if (EnumAppConfig.IsLocal)
            {
                return _localUnitOfWork.DataContext.Queues.Any(item => item.Appointment_ID.ToString() == appointment_ID);
            }

            return false;
        }
        public bool IsUserAlreadyQueue(string UserID, DateTime date)
        {
            date = date.Date;
            if (EnumAppConfig.IsLocal)
            {
                return _localUnitOfWork.DataContext.Queues.Any(item => item.UserId == UserID && DbFunctions.TruncateTime(item.CreatedTime) == date);
            }

            return false;
        }
        #endregion


        public Trinity.DAL.DBContext.Queue InsertQueueNumberFromDO(string UserID, string station, string userCreateQueue)
        {
            var timeslot = GetTimeSlotEmpty();
            if (timeslot == null)
            {
                throw new Exception("Sorry all timeslots are fully booked!");
            }
            else
            {
                var listStation = EnumStation.GetListStation();
                var generateQNo = Trinity.Common.CommonUtil.GetQueueNumber(_localUnitOfWork.DataContext.Membership_Users.Find(UserID).NRIC);
                Trinity.DAL.DBContext.Queue dataInsert = new Trinity.DAL.DBContext.Queue()
                {
                    Queue_ID = Guid.NewGuid(),
                    UserId = UserID,
                    Timeslot_ID = timeslot.Timeslot_ID,
                    CurrentStation = station,
                    Outcome = EnumOutcome.GetQueue,
                    CreatedTime = DateTime.Now,
                    QueuedNumber = generateQNo,
                    Created_By = userCreateQueue
                };
                List<Trinity.DAL.DBContext.QueueDetail> arrayQueueDetail = new List<Trinity.DAL.DBContext.QueueDetail>();
                foreach (var item in listStation)
                {
                    var queueDetails = new Trinity.DAL.DBContext.QueueDetail { Queue_ID = dataInsert.Queue_ID, Station = item, Status = EnumQueueStatuses.Waiting };
                    if (queueDetails.Station == EnumStation.APS)
                        queueDetails.Status = EnumQueueStatuses.Finished;
                    arrayQueueDetail.Add(queueDetails);

                }
                _localUnitOfWork.GetRepository<Trinity.DAL.DBContext.Queue>().Add(dataInsert);
                _localUnitOfWork.GetRepository<Trinity.DAL.DBContext.QueueDetail>().AddRange(arrayQueueDetail);
                _localUnitOfWork.Save();
                return dataInsert;
            }
        }

        public Trinity.DAL.DBContext.Queue InsertQueueNumber(Guid appointmentID, string userId, string station, string userCreateQueue)
        {
            try
            {
                var generateQNo = Trinity.Common.CommonUtil.GetQueueNumber(_localUnitOfWork.DataContext.Membership_Users.Find(userId).NRIC);
                var listStation = EnumStation.GetListStation();
                var today = DateTime.Now;
                var appointment = new DAL_Appointments().GetAppmtDetails(appointmentID);


                var timeslot = GetTimeSlotEmpty();
                string timeslotID = string.Empty;
                if (timeslot != null && timeslot.EndTime < appointment.EndTime)
                {
                    timeslotID = timeslot.Timeslot_ID;
                }
                else if (appointment.EndTime > DateTime.Now.TimeOfDay)
                {
                    timeslotID = appointment.Timeslot_ID;
                }
                else if (timeslot != null && appointment.EndTime < DateTime.Now.TimeOfDay)
                {
                    timeslotID = timeslot.Timeslot_ID;
                }

                if (string.IsNullOrEmpty(timeslotID))
                {
                    throw new Exception("Sorry all timeslots are fully booked!");
                }

                Trinity.DAL.DBContext.Queue dataInsert = new Trinity.DAL.DBContext.Queue()
                {
                    Queue_ID = Guid.NewGuid(),
                    UserId = userId,
                    Timeslot_ID = timeslotID,
                    Appointment_ID = appointmentID,
                    CurrentStation = station,
                    Outcome = EnumOutcome.GetQueue,
                    CreatedTime = DateTime.Now,
                    QueuedNumber = generateQNo,
                    Created_By = userCreateQueue
                };
                //insert to queue details
                List<Trinity.DAL.DBContext.QueueDetail> arrayQueueDetail = new List<Trinity.DAL.DBContext.QueueDetail>();
                foreach (var item in listStation)
                {
                    var queueDetails = new Trinity.DAL.DBContext.QueueDetail { Queue_ID = dataInsert.Queue_ID, Station = item, Status = EnumQueueStatuses.Waiting };
                    if (queueDetails.Station == EnumStation.APS)
                        queueDetails.Status = EnumQueueStatuses.Finished;
                    arrayQueueDetail.Add(queueDetails);

                }
                if (EnumAppConfig.IsLocal)
                {
                    _localUnitOfWork.GetRepository<Trinity.DAL.DBContext.Queue>().Add(dataInsert);
                    _localUnitOfWork.GetRepository<Trinity.DAL.DBContext.QueueDetail>().AddRange(arrayQueueDetail);

                    Trinity.DAL.DBContext.Appointment appointmentUpdate = _localUnitOfWork.DataContext.Appointments.FirstOrDefault(d => d.ID == appointmentID);
                    appointmentUpdate.Status = EnumAppointmentStatuses.Reported;
                    _localUnitOfWork.GetRepository<Trinity.DAL.DBContext.Appointment>().Update(appointmentUpdate);

                    _localUnitOfWork.Save();
                    return dataInsert;
                }
                else
                {
                    _centralizedUnitOfWork.GetRepository<Trinity.DAL.DBContext.Queue>().Add(dataInsert);
                    _centralizedUnitOfWork.GetRepository<Trinity.DAL.DBContext.QueueDetail>().AddRange(arrayQueueDetail);
                    _centralizedUnitOfWork.Save();
                    return dataInsert;
                }

                return null;
            }
            catch (Exception ex)
            {

                return null;
            }
        }

        public void UpdateQueueStatus_SSK(string timeslot_ID)
        {
            try
            {
                var queues = _localUnitOfWork.DataContext.Queues
                    .Where(item => item.Appointment.Timeslot_ID == timeslot_ID
                    && item.QueueDetails.Any(x => x.Station == EnumStation.SSK)
                    && item.QueueDetails.Any(x => x.Status == EnumQueueStatuses.Waiting))
                    .Select(item => item.QueueDetails.Where(x => x.Station == EnumStation.SSK && x.Status == EnumQueueStatuses.Waiting).FirstOrDefault()).ToList();

                foreach (var item in queues)
                {
                    item.Status = EnumQueueStatuses.Processing;
                }

                _localUnitOfWork.DataContext.SaveChanges();
            }
            catch (Exception ex)
            {

            }
        }

        public List<BE.QueueDetail> GetAllQueue_SSK(DateTime date)
        {
            if (EnumAppConfig.IsLocal)
            {
                var data = _localUnitOfWork.DataContext.Queues.Where(item => DbFunctions.TruncateTime(item.CreatedTime).Value == date.Date).ToList()
                    .Select(item => new QueueDetail()
                    {
                        Queue_ID = item.Queue_ID,
                        Station = item.QueueDetails.FirstOrDefault(dt => dt.Station == EnumStation.SSK)?.Station,
                        Status = item.QueueDetails.FirstOrDefault(dt => dt.Station == EnumStation.SSK)?.Status,
                        Message = item.QueueDetails.FirstOrDefault(dt => dt.Station == EnumStation.SSK)?.Message,
                        QueuedNumber = item.QueuedNumber,
                        Timeslot_ID = item.Appointment.Timeslot_ID
                    }).ToList();

                return data;
            }
            else // request from centralizedapi
            {
                return null;
            }
        }

        public Trinity.DAL.DBContext.QueueDetail GetQueueDetailByAppointment(DAL.DBContext.Appointment appointment, string station)
        {
            try
            {
                var today = DateTime.Now;
                if (EnumAppConfig.IsLocal)
                {
                    var data = _localUnitOfWork.DataContext.Queues.Include("QueueDetails").Where(q => q.Appointment_ID == appointment.ID && DbFunctions.TruncateTime(appointment.Date) == today.Date).Select(qd => qd.QueueDetails.FirstOrDefault(s => s.Station == station)).FirstOrDefault();
                    if (data != null)
                    {
                        return data;
                    }
                    else
                    {
                        bool centralizeStatus;
                        var centralData = CallCentralized.Get<DAL.DBContext.QueueDetail>(EnumAPIParam.QueueNumber, "GetQueueDetailByAppointment", out centralizeStatus, "appointmentId=" + appointment.ID.ToString(), "station=" + station);
                        if (centralizeStatus)
                        {
                            return centralData;
                        }
                    }

                }
                else
                {
                    var data = _centralizedUnitOfWork.DataContext.Queues.Include("QueueDetails").Where(q => q.Appointment_ID == appointment.ID && DbFunctions.TruncateTime(appointment.Date) == today.Date).Select(qd => qd.QueueDetails.FirstOrDefault(s => s.Station == station)).FirstOrDefault();
                    if (data != null)
                    {
                        return data;
                    }
                }
                return null;
            }
            catch (Exception)
            {

                return null;
            }

        }

        //public List<Trinity.DAL.DBContext.Queue> GetAllQueueNumberByDate(DateTime date, string station)
        //{
        //    try
        //    {
        //        date = date.Date;
        //        if (EnumAppConfig.IsLocal)
        //        {
        //            var data = _localUnitOfWork.DataContext.QueueDetails.Include("Queue").Where(d => d.Station == station && (d.Status.Equals(EnumQueueStatuses.Waiting, StringComparison.InvariantCultureIgnoreCase) || d.Status.Equals(EnumQueueStatuses.Processing, StringComparison.InvariantCultureIgnoreCase)) && DbFunctions.TruncateTime(d.Queue.CreatedTime).Value == date).ToList().Select(d => d.Queue).ToList();
        //            if (data != null)
        //            {
        //                return data;
        //            }
        //            else
        //            {
        //                bool centralizeStatus;
        //                var centralData = CallCentralized.Get<List<Queue>>(EnumAPIParam.QueueNumber, "GetAllQueueNumberByDate", out centralizeStatus, "date=" + date.ToString(), "station=" + station);
        //                if (centralizeStatus)
        //                {
        //                    return centralData;
        //                }
        //            }
        //        }
        //        else
        //        {
        //            var data = _centralizedUnitOfWork.DataContext.QueueDetails.Include("Queue").Where(d => d.Station == station && (d.Status.Equals(EnumQueueStatuses.Waiting, StringComparison.InvariantCultureIgnoreCase) || d.Status.Equals(EnumQueueStatuses.Processing, StringComparison.InvariantCultureIgnoreCase)) && DbFunctions.TruncateTime(d.Queue.CreatedTime).Value == date).ToList().Select(d => d.Queue).ToList();

        //            if (data != null)
        //            {
        //                return data;
        //            }

        //        }
        //        return null;
        //    }
        //    catch (Exception)
        //    {

        //        return null;
        //    }
        //}

        public List<Trinity.DAL.DBContext.Queue> GetAllQueueByNextimeslot(TimeSpan timeSlot, string station)
        {
            try
            {
                if (EnumAppConfig.IsLocal)
                {
                    var data = (from apm in _localUnitOfWork.DataContext.Appointments
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
                    if (data != null)
                    {
                        return data;
                    }
                    else
                    {
                        bool centralizeStatus;
                        var centralData = CallCentralized.Get<List<DBContext.Queue>>(EnumAPIParam.QueueNumber, "GetAllQueueByNextimeslot", out centralizeStatus, "timeslot=" + timeSlot.ToString(), "station=" + station);
                        if (centralizeStatus)
                        {
                            return centralData;
                        }
                    }
                }
                else
                {
                    var data = (from apm in _centralizedUnitOfWork.DataContext.Appointments
                                join usr in _centralizedUnitOfWork.DataContext.Membership_Users
                                  on apm.UserId equals usr.UserId
                                join q in _localUnitOfWork.DataContext.Queues
                                 on apm.ID equals q.Appointment_ID
                                join qd in _localUnitOfWork.DataContext.QueueDetails
                                on q.Queue_ID equals qd.Queue_ID
                                join ts in _localUnitOfWork.DataContext.Timeslots
                                on apm.Timeslot_ID equals ts.Timeslot_ID
                                where ts.StartTime.Value == timeSlot && qd.Station == station && qd.Status.Equals(EnumQueueStatuses.Waiting, StringComparison.InvariantCultureIgnoreCase)
                                select q).ToList();
                    if (data != null)
                    {
                        return data;
                    }
                }
                return null;
            }
            catch (Exception)
            {

                return null;
            }

        }

        public string GetQueueStatusByStation(Guid queueId, string station)
        {
            try
            {
                if (EnumAppConfig.IsLocal)
                {
                    var data = _localUnitOfWork.DataContext.QueueDetails.FirstOrDefault(qd => qd.Queue_ID == queueId && qd.Station == station);
                    if (data != null)
                    {
                        return data.Status;
                    }
                    else
                    {
                        bool centralizeStatus;
                        var centralData = CallCentralized.Get<string>(EnumAPIParam.QueueNumber, "GetQueueStatusByStation", out centralizeStatus, "queueId=" + queueId.ToString(), "station=" + station);
                        if (centralizeStatus)
                        {
                            return centralData;
                        }
                    }

                }
                else
                {
                    var data = _centralizedUnitOfWork.DataContext.QueueDetails.FirstOrDefault(qd => qd.Queue_ID == queueId && qd.Station == station);
                    if (data != null)
                    {
                        return data.Status;
                    }
                }
                return null;
            }
            catch (Exception)
            {

                return string.Empty;
            }
        }


        public List<Trinity.DAL.DBContext.Queue> GetAllQueueByDateIncludeDetail(DateTime date)
        {
            date = date.Date;
            try
            {

                if (EnumAppConfig.IsLocal)
                {
                    var data = _localUnitOfWork.DataContext.Queues.Include("QueueDetails").Include("Appointment").Include("Appointment.Membership_Users").Where(d => DbFunctions.TruncateTime(d.CreatedTime).Value == date).OrderByDescending(d => d.CreatedTime).ToList();
                    if (data != null)
                    {
                        return data;
                    }
                    else
                    {
                        bool centralizeStatus;
                        var centralData = CallCentralized.Get<List<Trinity.DAL.DBContext.Queue>>(EnumAPIParam.QueueNumber, "GetAllQueueByDateIncludeDetail", out centralizeStatus, "date=" + date.ToString());
                        if (centralizeStatus)
                        {
                            return centralData;
                        }
                    }
                }
                else
                {
                    var data = _localUnitOfWork.DataContext.Queues.Include("QueueDetails").Include("Appointment").Include("Appointment.Membership_Users").Where(d => DbFunctions.TruncateTime(d.CreatedTime).Value == date).OrderByDescending(d => d.CreatedTime).ToList();

                    if (data != null)
                    {
                        return data;
                    }

                }
                return null;
            }
            catch (Exception)
            {

                return null;
            }
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
            try
            {
                var today = DateTime.Now.Date;
                if (EnumAppConfig.IsLocal)
                {
                    var data = _localUnitOfWork.DataContext.Queues.Include(u => u.Appointment).Include(q => q.QueueDetails).Where(d => DbFunctions.TruncateTime(d.CreatedTime).Value == today && d.Appointment.UserId == userId && d.QueueDetails.FirstOrDefault(qd => qd.Station == station) != null && d.QueueDetails.FirstOrDefault(qd => qd.Station == station).Status == status);
                    if (data != null)
                    {
                        return data.Count();
                    }
                    else
                    {
                        bool centralizeStatus;
                        var centralData = CallCentralized.Get<int>(EnumAPIParam.QueueNumber, "CountQueueByStatus", out centralizeStatus, "userId=" + userId, "status=" + status, "station=" + station);
                        if (centralizeStatus)
                        {
                            return centralData;
                        }
                    }
                }
                else
                {
                    var data = _centralizedUnitOfWork.DataContext.Queues.Include(u => u.Appointment).Include(q => q.QueueDetails).Where(d => DbFunctions.TruncateTime(d.CreatedTime).Value == today && d.Appointment.UserId == userId && d.QueueDetails.FirstOrDefault(qd => qd.Station == station) != null && d.QueueDetails.FirstOrDefault(qd => qd.Station == station).Status == status);
                    if (data != null)
                    {
                        return data.Count();
                    }
                }

                return 0;
            }
            catch (Exception)
            {

                return 0;
            }
        }

        public int UpdateQueueStatus(Guid queueId, string status, string station)
        {
            try
            {
                if (EnumAppConfig.IsLocal)
                {

                    bool centralizeStatus;
                    var centralData = CallCentralized.Post<int>(EnumAPIParam.QueueNumber, "UpdateQueueStatus", out centralizeStatus, "queueId=" + queueId.ToString(), "status=" + status, "station=" + station);
                    if (centralizeStatus)
                    {
                        var queueDetailRepo = _localUnitOfWork.GetRepository<Trinity.DAL.DBContext.QueueDetail>();
                        var dbqueueDetail = queueDetailRepo.Get(q => q.Queue_ID == queueId && q.Station == station);
                        dbqueueDetail.Status = status;
                        queueDetailRepo.Update(dbqueueDetail);
                        _localUnitOfWork.Save();

                        return centralData;
                    }

                }
                else
                {
                    var queueDetailRepo = _centralizedUnitOfWork.GetRepository<Trinity.DAL.DBContext.QueueDetail>();
                    var dbqueueDetail = queueDetailRepo.Get(q => q.Queue_ID == queueId && q.Station == station);
                    dbqueueDetail.Status = status;
                    queueDetailRepo.Update(dbqueueDetail);
                    return _centralizedUnitOfWork.Save();
                }
                return 0;
            }
            catch (Exception)
            {

                return 0;
            }

        }

        public int UpdateQueueDetailMessage(Guid queueId, string message, string station)
        {
            try
            {
                if (EnumAppConfig.IsLocal)
                {

                    bool centralizeStatus;
                    var centralData = CallCentralized.Post<int>(EnumAPIParam.QueueNumber, "UpdateQueueDetailMessage", out centralizeStatus, "queueId=" + queueId.ToString(), "message=" + message, "station=" + station);
                    if (centralizeStatus)
                    {
                        var queueDetailRepo = _localUnitOfWork.GetRepository<Trinity.DAL.DBContext.QueueDetail>();
                        var dbqueueDetail = queueDetailRepo.Get(q => q.Queue_ID == queueId && q.Station == station);
                        dbqueueDetail.Message = message;
                        queueDetailRepo.Update(dbqueueDetail);
                        _localUnitOfWork.Save();

                        return centralData;
                    }

                }
                else
                {
                    var queueDetailRepo = _centralizedUnitOfWork.GetRepository<Trinity.DAL.DBContext.QueueDetail>();
                    var dbqueueDetail = queueDetailRepo.Get(q => q.Queue_ID == queueId && q.Station == station);
                    dbqueueDetail.Message = message;
                    queueDetailRepo.Update(dbqueueDetail);
                    return _centralizedUnitOfWork.Save();
                }
                return 0;
            }
            catch (Exception)
            {

                return 0;
            }

        }
        public int UpdateQueueCurrentStation(Guid queueId, string station)
        {
            try
            {
                if (EnumAppConfig.IsLocal)
                {

                    bool centralizeStatus;
                    var centralData = CallCentralized.Post<int>(EnumAPIParam.QueueNumber, "UpdateQueueCurrentStation", out centralizeStatus, "queueId=" + queueId.ToString(), "station=" + station);
                    if (centralizeStatus)
                    {
                        var queueRepo = _localUnitOfWork.GetRepository<Trinity.DAL.DBContext.Queue>();
                        var dbQueue = queueRepo.Get(q => q.Queue_ID == queueId);
                        dbQueue.CurrentStation = station;
                        queueRepo.Update(dbQueue);
                        _localUnitOfWork.Save();

                        return centralData;
                    }

                }
                else
                {
                    var queueRepo = _centralizedUnitOfWork.GetRepository<Trinity.DAL.DBContext.Queue>();
                    var dbQueue = queueRepo.Get(q => q.Queue_ID == queueId);
                    dbQueue.CurrentStation = station;
                    queueRepo.Update(dbQueue);

                    return _centralizedUnitOfWork.Save();
                }
                return 0;
            }
            catch (Exception)
            {

                return 0;
            }
        }

        /// <summary>
        ///      Return the list of supervisees those who have to report by today but are being blocked because of absence more than 3 times
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public List<string> GetHoldingListByDate(DateTime date)
        {
            try
            {
                date = date.Date;
                if (EnumAppConfig.IsLocal)
                {
                    List<string> data = _localUnitOfWork.DataContext.Appointments.Include("Membership_Users")
                        .Where(d => DbFunctions.TruncateTime(d.Date).Value == date
                        && d.Membership_Users.Status.Equals(EnumUserStatuses.Blocked, StringComparison.InvariantCultureIgnoreCase))
                        .Select(d => d.Membership_Users.NRIC).Distinct().ToList();

                    if (data != null)
                    {
                        return data;
                    }
                    else
                    {
                        bool centralizeStatus;
                        var centralData = CallCentralized.Get<List<string>>(EnumAPIParam.QueueNumber, "GetHoldingListByDate", out centralizeStatus, "date=" + date.ToString());
                        if (centralizeStatus)
                        {
                            return centralData;
                        }
                    }

                }
                else
                {
                    List<string> holdingList = _centralizedUnitOfWork.DataContext.Appointments.Include("Membership_Users").Where(d => DbFunctions.TruncateTime(d.Date).Value == date && d.Membership_Users.Status.Equals(EnumUserStatuses.Blocked, StringComparison.InvariantCultureIgnoreCase)).Select(d => d.Membership_Users.NRIC).Distinct().ToList();

                    return holdingList;
                }
                return null;
            }
            catch (Exception)
            {

                return null;
            }
        }

        public BE.QueueInfo GetQueueInfoByQueueID(Guid queue_ID)
        {
            try
            {
                var dbQueue = new DBContext.Queue();
                var queueDetails = new List<BE.QueueDetail>();
                if (EnumAppConfig.IsLocal)
                {
                    var queueData = _localUnitOfWork.DataContext.Queues.Include("Appointment").Include("Appointment.Membership_Users").FirstOrDefault(q => q.Queue_ID == queue_ID);
                    var queueDataDetails = _localUnitOfWork.DataContext.QueueDetails.Where(qd => qd.Queue_ID == queue_ID).ToList().Select(d => d.Map<BE.QueueDetail>()).ToList();

                    if (queueData != null)
                    {
                        dbQueue = queueData;
                        queueDetails = queueDataDetails;
                        BE.QueueInfo queueInfo = SetQueueInfo(dbQueue, queueDetails);

                        return queueInfo;
                    }
                    else
                    {
                        bool centralizeStatus;
                        var centralData = CallCentralized.Get<BE.QueueInfo>(EnumAPIParam.QueueNumber, "GetQueueInfoByQueueID", out centralizeStatus, "queueId=" + queue_ID.ToString());
                        if (centralizeStatus)
                        {
                            return centralData;
                        }
                    }
                }
                else
                {
                    var queueData = _centralizedUnitOfWork.DataContext.Queues.Include("Appointment").Include("Appointment.Membership_Users").FirstOrDefault(q => q.Queue_ID == queue_ID);
                    var queueDataDetails = _centralizedUnitOfWork.DataContext.QueueDetails.Where(qd => qd.Queue_ID == queue_ID).ToList().Select(d => d.Map<BE.QueueDetail>()).ToList();
                    if (queueData != null)
                    {
                        dbQueue = queueData;
                        queueDetails = queueDataDetails;
                    }
                    BE.QueueInfo queueInfo = SetQueueInfo(dbQueue, queueDetails);

                    return queueInfo;
                }
                return null;


            }
            catch (Exception)
            {

                return null;
            }



        }

        private static BE.QueueInfo SetQueueInfo(DBContext.Queue dbQueue, List<BE.QueueDetail> queueDetails)
        {
            BE.QueueInfo queueInfo = new BE.QueueInfo();
            if (dbQueue != null)
            {
                queueInfo.Queue_ID = dbQueue.Queue_ID;
                queueInfo.NRIC = dbQueue.Appointment.Membership_Users.NRIC;
                queueInfo.Name = dbQueue.Appointment.Membership_Users.Name;
                queueInfo.CurrentStation = dbQueue.CurrentStation;
                queueInfo.Status = queueDetails.FirstOrDefault(qd => qd.Station.Equals(dbQueue.CurrentStation)).Status;
                queueInfo.QueueDetail = queueDetails.Where(qd => qd.Message != null && qd.Message != "").ToList();
                queueInfo.UserId = dbQueue.UserId;
            }

            return queueInfo;
        }

        public int UpdateQueueStatusByUserId(string userId, string currentStation, string statusCurrentStattion, string nextStation, string statusNextStation, string messageNextStation, string outcome)
        {
            try
            {
                if (EnumAppConfig.IsLocal)
                {
                    DBContext.Queue dbQueue = _localUnitOfWork.DataContext.Queues.Include("Appointment").FirstOrDefault(d => d.Appointment.UserId == userId);

                    if (dbQueue == null)
                        return 0;

                    dbQueue.CurrentStation = nextStation;
                    if (!string.IsNullOrEmpty(outcome))
                    {
                        dbQueue.Outcome = outcome;
                    }
                    DBContext.QueueDetail dbQueueDetailCurrent = _localUnitOfWork.DataContext.QueueDetails.FirstOrDefault(d => d.Queue_ID == dbQueue.Queue_ID && d.Station == currentStation);
                    if (dbQueueDetailCurrent != null)
                    {
                        dbQueueDetailCurrent.Status = statusCurrentStattion;
                        _localUnitOfWork.GetRepository<DBContext.QueueDetail>().Update(dbQueueDetailCurrent);
                    }

                    DBContext.QueueDetail dbQueueDetailNextStation = _localUnitOfWork.DataContext.QueueDetails.FirstOrDefault(d => d.Queue_ID == dbQueue.Queue_ID && d.Station == nextStation);
                    if (dbQueueDetailNextStation != null)
                    {
                        dbQueueDetailNextStation.Status = statusNextStation;
                        dbQueueDetailNextStation.Message = messageNextStation;
                        
                        _localUnitOfWork.GetRepository<DBContext.QueueDetail>().Update(dbQueueDetailNextStation);
                    }

                    _localUnitOfWork.GetRepository<DBContext.Queue>().Update(dbQueue);

                    var result = _localUnitOfWork.Save();

                    return result;
                    //if (EnumAppConfig.ByPassCentralizedDB)
                    //{
                    //    return result;
                    //}
                    //else
                    //{
                    //    bool centralizeStatus;
                    //    var centralData = CallCentralized.Post<int>(EnumAPIParam.QueueNumber, "UpdateQueueStatusByUserId", out centralizeStatus, "userId=" + userId, "currentStation=" + currentStation, "nextStation=" + nextStation, "status=" + status, "outcome=" + outcome);
                    //    if (centralizeStatus)
                    //    {
                    //        return centralData;
                    //    }
                    //    else
                    //    {
                    //        throw new Exception(EnumMessage.NotConnectCentralized);
                    //    }
                    //}
                }
                else
                {
                    DBContext.Queue dbQueue = _centralizedUnitOfWork.DataContext.Queues.Include("Appointment").FirstOrDefault(d => d.Appointment.UserId == userId);

                    if (dbQueue == null)
                        return 0;

                    dbQueue.CurrentStation = nextStation;
                    if (!string.IsNullOrEmpty(outcome))
                    {
                        dbQueue.Outcome = outcome;
                    }
                    DBContext.QueueDetail dbQueueDetailCurrent = _centralizedUnitOfWork.DataContext.QueueDetails.FirstOrDefault(d => d.Queue_ID == dbQueue.Queue_ID && d.Station == currentStation);
                    if (dbQueueDetailCurrent != null)
                    {
                        dbQueueDetailCurrent.Status = statusCurrentStattion;
                        _centralizedUnitOfWork.GetRepository<DBContext.QueueDetail>().Update(dbQueueDetailCurrent);
                    }

                    DBContext.QueueDetail dbQueueDetailNextStation = _centralizedUnitOfWork.DataContext.QueueDetails.FirstOrDefault(d => d.Queue_ID == dbQueue.Queue_ID && d.Station == nextStation);
                    if (dbQueueDetailNextStation != null)
                    {
                        dbQueueDetailNextStation.Status = statusNextStation;
                        dbQueueDetailNextStation.Message = messageNextStation;

                        _centralizedUnitOfWork.GetRepository<DBContext.QueueDetail>().Update(dbQueueDetailNextStation);
                    }

                    _centralizedUnitOfWork.GetRepository<DBContext.Queue>().Update(dbQueue);

                    var result = _centralizedUnitOfWork.Save();

                    return result;
                }

            }
            catch (Exception)
            {

                return 0;
            }

        }

        public int UpdateQueueOutcomeByQueueId(Guid queueId, string outcome)
        {
            try
            {
                if (EnumAppConfig.IsLocal)
                {
                    var queueRepo = _localUnitOfWork.GetRepository<Trinity.DAL.DBContext.Queue>();
                    var dbQueue = queueRepo.Get(q => q.Queue_ID == queueId);
                    if (dbQueue != null)
                    {
                        dbQueue.Outcome = outcome;
                        dbQueue.LastUpdatedDate = DateTime.Now;
                        queueRepo.Update(dbQueue);
                        _localUnitOfWork.Save();
                    }

                    if (!EnumAppConfig.ByPassCentralizedDB)
                    {
                        bool centralizeStatus;
                        var centralData = CallCentralized.Post<int>(EnumAPIParam.QueueNumber, "UpdateQueueOutcomeByQueueId", out centralizeStatus, "queueId=" + queueId.ToString(), "outcome=" + outcome);
                        if (!centralizeStatus)
                        {
                            throw new Exception(EnumMessage.NotConnectCentralized);
                        }
                    }

                    return 1;
                }
                else
                {
                    var queueRepo = _centralizedUnitOfWork.GetRepository<Trinity.DAL.DBContext.Queue>();
                    var dbQueue = queueRepo.Get(q => q.Queue_ID == queueId);
                    if (dbQueue != null)
                    {
                        dbQueue.Outcome = outcome;
                        dbQueue.LastUpdatedDate = DateTime.Now;
                        queueRepo.Update(dbQueue);
                    }

                    return _centralizedUnitOfWork.Save();
                }
            }
            catch (Exception e)
            {
                return 0;
            }
        }
    }
}
