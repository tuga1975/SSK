using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity.DAL.DBContext;
using Trinity.DAL.Repository;

namespace Trinity.DAL
{
    public class DAL_Timeslots
    {
        Local_UnitOfWork _localUnitOfWork = new Local_UnitOfWork();
        Centralized_UnitOfWork _centralizedUnitOfWork = new Centralized_UnitOfWork();


        public Timeslot GetByID(string Timeslot_ID)
        {
            return _localUnitOfWork.DataContext.Timeslots.FirstOrDefault(d => d.Timeslot_ID == Timeslot_ID);
        }

        public List<Timeslot> GetTimeSlots(DateTime date)
        {
            try
            {
                List<Timeslot> timeslots = _localUnitOfWork.DataContext.Timeslots.Where(item => DbFunctions.TruncateTime(item.Date) == date.Date).OrderBy(item => item.StartTime).ToList();

                return timeslots;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public Timeslot GetNextTimeSlotFromDB()
        {
            try
            {
                var date = DateTime.Now.Date;
                Timeslot timeslot = _localUnitOfWork.DataContext.Timeslots
                    .Where(item => DbFunctions.TruncateTime(item.Date) > date)
                    .OrderBy(item => item.StartTime)
                    .FirstOrDefault();

                return timeslot;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public bool CheckTimeslot(string timeslotId,DateTime date)
        {
            var today = DateTime.Now.TimeOfDay;
            try
            {
                // get data from local db
                if (EnumAppConfig.IsLocal)
                {
                    Timeslot timeslot = _localUnitOfWork.DataContext.Timeslots.FirstOrDefault(item => item.Timeslot_ID == timeslotId);
                    if (date>DateTime.Now.Date)
                    {
                        return false;
                    }
                    if (timeslot.EndTime < today)
                    {
                        return true;//is past
                    }
                    return false;


                }
                else // request from centralized api
                {
                    Timeslot timeslot = _centralizedUnitOfWork.DataContext.Timeslots.FirstOrDefault(item => item.Timeslot_ID == timeslotId);
                    if (timeslot.EndTime < today)
                    {
                        return true;//is past
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                return true;
            }
        }

        //public bool CheckAvailableTimeslot(Timeslot time)
        //{
        //    int totalSlot = _localUnitOfWork.DataContext.Appointments.Count(d => !string.IsNullOrEmpty(d.Timeslot_ID) && d.Timeslot_ID == time.Timeslot_ID) + _localUnitOfWork.DataContext.Queues.Count(d => d.Timeslot_ID == time.Timeslot_ID && (!d.Appointment_ID.HasValue || (d.Appointment_ID.HasValue && !string.IsNullOrEmpty(d.Appointment.Timeslot_ID) && d.Appointment.Timeslot_ID != time.Timeslot_ID)));

        //    if (totalSlot>=time.MaximumSupervisee)
        //    {
        //        return false;
        //    }
        //    return true;
        //}
    }
}
