using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;


namespace Trinity.DAL.DBContext
{

    public partial class Appointment
    {
        public string GetDateTxt
        {
            get
            {
                return Date.ToString("dddd, dd MMM yyyy");
            }
        }
        public string FromTimeTxt
        {
            get
            {
                if (Timeslot != null)
                {
                    return Timeslot.StartTime.HasValue ? string.Format("{0:D2}:{1:D2}", Timeslot.StartTime.Value.Hours, Timeslot.StartTime.Value.Minutes) : string.Empty;
                }
                return string.Empty;

            }
        }
        public string ToTimeTxt
        {
            get
            {
                if (Timeslot != null)
                {
                    return Timeslot.EndTime.HasValue ? string.Format("{0:D2}:{1:D2}", Timeslot.EndTime.Value.Hours, Timeslot.EndTime.Value.Minutes) : string.Empty;
                }
                return string.Empty;
            }
        }
        public Trinity.DAL.DBContext.Queue Queue
        {
            get
            {
                return this.Queues.FirstOrDefault();
            }
        }
        public string Color(string Station)
        {
            if (Station == EnumStation.APS)
            {
                if (this.Status == EnumAppointmentStatuses.Pending)
                    return EnumColors.Grey;
                else
                    return EnumColors.Green;
            }
            else if (Station == EnumStation.SSK)
            {
                if (this.Status != EnumAppointmentStatuses.Reported)
                    return EnumColors.Grey;
                else
                    return EnumColors.Green;
            }
            else if (Queue != null)
            {
                return Queue.QueueDetails.FirstOrDefault(c => c.Station == Station).Color;
            }
            return EnumColors.Grey;
        }
    }

    public partial class Timeslot
    {

        public string FromTimeTxt
        {
            get
            {
                if (this.StartTime != null)
                {
                    return string.Format("{0:D2}:{1:D2}", StartTime.Value.Hours, StartTime.Value.Minutes);
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
                    return string.Format("{0:D2}:{1:D2}", EndTime.Value.Hours, EndTime.Value.Minutes);
                }
                return string.Empty;
            }
        }
    }
}
