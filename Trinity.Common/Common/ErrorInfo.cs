using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity.Common.Common;

namespace Trinity.Common
{

    public class ErrorInfo
    {
        private EnumErrorCodes _errorCode;
        public ErrorInfo(EnumErrorCodes errorCode)
        {
            _errorCode = errorCode;
        }

        public string GetErrorMessage(EnumErrorCodes errorCode)
        {
            switch (errorCode)
            {
                case EnumErrorCodes.FatalError:
                    return "An unknown error has occurred.";

                case EnumErrorCodes.DocumentScannerNull:
                    return "There is no avaiable scanner.";

                case EnumErrorCodes.UnknownError:
                    return "An unknown error has occurred.";

                case EnumErrorCodes.FingerprintNull:
                    return "fingerprint_Template can not be null.";

                case EnumErrorCodes.SmartCardReaderNull:
                    return "SmartCardReader is not connected.";

                case EnumErrorCodes.UserNameNull:
                    return "UserName can not be null.";

                case EnumErrorCodes.NRICNull:
                    return "NRIC can not be null.";

                default:
                    break;
            }
            return "Unknown error.";
        }
    }
}
