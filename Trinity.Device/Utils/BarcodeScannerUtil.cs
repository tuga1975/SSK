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

        private const Byte STX = 0x02;
        private const Byte ETX = 0x03;
        private const Byte CR = 0x0d;

        private const int RECV_DATA_MAX = 10240;
        private const bool binaryDataMode = false;  // Whether using binary data mode
        private SerialPort[] serialPortInstance;    // Array to store instances of COM ports used

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
            this.serialPort1 = new System.IO.Ports.SerialPort();
            this.serialPort1.BaudRate = 115200;         // 9600, 19200, 38400, 57600 or 115200
            this.serialPort1.DataBits = 8;              // 7 or 8
            this.serialPort1.Parity = Parity.Even;    // Even or Odd
            this.serialPort1.StopBits = StopBits.One;   // One or Two
            this.serialPort1.PortName = "COM2";

            //
            // Store COM ports instances in the array.
            //
            serialPortInstance = new SerialPort[1] { this.serialPort1 };
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
            return new EnumDeviceStatus[] { EnumDeviceStatus.Disconnected };
        }

        private bool Connect()
        {
            try
            {//
             // Close the COM port if opened.
             //
                if (serialPortInstance[0].IsOpen)
                {
                    this.serialPortInstance[0].Close();
                }

                //
                // Open the COM port.
                //
                this.serialPortInstance[0].Open();

                //
                // Set 100 milliseconds to receive timeout.
                //
                this.serialPortInstance[0].ReadTimeout = 100;

                //MessageBox.Show("Open port OK: " + serialPortInstance[i].PortName);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(serialPortInstance[0].PortName + "\r\n" + ex.Message);  // non-existent or disappeared
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
                if (this.serialPortInstance[0].IsOpen)
                {
                    StopScanning();
                    this.serialPortInstance[0].Close();
                    //MessageBox.Show("Close port OK: " + serialPortInstance[i].PortName);
                }

                return true;
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
            Connect();

            //
            // Send "LON" command.
            // Set STX to command header and ETX to the terminator to distinguish between command respons
            // and read data when receives data from readers.
            // 
            string lon = "\x02LON\x03";   // <STX>LON<ETX>
            Byte[] sendBytes = ASCIIEncoding.ASCII.GetBytes(lon);

            if (this.serialPortInstance[0].IsOpen)
            {
                try
                {
                    this.serialPortInstance[0].Write(sendBytes, 0, sendBytes.Length);
                    //MessageBox.Show(serialPortInstance[i].PortName + "\r\n Write OK");

                    string value = string.Empty;
                    while (value == string.Empty)
                    {
                        value = ReceiveData();
                    }
                    System.Threading.Tasks.Task.Factory.StartNew(() => barcodeScannerCallback(value, string.Empty));
                }
                catch (IOException ex)
                {
                    //MessageBox.Show(serialPortInstance[0].PortName + "\r\n" + ex.Message);    // disappeared
                    barcodeScannerCallback(string.Empty, serialPortInstance[0].PortName + "\r\n" + ex.Message);
                }
            }
            else
            {
                //MessageBox.Show(serialPortInstance[0].PortName + " is disconnected.");
                barcodeScannerCallback(string.Empty, serialPortInstance[0].PortName + " is disconnected.");
            }
        }

        /// <summary>
        /// handler for "Timing OFF
        /// </summary>
        public bool StopScanning()
        {
            //
            // Send "LOFF" command.
            // Set STX to command header and ETX to the terminator to distinguish between command respons
            // and read data when receives data from readers.
            // 
            string loff = "\x02LOFF\x03";   // <STX>LOFF<ETX>
            Byte[] sendBytes = ASCIIEncoding.ASCII.GetBytes(loff);

            if (this.serialPortInstance[0].IsOpen)
            {
                try
                {
                    this.serialPortInstance[0].Write(sendBytes, 0, sendBytes.Length);
                    //MessageBox.Show(serialPortInstance[i].PortName + "\r\n Write OK");
                    return true;
                }
                catch (IOException ex)
                {
                    //MessageBox.Show(serialPortInstance[0].PortName + "\r\n" + ex.Message);    // disappeared
                    return false;
                }
            }
            else
            {
                //MessageBox.Show(serialPortInstance[0].PortName + " is disconnected.");
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

            for (int i = 0; i < this.serialPortInstance.Length; i++)
            {
                if (this.serialPortInstance[i].IsOpen == false)
                {
                    //MessageBox.Show(serialPortInstance[i].PortName + " is disconnected.");
                    continue;
                }

                for (; ; )
                {
                    try
                    {
                        recvSize = readDataSub(recvBytes, this.serialPortInstance[i]);
                    }
                    catch (IOException ex)
                    {
                        //MessageBox.Show(serialPortInstance[i].PortName + "\r\n" + ex.Message);    // disappeared
                        returnValue = string.Empty;
                        break;
                    }

                    if (recvSize == 0)
                    {
                        //MessageBox.Show(serialPortInstance[i].PortName + " has no data.");
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
                        //MessageBox.Show(serialPortInstance[i].PortName + "\r\n" + Encoding.GetEncoding("Shift_JIS").GetString(recvBytes));
                        //MessageBox.Show(PrintByteArray(recvBytes));
                        //returnValue = Encoding.UTF8.GetString(recvBytes);
                        break;
                    }
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
