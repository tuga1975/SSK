using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum NavigatorEnums
{
    Authentication_SmartCard = 0,
    Authentication_Fingerprint = 1,
    Authentication_NRIC = 2,
    BookAppointment = 3,
    Document = 4,
    Notification = 5,
    Profile = 6,
    Supervisee = 7
};
public enum StatusEnums
{
    Delete = -1,
    Deactivate = 0,
    Active = 1,
    Create = 2,
    Open = 3,
    Wait = 4,
    Working = 5,
    Success = 6,
    Miss = 7
}
public enum Frequency
{
    Weekly = 1
}
