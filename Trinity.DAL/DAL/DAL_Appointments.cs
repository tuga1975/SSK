using System;
using System.Collections.Generic;
using System.Linq;
using Trinity.DAL.DBContext;
using Trinity.DAL.Repository;


namespace Trinity.DAL
{
    public class DAL_Appointments
    {
        Local_UnitOfWork _localUnitOfWork = new Local_UnitOfWork();
        Centralized_UnitOfWork _centralizedUnitOfWork = new Centralized_UnitOfWork();

        public Appointment GetMyAppointmentByID(Guid ID)
        {
            return _localUnitOfWork.DataContext.Appointments.FirstOrDefault(d => d.ID == ID);
        }

        public Trinity.BE.Appointment GetAppointmentDetails(Guid ID)
        {
            var setting = new DAL_Setting().GetSettings(EnumSettingStatuses.Active);
            Appointment appointment = _localUnitOfWork.DataContext.Appointments.Include("TimeSlot").FirstOrDefault(d => d.ID == ID);
            if (appointment != null)
            {
                Membership_Users user = _localUnitOfWork.DataContext.Membership_Users.FirstOrDefault(u => u.UserId == appointment.UserId);
                Trinity.BE.Appointment result = new BE.Appointment()
                {
                    UserId = appointment.UserId,
                    AppointmentDate = appointment.Date,
                    ChangedCount = appointment.ChangedCount,
                    Timeslot_ID = appointment.Timeslot_ID,
                    Name = user.Name,
                    NRIC = user.NRIC,
                    Status = (EnumAppointmentStatuses)appointment.Status,
                    ReportTime = appointment.ReportTime,
                    StartTime = appointment.Timeslot.StartTime,
                    EndTime = appointment.Timeslot.EndTime,

                };
                return result;
            }
            return null;
        }

        public Appointment GetMyAppointmentByDate(string UserId, DateTime date)
        {
            return _localUnitOfWork.DataContext.Appointments.FirstOrDefault(d => d.UserId == UserId && d.Date == date);
        }
        public List<Appointment> GetMyAppointmentBy(string UserId)
        {
            return _localUnitOfWork.DataContext.Appointments.Where(d => d.UserId == UserId && d.Date >= DateTime.Today).ToList();
        }
        public Appointment GetTodayAppointment(string UserId)
        {
            
            return _localUnitOfWork.DataContext.Appointments.Where(d => d.UserId == UserId && d.Date == DateTime.Today).OrderBy(d => d.Date).FirstOrDefault();
        }

        public List<Appointment> GetAllCurrentTimeslotAppointment(TimeSpan currentTime)
        {
            return _localUnitOfWork.DataContext.Appointments.Include("Queues").Include("Timeslot").Where(d => d.Date == DateTime.Today && d.Timeslot_ID.HasValue && d.Timeslot.StartTime.Value == currentTime && d.Queues.Any(q => q.Appointment_ID == d.ID)==false).OrderBy(d => d.Date).ToList();

        }

        public Appointment GetNearestAppointment(string UserId)
        {
            return _localUnitOfWork.DataContext.Appointments.Where(d => d.UserId == UserId && d.Date > DateTime.Today).OrderBy(d => d.Date).FirstOrDefault();
        }
        public Appointment UpdateBookTime(string IDAppointment, string timeStart, string timeEnd)
        {
            Trinity.DAL.DBContext.Appointment appointment = GetMyAppointmentByID(new Guid(IDAppointment));

            var timeSlot = GetTimeslotByAppointmentDate(appointment.Date, TimeSpan.Parse(timeStart), TimeSpan.Parse(timeEnd));
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
            _localUnitOfWork.GetRepository<Appointment>().Update(appointment);
            _localUnitOfWork.Save();
            return appointment;
        }
        public int CountMyAbsence(string UserID)
        {
            return _localUnitOfWork.DataContext.Appointments.Count(d => d.UserId == UserID && d.Date < DateTime.Today && (d.Status == (int)EnumAppointmentStatuses.Pending ||
            d.Status == (int)EnumAppointmentStatuses.Booked) && d.AbsenceReporting_ID == null);
        }

        public List<Appointment> GetMyAbsentAppointments(string UserID)
        {
            return _localUnitOfWork.DataContext.Appointments.Where(d => d.UserId == UserID && System.Data.Entity.DbFunctions.AddDays(d.Date, 1) <= DateTime.Today && (d.Status == (int)EnumAppointmentStatuses.Pending ||
            d.Status == (int)EnumAppointmentStatuses.Booked) && d.AbsenceReporting_ID == null).ToList();
        }

        /// <summary>
        /// UpdateReason for absence appointment 
        /// </summary>
        /// <param name="appointmentId"></param>
        /// <param name="absenceId"></param>
        /// <returns></returns>
        public Appointment UpdateReason(Guid appointmentId, Guid absenceId)
        {
            Trinity.DAL.DBContext.Appointment appointment = GetMyAppointmentByID(appointmentId);
            appointment.AbsenceReporting_ID = absenceId;
            appointment.Status = (int)EnumAppointmentStatuses.Reported;
            _localUnitOfWork.GetRepository<Appointment>().Update(appointment);
            _localUnitOfWork.Save();
            return appointment;
        }

        public List<Appointment> GetListAppointmentFromSelectedDate(List<string> listID)
        {
            var localDbAppointment = new List<Appointment>();


            foreach (var item in listID)
            {
                localDbAppointment.Add(_localUnitOfWork.DataContext.Appointments.Find(Guid.Parse(item)));

            }
            return localDbAppointment;
        }

