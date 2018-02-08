using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.BE
{

    
    public class Notification
    {
        [DataMember]
        public string ID { get; set; }

        [DataMember]
        public DateTime Date { get; set; }

        [DataMember]
        public string FromUserName { get; set; }

        [DataMember]
        public string FromUserId { get; set; }

        [DataMember]
        public string ToUserId { get; set; }

        [DataMember]
        public string ToUserName { get; set; }

        [DataMember]
        public string Subject { get; set; }

        [DataMember]
        public string Content { get; set; }

        [DataMember]
        public bool IsRead { get; set; }

        [DataMember]
        public bool IsFromSupervisee { get; set; }
            
        [DataMember]
        public string Type { get; set; }

        [DataMember]
        public string Source { get; set; }
    }
}
