using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.BE
{

    [Serializable]
    public class EnvironmentTime
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

    public class EnvironmentTimeDetail
    {
        public List<EnvironmentTime> Morning { get; set; }
        public List<EnvironmentTime> Afternoon { get; set; }
        public List<EnvironmentTime> Evening { get; set; }
        public EnvironmentTimeDetail()
        {
            Morning = new List<EnvironmentTime>();
            Afternoon = new List<EnvironmentTime>();
            Evening = new List<EnvironmentTime>();
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
        public int MaxSuperviseePerTimeslot { get; set; }
        public int ReservedForSpare { get; set; }

    }

    public class SettingDetails
    {
        public Nullable<System.TimeSpan> StartTime { get; set; }
        public Nullable<System.TimeSpan> EndTime { get; set; }
        public Nullable<int> Duration { get; set; }

    }
    public class SettingBE
    {
        public Nullable<System.TimeSpan> Mon_Open_Time { get; set; }
        public Nullable<System.TimeSpan> Mon_Close_Time { get; set; }
        public Nullable<int> Mon_Interval { get; set; }
        public Nullable<System.TimeSpan> Tue_Open_Time { get; set; }
        public Nullable<System.TimeSpan> Tue_Close_Time { get; set; }
        public Nullable<int> Tue_Interval { get; set; }
        public Nullable<System.TimeSpan> Wed_Open_Time { get; set; }
        public Nullable<System.TimeSpan> Wed_Close_Time { get; set; }
        public Nullable<int> Wed_Interval { get; set; }
        public Nullable<System.TimeSpan> Thu_Open_Time { get; set; }
        public Nullable<System.TimeSpan> Thu_Close_Time { get; set; }
        public Nullable<int> Thu_Interval { get; set; }
        public Nullable<System.TimeSpan> Fri_Open_Time { get; set; }
        public Nullable<System.TimeSpan> Fri_Close_Time { get; set; }
        public Nullable<int> Fri_Interval { get; set; }
        public Nullable<System.TimeSpan> Sat_Open_Time { get; set; }
        public Nullable<System.TimeSpan> Sat_Close_Time { get; set; }
        public Nullable<int> Sat_Interval { get; set; }
        public Nullable<System.TimeSpan> Sun_Open_Time { get; set; }
        public Nullable<System.TimeSpan> Sun_Close_Time { get; set; }
        public Nullable<int> Sun_Interval { get; set; }
        public System.DateTime Last_Updated_Date { get; set; }
        public int MaxSuperviseePerTimeslot { get; set; }
        public int ReservedForSpare { get; set; }

        public SettingModel ToSettingModel(SettingBE rawData)
        {
            var settingModel = new SettingModel
            {
                Monday = new SettingDetails
                {
                    EndTime = rawData.Mon_Close_Time,
                    StartTime = rawData.Mon_Open_Time,
                    Duration = rawData.Mon_Interval,

                },
                Tuesday = new SettingDetails
                {
                    EndTime = rawData.Tue_Close_Time,
                    StartTime = rawData.Tue_Open_Time,
                    Duration = rawData.Tue_Interval,

                },
                WednesDay = new SettingDetails
                {
                    EndTime = rawData.Wed_Close_Time,
                    StartTime = rawData.Wed_Open_Time,
                    Duration = rawData.Wed_Interval,

                },
                Thursday = new SettingDetails
                {
                    EndTime = rawData.Thu_Close_Time,
                    StartTime = rawData.Thu_Open_Time,
                    Duration = rawData.Thu_Interval,

                },
                Friday = new SettingDetails
                {
                    EndTime = rawData.Fri_Close_Time,
                    StartTime = rawData.Fri_Open_Time,
                    Duration = rawData.Fri_Interval,

                },
                Saturday = new SettingDetails
                {
                    EndTime = rawData.Sat_Close_Time,
                    StartTime = rawData.Sat_Open_Time,
                    Duration = rawData.Sat_Interval,

                },
                Sunday = new SettingDetails
                {
                    EndTime = rawData.Sun_Close_Time,
                    StartTime = rawData.Sun_Open_Time,
                    Duration = rawData.Sun_Interval,

                },
                Last_Updated_Date = rawData.Last_Updated_Date,
                MaxSuperviseePerTimeslot = rawData.MaxSuperviseePerTimeslot,
                ReservedForSpare = rawData.ReservedForSpare
            };

            return settingModel;
        }

    }
    public class TimeslotDetails
    {
        public int Timeslot_ID { get; set; }
        public int DateOfWeek { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public int? Duration { get; set; }
    }


}
