using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.BE
{

    [Serializable]
    public class AppointmentTimeDetails
    {
        public bool IsAvailble { get; set; }
        public bool IsSelected { get; set; }
        public System.TimeSpan StartTime { get; set; }
        public System.TimeSpan EndTime { get; set; }

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

    public class AppointmentTime
    {
        public List<AppointmentTimeDetails> Morning { get; set; }
        public List<AppointmentTimeDetails> Afternoon { get; set; }
        public List<AppointmentTimeDetails> Evening { get; set; }
        public AppointmentTime()
        {
            Morning = new List<AppointmentTimeDetails>();
            Afternoon = new List<AppointmentTimeDetails>();
            Evening = new List<AppointmentTimeDetails>();
        }
    }

    public class SettingModel
    {
        public SettingDetails Monday { get; set; }
        public SettingDetails Tuesday { get; set; }
        public SettingDetails WednesDay { get; set; }
        public SettingDetails Thursday { get; set; }
        public SettingDetails Friday { get; set; }
        public SettingDetails Saturday { get; set; }
        public SettingDetails Sunday { get; set; }
        public System.DateTime Last_Updated_Date { get; set; }
        public string Last_Updated_By { get; set; }
        public string Description { get; set; }
        public System.Guid Setting_ID { get; set; }
        public int WeekNum { get; set; }
        public int Year { get; set; }
        public string Status { get; set; }

    }

    public class SettingDetails
    {
        public System.Guid Setting_ID { get; set; }
        public Nullable<System.TimeSpan> StartTime { get; set; }
        public Nullable<System.TimeSpan> EndTime { get; set; }
        public Nullable<int> Duration { get; set; }
        public Nullable<int> MaximumAppointment { get; set; }
        public Nullable<int> ReservedForSpare { get; set; }
    }
    public class SettingBE
    {
        public System.Guid Setting_ID { get; set; }
        public int WeekNum { get; set; }
        public int Year { get; set; }
        public string Status { get; set; }
        public Nullable<System.TimeSpan> Mon_Open_Time { get; set; }
        public Nullable<System.TimeSpan> Mon_Close_Time { get; set; }
        public Nullable<int> Mon_Interval { get; set; }
        public Nullable<int> Mon_MaximumNum { get; set; }
        public Nullable<int> Mon_ReservedForSpare { get; set; }
        public Nullable<System.TimeSpan> Tue_Open_Time { get; set; }
        public Nullable<System.TimeSpan> Tue_Close_Time { get; set; }
        public Nullable<int> Tue_Interval { get; set; }
        public Nullable<int> Tue_MaximumNum { get; set; }
        public Nullable<int> Tue_ReservedForSpare { get; set; }
        public Nullable<System.TimeSpan> Wed_Open_Time { get; set; }
        public Nullable<System.TimeSpan> Wed_Close_Time { get; set; }
        public Nullable<int> Wed_Interval { get; set; }
        public Nullable<int> Wed_MaximumNum { get; set; }
        public Nullable<int> Wed_ReservedForSpare { get; set; }
        public Nullable<System.TimeSpan> Thu_Open_Time { get; set; }
        public Nullable<System.TimeSpan> Thu_Close_Time { get; set; }
        public Nullable<int> Thu_MaximumNum { get; set; }
        public Nullable<int> Thu_Interval { get; set; }
        public Nullable<int> Thu_ReservedForSpare { get; set; }
        public Nullable<System.TimeSpan> Fri_Open_Time { get; set; }
        public Nullable<System.TimeSpan> Fri_Close_Time { get; set; }
        public Nullable<int> Fri_Interval { get; set; }
        public Nullable<int> Fri_MaximumNum { get; set; }
        public Nullable<int> Fri_ReservedForSpare { get; set; }
        public Nullable<System.TimeSpan> Sat_Open_Time { get; set; }
        public Nullable<System.TimeSpan> Sat_Close_Time { get; set; }
        public Nullable<int> Sat_Interval { get; set; }
        public Nullable<int> Sat_MaximumNum { get; set; }
        public Nullable<int> Sat_ReservedForSpare { get; set; }
        public Nullable<System.TimeSpan> Sun_Open_Time { get; set; }
        public Nullable<System.TimeSpan> Sun_Close_Time { get; set; }
        public Nullable<int> Sun_Interval { get; set; }
        public Nullable<int> Sun_MaximumNum { get; set; }
        public Nullable<int> Sun_ReservedForSpare { get; set; }
        public string Last_Updated_By { get; set; }
        public System.DateTime Last_Updated_Date { get; set; }
        public string Description { get; set; }

        public SettingModel ToSettingModel(SettingBE rawData)
        {
            var settingModel = new SettingModel
            {
                Monday = new SettingDetails
                {
                    Setting_ID = rawData.Setting_ID,
                    EndTime = rawData.Mon_Close_Time,
                    StartTime = rawData.Mon_Open_Time,
                    Duration = rawData.Mon_Interval,
                    MaximumAppointment = rawData.Mon_MaximumNum,
                    ReservedForSpare = rawData.Mon_ReservedForSpare

                },
                Tuesday = new SettingDetails
                {
                    Setting_ID = rawData.Setting_ID,
                    EndTime = rawData.Tue_Close_Time,
                    StartTime = rawData.Tue_Open_Time,
                    Duration = rawData.Tue_Interval,
                    MaximumAppointment = rawData.Tue_MaximumNum,
                    ReservedForSpare = rawData.Tue_ReservedForSpare

                },
                WednesDay = new SettingDetails
                {
                    Setting_ID = rawData.Setting_ID,
                    EndTime = rawData.Wed_Close_Time,
                    StartTime = rawData.Wed_Open_Time,
                    Duration = rawData.Wed_Interval,
                    MaximumAppointment = rawData.Wed_MaximumNum,
                    ReservedForSpare = rawData.Wed_ReservedForSpare

                },
                Thursday = new SettingDetails
                {
                    Setting_ID = rawData.Setting_ID,
                    EndTime = rawData.Thu_Close_Time,
                    StartTime = rawData.Thu_Open_Time,
                    Duration = rawData.Thu_Interval,
                    MaximumAppointment = rawData.Thu_MaximumNum,
                    ReservedForSpare = rawData.Thu_ReservedForSpare

                },
                Friday = new SettingDetails
                {
                    Setting_ID = rawData.Setting_ID,
                    EndTime = rawData.Fri_Close_Time,
                    StartTime = rawData.Fri_Open_Time,
                    Duration = rawData.Fri_Interval,
                    MaximumAppointment = rawData.Fri_MaximumNum,
                    ReservedForSpare = rawData.Fri_ReservedForSpare

                },
                Saturday = new SettingDetails
                {
                    Setting_ID = rawData.Setting_ID,
                    EndTime = rawData.Sat_Close_Time,
                    StartTime = rawData.Sat_Open_Time,
                    Duration = rawData.Sat_Interval,
                    MaximumAppointment = rawData.Sat_MaximumNum,
                    ReservedForSpare = rawData.Sat_ReservedForSpare

                },
                Sunday = new SettingDetails
                {
                    Setting_ID = rawData.Setting_ID,
                    EndTime = rawData.Sun_Close_Time,
                    StartTime = rawData.Sun_Open_Time,
                    Duration = rawData.Sun_Interval,
                    MaximumAppointment = rawData.Sun_MaximumNum,
                    ReservedForSpare = rawData.Sun_ReservedForSpare

                },
                WeekNum = rawData.WeekNum,
                Setting_ID = rawData.Setting_ID,
                Status = rawData.Status,
                Year = rawData.Year,
                Last_Updated_Date = rawData.Last_Updated_Date,
                Last_Updated_By = rawData.Last_Updated_By,
                Description = rawData.Description
            };

            return settingModel;
        }

    }
    public class TimeslotDetails
    {
        public int Timeslot_ID { get; set; }
        public Nullable<int> DateOfWeek { get; set; }
        public Nullable<System.TimeSpan> StartTime { get; set; }
        public Nullable<System.TimeSpan> EndTime { get; set; }
        public int? Duration { get; set; }
        public System.Guid Setting_ID { get; set; }
        public string Category { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string LastUpdatedBy { get; set; }
        public Nullable<System.DateTime> LastUpdatedDate { get; set; }
        public string Description { get; set; }
    }


}
