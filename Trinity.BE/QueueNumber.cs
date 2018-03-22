using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.BE
{

    
    public class Queue
    {
        public DateTime Time { get; set; }

        public string Status { get; set; }

        public string QueueNumber { get; set; }

        public Guid AppointmentId { get; set; }

        public Guid Queue_ID { get; set; }
    }

    public class QueueInfo
    {
        public Guid Queue_ID { get; set; }
        public string NRIC { get; set; }
        public string Name { get; set; }
        public string UserId { get; set; }
        public string CurrentStation { get; set; }
        public string Status { get; set; }
        public List<QueueDetail> QueueDetail { get; set; }
        public DateTime Date { get; set; }
    }

    public class QueueDetail
    {
        public Guid Queue_ID { get; set; }
        public string Station { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public string QueuedNumber { get; set; }
        public string Timeslot_ID { get; set; }
    }
}
