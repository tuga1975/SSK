﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Trinity.Common
{
    public class ExceptionArgs : Exception
    {
        public long ErrorCode { get; set; }
        public string ErrorMessage { get; set; }

        public ExceptionArgs() { }

        public ExceptionArgs(string ErrorMessage)
        {
            this.ErrorMessage = ErrorMessage;
        }

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

    public class MUBTTEventArgs
    {
        private string _name;
        private bool _status;

        public MUBTTEventArgs(string name, bool status)
        {
            _name = name;
            _status = status;
        }

        public string Name
        {
            get
            {
                return _name;
            }
        }

        public bool Status
        {
            get
            {
                return _status;
            }
        }
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
        public const string LOAD_UPDATE_PHOTOS = "LoadUpdatePhotos";
        public const string CONFIRM_CAPTURE_PICTURE = "ConfirmCapturePicture";
        public const string CANCEL_CONFIRM_CAPTURE_PICTURE = "CancelConfirmCapturePicture";
        public const string CANCEL_CAPTURE_PICTURE = "CancelCapturePicture";
        public const string CANCEL_UPDATE_PICTURE = "CancelUpdatePicture";
        public const string SUPERVISEE_DATA_UPDATE_CANCELED = "SuperviseeDataUpdateCanceled";
        public const string OPEN_FINGERPRINT_CAPTURE_FORM = "OpenFingerprintCaptureForm";
        public const string PHOTO_CAPTURE_FAILED = "PhotoCaptureFailed";
        public const string FINGERPRINT_CAPTURE_FAILED = "FingerCaptureFailed";
        public const string ABLE_TO_PRINT_FAILED = "AbleToPrintFailed";
        public const string CANCEL_CAPTURE_FINGERPRINT = "CancelCaptureFingerPrint";
        public const string CANCEL_PRINT_SMARTCARD = "CancelPrintSmartCard";
        public const string CONFIRM_CAPTURE_FINGERPRINT = "ConfirmCaptureFingerprint";
        public const string LOAD_UPDATE_SUPERVISEE_BIODATA_SUCCEEDED = "LoadUpdateSuperviseeBiodata";
        public const string UPDATE_SUPERVISEE_BIODATA = "UpdateSuperviseeBiodata";
        public const string ALERT_MESSAGE = "AlertMessage";
        public const string ABSENCE_MORE_THAN_3 = "AbsenceMoreThan3";
        public const string ABSENCE_LESS_THAN_3 = "AbsenceLessThan3";
        public const string SOMETHING_WENT_WRONG = "SomethingWentWrong";
        public const string LOAD_EDIT_SUPERVISEE_SUCCEEDED = "LoadEditSupervisee";
        public const string FINGERPRINT_FAILED_MORE_THAN_3 = "ScanFingerprintFailMorethan3";

        public const string USER_LOGGED_IN = "USER_LOGGED_IN";
        public const string USER_LOGGED_OUT = "USER_LOGGED_OUT";
        public const string QUEUE_COMPLETED = "QUEUE_COMPLETED";
        public const string DEVICE_STATUS_CHANGED = "DEVICE_STATUS_CHANGED";
        public const string APP_DISCONNECTED = "APP_DISCONNECTED";
    }

    public class EventInfo
    {
        public string Name { get; set; }
        public int Code { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
        public object Source { get; set; }
    }

    public static class NotificationNames
    {
        public const string USER_LOGGED_IN = "USER_LOGGED_IN";
        public const string USER_LOGGED_OUT = "USER_LOGGED_OUT";
        public const string QUEUE_COMPLETED = "QUEUE_COMPLETED";
        public const string DEVICE_STATUS_CHANGED = "DEVICE_STATUS_CHANGED";
        public const string APP_DISCONNECTED = "APP_DISCONNECTED";
        public const string ALERT_MESSAGE = "ALERT_MESSAGE";
        public const string DO_UNBLOCK_SUPERVISEE = "DO_UNBLOCK_SUPERVISEE";
        public const string APPOINTMENT_BOOKED = "APPOINTMENT_BOOKED";
        public const string APPOINTMENT_REPORTED = "APPOINTMENT_REPORTED";
        public const string QUEUE_INSERTED = "QUEUE_INSERTED";
        public const string SSA_COMPLETED = "SSA_COMPLETED";
        public const string SSA_PRINTING_LABEL = "SSA_PRINTING_LABEL";
        public const string BACKEND_API_SEND_DO = "BACKEND_API_SEND_DO";

        public const string SHP_COMPLETED = "SHP_COMPLETED";
        public const string SSP_COMPLETED = "SSP_COMPLETED";
        public const string SSP_ERROR = "SSP_ERROR";
        public const string DO_READMESSAGE = "DO_READMESSAGE";
    }

    public class NotificationInfo
    {
        public string NotificationID { get; set; }
        public string Name { get; set; }
        public string FromUserId { get; set; }
        public string[] ToUserIds { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public string Type { get; set; }
        public string Code { get; set; }
        public object Data { get; set; }
        public object Source { get; set; }
        public string NRIC { get; set; }
        public string UserID { get; set; }
        public string AppointmentID { get; set; }
        public string TimeSlotID { get; set; }
        public string QueueID { get; set; }
        public string Status { get; set; }
        public string notification_code { get; set; }

        public DateTime dateSend { get; set; }
    }

    public class PrintMUBAndTTLabelsEventArgs
    {
        private LabelInfo _labelInfo;
        public PrintMUBAndTTLabelsEventArgs(LabelInfo labeInfo)
        {
            _labelInfo = labeInfo;
        }
        public LabelInfo LabelInfo
        {
            get
            {
                return _labelInfo;
            }
            set
            {
                _labelInfo = value;
            }
        }
    }

    //Print UB
    public class PrintUBLabelsSucceedEventArgs
    {
        private LabelInfo _labelInfo;
        public PrintUBLabelsSucceedEventArgs(LabelInfo labeInfo)
        {
            _labelInfo = labeInfo;
        }
        public LabelInfo LabelInfo
        {
            get
            {
                return _labelInfo;
            }
            set
            {
                _labelInfo = value;
            }
        }
    }
}
