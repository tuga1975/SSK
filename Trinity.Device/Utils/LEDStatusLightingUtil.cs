using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using Trinity.Common.Utils;

namespace Trinity.Device.Util
{
    public enum EnumCommands
    {
        Unknown = -1,
        CheckIfMUBApplicatorIsReady = 0,
        CheckIfMUBIsPresent = 1,
        CheckIfMUBApplicatorIsStarted = 2,
        CheckIfMUBApplicatorIsFinished = 3,
        CheckIfMUBIsRemoved = 4,
        CheckIfMUBDoorIsFullyClosed = 5,
        CheckIfMUBDoorIsFullyOpen = 6,
        CheckIfTTApplicatorIsReady = 7,
        CheckIfTTIsPresent = 8,
        CheckIfTTApplicatorIsStarted = 9,
        CheckIfTTApplicatorIsFinished = 10,
        CheckIfTTIsRemoved = 11,
        CheckIfTTDoorIsFullyClosed = 12,
        CheckIfTTDoorIsFullyOpen = 13,
        InitializeMUBApplicator = 14,
        StartMUBApplicator = 15,
        CloseMUBDoor = 16,
        OpenMUBDoor = 17,
        MoveUpMUBRobot = 18,
        MoveDownMUBRobot = 19,
        InitializeTTApplicator = 20,
        StartTTApplicator = 21,
        CloseTTDoor = 22,
        OpenTTDoor = 23,
        MoveUpTTRobot = 24,
        MoveDownTTRobot = 25,
        OpenMUBHolder = 26
    }

    public class LEDStatusLightingUtil
    {
        public event EventHandler<string> OnNewEvent;
        private Dictionary<EnumCommands, string> _rs232Commands = new Dictionary<EnumCommands, string>();
        private Dictionary<EnumCommands, int> _rs232CommandsMaxRetryCount = new Dictionary<EnumCommands, int>();

