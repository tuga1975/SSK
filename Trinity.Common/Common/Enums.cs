using System;
using System.Collections.Generic;
using System.Configuration;
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
    Supervisee_NRIC = 8,
    Supervisee_Particulars = 9,
    Login = 10,
    WebcamCapture = 11,
    Authentication_Facial = 12,
    Queue = 13
};

//public static class EnumSettingStatuses
//{
//    public const string Archived = "Archived";
//    public const string Active = "Active";
//    public const string Pending = "Pending";
//}

public static class EnumUserRoles
{
    public const string DutyOfficer = "DutyOfficer";
    public const string SuperAdmin = "SuperAdmin";
    public const string Supervisee = "Supervisee";
    public const string EnrolmentOfficer = "EnrolmentOfficer";
    public const string CaseOfficer = "CaseOfficer";
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

//public enum EnumAppointmentStatuses
//{
//    Deleted = -1,
//    Pending = 0,
//    Booked = 1,
//    Reported = 2,
//    Completed = 3,
//    Absent = 4
//}

public static class EnumAppointmentStatuses
{
    //public const string Deleted = "Deleted";
    public const string Pending = "Pending";
    public const string Booked = "Booked";
    public const string Reported = "Reported";
    //public const string Completed = "Completed";
    public const string Absent = "Absent";
}


public static class EnumQueueStatuses
{

    [Custom(EnumColors.Red,"")]
    public static string Missed = "Missed";
    [Custom(EnumColors.White, "")]
    public const string Waiting = "Waiting";
    [Custom(EnumColors.White, "")]
    public const string Processing = "Processing";
    [Custom(EnumColors.Red, "")]
    public const string Errors = "Errors";
    [Custom(EnumColors.Green, "")]
    public const string Finished = "Finished";
    [Custom(EnumColors.Notrequired, "")]
    public const string NotRequired = "NotRequired";

}

public static class EnumUserStatuses
{
    public const string New = "NEW";
    public const string Enrolled = "ENROLLED";
    public const string Blocked = "BLOCKED";
    public const string ReEnrolled = "RE-ENROLLED";
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

public enum EnumDeviceId
{
    SmartCardReader = 1,
    FingerprintScanner = 2,
    DocumentScanner = 3,
    ReceiptPrinter = 4,
    BarcodeScanner = 5,
    QueueScreenMonitor = 6,
    Camera = 7,
    SmartCardPrinter = 8,
    MUBLabelPrinter = 9,
    UBLabelPrinter = 10,
    TTLabelPrinter = 11,
    Speaker = 12
}


public static class EnumColors
{
    public const string Red = "#ff0000";
    public const string Yellow = "#ffd800";
    public const string Blue = "#0026ff";
    public const string White = "#ffffff";
    public const string Black = "#000000";
    public const string Green = "#00ff21";
    public const string Notrequired = "notrequired";
}

public enum EnumDeviceStatus
{
    Connected = 10,
    //
    // Summary:
    //     Status is not specified.
    None = 0,
    //
    // Summary:
    //     The print queue is paused.
    Paused = 1,
    //
    // Summary:
    //     The printer cannot print due to an error condition.
    Error = 2,
    //
    // Summary:
    //     The print queue is deleting a print job.
    PendingDeletion = 4,
    //
    // Summary:
    //     The paper in the printer is jammed.
    PaperJam = 8,
    //
    // Summary:
    //     The printer does not have, or is out of, the type of paper needed for the current
    //     print job.
    PaperOut = 16,
    //
    // Summary:
    //     The printer is waiting for a user to place print media in the manual feed bin.
    ManualFeed = 32,
    //
    // Summary:
    //     The paper in the printer is causing an unspecified error condition.
    PaperProblem = 64,
    //
    // Summary:
    //     The printer is offline.
    Offline = 128,
    //
    // Summary:
    //     The printer is exchanging data with the print server.
    IOActive = 256,
    //
    // Summary:
    //     The printer is busy.
    Busy = 512,
    //
    // Summary:
    //     The device is printing.
    Printing = 1024,
    //
    // Summary:
    //     The printer's output bin is full.
    OutputBinFull = 2048,
    //
    // Summary:
    //     Status information is unavailable.
    NotAvailable = 4096,
    //
    // Summary:
    //     The printer is waiting for a print job.
    Waiting = 8192,
    //
    // Summary:
    //     The device is doing some kind of work, which need not be printing if the device
    //     is a combination printer, fax machine, and scanner.
    Processing = 16384,
    //
    // Summary:
    //     The printer is initializing.
    Initializing = 32768,
    //
    // Summary:
    //     The printer is warming up.
    WarmingUp = 65536,
    //
    // Summary:
    //     Only a small amount of toner remains in the printer.
    TonerLow = 131072,
    //
    // Summary:
    //     The printer is out of toner.
    NoToner = 262144,
    //
    // Summary:
    //     The printer is unable to print the current page.
    PagePunt = 524288,
    //
    // Summary:
    //     The printer requires user action to correct an error condition.
    UserIntervention = 1048576,
    //
    // Summary:
    //     The printer has no available memory.
    OutOfMemory = 2097152,
    //
    // Summary:
    //     A door on the printer is open.
    DoorOpen = 4194304,
    //
    // Summary:
    //     The printer is in an error state.
    ServerUnknown = 8388608,
    //
    // Summary:
    //     The printer is in power save mode.
    PowerSave = 16777216,
    Disconnected = -1
}
public static class EnumAppConfig
{
    public static bool IsLocal
    {
        get
        {
            if (ConfigurationManager.AppSettings["IsLocal"] == "true")
                return true;
            else
                return false;
        }
    }
    public static bool ByPassCentralizedDB
    {
        get
        {
            if (ConfigurationManager.AppSettings["ByPassCentralizedDB"] == "true")
                return true;
            else
                return false;
        }
    }
    public static string web_api_url
    {
        get
        {
            return ConfigurationManager.AppSettings["web_api_url"];
        }
    }
    public static string DateFormat
    {
        get
        {
            return ConfigurationManager.AppSettings["DateFormat"];
        }
    }
    public static int Card_Expired_Date
    {
        get
        {
            return Convert.ToInt32(ConfigurationManager.AppSettings["Card_Expired_Date"]);
        }
    }
    public static string NotificationServerUrl
    {
        get
        {
            return ConfigurationManager.AppSettings["NotificationServerUrl"];
        }
    }

}
public static class EnumDeviceNames
{

