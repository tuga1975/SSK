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


        public List<Trinity.BE.EnvironmentTime> GetEnvironment(DateTime date)
        {
            List<Trinity.BE.EnvironmentTime> array = new List<BE.EnvironmentTime>();
            int DateOfWeek = date.DayOfWeek();
            var setting = _localUnitOfWork.DataContext.Environments.Where(d => d.DateOfWeek == DateOfWeek && d.Frequency == (int)Frequency.Weekly).FirstOrDefault();
            if (setting != null)
            {
                var allTimeBooked = _localUnitOfWork.DataContext.Appointments.Where(d =>d.Date == date.Date && d.FromTime.HasValue && d.ToTime.HasValue).Select(d => d.FromTime.Value.Hours+ d.FromTime.Value.Minutes+ d.ToTime.Value.Hours + d.ToTime.Value.Minutes).Distinct().ToList();
                while (setting.StartTime < setting.EndTime)
                {
                    array.Add(new BE.EnvironmentTime() {
                        StartTime = setting.StartTime,
                        EndTime = setting.StartTime.Add(TimeSpan.FromMinutes(setting.Duration)),
                        IsAvailble = !allTimeBooked.Contains(setting.StartTime.Hours+ setting.StartTime.Minutes+ setting.StartTime.Add(TimeSpan.FromMinutes(setting.Duration)).Hours+ setting.StartTime.Add(TimeSpan.FromMinutes(setting.Duration)).Minutes)
                    });
                    setting.StartTime = setting.StartTime.Add(TimeSpan.FromMinutes(setting.Duration));
                }
            }
            return array;
        }
    }
}
