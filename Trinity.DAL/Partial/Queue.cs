using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;


namespace Trinity.DAL.DBContext
{

    public partial class Queue
    {

        public int Priority
        {
            get
            {
                if (this.Membership_Users.Membership_UserRoles.Select(d => d.Membership_Roles).Any(d => d.Name == EnumUserRoles.DutyOfficer))
                    return 1;
                else if (this.Membership_Users.Membership_UserRoles.Select(d => d.Membership_Roles).Any(d => d.Name == EnumUserRoles.Supervisee))
                    return 2;
                else
                    return 4;
            }
        }
        public int InTimeSlot
        {
            get
            {
                if (this.Appointment_ID.HasValue && this.Appointment.Timeslot_ID == this.Timeslot.Timeslot_ID)
                    return 0;
                return 1;
            }
        }
    }
}
