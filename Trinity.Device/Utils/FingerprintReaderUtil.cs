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
                int defaultInterface = ScanAPIHelper.Device.BaseInterface;
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


                //Debug.WriteLine("GetDeviceStatus is starting, please wait ...");

                //// set default connected status is false
                //_deviceConnected = true;
                //// create default return value
                //EnumDeviceStatuses[] returnValue = new EnumDeviceStatuses[] { EnumDeviceStatuses.Connected };

                //// set timeout for waiting OnVerificationComplete response
                //int milisecondsTimeOut = 1000;

                //// create fingerprint template for verification
                //string strFingerprintTemplate = "k\n\0'wwwwwwwwwwwwwwwwwwwwww\f\v\v\n\t\b\awwwww\r\f\v\n\t\b\b\0;www\f\v\t\b\b\a\b\a\0::www\f\v\b\b\b\a;:9ww\r\r\f\t\b\a\a\0;::8ww\r\f\f\f\t\a\0::97ww\f\v\v\n\t\t\b\a;:986ww\v\n\n\n\n\t\t\b;9876ww\r\f\v\f\f\n\b\b\0:875ww\r\f\f\f\f\n\t\b\a;:865ww\f\f\f\f\f\v\n\b\a:8765ww\v\v\f\f\v\v\n\t\a:7765ww\v\v\f\r\v\v\v\t97654ww\f\f\r\r\f\v\t\086542ww\r\r\r\f\n\a:75421ww\f\v\a95321/ww\f\a\0610/.-ww\r\b91,++++ww\v\f\r\f+(''(**ww\t\t\t\f !#%()ww\t\t\b\n\f\f\r #&(ww\t\t\n\v\v\f\f\r!$(ww\a\t\n\f\r!&wwwwwwwwwwwwwwwwwwwbo*5@cm`lU,CEASq%12y_K=&[Wƒ6Sqc*t2{\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\08?GJKY[akmp“¥°\b%.:EHU^~‹”¦©°µ·º»\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\f\v\0\n\n\n\0\v\n\v\0\r\0\t\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0+wwwwwwwwwwwwwwwwwwww\f\r\f\v\v\t\bwww\f\n\r\f\v\n\n\n\b\0ww\r\v\r\r\v\v\n\n\b\0;ww\f\n\n\t\a;;ww\f\n\t\b\a\0::ww\r\f\n\n\b\a\0::ww\r\r\r\r\r\r\f\v\n\t\a::ww\v\v\f\r\r\r\v\v\n\b;;ww\f\f\r\v\n\b\0::ww\f\r\f\n\b\099ww\v\r\r\r\r\v\b\098ww\f\r\r\r\r\r\v\a97ww\r\r\f\r\r\f\b\086ww\r\f\r\r\t;64ww\r\r\r\n931ww\v\f\r\v4/,ww\n\v\r\0,)'ww\n\n\n\n\f\f\"\"#ww\n\t\b\b\b\t\v ww\t\t\b\b\b\b\t\v\f\rww\a\a\b\b\t\n\v\f\rwww\a\b\n\v\v\f\rwww\b\t\n\v\fwwwwwwwwwwwwwwwwwwwj#;QGRt~~CX1[mlm@DM@\\k7ipN_l,~H)MS#JR\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0227==>>MZcdfhp……‹’•¥¶¸#<@IINuv}•›¢ª­¶º¿¿\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\b\n\0\f\0\n\t\0\a\n\v\t\t\v\0\t\v\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0 )wwwwwwwwwwwwwwwwwwwww\f\v\v\n\n\b\a\awwww\r\f\f\f\f\v\t\b\a\0wwww\r\r\r\r\f\v\n\t\a\0:8ww\r\r\r\f\n\n\t\t\b\0:97ww\r\v\n\n\t\t\a;:97ww\f\n\n\t\b\0;:86ww\f\n\t\b\b\0:975ww\r\r\r\f\v\v\n\t\a\0:965ww\f\f\f\f\f\v\v\t\b\0:865ww\r\r\r\f\v\n\t\0:864ww\r\r\r\f\n\t\a;9764ww\r\v\t;8775ww\r\r\f\t\08765ww\r\r\r\r\t;8654ww\r\r\n;7532ww\f\b95321ww\t7210/ww\n;3/,,-ww\r9-*))*ww##$&(www\f\f!#&wwww\n\t\n!$wwww\t\t\v\"$wwwwwwwwwwwwwwwwwww1j…&<2al=_/FJI[z1F^u-YGW#Vnƒ;Z]z^n''_7\\\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\n4BCHMMN[stx…•¢¦¹\t#)4LPX[`„“–Ÿ©ª°´´¹»\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\n\0\f\v\t\t\v\t\r\0\v\b\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\01wwwwwwwwwwwwwwwwwww\r\f\n\n\t\b\b\a\0:875ww\r\v\n\t\b\a\0;:865ww\f\t\b\a;:9875ww\f\t\b\a:98764ww\r\r\f\v\n\t\b:98665ww\r\r\r\f\v\n\b\a;97655ww\r\f\n\t\b;97544ww\r\v\t\b:87643ww\r\n\b:87753ww\f\b;87653ww\f\b;86532ww\r\n:75321ww\f\a953210ww\t620//.ww\v;1-,,,,ww+('')*+ww!#%()ww\v\v\f\r!#&(ww\v\v\v\v\r $(ww\v\v\v\f%ww\b\t\n\f ww\b\b\b\t\n\v\f\nwww\a\a\b\t\f\f\f\f\v\twwwwwwwwwwwwwwwwwwwO}3)3WcVd>%<B3SARsƒw+3ƒ<`v%?NM{;Spƒg0;B\fvo}\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0'(-344BCSY[]jlyzƒ‰“šž°°°·º\b25>HQqxŽŽž¢¤¦¬®¾¿¿\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\n\0\f\v\0\t\a\t\v\t\0\r\t\0\b\t\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0";
                //byte[] fingerprintTemplate = Encoding.ASCII.GetBytes(strFingerprintTemplate);

                //FutronicVerification futronicVerification_GetDeviceStatus = new FutronicVerification(fingerprintTemplate)
                //{
                //    // Set control properties
                //    FakeDetection = true,
                //    FFDControl = true,
                //    FARN = 200,
                //    Version = VersionCompatible.ftr_version_compatible,
                //    FastMode = true,
                //    MinMinuitaeLevel = 3,
                //    MinOverlappedLevel = 3
                //};

                //// register events
                ////_futronicVerification_GetDeviceStatus.OnPutOn += OnPutOn;
                ////_futronicVerification_GetDeviceStatus.OnTakeOff += OnTakeOff;
                ////_futronicVerification_GetDeviceStatus.UpdateScreenImage += new UpdateScreenImageHandler(this.UpdateScreenImage);
                //futronicVerification_GetDeviceStatus.OnFakeSource += OnFakeSource;
                //futronicVerification_GetDeviceStatus.OnVerificationComplete += GetDeviceStatus_OnVerificationComplete;

                //// start verification process
                //futronicVerification_GetDeviceStatus.Verification();

                //// sleep x seconds, wait for OnVerificationComplete update _deviceConnected
                //System.Threading.Thread.Sleep(milisecondsTimeOut);

                //// if after milisecondsTimeOut GetDeviceStatus_OnVerificationComplete isn't triggered, set return value
                //if (!_deviceConnected)
                //{
                //    returnValue = new EnumDeviceStatuses[] { EnumDeviceStatuses.Disconnected };
                //}

                //// unregister events
                //futronicVerification_GetDeviceStatus.OnFakeSource -= OnFakeSource;
                //futronicVerification_GetDeviceStatus.OnVerificationComplete -= GetDeviceStatus_OnVerificationComplete;
                ////futronicVerification_GetDeviceStatus = null;
                //futronicVerification_GetDeviceStatus.Dispose();
                //return returnValue;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("FingerprintReaderUtils. GetDeviceStatus exception: " + ex.Message);
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
