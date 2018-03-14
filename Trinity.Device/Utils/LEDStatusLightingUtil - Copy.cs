﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows.Forms;

namespace Trinity.Device.Util
{
    //public class SerialDataReceivedEventArgs2
    //{
    //    public SerialPort SourceObject { get; set; }
    //    public SerialDataReceivedEventArgs Data { get; set; }
    //}
    public enum EnumMUBApplicatorStatus
    {
        NotReady = 0,
        Ready = 1, // Supervisee can place MUB
        Started = 2, // Ready to print
        Finished = 3 // Applicator is finished
    }

    public enum EnumMUBStatus
    {
        Removed = 0, // The MUB has been removed from the holder
        Placed = 1  // The MUB has been placed on the holder
    }

    public enum EnumMUBDoorStatus
    {
        FullyOpen = 0,
        NotFullyOpen = 1,
        FullyClosed = 2,
        NotFullyClosed = 3
    }

    public enum EnumMUBCommands
    {
        Unknown = -1,
        CheckIfApplicatorIsReady = 0,
        CheckIfMUBIsPresent = 1,
        CheckIfApplicatorIsStarted = 2,
        CheckIfApplicatorIsFinished = 3,
        CheckIfMUBIsRemoved = 4,
        CheckIfMUBDoorIsFullyClosed = 5,
        CheckIfMUBDoorIsFullyOpen = 6
    }

    public class LEDStatusLightingUtil
    {
        private int _retryCount = 0;
        private const int _maxRetryCount = 50;
        private EnumMUBApplicatorStatus _previousMUBApplicatorStatus = EnumMUBApplicatorStatus.NotReady;
        public event EventHandler<string> DataReceived;
        //public event EventHandler<string> MUBAutoFlagApplicatorReadyOK;
        public event EventHandler<EnumMUBApplicatorStatus> MUBApplicatorStatusUpdated;
        //public event EventHandler<string> MUBIsPresent;
        //public event EventHandler<string> MUBReadyToPrint;
        //public event EventHandler<string> MUBReadyToRemove;
        public event EventHandler<EnumMUBStatus> MUBStatusUpdated;
        //public event EventHandler<string> MUBDoorFullyClosed;
        public event EventHandler<EnumMUBDoorStatus> MUBDoorStatusChanged;

        public bool _isBusy = false;

        private string _station = null;
        private System.IO.Ports.SerialPort _serialPort = null;
        private System.Timers.Timer _timer_BlueLightFlashing = null;
        private bool _isOn_BlueLightFlashing = true;
        private System.Timers.Timer _timer_YellowLightFlashing = null;
        private bool _isOn_YellowLightFlashing = true;

        #region Singleton Implementation
        // The variable is declared to be volatile to ensure that assignment to the instance variable completes before the instance variable can be accessed
        private static volatile LEDStatusLightingUtil _instance;

        // Uses a syncRoot instance to lock on, rather than locking on the type itself, to avoid deadlocks.
        private static object syncRoot = new Object();

        private LEDStatusLightingUtil()
        {
            int interval = 500;

            // BlueLightFlashing
            _timer_BlueLightFlashing = new System.Timers.Timer(interval);
            _timer_BlueLightFlashing.Enabled = false;
            _timer_BlueLightFlashing.Elapsed += new System.Timers.ElapsedEventHandler(OnBlueLightFlashingTimedEvent);

            // YellowLightFlashing
            _timer_YellowLightFlashing = new System.Timers.Timer(interval);
            _timer_YellowLightFlashing.Enabled = false;
            _timer_YellowLightFlashing.Elapsed += new System.Timers.ElapsedEventHandler(OnYellowLightFlashingTimedEvent);

        }

