using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.BE
{
    [Serializable]
    public class CheckWarningSaveSetting
    {
        public List<CheckWarningSaveSettingDetail> arrayDetail = new List<CheckWarningSaveSettingDetail>();
        public bool isDeleteTimeSlot = false;
    }
    public class CheckWarningSaveSettingDetail
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public DateTime Date { get; set; }
        public string Timeslot_ID { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public Nullable<Guid> Queue_ID { get; set; }
        public Guid AppointmentID { get; set; }
    }
}
