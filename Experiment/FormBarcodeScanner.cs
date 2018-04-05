using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Experiment
{
    public partial class FormBarcodeScanner : Form
    {
        private const Byte STX = 0x02;
        private const Byte ETX = 0x03;
        private const Byte CR = 0x0d;
        private const int RECV_DATA_MAX = 10240;
        private const bool binaryDataMode = false;  // Whether using binary data mode
        private bool _stopReceiveData = true;

        public FormBarcodeScanner()
        {
            InitializeComponent();

            this.serialPortManual.BaudRate = 115200;         // 9600, 19200, 38400, 57600 or 115200
            this.serialPortManual.DataBits = 8;              // 7 or 8
            this.serialPortManual.Parity = Parity.Even;    // Even or Odd
            this.serialPortManual.StopBits = StopBits.One;   // One or Two
            this.serialPortManual.PortName = "COM2";

            this.serialPortAuto.BaudRate = 115200;         // 9600, 19200, 38400, 57600 or 115200
            this.serialPortAuto.DataBits = 8;              // 7 or 8
            this.serialPortAuto.Parity = Parity.Even;    // Even or Odd
            this.serialPortAuto.StopBits = StopBits.One;   // One or Two
            this.serialPortAuto.PortName = "COM2";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Connect(serialPortManual);
        }

        private void Connect(SerialPort serialPort)
        {
            try
            {
                //
                // Close the COM port if opened.
                //
                if (serialPort.IsOpen)
                {
                    serialPort.Close();
                }

                //
                // Open the COM port.
                //
                serialPort.Open();

                //
                // Set 100 milliseconds to receive timeout.
                //
                serialPort.ReadTimeout = 100;
            }
            catch (Exception ex)
            {
                MessageBox.Show(serialPort.PortName + "\r\n" + ex.Message);  // non-existent or disappeared
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Disconnect(serialPortManual);
        }

        private void Disconnect(SerialPort serialPort)
        {
            try
            {
                serialPort.Close();
            }
            catch (IOException ex)
            {
                MessageBox.Show(serialPort.PortName + "\r\n" + ex.Message);    // disappeared
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            TimingOn(serialPortManual);
        }

        private bool TimingOn(SerialPort serialPort)
        {
            //
            // Send "LON" command.
            // Set STX to command header and ETX to the terminator to distinguish between command respons
            // and read data when receives data from readers.
            // 
            string lon = "\x02LON\x03";   // <STX>LON<ETX>
            Byte[] sendBytes = ASCIIEncoding.ASCII.GetBytes(lon);

            if (serialPort.IsOpen)
            {
                try
                {
                    serialPort.Write(sendBytes, 0, sendBytes.Length);
                    return true;
                }
                catch (IOException ex)
                {
                    MessageBox.Show(serialPort.PortName + "\r\n" + ex.Message);    // disappeared
                    return false;
                }
            }
            else
            {
                MessageBox.Show(serialPort.PortName + " is disconnected.");
                return false;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            TimingOff(serialPortManual);
        }

        private void TimingOff(SerialPort serialPort)
        {
            //
            // Send "LOFF" command.
            // Set STX to command header and ETX to the terminator to distinguish between command respons
            // and read data when receives data from readers.
            // 
            string loff = "\x02LOFF\x03";   // <STX>LOFF<ETX>
            Byte[] sendBytes = ASCIIEncoding.ASCII.GetBytes(loff);

            if (serialPort.IsOpen)
            {
                try
                {
                    serialPort.Write(sendBytes, 0, sendBytes.Length);
                }
                catch (IOException ex)
                {
                    MessageBox.Show(serialPort.PortName + "\r\n" + ex.Message);    // disappeared
                }
            }
            else
            {
                MessageBox.Show(serialPort.PortName + " is disconnected.");
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            ReceiveData(serialPortManual);
        }

        private void ReceiveData(SerialPort serialPort)
        {
            Byte[] recvBytes = new Byte[RECV_DATA_MAX];
            int recvSize;

            if (serialPort.IsOpen == false)
            {
                MessageBox.Show(serialPort.PortName + " is disconnected.");
                return;
            }

            for (; ; )
            {
                try
                {
                    recvSize = readDataSub(recvBytes, serialPort);
                }
                catch (IOException ex)
                {
                    MessageBox.Show(serialPort.PortName + "\r\n" + ex.Message);    // disappeared
                    break;
                }
                if (recvSize == 0)
                {
                    MessageBox.Show(serialPort.PortName + " has no data.");
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
                    MessageBox.Show(serialPort.PortName + "\r\n" + Encoding.GetEncoding("Shift_JIS").GetString(recvBytes));
                    break;
                }
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
                    returnValue = serialPort.PortName + "\r\n" + Encoding.GetEncoding("Shift_JIS").GetString(recvBytes);
                    break;
                }
            }

            return returnValue;
        }

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

        private void button9_Click(object sender, EventArgs e)
        {
            Connect(serialPortAuto);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            Disconnect(serialPortAuto);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            bool timingOn = TimingOn(serialPortAuto);

            if (timingOn)
            {
                var cancellationTokenSource = new CancellationTokenSource();
                var token = cancellationTokenSource.Token;
                Task.Factory.StartNew(() =>
                {
                    string data = string.Empty;
                    string description  = string.Empty;
                    _stopReceiveData = false;
                    for (; ; )
                    {
                        Thread.Sleep(100);
                        data = ReceiveData_Auto(serialPortAuto, ref description);
                        SetText(DateTime.Now.ToString() + ": " + data + " - " + description);
                        if (_stopReceiveData || !string.IsNullOrEmpty(data) || !description.Contains("no data"))
                        {
                            break;
                        }
                    }
                    //while (string.IsNullOrEmpty(data) || string.IsNullOrEmpty(error) || !_stopReceiveData)
                    //{
                    //    Thread.Sleep(100);
                    //    //data = ReceiveData_Auto(serialPortAuto, ref error);
                    //    SetText(DateTime.Now.ToString());
                    //    //this.Invoke(() => { richTextBox1.Text = DateTime.Now.ToString(); });
                    //}

                    MessageBox.Show(data);
                }, token);
            }
        }

        delegate void SetTextCallback(string text);

        private void SetText(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.richTextBox1.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.richTextBox1.Text = text;
                //richTextBox1.AppendText(text);
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            TimingOff(serialPortAuto);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            _stopReceiveData = true;
        }
    }
}
