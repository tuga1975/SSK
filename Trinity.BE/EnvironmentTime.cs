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
        public string TimeTxt
        {
            get
            {
                return StartTime.ToString() + " - " + EndTime.ToString();
            }
        }
    }
}
