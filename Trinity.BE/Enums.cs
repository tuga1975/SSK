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

public enum EnumUserRoles
{
    DutyOfficer = 0,
    Supervisee = 1,
    EnrolmentOfficer = 2,
    CaseOfficer = 3
};

public enum EnumErrorCodes
{
    FatalError = 0,
    DocumentScannerNull = -1,
    UnknownError = -2,
    FingerprintNull = -3,
    SmartCardReaderNull = -4,
    UserNameNull = -5,
    NRICNull = -6
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

public static class EnumDeviceTypes
{
    public const string ReceiptPrinter = "ReceiptPrinter";
    public const string FingerprintScanner = "Fingerprint";
    public const string DocumentScanner = "DocumentScanner";
    public const string SmartCardReader = "SmartCardScanner";
    public const string BarcodeScanner = "BarcodeScanner";
    public const string LEDDisplayMonitor = "LEDDisplayMonitor";
    public const string Camera = "Camera";
}

public enum EnumDeviceStatuses
{
    Connected = 0,
    None = -1,
    Paused = -2,
    Error = -3,
    PendingDeletion = -4,
    PaperJam = -5,
    PaperOut = -6,
    ManualFeed = -7,
    PaperProblem = -8,
    Offline = -9,
    IOActive = -10,
    Busy = -11,
    Printing = -12,
    OutputBinFull = -13,
    NotAvailable = -14,
    Waiting = -15,
    Processing = -16,
    Initializing = -17,
    WarmingUp = -18,
    TonerLow = -19,
    NoToner = -20,
    PagePunt = -21,
    UserIntervention = -22,
    OutOfMemory = -23,
    DoorOpen = -24,
    ServerUnknown = -25,
    PowerSave = -26,
    Disconnected = -27
}



