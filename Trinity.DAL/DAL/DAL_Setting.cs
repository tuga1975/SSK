using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Windows.Forms;
using Trinity.BE;
using Trinity.DAL.DBContext;
using Trinity.DAL.Repository;
using Trinity.Common;


namespace Trinity.DAL
{
    public class DAL_Setting
    {
        Local_UnitOfWork _localUnitOfWork = new Local_UnitOfWork();
        Centralized_UnitOfWork _centralizedUnitOfWork = new Centralized_UnitOfWork();


        public Response<WorkingTimeshift> GetAppointmentTime(DateTime date)
        {
            try
            {

                var appointmentTime = new Trinity.BE.WorkingTimeshift();
                var listTimeSlot = GetTimeslots(date);
                if (listTimeSlot.Count > 0)
                {

                    var morningTimeSpan = new TimeSpan(12, 0, 0);
                    var eveningTimeSpan = new TimeSpan(17, 0, 0);

                    appointmentTime.Morning = listTimeSlot.Where(d => d.Category == EnumTimeshift.Morning).Select(d => SetAppointmentTime(d)).ToList();
                    appointmentTime.Evening = listTimeSlot.Where(d => d.Category == EnumTimeshift.Evening).Select(d => SetAppointmentTime(d)).ToList();
                    appointmentTime.Afternoon = listTimeSlot.Where(d => d.Category == EnumTimeshift.Afternoon).Select(d => SetAppointmentTime(d)).ToList();


                    //foreach (var item in listTimeSlot)
                    //{
                    //    var setTime = SetAppointmentTime(item);
                    //    if (setTime.EndTime <= morningTimeSpan)
                    //    {
                    //        appointmentTime.Morning.Add(setTime);
                    //    }
                    //    else if (setTime.EndTime > eveningTimeSpan)
                    //    {
                    //        appointmentTime.Evening.Add(setTime);
                    //    }
                    //    else
                    //    {
                    //        appointmentTime.Afternoon.Add(setTime);
                    //    }
                    //}

                }
                return new Response<WorkingTimeshift>((int)EnumResponseStatuses.Success, EnumResponseMessage.Success, appointmentTime);
            }
            catch (Exception)
            {

                return new Response<WorkingTimeshift>((int)EnumResponseStatuses.ErrorSystem,EnumResponseMessage.ErrorSystem,null);
            }
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

        public Response<WorkingTimeshift> GetCurrentAppointmentTime()
        {
            var today = DateTime.Now;
            var result= GetAppointmentTime(today);
            return result;
        }

        public List<Timeslot> GetTimeslots(DateTime date)
        {
            SettingModel setting = GetSettings();
            //GenerateTimeslots("dfbb2a6a-9e45-4a76-9f75-af1a7824a947");
            int dayOfWeek = date.DayOfWeek();
            if (EnumAppConfig.IsLocal)
            {
                return _localUnitOfWork.DataContext.Timeslots.Where(t => DbFunctions.TruncateTime(t.Date) == date.Date).ToList();
            }
            return _centralizedUnitOfWork.DataContext.Timeslots.Where(t => DbFunctions.TruncateTime(t.Date) == date.Date).ToList();


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
            GenerateTimeslotAndInsert(date.Date, settings.Wednesday, createdBy);

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
            if (model.Morning_Open_Time.HasValue && model.Morning_Close_Time.HasValue && model.Morning_Interval.HasValue&&!model.Morning_Is_Closed)
            {
                GenerateByTimeshift(date, model.Morning_Open_Time, model.Morning_Close_Time, model.Morning_Interval, createBy, maxSupervisee);
            }

            if (model.Afternoon_Open_Time.HasValue && model.Afternoon_Close_Time.HasValue && model.Afternoon_Interval.HasValue&&!model.Afternoon_Is_Closed)
            {
                GenerateByTimeshift(date, model.Afternoon_Open_Time, model.Afternoon_Close_Time, model.Afternoon_Interval, createBy, maxSupervisee);
            }

            if (model.Evening_Open_Time.HasValue && model.Evening_Close_Time.HasValue && model.Evening_Interval.HasValue&&!model.Evening_Is_Closed)
            {
                GenerateByTimeshift(date, model.Evening_Open_Time, model.Evening_Close_Time, model.Evening_Interval, createBy, maxSupervisee);
            }

        }
        private void GenerateByTimeshift(DateTime date, TimeSpan? openTime, TimeSpan? closeTime, int? duration, string createBy, int maxSupervisee = 0)
        {
            var fromTime = openTime;
            var toTime = closeTime;
            var morningTimeSpan = new TimeSpan(12, 0, 0);
            var eveningTimeSpan = new TimeSpan(17, 0, 0);
            var id = _localUnitOfWork.DataContext.Timeslots.Any() ? _localUnitOfWork.DataContext.Timeslots.Max(t => t.Timeslot_ID) : null;
            while (fromTime < toTime)
            {
                var timeSlot = new BE.TimeslotDetails();

                

                timeSlot.Timeslot_ID = Guid.NewGuid().ToString();

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
            settingBE.Morning_Is_Closed = setting.Morning_Is_Closed;

            settingBE.Afternoon_Open_Time = setting.Afternoon_Open_Time;
            settingBE.Afternoon_Close_Time = setting.Afternoon_Close_Time;
            settingBE.Afternoon_Interval = setting.Afternoon_Interval;
            settingBE.Afternoon_MaximumSupervisee = setting.Afternoon_MaximumSupervisee;
            settingBE.Afternoon_Spare_Slots = setting.Afternoon_Spare_Slots;
            settingBE.Afternoon_Is_Closed = setting.Afternoon_Is_Closed;

            settingBE.Evening_Open_Time = setting.Evening_Open_Time;
            settingBE.Evening_Close_Time = setting.Evening_Close_Time;
            settingBE.Evening_Interval = setting.Evening_Interval;
            settingBE.Evening_MaximumSupervisee = setting.Evening_MaximumSupervisee;
            settingBE.Evening_Spare_Slots = setting.Evening_Spare_Slots;
            settingBE.Evening_Is_Closed = setting.Evening_Is_Closed;


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
            setting.Morning_Is_Closed = settingBE.Morning_Is_Closed;

            setting.Afternoon_Open_Time = settingBE.Afternoon_Open_Time;
            setting.Afternoon_Close_Time = settingBE.Afternoon_Close_Time;
            setting.Afternoon_Interval = settingBE.Afternoon_Interval;
            setting.Afternoon_MaximumSupervisee = settingBE.Afternoon_MaximumSupervisee;
            setting.Afternoon_Spare_Slots = settingBE.Afternoon_Spare_Slots;
            setting.Afternoon_Is_Closed = settingBE.Afternoon_Is_Closed;

            setting.Evening_Open_Time = settingBE.Evening_Open_Time;
            setting.Evening_Close_Time = settingBE.Evening_Close_Time;
            setting.Evening_Interval = settingBE.Evening_Interval;
            setting.Evening_MaximumSupervisee = settingBE.Evening_MaximumSupervisee;
            setting.Evening_Spare_Slots = settingBE.Evening_Spare_Slots;
            setting.Evening_Is_Closed = settingBE.Evening_Is_Closed;

            setting.Last_Updated_By = settingBE.Last_Updated_By;
            setting.Last_Updated_Date = settingBE.Last_Updated_Date;
            setting.Description = settingBE.Description;
        }

        public BE.SettingModel GetSettings()
        {
            OperationSetting dbSetting = new OperationSetting() ;
            if (EnumAppConfig.IsLocal)
            {
                dbSetting = _localUnitOfWork.DataContext.OperationSettings.FirstOrDefault();
            }
            else
            {
                dbSetting = _centralizedUnitOfWork.DataContext.OperationSettings.FirstOrDefault();
            }
            
            var settingBE = new BE.SettingBE();

            if (dbSetting != null)
            {
                SetInfoToSettingBE(settingBE, dbSetting);
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
                Wednesday = arraySetting.FirstOrDefault(d => d.DayOfWeek == (int)EnumDayOfWeek.Wednesday).Map<BE.SettingDetails>(),
                Thursday = arraySetting.FirstOrDefault(d => d.DayOfWeek == (int)EnumDayOfWeek.Thursday).Map<BE.SettingDetails>(),
                Friday = arraySetting.FirstOrDefault(d => d.DayOfWeek == (int)EnumDayOfWeek.Friday).Map<BE.SettingDetails>(),
                Saturday = arraySetting.FirstOrDefault(d => d.DayOfWeek == (int)EnumDayOfWeek.Saturday).Map<BE.SettingDetails>(),
                Sunday = arraySetting.FirstOrDefault(d => d.DayOfWeek == (int)EnumDayOfWeek.Sunday).Map<BE.SettingDetails>(),
                HoliDays = GetHolidays(),
                ChangeHistorySettings = GetHistoryChangeSettings()
            };

            settingModel.Monday = settingModel.Monday == null ? new SettingDetails() { DayOfWeek = (int)EnumDayOfWeek.Monday } : settingModel.Monday;
            settingModel.Tuesday = settingModel.Tuesday == null ? new SettingDetails() { DayOfWeek = (int)EnumDayOfWeek.Tuesday } : settingModel.Tuesday;
            settingModel.Wednesday = settingModel.Wednesday == null ? new SettingDetails() { DayOfWeek = (int)EnumDayOfWeek.Wednesday } : settingModel.Wednesday;
            settingModel.Thursday = settingModel.Thursday == null ? new SettingDetails() { DayOfWeek = (int)EnumDayOfWeek.Thursday } : settingModel.Thursday;
            settingModel.Friday = settingModel.Friday == null ? new SettingDetails() { DayOfWeek = (int)EnumDayOfWeek.Friday } : settingModel.Friday;
            settingModel.Saturday = settingModel.Saturday == null ? new SettingDetails() { DayOfWeek = (int)EnumDayOfWeek.Saturday } : settingModel.Saturday;
            settingModel.Sunday = settingModel.Sunday == null ? new SettingDetails() { DayOfWeek = (int)EnumDayOfWeek.Sunday } : settingModel.Sunday;

            return settingModel;
        }

        #region Duty Officer        

        private DateTime FindNexDateByDayOfWeek(EnumDayOfWeek dayOfWeek)
        {
            int currentDayOfWeek = (int)Common.CommonUtil.ConvertToCustomDateOfWeek(DateTime.Now.DayOfWeek);
            int daysToAdd = ((int)dayOfWeek - currentDayOfWeek + 7) % 7;
            return DateTime.Today.AddDays(daysToAdd);
        }

        public List<Trinity.BE.OperationSettings_ChangeHist> GetHistoryChangeSettings()
        {
            var repoChangeHistorySetting = _localUnitOfWork.GetRepository<DBContext.OperationSettings_ChangeHist>();
            List<BE.OperationSettings_ChangeHist> results = repoChangeHistorySetting.GetAll().ToList().Select(d => d.Map<BE.OperationSettings_ChangeHist>()).OrderByDescending(t => t.LastUpdatedDate).ToList();

            return results;
        }

        public List<BE.Holiday> GetHolidays()
        {
            var repoHolidays = _localUnitOfWork.GetRepository<DBContext.Holiday>();
            List<BE.Holiday> results = repoHolidays.GetAll().ToList().Select(d => d.Map<BE.Holiday>()).ToList();

            return results;
        }

        public void AddHoliday(DBContext.Holiday holiday, string updatedByName, string updatedByID)
        {
            try
            {
                _localUnitOfWork.GetRepository<DBContext.Holiday>().Add(holiday);

                // Insert to Change History Setting
                int dayOfWeek = (int)Common.CommonUtil.ConvertToCustomDateOfWeek(holiday.Holiday1.DayOfWeek);
                var operationSetting = _localUnitOfWork.DataContext.OperationSettings.FirstOrDefault(s => s.DayOfWeek == dayOfWeek);
                if (operationSetting == null)
                {
                    operationSetting = new OperationSetting();
                    operationSetting.DayOfWeek = dayOfWeek;
                    operationSetting.Morning_Is_Closed = false;
                    operationSetting.Afternoon_Is_Closed = false;
                    operationSetting.Evening_Is_Closed = false;
                    operationSetting.Last_Updated_By = updatedByID;
                    operationSetting.Last_Updated_Date = DateTime.Now;
                    _localUnitOfWork.GetRepository<OperationSetting>().Add(operationSetting);
                }

                var changeHistoryID = _localUnitOfWork.DataContext.OperationSettings_ChangeHist.Any() ? _localUnitOfWork.DataContext.OperationSettings_ChangeHist.Max(t => t.ID) : 0;
                DBContext.OperationSettings_ChangeHist changeHistory = new DBContext.OperationSettings_ChangeHist();
                changeHistory.ID = changeHistoryID + 1;
                changeHistory.DayOfWeek = dayOfWeek;
                changeHistory.LastUpdatedBy = updatedByName;
                changeHistory.LastUpdatedDate = DateTime.Now;
                changeHistory.ChangeDetails = "The holiday " + holiday.Holiday1.ToString("dd/MM/yyyy") + " has been added";

                _localUnitOfWork.GetRepository<DBContext.OperationSettings_ChangeHist>().Add(changeHistory);

                _localUnitOfWork.Save();
            }
            catch (Exception e)
            {
            }
        }

        public void DeleteHoliday(DateTime date, string updatedBy)
        {
            try
            {
                _localUnitOfWork.GetRepository<DBContext.Holiday>().Delete(h => h.Holiday1.Year == date.Year && h.Holiday1.Month == date.Month && h.Holiday1.Day == date.Day);

                // Insert to Change History Setting
                var changeHistoryID = _localUnitOfWork.DataContext.OperationSettings_ChangeHist.Any() ? _localUnitOfWork.DataContext.OperationSettings_ChangeHist.Max(t => t.ID) : 0;
                DBContext.OperationSettings_ChangeHist changeHistory = new DBContext.OperationSettings_ChangeHist();
                changeHistory.ID = changeHistoryID + 1;
                changeHistory.DayOfWeek = (int)Common.CommonUtil.ConvertToCustomDateOfWeek(date.DayOfWeek);
                changeHistory.LastUpdatedBy = updatedBy;
                changeHistory.LastUpdatedDate = DateTime.Now;
                changeHistory.ChangeDetails = "The holiday " + date.ToString("dd/MM/yyyy") + " has been deleted";

                _localUnitOfWork.GetRepository<DBContext.OperationSettings_ChangeHist>().Add(changeHistory);

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
                    return GetSettings().Wednesday;
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
                        arrayUpdateHistory.Add("Changed " + cusAttr.Name + " of "+ model.DayOfWeekTxt + " to " + (dataUpdate == null ? string.Empty : dataUpdate.ToString()));
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
                                arrayUpdateHistory.Add("Changed " + cusAttr.Name + " of " + model.DayOfWeekTxt + " to " + (dataUpdate==null?string.Empty: dataUpdate.ToString()));
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

            // Insert to Change History Setting
            if (arrayUpdateHistory.Count > 0)
            {
                var dalUser = new DAL_User();
                var result = dalUser.GetUserByUserId(model.Last_Updated_By);
                Trinity.BE.User oficcer = result.Data;
                var changeHistoryID = _localUnitOfWork.DataContext.OperationSettings_ChangeHist.Any() ? _localUnitOfWork.DataContext.OperationSettings_ChangeHist.Max(t => t.ID) : 0;
                //System.Text.StringBuilder changeDetails = new System.Text.StringBuilder();
                foreach (var detail in arrayUpdateHistory)
                {
                    //changeDetails.Append(detail + Environment.NewLine);
                    DBContext.OperationSettings_ChangeHist changeHistory = new DBContext.OperationSettings_ChangeHist();
                    changeHistory.ID = changeHistoryID + 1;
                    changeHistory.DayOfWeek = model.DayOfWeek;
                    changeHistory.LastUpdatedBy = oficcer.Name;
                    changeHistory.LastUpdatedDate = DateTime.Now;
                    changeHistory.ChangeDetails = detail;

                    _localUnitOfWork.GetRepository<DBContext.OperationSettings_ChangeHist>().Add(changeHistory);

                    changeHistoryID += 1;
                }
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
                arrTimeSlot.AddRange(GenerateTimeSlot(dateGenTimeSlot, operationSetting.Morning_Open_Time.Value, operationSetting.Morning_Close_Time.Value, operationSetting.Morning_Interval.HasValue ? operationSetting.Morning_Interval.Value : 15,
                                                        model.Last_Updated_By, operationSetting.Morning_MaximumSupervisee.HasValue ? operationSetting.Morning_MaximumSupervisee.Value : 0, EnumTimeshift.Morning));
                #endregion
                #region Evening
                arrTimeSlot.AddRange(GenerateTimeSlot(dateGenTimeSlot, operationSetting.Evening_Open_Time.Value, operationSetting.Evening_Close_Time.Value, operationSetting.Evening_Interval.HasValue ? operationSetting.Evening_Interval.Value : 15,
                                                        model.Last_Updated_By, operationSetting.Evening_MaximumSupervisee.HasValue ? operationSetting.Evening_MaximumSupervisee.Value : 0, EnumTimeshift.Evening));
                #endregion
                #region Afternoon
                arrTimeSlot.AddRange(GenerateTimeSlot(dateGenTimeSlot, operationSetting.Afternoon_Open_Time.Value, operationSetting.Afternoon_Close_Time.Value, operationSetting.Afternoon_Interval.HasValue ? operationSetting.Afternoon_Interval.Value : 15,
                                                        model.Last_Updated_By, operationSetting.Afternoon_MaximumSupervisee.HasValue ? operationSetting.Afternoon_MaximumSupervisee.Value : 0, EnumTimeshift.Afternoon));
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

        //public Setting GetSettingSystemByYear(int year)
        //{
        //    return _localUnitOfWork.DataContext.Settings.FirstOrDefault(s => s.Year == year);
        //}

        //public void UpdateSettingSystem(Setting setting)
        //{
        //    Setting dbSetting = _localUnitOfWork.DataContext.Settings.FirstOrDefault(s => s.Year == setting.Year);

        //    if (dbSetting == null)
        //    {
        //        _localUnitOfWork.GetRepository<Setting>().Add(setting);
        //    }
        //    else
        //    {
        //        _localUnitOfWork.GetRepository<Setting>().Update(setting);
        //    }

        //    _localUnitOfWork.Save();
        //}
    }
}
