using System;
using System.IO;
using System.IO.Ports;
using System.Text;
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
        private string _station = null;
        private System.IO.Ports.SerialPort _serialPort = null;

        #region Singleton Implementation
        // The variable is declared to be volatile to ensure that assignment to the instance variable completes before the instance variable can be accessed
        private static volatile LEDStatusLightingUtil _instance;

        // Uses a syncRoot instance to lock on, rather than locking on the type itself, to avoid deadlocks.
        private static object syncRoot = new Object();

        private LEDStatusLightingUtil() { }

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
                return _serialPort.IsOpen;
            }
        }

        public void TurnOffAllLEDs()
        {
            if (_station.Equals("SSK", StringComparison.InvariantCultureIgnoreCase))
            {
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

        #endregion

    }
}
