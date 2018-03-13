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
        CheckIfMUBDoorIsFullyOpen = 6
    }

    public enum EnumMUBCommands
    {
        Unknown = -1,
        CheckIfMUBApplicatorIsReady = 0,
        CheckIfMUBIsPresent = 1,
        CheckIfMUBApplicatorIsStarted = 2,
        CheckIfMUBApplicatorIsFinished = 3,
        CheckIfMUBIsRemoved = 4,
        CheckIfMUBDoorIsFullyClosed = 5,
        CheckIfMUBDoorIsFullyOpen = 6
    }

    public enum EnumTTCommands
    {
        Unknown = -1,
        CheckIfTTApplicatorIsReady = 0,
        CheckIfTTIsPresent = 1,
        CheckIfTTApplicatorIsStarted = 2,
        CheckIfTTApplicatorIsFinished = 3,
        CheckIfTTIsRemoved = 4,
        CheckIfTTDoorIsFullyClosed = 5,
        CheckIfTTDoorIsFullyOpen = 6
    }

    public class LEDStatusLightingUtil
    {
        private Dictionary<EnumCommands, string> _rs232Commands = new Dictionary<EnumCommands, string>();

        private int _mubRetryCount = 0;
        private int _ttRetryCount = 0;
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
            if (_serialPort == null)
            {
                _serialPort = new System.IO.Ports.SerialPort();
                _serialPort.DataBits = 8;
                _serialPort.ReadTimeout = 1000;
                _serialPort.WriteTimeout = 1000;
                _serialPort.StopBits = System.IO.Ports.StopBits.One;                
            }

            // Init MUB Commands
            _rs232MUBCommands[EnumMUBCommands.CheckIfMUBApplicatorIsReady] = "RD MR3";
            _rs232MUBCommands[EnumMUBCommands.CheckIfMUBIsPresent] = "RD 14";
            _rs232MUBCommands[EnumMUBCommands.CheckIfMUBApplicatorIsStarted] = "RD MR7";
            _rs232MUBCommands[EnumMUBCommands.CheckIfMUBApplicatorIsFinished] = "RD MR15";
            _rs232MUBCommands[EnumMUBCommands.CheckIfMUBIsRemoved] = "RD 14";
            _rs232MUBCommands[EnumMUBCommands.CheckIfMUBDoorIsFullyClosed] = "RD 1";
            _rs232MUBCommands[EnumMUBCommands.CheckIfMUBDoorIsFullyOpen] = "RD 0";

            // Init TT Commands
            _rs232TTCommands[EnumTTCommands.CheckIfTTApplicatorIsReady] = "RD MR103";
            _rs232TTCommands[EnumTTCommands.CheckIfTTIsPresent] = "RD 15";
            _rs232TTCommands[EnumTTCommands.CheckIfTTApplicatorIsStarted] = "RD MR106";
            _rs232TTCommands[EnumTTCommands.CheckIfTTApplicatorIsFinished] = "RD MR115";
            _rs232TTCommands[EnumTTCommands.CheckIfTTIsRemoved] = "RD 15";
            _rs232TTCommands[EnumTTCommands.CheckIfTTDoorIsFullyClosed] = "RD 9";
            _rs232TTCommands[EnumTTCommands.CheckIfTTDoorIsFullyOpen] = "RD 8";
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

                Thread thread = new Thread(new ThreadStart(CommandsHandler));
                thread.Start();

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

        public void MoveUpMUBRobot_Async()
        {
            // Send command to open MUB Door
            string asciiCommand = "WR MR500 1";
            SendASCIICommand(asciiCommand);
        }

        public void MoveDownMUBRobot_Async()
        {
            // Send command to open MUB Door
            string asciiCommand = "WR MR501 1";
            SendASCIICommand(asciiCommand);
        }

        public delegate void MUBWorkCompletedCallback(bool status);

        private MUBWorkCompletedCallback _mubCallback = null;
        private EnumMUBCommands _mubCommand;
        private Dictionary<EnumMUBCommands, string> _rs232MUBCommands = new Dictionary<EnumMUBCommands, string>();
        private Dictionary<EnumTTCommands, string> _rs232TTCommands = new Dictionary<EnumTTCommands, string>();

        private List<EnumCommands> _waitingCommands = new List<EnumCommands>();
        private EnumCommands _currentCommand = EnumCommands.Unknown;
        private Dictionary<EnumCommands, int> _commandsRetryCount = new Dictionary<EnumCommands, int>();
        private void CommandsHandler()
        {
            while (true)
            {
                if (_currentCommand ==  EnumCommands.Unknown)
                {
                    if (_waitingCommands.Count > 0)
                    {
                        _currentCommand = _waitingCommands[0];
                        //_waitingCommands.RemoveAt(0);

                        // Start to process new command
                        this.DataReceived += SendCommand_Callback;
                        string asciiCommand = _rs232Commands[_currentCommand];
                        SendASCIICommand(asciiCommand);
                        _commandsRetryCount[_currentCommand] = 0;
                    }
                }

                Thread.Sleep(200);
            }
        }

        private void SendCommand(EnumCommands command)
        {
            _waitingCommands.Add(command);
        }
        private void SendCommand_Callback(object sender, string response)
        {
            this.DataReceived -= SendCommand_Callback;

            if (_currentCommand == EnumCommands.CheckIfMUBIsPresent)
            {
                if (response == "0")
                {
                    _currentCommand = EnumCommands.Unknown;
                    _waitingCommands.RemoveAt(0);
                    _mubCallback(false);
                    return;
                }
                else
                {
                    _currentCommand = EnumCommands.Unknown;
                    _waitingCommands.RemoveAt(0);
                    _mubCallback(true);
                    return;
                }
            }
            else if (_currentCommand == EnumCommands.CheckIfMUBIsRemoved)
            {
                if (response == "0")
                {
                    _currentCommand = EnumCommands.Unknown;
                    _waitingCommands.RemoveAt(0);
                    _mubCallback(true);
                    return;
                }
                else
                {
                    _currentCommand = EnumCommands.Unknown;
                    _waitingCommands.RemoveAt(0);
                    _mubCallback(false);
                    return;
                }
            }
            else
            {
                if (response == "1" || response.ToLower() == "ok" || response.ToLower() == "yes")
                {
                    _currentCommand = EnumCommands.Unknown;
                    _waitingCommands.RemoveAt(0);
                    _mubCallback(true);
                    return;
                }
                else
                {
                    if (_commandsRetryCount[_currentCommand] == _maxRetryCount)
                    {
                        _mubRetryCount = 0;
                        _currentCommand = EnumCommands.Unknown;
                        _waitingCommands.RemoveAt(0);
                        _mubCallback(false);
                        return;
                    }
                    else
                    {
                        _commandsRetryCount[_currentCommand]++;
                        Thread.Sleep(200);

                        this.DataReceived += SendCommand_Callback;
                        string asciiCommand = _rs232Commands[_currentCommand];
                        SendASCIICommand(asciiCommand);
                    }
                }
            }

        }
        ///////////////////////////

        public void CheckMUBStatus_Async(EnumMUBCommands mubCommand, MUBWorkCompletedCallback callback)
        {
            _mubRetryCount = 0;
            _mubCallback = callback;
            _mubCommand = mubCommand;

            // Check MUB Applicator Status
            this.DataReceived += CheckMUBStatus_Async_Callback;
            string asciiCommand = _rs232MUBCommands[_mubCommand];
            SendASCIICommand(asciiCommand);
        }

        private void CheckMUBStatus_Async_Callback(object sender, string response)
        {
            this.DataReceived -= CheckMUBStatus_Async_Callback;

            if (_mubCommand == EnumMUBCommands.CheckIfMUBIsPresent)
            {
                if (response == "0")
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
                if (response == "1" || response.ToLower() == "ok" || response.ToLower() == "yes")
                {
                    _mubCommand = EnumMUBCommands.Unknown;
                    _mubCallback(true);
                    return;
                }
                else
                {
                    if (_mubRetryCount == _maxRetryCount)
                    {
                        _mubRetryCount = 0;
                        _mubCommand = EnumMUBCommands.Unknown;

                        _mubCallback(false);
                        return;
                    }
                    else
                    {
                        _mubRetryCount++;
                        Thread.Sleep(200);

                        this.DataReceived += CheckMUBStatus_Async_Callback;
                        string asciiCommand = _rs232MUBCommands[_mubCommand];
                        SendASCIICommand(asciiCommand);
                    }
                }
            }

        }

        #endregion

        #region TT functions
        public void InitializeTTApplicator_Async()
        {
            // Send command to initialize TT Applicator
            string asciiCommand = "WR MR100 1";
            SendASCIICommand(asciiCommand);
        }

        public void StartTTApplicator_Async()
        {
            // Send command to start TT Applicator
            string asciiCommand = "WR MR104 1";
            SendASCIICommand(asciiCommand);
        }

        public void CloseTTDoor_Async()
        {
            // Send command to close MUB Door
            string asciiCommand = "WR MR511 1";
            SendASCIICommand(asciiCommand);
        }

        public void OpenTTDoor_Async()
        {
            // Send command to open MUB Door
            string asciiCommand = "WR MR510 1";
            SendASCIICommand(asciiCommand);
        }

        public void MoveUpTTRobot_Async()
        {
            // Send command to open MUB Door
            string asciiCommand = "WR MR502 1";
            SendASCIICommand(asciiCommand);
        }

        public void MoveDownTTRobot_Async()
        {
            // Send command to open MUB Door
            string asciiCommand = "WR MR503 1";
            SendASCIICommand(asciiCommand);
        }

        public delegate void TTWorkCompletedCallback(bool status);

        private TTWorkCompletedCallback _ttCallback = null;
        private EnumTTCommands _ttCommand;

        public void CheckTTStatus_Async(EnumTTCommands ttCommand, TTWorkCompletedCallback callback)
        {
            _ttRetryCount = 0;
            _ttCallback = callback;
            _ttCommand = ttCommand;

            // Check TT Applicator Status
            this.DataReceived += CheckTTStatus_Async_Callback;
            string asciiCommand = _rs232TTCommands[_ttCommand];
            SendASCIICommand(asciiCommand);
        }

        private void CheckTTStatus_Async_Callback(object sender, string response)
        {
            this.DataReceived -= CheckTTStatus_Async_Callback;

            if (_ttCommand == EnumTTCommands.CheckIfTTIsPresent)
            {
                if (response == "0")
                {
                    _ttCommand = EnumTTCommands.Unknown;
                    _ttCallback(false);
                    return;
                }
                else
                {
                    _ttCommand = EnumTTCommands.Unknown;
                    _ttCallback(true);
                    return;
                }
            }
            else if (_ttCommand == EnumTTCommands.CheckIfTTIsRemoved)
            {
                if (response == "0")
                {
                    _ttCommand = EnumTTCommands.Unknown;
                    _ttCallback(true);
                    return;
                }
                else
                {
                    _ttCommand = EnumTTCommands.Unknown;
                    _ttCallback(false);
                    return;
                }
            }
            else
            {
                if (response == "1" || response.ToLower() == "ok" || response.ToLower() == "yes")
                {
                    _ttCommand = EnumTTCommands.Unknown;
                    _ttCallback(true);
                    return;
                }
                else
                {
                    if (_ttRetryCount == _maxRetryCount)
                    {
                        _ttRetryCount = 0;
                        _ttCommand = EnumTTCommands.Unknown;

                        _ttCallback(false);
                        return;
                    }
                    else
                    {
                        _ttRetryCount++;
                        Thread.Sleep(200);

                        this.DataReceived += CheckTTStatus_Async_Callback;
                        string asciiCommand = _rs232TTCommands[_ttCommand];
                        SendASCIICommand(asciiCommand);
                    }
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
            lock (_serialPort)
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
