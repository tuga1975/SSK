﻿using System;
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
        public string Color(string Station)
        {
            QueueDetail detail = this.QueueDetails.FirstOrDefault(c => c.Station == EnumStation.ARK);
            if (detail != null)
            {
                if (Station == EnumStation.ARK)
                {
                    if (detail.Status == EnumQueueStatuses.Errors)
                    {
                        return EnumColors.Red;
                    }
                    else
                    {
                        return EnumColors.Green;
                    }
                }
                else if (Station == EnumStation.APS)
                {
                    if (this.Appointment_ID.HasValue)
                    {
                        return EnumColors.Green;
                    }
                    else
                    {
                        return EnumColors.Notrequired;
                    }
                }
            }
            return EnumColors.Grey;
        }
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
        public string Type
        {
            get
            {
                if (this.Queue_ID == Guid.Empty)
                {
                    return "red";
                }
                else if (!this.Appointment_ID.HasValue || (this.Appointment_ID.HasValue && this.Appointment.Timeslot_ID != this.Timeslot_ID) || (this.CreatedTime <= DateTime.Now.Date.Add(this.Timeslot.StartTime.Value)) || (this.Appointment_ID.HasValue && this.Created_By != this.Appointment.UserId))
                {
                    return "blue";
                }
                return string.Empty;
            }
        }
    }
}
