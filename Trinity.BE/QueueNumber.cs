using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.BE
{

    [Serializable]
    public class Queue
    {
        public DateTime Time { get; set; }

        public string Status { get; set; }

        public string QueueNumber { get; set; }

        public Guid AppointmentId { get; set; }

        public Guid ID { get; set; }

    }
}