        public int CountListAppointmentByTimeslot(DateTime appointmentDate, TimeSpan fromTime, TimeSpan toTime)
        {
            var timeSlot = GetTimeslotByAppointmentDate(appointmentDate, fromTime, toTime);
            if (timeSlot != null)
            {

                return _localUnitOfWork.DataContext.Appointments.Count(a => a.Timeslot_ID == timeSlot.Timeslot_ID && a.Date == appointmentDate);
            }
            return 0;

        }

        private Timeslot GetTimeslotByAppointmentDate(DateTime AppointmentDate, TimeSpan startTime, TimeSpan endTime)
        {
            var dayOfWeek = (int)Common.CommonUtil.ConvertToCustomDateOfWeek(AppointmentDate.DayOfWeek);
            var timeSlotEnt = _localUnitOfWork.DataContext.Timeslots.FirstOrDefault(t => t.StartTime == startTime && t.EndTime == endTime && t.DateOfWeek == dayOfWeek);
            return timeSlotEnt;
        }

        public List<BE.Appointment> GetAllAppointments()
        {
            try
            {
                var lstModels = from a in _localUnitOfWork.DataContext.Appointments
                                join tl in _localUnitOfWork.DataContext.Timeslots on a.Timeslot_ID equals tl.Timeslot_ID
                                join u in _localUnitOfWork.DataContext.Membership_Users on a.UserId equals u.UserId
                                orderby a.Date descending, tl.StartTime
                                select new BE.Appointment()
                                {
                                    NRIC = u.NRIC,
                                    Name = u.Name,
                                    ReportTime = a.ReportTime,
                                    Status = (EnumAppointmentStatuses)a.Status,
                                    AppointmentDate = a.Date,
                                    StartTime = tl.StartTime,
                                    EndTime = tl.EndTime,
                                    Category = tl.Category
                                };

                return lstModels.ToList();
            }
            catch (Exception e)
            {
                return new List<BE.Appointment>();
            }
        }

        public List<BE.Statistics> GetAllStatistics()
        {
            var model = from a in _localUnitOfWork.DataContext.Appointments
                        join tl in _localUnitOfWork.DataContext.Timeslots on a.Timeslot_ID equals tl.Timeslot_ID
                        select new BE.Statistics()
                        {
                            Timeslot_ID = tl.Timeslot_ID,
                            StartTime = tl.StartTime,
                            EndTime = tl.EndTime,
                            Date = a.Date
                        };
            return model.Distinct().OrderByDescending(m=>m.Date.Value).ThenBy(m=>m.StartTime.Value).ToList();
        }

        public int CountAppointmentBookedByTimeslot(int timeslotID)
        {
            return _localUnitOfWork.DataContext.Appointments.Count(a => a.Timeslot_ID == timeslotID && a.Status == (int)EnumAppointmentStatuses.Booked);
        }

        public int CountAppointmentReportedByTimeslot(int timeslotID)
        {
            return _localUnitOfWork.DataContext.Appointments.Count(a => a.Timeslot_ID == timeslotID && a.Status == (int)EnumAppointmentStatuses.Reported);
        }

        public int CountAppointmentNoShowByTimeslot(int timeslotID)
        {
            return _localUnitOfWork.DataContext.Appointments.Count(a => a.Timeslot_ID == timeslotID && a.Status != (int)EnumAppointmentStatuses.Booked && a.Status != (int)EnumAppointmentStatuses.Reported);
        }

        public int GetMaximumNumberOfTimeslot(int timeslotID)
        {
            try
            {
                var timeslot = _localUnitOfWork.DataContext.Timeslots.FirstOrDefault(t => t.Timeslot_ID == timeslotID);
                var settingTimeslot = _localUnitOfWork.DataContext.Settings.FirstOrDefault(s => s.Setting_ID == timeslot.Setting_ID);

                switch (timeslot.DateOfWeek)
                {
                    case (int)EnumDayOfWeek.Monday:
                        return settingTimeslot.Mon_MaximumNum.Value;
                    case (int)EnumDayOfWeek.Tuesday:
                        return settingTimeslot.Tue_MaximumNum.Value;
                    case (int)EnumDayOfWeek.Wednesday:
                        return settingTimeslot.Wed_MaximumNum.Value;
                    case (int)EnumDayOfWeek.Thursday:
                        return settingTimeslot.Thu_MaximumNum.Value;
                    case (int)EnumDayOfWeek.Friday:
                        return settingTimeslot.Fri_MaximumNum.Value;
                    case (int)EnumDayOfWeek.Saturday:
                        return settingTimeslot.Sat_MaximumNum.Value;
                    case (int)EnumDayOfWeek.Sunday:
                        return settingTimeslot.Sun_MaximumNum.Value;
                    default:
                        return 0;
                }
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        public void CreateAppointmentsForAllUsers(DateTime date)
        {
            _localUnitOfWork.GetRepository<Appointment>().Delete(t => t.Date.Year == date.Year && t.Date.Month == date.Month && t.Date.Day == date.Day);
            _localUnitOfWork.Save();

            List<string> userIds = _localUnitOfWork.DataContext.Membership_Users.Select(u => u.UserId).ToList();
            var repoAppointment = _localUnitOfWork.GetRepository<Appointment>();
            foreach (string userId in userIds)
            {
                Appointment appointment = new Appointment()
                {
                    ID = Guid.NewGuid(),
                    UserId = userId,
                    ChangedCount = 0,
                    Date = date,
                    Status = (int)EnumAppointmentStatuses.Pending
                };
                repoAppointment.Add(appointment);
            }
            _localUnitOfWork.Save();
        }
    }
}