    public static string TTLabelPrinter
    {
        get { return ConfigurationManager.AppSettings["TTLabelPrinterName"]; }
    }

    public static string MUBLabelPrinter
    {
        get { return ConfigurationManager.AppSettings["MUBLabelPrinterName"]; }
    }

    public static string UBLabelPrinter
    {
        get { return ConfigurationManager.AppSettings["UBLabelPrinterName"]; }
    }

    public static string ReceiptPrinter
    {
        get { return ConfigurationManager.AppSettings["ReceiptPrinterName"]; }
    }

    public static string SmartCardContactlessReader
    {
        get { return ConfigurationManager.AppSettings["SmartCardContactlessReaderName"]; }
    }

    public static string SmartCardPrinterContactlessReader
    {
        get { return ConfigurationManager.AppSettings["SmartCardPrinterContactlessReaderName"]; }
    }

    public static string SmartCardPrinterContactReader
    {
        get { return ConfigurationManager.AppSettings["SmartCardPrinterContactReaderName"]; }
    }

    public static string SmartCardPrinterSerialNumber
    {
        get { return ConfigurationManager.AppSettings["SmartCardPrinterSerialNumber"]; }
    }

    public static string SmartCardPrinterName
    {
        get { return ConfigurationManager.AppSettings["SmartCardPrinterName"]; }
    }
}

public static class EnumNotificationTypes
{
    public const string Error = "Error";
    public const string Notification = "Notification";
    public const string Caution = "Caution";
}

public static class EnumStation
{
    public const string SSK = "SSK";
    public const string SSA = "SSA";
    public const string UHP = "UHP";
    public const string APS = "APS";
    public const string HSA = "HSA";
    public const string ESP = "ESP";

    public const string ENROLMENT = "Enrolment";
    public const string DUTYOFFICER = "DutyOfficer";

    public static List<string> GetListStation()
    {
        return new List<string>() { SSK, SSA, UHP, APS, HSA, ESP, ENROLMENT, DUTYOFFICER };
    }

}

public static class EnumLabelType
{
    public const string MUB = "MUB";
    public const string TT = "TT";
    public const string UB = "UB";
};


public static class EnumTimeshift
{
    public const string Morning = "Morning";
    public const string Afternoon = "Afternoon";
    public const string Evening = "Evening";
};
public enum EnumDayOfWeek
{
    Monday = 2,
    Tuesday = 3,
    Wednesday = 4,
    Thursday = 5,
    Friday = 6,
    Saturday = 7,
    Sunday = 8,
}

public static class EnumIssuedCards
{
    public const string Active = "Active";
    public const string Inactive = "Inactive";
};

public static class EnumOutcome
{
    public const string Processing = "Processing...";
    public const string GetQueue = "Get Queue Number";

};

public enum EnumResponseStatuses
{
    /// <summary>
    /// Success
    /// </summary>
    Success = 200,

