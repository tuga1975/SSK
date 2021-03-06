﻿using System;
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
        public List<Trinity.DAL.DBContext.Appointment> GetByDate(DateTime date)
        {
            date = date.Date;
            return _localUnitOfWork.DataContext.Appointments.Where(d => d.Date == date).ToList();
        }
        public Trinity.DAL.DBContext.Appointment GetNextAppointment(string userId)
        {
            try
            {
                Appointment appointment = _localUnitOfWork.DataContext.Appointments.Where(item => item.Date >= DateTime.Today && item.UserId == userId)
                    .OrderBy(item => item.Date).FirstOrDefault();

                return appointment;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public Trinity.DAL.DBContext.Appointment GetNextAppointmentByStatus(string userId,string status)
        {
            try
            {
                Appointment appointment = _localUnitOfWork.DataContext.Appointments.Where(item => item.Date > DateTime.Today && item.UserId == userId && (item.Status==EnumAppointmentStatuses.Booked||item.Status==EnumAppointmentStatuses.Pending))
                    .OrderBy(item => item.Date).FirstOrDefault();

                return appointment;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public AppointmentDetails GetAppointmentDetails(string appointment_ID)
        {
            try
            {
                // Get Company name and Venue
                AppointmentDetails appointmentdetails = _localUnitOfWork.DataContext.Settings.Where(item => item.Year == DateTime.Now.Year)
                    .Select(item => new AppointmentDetails
                    {
                        CompanyName = item.CompanyName,
                        Venue = item.VenueName
                    }).FirstOrDefault();

                // Get supervisee info
                if (appointmentdetails != null)
                {
                    var appointmentInfo = _localUnitOfWork.DataContext.Appointments.Where(item => item.ID.ToString() == appointment_ID)
                        .Select(item => new
                        {
                            Name = item.Membership_Users.Name,
                            Date = item.Date,
                            StartTime = item.Timeslot.StartTime
                        }).FirstOrDefault();

                    if (appointmentInfo != null)
                    {
                        appointmentdetails.Name = appointmentInfo.Name;
                        appointmentdetails.Date = appointmentInfo.Date;
                        appointmentdetails.StartTime = appointmentInfo.StartTime;
                    }
                }

                return appointmentdetails;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public Appointment GetAppointmentByID(Guid ID)
        {
            var data = _localUnitOfWork.DataContext.Appointments.FirstOrDefault(d => d.ID == ID);
            return data;
        }

        public BE.Appointment GetAppmtDetails(Guid ID)
        {
            Appointment appointment = _localUnitOfWork.DataContext.Appointments.Include("TimeSlot").Include("Membership_Users").FirstOrDefault(d => d.ID == ID);
            if (appointment != null)
            {
                BE.Appointment result = SetAppointmentBE(appointment);
                return result;
            }
            return null;
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
                Status = appointment.Status,
                ReportTime = appointment.ReportTime,
                StartTime = appointment.Timeslot.StartTime,
                EndTime = appointment.Timeslot.EndTime,

            };
        }

        public Appointment GetAppointmentByDate(string userId, DateTime date)
        {
            try
            {
                var data = _localUnitOfWork.DataContext.Appointments.FirstOrDefault(d => d.UserId == userId && d.Date == date);
                return data;
            }
            catch (Exception)
            {
                return null;
            }

        }

        public Appointment GetTodayAppointmentByUserId(string UserId)
        {
            try
            {
                var data = _localUnitOfWork.DataContext.Appointments.Where(d => d.UserId == UserId && d.Date == DateTime.Today).OrderBy(d => d.Date).FirstOrDefault();
                return data;
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
                    appointment.Status = EnumAppointmentStatuses.Booked;
                    //appointment.ReportTime = DateTime.Now;

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


            appointment.Status = EnumAppointmentStatuses.Booked;
            try
            {
                _localUnitOfWork.GetRepository<Appointment>().Update(appointment);
                _localUnitOfWork.Save();
                return appointment;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public int CountAbsenceReport(string UserID)
        {
            if (EnumAppConfig.IsLocal)
            {
                return _localUnitOfWork.DataContext.Appointments.Count(d => d.UserId == UserID && d.Status == EnumAppointmentStatuses.Absent && d.AbsenceReporting_ID==null);
            }
            else
            {
                return _centralizedUnitOfWork.DataContext.Appointments.Count(d => d.UserId == UserID && d.Status == EnumAppointmentStatuses.Absent && d.AbsenceReporting_ID == null);
            }
        }

        public List<Appointment> GetAbsentAppointments(string UserID)
        {
            var data = _localUnitOfWork.DataContext.Appointments.Where(d => d.UserId == UserID && d.Status == EnumAppointmentStatuses.Absent && d.AbsenceReporting_ID == null).OrderBy(d => d.Date).ToList();
            return data;
        }

        public Appointment UpdateAbsenceReason(Guid appointmentId, Guid absenceId)
        {
            try
            {
                var appointment = GetAppointmentByID(appointmentId);
                appointment.AbsenceReporting_ID = absenceId;
                appointment.Status = EnumAppointmentStatuses.Absent;

                _localUnitOfWork.GetRepository<Appointment>().Update(appointment);
                _localUnitOfWork.Save();
                return appointment;
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
                    }

                }
                return dbAppointment;

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
                var data = _localUnitOfWork.DataContext.Appointments.Where(a => a.Timeslot_ID == timeSlot.Timeslot_ID && a.Date == appointment.Date);
                if (data != null)
                {
                    return data.Count();
                }
            }
            return 0;
        }
        public List<BE.Appointment> GetAppointmentsByDate(DateTime date,string category, string timeslot)
        {
            date = date.Date;

            var queryFirst = _localUnitOfWork.DataContext.Appointments.Where(d => d.Date == date && d.Status != EnumAppointmentStatuses.Pending && !string.IsNullOrEmpty(d.Timeslot_ID));
            if (string.IsNullOrEmpty(timeslot))
            {
                if (!string.IsNullOrEmpty(category))
                {
                    queryFirst = queryFirst.Where(d => d.Timeslot.Category.Equals(category));
                }
            }
            else
            {
                queryFirst = queryFirst.Where(d=>d.Timeslot_ID.Equals(timeslot));
            }
            //var query = (
            //    from appt in queryFirst
            //    join label in _localUnitOfWork.DataContext.Labels.Where(d=>DbFunctions.TruncateTime(d.Date)==date && d.Label_Type==EnumLabelType.UB) on appt.UserId equals label.UserId into label_temp
            //    from label_value in label_temp.DefaultIfEmpty()
            //    select new { Appointment = appt, Label = label_value });

            return queryFirst.Select(item => new BE.Appointment()
            {
                ID=item.ID.ToString().Trim(),
                NRIC = item.Membership_Users.NRIC,
                Name = item.Membership_Users.Name,
                ReportTime = item.ReportTime,
                Status = item.Status,
                AppointmentDate = item.Date,
                StartTime = item.Timeslot.StartTime,
                EndTime = item.Timeslot.EndTime,
                AbsenceReporting_ID = item.AbsenceReporting_ID,
                Category = item.Timeslot.Category
            }).ToList();
        }
        public List<BE.Appointment> GetAllApptmts()
        {
            try
            {
                var data = _localUnitOfWork.DataContext.Appointments.Include("Timeslot").Include("Membership_Users")
                            .Where(d => !string.IsNullOrEmpty(d.Timeslot_ID) && (d.Status == EnumAppointmentStatuses.Booked || d.Status == EnumAppointmentStatuses.Reported || d.Status == EnumAppointmentStatuses.Absent || d.Status == EnumAppointmentStatuses.Pending))
                            .OrderBy(d => d.Timeslot.StartTime).Select(d => new BE.Appointment()
                            {
                                NRIC = d.Membership_Users.NRIC,
                                Name = d.Membership_Users.Name,
                                ReportTime = d.ReportTime,
                                Status = d.Status,
                                AppointmentDate = d.Date,
                                StartTime = d.Timeslot.StartTime,
                                EndTime = d.Timeslot.EndTime,
                                AbsenceReporting_ID = d.AbsenceReporting_ID,
                                Category = d.Timeslot.Category
                            }).ToList();
                return data;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public List<BE.Statistics> GetAllStats()
        {
            try
            {
                var data = _localUnitOfWork.DataContext.Appointments.Include("Timeslot")
                    .Where(d => !string.IsNullOrEmpty(d.Timeslot_ID) && (d.Status == EnumAppointmentStatuses.Booked || d.Status == EnumAppointmentStatuses.Reported || d.Status == EnumAppointmentStatuses.Absent))
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
                    }).OrderBy(d => d.StartTime).ToList();

                return data;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public int CountApptmtHasUseByTimeslot(string timeslotID)
        {
            try
            {
                if (EnumAppConfig.IsLocal)
                {
                    var data = _localUnitOfWork.DataContext.Appointments.Count(d => !string.IsNullOrEmpty(d.Timeslot_ID) && d.Timeslot_ID == timeslotID) + _localUnitOfWork.DataContext.Queues.Count(d => d.Timeslot_ID == timeslotID && (!d.Appointment_ID.HasValue || (d.Appointment_ID.HasValue && !string.IsNullOrEmpty(d.Appointment.Timeslot_ID) && d.Appointment.Timeslot_ID != timeslotID)));

                    return data;
                }
                else
                {
                    var data = _centralizedUnitOfWork.DataContext.Appointments.Count(d => !string.IsNullOrEmpty(d.Timeslot_ID) && d.Timeslot_ID == timeslotID) + _localUnitOfWork.DataContext.Queues.Count(d => d.Timeslot_ID == timeslotID && (!d.Appointment_ID.HasValue || (d.Appointment_ID.HasValue && !string.IsNullOrEmpty(d.Appointment.Timeslot_ID) && d.Appointment.Timeslot_ID != timeslotID)));

                    return data;
                }

            }
            catch (Exception)
            {

                return 0;
            }
        }

        public int CountApptmtBookedByTimeslot(string timeslotID)
        {
            try
            {
                var data = _localUnitOfWork.DataContext.Appointments.Count(d => !string.IsNullOrEmpty(d.Timeslot_ID) && d.Timeslot_ID == timeslotID && d.Status == EnumAppointmentStatuses.Booked);
                return data;
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
                var data = _localUnitOfWork.DataContext.Appointments.Where(a => a.Timeslot_ID == timeslotID && a.Status == EnumAppointmentStatuses.Reported);
                if (data != null)
                {
                    return data.Count();
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
                var data = _localUnitOfWork.DataContext.Appointments.Where(a => a.Timeslot_ID == timeslotID && a.Status == EnumAppointmentStatuses.Absent);
                if (data != null)
                {
                    return data.Count();
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
                timeslot = _localUnitOfWork.DataContext.Timeslots.FirstOrDefault(t => t.Timeslot_ID == timeslotID);
                if (timeslot != null && timeslot.MaximumSupervisee.HasValue)
                {
                    return timeslot.MaximumSupervisee.Value;
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
                        Status = EnumAppointmentStatuses.Pending
                    });
                }

                _localUnitOfWork.GetRepository<Appointment>().AddRange(appointments);
                _localUnitOfWork.Save();

                return _localUnitOfWork.Save() > 0;
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
                var data = _localUnitOfWork.DataContext.Timeslots.Where(t => DbFunctions.TruncateTime(t.Date) >= currentDate.Date && t.StartTime.Value >= currentDate.TimeOfDay)
                    .OrderBy(t => t.Date).ThenBy(t => t.StartTime).FirstOrDefault();
                return data;

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
                _localUnitOfWork.GetRepository<Appointment>().Update(appointment);
                _localUnitOfWork.Save();

                return appointment;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private Timeslot GetTimeslotByAppointment(Appointment appointment)
        {
            var data = _localUnitOfWork.DataContext.Timeslots.FirstOrDefault(t => t.Timeslot_ID == appointment.Timeslot_ID);
            return data;
        }
        #endregion
    }
}
