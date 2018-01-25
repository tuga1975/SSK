﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Windows.Forms;
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

        private void GenerateTimeslotAndInsert(DateTime date, Trinity.BE.SettingDetails model, string createBy, int maxSupervisee = 0)
        {
            if (model.Morning_Open_Time.HasValue && model.Morning_Close_Time.HasValue && model.Morning_Interval.HasValue)
            {
                GenerateByTimeshift(date, model.Morning_Open_Time, model.Morning_Close_Time, model.Morning_Interval, createBy, maxSupervisee);
            }

            if (model.Afternoon_Open_Time.HasValue && model.Afternoon_Close_Time.HasValue && model.Afternoon_Interval.HasValue)
            {
                GenerateByTimeshift(date, model.Afternoon_Open_Time, model.Afternoon_Close_Time, model.Afternoon_Interval, createBy, maxSupervisee);
            }

            if (model.Evening_Open_Time.HasValue && model.Evening_Close_Time.HasValue && model.Evening_Interval.HasValue)
            {
                GenerateByTimeshift(date, model.Evening_Open_Time, model.Evening_Close_Time, model.Evening_Interval, createBy, maxSupervisee);
            }

        }
        private void GenerateByTimeshift(DateTime date, TimeSpan? openTime, TimeSpan? closeTime, int? duration, string createBy, int maxSupervisee = 0)
        {
            var fromTime = openTime;
            var toTime = closeTime;
            var id = _localUnitOfWork.DataContext.Timeslots.Any() ? _localUnitOfWork.DataContext.Timeslots.Max(t => t.Timeslot_ID) : null;
            while (fromTime < toTime)
            {
                var timeSlot = new BE.TimeslotDetails();

                var morningTimeSpan = new TimeSpan(12, 0, 0);
                var eveningTimeSpan = new TimeSpan(17, 0, 0);

                //timeSlot.Timeslot_ID = (id + 1);

                timeSlot.StartTime = fromTime;
                timeSlot.EndTime = fromTime.Value.Add(TimeSpan.FromMinutes(duration.Value));

                //timeSlot.Category = EnumTimeshift.Morning;
                timeSlot.Date = date.Date;
                timeSlot.CreatedDate = DateTime.Today;
                timeSlot.Description = string.Empty;

                timeSlot.CreatedBy = createBy;
                timeSlot.LastUpdatedBy = createBy;
                timeSlot.LastUpdatedDate = DateTime.Now;
                timeSlot.MaximumSupervisee = maxSupervisee;


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
            dbTimeslot.MaximumSupervisee = model.MaximumSupervisee;

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
        public BE.SettingModel GetOperationSettings()
        {

            List<OperationSetting> arraySetting = _localUnitOfWork.DataContext.OperationSettings.ToList();


            var settingModel = new BE.SettingModel
            {
                Monday = arraySetting.FirstOrDefault(d => d.DayOfWeek == (int)EnumDayOfWeek.Monday).Map<BE.SettingDetails>(),
                Tuesday = arraySetting.FirstOrDefault(d => d.DayOfWeek == (int)EnumDayOfWeek.Tuesday).Map<BE.SettingDetails>(),
                WednesDay = arraySetting.FirstOrDefault(d => d.DayOfWeek == (int)EnumDayOfWeek.Wednesday).Map<BE.SettingDetails>(),
                Thursday = arraySetting.FirstOrDefault(d => d.DayOfWeek == (int)EnumDayOfWeek.Thursday).Map<BE.SettingDetails>(),
                Friday = arraySetting.FirstOrDefault(d => d.DayOfWeek == (int)EnumDayOfWeek.Friday).Map<BE.SettingDetails>(),
                Saturday = arraySetting.FirstOrDefault(d => d.DayOfWeek == (int)EnumDayOfWeek.Saturday).Map<BE.SettingDetails>(),
                Sunday = arraySetting.FirstOrDefault(d => d.DayOfWeek == (int)EnumDayOfWeek.Sunday).Map<BE.SettingDetails>(),
                HoliDays = GetHolidays()
            };

            settingModel.Monday = settingModel.Monday == null ? new SettingDetails() { DayOfWeek = (int)EnumDayOfWeek.Monday } : settingModel.Monday;
            settingModel.Tuesday = settingModel.Tuesday == null ? new SettingDetails() { DayOfWeek = (int)EnumDayOfWeek.Tuesday } : settingModel.Tuesday;
            settingModel.WednesDay = settingModel.WednesDay == null ? new SettingDetails() { DayOfWeek = (int)EnumDayOfWeek.Wednesday } : settingModel.WednesDay;
            settingModel.Thursday = settingModel.Thursday == null ? new SettingDetails() { DayOfWeek = (int)EnumDayOfWeek.Thursday } : settingModel.Thursday;
            settingModel.Friday = settingModel.Friday == null ? new SettingDetails() { DayOfWeek = (int)EnumDayOfWeek.Friday } : settingModel.Friday;
            settingModel.Saturday = settingModel.Saturday == null ? new SettingDetails() { DayOfWeek = (int)EnumDayOfWeek.Saturday } : settingModel.Saturday;
            settingModel.Sunday = settingModel.Sunday == null ? new SettingDetails() { DayOfWeek = (int)EnumDayOfWeek.Sunday } : settingModel.Sunday;

            //OperationSetting settingMonday = _localUnitOfWork.DataContext.OperationSettings.FirstOrDefault(s => s.DayOfWeek == 2);
            //OperationSetting settingTuesday = _localUnitOfWork.DataContext.OperationSettings.FirstOrDefault(s => s.DayOfWeek == 3);
            //OperationSetting settingWednesday = _localUnitOfWork.DataContext.OperationSettings.FirstOrDefault(s => s.DayOfWeek == 4);
            //OperationSetting settingThurday = _localUnitOfWork.DataContext.OperationSettings.FirstOrDefault(s => s.DayOfWeek == 5);
            //OperationSetting settingFriday = _localUnitOfWork.DataContext.OperationSettings.FirstOrDefault(s => s.DayOfWeek == 6);
            //OperationSetting settingSaturday = _localUnitOfWork.DataContext.OperationSettings.FirstOrDefault(s => s.DayOfWeek == 7);
            //OperationSetting settingSunday = _localUnitOfWork.DataContext.OperationSettings.FirstOrDefault(s => s.DayOfWeek == 8);

            //if (settingMonday != null)
            //{
            //    settingModel.Monday.Morning_Open_Time = settingMonday.Morning_Open_Time;
            //    settingModel.Monday.Morning_Close_Time = settingMonday.Morning_Close_Time;
            //    settingModel.Monday.Morning_Interval = settingMonday.Morning_Interval;
            //    settingModel.Monday.Morning_Is_Closed = settingMonday.Morning_Is_Closed;
            //    settingModel.Monday.Morning_MaximumSupervisee = settingMonday.Morning_MaximumSupervisee;
            //    settingModel.Monday.Morning_Spare_Slots = settingMonday.Morning_Spare_Slots;

            //    settingModel.Monday.Afternoon_Open_Time = settingMonday.Afternoon_Open_Time;
            //    settingModel.Monday.Afternoon_Close_Time = settingMonday.Afternoon_Close_Time;
            //    settingModel.Monday.Afternoon_Interval = settingMonday.Afternoon_Interval;
            //    settingModel.Monday.Afternoon_Is_Closed = settingMonday.Afternoon_Is_Closed;
            //    settingModel.Monday.Afternoon_MaximumSupervisee = settingMonday.Afternoon_MaximumSupervisee;
            //    settingModel.Monday.Afternoon_Spare_Slots = settingMonday.Afternoon_Spare_Slots;

            //    settingModel.Monday.Evening_Open_Time = settingMonday.Evening_Open_Time;
            //    settingModel.Monday.Evening_Close_Time = settingMonday.Evening_Close_Time;
            //    settingModel.Monday.Evening_Interval = settingMonday.Evening_Interval;
            //    settingModel.Monday.Evening_Is_Closed = settingMonday.Evening_Is_Closed;
            //    settingModel.Monday.Evening_MaximumSupervisee = settingMonday.Evening_MaximumSupervisee;
            //    settingModel.Monday.Evening_Spare_Slots = settingMonday.Evening_Spare_Slots;
            //}

            //if (settingTuesday != null)
            //{
            //    settingModel.Tuesday.Morning_Open_Time = settingTuesday.Morning_Open_Time;
            //    settingModel.Tuesday.Morning_Close_Time = settingTuesday.Morning_Close_Time;
            //    settingModel.Tuesday.Morning_Interval = settingTuesday.Morning_Interval;
            //    settingModel.Tuesday.Morning_Is_Closed = settingTuesday.Morning_Is_Closed;
            //    settingModel.Tuesday.Morning_MaximumSupervisee = settingTuesday.Morning_MaximumSupervisee;
            //    settingModel.Tuesday.Morning_Spare_Slots = settingTuesday.Morning_Spare_Slots;

            //    settingModel.Tuesday.Afternoon_Open_Time = settingTuesday.Afternoon_Open_Time;
            //    settingModel.Tuesday.Afternoon_Close_Time = settingMonday.Afternoon_Close_Time;
            //    settingModel.Tuesday.Afternoon_Interval = settingTuesday.Afternoon_Interval;
            //    settingModel.Tuesday.Afternoon_Is_Closed = settingTuesday.Afternoon_Is_Closed;
            //    settingModel.Tuesday.Afternoon_MaximumSupervisee = settingTuesday.Afternoon_MaximumSupervisee;
            //    settingModel.Tuesday.Afternoon_Spare_Slots = settingTuesday.Afternoon_Spare_Slots;

            //    settingModel.Tuesday.Evening_Open_Time = settingTuesday.Evening_Open_Time;
            //    settingModel.Tuesday.Evening_Close_Time = settingTuesday.Evening_Close_Time;
            //    settingModel.Tuesday.Evening_Interval = settingTuesday.Evening_Interval;
            //    settingModel.Tuesday.Evening_Is_Closed = settingTuesday.Evening_Is_Closed;
            //    settingModel.Tuesday.Evening_MaximumSupervisee = settingTuesday.Evening_MaximumSupervisee;
            //    settingModel.Tuesday.Evening_Spare_Slots = settingTuesday.Evening_Spare_Slots;
            //}

            //if (settingWednesday != null)
            //{
            //    settingModel.WednesDay.Morning_Open_Time = settingWednesday.Morning_Open_Time;
            //    settingModel.WednesDay.Morning_Close_Time = settingWednesday.Morning_Close_Time;
            //    settingModel.WednesDay.Morning_Interval = settingWednesday.Morning_Interval;
            //    settingModel.WednesDay.Morning_Is_Closed = settingWednesday.Morning_Is_Closed;
            //    settingModel.WednesDay.Morning_MaximumSupervisee = settingWednesday.Morning_MaximumSupervisee;
            //    settingModel.WednesDay.Morning_Spare_Slots = settingWednesday.Morning_Spare_Slots;

            //    settingModel.WednesDay.Afternoon_Open_Time = settingWednesday.Afternoon_Open_Time;
            //    settingModel.WednesDay.Afternoon_Close_Time = settingWednesday.Afternoon_Close_Time;
            //    settingModel.WednesDay.Afternoon_Interval = settingWednesday.Afternoon_Interval;
            //    settingModel.WednesDay.Afternoon_Is_Closed = settingWednesday.Afternoon_Is_Closed;
            //    settingModel.WednesDay.Afternoon_MaximumSupervisee = settingWednesday.Afternoon_MaximumSupervisee;
            //    settingModel.WednesDay.Afternoon_Spare_Slots = settingWednesday.Afternoon_Spare_Slots;

            //    settingModel.WednesDay.Evening_Open_Time = settingWednesday.Evening_Open_Time;
            //    settingModel.WednesDay.Evening_Close_Time = settingWednesday.Evening_Close_Time;
            //    settingModel.WednesDay.Evening_Interval = settingWednesday.Evening_Interval;
            //    settingModel.WednesDay.Evening_Is_Closed = settingWednesday.Evening_Is_Closed;
            //    settingModel.WednesDay.Evening_MaximumSupervisee = settingWednesday.Evening_MaximumSupervisee;
            //    settingModel.WednesDay.Evening_Spare_Slots = settingWednesday.Evening_Spare_Slots;
            //}

            //if (settingThurday != null)
            //{
            //    settingModel.Thursday.Morning_Open_Time = settingThurday.Morning_Open_Time;
            //    settingModel.Thursday.Morning_Close_Time = settingThurday.Morning_Close_Time;
            //    settingModel.Thursday.Morning_Interval = settingThurday.Morning_Interval;
            //    settingModel.Thursday.Morning_Is_Closed = settingThurday.Morning_Is_Closed;
            //    settingModel.Thursday.Morning_MaximumSupervisee = settingThurday.Morning_MaximumSupervisee;
            //    settingModel.Thursday.Morning_Spare_Slots = settingThurday.Morning_Spare_Slots;

            //    settingModel.Thursday.Afternoon_Open_Time = settingThurday.Afternoon_Open_Time;
            //    settingModel.Thursday.Afternoon_Close_Time = settingThurday.Afternoon_Close_Time;
            //    settingModel.Thursday.Afternoon_Interval = settingThurday.Afternoon_Interval;
            //    settingModel.Thursday.Afternoon_Is_Closed = settingThurday.Afternoon_Is_Closed;
            //    settingModel.Thursday.Afternoon_MaximumSupervisee = settingThurday.Afternoon_MaximumSupervisee;
            //    settingModel.Thursday.Afternoon_Spare_Slots = settingThurday.Afternoon_Spare_Slots;

            //    settingModel.Thursday.Evening_Open_Time = settingThurday.Evening_Open_Time;
            //    settingModel.Thursday.Evening_Close_Time = settingThurday.Evening_Close_Time;
            //    settingModel.Thursday.Evening_Interval = settingThurday.Evening_Interval;
            //    settingModel.Thursday.Evening_Is_Closed = settingThurday.Evening_Is_Closed;
            //    settingModel.Thursday.Evening_MaximumSupervisee = settingThurday.Evening_MaximumSupervisee;
            //    settingModel.Thursday.Evening_Spare_Slots = settingThurday.Evening_Spare_Slots;
            //}

            //if (settingFriday != null)
            //{
            //    settingModel.Friday.Morning_Open_Time = settingFriday.Morning_Open_Time;
            //    settingModel.Friday.Morning_Close_Time = settingFriday.Morning_Close_Time;
            //    settingModel.Friday.Morning_Interval = settingFriday.Morning_Interval;
            //    settingModel.Friday.Morning_Is_Closed = settingFriday.Morning_Is_Closed;
            //    settingModel.Friday.Morning_MaximumSupervisee = settingFriday.Morning_MaximumSupervisee;
            //    settingModel.Friday.Morning_Spare_Slots = settingFriday.Morning_Spare_Slots;

            //    settingModel.Friday.Afternoon_Open_Time = settingFriday.Afternoon_Open_Time;
            //    settingModel.Friday.Afternoon_Close_Time = settingFriday.Afternoon_Close_Time;
            //    settingModel.Friday.Afternoon_Interval = settingFriday.Afternoon_Interval;
            //    settingModel.Friday.Afternoon_Is_Closed = settingFriday.Afternoon_Is_Closed;
            //    settingModel.Friday.Afternoon_MaximumSupervisee = settingFriday.Afternoon_MaximumSupervisee;
            //    settingModel.Friday.Afternoon_Spare_Slots = settingFriday.Afternoon_Spare_Slots;

            //    settingModel.Friday.Evening_Open_Time = settingFriday.Evening_Open_Time;
            //    settingModel.Friday.Evening_Close_Time = settingFriday.Evening_Close_Time;
            //    settingModel.Friday.Evening_Interval = settingFriday.Evening_Interval;
            //    settingModel.Friday.Evening_Is_Closed = settingFriday.Evening_Is_Closed;
            //    settingModel.Friday.Evening_MaximumSupervisee = settingFriday.Evening_MaximumSupervisee;
            //    settingModel.Friday.Evening_Spare_Slots = settingFriday.Evening_Spare_Slots;
            //}

            //if (settingSaturday != null)
            //{
            //    settingModel.Saturday.Morning_Open_Time = settingSaturday.Morning_Open_Time;
            //    settingModel.Saturday.Morning_Close_Time = settingSaturday.Morning_Close_Time;
            //    settingModel.Saturday.Morning_Interval = settingSaturday.Morning_Interval;
            //    settingModel.Saturday.Morning_Is_Closed = settingSaturday.Morning_Is_Closed;
            //    settingModel.Saturday.Morning_MaximumSupervisee = settingSaturday.Morning_MaximumSupervisee;
            //    settingModel.Saturday.Morning_Spare_Slots = settingSaturday.Morning_Spare_Slots;

            //    settingModel.Saturday.Afternoon_Open_Time = settingSaturday.Afternoon_Open_Time;
            //    settingModel.Saturday.Afternoon_Close_Time = settingSaturday.Afternoon_Close_Time;
            //    settingModel.Saturday.Afternoon_Interval = settingSaturday.Afternoon_Interval;
            //    settingModel.Saturday.Afternoon_Is_Closed = settingSaturday.Afternoon_Is_Closed;
            //    settingModel.Saturday.Afternoon_MaximumSupervisee = settingSaturday.Afternoon_MaximumSupervisee;
            //    settingModel.Saturday.Afternoon_Spare_Slots = settingSaturday.Afternoon_Spare_Slots;

            //    settingModel.Saturday.Evening_Open_Time = settingSaturday.Evening_Open_Time;
            //    settingModel.Saturday.Evening_Close_Time = settingSaturday.Evening_Close_Time;
            //    settingModel.Saturday.Evening_Interval = settingSaturday.Evening_Interval;
            //    settingModel.Saturday.Evening_Is_Closed = settingSaturday.Evening_Is_Closed;
            //    settingModel.Saturday.Evening_MaximumSupervisee = settingSaturday.Evening_MaximumSupervisee;
            //    settingModel.Saturday.Evening_Spare_Slots = settingSaturday.Evening_Spare_Slots;
            //}

            //if (settingSunday != null)
            //{
            //    settingModel.Sunday.Morning_Open_Time = settingSunday.Morning_Open_Time;
            //    settingModel.Sunday.Morning_Close_Time = settingSunday.Morning_Close_Time;
            //    settingModel.Sunday.Morning_Interval = settingSunday.Morning_Interval;
            //    settingModel.Sunday.Morning_Is_Closed = settingSunday.Morning_Is_Closed;
            //    settingModel.Sunday.Morning_MaximumSupervisee = settingSunday.Morning_MaximumSupervisee;
            //    settingModel.Sunday.Morning_Spare_Slots = settingSunday.Morning_Spare_Slots;

            //    settingModel.Sunday.Afternoon_Open_Time = settingSunday.Afternoon_Open_Time;
            //    settingModel.Sunday.Afternoon_Close_Time = settingSunday.Afternoon_Close_Time;
            //    settingModel.Sunday.Afternoon_Interval = settingSunday.Afternoon_Interval;
            //    settingModel.Sunday.Afternoon_Is_Closed = settingSunday.Afternoon_Is_Closed;
            //    settingModel.Sunday.Afternoon_MaximumSupervisee = settingSunday.Afternoon_MaximumSupervisee;
            //    settingModel.Sunday.Afternoon_Spare_Slots = settingSunday.Afternoon_Spare_Slots;

            //    settingModel.Sunday.Evening_Open_Time = settingSunday.Evening_Open_Time;
            //    settingModel.Sunday.Evening_Close_Time = settingSunday.Evening_Close_Time;
            //    settingModel.Sunday.Evening_Interval = settingSunday.Evening_Interval;
            //    settingModel.Sunday.Evening_Is_Closed = settingSunday.Evening_Is_Closed;
            //    settingModel.Sunday.Evening_MaximumSupervisee = settingSunday.Evening_MaximumSupervisee;
            //    settingModel.Sunday.Evening_Spare_Slots = settingSunday.Evening_Spare_Slots;
            //}

            return settingModel;
        }

        #region Duty Officer
        // Save Setting from DutyOfficer
        //public bool SaveOperationSetting(BE.SettingDetails model)
        //{
        //    try
        //    {
        //        var repo = _localUnitOfWork.GetRepository<OperationSetting>();
        //        var repoAppointment = _localUnitOfWork.GetRepository<DBContext.Appointment>();
        //        var repoTimeslot = _localUnitOfWork.GetRepository<DBContext.Timeslot>();

        //        var operationSetting = _localUnitOfWork.DataContext.OperationSettings.FirstOrDefault(s => s.DayOfWeek == model.DayOfWeek);
        //        if (operationSetting == null)
        //        {
        //            operationSetting = new OperationSetting();
        //            operationSetting.DayOfWeek = model.DayOfWeek;
        //            operationSetting.Morning_Open_Time = model.Morning_Open_Time;
        //            operationSetting.Morning_Close_Time = model.Morning_Close_Time;
        //            operationSetting.Morning_Spare_Slots = model.Morning_Spare_Slots;
        //            operationSetting.Morning_Interval = model.Morning_Interval;
        //            operationSetting.Morning_MaximumSupervisee = model.Morning_MaximumSupervisee;
        //            operationSetting.Morning_Is_Closed = model.Morning_Is_Closed;
        //            operationSetting.Afternoon_Open_Time = model.Afternoon_Open_Time;
        //            operationSetting.Afternoon_Close_Time = model.Afternoon_Close_Time;
        //            operationSetting.Afternoon_Spare_Slots = model.Afternoon_Spare_Slots;
        //            operationSetting.Afternoon_Interval = model.Afternoon_Interval;
        //            operationSetting.Afternoon_MaximumSupervisee = model.Afternoon_MaximumSupervisee;
        //            operationSetting.Afternoon_Is_Closed = model.Afternoon_Is_Closed;
        //            operationSetting.Evening_Open_Time = model.Evening_Open_Time;
        //            operationSetting.Evening_Close_Time = model.Evening_Close_Time;
        //            operationSetting.Evening_Spare_Slots = model.Evening_Spare_Slots;
        //            operationSetting.Evening_Interval = model.Evening_Interval;
        //            operationSetting.Evening_MaximumSupervisee = model.Evening_MaximumSupervisee;
        //            operationSetting.Evening_Is_Closed = model.Evening_Is_Closed;
        //            operationSetting.Last_Updated_By = model.Last_Updated_By;
        //            operationSetting.Last_Updated_Date = DateTime.Now;

        //            repo.Add(operationSetting);
        //        }
        //        else
        //        {
        //            operationSetting.Morning_Open_Time = model.Morning_Open_Time;
        //            operationSetting.Morning_Close_Time = model.Morning_Close_Time;
        //            operationSetting.Morning_Spare_Slots = model.Morning_Spare_Slots;
        //            operationSetting.Morning_Interval = model.Morning_Interval;
        //            operationSetting.Morning_MaximumSupervisee = model.Morning_MaximumSupervisee;
        //            operationSetting.Morning_Is_Closed = model.Morning_Is_Closed;
        //            operationSetting.Afternoon_Open_Time = model.Afternoon_Open_Time;
        //            operationSetting.Afternoon_Close_Time = model.Afternoon_Close_Time;
        //            operationSetting.Afternoon_Spare_Slots = model.Afternoon_Spare_Slots;
        //            operationSetting.Afternoon_Interval = model.Afternoon_Interval;
        //            operationSetting.Afternoon_MaximumSupervisee = model.Afternoon_MaximumSupervisee;
        //            operationSetting.Afternoon_Is_Closed = model.Afternoon_Is_Closed;
        //            operationSetting.Evening_Open_Time = model.Evening_Open_Time;
        //            operationSetting.Evening_Close_Time = model.Evening_Close_Time;
        //            operationSetting.Evening_Spare_Slots = model.Evening_Spare_Slots;
        //            operationSetting.Evening_Interval = model.Evening_Interval;
        //            operationSetting.Evening_MaximumSupervisee = model.Evening_MaximumSupervisee;
        //            operationSetting.Evening_Is_Closed = model.Evening_Is_Closed;
        //            operationSetting.Last_Updated_By = model.Last_Updated_By;
        //            operationSetting.Last_Updated_Date = DateTime.Now;

        //            repo.Update(operationSetting);
        //        }

        //        // Generate Timeslote here
        //        var date = FindNexDateByDayOfWeek((EnumDayOfWeek)model.DayOfWeek);
        //        var lstTimeslots = GetTimeslotByDate(date);
        //        if (lstTimeslots.Count > 0)
        //        {
        //            bool isDeleteAppointment = false;
        //            bool isHasQueue = false;

        //            // Check Queue
        //            foreach (var item in lstTimeslots)
        //            {
        //                DBContext.Appointment appointment = GetAppointmentByTimeslotID(item.Timeslot_ID);
        //                if (appointment != null)
        //                {
        //                    // If have Queue, show message and no change update
        //                    if (GetQueueByAppointmentID(appointment.ID) != null)
        //                    {
        //                        MessageBox.Show("The day was order Queue, can not update.");
        //                        isHasQueue = true;
        //                        return false;
        //                    }
        //                }
        //            }

        //            if (!isHasQueue)
        //            {
        //                // Check Appointment
        //                foreach (var item in lstTimeslots)
        //                {
        //                    DBContext.Appointment appointment = GetAppointmentByTimeslotID(item.Timeslot_ID);
        //                    if (appointment != null)
        //                    {
        //                        DialogResult dr = MessageBox.Show("The day was booked appointment, do you want to continue?", "Confirm", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information);

        //                        if (dr == DialogResult.Yes)
        //                        {
        //                            isDeleteAppointment = true;
        //                        }
        //                        break;
        //                    }
        //                }

        //                foreach (var item in lstTimeslots)
        //                {
        //                    DBContext.Appointment appointment = GetAppointmentByTimeslotID(item.Timeslot_ID);
        //                    if (appointment != null)
        //                    {
        //                        if (!isDeleteAppointment)
        //                        {
        //                            return false;
        //                        }
        //                        else
        //                        {
        //                            appointment.Timeslot_ID = null;
        //                            repoAppointment.Update(appointment);
        //                        }
        //                    }
        //                    repoTimeslot.Delete(item);
        //                }

        //                int maxSupervisee = (model.Morning_MaximumSupervisee.HasValue ? model.Morning_MaximumSupervisee.Value : 0)
        //                                + (model.Afternoon_MaximumSupervisee.HasValue ? model.Afternoon_MaximumSupervisee.Value : 0)
        //                                + (model.Evening_MaximumSupervisee.HasValue ? model.Evening_MaximumSupervisee.Value : 0);

        //                GenerateTimeslotAndInsert(date, model, model.Last_Updated_By, maxSupervisee);
        //            }
        //        }
        //        else
        //        {
        //            int maxSupervisee = (model.Morning_MaximumSupervisee.HasValue ? model.Morning_MaximumSupervisee.Value : 0)
        //                                + (model.Afternoon_MaximumSupervisee.HasValue ? model.Afternoon_MaximumSupervisee.Value : 0)
        //                                + (model.Evening_MaximumSupervisee.HasValue ? model.Evening_MaximumSupervisee.Value : 0);
        //            GenerateTimeslotAndInsert(date, model, model.Last_Updated_By, maxSupervisee);
        //        }

        //        _localUnitOfWork.Save();

        //        return true;
        //    }
        //    catch (Exception e)
        //    {
        //        return false;
        //    }
        //}

        private List<Timeslot> GetTimeslotByDate(DateTime date)
        {
            return _localUnitOfWork.DataContext.Timeslots.Where(t => DbFunctions.TruncateTime(t.Date) == date.Date).ToList();
        }

        private DateTime FindNexDateByDayOfWeek(EnumDayOfWeek dayOfWeek)
        {
            int currentDayOfWeek = (int)Common.CommonUtil.ConvertToCustomDateOfWeek(DateTime.Now.DayOfWeek);
            int daysToAdd = ((int)dayOfWeek - currentDayOfWeek + 7) % 7;
            return DateTime.Today.AddDays(daysToAdd);
        }

        //private DBContext.Appointment GetAppointmentByTimeslotID(int timeslotID)
        //{
        //    return _localUnitOfWork.DataContext.Appointments.FirstOrDefault(a => a.Timeslot_ID == timeslotID);
        //}

        private DBContext.Queue GetQueueByAppointmentID(Guid appointmentID)
        {
            return _localUnitOfWork.DataContext.Queues.FirstOrDefault(q => q.Appointment_ID == appointmentID);
        }

        public List<BE.Holiday> GetHolidays()
        {
            //List<BE.Holiday> results = new List<BE.Holiday>();
            var repoHolidays = _localUnitOfWork.GetRepository<DBContext.Holiday>();
            List<BE.Holiday> results = repoHolidays.GetAll().ToList().Select(d => d.Map<BE.Holiday>()).ToList();

            //foreach (var item in lstHolidays)
            //{
            //    BE.Holiday holiday = new BE.Holiday();
            //    holiday.Holiday1 = item.Holiday1;
            //    //holiday.IsSingHoliday = item.IsSingHoliday;
            //    //holiday.IsMalayHoliday = item.IsMalayHoliday;
            //    holiday.ShortDesc = item.ShortDesc;
            //    holiday.Notes = item.Notes;

            //    results.Add(holiday);
            //}

            return results;
        }

        public void AddHoliday(DBContext.Holiday holiday)
        {
            try
            {
                _localUnitOfWork.GetRepository<DBContext.Holiday>().Add(holiday);
                _localUnitOfWork.Save();
            }
            catch (Exception e)
            {
            }
        }

        public void DeleteHoliday(DateTime date)
        {
            try
            {
                _localUnitOfWork.GetRepository<DBContext.Holiday>().Delete(h => h.Holiday1.Year == date.Year && h.Holiday1.Month == date.Month && h.Holiday1.Day == date.Day);
                _localUnitOfWork.Save();
            }
            catch (Exception e)
            {
            }
        }
        #endregion

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
                if (item.StartTime == currentTime)
                {
                    nextIdx = listTimeslot.IndexOf(item) + 1;
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
            var _dayOfWeek = dayOfWeek == EnumDayOfWeek.Sunday ? 1 : (int)dayOfWeek;

            var listTimeslot = _localUnitOfWork.DataContext.Timeslots.Where(t => SqlFunctions.DatePart("dw", t.Date.Value) == _dayOfWeek).OrderBy(d => d.StartTime).ToList();
            return listTimeslot;

        }

        #region Setting & TimeSlot
        public void UpdateSettingAndTimeSlot(CheckWarningSaveSetting modelWarning, BE.SettingDetails model)
        {
            #region Update OperationSetting
            var operationSetting = _localUnitOfWork.DataContext.OperationSettings.FirstOrDefault(s => s.DayOfWeek == model.DayOfWeek);
            List<string> arrayUpdateHistory = new List<string>();
            var sourceProps = model.GetType().GetProperties().Where(x => x.CanRead).ToList();
            if (operationSetting == null)
            {
                foreach (var sourceProp in sourceProps)
                {
                    var dataUpdate = sourceProp.GetValue(model, null);
                    CustomAttribute cusAttr = sourceProp.GetMyCustomAttributes();
                    if (cusAttr != null && cusAttr.Name != null && dataUpdate != null)
                    {
                        arrayUpdateHistory.Add("Changed " + cusAttr.Name + " to " + (dataUpdate == null ? string.Empty : dataUpdate.ToString()));
                    }
                }
            }
            else
            {
                var destProps = operationSetting.GetType().GetProperties()
                    .Where(x => x.CanWrite)
                    .ToList();
                foreach (var sourceProp in sourceProps)
                {
                    var dataUpdate = sourceProp.GetValue(model, null);
                    CustomAttribute cusAttr = sourceProp.GetMyCustomAttributes();
                    if (cusAttr != null && cusAttr.Name != null && dataUpdate != null)
                    {
                        if (destProps.Any(x => x.Name == sourceProp.Name))
                        {
                            var p = destProps.First(x => x.Name == sourceProp.Name);
                            var dataValue = p.GetValue(operationSetting, null);
                            if (!dataUpdate.Equals(dataValue))
                                arrayUpdateHistory.Add("Changed " + cusAttr.Name + " to " + (dataUpdate==null?string.Empty: dataUpdate.ToString()));
                        }

                    }
                }
            }

            ////////////////



            if (operationSetting == null)
            {
                operationSetting = new OperationSetting();
                operationSetting.DayOfWeek = model.DayOfWeek;
                operationSetting.Morning_Open_Time = model.Morning_Open_Time;
                operationSetting.Morning_Close_Time = model.Morning_Close_Time;
                operationSetting.Morning_Spare_Slots = model.Morning_Spare_Slots;
                operationSetting.Morning_Interval = model.Morning_Interval;
                operationSetting.Morning_MaximumSupervisee = model.Morning_MaximumSupervisee;
                operationSetting.Morning_Is_Closed = model.Morning_Is_Closed;
                operationSetting.Afternoon_Open_Time = model.Afternoon_Open_Time;
                operationSetting.Afternoon_Close_Time = model.Afternoon_Close_Time;
                operationSetting.Afternoon_Spare_Slots = model.Afternoon_Spare_Slots;
                operationSetting.Afternoon_Interval = model.Afternoon_Interval;
                operationSetting.Afternoon_MaximumSupervisee = model.Afternoon_MaximumSupervisee;
                operationSetting.Afternoon_Is_Closed = model.Afternoon_Is_Closed;
                operationSetting.Evening_Open_Time = model.Evening_Open_Time;
                operationSetting.Evening_Close_Time = model.Evening_Close_Time;
                operationSetting.Evening_Spare_Slots = model.Evening_Spare_Slots;
                operationSetting.Evening_Interval = model.Evening_Interval;
                operationSetting.Evening_MaximumSupervisee = model.Evening_MaximumSupervisee;
                operationSetting.Evening_Is_Closed = model.Evening_Is_Closed;
                operationSetting.Last_Updated_By = model.Last_Updated_By;
                operationSetting.Last_Updated_Date = DateTime.Now;
                _localUnitOfWork.GetRepository<OperationSetting>().Add(operationSetting);
            }
            else
            {
                operationSetting.Morning_Open_Time = model.Morning_Open_Time;
                operationSetting.Morning_Close_Time = model.Morning_Close_Time;
                operationSetting.Morning_Spare_Slots = model.Morning_Spare_Slots;
                operationSetting.Morning_Interval = model.Morning_Interval;
                operationSetting.Morning_MaximumSupervisee = model.Morning_MaximumSupervisee;
                operationSetting.Morning_Is_Closed = model.Morning_Is_Closed;
                operationSetting.Afternoon_Open_Time = model.Afternoon_Open_Time;
                operationSetting.Afternoon_Close_Time = model.Afternoon_Close_Time;
                operationSetting.Afternoon_Spare_Slots = model.Afternoon_Spare_Slots;
                operationSetting.Afternoon_Interval = model.Afternoon_Interval;
                operationSetting.Afternoon_MaximumSupervisee = model.Afternoon_MaximumSupervisee;
                operationSetting.Afternoon_Is_Closed = model.Afternoon_Is_Closed;
                operationSetting.Evening_Open_Time = model.Evening_Open_Time;
                operationSetting.Evening_Close_Time = model.Evening_Close_Time;
                operationSetting.Evening_Spare_Slots = model.Evening_Spare_Slots;
                operationSetting.Evening_Interval = model.Evening_Interval;
                operationSetting.Evening_MaximumSupervisee = model.Evening_MaximumSupervisee;
                operationSetting.Evening_Is_Closed = model.Evening_Is_Closed;
                operationSetting.Last_Updated_By = model.Last_Updated_By;
                operationSetting.Last_Updated_Date = DateTime.Now;

                _localUnitOfWork.GetRepository<OperationSetting>().Update(operationSetting);
            }
            #endregion
            #region update appointments & remove queue
            if (modelWarning != null && modelWarning.arrayDetail.Count > 0)
            {
                List<Guid> arrAppointmentID = modelWarning.arrayDetail.Select(d => d.AppointmentID).ToList();
                List<DBContext.Appointment> arrAppointmentUpdate = _localUnitOfWork.DataContext.Appointments.Where(d => arrAppointmentID.Contains(d.ID)).ToList();
                arrAppointmentUpdate.ForEach(item =>
                {
                    item.Timeslot_ID = null;
                    _localUnitOfWork.GetRepository<DBContext.Appointment>().Update(item);
                });
                List<Guid> arrQueueID = modelWarning.arrayDetail.Where(d => d.Queue_ID.HasValue).Select(d => d.Queue_ID.Value).ToList();
                _localUnitOfWork.DataContext.QueueDetails.Where(d => arrQueueID.Contains(d.Queue_ID)).ToList().ForEach(item =>
                {
                    _localUnitOfWork.GetRepository<DBContext.QueueDetail>().Delete(item);
                });
                _localUnitOfWork.DataContext.Queues.Where(d => arrQueueID.Contains(d.Queue_ID)).ToList().ForEach(item =>
                {
                    _localUnitOfWork.GetRepository<DBContext.Queue>().Delete(item);
                });
            }
            #endregion
            #region Delete TimeSlot & Generate Time
            if (modelWarning != null && modelWarning.isDeleteTimeSlot)
            {
                var DateNow = DateTime.Now.Date;
                var _dayOfWeek = model.DayOfWeek == (int)EnumDayOfWeek.Sunday ? 1 : model.DayOfWeek;
                _localUnitOfWork.DataContext.Timeslots.Where(d => DbFunctions.TruncateTime(d.Date) >= DateNow && SqlFunctions.DatePart("dw", d.Date) == _dayOfWeek).ToList().ForEach(item =>
                {
                    _localUnitOfWork.GetRepository<DBContext.Timeslot>().Delete(item);
                });

                var dateGenTimeSlot = FindNexDateByDayOfWeek((EnumDayOfWeek)model.DayOfWeek);
                List<DBContext.Timeslot> arrTimeSlot = new List<Timeslot>();

                #region Morning
                arrTimeSlot.AddRange(GenerateTimeSlot(dateGenTimeSlot, operationSetting.Morning_Open_Time.Value, operationSetting.Morning_Close_Time.Value, operationSetting.Morning_Interval.Value, model.Last_Updated_By, operationSetting.Morning_MaximumSupervisee.Value, EnumTimeshift.Morning));
                #endregion
                #region Evening
                arrTimeSlot.AddRange(GenerateTimeSlot(dateGenTimeSlot, operationSetting.Evening_Open_Time.Value, operationSetting.Evening_Close_Time.Value, operationSetting.Evening_Interval.Value, model.Last_Updated_By, operationSetting.Evening_MaximumSupervisee.Value, EnumTimeshift.Evening));
                #endregion
                #region Afternoon
                arrTimeSlot.AddRange(GenerateTimeSlot(dateGenTimeSlot, operationSetting.Afternoon_Open_Time.Value, operationSetting.Afternoon_Close_Time.Value, operationSetting.Afternoon_Interval.Value, model.Last_Updated_By, operationSetting.Afternoon_MaximumSupervisee.Value, EnumTimeshift.Afternoon));
                #endregion
                _localUnitOfWork.GetRepository<DBContext.Timeslot>().AddRange(arrTimeSlot);
            }
            #endregion
            _localUnitOfWork.Save();
        }
        private List<DBContext.Timeslot> GenerateTimeSlot(DateTime date, TimeSpan openTime, TimeSpan closeTime, int duration, string createBy, int maxSupervisee, string Category)
        {
            var fromTime = openTime;
            var toTime = closeTime;
            List<DBContext.Timeslot> array = new List<Timeslot>();

            while (fromTime < toTime)
            {
                var timeSlot = new DBContext.Timeslot();
                timeSlot.Timeslot_ID = Guid.NewGuid().ToString().Trim();
                timeSlot.StartTime = fromTime;
                timeSlot.EndTime = fromTime.Add(TimeSpan.FromMinutes(duration));
                timeSlot.Date = date.Date.Date;
                timeSlot.Description = string.Empty;

                timeSlot.MaximumSupervisee = maxSupervisee;
                timeSlot.Category = Category;

                timeSlot.CreatedDate = DateTime.Today;
                timeSlot.CreatedBy = createBy;
                timeSlot.LastUpdatedBy = createBy;
                timeSlot.LastUpdatedDate = DateTime.Now;


                fromTime = fromTime.Add(TimeSpan.FromMinutes(duration));
                array.Add(timeSlot);
            }

            return array;
        }
        public CheckWarningSaveSetting CheckWarningSaveSetting(EnumDayOfWeek DayOfWeek)
        {
            CheckWarningSaveSetting modelReturn = new BE.CheckWarningSaveSetting();
            var _dayOfWeek = DayOfWeek == EnumDayOfWeek.Sunday ? 1 : (int)DayOfWeek;
            var DateNow = DateTime.Now.Date;
            var arrayBookAppoint = _localUnitOfWork.DataContext.Appointments.Include("Membership_Users").Include("Timeslot").Include("Queues").Where(d => !string.IsNullOrEmpty(d.Timeslot_ID) && DbFunctions.TruncateTime(d.Date) >= DateNow && SqlFunctions.DatePart("dw", d.Date) == _dayOfWeek).ToList();
            // danh sách những user đc warning
            List<CheckWarningSaveSetting> arrayListUser = new List<CheckWarningSaveSetting>();
            if ((int)DayOfWeek == DateTime.Now.DayOfWeek())
            {
                //Nếu là thứ hiện tại cập nhật và chưa đã có queue đc chạy
                bool isQueueStarted = arrayBookAppoint.Any(d => d.Date.Date == DateTime.Now.Date && d.Timeslot.StartTime.Value <= DateTime.Now.TimeOfDay);
                if (!isQueueStarted)
                {
                    modelReturn.arrayDetail = arrayBookAppoint.Select(d => new CheckWarningSaveSettingDetail()
                    {
                        Date = d.Date,
                        Email = d.Membership_Users.Email,
                        StartTime = d.Timeslot.StartTime.Value,
                        EndTime = d.Timeslot.EndTime.Value,
                        Timeslot_ID = d.Timeslot_ID,
                        UserId = d.UserId,
                        UserName = d.Membership_Users.UserName,
                        Queue_ID = d.Queues.Select(c => c.Queue_ID).FirstOrDefault(),
                        AppointmentID = d.ID
                    }).ToList();
                    modelReturn.isDeleteTimeSlot = true;
                }
            }
            else
            {
                modelReturn.arrayDetail = arrayBookAppoint.Select(d => new CheckWarningSaveSettingDetail()
                {
                    Date = d.Date,
                    Email = d.Membership_Users.Email,
                    StartTime = d.Timeslot.StartTime.Value,
                    EndTime = d.Timeslot.EndTime.Value,
                    Timeslot_ID = d.Timeslot_ID,
                    UserId = d.UserId,
                    UserName = d.Membership_Users.UserName,
                    Queue_ID = d.Queues.Select(c => c.Queue_ID).FirstOrDefault(),
                    AppointmentID = d.ID
                }).ToList();
                modelReturn.isDeleteTimeSlot = true;
            }

            return modelReturn;
        }
        #endregion
    }
}
