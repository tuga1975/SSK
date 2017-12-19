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
        Connected = 1
    };

    public enum ErrorCodes
    {
        FatalError = 0,
        DocumentScannerNull = -1,
        UnknownError = -2,
        FingerprintNull = -3,
        SmartCardReaderNull = -4,
        UserNameNull = -5
    };

    public static class ErrorMessages
    {
        public static string DocumentScannerNull = "There is no avaiable scanner.";
        public static string UnknownError = "An unknown error has occurred.";
        public static string FingerprintNull = "fingerprint_Template can not be null.";
        public static string SmartCardReaderNull = "SmartCardReader is not connected.";
        public static string UserNameNull = "UserName can not be null.";
    }
}
