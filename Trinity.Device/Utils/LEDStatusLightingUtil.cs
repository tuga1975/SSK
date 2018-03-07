using System;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Timers;
using System.Windows.Forms;

namespace Trinity.Device.Util
{
    public class SerialDataReceivedEventArgs2
    {
        public SerialPort SourceObject { get; set; }
        public SerialDataReceivedEventArgs Data { get; set; }
    }

    public class LEDStatusLightingUtil
    {
        public event EventHandler<SerialDataReceivedEventArgs2> DataReceived;
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
        }

        #region Public functions
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
                    return "Please select a station: SSA or SSA";
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
                return _serialPort!=null?_serialPort.IsOpen:false;
            }
        }

        public void TurnOffAllLEDs()
        {
            if (_station.Equals("SSK", StringComparison.InvariantCultureIgnoreCase))
            {
                // stop flashing
                _timer_BlueLightFlashing.Enabled = false;
                _timer_YellowLightFlashing.Enabled = false;

                // turn off leds
                string hexCommand = "00AAFF5501";
                SendCommand(hexCommand);
            }
            else if (_station.Equals("SSA", StringComparison.InvariantCultureIgnoreCase))
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
            if (_station.Equals("SSA", StringComparison.InvariantCultureIgnoreCase))
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
            else if (_station.Equals("SSK", StringComparison.InvariantCultureIgnoreCase))
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
            if (_station.Equals("SSA", StringComparison.InvariantCultureIgnoreCase))
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
            else if (_station.Equals("SSK", StringComparison.InvariantCultureIgnoreCase))
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
            if (_station.Equals("SSA", StringComparison.InvariantCultureIgnoreCase))
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
            else if (_station.Equals("SSK", StringComparison.InvariantCultureIgnoreCase))
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
            if (_station.Equals("SSA", StringComparison.InvariantCultureIgnoreCase))
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
            else if (_station.Equals("SSK", StringComparison.InvariantCultureIgnoreCase))
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
            try
            {
                // turn off all leds first
                TurnOffAllLEDs();

                // get device status
                EnumHealthStatus applicationStatus = new DAL.DAL_DeviceStatus().GetApplicationStatus();

                // ready to use
                if (applicationStatus == EnumHealthStatus.Ready)
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
                if (applicationStatus == EnumHealthStatus.Caution)
                {
                    //SwitchYELLOWLightFlashingOnOff(true);
                }

                // error
                if (applicationStatus == EnumHealthStatus.Error)
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

        private void _serialPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            SerialDataReceivedEventArgs2 eventArgs = new SerialDataReceivedEventArgs2()
            {
                SourceObject = _serialPort,
                Data = e
            };
            DataReceived?.Invoke(this, eventArgs);
        }

        #region Private functions

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

        private string SendCommand(string hexCommand)
        {
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

        private void OnBlueLightFlashingTimedEvent(object sender, ElapsedEventArgs e)
        {
            try
            {
                //SwitchBLUELightOnOff(_isOn_BlueLightFlashing);
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
