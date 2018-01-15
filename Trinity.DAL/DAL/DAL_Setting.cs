using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Trinity.BE;
using Trinity.DAL.DBContext;
using Trinity.DAL.Repository;


namespace Trinity.DAL
{
    public class DAL_Setting
    {
        Local_UnitOfWork _localUnitOfWork = new Local_UnitOfWork();
        Centralized_UnitOfWork _centralizedUnitOfWork = new Centralized_UnitOfWork();


        public Trinity.BE.AppointmentTime GetAppointmentTime(DateTime date)
        {
            var appointmentTime = new Trinity.BE.AppointmentTime();
            var listTimeSlot = GetTimeslots(date);
            if (listTimeSlot.Count > 0)
            {

                var morningTimeSpan = new TimeSpan(12, 0, 0);
                var eveningTimeSpan = new TimeSpan(17, 0, 0);

                foreach (var item in listTimeSlot)
                {
                    var setTime = SetAppointmentTime(item);
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

        private static BE.AppointmentTimeDetails SetAppointmentTime(DBContext.Timeslot timeSlot)
        {
            var environmentTime = new BE.AppointmentTimeDetails()
            {
                StartTime = timeSlot.StartTime.Value,
                EndTime = timeSlot.EndTime.Value,
                IsAvailble = true,
                IsSelected = false
            };
            return environmentTime;
        }

        public Trinity.BE.AppointmentTime GetCurrentAppointmentTime()
        {
            var today = DateTime.Now;
            return GetAppointmentTime(today);
        }

        public List<Timeslot> GetTimeslots(DateTime date)
        {
            SettingModel setting = GetSettings(EnumSettingStatuses.Active);
            //GenerateTimeslots("dfbb2a6a-9e45-4a76-9f75-af1a7824a947");
            int dayOfWeek = date.DayOfWeek();
            return _localUnitOfWork.DataContext.Timeslots.Where(t => t.DateOfWeek == dayOfWeek && t.Setting_ID.HasValue && t.Setting_ID.Value == setting.Setting_ID).ToList();
        }

        public void GenerateTimeslots(string createdBy)
        {
            // Allow to change pending settings only
            SettingModel settings = GetSettings(EnumSettingStatuses.Pending);
            if (settings == null)
            {
                // Couldn't change timeslot
                return;
            }

            //mon
            GenerateTimeslotAndInsert((int)EnumDayOfWeek.Monday, settings.Monday, createdBy);

            //tue
            GenerateTimeslotAndInsert((int)EnumDayOfWeek.Tuesday, settings.Tuesday, createdBy);

            //wed
            GenerateTimeslotAndInsert((int)EnumDayOfWeek.Wednesday, settings.WednesDay, createdBy);

            //thu
            GenerateTimeslotAndInsert((int)EnumDayOfWeek.Thursday, settings.Thursday, createdBy);

            //fri
            GenerateTimeslotAndInsert((int)EnumDayOfWeek.Friday, settings.Friday, createdBy);

            //sat
            GenerateTimeslotAndInsert((int)EnumDayOfWeek.Saturday, settings.Saturday, createdBy);

            //sun
            GenerateTimeslotAndInsert((int)EnumDayOfWeek.Sunday, settings.Sunday, createdBy);

        }

        private void GenerateTimeslotAndInsert(int dayOfWeek, Trinity.BE.SettingDetails model, string createBy)
        {
            if (model.StartTime.HasValue && model.EndTime.HasValue && model.Duration.HasValue)
            {
                var fromTime = model.StartTime;
                var toTime = model.EndTime;
                var id = _localUnitOfWork.DataContext.Timeslots.Any() ? _localUnitOfWork.DataContext.Timeslots.Max(t => t.Timeslot_ID) : 0;

                var morningTimeSpan = new TimeSpan(12, 0, 0);
                var eveningTimeSpan = new TimeSpan(17, 0, 0);
                while (fromTime < toTime)
                {
                    var timeSlot = new BE.TimeslotDetails();

                    timeSlot.Timeslot_ID = (id + 1);
                    timeSlot.DateOfWeek = dayOfWeek;
                    timeSlot.StartTime = fromTime;
                    timeSlot.EndTime = fromTime.Value.Add(TimeSpan.FromMinutes(model.Duration.Value));
                    if (timeSlot.EndTime <= morningTimeSpan)
                    {
                        timeSlot.Category = EnumTimeshift.Morning;
                    }
                    else if (timeSlot.EndTime > eveningTimeSpan)
                    {
                        timeSlot.Category = EnumTimeshift.Evening;
                    }
                    else
                    {
                        timeSlot.Category = EnumTimeshift.Afternoon;
                    }
                    timeSlot.CreatedDate = DateTime.Today;
                    timeSlot.Description = string.Empty;
                    timeSlot.Setting_ID = model.Setting_ID;
                    timeSlot.CreatedBy = createBy;
                    timeSlot.LastUpdatedBy = createBy;
                    timeSlot.LastUpdatedDate = DateTime.Now;



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
            dbTimeslot.Setting_ID = model.Setting_ID;
            dbTimeslot.Category = model.Category;
            dbTimeslot.CreatedBy = model.CreatedBy;
            dbTimeslot.CreatedDate = model.CreatedDate;
            dbTimeslot.Description = model.Description;
            dbTimeslot.LastUpdatedBy = model.LastUpdatedBy;
            dbTimeslot.LastUpdatedDate = model.LastUpdatedDate;

            return dbTimeslot;
        }

        private void SetInfoToSettingBE(BE.SettingBE settingBE, Setting setting)
        {
            settingBE.Setting_ID = setting.Setting_ID;
            settingBE.Status = setting.Status;
            settingBE.WeekNum = setting.WeekNum;
            settingBE.Year = setting.Year;

            settingBE.Mon_Open_Time = setting.Mon_Open_Time;
            settingBE.Mon_Close_Time = setting.Mon_Close_Time;
            settingBE.Mon_Interval = setting.Mon_Interval;
            settingBE.Mon_MaximumNum = setting.Mon_MaximumNum;
            settingBE.Mon_ReservedForSpare = setting.Mon_ReservedForSpare;

            settingBE.Tue_Open_Time = setting.Tue_Open_Time;
            settingBE.Tue_Close_Time = setting.Tue_Close_Time;
            settingBE.Tue_Interval = setting.Tue_Interval;
            settingBE.Tue_MaximumNum = setting.Tue_MaximumNum;
            settingBE.Tue_ReservedForSpare = setting.Tue_ReservedForSpare;

            settingBE.Wed_Open_Time = setting.Wed_Open_Time;
            settingBE.Wed_Close_Time = setting.Wed_Close_Time;
            settingBE.Wed_Interval = setting.Wed_Interval;
            settingBE.Wed_MaximumNum = setting.Wed_MaximumNum;
            settingBE.Wed_ReservedForSpare = setting.Wed_ReservedForSpare;

            settingBE.Thu_Open_Time = setting.Thu_Open_Time;
            settingBE.Thu_Close_Time = setting.Thu_Close_Time;
            settingBE.Thu_Interval = setting.Thu_Interval;
            settingBE.Thu_MaximumNum = setting.Thu_MaximumNum;
            settingBE.Thu_ReservedForSpare = setting.Thu_ReservedForSpare;


            settingBE.Fri_Open_Time = setting.Fri_Open_Time;
            settingBE.Fri_Close_Time = setting.Fri_Close_Time;
            settingBE.Fri_Interval = setting.Fri_Interval;
            settingBE.Fri_MaximumNum = setting.Fri_MaximumNum;
            settingBE.Fri_ReservedForSpare = setting.Fri_ReservedForSpare;

            settingBE.Sat_Open_Time = setting.Sat_Open_Time;
            settingBE.Sat_Close_Time = setting.Sat_Close_Time;
            settingBE.Sat_Interval = setting.Sat_Interval;
            settingBE.Sat_MaximumNum = setting.Sat_MaximumNum;
            settingBE.Sat_ReservedForSpare = setting.Sat_ReservedForSpare;

            settingBE.Sun_Open_Time = setting.Sun_Open_Time;
            settingBE.Sun_Close_Time = setting.Sun_Close_Time;
            settingBE.Sun_Interval = setting.Sun_Interval;
            settingBE.Sun_MaximumNum = setting.Sun_MaximumNum;
            settingBE.Sun_ReservedForSpare = setting.Sun_ReservedForSpare;

            settingBE.Last_Updated_By = setting.Last_Updated_By;
            settingBE.Last_Updated_Date = setting.Last_Updated_Date;
            settingBE.Description = setting.Description;

        }

        private void SetInfoToSettingDB(BE.SettingBE settingBE, Setting setting)
        {
            setting.Mon_Open_Time = settingBE.Mon_Open_Time;
            setting.Mon_Close_Time = settingBE.Mon_Close_Time;
            setting.Mon_Interval = settingBE.Mon_Interval;
            setting.Mon_MaximumNum = settingBE.Mon_MaximumNum;
            setting.Mon_ReservedForSpare = settingBE.Mon_ReservedForSpare;

            setting.Tue_Open_Time = settingBE.Tue_Open_Time;
            setting.Tue_Close_Time = settingBE.Tue_Close_Time;
            setting.Tue_Interval = settingBE.Tue_Interval;
            setting.Tue_MaximumNum = settingBE.Tue_MaximumNum;
            setting.Tue_ReservedForSpare = settingBE.Tue_ReservedForSpare;

            setting.Wed_Open_Time = settingBE.Wed_Open_Time;
            setting.Wed_Close_Time = settingBE.Wed_Close_Time;
            setting.Wed_Interval = settingBE.Wed_Interval;
            setting.Wed_MaximumNum = settingBE.Wed_MaximumNum;
            setting.Wed_ReservedForSpare = settingBE.Wed_ReservedForSpare;

            setting.Thu_Open_Time = settingBE.Thu_Open_Time;
            setting.Thu_Close_Time = settingBE.Thu_Close_Time;
            setting.Thu_Interval = settingBE.Thu_Interval;
            setting.Thu_MaximumNum = settingBE.Thu_MaximumNum;
            setting.Thu_ReservedForSpare = settingBE.Thu_ReservedForSpare;


            setting.Fri_Open_Time = settingBE.Fri_Open_Time;
            setting.Fri_Close_Time = settingBE.Fri_Close_Time;
            setting.Fri_Interval = settingBE.Fri_Interval;
            setting.Fri_MaximumNum = settingBE.Fri_MaximumNum;
            setting.Fri_ReservedForSpare = settingBE.Fri_ReservedForSpare;

            setting.Sat_Open_Time = settingBE.Sat_Open_Time;
            setting.Sat_Close_Time = settingBE.Sat_Close_Time;
            setting.Sat_Interval = settingBE.Sat_Interval;
            setting.Sat_MaximumNum = settingBE.Sat_MaximumNum;
            setting.Sat_ReservedForSpare = settingBE.Sat_ReservedForSpare;

            setting.Sun_Open_Time = settingBE.Sun_Open_Time;
            setting.Sun_Close_Time = settingBE.Sun_Close_Time;
            setting.Sun_Interval = settingBE.Sun_Interval;
            setting.Sun_MaximumNum = settingBE.Sun_MaximumNum;
            setting.Sun_ReservedForSpare = settingBE.Sun_ReservedForSpare;

            setting.Last_Updated_By = settingBE.Last_Updated_By;
            setting.Last_Updated_Date = settingBE.Last_Updated_Date;
            setting.Description = settingBE.Description;
        }

        public BE.SettingModel GetSettings(string status)
        {
            var dbSeting = _localUnitOfWork.DataContext.Settings.Where(s => s.Status.Equals(status, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            var settingBE = new BE.SettingBE();

            if (dbSeting != null)
            {
                SetInfoToSettingBE(settingBE, dbSeting);
            }

            return new BE.SettingBE().ToSettingModel(settingBE);
        }

        public BE.SettingModel GetSettings(DateTime date)
        {
            int dayOfWeek = date.DayOfWeek();
            int year = date.Year;
            int weekNum = date.WeekNum();
            var dbSeting = _localUnitOfWork.DataContext.Settings.Where(s => s.Year == year && s.WeekNum == weekNum).FirstOrDefault();
            var settingBE = new BE.SettingBE();
            SetInfoToSettingBE(settingBE, dbSeting);

            return new BE.SettingBE().ToSettingModel(settingBE);
        }

        public void UpdateTimeslotForNewWeek(string createdBy)
        {
            var timeSlotRepo = _localUnitOfWork.GetRepository<Timeslot>();

            //delete
            var listDbTimeslot = timeSlotRepo.GetAll().ToList();

            foreach (var item in listDbTimeslot)
            {
                timeSlotRepo.Delete(item);
            }

            _localUnitOfWork.Save();
            //add new
            GenerateTimeslots(createdBy);


        }
        public void UpdateSettings(Trinity.BE.SettingBE model, string lastUpdateBy)
        {
            var repo = _localUnitOfWork.GetRepository<Setting>();
            var dbSetting = repo.GetAll().FirstOrDefault();
            SetInfoToSettingDB(model, dbSetting);

            repo.Update(dbSetting);
            _localUnitOfWork.Save();
            UpdateTimeslotForNewWeek(lastUpdateBy);
        }

        public Timeslot GetTimeslot(Guid settingId, DateTime currentDate)
        {
            int dayOfWeek = currentDate.DayOfWeek();

            return _localUnitOfWork.DataContext.Timeslots.Where(t => t.DateOfWeek == dayOfWeek && t.Setting_ID == settingId).FirstOrDefault();
        }

        // Save Setting from DutyOfficer
        public bool SaveSetting (Trinity.BE.SettingModel model, string lastUpdateBy)
        {
            try
            {
                var repo = _localUnitOfWork.GetRepository<Setting>();
                var settingLastest = _localUnitOfWork.DataContext.Settings.OrderBy(s => s.Year).ThenBy(s => s.WeekNum).FirstOrDefault();
                var setting = _localUnitOfWork.DataContext.Settings.FirstOrDefault(s => s.Setting_ID == model.Setting_ID);
                if (setting == null)
                {
                    setting = new Setting();
                    setting.Setting_ID = Guid.NewGuid();
                    setting.Status = EnumSettingStatuses.Pending;
                    setting.Last_Updated_By = lastUpdateBy;
                    setting.Last_Updated_Date = DateTime.Now;

                    if (settingLastest != null)
                    {
                        if (settingLastest.WeekNum == 52)
                        {
                            setting.WeekNum = 1;
                            setting.Year = settingLastest.Year + 1;
                        }
                        else
                        {
                            setting.WeekNum = settingLastest.WeekNum + 1;
                            setting.Year = settingLastest.Year;
                        }
                    }
                    else
                    {
                        setting.WeekNum = DateTime.Now.WeekNum();
                        setting.Year = DateTime.Now.Year;
                    }

                    SetSettingModelToSetting(model, setting);

                    repo.Add(setting);
                }
                else
                {
                    setting.Status = EnumSettingStatuses.Pending;
                    setting.Last_Updated_By = lastUpdateBy;
                    setting.Last_Updated_Date = DateTime.Now;
                    setting.WeekNum = model.WeekNum;
                    setting.Year = model.Year;

                    SetSettingModelToSetting(model, setting);

                    repo.Update(setting);
                }

                _localUnitOfWork.Save();

                // Generate Timeslote here
                GenerateTimeslots(lastUpdateBy);

                return true;
            }
            catch(Exception e)
            {
                return false;
            }
        }

        private void SetSettingModelToSetting(SettingModel model, Setting setting)
        {
            //Monday
            setting.Mon_Open_Time = model.Monday.StartTime;
            setting.Mon_Close_Time = model.Monday.EndTime;
            setting.Mon_Interval = model.Monday.Duration;
            setting.Mon_MaximumNum = model.Monday.MaximumAppointment;
            setting.Mon_ReservedForSpare = model.Monday.ReservedForSpare;

            //Tuesday
            setting.Tue_Open_Time = model.Tuesday.StartTime;
            setting.Tue_Close_Time = model.Tuesday.EndTime;
            setting.Tue_Interval = model.Tuesday.Duration;
            setting.Tue_MaximumNum = model.Tuesday.MaximumAppointment;
            setting.Tue_ReservedForSpare = model.Tuesday.ReservedForSpare;

            //Wednesday
            setting.Wed_Open_Time = model.WednesDay.StartTime;
            setting.Wed_Close_Time = model.WednesDay.EndTime;
            setting.Wed_Interval = model.WednesDay.Duration;
            setting.Wed_MaximumNum = model.WednesDay.MaximumAppointment;
            setting.Wed_ReservedForSpare = model.WednesDay.ReservedForSpare;

            //Thursday
            setting.Thu_Open_Time = model.Thursday.StartTime;
            setting.Thu_Close_Time = model.Thursday.EndTime;
            setting.Thu_Interval = model.Thursday.Duration;
            setting.Thu_MaximumNum = model.Thursday.MaximumAppointment;
            setting.Thu_ReservedForSpare = model.Thursday.ReservedForSpare;

            //Friday
            setting.Fri_Open_Time = model.Friday.StartTime;
            setting.Fri_Close_Time = model.Friday.EndTime;
            setting.Fri_Interval = model.Friday.Duration;
            setting.Fri_MaximumNum = model.Friday.MaximumAppointment;
            setting.Fri_ReservedForSpare = model.Friday.ReservedForSpare;

            //Saturday
            setting.Sat_Open_Time = model.Saturday.StartTime;
            setting.Sat_Close_Time = model.Saturday.EndTime;
            setting.Sat_Interval = model.Saturday.Duration;
            setting.Sat_MaximumNum = model.Saturday.MaximumAppointment;
            setting.Sat_ReservedForSpare = model.Saturday.ReservedForSpare;

            //Sunday
            setting.Sun_Open_Time = model.Sunday.StartTime;
            setting.Sun_Close_Time = model.Sunday.EndTime;
            setting.Sun_Interval = model.Sunday.Duration;
            setting.Sun_MaximumNum = model.Sunday.MaximumAppointment;
            setting.Sun_ReservedForSpare = model.Sunday.ReservedForSpare;
        }

        public SettingDetails GetSettingDetails(EnumDayOfWeek dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case EnumDayOfWeek.Monday:
                    return GetSettings(DateTime.Now).Monday;
                case EnumDayOfWeek.Tuesday:
                    return GetSettings(DateTime.Now).Tuesday;
                case EnumDayOfWeek.Wednesday:
                    return GetSettings(DateTime.Now).WednesDay;
                case EnumDayOfWeek.Thursday:
                    return GetSettings(DateTime.Now).Thursday;
                case EnumDayOfWeek.Friday:
                    return GetSettings(DateTime.Now).Friday;
                case EnumDayOfWeek.Saturday:
                    return GetSettings(DateTime.Now).Saturday;
                case EnumDayOfWeek.Sunday:
                    return GetSettings(DateTime.Now).Sunday;
                default:
                    return null;
            }

        }

        public Timeslot GetNextTimeslotToday(TimeSpan currentTime)
        {
          
            var dateOfWeek = Common.CommonUtil.ConvertToCustomDateOfWeek(DateTime.Now.DayOfWeek);
            var setting = GetSettingDetails(dateOfWeek);
            var nextTimeslot = _localUnitOfWork.DataContext.Timeslots.Where(t => t.StartTime == DbFunctions.AddMinutes(currentTime, setting.Duration) && t.DateOfWeek == (int)dateOfWeek && t.Setting_ID == setting.Setting_ID).FirstOrDefault();
            return nextTimeslot;
        }
    }
}
