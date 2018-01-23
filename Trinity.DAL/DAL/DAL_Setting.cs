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


        public Trinity.BE.WorkingTimeshift GetAppointmentTime(DateTime date)
        {
            var appointmentTime = new Trinity.BE.WorkingTimeshift();
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

        public BE.WorkingShiftDetails SetAppointmentTime(DBContext.Timeslot timeSlot)
        {
            var environmentTime = new BE.WorkingShiftDetails()
            {
                StartTime = timeSlot.StartTime.Value,
                EndTime = timeSlot.EndTime.Value,
                IsAvailble = true,
                IsSelected = false,
                Category = timeSlot.Category
            };
            return environmentTime;
        }

        public Trinity.BE.WorkingTimeshift GetCurrentAppointmentTime()
        {
            var today = DateTime.Now;
            return GetAppointmentTime(today);
        }

        public List<Timeslot> GetTimeslots(DateTime date)
        {
            SettingModel setting = GetSettings();
            //GenerateTimeslots("dfbb2a6a-9e45-4a76-9f75-af1a7824a947");
            int dayOfWeek = date.DayOfWeek();
            return _localUnitOfWork.DataContext.Timeslots.Where(t => DbFunctions.TruncateTime(t.Date) == date.Date).ToList();
        }

        public void GenerateTimeslots(DateTime date, string createdBy)
        {
            // Allow to change pending settings only
            SettingModel settings = GetSettings();
            if (settings == null)
            {
                // Couldn't change timeslot
                return;
            }

            //mon
            GenerateTimeslotAndInsert(date.Date, settings.Monday, createdBy);

            //tue
            GenerateTimeslotAndInsert(date.Date, settings.Tuesday, createdBy);

            //wed
            GenerateTimeslotAndInsert(date.Date, settings.WednesDay, createdBy);

            //thu
            GenerateTimeslotAndInsert(date.Date, settings.Thursday, createdBy);

            //fri
            GenerateTimeslotAndInsert(date.Date, settings.Friday, createdBy);

            //sat
            GenerateTimeslotAndInsert(date.Date, settings.Saturday, createdBy);

            //sun
            GenerateTimeslotAndInsert(date.Date, settings.Sunday, createdBy);

        }

        private void GenerateTimeslotAndInsert(DateTime date, Trinity.BE.SettingDetails model, string createBy)
        {
            if (model.Morning_Open_Time.HasValue && model.Morning_Close_Time.HasValue && model.Morning_Interval.HasValue)
            {
                GenerateByTimeshift(date, model.Morning_Open_Time, model.Morning_Close_Time, model.Morning_Interval, createBy);
            }

            if (model.Afternoon_Open_Time.HasValue && model.Afternoon_Close_Time.HasValue && model.Afternoon_Interval.HasValue)
            {
                GenerateByTimeshift(date, model.Afternoon_Open_Time, model.Afternoon_Close_Time, model.Afternoon_Interval, createBy);
            }

            if (model.Evening_Open_Time.HasValue && model.Evening_Close_Time.HasValue && model.Evening_Interval.HasValue)
            {
                GenerateByTimeshift(date, model.Evening_Open_Time, model.Evening_Close_Time, model.Evening_Interval, createBy);
            }

        }
        private void GenerateByTimeshift(DateTime date, TimeSpan? openTime, TimeSpan? closeTime, int? duration, string createBy)
        {
            var fromTime = openTime;
            var toTime = closeTime;
            var id = _localUnitOfWork.DataContext.Timeslots.Any() ? _localUnitOfWork.DataContext.Timeslots.Max(t => t.Timeslot_ID) : 0;
            while (fromTime < toTime)
            {
                var timeSlot = new BE.TimeslotDetails();

                timeSlot.Timeslot_ID = (id + 1);

                timeSlot.StartTime = fromTime;
                timeSlot.EndTime = fromTime.Value.Add(TimeSpan.FromMinutes(duration.Value));

                timeSlot.Category = EnumTimeshift.Morning;
                timeSlot.Date = date.Date;
                timeSlot.CreatedDate = DateTime.Today;
                timeSlot.Description = string.Empty;

                timeSlot.CreatedBy = createBy;
                timeSlot.LastUpdatedBy = createBy;
                timeSlot.LastUpdatedDate = DateTime.Now;



                fromTime = fromTime.Value.Add(TimeSpan.FromMinutes(duration.Value));

                _localUnitOfWork.GetRepository<Timeslot>().Add(SetInfo(new Timeslot(), timeSlot));
                _localUnitOfWork.Save();
            }
        }

        private Timeslot SetInfo(Timeslot dbTimeslot, BE.TimeslotDetails model)
        {
            dbTimeslot.Timeslot_ID = model.Timeslot_ID;
            dbTimeslot.StartTime = model.StartTime;
            dbTimeslot.EndTime = model.EndTime;
            dbTimeslot.Date = model.Date;
            dbTimeslot.Category = model.Category;
            dbTimeslot.CreatedBy = model.CreatedBy;
            dbTimeslot.CreatedDate = model.CreatedDate;
            dbTimeslot.Description = model.Description;
            dbTimeslot.LastUpdatedBy = model.LastUpdatedBy;
            dbTimeslot.LastUpdatedDate = model.LastUpdatedDate;

            return dbTimeslot;
        }

        private void SetInfoToSettingBE(BE.SettingBE settingBE, OperationSetting setting)
        {

            settingBE.DayOfWeek = setting.DayOfWeek;

            settingBE.Morning_Open_Time = setting.Morning_Open_Time;
            settingBE.Morning_Close_Time = setting.Morning_Close_Time;
            settingBE.Morning_Interval = setting.Morning_Interval;
            settingBE.Morning_MaximumSupervisee = setting.Morning_MaximumSupervisee;
            settingBE.Morning_Spare_Slots = setting.Morning_Spare_Slots;

            settingBE.Afternoon_Open_Time = setting.Morning_Open_Time;
            settingBE.Afternoon_Close_Time = setting.Morning_Close_Time;
            settingBE.Afternoon_Interval = setting.Morning_Interval;
            settingBE.Afternoon_MaximumSupervisee = setting.Morning_MaximumSupervisee;
            settingBE.Afternoon_Spare_Slots = setting.Morning_Spare_Slots;

            settingBE.Evening_Open_Time = setting.Morning_Open_Time;
            settingBE.Evening_Close_Time = setting.Morning_Close_Time;
            settingBE.Evening_Interval = setting.Morning_Interval;
            settingBE.Evening_MaximumSupervisee = setting.Morning_MaximumSupervisee;
            settingBE.Evening_Spare_Slots = setting.Morning_Spare_Slots;


            settingBE.Last_Updated_By = setting.Last_Updated_By;
            settingBE.Last_Updated_Date = setting.Last_Updated_Date;
            settingBE.Description = setting.Description;

        }

        private void SetInfoToSettingDB(BE.SettingBE settingBE, OperationSetting setting)
        {
            setting.DayOfWeek = settingBE.DayOfWeek;

            setting.Morning_Open_Time = settingBE.Morning_Open_Time;
            setting.Morning_Close_Time = settingBE.Morning_Close_Time;
            setting.Morning_Interval = settingBE.Morning_Interval;
            setting.Morning_MaximumSupervisee = settingBE.Morning_MaximumSupervisee;
            setting.Morning_Spare_Slots = settingBE.Morning_Spare_Slots;

            setting.Afternoon_Open_Time = settingBE.Morning_Open_Time;
            setting.Afternoon_Close_Time = settingBE.Morning_Close_Time;
            setting.Afternoon_Interval = settingBE.Morning_Interval;
            setting.Afternoon_MaximumSupervisee = settingBE.Morning_MaximumSupervisee;
            setting.Afternoon_Spare_Slots = settingBE.Morning_Spare_Slots;

            setting.Evening_Open_Time = settingBE.Morning_Open_Time;
            setting.Evening_Close_Time = settingBE.Morning_Close_Time;
            setting.Evening_Interval = settingBE.Morning_Interval;
            setting.Evening_MaximumSupervisee = settingBE.Morning_MaximumSupervisee;
            setting.Evening_Spare_Slots = settingBE.Morning_Spare_Slots;


            setting.Last_Updated_By = settingBE.Last_Updated_By;
            setting.Last_Updated_Date = settingBE.Last_Updated_Date;
            setting.Description = settingBE.Description;
        }

        public BE.SettingModel GetSettings()
        {
            var dbSeting = _localUnitOfWork.DataContext.OperationSettings.FirstOrDefault();
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
            var dbSeting = _localUnitOfWork.DataContext.OperationSettings.FirstOrDefault();
            var settingBE = new BE.SettingBE();
            SetInfoToSettingBE(settingBE, dbSeting);

            return new BE.SettingBE().ToSettingModel(settingBE);
        }

        public void UpdateTimeslots(DateTime date, string createdBy)
        {
            var timeSlotRepo = _localUnitOfWork.GetRepository<Timeslot>();

            //delete
            //var listDbTimeslot = timeSlotRepo.GetAll().ToList();
            var listDbTimeslot = _localUnitOfWork.DataContext.Timeslots.ToList();

            foreach (var item in listDbTimeslot)
            {
                timeSlotRepo.Delete(item);
            }

            _localUnitOfWork.Save();
            //add new
            GenerateTimeslots(date, createdBy);


        }
        public void UpdateSettings(DateTime date, Trinity.BE.SettingBE model, string lastUpdateBy)
        {
            var repo = _localUnitOfWork.GetRepository<OperationSetting>();
            var dbSetting = repo.GetAll().FirstOrDefault();
            SetInfoToSettingDB(model, dbSetting);

            repo.Update(dbSetting);
            _localUnitOfWork.Save();
            UpdateTimeslots(date, lastUpdateBy);
        }

        //public Timeslot GetTimeslot(Guid settingId, DateTime currentDate)
        //{
        //    int dayOfWeek = currentDate.DayOfWeek();

        //    return _localUnitOfWork.DataContext.Timeslots.Where(t => t.DateOfWeek == dayOfWeek && t.Setting_ID == settingId).FirstOrDefault();
        //}

        // Save OperationSetting from DutyOfficer
        public bool SaveSetting(Trinity.BE.SettingDetails model, string lastUpdateBy)
        {
            try
            {
                var repo = _localUnitOfWork.GetRepository<OperationSetting>();
                //  var settingLastest = _localUnitOfWork.DataContext.Settings.OrderBy(s => s.Year).ThenBy(s => s.WeekNum).FirstOrDefault();
                var setting = _localUnitOfWork.DataContext.OperationSettings.FirstOrDefault();
                if (setting == null)
                {
                    setting = new OperationSetting();
                    setting.Last_Updated_By = lastUpdateBy;
                    setting.Last_Updated_Date = DateTime.Now;

                    SetSettingModelToSetting(model, setting);

                    repo.Add(setting);
                }
                else
                {

                    setting.Last_Updated_By = lastUpdateBy;
                    setting.Last_Updated_Date = DateTime.Now;



                    SetSettingModelToSetting(model, setting);

                    repo.Update(setting);
                }

                _localUnitOfWork.Save();

                // Save HoliDays
                SaveHolidays(model.HoliDays);

                // Generate Timeslote here
                UpdateTimeslots(DateTime.Now.Date, lastUpdateBy);

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private void SetSettingModelToSetting(SettingDetails settingBE, OperationSetting setting)
        {
            setting.DayOfWeek = settingBE.DayOfWeek;
            setting.Morning_Open_Time = settingBE.Morning_Open_Time;
            setting.Morning_Close_Time = settingBE.Morning_Close_Time;
            setting.Morning_Interval = settingBE.Morning_Interval;
            setting.Morning_MaximumSupervisee = settingBE.Morning_MaximumSupervisee;
            setting.Morning_Spare_Slots = settingBE.Morning_Spare_Slots;

            setting.Afternoon_Open_Time = settingBE.Morning_Open_Time;
            setting.Afternoon_Close_Time = settingBE.Morning_Close_Time;
            setting.Afternoon_Interval = settingBE.Morning_Interval;
            setting.Afternoon_MaximumSupervisee = settingBE.Morning_MaximumSupervisee;
            setting.Afternoon_Spare_Slots = settingBE.Morning_Spare_Slots;

            setting.Evening_Open_Time = settingBE.Morning_Open_Time;
            setting.Evening_Close_Time = settingBE.Morning_Close_Time;
            setting.Evening_Interval = settingBE.Morning_Interval;
            setting.Evening_MaximumSupervisee = settingBE.Morning_MaximumSupervisee;
            setting.Evening_Spare_Slots = settingBE.Morning_Spare_Slots;


            setting.Last_Updated_By = settingBE.Last_Updated_By;
            setting.Last_Updated_Date = settingBE.Last_Updated_Date;
            setting.Description = settingBE.Description;
        }

        public List<BE.Holiday> GetHolidays()
        {
            List<BE.Holiday> results = new List<BE.Holiday>();
            var repoHolidays = _localUnitOfWork.GetRepository<DBContext.Holiday>();
            var lstHolidays = repoHolidays.GetAll().ToList();

            foreach (var item in lstHolidays)
            {
                BE.Holiday holiday = new BE.Holiday();
                holiday.Holiday1 = item.Holiday1;
                holiday.ShortDesc = item.ShortDesc;
                holiday.Notes = item.Notes;

                results.Add(holiday);
            }

            return results;
        }

        private void SaveHolidays(List<BE.Holiday> lstHolidays)
        {
            try
            {
                var repo = _localUnitOfWork.GetRepository<DBContext.Holiday>();

                //delete all holiday before update and insert from new list
                var listExistHolidays = repo.GetAll().ToList();

                foreach (var item in listExistHolidays)
                {
                    repo.Delete(item);
                }

                foreach (var item in lstHolidays)
                {
                    DBContext.Holiday holiday = new DBContext.Holiday();
                    holiday.Holiday1 = item.Holiday1;
                    holiday.ShortDesc = item.ShortDesc;
                    holiday.Notes = item.Notes;

                    repo.Add(holiday);
                }
                _localUnitOfWork.Save();

            }
            catch (Exception e)
            {
            }
        }

        public SettingDetails GetSettingDetails(EnumDayOfWeek dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case EnumDayOfWeek.Monday:
                    return GetSettings().Monday;
                case EnumDayOfWeek.Tuesday:
                    return GetSettings().Tuesday;
                case EnumDayOfWeek.Wednesday:
                    return GetSettings().WednesDay;
                case EnumDayOfWeek.Thursday:
                    return GetSettings().Thursday;
                case EnumDayOfWeek.Friday:
                    return GetSettings().Friday;
                case EnumDayOfWeek.Saturday:
                    return GetSettings().Saturday;
                case EnumDayOfWeek.Sunday:
                    return GetSettings().Sunday;
                default:
                    return null;
            }

        }

        public Timeslot GetNextTimeslotToday(TimeSpan currentTime)
        {

            var dateOfWeek = Common.CommonUtil.ConvertToCustomDateOfWeek(DateTime.Now.DayOfWeek);
            var setting = GetSettingDetails(dateOfWeek);
            var listTimeslot = _localUnitOfWork.DataContext.Timeslots.Where(t => DbFunctions.TruncateTime(t.Date) == DateTime.Now.Date).ToList();
            var nextIdx = 0;
            foreach (var item in listTimeslot)
            {
                if (item.StartTime==currentTime)
                {
                    nextIdx = listTimeslot.IndexOf(item)+1;
                    break;
                }
            }

            var nextTimeslot = listTimeslot[nextIdx];
            return nextTimeslot;
        }

        public List<Timeslot> GetListTodayTimeslot()
        {
            var dayOfWeek = Common.CommonUtil.ConvertToCustomDateOfWeek(DateTime.Now.DayOfWeek);
            var setting = GetSettingDetails(dayOfWeek);
            var listTimeslot = _localUnitOfWork.DataContext.Timeslots.Where(t => DbFunctions.TruncateTime(t.Date) == DateTime.Now.Date).ToList();
            return listTimeslot;

        }
    }
}
