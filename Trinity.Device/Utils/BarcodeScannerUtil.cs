using System;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Trinity.Common;

namespace Trinity.Device.Util
{
    public class BarcodeScannerUtil
    {
        private System.IO.Ports.SerialPort serialPort1;
        private bool _stopReceiveData = false;

        private const Byte STX = 0x02;
        private const Byte ETX = 0x03;
        private const Byte CR = 0x0d;

        private const int RECV_DATA_MAX = 10240;
        private const bool binaryDataMode = false;  // Whether using binary data mode
        //private SerialPort[] serialPortInstance;    // Array to store instances of COM ports used

        #region Singleton Implementation
        // The variable is declared to be volatile to ensure that assignment to the instance variable completes before the instance variable can be accessed
        private static volatile BarcodeScannerUtil _instance;

        // Uses a syncRoot instance to lock on, rather than locking on the type itself, to avoid deadlocks.
        private static object syncRoot = new Object();

        /// <summary>
        /// Constructor
        /// </summary>
        private BarcodeScannerUtil()
        {
            // Use port 2
            //
            // Set RS-232C parameters.
            //
            if (!string.IsNullOrEmpty(EnumDeviceNames.BarcodeScannerPortName))
            {
                this.serialPort1 = new System.IO.Ports.SerialPort();
                this.serialPort1.BaudRate = 115200;         // 9600, 19200, 38400, 57600 or 115200
                this.serialPort1.DataBits = 8;              // 7 or 8
                this.serialPort1.Parity = Parity.Even;    // Even or Odd
                this.serialPort1.StopBits = StopBits.One;   // One or Two
                this.serialPort1.PortName = EnumDeviceNames.BarcodeScannerPortName;

                //
                // Store COM ports instances in the array.
                //
                //serialPortInstance = new SerialPort[1] { this.serialPort1 };
            }
        }

