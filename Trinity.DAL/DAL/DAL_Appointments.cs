﻿using System;
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
    }
}