        public static LEDStatusLightingUtil Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (_instance == null)
                            _instance = new LEDStatusLightingUtil();
                    }
                }

                return _instance;
            }
        }
        #endregion

        private void InitializeSerialPort()
        {
            if (_serialPort == null)
            {
                _serialPort = new System.IO.Ports.SerialPort();
                _serialPort.DataBits = 8;
                _serialPort.ReadTimeout = 1000;
                _serialPort.WriteTimeout = 1000;
                _serialPort.StopBits = System.IO.Ports.StopBits.One;
            }

            _rs232Commands[EnumMUBCommands.CheckIfApplicatorIsReady] = "RD MR3";
            _rs232Commands[EnumMUBCommands.CheckIfMUBIsPresent] = "RD 14";
            _rs232Commands[EnumMUBCommands.CheckIfApplicatorIsStarted] = "RD MR7";
            _rs232Commands[EnumMUBCommands.CheckIfApplicatorIsFinished] = "RD MR15";
            _rs232Commands[EnumMUBCommands.CheckIfMUBIsRemoved] = "RD 14";
            _rs232Commands[EnumMUBCommands.CheckIfMUBDoorIsFullyClosed] = "RD 1";
            _rs232Commands[EnumMUBCommands.CheckIfMUBDoorIsFullyOpen] = "RD 0";
        }

        #region Public functions

        public bool OpenPort()
        {
            InitializeSerialPort();

            try
            {
                if (string.IsNullOrEmpty(_station))
                {
                    _station = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
                }

                string comPort = ConfigurationManager.AppSettings["COMPort"];
                int baudRate = int.Parse(ConfigurationManager.AppSettings["BaudRate"]);
                string parity = ConfigurationManager.AppSettings["Parity"];

                // Check if the port already open
                if (_serialPort.IsOpen)
                {
                    // return "The serial port already open";
                    return false;
                }

                _serialPort.PortName = comPort;
                _serialPort.BaudRate = baudRate;

                if (parity.Equals("None", StringComparison.InvariantCultureIgnoreCase))
                {
                    _serialPort.Parity = System.IO.Ports.Parity.None;
                }
                else if (parity.Equals("Even", StringComparison.InvariantCultureIgnoreCase))
                {
                    _serialPort.Parity = System.IO.Ports.Parity.Even;
                }

                _serialPort.Open();

                _serialPort.DataReceived += _serialPort_DataReceived;

                // Start Communication
                StartCommunication();

                // Reset PLC
                ResetPLC();

                return true;
            }
            catch (IOException ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Return:
        ///     string.empty: Open port successfully
        ///     Not an empty string: Error Message. Open port failed
        /// </summary>
        /// <param name="portName"></param>
        /// <param name="baudRate"></param>
        /// <param name="parity"></param>
        /// <returns></returns>
        public string OpenPort(string station, string portName, int baudRate, string parity)
        {
            InitializeSerialPort();

            try
            {
                if (string.IsNullOrEmpty(station))
                {
                    return "Please select a station: SSK or SSA";
                }
                if (string.IsNullOrEmpty(portName) || string.IsNullOrEmpty(parity))
                {
                    return "Please select port settings";
                }
                else
                {
                    // Check if the port already open
                    if (_serialPort.IsOpen)
                    {
                        return "The serial port already open";
                    }
                    _station = station;
                    _serialPort.PortName = portName;
                    _serialPort.BaudRate = baudRate;
                    if (parity.Equals("None", StringComparison.InvariantCultureIgnoreCase))
                    {
                        _serialPort.Parity = System.IO.Ports.Parity.None;
                    }
                    else if (parity.Equals("Even", StringComparison.InvariantCultureIgnoreCase))
                    {
                        _serialPort.Parity = System.IO.Ports.Parity.Even;
                    }
                    _serialPort.Open();

                    _serialPort.DataReceived += _serialPort_DataReceived;

                    // Start Communication
                    StartCommunication();

                    // Reset PLC
                    ResetPLC();

                    return string.Empty;
                }
            }
            catch (IOException ex)
            {
                MessageBox.Show(ex.Message);
                return ex.Message;
            }
        }

        public string ClosePort()
        {
            if (_serialPort == null)
            {
                return string.Empty;
            }
            try
            {
                // Turn off all LED(s) when the port is closed
                TurnOffAllLEDs();

                _serialPort.DataReceived -= _serialPort_DataReceived;
                _serialPort.Close();
                return string.Empty;
            }
            catch (IOException ioEx)
            {
                return ioEx.Message;
            }
        }

        public bool IsPortOpen
        {
            get
            {
                return _serialPort != null && _serialPort.IsOpen;
            }
        }
        #endregion

        #region LED Control functions
        public void TurnOffAllLEDs()
        {
            if (!IsPortOpen)
            {
                return;
            }
            if (_station.Equals(EnumStation.SSK, StringComparison.InvariantCultureIgnoreCase))
            {
                // stop flashing
                _timer_BlueLightFlashing.Enabled = false;
                _timer_YellowLightFlashing.Enabled = false;

                // turn off leds
                string hexCommand = "00AAFF5501";
                SendCommand(hexCommand);
            }
            else if (_station.Equals(EnumStation.SSA, StringComparison.InvariantCultureIgnoreCase))
            {
                string hexCommand = "";
                string asciiCommand = "";
                //
                // Turn of RED light
                //
                asciiCommand = "WR 605 0";
                hexCommand = ToHEX(asciiCommand);
                SendCommand(hexCommand);
                //
                // Turn of GREEN light
                //
                asciiCommand = "WR 606 0";
                hexCommand = ToHEX(asciiCommand);
                SendCommand(hexCommand);
                //
                // Turn of BLUE light
                //
                asciiCommand = "WR 604 0";
                hexCommand = ToHEX(asciiCommand);
                SendCommand(hexCommand);
            }
        }

        public void SwitchREDLightOnOff(bool isOn)
        {
            if (!IsPortOpen)
            {
                return;
            }
            if (_station.Equals(EnumStation.SSA, StringComparison.InvariantCultureIgnoreCase))
            {
                string hexCommand = "";
                string asciiCommand = "";
                if (isOn)
                {
                    asciiCommand = "WR 605 1";
                }
                else
                {
                    asciiCommand = "WR 605 0";
                }
                hexCommand = ToHEX(asciiCommand);
                SendCommand(hexCommand);
            }
            else if (_station.Equals(EnumStation.SSK, StringComparison.InvariantCultureIgnoreCase))
            {
                string hexCommand = "";
                if (isOn)
                {
                    hexCommand = "00AADF5501";
                }
                else
                {
                    hexCommand = "00AAFF5501";
                }
                SendCommand(hexCommand);
            }
        }

        public void SwitchGREENLightOnOff(bool isOn)
        {
            if (!IsPortOpen)
            {
                return;
            }
            if (_station.Equals(EnumStation.SSA, StringComparison.InvariantCultureIgnoreCase))
            {
                string hexCommand = "";
                string asciiCommand = "";
                if (isOn)
                {
                    asciiCommand = "WR 606 1";
                }
                else
                {
                    asciiCommand = "WR 606 0";
                }
                hexCommand = ToHEX(asciiCommand);
                SendCommand(hexCommand);
            }
            else if (_station.Equals(EnumStation.SSK, StringComparison.InvariantCultureIgnoreCase))
            {
                string hexCommand = "";
                if (isOn)
                {
                    hexCommand = "00AAFB5501";
                }
                else
                {
                    hexCommand = "00AAFF5501";
                }
                SendCommand(hexCommand);
            }
        }

        public void SwitchBLUELightOnOff(bool isOn)
        {
            if (!IsPortOpen)
            {
                return;
            }
            if (_station.Equals(EnumStation.SSA, StringComparison.InvariantCultureIgnoreCase))
            {
                string hexCommand = "";
                string asciiCommand = "";
                if (isOn)
                {
                    asciiCommand = "WR 604 1";
                }
                else
                {
                    asciiCommand = "WR 604 0";
                }
                hexCommand = ToHEX(asciiCommand);
                SendCommand(hexCommand);
            }
            else if (_station.Equals(EnumStation.SSK, StringComparison.InvariantCultureIgnoreCase))
            {
                string hexCommand = "";
                if (isOn)
                {
                    hexCommand = "00AAFD5501";
                }
                else
                {
                    hexCommand = "00AAFF5501";
                }
                SendCommand(hexCommand);
            }
        }

        public void SwitchYELLOWLightOnOff(bool isOn)
        {
            if (!IsPortOpen)
            {
                return;
            }
            if (_station.Equals(EnumStation.SSA, StringComparison.InvariantCultureIgnoreCase))
            {
                string hexCommand = "";
                string asciiCommand = "";
                if (isOn)
                {
                    // Turn on RED light
                    //
                    asciiCommand = "WR 605 1";
                    hexCommand = ToHEX(asciiCommand);
                    SendCommand(hexCommand);
                    //
                    // Turn on GREEN light
                    //
                    asciiCommand = "WR 606 1";
                    hexCommand = ToHEX(asciiCommand);
                    SendCommand(hexCommand);
                }
                else
                {
                    // Turn of RED light
                    //
                    asciiCommand = "WR 605 0";
                    hexCommand = ToHEX(asciiCommand);
                    SendCommand(hexCommand);
                    //
                    // Turn of GREEN light
                    //
                    asciiCommand = "WR 606 0";
                    hexCommand = ToHEX(asciiCommand);
                    SendCommand(hexCommand);
                }
                hexCommand = ToHEX(asciiCommand);
                SendCommand(hexCommand);
            }
            else if (_station.Equals(EnumStation.SSK, StringComparison.InvariantCultureIgnoreCase))
            {
                string hexCommand = "";
                if (isOn)
                {
                    hexCommand = "00AADB5501";
                }
                else
                {
                    hexCommand = "00AAFF5501";
                }
                SendCommand(hexCommand);
            }
        }

        public void StartBLUELightFlashing()
        {
            try
            {
                _isOn_BlueLightFlashing = true;
                _timer_BlueLightFlashing.Enabled = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("LEDStatusLightingUtil.StartBLUELightFlashing exception: " + ex.ToString());
            }
        }

        public void StopBLUELightFlashing()
        {
            try
            {
                _timer_BlueLightFlashing.Enabled = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("LEDStatusLightingUtil.StopBLUELightFlashing exception: " + ex.ToString());
            }
        }

        public void StartYELLOWLightFlashing()
        {
            try
            {
                _isOn_YellowLightFlashing = true;
                _timer_YellowLightFlashing.Enabled = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("LEDStatusLightingUtil.StartYELLOWLightFlashing exception: " + ex.ToString());
            }
        }

        public void StopYELLOWLightFlashing()
        {
            try
            {
                _timer_YellowLightFlashing.Enabled = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("LEDStatusLightingUtil.StopYELLOWLightFlashing exception: " + ex.ToString());
            }
        }

        public void DisplayLedLight_DeviceStatus()
        {
            if (!IsPortOpen)
            {
                return;
            }
            try
            {
                // turn off all leds first
                TurnOffAllLEDs();

                // get device status
                EnumDeviceStatusSumary applicationStatus = new DAL.DAL_DeviceStatus().GetApplicationStatus();

                // ready to use
                if (applicationStatus == EnumDeviceStatusSumary.Ready)
                {
                    if (_isBusy)
                    {
                        // if machine is busy, display blue light
                        SwitchBLUELightOnOff(true);
                        return;
                    }
                    else
                    {
                        // ready to use, display green light
                        SwitchGREENLightOnOff(true);
                        return;
                    }
                }

                // caution
                if (applicationStatus == EnumDeviceStatusSumary.Caution)
                {
                    //SwitchYELLOWLightFlashingOnOff(true);
                }

                // error
                if (applicationStatus == EnumDeviceStatusSumary.Error)
                {
                    SwitchREDLightOnOff(true);
                }
            }
            catch (Exception ex)
            {
                // display error colour
                SwitchREDLightOnOff(true);
            }
        }
        #endregion

        #region MUB functions
        public void InitializeMUBApplicator_Async()
        {
            // Send command to initialize MUB Applicator
            string asciiCommand = "WR MR0 1";
            SendASCIICommand(asciiCommand);
        }

        public void StartMUBApplicator_Async()
        {
            // Send command to start MUB Applicator
            string asciiCommand = "WR MR4 1";
            SendASCIICommand(asciiCommand);
        }

        public void CloseMUBDoor_Async()
        {
            // Send command to close MUB Door
            string asciiCommand = "WR MR509 1";
            SendASCIICommand(asciiCommand);
        }

        public void OpenMUBDoor_Async()
        {
            // Send command to open MUB Door
            string asciiCommand = "WR MR508 1";
            SendASCIICommand(asciiCommand);
        }

        public delegate void WorkCompletedCallback(bool status);

        private WorkCompletedCallback _mubCallback = null;
        private EnumMUBCommands _mubCommand;

        private Dictionary<EnumMUBCommands, string> _rs232Commands = new Dictionary<EnumMUBCommands, string>();

        public void CheckMUBStatus_Async(EnumMUBCommands mubCommand, WorkCompletedCallback callback)
        {
            _retryCount = 0;
            _mubCallback = callback;
            _mubCommand = mubCommand;

            // Check MUB Applicator Status
            this.DataReceived += CheckMUBStatus_Async_Callback;
            string asciiCommand = _rs232Commands[_mubCommand];
            SendASCIICommand(asciiCommand);
        }

        private void CheckMUBStatus_Async_Callback(object sender, string response)
        {
            this.DataReceived -= CheckMUBStatus_Async_Callback;

            if (_mubCommand ==  EnumMUBCommands.CheckIfMUBIsPresent )
            {
                if (response == "0" )
                {
                    _mubCommand = EnumMUBCommands.Unknown;
                    _mubCallback(false);
                    return;
                }
                else
                {
                    _mubCommand = EnumMUBCommands.Unknown;
                    _mubCallback(true);
                    return;
                }
            }
            else if (_mubCommand == EnumMUBCommands.CheckIfMUBIsRemoved)
            {
                if (response == "0")
                {
                    _mubCommand = EnumMUBCommands.Unknown;
                    _mubCallback(true);
                    return;
                }
                else
                {
                    _mubCommand = EnumMUBCommands.Unknown;
                    _mubCallback(false);
                    return;
                }
            }
            else
            {
                if (response== "1" || response.ToLower() =="ok" || response.ToLower() == "yes")
                {
                    _mubCommand = EnumMUBCommands.Unknown;
                    _mubCallback(true);
                    return;
                }
                else
                {
                    if (_retryCount == _maxRetryCount)
                    {
                        _retryCount = 0;
                        _mubCommand = EnumMUBCommands.Unknown;

                        _mubCallback(false);
                        return;
                    }
                    else
                    {
                        _retryCount++;
                        Thread.Sleep(200);

                        this.DataReceived += CheckMUBStatus_Async_Callback;
                        string asciiCommand = _rs232Commands[_mubCommand];
                        SendASCIICommand(asciiCommand);
                    }
                }
            }
            
        }

        #endregion

        #region MUB Labeller Control functions

        public void InitializeMUBApplicator()
        {
            _retryCount = 0;
            this.DataReceived += MUBApplicatorReadyStatus_Received;

            // Send command to initialize MUB Applicator
            string asciiCommand = "WR MR0 1";
            SendASCIICommand(asciiCommand);
        }

        /// <summary>
        /// Auto Flag applicator Ready Status
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="response"></param>
        private void MUBApplicatorReadyStatus_Received(object sender, string response)
        {
            this.DataReceived -= MUBApplicatorReadyStatus_Received;

            // Wait 1 second 
            Thread.Sleep(1000);

            // Then check MUB Applicator Status
            this.DataReceived += MUBApplicatorReadyOKStatus_Received;
            string asciiCommand = "RD MR3";
            SendASCIICommand(asciiCommand);
        }

        /// <summary>
        /// Auto Flag applicator Ready OK
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="response"></param>
        private void MUBApplicatorReadyOKStatus_Received(object sender, string response)
        {
            this.DataReceived -= MUBApplicatorReadyOKStatus_Received;
            if (response == "1" || response.ToLower() == "ok" || response.ToLower() == "yes")
            {
                _previousMUBApplicatorStatus = EnumMUBApplicatorStatus.Ready;
                // MUB Applicator is ready
                // Inform the Supervisee to place the MUB on the holder
                MUBApplicatorStatusUpdated?.Invoke(this, EnumMUBApplicatorStatus.Ready);
            }
            else
            {
                if (_retryCount == _maxRetryCount)
                {
                    _retryCount = 0;
                    _previousMUBApplicatorStatus = EnumMUBApplicatorStatus.NotReady;

                    // MUB Applicator is not ready
                    MUBApplicatorStatusUpdated?.Invoke(this, EnumMUBApplicatorStatus.NotReady);
                }
                else
                {
                    _retryCount++;
                    Thread.Sleep(200);

                    // Then check MUB Applicator Status
                    this.DataReceived += MUBApplicatorReadyOKStatus_Received;
                    string asciiCommand = "RD MR3";
                    SendASCIICommand(asciiCommand);
                }
            }
        }

        public void VerifyMUBPresence()
        {
            this.DataReceived += BottleSensorStatus_Received;
            string asciiCommand = "RD 14";
            SendASCIICommand(asciiCommand);
        }

        private void BottleSensorStatus_Received(object sender, string response)
        {
            this.DataReceived -= BottleSensorStatus_Received;
            if (response == "1" || response.ToLower() == "ok" || response.ToLower() == "yes")
            {
                // Inform Supervisee that the MUB is present and ask his/her confirmation for the next steps
                MUBStatusUpdated?.Invoke(this, EnumMUBStatus.Placed);
            }
            else
            {
                MUBStatusUpdated?.Invoke(this, EnumMUBStatus.Removed);
            }
        }

        /// <summary>
        /// Start MUB Applicator
        /// </summary>
        public void StartMUBApplicator()
        {
            this.DataReceived += MUBApplicatorStartStatus_Received;

            _retryCount = 0;

            // Start MUB applicator
            string asciiCommand = "WR MR4 1";
            SendASCIICommand(asciiCommand);
        }

        private void MUBApplicatorStartStatus_Received(object sender, string response)
        {
            this.DataReceived -= MUBApplicatorStartStatus_Received;

            // Wait 1 second
            Thread.Sleep(1000);

            // Next send command to check MUB Applicator Stand by status
            this.DataReceived += MUBApplicatorStandByStatus_Received;
            string asciiCommand = "RD MR7";
            SendASCIICommand(asciiCommand);
        }

        private void MUBApplicatorStandByStatus_Received(object sender, string response)
        {
            this.DataReceived -= MUBApplicatorStandByStatus_Received;
            if (response == "1" || response.ToLower() == "ok" || response.ToLower() == "yes")
            {
                _previousMUBApplicatorStatus = EnumMUBApplicatorStatus.Started;
                // MUB Applicator is started. Ready to print
                MUBApplicatorStatusUpdated?.Invoke(this, EnumMUBApplicatorStatus.Started);
            }
            else
            {
                if (_retryCount == _maxRetryCount)
                {
                    _retryCount = 0;

                    // MUB Applicator is not started
                    MUBApplicatorStatusUpdated?.Invoke(this, _previousMUBApplicatorStatus);
                }
                else
                {
                    _retryCount++;
                    Thread.Sleep(200);

                    // Check MUB Applicator Stand by status
                    this.DataReceived += MUBApplicatorStandByStatus_Received;
                    string asciiCommand = "RD MR7";
                    SendASCIICommand(asciiCommand);
                }
            }
        }

        public void CheckMUBApplicatorFinishStatus()
        {
            this.DataReceived += MUBApplicatorFinishStatus_Received;

            // Send command to check if MUB Applicator is finished
            string asciiCommand = "RD MR15";
            SendASCIICommand(asciiCommand);
        }

        private void MUBApplicatorFinishStatus_Received(object sender, string response)
        {
            this.DataReceived -= MUBApplicatorFinishStatus_Received;
            if (response == "1" || response.ToLower() == "ok" || response.ToLower() == "yes")
            {
                _previousMUBApplicatorStatus = EnumMUBApplicatorStatus.Finished;

                // MUB Applicator is finished
                // Inform the Supervisee to remove the MUB from the holder
                //MUBReadyToRemove?.Invoke(this, "");
                MUBApplicatorStatusUpdated?.Invoke(this, EnumMUBApplicatorStatus.Finished);
            }
            else
            {
                // MUB Applicator is not finished
                MUBApplicatorStatusUpdated?.Invoke(this, _previousMUBApplicatorStatus);
            }
        }

        //public void CheckIfMUBRemoved()
        //{
        //    this.DataReceived += MUBRemoveStatus_Received;

        //    // Verify Supervisee remove the MUB before close the door.
        //    string asciiCommand = "RD 14";
        //    SendASCIICommand(asciiCommand);
        //}

        //private void MUBRemoveStatus_Received(object sender, string response)
        //{
        //    this.DataReceived -= MUBRemoveStatus_Received;
        //    //MessageBox.Show("MUBRemoveStatus_Received:" + response);
        //    if (response == "0")
        //    {
        //        MUBStatusUpdated?.Invoke(this, "0");

        //        this.DataReceived += MUBDoorClosedStatus_Received;
        //        // Close MUB Door
        //        string asciiCommand = "WR MR509 1";
        //        SendASCIICommand(asciiCommand);

        //    }
        //    else if (response == "1")
        //    {
        //        MUBStatusUpdated?.Invoke(this, "1");
        //    }
        //    else
        //    {

        //        Thread.Sleep(200);
        //        CheckIfMUBRemoved();
        //    }
        //}

        public void CloseMUBDoor()
        {
            // Close MUB Door
            string asciiCommand = "WR MR509 1";
            SendASCIICommand(asciiCommand);

            CheckMUBDoorCloseStatus();
        }

        public void CheckMUBDoorCloseStatus()
        {
            _retryCount = 0;

            // Send command to verify if the MUB Door is fully closed or not
            this.DataReceived += CheckMUBDoorCloseStatus_Received;
            string asciiCommand = "RD 1";
            SendASCIICommand(asciiCommand);
        }

        private void CheckMUBDoorCloseStatus_Received(object sender, string response)
        {
            this.DataReceived -= CheckMUBDoorCloseStatus_Received;
            if (response == "1" || response.ToLower() == "ok" || response.ToLower() == "yes")
            {
                // The MUB Door is fully closed
                MUBDoorStatusChanged?.Invoke(this, EnumMUBDoorStatus.FullyClosed);
            }
            else
            {
                if (_retryCount == 100)
                {
                    _retryCount = 0;
                    // The MUB Door is not fully closed
                    MUBDoorStatusChanged?.Invoke(this, EnumMUBDoorStatus.NotFullyClosed);
                }
                else
                {
                    _retryCount++;
                    Thread.Sleep(200);

                    this.DataReceived += CheckMUBDoorCloseStatus_Received;
                    string asciiCommand = "RD 1";
                    SendASCIICommand(asciiCommand);
                }
            }
        }

        public void OpenMUBDoor()
        {
            // Open MUB Door
            string asciiCommand = "WR MR508 1";
            SendASCIICommand(asciiCommand);

            CheckDoorOpenStatus();
        }

        public void CheckDoorOpenStatus()
        {
            this.DataReceived += CheckDoorOpenStatus_Received;

            // Check Door Open status
            _retryCount = 0;
            string asciiCommand = "RD 0";
            SendASCIICommand(asciiCommand);
        }

        private void CheckDoorOpenStatus_Received(object sender, string response)
        {
            this.DataReceived -= CheckDoorOpenStatus_Received;
            if (response == "1" || response.ToLower() == "ok" || response.ToLower() == "yes")
            {
                // The MUB Door is fully open
                MUBDoorStatusChanged?.Invoke(this, EnumMUBDoorStatus.FullyOpen);
            }
            else
            {
                if (_retryCount == 100)
                {
                    _retryCount = 0;
                    // The MUB Door is not fully open
                    MUBDoorStatusChanged?.Invoke(this, EnumMUBDoorStatus.NotFullyOpen);
                }
                else
                {
                    _retryCount++;
                    Thread.Sleep(200);

                    this.DataReceived += CheckDoorOpenStatus_Received;
                    string asciiCommand = "RD 0";
                    SendASCIICommand(asciiCommand);
                }
            }
        }
        #endregion

        #region Private functions
        private void _serialPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            // Initialize a buffer to hold the received data 
            byte[] buffer = new byte[_serialPort.ReadBufferSize];

            // There is no accurate method for checking how many bytes are read 
            // unless you check the return from the Read method 
            int bytesRead = _serialPort.Read(buffer, 0, buffer.Length);

            //For the example assume the data we are received is ASCII data. 
            string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);

            //SerialDataReceivedEventArgs2 eventArgs = new SerialDataReceivedEventArgs2()
            //{
            //    SourceObject = _serialPort,
            //    Data = e
            //};
            DataReceived?.Invoke(this, response.Trim());
        }

        private string StartCommunication()
        {
            return SendCommand("43520D");
        }

        private string ResetPLC()
        {
            return SendCommand("4D310D");
        }

        private byte[] ToByteArray(String hexString)
        {
            int NumberChars = hexString.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
            }
            return bytes;
        }

        private string ToHEX(string ascii)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in ascii)
                sb.AppendFormat("{0:X2}", (int)c);
            return sb.Append("0D").ToString().Trim();
        }

        public string SendCommand(string hexCommand)
        {
            if (!IsPortOpen)
            {
                return "The serial port is closed.";
            }

            byte[] bytestosend = ToByteArray(hexCommand);

            try
            {
                _serialPort.Write(bytestosend, 0, bytestosend.Length);
                return string.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public string SendASCIICommand(string asciiCommand)
        {
            string hexCommand = ToHEX(asciiCommand);
            return SendCommand(hexCommand);
        }

        private void OnBlueLightFlashingTimedEvent(object sender, ElapsedEventArgs e)
        {
            try
            {
                SwitchBLUELightOnOff(_isOn_BlueLightFlashing);
                System.Diagnostics.Debug.WriteLine("LEDStatusLightingUtil.OnBlueLightFlashingTimedEvent : " + _isOn_BlueLightFlashing.ToString());
                _isOn_BlueLightFlashing = !_isOn_BlueLightFlashing;
            }
            catch (Exception ex)
            {
                Console.WriteLine("LEDStatusLightingUtil.OnBlueLightFlashingTimedEvent exception: " + ex.ToString());
            }
        }

        private void OnYellowLightFlashingTimedEvent(object sender, ElapsedEventArgs e)
        {
            try
            {
                SwitchYELLOWLightOnOff(_isOn_YellowLightFlashing);
                _isOn_YellowLightFlashing = !_isOn_YellowLightFlashing;
            }
            catch (Exception ex)
            {
                Console.WriteLine("LEDStatusLightingUtil.OnYellowLightFlashingTimedEvent exception: " + ex.ToString());
            }
        }
        #endregion

    }
}