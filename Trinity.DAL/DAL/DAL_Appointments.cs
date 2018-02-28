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

        #region refactor 2018
        public Trinity.DAL.DBContext.Appointment GetNextAppointment(string userId)
        {
            try
            {
                // get from localdb
                if (EnumAppConfig.IsLocal)
                {
                    Appointment appointment = _localUnitOfWork.DataContext.Appointments.Where(item => item.Date >= DateTime.Today && item.UserId == userId)
                        .OrderBy(item => item.Date).FirstOrDefault();

                    // if local have no data, get data from centralizeddb and update localdb
                    if (appointment == null && !EnumAppConfig.ByPassCentralizedDB)
                    {

                    }

                    return appointment;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public Trinity.BE.Appointment GetAppointment(string appointment_ID)
        {
            try
            {
                // get from localdb
                if (EnumAppConfig.IsLocal)
                {
                    Trinity.BE.Appointment appointment = _localUnitOfWork.DataContext.Appointments
                        .Where(item => item.ID.ToString() == appointment_ID)
                        .Select(item => new BE.Appointment
                        {
                            UserId = item.UserId,
                            AppointmentDate = item.Date,
                            ChangedCount = item.ChangedCount,
                            Timeslot_ID = item.Timeslot_ID,
                            Name = item.Membership_Users.Name,
                            NRIC = item.Membership_Users.NRIC,
                            Status = (EnumAppointmentStatuses)item.Status,
                            ReportTime = item.ReportTime,
                            StartTime = item.Timeslot.StartTime,
                            EndTime = item.Timeslot.EndTime,

                        }).FirstOrDefault();

                    return appointment;
                }

                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

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
                    if (data != null || EnumAppConfig.ByPassCentralizedDB)
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

        public bool UpdateTimeslot_ID(string appointment_ID, string timeslot_ID)
        {
            try
            {
                if (EnumAppConfig.IsLocal)
                {
                    var appointment = _localUnitOfWork.DataContext.Appointments.FirstOrDefault(item => item.ID.ToString() == appointment_ID);
                    appointment.Timeslot_ID = timeslot_ID;
                    appointment.ChangedCount += 1;

                    var entry = _localUnitOfWork.DataContext.Entry(appointment);
                    entry.State = EntityState.Modified;

                    return _localUnitOfWork.DataContext.SaveChanges() > 0;
                }

                return false;
            }
            catch (Exception ex)
            {
                return false;
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
            var result = GetAppointmentByID(new Guid(IDAppointment));
            var appointment = result;
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
                    var data = _localUnitOfWork.DataContext.Appointments.Where(d => d.UserId == UserID && d.Date < DateTime.Today && !d.Queues.Any() && d.AbsenceReporting_ID == null);
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
                    var data = _centralizedUnitOfWork.DataContext.Appointments.Where(d => d.UserId == UserID && d.Date < DateTime.Today && !d.Queues.Any() && d.AbsenceReporting_ID == null);
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
                    var data = _localUnitOfWork.DataContext.Appointments.Where(d => d.UserId == UserID && System.Data.Entity.DbFunctions.TruncateTime(d.Date) < DateTime.Today && !d.Queues.Any()  && d.AbsenceReporting_ID == null).ToList();

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
                    var data = _centralizedUnitOfWork.DataContext.Appointments.Where(d => d.UserId == UserID && System.Data.Entity.DbFunctions.TruncateTime(d.Date) < DateTime.Today && !d.Queues.Any() && d.AbsenceReporting_ID == null).ToList();

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
                        var guid = Guid.Empty;
                        if (Guid.TryParse(item, out guid))
                        {
                            var find = _localUnitOfWork.DataContext.Appointments.Find(guid);
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
                    return dbAppointment;
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

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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
                    var data = _localUnitOfWork.DataContext.Appointments.Include("Timeslot").Include("Membership_Users")
                                .Where(d => !string.IsNullOrEmpty(d.Timeslot_ID) && (d.Status == (int)EnumAppointmentStatuses.Booked || d.Status == (int)EnumAppointmentStatuses.Reported || d.AbsenceReporting_ID != null))
                                .OrderBy(d => d.Timeslot.StartTime).Select(d => new BE.Appointment()
                                {
                                    NRIC = d.Membership_Users.NRIC,
                                    Name = d.Membership_Users.Name,
                                    ReportTime = d.ReportTime,
                                    Status = (EnumAppointmentStatuses)d.Status,
                                    AppointmentDate = d.Date,
                                    StartTime = d.Timeslot.StartTime,
                                    EndTime = d.Timeslot.EndTime,
                                    AbsenceReporting_ID = d.AbsenceReporting_ID
                                }).ToList();
                    if (data != null || EnumAppConfig.ByPassCentralizedDB)
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

                    var data = _centralizedUnitOfWork.DataContext.Appointments.Include("Timeslot").Include("Membership_Users")
                                .Where(d => !string.IsNullOrEmpty(d.Timeslot_ID) && (d.Status == (int)EnumAppointmentStatuses.Booked || d.Status == (int)EnumAppointmentStatuses.Reported || d.AbsenceReporting_ID != null))
                                .OrderBy(d => d.Timeslot.StartTime).Select(d => new BE.Appointment()
                                {
                                    NRIC = d.Membership_Users.NRIC,
                                    Name = d.Membership_Users.Name,
                                    ReportTime = d.ReportTime,
                                    Status = (EnumAppointmentStatuses)d.Status,
                                    AppointmentDate = d.Date,
                                    StartTime = d.Timeslot.StartTime,
                                    EndTime = d.Timeslot.EndTime,
                                    AbsenceReporting_ID = d.AbsenceReporting_ID
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
                    var data = _localUnitOfWork.DataContext.Appointments.Include("Timeslot").Where(d => !string.IsNullOrEmpty(d.Timeslot_ID) && (d.Status == (int)EnumAppointmentStatuses.Booked || d.Status == (int)EnumAppointmentStatuses.Reported || d.AbsenceReporting_ID != null))
                        .Select(d => new
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

                    if (data != null || EnumAppConfig.ByPassCentralizedDB)
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
                    var data = _centralizedUnitOfWork.DataContext.Appointments.Include("Timeslot").Where(d => !string.IsNullOrEmpty(d.Timeslot_ID) && (d.Status == (int)EnumAppointmentStatuses.Booked || d.Status == (int)EnumAppointmentStatuses.Reported || d.AbsenceReporting_ID != null))
                        .Select(d => new
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
                    var data = _localUnitOfWork.DataContext.Appointments.Count(d => !string.IsNullOrEmpty(d.Timeslot_ID) && d.Timeslot_ID == timeslotID) + _localUnitOfWork.DataContext.Queues.Count(d => d.Timeslot_ID == timeslotID && (!d.Appointment_ID.HasValue || (d.Appointment_ID.HasValue && !string.IsNullOrEmpty(d.Appointment.Timeslot_ID) && d.Appointment.Timeslot_ID != timeslotID)));

                    return data;
                    // if (!EnumAppConfig.ByPassCentralizedDB)
                    //{
                    //    bool centralizeStatus;
                    //    var centralData = CallCentralized.Get<int>(EnumAPIParam.Appointment, EnumAPIParam.CountBookedByTimeslot, out centralizeStatus);
                    //    if (centralizeStatus)
                    //    {
                    //        return centralData;
                    //    }
                    //}

                }
                else
                {
                    var data = _localUnitOfWork.DataContext.Appointments.Count(d => !string.IsNullOrEmpty(d.Timeslot_ID) && d.Timeslot_ID == timeslotID) + _localUnitOfWork.DataContext.Queues.Count(d => d.Timeslot_ID == timeslotID && (!d.Appointment_ID.HasValue || (d.Appointment_ID.HasValue && !string.IsNullOrEmpty(d.Appointment.Timeslot_ID) && d.Appointment.Timeslot_ID != timeslotID)));

                    return data;
                }

            }
            catch (Exception)
            {

                return 0;
            }
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
                    else if (!EnumAppConfig.ByPassCentralizedDB)
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

        public int CountApptmtAbsentByTimeslot(string timeslotID)
        {
            try
            {
                if (EnumAppConfig.IsLocal)
                {
                    var data = _localUnitOfWork.DataContext.Appointments.Where(a => a.Timeslot_ID == timeslotID && a.AbsenceReporting_ID != null);
                    if (data != null)
                    {
                        return data.Count();
                    }
                    else if (!EnumAppConfig.ByPassCentralizedDB)
                    {
                        bool centralizeStatus;
                        var centralData = CallCentralized.Get<int>(EnumAPIParam.Appointment, "CountAbsentdByTimeslot", out centralizeStatus);
                        if (centralizeStatus)
                        {
                            return centralData;
                        }
                    }
                }
                else
                {
                    var data = _centralizedUnitOfWork.DataContext.Appointments.Where(a => a.Timeslot_ID == timeslotID && a.AbsenceReporting_ID != null);
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
                    else if (!EnumAppConfig.ByPassCentralizedDB)
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
                    return timeslot.MaximumSupervisee != null ? timeslot.MaximumSupervisee.Value : 0;
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

                        var superviseeRoleId = _localUnitOfWork.DataContext.Membership_Roles.Where(item => item.Name == EnumUserRoles.Supervisee).Select(item => item.Id).FirstOrDefault();

                        var userIds = _localUnitOfWork.DataContext.Membership_Users.Where(item => item.Membership_UserRoles.Any(item2 => item2.RoleId == superviseeRoleId))
                            .Select(d => d.UserId).ToList();

                        List<Appointment> appointments = new List<Appointment>();
                        foreach (var item in userIds)
                        {
                            appointments.Add(new Appointment()
                            {
                                ID = Guid.NewGuid(),
                                UserId = item,
                                ChangedCount = 0,
                                Date = date,
                                Status = (int)EnumAppointmentStatuses.Pending
                            });
                        }

                        _localUnitOfWork.GetRepository<Appointment>().AddRange(appointments);
                        _localUnitOfWork.Save();

                        return centralizeStatus;
                    }
                }
                else
                {
                    _centralizedUnitOfWork.GetRepository<Appointment>().Delete(t => t.Date.Year == date.Year && t.Date.Month == date.Month && t.Date.Day == date.Day);
                    _centralizedUnitOfWork.Save();

                    var superviseeRoleId = _centralizedUnitOfWork.DataContext.Membership_Roles.Where(item => item.Name == EnumUserRoles.Supervisee).Select(item => item.Id).FirstOrDefault();

                    var userIds = _centralizedUnitOfWork.DataContext.Membership_Users.Where(item => item.Membership_UserRoles.Any(item2 => item2.RoleId == superviseeRoleId))
                        .Select(d => d.UserId).ToList();

                    List<Appointment> appointments = new List<Appointment>();
                    foreach (var item in userIds)
                    {
                        appointments.Add(new Appointment()
                        {
                            ID = Guid.NewGuid(),
                            UserId = item,
                            ChangedCount = 0,
                            Date = date,
                            Status = (int)EnumAppointmentStatuses.Pending
                        });
                    }

                    _centralizedUnitOfWork.GetRepository<Appointment>().AddRange(appointments);
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
                    if (data != null || EnumAppConfig.ByPassCentralizedDB)
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
