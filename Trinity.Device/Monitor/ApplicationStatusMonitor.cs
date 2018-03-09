using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity.Device.Util;

namespace Trinity.Device.Monitor
{
    public class ApplicationStatusMonitor
    {
        private string _station;
        private EnumApplicationStatus _applicationStatus;
        private bool _isInitializing = false;
        private bool _isInitializationFinished = false;
        private bool _isInitializationFinished_LayerWeb = false;

        // value: initialization finished (update = true after first device status report)
        private Dictionary<EnumDeviceId, bool> _lstDevices_SSK;

        public EnumApplicationStatus ApplicationStatus
        {
            get { return _applicationStatus; }
        }

        public void StartInitialization()
        {
            try
            {
                if (!_isInitializationFinished)
                {
                    _isInitializing = true;
                    UpdateApplicationStatus(EnumApplicationStatus.Initiation);
                    UpdateLEDsLight();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("StartInitialization exception: " + ex.ToString());
            }
        }

        #region Singleton Implementation
        // The variable is declared to be volatile to ensure that assignment to the instance variable completes before the instance variable can be accessed
        private static volatile ApplicationStatusMonitor _instance;

        // Uses a syncRoot instance to lock on, rather than locking on the type itself, to avoid deadlocks.
        private static object syncRoot = new Object();

        private ApplicationStatusMonitor()
        {
            _station = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;

            if (_station == EnumStation.SSK)
            {
                // Initiate LEDs light and open port
                LEDStatusLightingUtil.Instance.OpenPort();

                // Define list devices which application use
                _lstDevices_SSK = new Dictionary<EnumDeviceId, bool>
                {
                    { EnumDeviceId.Camera, false },
                    { EnumDeviceId.Speaker, false },
                    { EnumDeviceId.DocumentScanner, false },
                    { EnumDeviceId.FingerprintScanner, false },
                    { EnumDeviceId.BarcodeScanner, false },
                    { EnumDeviceId.SmartCardReader, false },
                    { EnumDeviceId.ReceiptPrinter, false },
                    { EnumDeviceId.QueueScreenMonitor, false }
                };

                // start device monitors
                CameraMonitor.Start();
                SpeakerMonitor.Start();
                DocumentScannerMonitor.Start();
                FingerprintReaderMonitor.Start();
                BarcodeScannerMonitor.Start();
                SmartCardReaderMonitor.Start();
                ReceiptPrinterMonitor.Start();
                QueueScreenMonitor.Start();
            }
        }

        public static ApplicationStatusMonitor Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (_instance == null)
                            _instance = new ApplicationStatusMonitor();
                    }
                }

                return _instance;
            }
        }
        #endregion

        public void UpdateApplicationStatus(EnumApplicationStatus status)
        {
            try
            {
                if (_applicationStatus != status)
                {
                    // Update application status
                    _applicationStatus = status;

                    if (status == EnumApplicationStatus.Initiation)
                    {
                        _isInitializing = true;
                        UpdateLEDsLight();
                    }

                    if (status == EnumApplicationStatus.Busy)
                    {
                        UpdateLEDsLight();
                    }

                    // Only update when application recieved device status reporting
                    if (!_isInitializing && _applicationStatus != EnumApplicationStatus.Busy)
                    {
                        UpdateLEDsLight();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("UpdateApplicationStatus exception: " + ex.ToString());
            }
        }

        private void UpdateLEDsLight()
        {
            try
            {
                LEDStatusLightingUtil ledLightUtil = LEDStatusLightingUtil.Instance;

                if (_station.ToUpper() == EnumStation.SSK)
                {
                    // Always turn off all LEDs before select which LED(s) to turn on.
                    ledLightUtil.TurnOffAllLEDs();

                    // Turn on the LEDs
                    switch (_applicationStatus)
                    {
                        case EnumApplicationStatus.Initiation:
                            ledLightUtil.StartBLUELightFlashing();
                            break;
                        case EnumApplicationStatus.Ready:
                            ledLightUtil.SwitchGREENLightOnOff(true);
                            break;
                        case EnumApplicationStatus.Caution:
                            ledLightUtil.StartYELLOWLightFlashing();
                            break;
                        case EnumApplicationStatus.Error:
                            ledLightUtil.SwitchREDLightOnOff(true);
                            break;
                        default:
                            break;
                    }
                }
                else if (_station.ToUpper() == EnumStation.SSA)
                {
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("UpdateLEDsLight exception: " + ex.ToString());
            }
        }

        public void ReportDeviceStatus(EnumDeviceId deviceId, EnumDeviceStatus[] deviceStatuses)
        {
            try
            {
                // If application initialization is not fisnished and this device status is not reported yet
                // update device initialization finished
                if (!_isInitializationFinished && _lstDevices_SSK.FirstOrDefault(item => item.Key == deviceId).Value == false)
                {
                    _lstDevices_SSK[deviceId] = true;

                    UpdateInitializationStatus();
                }

                // Update _applicationStatus
                // Error status
                if (deviceStatuses == null || deviceStatuses.Length == 0 || deviceStatuses.Contains(EnumDeviceStatus.Disconnected))
                {
                    UpdateApplicationStatus(EnumApplicationStatus.Error);
                }
                // if application have no device disconnected, and have any device status is diffirent connected, update status caution
                // Need to define caution statuses group
                else if (deviceStatuses.Any(item => item != EnumDeviceStatus.Connected))
                {
                    UpdateApplicationStatus(EnumApplicationStatus.Caution);
                }
                // if all devices are connected and have no caution, update status is ready
                else
                {
                    UpdateApplicationStatus(EnumApplicationStatus.Ready);
                }

                // Update Application device status table
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ReportDeviceStatus exception: " + ex.ToString());
            }
        }

        private void UpdateInitializationStatus()
        {
            // If application initialization is not finished
            // and weblayer and all devices are finished
            // update _isInitializationFinished = true
            if (!_isInitializationFinished && _isInitializationFinished_LayerWeb && !_lstDevices_SSK.Any(item => item.Value == false))
            {
                _isInitializationFinished = true;
                UpdateLEDsLight();
            }
        }

        public void LayerWebInitilizationCompleted()
        {
            if (!_isInitializationFinished_LayerWeb)
            {
                _isInitializationFinished_LayerWeb = true;
                UpdateInitializationStatus();
            }
        }
    }
}
