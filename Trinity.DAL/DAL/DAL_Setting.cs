using System;
using System.Collections.Generic;
using System.Linq;
using Trinity.DAL.DBContext;
using Trinity.DAL.Repository;


namespace Trinity.DAL
{
    public class DAL_Setting
    {
        Local_UnitOfWork _localUnitOfWork = new Local_UnitOfWork();
        Centralized_UnitOfWork _centralizedUnitOfWork = new Centralized_UnitOfWork();


        public Trinity.BE.EnvironmentTimeDetail GetEnvironmentTime(DateTime date)
        {
            var appointmentTime = new Trinity.BE.EnvironmentTimeDetail();
            int dateOfWeek = date.DayOfWeek();
            var listTimeSlot = GetListTimeslotByDayOfWeek(dateOfWeek);
            if (listTimeSlot.Count > 0)
            {

                var morningTimeSpan = new TimeSpan(12, 0, 0);
                var eveningTimeSpan = new TimeSpan(17, 0, 0);

                foreach (var item in listTimeSlot)
                {
                    var setTime = SetEnviromentTime(item);
                    if (setTime.EndTime <= morningTimeSpan)
                    {
                        appointmentTime.Morning.Add(setTime);
                    }
                    else if (setTime.EndTime > eveningTimeSpan)
                    {
                        appointmentTime.Evening.Add(setTime);
                    }
                    else
                    {
                        appointmentTime.Afternoon.Add(setTime);
                    }
                }

            }
            return appointmentTime;
        }

        private static BE.EnvironmentTime SetEnviromentTime(DBContext.Timeslot timeSlot)
        {

            var environmentTime = new BE.EnvironmentTime()
            {
                StartTime = timeSlot.StartTime.Value,
                EndTime = timeSlot.EndTime.Value,
                IsAvailble = true,
                IsSelected = false
            };
            return environmentTime;
        }

        public Trinity.BE.EnvironmentTimeDetail GetTodayEnvironmentSetting()
        {
            var today = DateTime.Now;
            int DateOfWeek = today.DayOfWeek();
            return GetEnvironmentTime(today);
        }

        public List<Timeslot> GetListTimeslotByDayOfWeek(int dayOfWeek)
        {
            return _localUnitOfWork.DataContext.Timeslots.Where(t => t.DateOfWeek == dayOfWeek).ToList();
        }
    }
}