        public static BarcodeScannerUtil Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (_instance == null)
                            _instance = new BarcodeScannerUtil();
                    }
                }

                return _instance;
            }
        }
        #endregion

        public EnumDeviceStatus[] GetDeviceStatus()
        {
            if (EnumDeviceNames.EnableBarcodeScanner)
            {
                return new EnumDeviceStatus[] { EnumDeviceStatus.Connected };
            }
            else
            {
                return new EnumDeviceStatus[] { EnumDeviceStatus.Disconnected };
            }
        }

        private bool Connect()
        {
            try
            {//
             // Close the COM port if opened.
             //
                if (this.serialPort1 != null )
                {
                    if (serialPort1.IsOpen)
                    {
                        this.serialPort1.Close();
                    }

                    //
                    // Open the COM port.
                    //
                    this.serialPort1.Open();

                    //
                    // Set 100 milliseconds to receive timeout.
                    //
                    this.serialPort1.ReadTimeout = 100;

                    //MessageBox.Show("Open port OK: " + this.serialPort1.PortName);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(this.serialPort1.PortName + "\r\n" + ex.Message);  // non-existent or disappeared
                return false;
            }
        }


        /// <summary>
        /// handler for "Disonnect" 
        /// </summary>
        public bool Disconnect()
        {
            try
            {
                if (this.serialPort1 != null)
                {
                    if (this.serialPort1.IsOpen)
                    {
                        StopScanning();
                        this.serialPort1.Close();
                        //MessageBox.Show("Close port OK: " + this.serialPort1.PortName);
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (IOException ex)
            {
                //MessageBox.Show(serialPortInstance[0].PortName + "\r\n" + ex.Message);    // disappeared
                return false;
            }
        }

        /// <summary>
        /// handler for "Timing ON"
        /// </summary>
        public void StartScanning(Action<string, string> barcodeScannerCallback)
        {
            // Open port
            if (!Connect())
            {
                barcodeScannerCallback(string.Empty, "Port " + EnumDeviceNames.BarcodeScannerPortName + " is disconnected.");
                return;
            }

            //MessageBox.Show("StartScanning OK");
            _stopReceiveData = false;

            // Timing ON
            //
            // Send "LON" command.
            // Set STX to command header and ETX to the terminator to distinguish between command respons
            // and read data when receives data from readers.
            // 
            string lon = "\x02LON\x03";   // <STX>LON<ETX>
            Byte[] sendBytes = ASCIIEncoding.ASCII.GetBytes(lon);

            if (this.serialPort1.IsOpen)
            {
                try
                {
                    this.serialPort1.Write(sendBytes, 0, sendBytes.Length);
                    //MessageBox.Show(this.serialPort1.PortName + "\r\n Write OK");

                    // get data
                    //MessageBox.Show("Receiving DataOK");
                    System.Threading.Tasks.Task.Factory.StartNew(() =>
                    {
                        string data = string.Empty;
                        string description = string.Empty;
                        _stopReceiveData = false;
                        for (; ; )
                        {
                            Thread.Sleep(100);
                            data = ReceiveData_Auto(this.serialPort1, ref description);

                            // Stop manually
                            if (_stopReceiveData)
                            {
                                break;
                            }

                            // Get data success
                            if (!string.IsNullOrEmpty(data))
                            {
                                System.Threading.Tasks.Task.Factory.StartNew(() => barcodeScannerCallback(data, string.Empty));
                                break;
                            }

                            // Get data unsuccess
                            if (!string.IsNullOrEmpty(description) && !description.Contains("no data"))
                            {
                                System.Threading.Tasks.Task.Factory.StartNew(() => barcodeScannerCallback(data, description));
                                break;
                            }
                        }
                    });
                }
                catch (IOException ex)
                {
                    //MessageBox.Show(this.serialPort1.PortName + "\r\n" + ex.Message);    // disappeared
                    barcodeScannerCallback(string.Empty, "Port " + EnumDeviceNames.BarcodeScannerPortName + "\r\n" + ex.Message);
                }
            }
            else
            {
                //MessageBox.Show(this.serialPort1.PortName + " is disconnected.");
                barcodeScannerCallback(string.Empty, "Port " + EnumDeviceNames.BarcodeScannerPortName + " is disconnected (can not connect).");
            }
        }

        private string ReceiveData_Auto(SerialPort serialPort, ref string description)
        {
            Byte[] recvBytes = new Byte[RECV_DATA_MAX];
            int recvSize;
            string returnValue = string.Empty;

            if (serialPort.IsOpen == false)
            {
                //MessageBox.Show(serialPort.PortName + " is disconnected.");
                description = serialPort.PortName + " is disconnected.";
                return returnValue;
            }

            for (; ; )
            {
                try
                {
                    recvSize = readDataSub(recvBytes, serialPort);
                }
                catch (IOException ex)
                {
                    //MessageBox.Show(serialPort.PortName + "\r\n" + ex.Message);    // disappeared
                    description = "IOException";
                    break;
                }
                if (recvSize == 0)
                {
                    //MessageBox.Show(serialPort.PortName + " has no data.");
                    description = serialPort.PortName + " has no data.";
                    break;
                }
                if (recvBytes[0] == STX)
                {
                    //
                    // Skip if command response.
                    //
                    continue;
                }
                else
                {
                    //
                    // Show the receive data after converting the receive data to Shift-JIS.
                    // Terminating null to handle as string.
                    //
                    recvBytes[recvSize] = 0;
                    //MessageBox.Show(serialPort.PortName + "\r\n" + Encoding.GetEncoding("Shift_JIS").GetString(recvBytes));

                    // Remove trailing zeros
                    int lastIndex = Array.FindLastIndex(recvBytes, b => b != 0);
                    //Array.Resize(ref recvBytes, lastIndex + 1);
                    Array.Resize(ref recvBytes, lastIndex);

                    returnValue = Encoding.GetEncoding("Shift_JIS").GetString(recvBytes);
                    break;
                }
            }

            return returnValue;
        }

        /// <summary>
        /// handler for "Timing OFF
        /// </summary>
        public bool StopScanning()
        {
            _stopReceiveData = true;

            //
            // Send "LOFF" command.
            // Set STX to command header and ETX to the terminator to distinguish between command respons
            // and read data when receives data from readers.
            // 
            string loff = "\x02LOFF\x03";   // <STX>LOFF<ETX>
            Byte[] sendBytes = ASCIIEncoding.ASCII.GetBytes(loff);

            if (this.serialPort1.IsOpen)
            {
                try
                {
                    this.serialPort1.Write(sendBytes, 0, sendBytes.Length);
                    //MessageBox.Show(this.serialPort1.PortName + "\r\n Write OK");
                    return true;
                }
                catch (IOException ex)
                {
                    //MessageBox.Show(this.serialPort1.PortName + "\r\n" + ex.Message);    // disappeared
                    return false;
                }
            }
            else
            {
                //MessageBox.Show(this.serialPort1.PortName + " is disconnected.");
                return false;
            }
        }


        /// <summary>
        ///  handler for "Receive Data" button is clicked
        /// </summary>
        private string ReceiveData()
        {
            Byte[] recvBytes = new Byte[RECV_DATA_MAX];
            int recvSize;
            string returnValue = string.Empty;

            if (this.serialPort1.IsOpen == false)
            {
                //MessageBox.Show(this.serialPort1.PortName + " is disconnected.");
                return string.Empty;
            }

            for (; ; )
            {
                try
                {
                    recvSize = readDataSub(recvBytes, this.serialPort1);
                }
                catch (IOException ex)
                {
                    //MessageBox.Show(this.serialPort1.PortName + "\r\n" + ex.Message);    // disappeared
                    returnValue = string.Empty;
                    break;
                }

                if (recvSize == 0)
                {
                    //MessageBox.Show(this.serialPort1.PortName + " has no data.");
                    returnValue = string.Empty;
                    break;
                }

                if (recvBytes[0] == STX)
                {
                    //
                    // Skip if command response.
                    //
                    continue;
                }
                else
                {
                    //
                    // Show the receive data after converting the receive data to Shift-JIS.
                    // Terminating null to handle as string.
                    //
                    recvBytes[recvSize] = 0;
                    //MessageBox.Show(this.serialPort1.PortName + "\r\n" + Encoding.GetEncoding("Shift_JIS").GetString(recvBytes));
                    //MessageBox.Show(PrintByteArray(recvBytes));
                    //returnValue = Encoding.UTF8.GetString(recvBytes);
                    break;
                }
            }

            // Remove trailing zeros
            int lastIndex = Array.FindLastIndex(recvBytes, b => b != 0);
            //Array.Resize(ref recvBytes, lastIndex + 1);
            Array.Resize(ref recvBytes, lastIndex);

            returnValue = Encoding.GetEncoding("Shift_JIS").GetString(recvBytes);
            //MessageBox.Show("'" + returnValue + "'");
            return returnValue;
        }

        /// <summary>
        /// Sub function to receive data
        /// </summary>
        private int readDataSub(Byte[] recvBytes, SerialPort serialPortInstance)
        {
            int recvSize = 0;
            bool isCommandRes = false;
            Byte d;

            //
            // Distinguish between command response and read data.
            //
            try
            {
                d = (Byte)serialPortInstance.ReadByte();
                recvBytes[recvSize++] = d;
                if (d == STX)
                {
                    isCommandRes = true;    // Distinguish between command response and read data.
                }
            }
            catch (TimeoutException)
            {
                return 0;   //  No data received.
            }

            //
            // Receive data until the terminator character.
            //
            for (; ; )
            {
                try
                {
                    d = (Byte)serialPortInstance.ReadByte();
                    recvBytes[recvSize++] = d;

                    if (isCommandRes && (d == ETX))
                    {
                        break;  // Command response is received completely.
                    }
                    else if (d == CR)
                    {
                        if (checkDataSize(recvBytes, recvSize))
                        {
                            break;  // Read data is received completely.
                        }
                    }
                }
                catch (TimeoutException ex)
                {
                    //
                    // No terminator is received.
                    //
                    MessageBox.Show(ex.Message);
                    return 0;
                }
            }

            return recvSize;
        }

        /// <summary>
        /// check data size
        /// </summary>
        private bool checkDataSize(Byte[] recvBytes, int recvSize)
        {
            const int dataSizeLen = 4;

            if (binaryDataMode == false)
            {
                return true;
            }

            if (recvSize < dataSizeLen)
            {
                return false;
            }

            int dataSize = 0;
            int mul = 1;
            for (int i = 0; i < dataSizeLen; i++)
            {
                dataSize += (recvBytes[dataSizeLen - 1 - i] - '0') * mul;
                mul *= 10;
            }

            return (dataSize + 1 == recvSize);
        }

        private static byte[] UnsignedBytesFromSignedBytes(sbyte[] signed)
        {
            var unsigned = new byte[signed.Length];
            Buffer.BlockCopy(signed, 0, unsigned, 0, signed.Length);
            return unsigned;
        }

        private string PrintByteArray(byte[] bytes)
        {
            var sb = new StringBuilder("new byte[] { ");
            foreach (var b in bytes)
            {
                sb.Append(b + ", ");
            }
            sb.Append("}");
            return sb.ToString();
            //Console.WriteLine(sb.ToString());
        }
    }
}
