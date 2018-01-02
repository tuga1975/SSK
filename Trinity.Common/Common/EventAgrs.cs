using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Trinity.Common
{
    public class ExceptionArgs
    {
        public long ErrorCode { get; set; }
        public string ErrorMessage { get; set; }

        public ExceptionArgs() { }

        public ExceptionArgs(FailedInfo failedInfo)
        {
            ErrorCode = failedInfo.ErrorCode;
            ErrorMessage = failedInfo.ErrorMessage;
        }
    }

    public class GetDeviceStatusCompletedArgs
    {
        public bool IsConnected { get; set; }
    }

    public class SmartCardReaderConnectedArgs
    {
        public List<string> NewReaders { get; set; }
        public List<string> Readers { get; set; }
    }

    public class ShowMessageEventArgs
    {
        private string _message;
        private string _caption;
        private MessageBoxButtons _button;
        private MessageBoxIcon _icon;

        public ShowMessageEventArgs(string message, string caption, MessageBoxButtons button, MessageBoxIcon icon)
        {
            _message = message;
            _caption = caption;
            _button = button;
            _icon = icon;
        }

        public string Message { get => _message; set => _message = value; }
        public string Caption { get => _caption; set => _caption = value; }
        public MessageBoxButtons Button { get => _button; set => _button = value; }
        public MessageBoxIcon Icon { get => _icon; set => _icon = value; }
    }
    public class NavigateEventArgs
    {
        private NavigatorEnums _navigatorEnum;
        public NavigateEventArgs(NavigatorEnums navigatorEnum)
        {
            _navigatorEnum = navigatorEnum;
        }
        public NavigatorEnums navigatorEnum
        {
            get
            {
                return _navigatorEnum;
            }
            set
            {
                _navigatorEnum = value;
            }
        }
    }

    public class NRICEventArgs
    {
        private string _message;
        public NRICEventArgs(string message)
        {
            _message = message;
        }
        public string Message
        {
            get
            {
                return _message;
            }
            set
            {
                _message = value;
            }
        }
    }

    public class LoginEventArgs
    {
        private int _errorCode;
        private string _errorMsg;

        public LoginEventArgs(int errorCode, string errorMsg)
        {
            _errorCode = errorCode;
            _errorMsg = errorMsg;
        }

        public int ErrorCode { get => _errorCode; set => _errorCode = value; }
        public string ErrorMsg { get => _errorMsg; set => _errorMsg = value; }
    }

    public static class EventNames
    {
        public const string LOGIN_FAILED = "LoginFailed";
        public const string LOGIN_SUCCEEDED = "LoginSucceeded";
        public const string LOGOUT_SUCCEEDED = "LogoutSucceeded";
        public const string GET_LIST_SUPERVISEE_SUCCEEDED = "GetListSuperviseeSucceeded";
        public const string GET_LIST_SUPERVISEE_FAILED = "GetListSuperviseeSFailed";
        public const string OPEN_PICTURE_CAPTURE_FORM = "OpenPictureCaptureForm";
        public const string OPEN_PICTURE_CAPTURE_FORM_FAILED = "OpenPictureCaptureFormFailed";
        public const string CAPTURE_PICTURE = "CapturePicture";
        public const string CONFIRM_CAPTURE_PICTURE = "ConfirmCapturePicture";
        public const string CANCEL_CONFIRM_CAPTURE_PICTURE = "CancelConfirmCapturePicture";
        public const string CANCEL_CAPTURE_PICTURE = "CancelCapturePicture";
        public const string OPEN_FINGERPRINT_CAPTURE_FORM = "OpenFingerprintCaptureForm";
        public const string PHOTO_CAPTURE_FAILED = "PhotoCaptureFailed";
        public const string FINGERPRINT_CAPTURE_FAILED = "FingerCaptureFailed";
        public const string ABLE_TO_PRINT_FAILED = "AbleToPrintFailed";
        public const string CANCEL_CAPTURE_FINGERPRINT = "CancelCaptureFingerPrint";
        public const string CANCEL_PRINT_SMARTCARD = "CancelPrintSmartCard";
        public const string CONFIRM_CAPTURE_FINGERPRINT = "ConfirmCaptureFingerprint";
    }

    public class EventInfo
    {
        public string Name { get; set; }
        public int Code { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
        public object Source { get; set; }
    }
}
