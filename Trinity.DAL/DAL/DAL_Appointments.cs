using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Trinity.DAL.DBContext;
using Trinity.DAL.Repository;
using Trinity.Common;

namespace Trinity.DAL
{
    public class DAL_Appointments
    {
        Local_UnitOfWork _localUnitOfWork = new Local_UnitOfWork();
        Centralized_UnitOfWork _centralizedUnitOfWork = new Centralized_UnitOfWork();


        #region NEW DAL_APPOINTMENT
        public Appointment GetAppointmentByID(Guid ID)
        {

            try
            {
                if (EnumAppConfig.IsLocal)
                {
                    var data = _localUnitOfWork.DataContext.Appointments.FirstOrDefault(d => d.ID == ID);

                    if (data != null)
                    {
                        return data;
                    }
                    else
                    {
                        bool centralizeStatus;
                        var centralData = CallCentralized.Get<DBContext.Appointment>("Appointment", "GetById", out centralizeStatus, "appointmentId=" + ID.ToString());
                        if (centralizeStatus)
                        {
                            return centralData;
                        }
                        return null;
                    }
                }
                else
                {
                    var data = _centralizedUnitOfWork.DataContext.Appointments.FirstOrDefault(d => d.ID == ID);
                    if (data != null)
                    {
                        return data;
                    }
                    return null;
                }


            }
            catch (Exception)
            {

                return null;
            }

        }

        public BE.Appointment GetAppmtDetails(Guid ID)
        {


            if (EnumAppConfig.IsLocal)
            {
                Appointment appointment = _localUnitOfWork.DataContext.Appointments.Include("TimeSlot").Include("Membership_Users").FirstOrDefault(d => d.ID == ID);
                if (appointment != null)
                {
                    BE.Appointment result = SetAppointmentBE(appointment);
                    return result;
                }
                else
                {
                    bool centralizeStatus;
                    var centralData = CallCentralized.Get<Appointment>("Appointment", "GetDetailsById", out centralizeStatus, "appointmentId=" + ID.ToString());
                    if (centralizeStatus)
                    {
                        BE.Appointment result = SetAppointmentBE(centralData);
                        return result;
                    }
                    return null;
                }
            }
            else
            {
                Appointment appointment = _localUnitOfWork.DataContext.Appointments.Include("TimeSlot").Include("Membership_Users").FirstOrDefault(d => d.ID == ID);
                if (appointment != null)
                {
                    BE.Appointment result = SetAppointmentBE(appointment);
                    return result;
                }
                return null;
            }


        }

        private static BE.Appointment SetAppointmentBE(Appointment appointment)
        {
            return new BE.Appointment()
            {
                UserId = appointment.UserId,
                AppointmentDate = appointment.Date,
                ChangedCount = appointment.ChangedCount,
                Timeslot_ID = appointment.Timeslot_ID,
                Name = appointment.Membership_Users.Name,
                NRIC = appointment.Membership_Users.NRIC,
                Status = (EnumAppointmentStatuses)appointment.Status,
                ReportTime = appointment.ReportTime,
                StartTime = appointment.Timeslot.StartTime,
                EndTime = appointment.Timeslot.EndTime,

            };
        }

        public Appointment GetAppointmentByDate(string userId, DateTime date)
        {

            try
            {
                if (EnumAppConfig.IsLocal)
                {
                    var data = _localUnitOfWork.DataContext.Appointments.FirstOrDefault(d => d.UserId == userId && d.Date == date);
                    if (data != null)
                    {
                        return data;
                    }
                    else
                    {
                        bool centralizeStatus;
                        var centralData = CallCentralized.Get<Appointment>("Appointment", "GetByUserIdAndDate", out centralizeStatus, "UserId=" + userId, "date=" + date.ToString());
                        if (centralizeStatus)
                        {
                            return centralData;
                        }
                        return null;
                    }
                }
                else
                {
                    var data = _centralizedUnitOfWork.DataContext.Appointments.FirstOrDefault(d => d.UserId == userId && d.Date == date);
                    if (data != null)
                    {
                        return data;
                    }
                    return null;

                }
            }
            catch (Exception)
            {

                return null;
            }

        }

