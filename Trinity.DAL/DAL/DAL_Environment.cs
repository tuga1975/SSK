using System;
using System.Collections.Generic;
using System.Linq;
using Trinity.DAL.DBContext;
using Trinity.DAL.Repository;


namespace Trinity.DAL
{
    public class DAL_Environment
    {
        Local_UnitOfWork _localUnitOfWork = new Local_UnitOfWork();
        Centralized_UnitOfWork _centralizedUnitOfWork = new Centralized_UnitOfWork();


        public Trinity.BE.EnvironmentTimeDetail GetEnvironmentTime(DateTime date)
        {
            var appointmentTime = new Trinity.BE.EnvironmentTimeDetail();
            int DateOfWeek = date.DayOfWeek();
            var setting = _localUnitOfWork.DataContext.Environments.Where(d => d.DateOfWeek == DateOfWeek && d.Frequency == EnumFrequency.Weekly).FirstOrDefault();
            if (setting != null)
            {

                var morningTimeSpan = new TimeSpan(12, 0, 0);
                var eveningTimeSpan = new TimeSpan(17, 0, 0);
                while (setting.StartTime < setting.EndTime)
                {
                    var setTime = SetEnviromentTime(setting);
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

                    setting.StartTime = setting.StartTime.Add(TimeSpan.FromMinutes(setting.Duration));
                }

            }
            return appointmentTime;
        }

        private static BE.EnvironmentTime SetEnviromentTime(DBContext.Environment setting)
        {

            var environmentTime = new BE.EnvironmentTime()
            {
                StartTime = setting.StartTime,
                EndTime = setting.StartTime.Add(TimeSpan.FromMinutes(setting.Duration)),
                IsAvailble = true,
                IsSelected = false
            };
            return environmentTime;
        }

        public DBContext.Environment GetTodayEnvironmentSetting()
        {
            var today = DateTime.Now;
            int DateOfWeek = today.DayOfWeek();
            return  _localUnitOfWork.DataContext.Environments.Where(d => d.DateOfWeek == DateOfWeek && d.Frequency == EnumFrequency.Weekly).FirstOrDefault();
        }
    }
}
