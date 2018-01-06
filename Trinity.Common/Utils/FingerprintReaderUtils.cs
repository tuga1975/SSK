using Futronic.SDKHelper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Text;
using Trinity.Common.Common;

namespace Trinity.Common
{
    public class FingerprintReaderUtils
    {
        #region Singleton Implementation
        // The variable is declared to be volatile to ensure that assignment to the instance variable completes before the instance variable can be accessed
        private static volatile FingerprintReaderUtils _instance;

        // Uses a syncRoot instance to lock on, rather than locking on the type itself, to avoid deadlocks.
        private static object syncRoot = new Object();

        private FingerprintReaderUtils() { }

        public static FingerprintReaderUtils Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (_instance == null)
                            _instance = new FingerprintReaderUtils();
                    }
                }

                return _instance;
            }
        }
        #endregion

        FutronicVerification _futronicVerification;
        OnVerificationCompleteHandler _onVerificationComplete;

        FutronicIdentification _futronicIdentification;
        Action<bool> _identificationCompleted;
        List<byte[]> _lstFingerprint_Templates;

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

        public void GetDeviceStatus(OnVerificationCompleteHandler onVerificationComplete)
        {
            try
            {
                Debug.WriteLine("OnGetDeviceStatusCompleted is starting, please wait ...");
                var test = new byte[100];
                test[0] = 1;
                _futronicVerification = new FutronicVerification(new byte[100]);

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
                _futronicVerification.OnVerificationComplete += onVerificationComplete;

                // start verification process
                this._futronicVerification.Verification();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetDeviceStatus Exception: ");
                Debug.WriteLine(ex.Message);
            }
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
                    _identificationCompleted(false);
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
                            _identificationCompleted(true);
                        }
                        else
                        {
                            // not found
                            // Not enough overlap minutiae.
                            Debug.WriteLine("not found");
                            _identificationCompleted(false);
                        }
                    }
                    else
                    {
                        // Identification failed.
                        Debug.WriteLine("Identification failed");
                        _identificationCompleted(false);
                    }
                }
            }
            else
            {
                // Can not retrieve user template
                // Error description: FutronicSdkBase.SdkRetCode2Message(nRetCode)
                Debug.WriteLine("Can not retrieve user template");
                _identificationCompleted(false);
            }

            // unregister events
            _futronicIdentification.OnPutOn -= OnPutOn;
            _futronicIdentification.OnTakeOff -= OnTakeOff;
            //_futronicIdentification.UpdateScreenImage += new UpdateScreenImageHandler(this.UpdateScreenImage);
            _futronicIdentification.OnFakeSource -= OnFakeSource;
            _futronicIdentification.OnGetBaseTemplateComplete -= OnGetBaseTemplateComplete;

            _lstFingerprint_Templates = null;
        }
    }
}
