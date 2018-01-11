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
                    EndTime = appointment.Timeslot.EndTime
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
        public Appointment GetMyAppointmentCurrent(string UserId)
        {
            return _localUnitOfWork.DataContext.Appointments.Include("Timeslot").Where(d => d.UserId == UserId && d.Date >= DateTime.Today).OrderBy(d => d.Date).FirstOrDefault();
        }
        public Appointment UpdateBookTime(string IDAppointment, string timeStart, string timeEnd)
        {
            Trinity.DAL.DBContext.Appointment appointment = GetMyAppointmentByID(new Guid(IDAppointment));

            var timeSlot = GetTodayTimeslotByStartAndEndTime(TimeSpan.Parse(timeStart), TimeSpan.Parse(timeEnd));
            if (timeSlot != null)
            {
                appointment.Timeslot_ID = timeSlot.Timeslot_ID;
            }

            appointment.ChangedCount += 1;
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

        public int CountListAppointmentByTimeslot(TimeSpan fromTime, TimeSpan toTime)
        {
            var timeSlot = GetTodayTimeslotByStartAndEndTime(fromTime, toTime);
            if (timeSlot != null)
            {

                return _localUnitOfWork.DataContext.Appointments.Count(a => a.Timeslot_ID == timeSlot.Timeslot_ID);
            }
            return 0;

        }

        private Timeslot GetTodayTimeslotByStartAndEndTime(TimeSpan startTime, TimeSpan endTime)
        {
            var dayOfWeek = (int)DateTime.Now.DayOfWeek;
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
                                orderby tl.StartTime
                                select new BE.Appointment()
                                {
                                    NRIC = u.NRIC,
                                    Name = u.Name,
                                    ReportTime = a.ReportTime,
                                    Status = (EnumAppointmentStatuses)a.Status,
                                    AppointmentDate = a.Date,
                                    StartTime = tl.StartTime,
                                    EndTime = tl.EndTime
                                };

                return lstModels.ToList();
            }
            catch(Exception e)
            {
                return new List<BE.Appointment>();
            }
        }
        
        public List<BE.Statistics> GetAllStatistics()
        {
            var model = from tl in _localUnitOfWork.DataContext.Timeslots
                        join a in _localUnitOfWork.DataContext.Appointments on tl.Timeslot_ID equals a.Timeslot_ID
                        select new BE.Statistics()
                        {
                            Timeslot_ID = tl.Timeslot_ID,
                            StartTime = tl.StartTime,
                            EndTime = tl.EndTime,
                            Date = a.Date//,
                            //Max = GetMaximumNumberOfTimeslot(tl.Timeslot_ID),
                            //Booked = CountAppointmentBookedByTimeslot(tl.Timeslot_ID),
                            //Reported = CountAppointmentReportedByTimeslot(tl.Timeslot_ID),
                            //No_Show = CountAppointmentNoShowByTimeslot(tl.Timeslot_ID),
                            //Available = GetMaximumNumberOfTimeslot(tl.Timeslot_ID) - CountAppointmentBookedByTimeslot(tl.Timeslot_ID) - CountAppointmentReportedByTimeslot(tl.Timeslot_ID) - CountAppointmentNoShowByTimeslot(tl.Timeslot_ID)
                        };
            return model.Distinct().ToList();
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
    }
}
