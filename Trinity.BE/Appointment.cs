using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.BE
{
    public class Appointment
    {
        public string ID { get; set; }

        [DataMember]
        public string UserId { get; set; }

        [DataMember]
        public string NRIC { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public  Nullable<System.DateTime> AppointmentDate { get; set; }

        public string GetDateTxt
        {
            get
            {
                return AppointmentDate.HasValue ? AppointmentDate.Value.ToString("dddd, dd MMM yyyy"):string.Empty;
            }
        }
       

        [DataMember]
        public Nullable<System.DateTime> ReportTime { get; set; }

        [DataMember]
        public Nullable<short> ChangedCount { get; set; }

        [DataMember]
        public string Status { get; set; }

        [DataMember]
        public string Timeslot_ID { get; set; }

        [DataMember]
        public Nullable<System.TimeSpan> StartTime { get; set; }

        [DataMember]
        public Nullable<System.TimeSpan> EndTime { get; set; }

        public Nullable<System.TimeSpan> TimeSlot { get; set; }

        public string FromTimeTxt
        {
            get
            {
                if (StartTime != null)
                {
                    return StartTime.HasValue ? string.Format("{0:D2}:{1:D2}", StartTime.Value.Hours, StartTime.Value.Minutes) : string.Empty;
                }
                return string.Empty;

            }
        }
        public string ToTimeTxt
        {
            get
            {
                if (EndTime != null)
                {
                    return EndTime.HasValue ? string.Format("{0:D2}:{1:D2}", EndTime.Value.Hours, EndTime.Value.Minutes) : string.Empty;
                }
                return string.Empty;
            }
        }

        public string Category { get; set; }

        public Nullable<Guid> AbsenceReporting_ID { get; set; }

        public Nullable<System.TimeSpan> ReportTimeSpan { get; set; }
    }
}