    /// <summary>
    /// ErrorRequestParam
    /// </summary>
    ErrorRequestParam = 400,

    /// <summary>
    /// Unauthorized
    /// </summary>
    Unauthorized = 401,

    /// <summary>
    /// ErrorSystem
    /// </summary>
    ErrorSystem = 500,
}
public static class EnumResponseMessage
{
    public const string Success = "Success";
    public const string ErrorCommonRequestParam = "The request was unacceptable, often due to missing a required parameter";
    public const string ErrorSystem = "Internal server error";
    public const string NotExistMsg = "This record is not exist in DB";
    public const string ErrorExistMsg = "This record is existing in DB";
    public const string UnauthorizedErrorMsg = "Authorization has been denied for this request";
    public const string SessionTimeOut = "Your session has expired. Please login again";
}
public static class EnumAPIParam
{
    //controller
    public const string Appointment = "Appointment";
    public const string Notification = "Notification";
    public const string User = "User";
    public const string Setting = "Setting";
    public const string QueueNumber = "QueueNumber";
    public const string SettingSystem = "SettingSystem";
    public const string Label = "Label";
    public const string DrugResult = "DrugResult";

    //action
    public const string GetByToday = "GetByToday";
    public const string GetNearest = "GetNearest";
    public const string GetMaximumNumberOfTimeslot = "GetMaximumNumberOfTimeslot";
    public const string GetById = "GetById";
    public const string GetDetailsById = "GetDetailsById";
    public const string GetByUserIdAndDate = "GetByUserIdAndDate";
    public const string GetListByUserId = "GetListByUserId";
    public const string GetListCurrentTimeslot = "GetListCurrentTimeslot";
    public const string UpdateBooktime = "UpdateBooktime";
    public const string CountAbsenceByUserId = "CountAbsenceByUserId";
    public const string GetAbsenceByUserId = "GetAbsenceByUserId";
    public const string UpdateReason = "UpdateReason";
    public const string GetListFromSelectedDate = "GetListFromSelectedDate";
    public const string CountByTimeslot = "CountByTimeslot";
    public const string GetAll = "GetAll";
    public const string GetAllStatistics = "GetAllStatistics";
    public const string CountBookedByTimeslot = "CountBookedByTimeslot";
    public const string CountReportedByTimeslot = "CountReportedByTimeslot";
    public const string CountNoShowdByTimeslot = "CountNoShowdByTimeslot";
    public const string CreateForAllUsers = "CreateForAllUsers";
    public const string GetNearestTimeslot = "GetNearestTimeslot";
    public const string UpdateTimeslot = "UpdateTimeslot";
    public const string GetSettingSystemByYear = "GetSettingSystemByYear";
    public const string UpdateSettingSystem = "UpdateSettingSystem";
    public const string UpdateLabel = "UpdateLabel";
}

public static class EnumPrintStatus
{
    public const string Successful = "Successful";
    public const string Failed = "Failed";
}

public static class EnumMessage
{
    public const string NotConnectCentralized = "Can not connect to Centralized";
    public const string SmartCardIsAlreadyInUse = "This smart card is already in use";
}

public static class EnumQueueOutcomeText
{
    public const string Processing = "Processing...";
    public const string UnconditionalRelease = "Unconditional Release";
    public const string TapSmartCardToContinue = "Tap smart card to continue";
}

public static class EnumUTResult
{
    public const string NEG = "NEG";
    public const string POS = "POS";
}

public enum EnumAbsentReasons
{
    Medical_Certificate = 0,
    Work_Commitment = 1,
    Family_Matters = 2,
    Other_Reasons = 3,
    No_Valid_Reason = 4,
    No_Supporting_Document = 5
}

public enum EnumApplicationStatus
{
    Initialization = 0,
    Ready = 1,
    Caution = 2,
    Error = 3,
    Busy = 4
}

public enum EnumDeviceStatusSumary
{
    Ready = 1,
    Caution = 2,
    Error = 3,
}