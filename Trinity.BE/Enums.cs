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
    Supervisee = 7,
    Supervisee_NRIC = 8
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
    Miss = 7,
    Absence = 8
}
public enum AbsenceReason
{
    Medical_Certificate = 0,
    Work_Commitment = 1,
    Family_Matters = 2,
    Other_Reasons = 3,
    No_Valid_Reason = 4,
    No_Supporting_Document = 5
}

public static class StatusConstant
{
    public const string Miss = "Miss";
    public const string Wait = "Wait";
    public const string Working = "Working";
}

public static class UserStatus
{
    public const string Active = "Active";
    public const string Blocked = "Blocked";
}

public static class Frequency
{
    public const string Weekly = "Weekly";
}


