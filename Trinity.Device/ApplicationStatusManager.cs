using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Trinity.BE;
using Trinity.DAL;
using Trinity.Device.Util;

namespace Trinity.Device
{
    public class ApplicationStatusManager
    {
        private string _station;
        private EnumApplicationStatus _applicationStatus = EnumApplicationStatus.Error;
        private bool _isInitializing = false;
        private bool _isInitializing_LayerWeb = false;
        private bool _isBusy = false;

        // value: initialization finished (update = true after first device status report)
        private List<DeviceStatus> _lstDevices;

        public List<DeviceStatus> Devices
        {
            get
            {
                return _lstDevices;
            }
        }

        public EnumApplicationStatus ApplicationStatus
        {
            get { return _applicationStatus; }
        }

        public bool IsBusy
        {
            get
            {
                return _isBusy;
            }

            set
            {
                _isBusy = value;

                if (_isBusy)
                {
                    if (_isInitializing)
                    {
                        _isInitializing = false;
                    }
                    if (_isInitializing_LayerWeb)
                    {
                        _isInitializing_LayerWeb = false;
                    }
                }
                else
                {
                    // After first login, if application have any device status is null, update it to disconnected (avoid always initialization)
                    if (GetApplicationStatus() == EnumApplicationStatus.Initialization)
                    {
                        foreach (DeviceStatus device in _lstDevices.Where(item => item.Status == null))
                        {
                            device.Status = new EnumDeviceStatus[] { EnumDeviceStatus.Disconnected };
                            device.Summary = EnumDeviceStatusSumary.Error;
                        }
                    }
                }

                UpdateLEDsLight();
            }
        }

