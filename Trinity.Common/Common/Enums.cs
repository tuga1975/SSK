using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.Common.Common
{
    public enum UserRoles
    {
        DutyOfficer = 0,
        Supervisee = 1
    };

    public enum DeviceStatus
    {
        Disconnected = 0,
        Connected = 1,
        Busy = 2
    };

    public class PrinterStatus
    {
        public bool Connected { get; set; }
        public bool None { get; set; }
        public bool Paused { get; set; }
        public bool Error { get; set; }
        public bool PendingDeletion { get; set; }
        public bool PaperJam { get; set; }
        public bool PaperOut { get; set; }
        public bool ManualFeed { get; set; }
        public bool PaperProblem { get; set; }
        public bool Offline { get; set; }
        public bool IOActive { get; set; }
        public bool Busy { get; set; }
        public bool Printing { get; set; }
        public bool OutputBinFull { get; set; }
        public bool NotAvailable { get; set; }
        public bool Waiting { get; set; }
        public bool Processing { get; set; }
        public bool Initializing { get; set; }
        public bool WarmingUp { get; set; }
        public bool TonerLow { get; set; }
        public bool NoToner { get; set; }
        public bool PagePunt { get; set; }
        public bool UserIntervention { get; set; }
        public bool OutOfMemory { get; set; }
        public bool DoorOpen { get; set; }
        public bool ServerUnknown { get; set; }
        public bool PowerSave { get; set; }

        public PrinterStatus()
        {
            Connected = false;
            None = false;
            Paused = false;
            Error = false;
            PendingDeletion = false;
            PaperJam = false;
            PaperOut = false;
            ManualFeed = false;
            PaperProblem = false;
            Offline = false;
            IOActive = false;
            Busy = false;
            Printing = false;
            OutputBinFull = false;
            NotAvailable = false;
            Waiting = false;
            Processing = false;
            Initializing = false;
            WarmingUp = false;
            TonerLow = false;
            NoToner = false;
            PagePunt = false;
            UserIntervention = false;
            OutOfMemory = false;
            DoorOpen = false;
            ServerUnknown = false;
            PowerSave = false;
        }

        public PrinterStatus(System.Printing.PrintQueue printQueue)
        {
            Connected = true;
            None = printQueue.QueueStatus.ToString() == "None";
            Paused = printQueue.IsPaused;
            Error = printQueue.IsInError;
            PendingDeletion = printQueue.IsPendingDeletion;
            PaperJam = printQueue.IsPaperJammed;
            PaperOut = printQueue.IsOutOfPaper;
            ManualFeed = printQueue.IsManualFeedRequired;
            PaperProblem = printQueue.HasPaperProblem;
            Offline = printQueue.IsOffline;
            IOActive = printQueue.IsIOActive;
            Busy = printQueue.IsBusy;
            Printing = printQueue.IsPrinting;
            OutputBinFull = printQueue.IsOutputBinFull;
            NotAvailable = printQueue.IsNotAvailable;
            Waiting = printQueue.IsWaiting;
            Processing = printQueue.IsProcessing;
            Initializing = printQueue.IsInitializing;
            WarmingUp = printQueue.IsWarmingUp;
            TonerLow = printQueue.IsTonerLow;
            NoToner = printQueue.HasToner == true ? false : true;
            PagePunt = printQueue.PagePunt;
            UserIntervention = printQueue.NeedUserIntervention;
            OutOfMemory = printQueue.IsOutOfMemory;
            DoorOpen = printQueue.IsDoorOpened;
            ServerUnknown = printQueue.IsServerUnknown;
            PowerSave = printQueue.IsPowerSaveOn;
        }
    }

    public enum ErrorCodes
    {
        FatalError = 0,
        DocumentScannerNull = -1,
        UnknownError = -2,
        FingerprintNull = -3,
        SmartCardReaderNull = -4,
        UserNameNull = -5,
        NRICNull = -6
    };

    public static class ErrorMessages
    {
        public static string DocumentScannerNull = "There is no avaiable scanner.";
        public static string UnknownError = "An unknown error has occurred.";
        public static string FingerprintNull = "fingerprint_Template can not be null.";
        public static string SmartCardReaderNull = "SmartCardReader is not connected.";
        public static string UserNameNull = "UserName can not be null.";
        public static string NRICNull = "NRIC can not be null.";
    }
}
