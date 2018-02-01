using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.BE
{

    
    public class WorkingShiftDetails
    {

        public int DayOfWeek { get; set; }
        public Nullable<int> Spare_Slots { get; set; }
        public Nullable<int> Interval { get; set; }
        public Nullable<int> MaximumSupervisee { get; set; }
        public bool Is_Closed { get; set; }

        public bool IsAvailble { get; set; }
        public bool IsSelected { get; set; }
        public System.TimeSpan StartTime { get; set; }
        public System.TimeSpan EndTime { get; set; }
        public string Category { get; set; }
        public string StartTimeTxt
        {
            get
            {
                return string.Format("{0:D2}:{1:D2}", StartTime.Hours, StartTime.Minutes);
            }
        }
        public string EndTimeTxt
        {
            get
            {
                return string.Format("{0:D2}:{1:D2}", EndTime.Hours, EndTime.Minutes);
            }
        }
        public string TimeTxt
        {
            get
            {
                return StartTime.ToString() + " - " + EndTime.ToString();
            }
        }
    }

    public class WorkingTimeshift
    {
        public List<WorkingShiftDetails> Morning { get; set; }
        public List<WorkingShiftDetails> Afternoon { get; set; }
        public List<WorkingShiftDetails> Evening { get; set; }
        public WorkingTimeshift()
        {
            Morning = new List<WorkingShiftDetails>();
            Afternoon = new List<WorkingShiftDetails>();
            Evening = new List<WorkingShiftDetails>();
        }

    }

    public class SettingModel
    {
        public SettingDetails Monday { get; set; }
        public SettingDetails Tuesday { get; set; }
        public SettingDetails Wednesday { get; set; }
        public SettingDetails Thursday { get; set; }
        public SettingDetails Friday { get; set; }
        public SettingDetails Saturday { get; set; }
        public SettingDetails Sunday { get; set; }
        public List<Trinity.BE.Holiday> HoliDays { get; set; }
        public List<Trinity.BE.OperationSettings_ChangeHist> ChangeHistorySettings { get; set; }
    }

    public class SettingDetails
    {
        public int DayOfWeek { get; set; }
        public string DayOfWeekTxt
        {
            get
            {
                return ((EnumDayOfWeek)DayOfWeek).ToString();
            }
        }
        [Custom(Name = "Morning start Time")]
        public Nullable<System.TimeSpan> Morning_Open_Time { get; set; }
        [Custom(Name = "Morning end Time")]
        public Nullable<System.TimeSpan> Morning_Close_Time { get; set; }
        [Custom(Name = "Morning reserved for spare")]
        public Nullable<int> Morning_Spare_Slots { get; set; }
        [Custom(Name = "Morning intervals")]
        public Nullable<int> Morning_Interval { get; set; }
        [Custom(Name = "Morning maximum Supervisee per timeslot")]
        public Nullable<int> Morning_MaximumSupervisee { get; set; }

        [Custom(Name = "Afternoon start Time")]
        public Nullable<System.TimeSpan> Afternoon_Open_Time { get; set; }
        [Custom(Name = "Afternoon end Time")]
        public Nullable<System.TimeSpan> Afternoon_Close_Time { get; set; }
        [Custom(Name = "Afternoon reserved for spare")]
        public Nullable<int> Afternoon_Spare_Slots { get; set; }
        [Custom(Name = "Afternoon intervals")]
        public Nullable<int> Afternoon_Interval { get; set; }
        [Custom(Name = "Afternoon maximum Supervisee per timeslot")]
        public Nullable<int> Afternoon_MaximumSupervisee { get; set; }

        [Custom(Name = "Evening start Time")]
        public Nullable<System.TimeSpan> Evening_Open_Time { get; set; }
        [Custom(Name = "Evening end Time")]
        public Nullable<System.TimeSpan> Evening_Close_Time { get; set; }
        [Custom(Name = "Evening reserved for spare")]
        public Nullable<int> Evening_Spare_Slots { get; set; }
        [Custom(Name = "Evening intervals")]
        public Nullable<int> Evening_Interval { get; set; }
        [Custom(Name = "Evening maximum Supervisee per timeslot")]
        public Nullable<int> Evening_MaximumSupervisee { get; set; }
        public bool Is_Closed { get; set; }
        public System.DateTime Last_Updated_Date { get; set; }
        public string Last_Updated_By { get; set; }
        public string Description { get; set; }

        [Custom(Name = "Morning closed")]
        public bool Morning_Is_Closed { get; set; }
        [Custom(Name = "Afternoon closed")]
        public bool Afternoon_Is_Closed { get; set; }
        [Custom(Name = "Evening closed")]
        public bool Evening_Is_Closed { get; set; }
        public List<Trinity.BE.Holiday> HoliDays { get; set; }

        public SettingDetails SetADay(SettingBE rawData)
        {
            return new SettingDetails
            {
                DayOfWeek = rawData.DayOfWeek,
                Morning_Open_Time = rawData.Morning_Open_Time,
                Morning_Close_Time = rawData.Morning_Close_Time,
                Morning_Spare_Slots = rawData.Morning_Spare_Slots,
                Morning_Interval = rawData.Morning_Interval,
                Morning_MaximumSupervisee = rawData.Morning_MaximumSupervisee,
                Afternoon_Open_Time = rawData.Afternoon_Open_Time,
                Afternoon_Close_Time = rawData.Afternoon_Close_Time,
                Afternoon_Spare_Slots = rawData.Afternoon_Spare_Slots,
                Afternoon_Interval = rawData.Afternoon_Interval,
                Afternoon_MaximumSupervisee = rawData.Afternoon_MaximumSupervisee,
                Evening_Open_Time = rawData.Evening_Open_Time,
                Evening_Close_Time = rawData.Evening_Close_Time,
                Evening_Spare_Slots = rawData.Evening_Spare_Slots,
                Evening_Interval = rawData.Evening_Interval,
                Evening_MaximumSupervisee = rawData.Evening_MaximumSupervisee,
                Is_Closed = rawData.Is_Closed,
                Description = rawData.Description,
                Last_Updated_By = rawData.Last_Updated_By,
                Last_Updated_Date = rawData.Last_Updated_Date
            };
        }
    }
    public class SettingBE
    {
        public int DayOfWeek { get; set; }
        public Nullable<System.TimeSpan> Morning_Open_Time { get; set; }
        public Nullable<System.TimeSpan> Morning_Close_Time { get; set; }
        public Nullable<int> Morning_Spare_Slots { get; set; }
        public Nullable<int> Morning_Interval { get; set; }
        public Nullable<int> Morning_MaximumSupervisee { get; set; }
        public Nullable<System.TimeSpan> Afternoon_Open_Time { get; set; }
        public Nullable<System.TimeSpan> Afternoon_Close_Time { get; set; }
        public Nullable<int> Afternoon_Spare_Slots { get; set; }
        public Nullable<int> Afternoon_Interval { get; set; }
        public Nullable<int> Afternoon_MaximumSupervisee { get; set; }
        public Nullable<System.TimeSpan> Evening_Open_Time { get; set; }
        public Nullable<System.TimeSpan> Evening_Close_Time { get; set; }
        public Nullable<int> Evening_Spare_Slots { get; set; }
        public Nullable<int> Evening_Interval { get; set; }
        public Nullable<int> Evening_MaximumSupervisee { get; set; }
        public bool Is_Closed { get; set; }
        public string Last_Updated_By { get; set; }
        public System.DateTime Last_Updated_Date { get; set; }
        public string Description { get; set; }

        public SettingModel ToSettingModel(SettingBE rawData)
        {
            var settingDetail = new SettingDetails();
            var settingModel = new SettingModel
            {
                Monday = settingDetail.SetADay(rawData),
                Tuesday = settingDetail.SetADay(rawData),
                Wednesday = settingDetail.SetADay(rawData),
                Thursday = settingDetail.SetADay(rawData),
                Friday = settingDetail.SetADay(rawData),
                Saturday = settingDetail.SetADay(rawData),
                Sunday = settingDetail.SetADay(rawData),
            };

            return settingModel;
        }

    }
    public class TimeslotDetails
    {
        public string Timeslot_ID { get; set; }

        public Nullable<System.TimeSpan> StartTime { get; set; }
        public Nullable<System.TimeSpan> EndTime { get; set; }
        public Nullable<System.DateTime> Date { get; set; }
        public int? Duration { get; set; }
        public string Category { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string LastUpdatedBy { get; set; }
        public Nullable<System.DateTime> LastUpdatedDate { get; set; }
        public string Description { get; set; }
        public Nullable<int> MaximumSupervisee { get; set; }
    }


}
