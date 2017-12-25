using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum EnumScreens
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
public enum EnumAppointmentStatuses
{
    Deleted = -1,
    Pending = 0,
    Booked = 1,
    Reported = 2,
    Completed = 3
}
public enum EnumAbsenceReasons
{
    Medical_Certificate = 0,
    Work_Commitment = 1,
    Family_Matters = 2,
    Other_Reasons = 3,
    No_Valid_Reason = 4,
    No_Supporting_Document = 5
}

public static class EnumQueueStatuses
{
    public const string Missed = "Missed";
    public const string Waiting = "Waiting";
    public const string Processing = "Processing";
}

public static class EnumUserStatuses
{
    public const string Active = "Active";
    public const string Blocked = "Blocked";
}

public static class EnumFrequency
{
    public const string Weekly = "Weekly";
    public const string Biweekly = "Biweekly";
    public const string Monthly = "Monthly";
}

public static class EnumDeviceType
{
    public const string ReceiptPrinter = "ReceiptPrinter";
    public const string FingerprintScanner = "Fingerprint";
    public const string DocumentScanner = "DocumentScanner";
    public const string SmartCardReader = "SmartCardScanner";
    public const string BarcodeScanner = "BarcodeScanner";
    public const string LEDDisplayMonitor = "LEDDisplayMonitor";
    public const string Camera = "Camera";

}



