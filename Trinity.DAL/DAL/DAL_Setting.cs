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

        public void GenerateTimeslot()
        {
            var setting = GetSetting();

            //mon
            GenerateTimeSlotAndInsert((int)EnumDayOfWeek.Monday, setting.Monday);

            //tue
            GenerateTimeSlotAndInsert((int)EnumDayOfWeek.Tuesday, setting.Tuesday);

            //wed
            GenerateTimeSlotAndInsert((int)EnumDayOfWeek.Wednesday, setting.WednesDay);

            //thu
            GenerateTimeSlotAndInsert((int)EnumDayOfWeek.Thursday, setting.Thursday);

            //fri
            GenerateTimeSlotAndInsert((int)EnumDayOfWeek.Friday, setting.Friday);

            //sat
            GenerateTimeSlotAndInsert((int)EnumDayOfWeek.Saturday, setting.Saturday);

            //sun
            GenerateTimeSlotAndInsert((int)EnumDayOfWeek.Sunday, setting.Sunday);

        }

        private void GenerateTimeSlotAndInsert(int dayOfWeek, Trinity.BE.SettingDetails model)
        {

            if (model.StartTime.HasValue && model.EndTime.HasValue && model.Duration.HasValue)
            {
                var fromTime = model.StartTime;
                var toTime = model.EndTime;
                var id = _localUnitOfWork.DataContext.Timeslots.Any() ? _localUnitOfWork.DataContext.Timeslots.Max(t => t.Timeslot_ID) : 0;
                while (fromTime < toTime)
                {
                    var timeSlot = new BE.TimeslotDetails()
                    {
                        Timeslot_ID = (id + 1),
                        DateOfWeek = dayOfWeek,
                        StartTime = fromTime,
                        EndTime = fromTime.Value.Add(TimeSpan.FromMinutes(model.Duration.Value))
                    };
                    fromTime = fromTime.Value.Add(TimeSpan.FromMinutes(model.Duration.Value));

                    _localUnitOfWork.GetRepository<Timeslot>().Add(SetInfo(new Timeslot(), timeSlot));
                    _localUnitOfWork.Save();
                }
            }

        }


        private Timeslot SetInfo(Timeslot dbTimeslot, BE.TimeslotDetails model)
        {
            dbTimeslot.Timeslot_ID = model.Timeslot_ID;
            dbTimeslot.StartTime = model.StartTime;
            dbTimeslot.EndTime = model.EndTime;
            dbTimeslot.DateOfWeek = model.DateOfWeek;

            return dbTimeslot;
        }

        private void SetInfoToSettingBE(BE.SettingBE settingBE, Setting setting)
        {
            settingBE.Mon_Open_Time = setting.Mon_Open_Time;
            settingBE.Mon_Close_Time = setting.Mon_Close_Time;
            settingBE.Mon_Interval = setting.Mon_Interval;
            settingBE.Tue_Open_Time = setting.Tue_Open_Time;
            settingBE.Tue_Close_Time = setting.Tue_Close_Time;
            settingBE.Tue_Interval = setting.Tue_Interval;
            settingBE.Wed_Open_Time = setting.Wed_Open_Time;
            settingBE.Wed_Close_Time = setting.Wed_Close_Time;
            settingBE.Wed_Interval = setting.Wed_Interval;
            settingBE.Thu_Open_Time = setting.Thu_Open_Time;
            settingBE.Thu_Close_Time = setting.Thu_Close_Time;
            settingBE.Thu_Interval = setting.Thu_Interval;
            settingBE.Fri_Open_Time = setting.Fri_Open_Time;
            settingBE.Fri_Close_Time = setting.Fri_Close_Time;
            settingBE.Fri_Interval = setting.Fri_Interval;
            settingBE.Sat_Open_Time = setting.Sat_Open_Time;
            settingBE.Sat_Close_Time = setting.Sat_Close_Time;
            settingBE.Sat_Interval = setting.Sat_Interval;
            settingBE.Sun_Open_Time = setting.Sun_Open_Time;
            settingBE.Sun_Close_Time = setting.Sun_Close_Time;
            settingBE.Sun_Interval = setting.Sun_Interval;
            settingBE.Last_Updated_Date = setting.Last_Updated_Date;
            settingBE.MaxSuperviseePerTimeslot = setting.MaxSuperviseePerTimeslot;
            settingBE.ReservedForSpare = setting.ReservedForSpare;
        }

        private void SetInfoToSettingDB(BE.SettingBE settingBE, Setting setting)
        {
            setting.Mon_Open_Time = settingBE.Mon_Open_Time;
            setting.Mon_Close_Time = settingBE.Mon_Close_Time;
            setting.Mon_Interval = settingBE.Mon_Interval;
            setting.Tue_Open_Time = settingBE.Tue_Open_Time;
            setting.Tue_Close_Time = settingBE.Tue_Close_Time;
            setting.Tue_Interval = settingBE.Tue_Interval;
            setting.Wed_Open_Time = settingBE.Wed_Open_Time;
            setting.Wed_Close_Time = settingBE.Wed_Close_Time;
            setting.Wed_Interval = settingBE.Wed_Interval;
            setting.Thu_Open_Time = settingBE.Thu_Open_Time;
            setting.Thu_Close_Time = settingBE.Thu_Close_Time;
            setting.Thu_Interval = settingBE.Thu_Interval;
            setting.Fri_Open_Time = settingBE.Fri_Open_Time;
            setting.Fri_Close_Time = settingBE.Fri_Close_Time;
            setting.Fri_Interval = settingBE.Fri_Interval;
            setting.Sat_Open_Time = settingBE.Sat_Open_Time;
            setting.Sat_Close_Time = settingBE.Sat_Close_Time;
            setting.Sat_Interval = settingBE.Sat_Interval;
            setting.Sun_Open_Time = settingBE.Sun_Open_Time;
            setting.Sun_Close_Time = settingBE.Sun_Close_Time;
            setting.Sun_Interval = settingBE.Sun_Interval;
            setting.Last_Updated_Date = settingBE.Last_Updated_Date;
            setting.MaxSuperviseePerTimeslot = settingBE.MaxSuperviseePerTimeslot;
            setting.ReservedForSpare = settingBE.ReservedForSpare;
        }
        private BE.SettingModel GetSetting()
        {
            var dbSeting = _localUnitOfWork.DataContext.Settings.FirstOrDefault();
            var settingBE = new BE.SettingBE();
            SetInfoToSettingBE(settingBE, dbSeting);

            return new BE.SettingBE().ToSettingModel(settingBE);
        }

        public void UpdateTimeslot(int dayOfWeek, BE.SettingDetails model)
        {
            var timeSlotRepo = _localUnitOfWork.GetRepository<Timeslot>();

            //delete
            var listDbTimeslot = timeSlotRepo.GetMany(t => t.DateOfWeek == dayOfWeek).ToList();

            foreach (var item in listDbTimeslot)
            {
                if (_localUnitOfWork.DataContext.Appointments.Any(a => a.Timeslot_ID == item.Timeslot_ID))
                {
                    break;
                }
                timeSlotRepo.Delete(item);
            }

            _localUnitOfWork.Save();
            //add new
            GenerateTimeSlotAndInsert(dayOfWeek, model);


        }
        public void UpdateSetting(Trinity.BE.SettingBE model)
        {
            var repo = _localUnitOfWork.GetRepository<Setting>();
            var dbSetting = repo.GetAll().FirstOrDefault();
            SetInfoToSettingDB(model, dbSetting);

            repo.Update(dbSetting);
            _localUnitOfWork.Save();

        }
    }
}
