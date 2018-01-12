using Futronic.SDKHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Trinity.Common;
using Trinity.Common.Common;
using Trinity.Common.Monitor;

public class FingerprintCapture
{

    #region Singleton Implementation
    private static volatile FingerprintCapture _instance;
    private static object syncRoot = new Object();
    private FingerprintCapture()
    {

    }

    public static FingerprintCapture Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (syncRoot)
                {
                    if (_instance == null)
                        _instance = new FingerprintCapture();
                }
            }
            return _instance;
        }
    }
    #endregion

    private event OnPutOnHandler OnPutOn;
    private event OnTakeOffHandler OnTakeOff;
    private event UpdateScreenImageHandler UpdateScreenImage;
    private event OnFakeSourceHandler OnFakeSource;
    private event OnEnrollmentCompleteHandler OnEnrollmentComplete;
    private FutronicEnrollment _futronicEnrollment;
    public void StartCapture(OnPutOnHandler OnPutOn, OnTakeOffHandler OnTakeOff, UpdateScreenImageHandler UpdateScreenImage, OnFakeSourceHandler OnFakeSource, OnEnrollmentCompleteHandler OnEnrollmentComplete)
    {
        Dispose();
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

        this.OnPutOn = OnPutOn;
        this.OnTakeOff = OnTakeOff;
        this.UpdateScreenImage = UpdateScreenImage;
        this.OnFakeSource = OnFakeSource;
        this.OnEnrollmentComplete = OnEnrollmentComplete;

        _futronicEnrollment.OnPutOn += OnPutOn;
        _futronicEnrollment.OnTakeOff += OnTakeOff;
        _futronicEnrollment.UpdateScreenImage += new UpdateScreenImageHandler(UpdateScreenImage);
        _futronicEnrollment.OnFakeSource += OnFakeSource;
        _futronicEnrollment.OnEnrollmentComplete += OnEnrollmentComplete;

        // start enrollment process
        _futronicEnrollment.Enrollment();
    }
    public void Dispose()
    {
        if (_futronicEnrollment != null)
        {
            _futronicEnrollment.OnPutOn -= OnPutOn;
            _futronicEnrollment.OnTakeOff -= OnTakeOff;
            _futronicEnrollment.UpdateScreenImage -= new UpdateScreenImageHandler(UpdateScreenImage);
            _futronicEnrollment.OnFakeSource -= OnFakeSource;
            _futronicEnrollment.OnEnrollmentComplete -= OnEnrollmentComplete;
            _futronicEnrollment.Dispose();
        }
    }
}