        private const int _maxRetryCount = 50;
        public event EventHandler<string> DataReceived;
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
            lock (syncRoot)
            {
                if (_serialPort == null)
                {
                    _serialPort = new System.IO.Ports.SerialPort();
                    _serialPort.DataBits = 8;
                    _serialPort.ReadTimeout = 1000;
                    _serialPort.WriteTimeout = 1000;
                    _serialPort.StopBits = System.IO.Ports.StopBits.One;
                }

                // Init MUB READ Commands
                _rs232Commands[EnumCommands.CheckIfMUBApplicatorIsReady] = "RD MR3";
                _rs232CommandsMaxRetryCount[EnumCommands.CheckIfMUBApplicatorIsReady] = 10;

                _rs232Commands[EnumCommands.CheckIfMUBIsPresent] = "RD 14";
                _rs232CommandsMaxRetryCount[EnumCommands.CheckIfMUBIsPresent] = 5;

                _rs232Commands[EnumCommands.CheckIfMUBApplicatorIsStarted] = "RD MR7";
                _rs232CommandsMaxRetryCount[EnumCommands.CheckIfMUBApplicatorIsStarted] = 10;

                _rs232Commands[EnumCommands.CheckIfMUBApplicatorIsFinished] = "RD MR15";
                _rs232CommandsMaxRetryCount[EnumCommands.CheckIfMUBApplicatorIsFinished] = 50;

                _rs232Commands[EnumCommands.CheckIfMUBIsRemoved] = "RD 14";
                _rs232CommandsMaxRetryCount[EnumCommands.CheckIfMUBIsRemoved] = 5;

                _rs232Commands[EnumCommands.CheckIfMUBDoorIsFullyClosed] = "RD 1";
                _rs232CommandsMaxRetryCount[EnumCommands.CheckIfMUBDoorIsFullyClosed] = 50;

                _rs232Commands[EnumCommands.CheckIfMUBDoorIsFullyOpen] = "RD 0";
                _rs232CommandsMaxRetryCount[EnumCommands.CheckIfMUBDoorIsFullyOpen] = 50;

                // Init TT Commands
                _rs232Commands[EnumCommands.CheckIfTTApplicatorIsReady] = "RD MR103";
                _rs232CommandsMaxRetryCount[EnumCommands.CheckIfTTApplicatorIsReady] = 10;

                _rs232Commands[EnumCommands.CheckIfTTIsPresent] = "RD 15";
                _rs232CommandsMaxRetryCount[EnumCommands.CheckIfTTIsPresent] = 5;

                _rs232Commands[EnumCommands.CheckIfTTApplicatorIsStarted] = "RD MR106";
                _rs232CommandsMaxRetryCount[EnumCommands.CheckIfTTApplicatorIsStarted] = 10;

                _rs232Commands[EnumCommands.CheckIfTTApplicatorIsFinished] = "RD MR115";
                _rs232CommandsMaxRetryCount[EnumCommands.CheckIfTTApplicatorIsFinished] = 50;

                _rs232Commands[EnumCommands.CheckIfTTIsRemoved] = "RD 15";
                _rs232CommandsMaxRetryCount[EnumCommands.CheckIfTTIsRemoved] = 5;

                _rs232Commands[EnumCommands.CheckIfTTDoorIsFullyClosed] = "RD 9";
                _rs232CommandsMaxRetryCount[EnumCommands.CheckIfTTDoorIsFullyClosed] = 50;

                _rs232Commands[EnumCommands.CheckIfTTDoorIsFullyOpen] = "RD 8";
                _rs232CommandsMaxRetryCount[EnumCommands.CheckIfTTDoorIsFullyOpen] = 50;

                // Init MUB and TT WRITE commands
                _rs232Commands[EnumCommands.InitializeMUBApplicator] = "WR MR0 1";
                _rs232Commands[EnumCommands.InitializeTTApplicator] = "WR MR100 1";
                _rs232Commands[EnumCommands.StartMUBApplicator] = "WR MR4 1";
                _rs232Commands[EnumCommands.StartTTApplicator] = "WR MR104 1";
                _rs232Commands[EnumCommands.OpenMUBDoor] = "WR MR508 1";
                _rs232Commands[EnumCommands.OpenTTDoor] = "WR MR510 1";
                _rs232Commands[EnumCommands.CloseMUBDoor] = "WR MR509 1";
                _rs232Commands[EnumCommands.CloseTTDoor] = "WR MR511 1";
                _rs232Commands[EnumCommands.MoveDownMUBRobot] = "WR MR501 1";
                _rs232Commands[EnumCommands.MoveDownTTRobot] = "WR MR503 1";
                _rs232Commands[EnumCommands.MoveUpMUBRobot] = "WR MR500 1";
                _rs232Commands[EnumCommands.MoveUpTTRobot] = "WR MR502 1";
                _rs232Commands[EnumCommands.OpenMUBHolder] = "WR MR505 1";

            }
        }

        private void StartCommandHandler()
        {
            _commandThread = new Thread(new ThreadStart(CommandsHandler));
            _commandThread.Start();
        }

        #region Public functions
        private Thread _commandThread = null;

