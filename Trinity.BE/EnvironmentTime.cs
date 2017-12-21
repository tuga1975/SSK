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
}
