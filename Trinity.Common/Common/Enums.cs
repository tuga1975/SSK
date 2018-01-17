﻿using System;
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

public static class EnumSettingStatuses
{
    public const string Archived = "Archived";
    public const string Active = "Active";
    public const string Pending = "Pending";
}

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

public enum EnumAppointmentStatuses
{
    Deleted = -1,
    Pending = 0,
    Booked = 1,
    Reported = 2,
    Completed = 3,
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

    [Custom(Color = "red")]
    public static string Missed = "Missed";
    [Custom(Color = "white")]
    public const string Waiting = "Waiting";
    [Custom(Color = "white")]
    public const string Processing = "Processing";
    [Custom(Color = "red")]
    public const string Errors = "Errors";
    [Custom(Color = "green")]
    public const string Finished = "Finished";
    [Custom(Color = "notrequired")]
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


public static class EnumColors
{
    public const string Red = "#ff0000";
    public const string Yellow = "#ffd800";
    public const string Blue = "#0026ff";
    public const string White = "#ffffff";
    public const string Black = "#000000";
    public const string Green = "#00ff21";

}

public enum EnumDeviceStatuses
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
}

public static class NotificationType
{
    public const string Error = "Error";
    public const string Notification = "Notification";
    public const string Caution = "Caution";
}

public static class EnumStations
{
    public const string SSK = "SSK";
    public const string SSA = "SSA";
    public const string UHP = "UHP";
    public const string APS = "APS";
    public const string HSA = "HSA";
    public const string ESP = "ESP";

    public const string ENROLMENT = "ENROLMENT";
    public const string DUTYOFFICER = "DOFFICER";

    public static List<string> GetListStation()
    {
        return new List<string>() { SSK, SSA, UHP, APS, HSA, ESP };
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
    public const string Deactivate = "Deactivate";
};

public static class EnumOutcome
{
    public const string Processing = "Processing...";
   
};