        public List<Appointment> GetAppointmentByUserId(string userId)
        {
            try
            {
                if (EnumAppConfig.IsLocal)
                {
                    var data = _localUnitOfWork.DataContext.Appointments.Where(d => d.UserId == userId && d.Date >= DateTime.Today).ToList();
                    if (data != null)
                    {
                        return data;
                    }
                    else
                    {
                        bool centralizeStatus;
                        var centralData = CallCentralized.Get<List<Appointment>>(EnumAPIParam.Appointment, "GetListByUserId", out centralizeStatus, "userId=" + userId);
                        if (centralizeStatus)
                        {
                            return centralData;
                        }
                        return null;
                    }

                }
                else
                {
                    var data = _centralizedUnitOfWork.DataContext.Appointments.Where(d => d.UserId == userId && d.Date >= DateTime.Today).ToList();
                    if (data != null)
                    {
                        return data;
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {

                return null;
            }
        }

        public Appointment GetTodayAppointmentByUserId(string UserId)
        {
            try
            {
                if (EnumAppConfig.IsLocal)
                {
                    var data = _localUnitOfWork.DataContext.Appointments.Where(d => d.UserId == UserId && d.Date == DateTime.Today).OrderBy(d => d.Date).FirstOrDefault();
                    if (data != null)
                    {
                        return data;
                    }
                    else
                    {
                        bool centralizeStatus;
                        var centralData = CallCentralized.Get<Appointment>(EnumAPIParam.Appointment, "GetByToday", out centralizeStatus, "userId=" + UserId);
                        if (centralizeStatus)
                        {
                            return centralData;
                        }
                        return null;
                    }
                }

                else
                {
                    var data = _centralizedUnitOfWork.DataContext.Appointments.Where(d => d.UserId == UserId && d.Date == DateTime.Today).OrderBy(d => d.Date).FirstOrDefault();
                    if (data != null)
                    {
                        return data;
                    }
                    return null;
                }

            }
            catch (Exception)
            {

                return null;
            }
        }

        public List<Appointment> GetAllCurrentTimeslotApptmt(TimeSpan currentTime)
        {
            try
            {

                if (EnumAppConfig.IsLocal)
                {
                    var data = _localUnitOfWork.DataContext.Appointments.Include("Queues").Include("Timeslot").Where(d => d.Date == DateTime.Today && !string.IsNullOrEmpty(d.Timeslot_ID) && d.Timeslot.StartTime.Value == currentTime && d.Queues.Any(q => q.Appointment_ID == d.ID) == false).OrderBy(d => d.Date).ToList();
                    if (data != null)
                    {
                        return data;
                    }
                    else
                    {
                        bool centralizeStatus;
                        var centralData = CallCentralized.Get<List<Appointment>>(EnumAPIParam.Appointment, EnumAPIParam.GetListCurrentTimeslot, out centralizeStatus, "currentTime=" + currentTime.ToString());
                        if (centralizeStatus)
                        {
                            return centralData;
                        }
                        return null;
                    }

                }
                else
                {
                    var data = _centralizedUnitOfWork.DataContext.Appointments.Include("Queues").Include("Timeslot").Where(d => d.Date == DateTime.Today && !string.IsNullOrEmpty(d.Timeslot_ID) && d.Timeslot.StartTime.Value == currentTime && d.Queues.Any(q => q.Appointment_ID == d.ID) == false).OrderBy(d => d.Date).ToList();
                    if (data != null)
                    {
                        return data;
                    }
                    return null;
                }
            }
            catch (Exception)
            {

                return null;
            }


        }

        public Appointment GetNearestApptmt(string UserId)
        {
            try
            {
                if (EnumAppConfig.IsLocal)
                {
                    var data = _localUnitOfWork.DataContext.Appointments.Where(d => d.UserId == UserId && d.Date > DateTime.Today).OrderBy(d => d.Date).FirstOrDefault();
                    if (data != null)
                    {
                        return data;
                    }
                    else
                    {
                        bool centralizeStatus;
                        var centralData = CallCentralized.Get<Appointment>(EnumAPIParam.Appointment, EnumAPIParam.GetNearest, out centralizeStatus, "userId=" + UserId);
                        if (centralizeStatus)
                        {
                            return centralData;
                        }
                        return null;
                    }
                }
                else
                {
                    var data = _centralizedUnitOfWork.DataContext.Appointments.Where(d => d.UserId == UserId && d.Date > DateTime.Today).OrderBy(d => d.Date).FirstOrDefault();
                    if (data != null)
                    {
                        return data;
                    }
                    return null;
                }

            }
            catch (Exception)
            {

                return null;
            }
        }

        public Appointment UpdateBookingTime(string IDAppointment, string timeStart, string timeEnd)
        {
            var result = GetMyAppointmentByID(new Guid(IDAppointment));
            var appointment = result.Data;
            var timeSlot = GetTimeslotByAppointment(appointment);
            if (timeSlot != null)
            {
                appointment.Timeslot_ID = timeSlot.Timeslot_ID;
                appointment.ChangedCount += 1;
            }
            else
            {
                appointment.ChangedCount = 1;
            }


            appointment.Status = (int)EnumAppointmentStatuses.Booked;
            try
            {
                if (EnumAppConfig.IsLocal)
                {
                    //update to central 
                    bool centralizeStatus;
                    var centralUpdate = CallCentralized.Post<Appointment>(EnumAPIParam.Appointment, EnumAPIParam.UpdateBooktime, out centralizeStatus, "appointmentId" + IDAppointment, "timeStart=" + timeStart, "timeEnd=" + timeEnd);
                    if (centralizeStatus)
                    {
                        _localUnitOfWork.GetRepository<Appointment>().Update(appointment);
                        _localUnitOfWork.Save();
                        return centralUpdate;

                    }
                    return null;


                }
                else
                {
                    _centralizedUnitOfWork.GetRepository<Appointment>().Update(appointment);
                    _centralizedUnitOfWork.Save();
                }
            }
            catch (Exception)
            {

                return null;
            }
            return null;
        }

        public int CountAbsenceReport(string UserID)
        {
            try
            {
                if (EnumAppConfig.IsLocal)
                {
                    var data = _localUnitOfWork.DataContext.Appointments.Where(d => d.UserId == UserID && d.Date < DateTime.Today && (d.Status == (int)EnumAppointmentStatuses.Pending ||
                d.Status == (int)EnumAppointmentStatuses.Booked) && d.AbsenceReporting_ID == null);
                    if (data != null)
                    {
                        return data.Count();
                    }
                    else
                    {
                        bool centralizeStatus;
                        var centralData = CallCentralized.Get<int>(EnumAPIParam.Appointment, EnumAPIParam.CountAbsenceByUserId, out centralizeStatus, "userId=" + UserID);
                        if (centralizeStatus)
                        {
                            return centralData;
                        }
                        return 0;
                    }
                }
                else
                {
                    var data = _centralizedUnitOfWork.DataContext.Appointments.Where(d => d.UserId == UserID && d.Date < DateTime.Today && (d.Status == (int)EnumAppointmentStatuses.Pending ||
               d.Status == (int)EnumAppointmentStatuses.Booked) && d.AbsenceReporting_ID == null);
                    if (data != null)
                    {
                        return data.Count();
                    }
                    return 0;
                }
            }
            catch (Exception)
            {

                return 0;
            }
        }

        public List<Appointment> GetAbsentAppointments(string UserID)
        {
            try
            {
                if (EnumAppConfig.IsLocal)
                {
                    var data = _localUnitOfWork.DataContext.Appointments.Where(d => d.UserId == UserID && System.Data.Entity.DbFunctions.AddDays(d.Date, 1) <= DateTime.Today && (d.Status == (int)EnumAppointmentStatuses.Pending ||
                 d.Status == (int)EnumAppointmentStatuses.Booked) && d.AbsenceReporting_ID == null).ToList();

                    if (data != null)
                    {
                        return data;
                    }
                    else
                    {
                        bool centralizeStatus;
                        var centralData = CallCentralized.Get<List<Appointment>>(EnumAPIParam.Appointment, EnumAPIParam.GetAbsenceByUserId, out centralizeStatus, "userId=" + UserID);
                        if (centralizeStatus)
                        {
                            return centralData;
                        }
                        return null;
                    }
                }
                else
                {
                    var data = _centralizedUnitOfWork.DataContext.Appointments.Where(d => d.UserId == UserID && System.Data.Entity.DbFunctions.AddDays(d.Date, 1) <= DateTime.Today && (d.Status == (int)EnumAppointmentStatuses.Pending ||
                 d.Status == (int)EnumAppointmentStatuses.Booked) && d.AbsenceReporting_ID == null).ToList();

                    if (data != null)
                    {
                        return data;
                    }
                    return null;
                }

            }
            catch (Exception)
            {

                return null;
            }
        }

        public Appointment UpdateAbsenceReason(Guid appointmentId, Guid absenceId)
        {

            try
            {
                var appointment = GetAppointmentByID(appointmentId);
                appointment.AbsenceReporting_ID = absenceId;
                appointment.Status = (int)EnumAppointmentStatuses.Reported;
                if (EnumAppConfig.IsLocal)
                {
                    bool centralizeStatus;
                    var centralUpdate = CallCentralized.Post<Appointment>(EnumAPIParam.Appointment, EnumAPIParam.UpdateReason, out centralizeStatus, "appointmentId" + appointmentId.ToString(), "absenceId=" + absenceId.ToString());
                    if (centralizeStatus)
                    {
                        _localUnitOfWork.GetRepository<Appointment>().Update(appointment);
                        _localUnitOfWork.Save();
                        return centralUpdate;

                    }
                    return null;

                }
                else
                {
                    _centralizedUnitOfWork.GetRepository<Appointment>().Update(appointment);
                    _centralizedUnitOfWork.Save();
                    return appointment;
                }
            }
            catch (Exception)
            {

                return null;
            }


        }

        public List<Appointment> GetListAppointmentFromListSelectedDate(string listID)
        {
            try
            {
                var listSelected = listID.Split(',').ToList();


                if (EnumAppConfig.IsLocal)
                {
                    var dbAppointment = new List<Appointment>();
                    foreach (var item in listSelected)
                    {
                        var find = _localUnitOfWork.DataContext.Appointments.Find(Guid.Parse(item));
                        if (find != null)
                        {
                            dbAppointment.Add(find);
                        }
                        else
                        {
                            bool centralizeStatus;
                            var centralData = CallCentralized.Get<List<Appointment>>(EnumAPIParam.Appointment, EnumAPIParam.GetListFromSelectedDate, out centralizeStatus, "listAppointmentId=" + listID);
                            if (centralizeStatus)
                            {
                                return centralData;
                            }
                        }
                    }
                }
                else
                {
                    var dbAppointment = new List<Appointment>();
                    foreach (var item in listSelected)
                    {
                        dbAppointment.Add(_centralizedUnitOfWork.DataContext.Appointments.Find(Guid.Parse(item)));

                    }
                    return dbAppointment;
                }

                return null;
            }
            catch (Exception)
            {

                return null;
            }
        }

        public int CountListApptmtByTimeslot(Appointment appointment)
        {
            var timeSlot = GetTimeslotByAppointment(appointment);
            if (timeSlot != null)
            {
                if (EnumAppConfig.IsLocal)
                {
                    var data = _localUnitOfWork.DataContext.Appointments.Where(a => a.Timeslot_ID == timeSlot.Timeslot_ID && a.Date == appointment.Date);
                    if (data != null)
                    {
                        return data.Count();
                    }
                    else
                    {
                        bool centralizeStatus;
                        var centralData = CallCentralized.Get<int>(EnumAPIParam.Appointment, "CountListApptmtByTimeslot", out centralizeStatus);
                        if (centralizeStatus)
                        {
                            return centralData;
                        }
                        return 0;
                    }
                }
                else
                {
                    var data = _centralizedUnitOfWork.DataContext.Appointments.Where(a => a.Timeslot_ID == timeSlot.Timeslot_ID && a.Date == appointment.Date);
                    if (data != null)
                    {
                        return data.Count();
                    }
                    return 0;
                }

            }
            return 0;

        }

        public List<BE.Appointment> GetAllApptmts()
        {
            try
            {
                if (EnumAppConfig.IsLocal)
                {
                    var data = _localUnitOfWork.DataContext.Appointments.Include("Timeslot").Include("Membership_Users").Where(d => !string.IsNullOrEmpty(d.Timeslot_ID)).OrderBy(d => d.Timeslot.StartTime).Select(d => new BE.Appointment()
                    {
                        NRIC = d.Membership_Users.NRIC,
                        Name = d.Membership_Users.Name,
                        ReportTime = d.ReportTime,
                        Status = (EnumAppointmentStatuses)d.Status,
                        AppointmentDate = d.Date,
                        StartTime = d.Timeslot.StartTime,
                        EndTime = d.Timeslot.EndTime
                    }).ToList();
                    if (data != null)
                    {
                        return data;
                    }
                    else
                    {
                        bool centralizeStatus;
                        var centralData = CallCentralized.Get<List<BE.Appointment>>(EnumAPIParam.Appointment, EnumAPIParam.GetAll, out centralizeStatus);
                        if (centralizeStatus)
                        {
                            return centralData;
                        }
                        return null;
                    }

                }
                else
                {

                    var data = _centralizedUnitOfWork.DataContext.Appointments.Include("Timeslot").Include("Membership_Users").Where(d => !string.IsNullOrEmpty(d.Timeslot_ID)).OrderBy(d => d.Timeslot.StartTime).Select(d => new BE.Appointment()
                    {
                        NRIC = d.Membership_Users.NRIC,
                        Name = d.Membership_Users.Name,
                        ReportTime = d.ReportTime,
                        Status = (EnumAppointmentStatuses)d.Status,
                        AppointmentDate = d.Date,
                        StartTime = d.Timeslot.StartTime,
                        EndTime = d.Timeslot.EndTime
                    }).ToList();
                    if (data != null)
                    {
                        return data;
                    }
                    return null;
                }



                //var lstModels = from a in _localUnitOfWork.DataContext.Appointments
                //                join tl in _localUnitOfWork.DataContext.Timeslots on a.Timeslot_ID equals tl.Timeslot_ID
                //                join u in _localUnitOfWork.DataContext.Membership_Users on a.UserId equals u.UserId
                //                orderby tl.StartTime
                //                select new BE.Appointment()
                //                {
                //                    NRIC = u.NRIC,
                //                    Name = u.Name,
                //                    ReportTime = a.ReportTime,
                //                    Status = (EnumAppointmentStatuses)a.Status,
                //                    AppointmentDate = a.Date,
                //                    StartTime = tl.StartTime,
                //                    EndTime = tl.EndTime
                //                };

                // return lstModels.ToList();
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public List<BE.Statistics> GetAllStats()
        {
            //var model = _localUnitOfWork.DataContext.Timeslots.Include("Appointments")
            try
            {
                if (EnumAppConfig.IsLocal)
                {
                    var data = _localUnitOfWork.DataContext.Appointments.Include("Timeslot").Where(d => !string.IsNullOrEmpty(d.Timeslot_ID)).Select(d => new
                    {
                        Timeslot_ID = d.Timeslot_ID,
                        StartTime = d.Timeslot.StartTime,
                        EndTime = d.Timeslot.EndTime,
                        Date = d.Date
                    }).Distinct().Select(d => new BE.Statistics()
                    {
                        Timeslot_ID = d.Timeslot_ID,
                        StartTime = d.StartTime,
                        EndTime = d.EndTime,
                        Date = d.Date
                    }).ToList();

                    if (data != null)
                    {
                        return data;
                    }
                    else
                    {
                        bool centralizeStatus;
                        var centralData = CallCentralized.Get<List<BE.Statistics>>(EnumAPIParam.Appointment, EnumAPIParam.GetAllStatistics, out centralizeStatus);
                        if (centralizeStatus)
                        {
                            return centralData;
                        }
                        return null;
                    }

                }
                else
                {
                    var data = _centralizedUnitOfWork.DataContext.Appointments.Include("Timeslot").Where(d => !string.IsNullOrEmpty(d.Timeslot_ID)).Select(d => new
                    {
                        Timeslot_ID = d.Timeslot_ID,
                        StartTime = d.Timeslot.StartTime,
                        EndTime = d.Timeslot.EndTime,
                        Date = d.Date
                    }).Distinct().Select(d => new BE.Statistics()
                    {
                        Timeslot_ID = d.Timeslot_ID,
                        StartTime = d.StartTime,
                        EndTime = d.EndTime,
                        Date = d.Date
                    }).ToList();

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

            //var model = from tl in _localUnitOfWork.DataContext.Timeslots
            //            join a in _localUnitOfWork.DataContext.Appointments on tl.Timeslot_ID equals a.Timeslot_ID
            //            select new BE.Statistics()
            //            {
            //                Timeslot_ID = tl.Timeslot_ID,
            //                StartTime = tl.StartTime,
            //                EndTime = tl.EndTime,
            //                Date = a.Date//,
            //                //Max = GetMaximumNumberOfTimeslot(tl.Timeslot_ID),
            //                //Booked = CountAppointmentBookedByTimeslot(tl.Timeslot_ID),
            //                //Reported = CountAppointmentReportedByTimeslot(tl.Timeslot_ID),
            //                //No_Show = CountAppointmentNoShowByTimeslot(tl.Timeslot_ID),
            //                //Available = GetMaximumNumberOfTimeslot(tl.Timeslot_ID) - CountAppointmentBookedByTimeslot(tl.Timeslot_ID) - CountAppointmentReportedByTimeslot(tl.Timeslot_ID) - CountAppointmentNoShowByTimeslot(tl.Timeslot_ID)
            //            };
            //return model.ToList();
        }

        public int CountApptmtBookedByTimeslot(string timeslotID)
        {
            try
            {
                if (EnumAppConfig.IsLocal)
                {
                    var data = _localUnitOfWork.DataContext.Appointments.Where(a => a.Timeslot_ID == timeslotID && a.Status == (int)EnumAppointmentStatuses.Booked);
                    if (data != null)
                    {
                        return data.Count();
                    }
                    else
                    {
                        bool centralizeStatus;
                        var centralData = CallCentralized.Get<int>(EnumAPIParam.Appointment, EnumAPIParam.CountBookedByTimeslot, out centralizeStatus);
                        if (centralizeStatus)
                        {
                            return centralData;
                        }
                        return 0;
                    }
                }
                else
                {
                    var data = _centralizedUnitOfWork.DataContext.Appointments.Where(a => a.Timeslot_ID == timeslotID && a.Status == (int)EnumAppointmentStatuses.Booked);
                    if (data != null)
                    {
                        return data.Count();
                    }
                }

            }
            catch (Exception)
            {

                return 0;
            }
            return 0;
        }

        public int CountApptmtReportedByTimeslot(string timeslotID)
        {
            try
            {
                if (EnumAppConfig.IsLocal)
                {
                    var data = _localUnitOfWork.DataContext.Appointments.Where(a => a.Timeslot_ID == timeslotID && a.Status == (int)EnumAppointmentStatuses.Reported);
                    if (data != null)
                    {
                        return data.Count();
                    }
                    else
                    {
                        bool centralizeStatus;
                        var centralData = CallCentralized.Get<int>(EnumAPIParam.Appointment, EnumAPIParam.CountReportedByTimeslot, out centralizeStatus);
                        if (centralizeStatus)
                        {
                            return centralData;
                        }
                    }
                }
                else
                {
                    var data = _centralizedUnitOfWork.DataContext.Appointments.Where(a => a.Timeslot_ID == timeslotID && a.Status == (int)EnumAppointmentStatuses.Reported);
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

        public int CountApptmtNoShowByTimeslot(string timeslotID)
        {
            try
            {
                if (EnumAppConfig.IsLocal)
                {
                    var data = _localUnitOfWork.DataContext.Appointments.Where(a => a.Timeslot_ID == timeslotID && a.Status != (int)EnumAppointmentStatuses.Booked && a.Status != (int)EnumAppointmentStatuses.Reported);
                    if (data != null)
                    {
                        return data.Count();
                    }
                    else
                    {
                        bool centralizeStatus;
                        var centralData = CallCentralized.Get<int>(EnumAPIParam.Appointment, EnumAPIParam.CountNoShowdByTimeslot, out centralizeStatus);
                        if (centralizeStatus)
                        {
                            return centralData;
                        }
                    }
                }
                else
                {
                    var data = _centralizedUnitOfWork.DataContext.Appointments.Where(a => a.Timeslot_ID == timeslotID && a.Status != (int)EnumAppointmentStatuses.Booked && a.Status != (int)EnumAppointmentStatuses.Reported);
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

        public int GetMaxNumberOfTimeslot(string timeslotID)
        {
            try
            {
                Timeslot timeslot;
                if (EnumAppConfig.IsLocal)
                {
                    timeslot = _localUnitOfWork.DataContext.Timeslots.FirstOrDefault(t => t.Timeslot_ID == timeslotID);
                    if (timeslot != null && timeslot.MaximumSupervisee.HasValue)
                    {
                        return timeslot.MaximumSupervisee.Value;
                    }
                    else
                    {
                        bool centralizeStatus;
                        var centralData = CallCentralized.Get<int>(EnumAPIParam.Appointment, EnumAPIParam.GetMaximumNumberOfTimeslot, out centralizeStatus);
                        if (centralizeStatus)
                        {
                            return centralData;
                        }
                    }
                }
                else
                {
                    timeslot = _centralizedUnitOfWork.DataContext.Timeslots.FirstOrDefault(t => t.Timeslot_ID == timeslotID);
                }
                return 0;

            }
            catch (Exception e)
            {
                return 0;
            }
        }

        public bool CreateApptmtsForAllUsers(DateTime date)
        {
            try
            {
                if (EnumAppConfig.IsLocal)
                {
                    bool centralizeStatus;
                    var centralData = CallCentralized.Post<bool>(EnumAPIParam.Appointment, EnumAPIParam.CreateForAllUsers, out centralizeStatus);
                    if (centralizeStatus)
                    {
                        _localUnitOfWork.GetRepository<Appointment>().Delete(t => t.Date.Year == date.Year && t.Date.Month == date.Month && t.Date.Day == date.Day);
                        _localUnitOfWork.Save();

                        _localUnitOfWork.GetRepository<Appointment>().AddRange(_localUnitOfWork.DataContext.Membership_Users.Select(d => d.UserId).Select(d => new Appointment()
                        {
                            ID = Guid.NewGuid(),
                            UserId = d,
                            ChangedCount = 0,
                            Date = date,
                            Status = (int)EnumAppointmentStatuses.Pending
                        }).ToList());
                        _localUnitOfWork.Save();

                        return centralData;
                    }
                }
                else
                {
                    _centralizedUnitOfWork.GetRepository<Appointment>().Delete(t => t.Date.Year == date.Year && t.Date.Month == date.Month && t.Date.Day == date.Day);
                    _centralizedUnitOfWork.Save();

                    _centralizedUnitOfWork.GetRepository<Appointment>().AddRange(_localUnitOfWork.DataContext.Membership_Users.Select(d => d.UserId).Select(d => new Appointment()
                    {
                        ID = Guid.NewGuid(),
                        UserId = d,
                        ChangedCount = 0,
                        Date = date,
                        Status = (int)EnumAppointmentStatuses.Pending
                    }).ToList());
                    _centralizedUnitOfWork.Save();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public Timeslot GetTimeslotNearestAppointment()
        {
            try
            {
                DateTime currentDate = DateTime.Now;
                if (EnumAppConfig.IsLocal)
                {
                    var data = _localUnitOfWork.DataContext.Timeslots.Where(t => DbFunctions.TruncateTime(t.Date) >= currentDate.Date && t.StartTime.Value >= currentDate.TimeOfDay).OrderBy(t => t.Date).ThenBy(t => t.StartTime).FirstOrDefault();
                    if (data != null)
                    {
                        return data;
                    }
                    else
                    {
                        bool centralizeStatus;
                        var centralData = CallCentralized.Get<Timeslot>(EnumAPIParam.Appointment, EnumAPIParam.GetNearestTimeslot, out centralizeStatus);
                        if (centralizeStatus)
                        {
                            return centralData;
                        }
                    }

                }
                else
                {
                    var data = _centralizedUnitOfWork.DataContext.Timeslots.Where(t => DbFunctions.TruncateTime(t.Date) >= currentDate.Date && t.StartTime.Value >= currentDate.TimeOfDay).OrderBy(t => t.Date).ThenBy(t => t.StartTime).FirstOrDefault();
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

        public Appointment UpdateTimeslotForApptmt(Guid appointmentId, string timeslotID)
        {
            try
            {
                var appointment = GetAppointmentByID(appointmentId);
                
                appointment.Timeslot_ID = timeslotID;
                if (EnumAppConfig.IsLocal)
                {
                    bool centralizeStatus;
                    var centralData = CallCentralized.Post<Appointment>(EnumAPIParam.Appointment, EnumAPIParam.UpdateTimeslot, out centralizeStatus);
                    if (centralizeStatus)
                    {
                        _localUnitOfWork.GetRepository<Appointment>().Update(appointment);
                        _localUnitOfWork.Save();

                        return centralData;
                    }

                 
                }
                else
                {
                    _centralizedUnitOfWork.GetRepository<Appointment>().Update(appointment);
                    _centralizedUnitOfWork.Save();
                    return appointment;
                }
                return null;
            }
            catch (Exception)
            {

                return null;
            }
        }
        #endregion

        #region obsolete
        public Response<Appointment> GetMyAppointmentByID(Guid ID)
        {
            try
            {
                //if (EnumAppConfig.IsLocal)
                //{
                //    var data = _localUnitOfWork.DataContext.Appointments.FirstOrDefault(d => d.ID == ID);
                //    return new Response<Appointment>((int)EnumResponseStatuses.Success, EnumResponseMessage.Success, data);

                //}
                //else
                //{
                var data = _centralizedUnitOfWork.DataContext.Appointments.FirstOrDefault(d => d.ID == ID);
                return new Response<Appointment>((int)EnumResponseStatuses.Success, EnumResponseMessage.Success, data);
                //}

            }
            catch (Exception)
            {

                return new Response<Appointment>((int)EnumResponseStatuses.ErrorSystem, EnumResponseMessage.ErrorSystem, null);
            }

        }

        public Response<BE.Appointment> GetAppointmentDetails(Guid ID)
        {
            Appointment appointment;
            //if (EnumAppConfig.IsLocal)
            //{
            //    appointment = _localUnitOfWork.DataContext.Appointments.Include("TimeSlot").Include("Membership_Users").FirstOrDefault(d => d.ID == ID);
            //}
            //else
            //{
            appointment = _centralizedUnitOfWork.DataContext.Appointments.Include("TimeSlot").Include("Membership_Users").FirstOrDefault(d => d.ID == ID);
            //}

            if (appointment != null)
            {
                Trinity.BE.Appointment result = new BE.Appointment()
                {
                    UserId = appointment.UserId,
                    AppointmentDate = appointment.Date,
                    ChangedCount = appointment.ChangedCount,
                    Timeslot_ID = appointment.Timeslot_ID,
                    Name = appointment.Membership_Users.Name,
                    NRIC = appointment.Membership_Users.NRIC,
                    Status = (EnumAppointmentStatuses)appointment.Status,
                    ReportTime = appointment.ReportTime,
                    StartTime = appointment.Timeslot.StartTime,
                    EndTime = appointment.Timeslot.EndTime,

                };
                return new Response<BE.Appointment>((int)EnumResponseStatuses.Success, EnumResponseMessage.Success, result);
            }
            return new Response<BE.Appointment>((int)EnumResponseStatuses.ErrorSystem, EnumResponseMessage.ErrorSystem, null);
        }

        public Response<Appointment> GetMyAppointmentByDate(string UserId, DateTime date)
        {
            try
            {
                //if (EnumAppConfig.IsLocal)
                //{

                //    return new Response<Appointment>((int)EnumResponseStatuses.Success, EnumResponseMessage.Success, _localUnitOfWork.DataContext.Appointments.FirstOrDefault(d => d.UserId == UserId && d.Date == date));
                //}
                //else
                {


                    return new Response<Appointment>((int)EnumResponseStatuses.Success, EnumResponseMessage.Success, _centralizedUnitOfWork.DataContext.Appointments.FirstOrDefault(d => d.UserId == UserId && d.Date == date));
                }
            }
            catch (Exception)
            {

                return new Response<Appointment>((int)EnumResponseStatuses.ErrorSystem, EnumResponseMessage.ErrorSystem, null);
            }

        }

        public Response<List<Appointment>> GetMyAppointmentBy(string UserId)
        {
            try
            {
                //if (EnumAppConfig.IsLocal)
                //{
                //    return new Response<List<Appointment>>((int)EnumResponseStatuses.Success, EnumResponseMessage.Success, _localUnitOfWork.DataContext.Appointments.Where(d => d.UserId == UserId && d.Date >= DateTime.Today).ToList());
                //}

                return new Response<List<Appointment>>((int)EnumResponseStatuses.Success, EnumResponseMessage.Success, _centralizedUnitOfWork.DataContext.Appointments.Where(d => d.UserId == UserId && d.Date >= DateTime.Today).ToList());

            }
            catch (Exception)
            {

                return new Response<List<Appointment>>((int)EnumResponseStatuses.ErrorSystem, EnumResponseMessage.ErrorSystem, null);
            }
        }

        public Response<Appointment> GetTodayAppointment(string UserId)
        {
            try
            {
                //if (EnumAppConfig.IsLocal)
                //{
                //    return new Response<Appointment>((int)EnumResponseStatuses.Success, EnumResponseMessage.Success, _localUnitOfWork.DataContext.Appointments.Where(d => d.UserId == UserId && d.Date == DateTime.Today).OrderBy(d => d.Date).FirstOrDefault());
                //}

                return new Response<Appointment>((int)EnumResponseStatuses.Success, EnumResponseMessage.Success, _centralizedUnitOfWork.DataContext.Appointments.Where(d => d.UserId == UserId && d.Date == DateTime.Today).OrderBy(d => d.Date).FirstOrDefault());
            }
            catch (Exception)
            {

                return new Response<Appointment>((int)EnumResponseStatuses.ErrorSystem, EnumResponseMessage.ErrorSystem, null);
            }
        }

        public Response<List<Appointment>> GetAllCurrentTimeslotAppointment(TimeSpan currentTime)
        {
            try
            {
                // get local
                // if null, get from centralized and 
                // 

                //if (EnumAppConfig.IsLocal)
                //{
                //    return new Response<List<Appointment>>((int)EnumResponseStatuses.Success, EnumResponseMessage.Success, _localUnitOfWork.DataContext.Appointments.Include("Queues").Include("Timeslot").Where(d => d.Date == DateTime.Today && !string.IsNullOrEmpty(d.Timeslot_ID) && d.Timeslot.StartTime.Value == currentTime && d.Queues.Any(q => q.Appointment_ID == d.ID) == false).OrderBy(d => d.Date).ToList());
                //}
                return new Response<List<Appointment>>((int)EnumResponseStatuses.Success, EnumResponseMessage.Success, _centralizedUnitOfWork.DataContext.Appointments.Include("Queues").Include("Timeslot").Where(d => d.Date == DateTime.Today && !string.IsNullOrEmpty(d.Timeslot_ID) && d.Timeslot.StartTime.Value == currentTime && d.Queues.Any(q => q.Appointment_ID == d.ID) == false).OrderBy(d => d.Date).ToList());
            }
            catch (Exception)
            {

                return new Response<List<Appointment>>((int)EnumResponseStatuses.ErrorSystem, EnumResponseMessage.ErrorSystem, null);
            }


        }

        public Response<Appointment> GetNearestAppointment(string UserId)
        {
            try
            {
                //if (EnumAppConfig.IsLocal)
                //{
                //    return new Response<Appointment>((int)EnumResponseStatuses.Success, EnumResponseMessage.Success, _localUnitOfWork.DataContext.Appointments.Where(d => d.UserId == UserId && d.Date > DateTime.Today).OrderBy(d => d.Date).FirstOrDefault());
                //}
                return new Response<Appointment>((int)EnumResponseStatuses.Success, EnumResponseMessage.Success, _centralizedUnitOfWork.DataContext.Appointments.Where(d => d.UserId == UserId && d.Date > DateTime.Today).OrderBy(d => d.Date).FirstOrDefault());
            }
            catch (Exception)
            {

                return new Response<Appointment>((int)EnumResponseStatuses.ErrorSystem, EnumResponseMessage.ErrorSystem, null);
            }
        }

        public Response<Appointment> UpdateBookTime(string IDAppointment, string timeStart, string timeEnd)
        {
            var result = GetMyAppointmentByID(new Guid(IDAppointment));
            var appointment = result.Data;
            var timeSlot = GetTimeslotByAppointment(appointment);
            if (timeSlot != null)
            {
                appointment.Timeslot_ID = timeSlot.Timeslot_ID;
                appointment.ChangedCount += 1;
            }
            else
            {
                appointment.ChangedCount = 1;
            }


            appointment.Status = (int)EnumAppointmentStatuses.Booked;
            try
            {
                if (EnumAppConfig.IsLocal)
                {

                    _localUnitOfWork.GetRepository<Appointment>().Update(appointment);
                    _localUnitOfWork.Save();
                }
                else
                {
                    _centralizedUnitOfWork.GetRepository<Appointment>().Update(appointment);
                    _centralizedUnitOfWork.Save();
                }
            }
            catch (Exception)
            {

                return new Response<Appointment>((int)EnumResponseStatuses.ErrorSystem, EnumResponseMessage.ErrorSystem, null);
            }


            return new Response<Appointment>((int)EnumResponseStatuses.Success, EnumResponseMessage.Success, appointment);
        }

        public Response<int> CountMyAbsence(string UserID)
        {
            try
            {
                // if (EnumAppConfig.IsLocal)
                // {
                //     return new Response<int>((int)EnumResponseStatuses.Success, EnumResponseMessage.Success, _localUnitOfWork.DataContext.Appointments.Count(d => d.UserId == UserID && d.Date < DateTime.Today && (d.Status == (int)EnumAppointmentStatuses.Pending ||
                //d.Status == (int)EnumAppointmentStatuses.Booked) && d.AbsenceReporting_ID == null));
                // }
                return new Response<int>((int)EnumResponseStatuses.Success, EnumResponseMessage.Success, _centralizedUnitOfWork.DataContext.Appointments.Count(d => d.UserId == UserID && d.Date < DateTime.Today && (d.Status == (int)EnumAppointmentStatuses.Pending ||
              d.Status == (int)EnumAppointmentStatuses.Booked) && d.AbsenceReporting_ID == null));
            }
            catch (Exception)
            {

                return new Response<int>((int)EnumResponseStatuses.ErrorSystem, EnumResponseMessage.ErrorSystem, 0);
            }
        }

        public Response<List<Appointment>> GetMyAbsentAppointments(string UserID)
        {
            try
            {
                //if (EnumAppConfig.IsLocal)
                //{
                //    return new Response<List<Appointment>>((int)EnumResponseStatuses.Success, EnumResponseMessage.Success, _localUnitOfWork.DataContext.Appointments.Where(d => d.UserId == UserID && System.Data.Entity.DbFunctions.AddDays(d.Date, 1) <= DateTime.Today && (d.Status == (int)EnumAppointmentStatuses.Pending ||
                //d.Status == (int)EnumAppointmentStatuses.Booked) && d.AbsenceReporting_ID == null).ToList());
                //}
                return new Response<List<Appointment>>((int)EnumResponseStatuses.Success, EnumResponseMessage.Success, _centralizedUnitOfWork.DataContext.Appointments.Where(d => d.UserId == UserID && System.Data.Entity.DbFunctions.AddDays(d.Date, 1) <= DateTime.Today && (d.Status == (int)EnumAppointmentStatuses.Pending ||
                d.Status == (int)EnumAppointmentStatuses.Booked) && d.AbsenceReporting_ID == null).ToList());
            }
            catch (Exception)
            {

                return new Response<List<Appointment>>((int)EnumResponseStatuses.ErrorSystem, EnumResponseMessage.ErrorSystem, null);
            }
        }

        public Response<Appointment> UpdateReason(Guid appointmentId, Guid absenceId)
        {
            var result = GetMyAppointmentByID(appointmentId);
            var appointment = result.Data;
            appointment.AbsenceReporting_ID = absenceId;
            appointment.Status = (int)EnumAppointmentStatuses.Reported;
            try
            {
                //if (EnumAppConfig.IsLocal)
                //{
                //    _localUnitOfWork.GetRepository<Appointment>().Update(appointment);
                //    _localUnitOfWork.Save();
                //}
                //else
                {
                    _centralizedUnitOfWork.GetRepository<Appointment>().Update(appointment);
                    _centralizedUnitOfWork.Save();
                }
            }
            catch (Exception)
            {

                return new Response<Appointment>((int)EnumResponseStatuses.ErrorSystem, EnumResponseMessage.ErrorSystem, null);
            }

            return new Response<Appointment>((int)EnumResponseStatuses.Success, EnumResponseMessage.Success, appointment);
        }

        public Response<List<Appointment>> GetListAppointmentFromSelectedDate(List<string> listID)
        {
            try
            {
                var dbAppointment = new List<Appointment>();

                //if (EnumAppConfig.IsLocal)
                //{
                //    foreach (var item in listID)
                //    {
                //        dbAppointment.Add(_localUnitOfWork.DataContext.Appointments.Find(Guid.Parse(item)));

                //    }
                //}
                //else
                {
                    foreach (var item in listID)
                    {
                        dbAppointment.Add(_centralizedUnitOfWork.DataContext.Appointments.Find(Guid.Parse(item)));

                    }
                }

                return new Response<List<Appointment>>((int)EnumResponseStatuses.Success, EnumResponseMessage.Success, dbAppointment);
            }
            catch (Exception)
            {

                return new Response<List<Appointment>>((int)EnumResponseStatuses.ErrorSystem, EnumResponseMessage.ErrorSystem, null);
            }
        }

        public int CountListAppointmentByTimeslot(Appointment appointment)
        {
            var timeSlot = GetTimeslotByAppointment(appointment);
            if (timeSlot != null)
            {
                //if (EnumAppConfig.IsLocal)
                //{
                //    return _localUnitOfWork.DataContext.Appointments.Count(a => a.Timeslot_ID == timeSlot.Timeslot_ID && a.Date == appointment.Date);
                //}
                return _centralizedUnitOfWork.DataContext.Appointments.Count(a => a.Timeslot_ID == timeSlot.Timeslot_ID && a.Date == appointment.Date);

            }
            return 0;

        }


        public Response<List<BE.Appointment>> GetAllAppointments()
        {
            try
            {
                //if (EnumAppConfig.IsLocal)
                //{
                //    return new Response<List<BE.Appointment>>((int)EnumResponseStatuses.Success, EnumResponseMessage.Success, _localUnitOfWork.DataContext.Appointments.Include("Timeslot").Include("Membership_Users").Where(d => !string.IsNullOrEmpty(d.Timeslot_ID)).OrderBy(d => d.Timeslot.StartTime).Select(d => new BE.Appointment()
                //    {
                //        NRIC = d.Membership_Users.NRIC,
                //        Name = d.Membership_Users.Name,
                //        ReportTime = d.ReportTime,
                //        Status = (EnumAppointmentStatuses)d.Status,
                //        AppointmentDate = d.Date,
                //        StartTime = d.Timeslot.StartTime,
                //        EndTime = d.Timeslot.EndTime
                //    }).ToList());
                //}
                return new Response<List<BE.Appointment>>((int)EnumResponseStatuses.Success, EnumResponseMessage.Success, _centralizedUnitOfWork.DataContext.Appointments.Include("Timeslot").Include("Membership_Users").Where(d => !string.IsNullOrEmpty(d.Timeslot_ID)).OrderBy(d => d.Timeslot.StartTime).Select(d => new BE.Appointment()
                {
                    NRIC = d.Membership_Users.NRIC,
                    Name = d.Membership_Users.Name,
                    ReportTime = d.ReportTime,
                    Status = (EnumAppointmentStatuses)d.Status,
                    AppointmentDate = d.Date,
                    StartTime = d.Timeslot.StartTime,
                    EndTime = d.Timeslot.EndTime
                }).ToList());


                //var lstModels = from a in _localUnitOfWork.DataContext.Appointments
                //                join tl in _localUnitOfWork.DataContext.Timeslots on a.Timeslot_ID equals tl.Timeslot_ID
                //                join u in _localUnitOfWork.DataContext.Membership_Users on a.UserId equals u.UserId
                //                orderby tl.StartTime
                //                select new BE.Appointment()
                //                {
                //                    NRIC = u.NRIC,
                //                    Name = u.Name,
                //                    ReportTime = a.ReportTime,
                //                    Status = (EnumAppointmentStatuses)a.Status,
                //                    AppointmentDate = a.Date,
                //                    StartTime = tl.StartTime,
                //                    EndTime = tl.EndTime
                //                };

                // return lstModels.ToList();
            }
            catch (Exception e)
            {
                return new Response<List<BE.Appointment>>((int)EnumResponseStatuses.ErrorSystem, EnumResponseMessage.ErrorSystem, null);
            }
        }

        public Response<List<BE.Statistics>> GetAllStatistics()
        {
            //var model = _localUnitOfWork.DataContext.Timeslots.Include("Appointments")
            try
            {
                //if (EnumAppConfig.IsLocal)
                //{
                //    return new Response<List<BE.Statistics>>((int)EnumResponseStatuses.Success, EnumResponseMessage.Success, _localUnitOfWork.DataContext.Appointments.Include("Timeslot").Where(d => !string.IsNullOrEmpty(d.Timeslot_ID)).Select(d => new
                //    {
                //        Timeslot_ID = d.Timeslot_ID,
                //        StartTime = d.Timeslot.StartTime,
                //        EndTime = d.Timeslot.EndTime,
                //        Date = d.Date
                //    }).Distinct().Select(d => new BE.Statistics()
                //    {
                //        Timeslot_ID = d.Timeslot_ID,
                //        StartTime = d.StartTime,
                //        EndTime = d.EndTime,
                //        Date = d.Date
                //    }).ToList());
                //}
                return new Response<List<BE.Statistics>>((int)EnumResponseStatuses.Success, EnumResponseMessage.Success, _centralizedUnitOfWork.DataContext.Appointments.Include("Timeslot").Where(d => !string.IsNullOrEmpty(d.Timeslot_ID)).Select(d => new
                {
                    Timeslot_ID = d.Timeslot_ID,
                    StartTime = d.Timeslot.StartTime,
                    EndTime = d.Timeslot.EndTime,
                    Date = d.Date
                }).Distinct().Select(d => new BE.Statistics()
                {
                    Timeslot_ID = d.Timeslot_ID,
                    StartTime = d.StartTime,
                    EndTime = d.EndTime,
                    Date = d.Date
                }).ToList());

            }
            catch (Exception)
            {

                return new Response<List<BE.Statistics>>((int)EnumResponseStatuses.ErrorSystem, EnumResponseMessage.ErrorSystem, null);
            }

            //var model = from tl in _localUnitOfWork.DataContext.Timeslots
            //            join a in _localUnitOfWork.DataContext.Appointments on tl.Timeslot_ID equals a.Timeslot_ID
            //            select new BE.Statistics()
            //            {
            //                Timeslot_ID = tl.Timeslot_ID,
            //                StartTime = tl.StartTime,
            //                EndTime = tl.EndTime,
            //                Date = a.Date//,
            //                //Max = GetMaximumNumberOfTimeslot(tl.Timeslot_ID),
            //                //Booked = CountAppointmentBookedByTimeslot(tl.Timeslot_ID),
            //                //Reported = CountAppointmentReportedByTimeslot(tl.Timeslot_ID),
            //                //No_Show = CountAppointmentNoShowByTimeslot(tl.Timeslot_ID),
            //                //Available = GetMaximumNumberOfTimeslot(tl.Timeslot_ID) - CountAppointmentBookedByTimeslot(tl.Timeslot_ID) - CountAppointmentReportedByTimeslot(tl.Timeslot_ID) - CountAppointmentNoShowByTimeslot(tl.Timeslot_ID)
            //            };
            //return model.ToList();
        }

        public Response<int> CountAppointmentBookedByTimeslot(string timeslotID)
        {
            try
            {
                //if (EnumAppConfig.IsLocal)
                //{
                //    return new Response<int>((int)EnumResponseStatuses.Success, EnumResponseMessage.Success, _localUnitOfWork.DataContext.Appointments.Count(a => a.Timeslot_ID == timeslotID && a.Status == (int)EnumAppointmentStatuses.Booked));
                //}
                return new Response<int>((int)EnumResponseStatuses.Success, EnumResponseMessage.Success, _centralizedUnitOfWork.DataContext.Appointments.Count(a => a.Timeslot_ID == timeslotID && a.Status == (int)EnumAppointmentStatuses.Booked));
            }
            catch (Exception)
            {

                return new Response<int>((int)EnumResponseStatuses.ErrorSystem, EnumResponseMessage.ErrorSystem, 0);
            }
        }

        public Response<int> CountAppointmentReportedByTimeslot(string timeslotID)
        {
            try
            {
                //if (EnumAppConfig.IsLocal)
                //{
                //    return new Response<int>((int)EnumResponseStatuses.Success, EnumResponseMessage.Success, _localUnitOfWork.DataContext.Appointments.Count(a => a.Timeslot_ID == timeslotID && a.Status == (int)EnumAppointmentStatuses.Reported));
                //}
                return new Response<int>((int)EnumResponseStatuses.Success, EnumResponseMessage.Success, _centralizedUnitOfWork.DataContext.Appointments.Count(a => a.Timeslot_ID == timeslotID && a.Status == (int)EnumAppointmentStatuses.Reported));
            }
            catch (Exception)
            {

                return new Response<int>((int)EnumResponseStatuses.ErrorSystem, EnumResponseMessage.ErrorSystem, 0);
            }
        }

        public Response<int> CountAppointmentNoShowByTimeslot(string timeslotID)
        {
            try
            {
                //if (EnumAppConfig.IsLocal)
                //{
                //    return new Response<int>((int)EnumResponseStatuses.Success, EnumResponseMessage.Success, _localUnitOfWork.DataContext.Appointments.Count(a => a.Timeslot_ID == timeslotID && a.Status != (int)EnumAppointmentStatuses.Booked && a.Status != (int)EnumAppointmentStatuses.Reported));
                //}
                return new Response<int>((int)EnumResponseStatuses.Success, EnumResponseMessage.Success, _centralizedUnitOfWork.DataContext.Appointments.Count(a => a.Timeslot_ID == timeslotID && a.Status != (int)EnumAppointmentStatuses.Booked && a.Status != (int)EnumAppointmentStatuses.Reported));
            }
            catch (Exception)
            {

                return new Response<int>((int)EnumResponseStatuses.ErrorSystem, EnumResponseMessage.ErrorSystem, 0);
            }
        }

        public Response<int> GetMaximumNumberOfTimeslot(string timeslotID)
        {
            try
            {
                Timeslot timeslot;
                //if (EnumAppConfig.IsLocal)
                //{
                //    timeslot = _localUnitOfWork.DataContext.Timeslots.FirstOrDefault(t => t.Timeslot_ID == timeslotID);
                //}
                //else
                {
                    timeslot = _centralizedUnitOfWork.DataContext.Timeslots.FirstOrDefault(t => t.Timeslot_ID == timeslotID);
                }
                if (timeslot != null)
                {
                    return new Response<int>((int)EnumResponseStatuses.Success, EnumResponseMessage.Success, timeslot.MaximumSupervisee.HasValue ? timeslot.MaximumSupervisee.Value : 0);
                }
                return new Response<int>((int)EnumResponseStatuses.Success, EnumResponseMessage.Success, 0);

            }
            catch (Exception e)
            {
                return new Response<int>((int)EnumResponseStatuses.ErrorSystem, EnumResponseMessage.ErrorSystem, 0);
            }
        }

        public Response<bool> CreateAppointmentsForAllUsers(DateTime date)
        {
            try
            {
                //if (EnumAppConfig.IsLocal)
                //{
                //    _localUnitOfWork.GetRepository<Appointment>().Delete(t => t.Date.Year == date.Year && t.Date.Month == date.Month && t.Date.Day == date.Day);
                //    _localUnitOfWork.Save();

                //    _localUnitOfWork.GetRepository<Appointment>().AddRange(_localUnitOfWork.DataContext.Membership_Users.Select(d => d.UserId).Select(d => new Appointment()
                //    {
                //        ID = Guid.NewGuid(),
                //        UserId = d,
                //        ChangedCount = 0,
                //        Date = date,
                //        Status = (int)EnumAppointmentStatuses.Pending
                //    }).ToList());
                //    _localUnitOfWork.Save();
                //}
                //else
                {



                    _centralizedUnitOfWork.GetRepository<Appointment>().Delete(t => t.Date.Year == date.Year && t.Date.Month == date.Month && t.Date.Day == date.Day);
                    _centralizedUnitOfWork.Save();

                    _centralizedUnitOfWork.GetRepository<Appointment>().AddRange(_localUnitOfWork.DataContext.Membership_Users.Select(d => d.UserId).Select(d => new Appointment()
                    {
                        ID = Guid.NewGuid(),
                        UserId = d,
                        ChangedCount = 0,
                        Date = date,
                        Status = (int)EnumAppointmentStatuses.Pending
                    }).ToList());
                    _centralizedUnitOfWork.Save();
                }
                return new Response<bool>((int)EnumResponseStatuses.Success, EnumResponseMessage.Success, true);
            }
            catch (Exception ex)
            {
                return new Response<bool>((int)EnumResponseStatuses.ErrorSystem, EnumResponseMessage.ErrorSystem, false);
            }
        }

        public Response<Timeslot> GetTimeslotNearest()
        {
            try
            {
                DateTime currentDate = DateTime.Now;
                //if (EnumAppConfig.IsLocal)
                //{
                //    return new Response<Timeslot>((int)EnumResponseStatuses.Success, EnumResponseMessage.Success, _localUnitOfWork.DataContext.Timeslots.Where(t => DbFunctions.TruncateTime(t.Date) >= currentDate.Date && t.StartTime.Value >= currentDate.TimeOfDay).OrderBy(t => t.Date).ThenBy(t => t.StartTime).FirstOrDefault());
                //}
                return new Response<Timeslot>((int)EnumResponseStatuses.Success, EnumResponseMessage.Success, _centralizedUnitOfWork.DataContext.Timeslots.Where(t => DbFunctions.TruncateTime(t.Date) >= currentDate.Date && t.StartTime.Value >= currentDate.TimeOfDay).OrderBy(t => t.Date).ThenBy(t => t.StartTime).FirstOrDefault());
            }
            catch (Exception)
            {

                return new Response<Timeslot>((int)EnumResponseStatuses.ErrorSystem, EnumResponseMessage.ErrorSystem, null);
            }
        }

        public Response<Appointment> UpdateTimeslotForAppointment(Guid appointmentId, string timeslotID)
        {
            try
            {
                var result = GetMyAppointmentByID(appointmentId);
                var appointment = result.Data;
                appointment.Timeslot_ID = timeslotID;
                //if (EnumAppConfig.IsLocal)
                //{
                //    _localUnitOfWork.GetRepository<Appointment>().Update(appointment);
                //    _localUnitOfWork.Save();
                //}
                //else
                {


                    _centralizedUnitOfWork.GetRepository<Appointment>().Update(appointment);
                    _centralizedUnitOfWork.Save();
                }
                return new Response<Appointment>((int)EnumResponseStatuses.Success, EnumResponseMessage.Success, appointment);
            }
            catch (Exception)
            {

                return new Response<Appointment>((int)EnumResponseStatuses.ErrorSystem, EnumResponseMessage.ErrorSystem, null);
            }
        }
        #endregion

        /// <summary>
        /// UpdateReason for absence appointment 
        /// </summary>
        /// <param name="appointmentId"></param>
        /// <param name="absenceId"></param>
        /// <returns></returns>

        private Timeslot GetTimeslotByAppointment(Appointment appointment)
        {
            if (EnumAppConfig.IsLocal)
            {
                var data = _localUnitOfWork.DataContext.Timeslots.FirstOrDefault(t => t.Timeslot_ID == appointment.Timeslot_ID);
                if (data != null)
                {
                    return data;
                }
                else
                {
                    bool centralizeStatus;
                    var centralData = CallCentralized.Get<Timeslot>(EnumAPIParam.Appointment, "GetTimeslotbyAppointment", out centralizeStatus, "appointmentId=" + appointment.ID.ToString());
                    if (centralizeStatus)
                    {
                        return centralData;
                    }
                    return null;
                }
            }
            else
            {
                return _centralizedUnitOfWork.DataContext.Timeslots.FirstOrDefault(t => t.Timeslot_ID == appointment.Timeslot_ID);
            }

        }


















    }
}
