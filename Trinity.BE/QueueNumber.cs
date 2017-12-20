using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.BE
{

    [Serializable]
    public class QueueNumber
    {
        public string Status { get; set; }
        public string Queue { get; set; }
        public string QueueEncoder
        {
            get
            {
                return string.IsNullOrEmpty(Queue) ? string.Empty : Queue.EncoderQueueNumber();
            }
        }
    }
}