        public void StartInitialization()
        {
            try
            {
                if (!_isInitializing)
                {
                    _isInitializing = true;
                    _isInitializing_LayerWeb = true;
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
        private static volatile ApplicationStatusManager _instance;

        // Uses a syncRoot instance to lock on, rather than locking on the type itself, to avoid deadlocks.
        private static object syncRoot = new Object();

        private ApplicationStatusManager()
        {
            // Initiate LEDs light and open port
            _station = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;

            if (_station == EnumStation.SSK)
            {
                LEDStatusLightingUtil.Instance.OpenPort();
                // Define list devices which application use
                _lstDevices = new List<DeviceStatus>();
                _lstDevices.Add(new DeviceStatus()
                {
                    DeviceID = EnumDeviceId.Camera,
                    DeviceName = string.Empty,
                    Status = null,
                    Summary = EnumDeviceStatusSumary.Error
                });
                _lstDevices.Add(new DeviceStatus()
                {
                    DeviceID = EnumDeviceId.Speaker,
                    DeviceName = string.Empty,
                    Status = null,
                    Summary = EnumDeviceStatusSumary.Error
                });
                _lstDevices.Add(new DeviceStatus()
                {
                    DeviceID = EnumDeviceId.DocumentScanner,
                    DeviceName = string.Empty,
                    Status = null,
                    Summary = EnumDeviceStatusSumary.Error
                });
                _lstDevices.Add(new DeviceStatus()
                {
                    DeviceID = EnumDeviceId.FingerprintScanner,
                    DeviceName = string.Empty,
                    Status = null,
                    Summary = EnumDeviceStatusSumary.Error
                });
                _lstDevices.Add(new DeviceStatus()
                {
                    DeviceID = EnumDeviceId.BarcodeScanner,
                    DeviceName = string.Empty,
                    Status = null,
                    Summary = EnumDeviceStatusSumary.Error
                });
                _lstDevices.Add(new DeviceStatus()
                {
                    DeviceID = EnumDeviceId.SmartCardReader,
                    DeviceName = string.Empty,
                    Status = null,
                    Summary = EnumDeviceStatusSumary.Error
                });
                _lstDevices.Add(new DeviceStatus()
                {
                    DeviceID = EnumDeviceId.ReceiptPrinter,
                    DeviceName = string.Empty,
                    Status = null,
                    Summary = EnumDeviceStatusSumary.Error
                });
                _lstDevices.Add(new DeviceStatus()
                {
                    DeviceID = EnumDeviceId.QueueScreenMonitor,
                    DeviceName = string.Empty,
                    Status = null,
                    Summary = EnumDeviceStatusSumary.Error
                });

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

        public static ApplicationStatusManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (_instance == null)
                            _instance = new ApplicationStatusManager();
                    }
                }

                return _instance;
            }
        }
        #endregion

        private void UpdateLEDsLight()
        {
            try
            {
                LEDStatusLightingUtil ledLightUtil = LEDStatusLightingUtil.Instance;

                if (_station.ToUpper() == EnumStation.SSK)
                {
                    // Get latest application status
                    EnumApplicationStatus newApplicationStatus = GetApplicationStatus();

                    if (newApplicationStatus != _applicationStatus)
                    {
                        _applicationStatus = newApplicationStatus;

                        // Always turn off all LEDs before select which LED(s) to turn on.
                        ledLightUtil.TurnOffAllLEDs();

                        //MessageBox.Show(_applicationStatus.ToString());
                        switch (_applicationStatus)
                        {
                            case EnumApplicationStatus.Initialization:
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
                            case EnumApplicationStatus.Busy:
                                ledLightUtil.SwitchBLUELightOnOff(true);
                                break;
                            default:
                                ledLightUtil.SwitchREDLightOnOff(true);
                                break;
                        }
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

        private EnumApplicationStatus GetApplicationStatus()
        {
            if (_isInitializing)
            {
                return EnumApplicationStatus.Initialization;
            }
            else if (_isBusy)
            {
                return EnumApplicationStatus.Busy;
            }
            else
            {
                // Get list device summary
                List<EnumDeviceStatusSumary> deviceStatusSumaries = _lstDevices.Select(item => item.Summary).ToList();

                // Error status
                if (deviceStatusSumaries.Any(item => item == EnumDeviceStatusSumary.Error))
                {
                    return EnumApplicationStatus.Error;
                }
                // if application have no device disconnected, and have any device status is diffirent connected, update status caution
                // Need to define caution statuses group
                else if (deviceStatusSumaries.Any(item => item != EnumDeviceStatusSumary.Ready))
                {
                    return EnumApplicationStatus.Caution;
                }
                // if all devices are connected and have no caution, update status is ready
                else
                {
                    return EnumApplicationStatus.Ready;
                }
            }
        }

        public void ReportDeviceStatus(EnumDeviceId enumDeviceId, EnumDeviceStatus[] newStatuses)
        {
            try
            {
                DeviceStatus deviceStatus = _lstDevices.FirstOrDefault(item => item.DeviceID == enumDeviceId);
                // Save new status
                deviceStatus.Status = newStatuses;

                // update summary
                // Error status
                if (newStatuses == null || newStatuses.Length == 0 || newStatuses.Contains(EnumDeviceStatus.Disconnected))
                {
                    deviceStatus.Summary = EnumDeviceStatusSumary.Error;
                }
                // if application have no device disconnected, and have any device status is diffirent connected, update status caution
                // Need to define caution statuses group
                else if (newStatuses.Any(item => item != EnumDeviceStatus.Connected))
                {
                    deviceStatus.Summary = EnumDeviceStatusSumary.Caution;
                }
                // if all devices are connected and have no caution, update status is ready
                else
                {
                    deviceStatus.Summary = EnumDeviceStatusSumary.Ready;
                }

                // If application initialization is not fisnished and this device status is not reported yet
                // update device initialization finished
                if (_isInitializing && !_isInitializing_LayerWeb && !_lstDevices.Any(item => item.Status == null))
                {
                    _isInitializing = false;
                }

                UpdateLEDsLight();

                // update local ApplicationDevice_Status
                DAL_DeviceStatus dAL_DeviceStatus = new DAL_DeviceStatus();
                dAL_DeviceStatus.Update((int)enumDeviceId, newStatuses);
                Trinity.SignalR.Client.Instance.DeviceStatusChanged((int)enumDeviceId, newStatuses);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ReportDeviceStatus exception: " + ex.ToString());
            }
        }

        public void LayerWebInitilizationCompleted()
        {
            if (_isInitializing_LayerWeb)
            {
                _isInitializing_LayerWeb = false;

                // If application initialization is not finished
                // and weblayer and all devices are finished
                // update _isInitializing = false
                if (_isInitializing && !_lstDevices.Any(item => item.Status == null))
                {
                    _isInitializing = false;
                }

                UpdateLEDsLight();
            }
        }
    }
}
