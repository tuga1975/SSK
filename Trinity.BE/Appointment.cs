using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.BE
{
    [Serializable]

    public class Appointment
    {
        [DataMember]
        public string UserId { get; set; }

        [DataMember]
        public string NRIC { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public Nullable<System.DateTime> AppointmentDate { get; set; }

        [DataMember]
        public Nullable<System.DateTime> ReportTime { get; set; }


        [DataMember]
        public Nullable<short> ChangedCount { get; set; }

        [DataMember]
        public EnumAppointmentStatuses Status { get; set; }

        [DataMember]
        public int? Timeslot_ID { get; set; }

        [DataMember]
        public Nullable<System.TimeSpan> StartTime { get; set; }

        [DataMember]
        public Nullable<System.TimeSpan> EndTime { get; set; }

    }
}
