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
            return _localUnitOfWork.DataContext.Appointments.Where(d => d.UserId == UserId && d.Date >= DateTime.Today).OrderBy(d=>d.Date).FirstOrDefault();
        }
        public void UpdateBookTime(string IDAppointment, string timeStart, string timeEnd)
        {
            Trinity.DAL.DBContext.Appointment appointment = GetMyAppointmentByID(new Guid(IDAppointment));
            appointment.FromTime = TimeSpan.Parse(timeStart);
            appointment.ToTime = TimeSpan.Parse(timeEnd);
            appointment.ChangedCount += 1;
            _localUnitOfWork.GetRepository<Appointment>().Update(appointment);
            _localUnitOfWork.Save();

        }
    }
}
