using Futronic.SDKHelper;
using System;
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
                        ErrorCode = (int)ErrorCodes.FingerprintNull,
                        ErrorMessage = ErrorMessages.FingerprintNull
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
                _futronicVerification.OnPutOn += this.OnPutOn;
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
                    ErrorCode = (int)ErrorCodes.UnknownError,
                    ErrorMessage = ErrorMessages.UnknownError
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
    }

    //public class FingerprintReaderUtils_BK
    //{
    //    FutronicIdentification _futronicIdentification;
    //    OnGetBaseTemplateCompleteHandler _onGetBaseTemplateComplete;
    //    FutronicVerification _futronicVerification;
    //    OnVerificationCompleteHandler _onVerificationComplete;

    //    public void StartIdentification(OnVerificationCompleteHandler onVerificationComplete, byte[] fingerprint_Template)
    //    {
    //        Debug.WriteLine("Programme is loading database, please wait ...");
    //        _onGetBaseTemplateComplete = onGetBaseTemplateComplete;
    //        //SSKEntities sskEntities = new SSKEntities();
    //        //_users = sskEntities.Fingerprints.ToList();
    //        //if (_users.Count == 0)
    //        //{
    //        //    Console.WriteLine("Users not found. Please, run enrollment process first.");
    //        //    return;
    //        //}

    //        _futronicIdentification = new FutronicIdentification();

    //        // Set control property
    //        _futronicIdentification.FakeDetection = true;
    //        _futronicIdentification.FFDControl = true;
    //        _futronicIdentification.FARN = 200;
    //        _futronicIdentification.Version = VersionCompatible.ftr_version_compatible;
    //        _futronicIdentification.FastMode = true;
    //        _futronicIdentification.MinMinuitaeLevel = 3;
    //        _futronicIdentification.MinOverlappedLevel = 3;

    //        // register events
    //        _futronicIdentification.OnPutOn += OnPutOn;
    //        _futronicIdentification.OnTakeOff += OnTakeOff;
    //        //_futronicIdentification.UpdateScreenImage += new UpdateScreenImageHandler(this.UpdateScreenImage);
    //        _futronicIdentification.OnFakeSource += OnFakeSource;
    //        _futronicIdentification.OnGetBaseTemplateComplete += _onGetBaseTemplateComplete;

    //        // start identification process
    //        _futronicIdentification.GetBaseTemplate();
    //    }

    //    public void StartVerification(OnVerificationCompleteHandler onVerificationComplete, byte[] fingerprint_Template)
    //    {
    //        if (fingerprint_Template == null)
    //        {
    //            return;
    //        }

    //        Debug.WriteLine("Verification is starting, please wait ...");
    //        _onVerificationComplete = onVerificationComplete;

    //        _futronicVerification = new FutronicVerification(fingerprint_Template);

    //        // Set control properties
    //        _futronicVerification.FakeDetection = true;
    //        _futronicVerification.FFDControl = true;
    //        _futronicVerification.FARN = 200;
    //        _futronicVerification.Version = VersionCompatible.ftr_version_compatible;
    //        _futronicVerification.FastMode = true;
    //        _futronicVerification.MinMinuitaeLevel = 3;
    //        _futronicVerification.MinOverlappedLevel = 3;

    //        // register events
    //        _futronicVerification.OnPutOn += this.OnPutOn;
    //        _futronicVerification.OnTakeOff += OnTakeOff;
    //        //futronicVerification.UpdateScreenImage += new UpdateScreenImageHandler(this.UpdateScreenImage);
    //        _futronicVerification.OnFakeSource += OnFakeSource;
    //        _futronicVerification.OnVerificationComplete += _onVerificationComplete;

    //        // start verification process
    //        _futronicVerification.Verification();
    //    }

    //    public bool VerificationResult(bool bSuccess, int nRetCode, bool bVerificationSuccess)
    //    {
    //        StringBuilder szResult = new StringBuilder();
    //        bool returnValue = false;
    //        if (bSuccess)
    //        {
    //            if (bVerificationSuccess)
    //            {
    //                szResult.Append("Verification is successful.");
    //                returnValue = true;
    //            }
    //            else
    //            {
    //                szResult.Append("Verification failed.");
    //            }
    //        }
    //        else
    //        {
    //            szResult.Append("Verification process failed.");
    //            szResult.Append("Error description: ");
    //            szResult.Append(FutronicSdkBase.SdkRetCode2Message(nRetCode));
    //        }

    //        Debug.WriteLine(szResult);

    //        // unregister events
    //        _futronicVerification.OnPutOn -= OnPutOn;
    //        _futronicVerification.OnTakeOff -= OnTakeOff;
    //        //_futronicVerification.UpdateScreenImage -= UpdateScreenImage;
    //        _futronicVerification.OnFakeSource -= OnFakeSource;
    //        _futronicVerification.OnVerificationComplete -= _onVerificationComplete;

    //        _futronicVerification = null;
    //        return returnValue;
    //    }

    //    [HandleProcessCorruptedStateExceptions]
    //    public bool IdentificationResult(bool bSuccess, int nRetCode, byte[] fingerprint_Template)
    //    {
    //        StringBuilder szMessage = new StringBuilder();
    //        bool returnValue = false;

    //        if (bSuccess)
    //        {
    //            Console.WriteLine("Starting identification...");

    //            int iRecords = 0;
    //            int nResult;

    //            FtrIdentifyRecord[] ftrIdentifyRecords = new FtrIdentifyRecord[1];
    //            ftrIdentifyRecords[0] = new FtrIdentifyRecord()
    //            {
    //                KeyValue = Guid.NewGuid().ToByteArray(),
    //                Template = fingerprint_Template
    //            };
                
    //            nResult = _futronicIdentification.Identification(ftrIdentifyRecords, ref iRecords);
    //            if (nResult == FutronicSdkBase.RETCODE_OK)
    //            {
    //                szMessage.Append("Identification process complete. User: ");
    //                if (iRecords != -1)
    //                {
    //                    szMessage.Append("found");
    //                    returnValue = true;
    //                }
    //                else
    //                {
    //                    szMessage.Append("not found");
    //                    returnValue = false;
    //                }
    //            }
    //            else
    //            {
    //                szMessage.Append("Identification failed.");
    //                szMessage.Append(FutronicSdkBase.SdkRetCode2Message(nResult));
    //                returnValue = false;
    //            }
    //        }
    //        else
    //        {
    //            szMessage.Append("Can not retrieve base template.");
    //            szMessage.Append("Error description: ");
    //            szMessage.Append(FutronicSdkBase.SdkRetCode2Message(nRetCode));
    //        }
    //        Debug.WriteLine(szMessage);

    //        // unregister events
    //        _futronicIdentification.OnPutOn -= OnPutOn;
    //        _futronicIdentification.OnTakeOff -= OnTakeOff;
    //        //_futronicIdentification.UpdateScreenImage -= new UpdateScreenImageHandler(this.UpdateScreenImage);
    //        _futronicIdentification.OnFakeSource -= OnFakeSource;
    //        _futronicIdentification.OnGetBaseTemplateComplete -= _onGetBaseTemplateComplete;

    //        _futronicIdentification = null;

    //        // set FingerprintMonitor status
    //        Trinity.Common.Monitor.FingerprintMonitor.IdentificationStarted = false;

    //        // return
    //        return returnValue;
    //    }

    //    private bool OnFakeSource(FTR_PROGRESS Progress)
    //    {
    //        Console.WriteLine("Fake source detected. Continue ...");
    //        return false;
    //    }

    //    private void OnTakeOff(FTR_PROGRESS Progress)
    //    {
    //        Console.WriteLine("Take off finger from device, please ...");
    //    }

    //    private void OnPutOn(FTR_PROGRESS Progress)
    //    {
    //        Console.WriteLine("Put finger into device, please ...");
    //    }
    //}
}