        public bool OpenPort()
        {
            if (string.IsNullOrEmpty(_station))
            {
                _station = Lib.Station;
            }
            string comPort = ConfigurationManager.AppSettings["COMPort"];
            int baudRate = int.Parse(ConfigurationManager.AppSettings["BaudRate"]);
            string parity = ConfigurationManager.AppSettings["Parity"];

            string retValue = OpenPort(_station, comPort, baudRate, parity);
            if (retValue != string.Empty)
            {
                return false;
            }
            else
            {
                return true;
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
                lock (syncRoot)
                {
                    if (string.IsNullOrEmpty(station))
                    {
                        return "Please select a station: ARK or ALK";
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

                        StartCommandHandler();

                        return string.Empty;
                    }
                }
            }
            catch (IOException ex)
            {
                LogManager.Error(ex.ToString());
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
                MessageBox.Show("Error in ClosePort. Details:" + ioEx.Message);
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
            try
            {


                if (!IsPortOpen)
                {
                    return;
                }
                if (_station.Equals(EnumStation.ARK, StringComparison.InvariantCultureIgnoreCase))
                {
                    // stop flashing
                    _timer_BlueLightFlashing.Enabled = false;
                    _timer_YellowLightFlashing.Enabled = false;

                    // turn off leds
                    string hexCommand = "00AAFF5501";
                    SendCommand(hexCommand);
                }
                else if (_station.Equals(EnumStation.ALK, StringComparison.InvariantCultureIgnoreCase))
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
            catch (Exception ex)
            {
                MessageBox.Show("Error in TurnOffAllLEDs. Details: " + ex.Message);
            }
        }

        public void SwitchREDLightOnOff(bool isOn)
        {
            if (!IsPortOpen)
            {
                return;
            }
            if (_station.Equals(EnumStation.ALK, StringComparison.InvariantCultureIgnoreCase))
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
            else if (_station.Equals(EnumStation.ARK, StringComparison.InvariantCultureIgnoreCase))
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
            if (_station.Equals(EnumStation.ALK, StringComparison.InvariantCultureIgnoreCase))
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
            else if (_station.Equals(EnumStation.ARK, StringComparison.InvariantCultureIgnoreCase))
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
            if (_station.Equals(EnumStation.ALK, StringComparison.InvariantCultureIgnoreCase))
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
            else if (_station.Equals(EnumStation.ARK, StringComparison.InvariantCultureIgnoreCase))
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
            if (_station.Equals(EnumStation.ALK, StringComparison.InvariantCultureIgnoreCase))
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
            else if (_station.Equals(EnumStation.ARK, StringComparison.InvariantCultureIgnoreCase))
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

        #region MUB WRITE functions
        public void InitializeMUBApplicator_Async()
        {
            SendCommand_Async(EnumCommands.InitializeMUBApplicator, null);
            //// Send command to initialize MUB Applicator
            //string asciiCommand = "WR MR0 1";
            //SendASCIICommand(asciiCommand);
        }

        public void StartMUBApplicator_Async()
        {
            //// Send command to start MUB Applicator
            //string asciiCommand = "WR MR4 1";
            //SendASCIICommand(asciiCommand);
            SendCommand_Async(EnumCommands.StartMUBApplicator, null);
        }

        public void CloseMUBDoor_Async()
        {
            //// Send command to close MUB Door
            //string asciiCommand = "WR MR509 1";
            //SendASCIICommand(asciiCommand);
            SendCommand_Async(EnumCommands.CloseMUBDoor, null);
        }

        public void OpenMUBDoor_Async()
        {
            //// Send command to open MUB Door
            //string asciiCommand = "WR MR508 1";
            //SendASCIICommand(asciiCommand);
            SendCommand_Async(EnumCommands.OpenMUBDoor, null);

        }

        /// <summary>
        /// Send command 'WR MR505 1' to open bottle holder
        /// </summary>
        public void OpenMUBHolder_Async()
        {
            SendCommand_Async(EnumCommands.OpenMUBHolder, null);
        }

        public void MoveUpMUBRobot_Async()
        {
            //// Send command to open MUB Door
            //string asciiCommand = "WR MR500 1";
            //SendASCIICommand(asciiCommand);
            SendCommand_Async(EnumCommands.MoveUpMUBRobot, null);

        }

        public void MoveDownMUBRobot_Async()
        {
            //// Send command to open MUB Door
            //string asciiCommand = "WR MR501 1";
            //SendASCIICommand(asciiCommand);
            SendCommand_Async(EnumCommands.MoveDownMUBRobot, null);

        }

        #endregion

        #region TT WRITE functions
        public void InitializeTTApplicator_Async()
        {
            //// Send command to initialize TT Applicator
            //string asciiCommand = "WR MR100 1";
            //SendASCIICommand(asciiCommand);
            SendCommand_Async(EnumCommands.InitializeTTApplicator, null);

        }

        public void StartTTApplicator_Async()
        {
            //// Send command to start TT Applicator
            //string asciiCommand = "WR MR104 1";
            //SendASCIICommand(asciiCommand);
            SendCommand_Async(EnumCommands.StartTTApplicator, null);

        }

        public void CloseTTDoor_Async()
        {
            //// Send command to close MUB Door
            //string asciiCommand = "WR MR511 1";
            //SendASCIICommand(asciiCommand);
            SendCommand_Async(EnumCommands.CloseTTDoor, null);

        }

        public void OpenTTDoor_Async()
        {
            //// Send command to open MUB Door
            //string asciiCommand = "WR MR510 1";
            //SendASCIICommand(asciiCommand);
            SendCommand_Async(EnumCommands.OpenTTDoor, null);

        }

        public void MoveUpTTRobot_Async()
        {
            //// Send command to open MUB Door
            //string asciiCommand = "WR MR502 1";
            //SendASCIICommand(asciiCommand);
            SendCommand_Async(EnumCommands.MoveUpTTRobot, null);

        }

        public void MoveDownTTRobot_Async()
        {
            //// Send command to open MUB Door
            //string asciiCommand = "WR MR503 1";
            //SendASCIICommand(asciiCommand);
            SendCommand_Async(EnumCommands.MoveDownTTRobot, null);

        }

        #endregion

        #region MUB & TT READ functions

        public delegate void WorkCompletedCallback(bool response);
        private List<EnumCommands> _waitingCommands = new List<EnumCommands>();
        private EnumCommands _currentCommand = EnumCommands.Unknown;
        private Dictionary<EnumCommands, int> _commandsRetryCount = new Dictionary<EnumCommands, int>();
        private Dictionary<EnumCommands, WorkCompletedCallback> _callbacks = new Dictionary<EnumCommands, WorkCompletedCallback>();
        private int _currentCommandIndex = 0;

        private void GetNextCommand()
        {
            _currentCommand = EnumCommands.Unknown;
            if (_waitingCommands.Count > 0)
            {
                if (_currentCommandIndex >= _waitingCommands.Count - 1)
                {
                    _currentCommandIndex = 0;
                }
                else
                {
                    _currentCommandIndex++;
                }
                _currentCommand = _waitingCommands[_currentCommandIndex];
                OnNewEvent?.Invoke(this, "Get next command:" + _currentCommand);
            }
        }

        private void CompleteCurrentCommand(bool? result)
        {
            try
            {
                lock (syncRoot)
                {
                    _waitingCommands.Remove(_currentCommand);
                    if (_commandsRetryCount.ContainsKey(_currentCommand))
                    {
                        _commandsRetryCount.Remove(_currentCommand);
                    }
                    WorkCompletedCallback callback = null;
                    if (_callbacks.ContainsKey(_currentCommand))
                    {
                        callback = _callbacks[_currentCommand];
                        _callbacks.Remove(_currentCommand);
                    }
                    if (callback != null && result != null)
                    {
                        callback(result.Value);
                    }
                    OnNewEvent?.Invoke(this, "Complete current command " + _currentCommand + " successfully. Result:" + (result != null ? result.Value.ToString() : "null"));
                    _currentCommand = EnumCommands.Unknown;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in CompleteCurrentCommand. Details:" + ex.Message);
            }
        }

        private void CommandsHandler()
        {
            try
            {
                // Start to process new command
                this.DataReceived += SendCommand_Async_Callback;

                OnNewEvent?.Invoke(this, "CommandsHandler is started.");
                while (IsPortOpen)
                {
                    if (_currentCommand == EnumCommands.Unknown)
                    {
                        lock (syncRoot)
                        {
                            if (_currentCommand == EnumCommands.Unknown)
                            {
                                GetNextCommand();
                                if (_currentCommand != EnumCommands.Unknown)
                                {
                                    OnNewEvent?.Invoke(this, "Start to proceed next command:" + _currentCommand);


                                    string asciiCommand = "";

                                    asciiCommand = _rs232Commands[_currentCommand];
                                    if (_commandsRetryCount.ContainsKey(_currentCommand))
                                    {
                                        _commandsRetryCount[_currentCommand]++;
                                    }
                                    else
                                    {
                                        _commandsRetryCount[_currentCommand] = 1;
                                    }
                                    OnNewEvent?.Invoke(this, "Start to send command:" + _currentCommand + ", retry:" + _commandsRetryCount[_currentCommand]);

                                    SendASCIICommand(asciiCommand);
                                }
                            }
                        }
                    }
                    Thread.Sleep(200);
                }
                this.DataReceived -= SendCommand_Async_Callback;
                OnNewEvent?.Invoke(this, "CommandsHandler is stopped.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in CommandsHandler. Details:" + ex.Message);
                StartCommandHandler();
            }
        }

        public void SendCommand_Async(EnumCommands command, WorkCompletedCallback callback)
        {
            try
            {
                lock (syncRoot)
                {
                    if (!_waitingCommands.Contains(command))
                    {
                        OnNewEvent?.Invoke(this, "Add command:" + command + " to waiting list");

                        _commandsRetryCount[command] = 0;
                        _waitingCommands.Add(command);
                        if (callback != null)
                        {
                            _callbacks[command] = callback;
                        }
                    }
                    else
                    {
                        OnNewEvent?.Invoke(this, "Command:" + command + " already exist and will be ignored.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in SendCommand_Async. Details:" + ex.Message);
            }
        }

        private void SendCommand_Async_Callback(object sender, string response)
        {
            try
            {
                lock (syncRoot)
                {
                    OnNewEvent?.Invoke(this, "SendCommand_Async_Callback, response:" + response + ", command:" + _currentCommand);

                    if (_currentCommand == EnumCommands.Unknown)
                    {
                        //MessageBox.Show("Tai sao lai xay ra truong hop nay");
                        return;
                    }
                    WorkCompletedCallback callback = null;
                    if (_callbacks.ContainsKey(_currentCommand))
                    {
                        callback = _callbacks[_currentCommand];
                    }
                    else
                    {
                        OnNewEvent?.Invoke(this, "Found no callback for command " + _currentCommand);

                        CompleteCurrentCommand(null);
                        //MessageBox.Show("Tai sao lai xay ra truong hop nay chu");
                        return;
                    }
                    //OnNewEvent?.Invoke(this, "SendCommand_Async_Callback, response:" + response);


                    if (_currentCommand == EnumCommands.CheckIfMUBIsPresent || _currentCommand == EnumCommands.CheckIfTTIsPresent)
                    {
                        if (response == "0")
                        {
                            CompleteCurrentCommand(false);
                            return;
                        }
                        else
                        {
                            CompleteCurrentCommand(true);
                            return;
                        }
                    }
                    else if (_currentCommand == EnumCommands.CheckIfMUBIsRemoved || _currentCommand == EnumCommands.CheckIfTTIsRemoved)
                    {
                        if (response == "0")
                        {
                            CompleteCurrentCommand(true);
                            return;
                        }
                        else
                        {
                            CompleteCurrentCommand(false);
                            return;
                        }
                    }
                    else
                    {
                        if (response == "1" || response.ToLower() == "ok" || response.ToLower() == "yes")
                        {
                            CompleteCurrentCommand(true);
                            return;
                        }
                        else
                        {
                            if (_commandsRetryCount[_currentCommand] == _maxRetryCount)
                            {
                                CompleteCurrentCommand(false);
                                return;
                            }
                            else
                            {
                                // Retry
                                OnNewEvent?.Invoke(this, "Continue retrying command " + _currentCommand + ", count:" + _commandsRetryCount[_currentCommand]);
                                _currentCommand = EnumCommands.Unknown;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CompleteCurrentCommand(false);
                Thread currentThread = Thread.CurrentThread;
                MessageBox.Show("Error in SendCommand_Async_Callback. Current thread:" + currentThread.Name + ". Details:" + ex.Message);
            }
        }
        ///////////////////////////

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
            lock (syncRoot)
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
