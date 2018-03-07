using Futronic.SDKHelper;
using ScanAPIHelper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Trinity.Common;

namespace Trinity.Device.Util
{
    public class FingerprintReaderUtil
    {
        #region Singleton Implementation
        // The variable is declared to be volatile to ensure that assignment to the instance variable completes before the instance variable can be accessed
        private static volatile FingerprintReaderUtil _instance;

        // Uses a syncRoot instance to lock on, rather than locking on the type itself, to avoid deadlocks.
        private static object syncRoot = new Object();

        private FingerprintReaderUtil() { }

        public static FingerprintReaderUtil Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (_instance == null)
                            _instance = new FingerprintReaderUtil();
                    }
                }

                return _instance;
            }
        }
        #endregion

        // for verification
        FutronicVerification _futronicVerification;
        OnVerificationCompleteHandler _onVerificationComplete;

        // for Identification
        FutronicIdentification _futronicIdentification;
        Action<bool> _identificationCompleted;
        List<byte[]> _lstFingerprint_Templates;

        // for get device status
        bool _deviceConnected = false;
        private event OnPutOnHandler OnPutOn_Enrollment;
        private event OnTakeOffHandler OnTakeOff_Enrollment;
        private event UpdateScreenImageHandler UpdateScreenImage_Enrollment;
        private event OnFakeSourceHandler OnFakeSource_Enrollment;
        private event OnEnrollmentCompleteHandler OnEnrollmentComplete_Enrollment;

        // for enrolment
        FutronicEnrollment _futronicEnrollment;

        public FingerprintScannerStartResult StartVerification(OnVerificationCompleteHandler onVerificationComplete, byte[] fingerprint_Template)
        {
            // Create returnValue
            FingerprintScannerStartResult returnValue = new FingerprintScannerStartResult()
            {
                Success = false,
                FailedInfo = null
            };

            try
            {
                if (fingerprint_Template == null)
                {
                    returnValue.FailedInfo = new FailedInfo()
                    {
                        ErrorCode = (int)EnumErrorCodes.FingerprintNull,
                        ErrorMessage = new ErrorInfo().GetErrorMessage(EnumErrorCodes.FingerprintNull)
                    };
                    return returnValue;
                }

                Debug.WriteLine("Verification is starting, please wait ...");
                _onVerificationComplete = onVerificationComplete;

                _futronicVerification = new FutronicVerification(fingerprint_Template);

                // Set control properties
                _futronicVerification.FakeDetection = true;
                _futronicVerification.FFDControl = true;
                _futronicVerification.FARN = 200;
                _futronicVerification.Version = VersionCompatible.ftr_version_compatible;
                _futronicVerification.FastMode = true;
                _futronicVerification.MinMinuitaeLevel = 3;
                _futronicVerification.MinOverlappedLevel = 3;

                // register events
                _futronicVerification.OnPutOn += OnPutOn;
                _futronicVerification.OnTakeOff += OnTakeOff;
                //futronicVerification.UpdateScreenImage += new UpdateScreenImageHandler(this.UpdateScreenImage);
                _futronicVerification.OnFakeSource += OnFakeSource;
                _futronicVerification.OnVerificationComplete += _onVerificationComplete;

                // start verification process
                _futronicVerification.Verification();

                // return value
                returnValue.Success = true;
                return returnValue;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("StartStartVerification Exception: ");
                Debug.WriteLine(ex.Message);

                returnValue.FailedInfo = new FailedInfo()
                {
                    ErrorCode = (int)EnumErrorCodes.UnknownError,
                    ErrorMessage = new ErrorInfo().GetErrorMessage(EnumErrorCodes.UnknownError)
                };

                return returnValue;
            }
        }

        internal void StopVerification()
        {
            // unregister events
            if (_futronicVerification != null)
            {
                _futronicVerification.OnPutOn -= OnPutOn;
                _futronicVerification.OnTakeOff -= OnTakeOff;
                //_futronicVerification.UpdateScreenImage -= UpdateScreenImage;
                _futronicVerification.OnFakeSource -= OnFakeSource;
                _futronicVerification.OnVerificationComplete -= _onVerificationComplete;

                _futronicVerification = null;
            }
        }

        private bool OnFakeSource(FTR_PROGRESS Progress)
        {
            Console.WriteLine("Fake source detected. Continue ...");
            return false;
        }

        private void OnTakeOff(FTR_PROGRESS Progress)
        {
            Console.WriteLine("Take off finger from device, please ...");
        }

        private void OnPutOn(FTR_PROGRESS Progress)
        {
            Console.WriteLine("Put finger into device, please ...");
        }

        /// <summary>
        /// start Verification event to capture connected status, need to be wait xxx miliseconds
        /// </summary>
        /// <returns>device connected's status</returns>
        public EnumDeviceStatuses[] GetDeviceStatus()
        {
            try
            {
                //int defaultInterface = ScanAPIHelper.Device.BaseInterface;
                FTRSCAN_INTERFACE_STATUS[] status = ScanAPIHelper.Device.GetInterfaces();

                //var device = new Device();
                //device.Open();
                // device.Information.DeviceCompatibility == 11 if device name is FS80H
                //device.Close();

                if (status != null && status.Contains(FTRSCAN_INTERFACE_STATUS.FTRSCAN_INTERFACE_STATUS_CONNECTED))
                {
                    // connected
                    return new EnumDeviceStatuses[] { EnumDeviceStatuses.Connected };
                }
                else
                {
                    // disconnected
                    return new EnumDeviceStatuses[] { EnumDeviceStatuses.Disconnected };
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("FingerprintReaderUtils.GetDeviceStatus exception: " + ex.Message);
                return new EnumDeviceStatuses[] { EnumDeviceStatuses.None };
            }
        }

        private void GetDeviceStatus_OnVerificationComplete(bool bSuccess, int nResult, bool bVerificationSuccess)
        {
            _deviceConnected = bSuccess;
        }

        /// <summary>
        /// StartIdentification
        /// </summary>
        /// <param name="lstFingerprint_Templates">list source fingerpint templates</param>
        /// <param name="IdentificationCompleted">callback function after IdentificationCompleted</param>
        public void StartIdentification(List<byte[]> lstFingerprint_Templates, Action<bool> IdentificationCompleted)
        {
            Debug.WriteLine("Programme is loading database, please wait ...");

            // 
            _lstFingerprint_Templates = lstFingerprint_Templates;
            _identificationCompleted = IdentificationCompleted;

            // clear ole instance
            if (_futronicIdentification != null)
            {
                // unregister events
                _futronicIdentification.OnPutOn -= OnPutOn;
                _futronicIdentification.OnTakeOff -= OnTakeOff;
                //_futronicIdentification.UpdateScreenImage += new UpdateScreenImageHandler(this.UpdateScreenImage);
                _futronicIdentification.OnFakeSource -= OnFakeSource;
                _futronicIdentification.OnGetBaseTemplateComplete -= OnGetBaseTemplateComplete;

                // set null
                _futronicIdentification = null;
            }

            // create new instance 
            _futronicIdentification = new FutronicIdentification();

            // Set control property
            _futronicIdentification.FakeDetection = true;
            _futronicIdentification.FFDControl = true;
            _futronicIdentification.FARN = 200;
            _futronicIdentification.Version = VersionCompatible.ftr_version_compatible;
            _futronicIdentification.FastMode = true;
            _futronicIdentification.MinMinuitaeLevel = 3;
            _futronicIdentification.MinOverlappedLevel = 3;

            // register events
            _futronicIdentification.OnPutOn += OnPutOn;
            _futronicIdentification.OnTakeOff += OnTakeOff;
            //_futronicIdentification.UpdateScreenImage += new UpdateScreenImageHandler(this.UpdateScreenImage);
            _futronicIdentification.OnFakeSource += OnFakeSource;
            _futronicIdentification.OnGetBaseTemplateComplete += OnGetBaseTemplateComplete;

            // start identification process
            _futronicIdentification.GetBaseTemplate();
        }

        private void OnGetBaseTemplateComplete(bool bSuccess, int nRetCode)
        {
            // returnValue
            bool success = false;

            // bSuccess = true: get user fingerprint success
            if (bSuccess)
            {
                Debug.WriteLine("Starting identification...");

                // remove null fingerprints
                _lstFingerprint_Templates.RemoveAll(item => item == null);

                // if _lstFingerprint_Templates == null, return
                if (_lstFingerprint_Templates == null)
                {
                    Debug.WriteLine("_lstFingerprint_Templates can not be null");
                    //_identificationCompleted(false);
                }
                else
                {
                    // intitialize base templates
                    int iRecords = 0;
                    FtrIdentifyRecord[] rgRecords = new FtrIdentifyRecord[_lstFingerprint_Templates.Count];

                    foreach (var item in _lstFingerprint_Templates)
                    {
                        rgRecords[iRecords].KeyValue = new byte[10];
                        rgRecords[iRecords].Template = item;
                        iRecords++;
                    }

                    // start identify
                    int nResult = _futronicIdentification.Identification(rgRecords, ref iRecords);

                    if (nResult == FutronicSdkBase.RETCODE_OK)
                    {
                        // iRecords != 1: base templates contain user fingerprint
                        if (iRecords != -1)
                        {
                            Debug.WriteLine("OK");
                            // callback
                            success = true;
                            //_identificationCompleted(true);
                        }
                        else
                        {
                            // not found
                            // Not enough overlap minutiae.
                            Debug.WriteLine("not found");
                            //_identificationCompleted(false);
                        }
                    }
                    else
                    {
                        // Identification failed.
                        Debug.WriteLine("Identification failed");
                        //_identificationCompleted(false);
                    }
                }
            }
            else
            {
                // Can not retrieve user template
                // Error description: FutronicSdkBase.SdkRetCode2Message(nRetCode)
                Debug.WriteLine("Can not retrieve user template");
                //_identificationCompleted(false);
            }

            // unregister events
            _futronicIdentification.OnPutOn -= OnPutOn;
            _futronicIdentification.OnTakeOff -= OnTakeOff;
            //_futronicIdentification.UpdateScreenImage += new UpdateScreenImageHandler(this.UpdateScreenImage);
            _futronicIdentification.OnFakeSource -= OnFakeSource;
            _futronicIdentification.OnGetBaseTemplateComplete -= OnGetBaseTemplateComplete;

            _lstFingerprint_Templates = null;

            // callback
            _identificationCompleted(success);
        }

        public byte[] GetTemplate
        {
            get
            {
                try
                {
                    return _futronicEnrollment.Template;
                }
                catch
                {
                    return new byte[] { };
                }
            }
        }

        public uint GetQuality
        {
            get
            {
                try
                {
                    return _futronicEnrollment.Quality;
                }
                catch
                {
                    return 0;
                }
            }
        }

        public void StartCapture(OnPutOnHandler OnPutOn, OnTakeOffHandler OnTakeOff, UpdateScreenImageHandler UpdateScreenImage, OnFakeSourceHandler OnFakeSource, OnEnrollmentCompleteHandler OnEnrollmentComplete)
        {
            lock (syncRoot)
            {
                //DisposeCapture();
                _futronicEnrollment = new FutronicEnrollment();

                // Set control properties
                _futronicEnrollment.FakeDetection = true;
                _futronicEnrollment.FFDControl = true;
                _futronicEnrollment.FARN = 200;
                _futronicEnrollment.Version = VersionCompatible.ftr_version_compatible;
                _futronicEnrollment.FastMode = true;
                _futronicEnrollment.MIOTControlOff = false;
                _futronicEnrollment.MaxModels = 5;
                _futronicEnrollment.MinMinuitaeLevel = 3;
                _futronicEnrollment.MinOverlappedLevel = 3;


                // register events
                OnPutOn_Enrollment = OnPutOn;
                OnTakeOff_Enrollment = OnTakeOff;
                UpdateScreenImage_Enrollment = UpdateScreenImage;
                OnFakeSource_Enrollment = OnFakeSource;
                OnEnrollmentComplete_Enrollment = OnEnrollmentComplete;

                _futronicEnrollment.OnPutOn += OnPutOn_Enrollment;
                _futronicEnrollment.OnTakeOff += OnTakeOff_Enrollment;
                _futronicEnrollment.UpdateScreenImage += UpdateScreenImage_Enrollment;
                _futronicEnrollment.OnFakeSource += OnFakeSource_Enrollment;
                _futronicEnrollment.OnEnrollmentComplete += OnEnrollmentComplete_Enrollment;

                // start enrollment process
                _futronicEnrollment.Enrollment();
            }
        }

        public void DisposeCapture()
        {
            lock (syncRoot)
            {
                if (_futronicEnrollment != null)
                {
                    _futronicEnrollment.OnPutOn -= OnPutOn_Enrollment;
                    _futronicEnrollment.OnTakeOff -= OnTakeOff_Enrollment;
                    _futronicEnrollment.UpdateScreenImage -= UpdateScreenImage_Enrollment;
                    _futronicEnrollment.OnFakeSource -= OnFakeSource_Enrollment;
                    _futronicEnrollment.OnEnrollmentComplete -= OnEnrollmentComplete_Enrollment;
                    //_futronicEnrollment.Dispose();
                    _futronicEnrollment = null;
                }
            }
        }
    }
}
