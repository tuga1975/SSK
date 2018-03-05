using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Trinity.Device.Util;

namespace Experiment
{
    public partial class FormLEDLightControl : Form
    {
        /// <summary> 
        /// Holds data received until we get a terminator. 
        /// </summary> 
        private string tString = string.Empty;
        /// <summary> 
        /// End of transmition byte in this case EOT (ASCII 4). 
        /// </summary> 
        private byte _terminator = 0x4;
        public FormLEDLightControl()
        {
            InitializeComponent();
        }

        private void FormLEDLightControl_Load(object sender, EventArgs e)
        {
            GetAvailablePorts();
        }

        public void GetAvailablePorts()
        {
            string[] ports = SerialPort.GetPortNames();
            cboPortNames.Items.AddRange(ports);
        }

        private void btnOpenPort_Click(object sender, EventArgs e)
        {
            try
            {
                LEDStatusLightingUtil ledStatusLightingUtil = LEDStatusLightingUtil.Instance;

                string station = "";
                string parity = "";
                if (chkSSA.Checked)
                {
                    station = "SSA";
                    parity = "Even";
                }
                else if (chkSSK.Checked)
                {
                    station = "SSK";
                    parity = "None";
                }
                ledStatusLightingUtil.DataReceived += LedStatusLightingUtil_DataReceived;
                txtReceivedData.Text = ledStatusLightingUtil.OpenPort(station, cboPortNames.Text, int.Parse(cboBaudRate.Text), parity);

                btnOpenPort.Enabled = false;
                btnClosePort.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            //try
            //{
            //    if (cboPortNames.Text == "" || cboBaudRate.Text == "")
            //    {
            //        txtReceivedData.Text = "Please select port settings";
            //    }
            //    else
            //    {
            //        serialPort1.PortName = cboPortNames.Text;
            //        serialPort1.BaudRate = int.Parse(cboBaudRate.Text);
            //        progressBar1.Value = 100;
            //        btnSend.Enabled = true;
            //        btnReceive.Enabled = true;
            //        txtHEXStringToSend.Enabled = true;
            //        btnOpenPort.Enabled = false;
            //        btnClosePort.Enabled = true;
            //        //if (cboParity.SelectedIndex == 0)
            //        //{
            //        //    serialPort1.Parity = Parity.Even;
            //        //}
            //        //else
            //        //{
            //        //    serialPort1.Parity = Parity.None;
            //        //}
            //        serialPort1.Parity = Parity.None;
            //        serialPort1.Open();

            //        serialPort1.DataReceived += SerialPort1_DataReceived;
            //    }
            //}
            //catch (UnauthorizedAccessException ex)
            //{
            //    txtReceivedData.Text = ex.Message;
            //}
        }

        private void LedStatusLightingUtil_DataReceived(object sender, SerialDataReceivedEventArgs2 e)
        {
            SerialPort serialPort = e.SourceObject;
            //Initialize a buffer to hold the received data 
            byte[] buffer = new byte[serialPort.ReadBufferSize];

            //There is no accurate method for checking how many bytes are read 
            //unless you check the return from the Read method 
            int bytesRead = serialPort.Read(buffer, 0, buffer.Length);

            //For the example assume the data we are received is ASCII data. 
            string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            //Check if string contains the terminator  

            //if (tString.IndexOf((char)_terminator) > -1)
            //{
            //    //If tString does contain terminator we cannot assume that it is the last character received 
            //    string workingString = tString.Substring(0, tString.IndexOf((char)_terminator));
            //    //Remove the data up to the terminator from tString 
            //    tString = tString.Substring(tString.IndexOf((char)_terminator));
            //    //Do something with workingString 
            //    //Console.WriteLine(workingString);
            //    txtReceivedData.Text = workingString + "\n" + txtReceivedData.Text;
            //}
            txtReceivedData.Text = response;
        }

        private void SerialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            
        }

        private void btnClosePort_Click(object sender, EventArgs e)
        {
            LEDStatusLightingUtil ledStatusLightingUtil = LEDStatusLightingUtil.Instance;
            ledStatusLightingUtil.DataReceived -= LedStatusLightingUtil_DataReceived;

            ledStatusLightingUtil.ClosePort();
            //serialPort1.Close();
            //serialPort1.PortName = cboPortNames.Text;
            //serialPort1.BaudRate = int.Parse(cboBaudRate.Text);
            progressBar1.Value = 0;
            btnSend.Enabled = false;
            btnReceive.Enabled = false;
            txtHEXStringToSend.Enabled = false;
            btnOpenPort.Enabled = true;
            btnClosePort.Enabled = false;
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            //string ascii = txtASCIIStringToSend.Text;
            //string hex = txtHEXStringToSend.Text;
            //if (!string.IsNullOrEmpty(ascii))
            //{
            //    hex = ToHEX(ascii);
            //    txtHEXStringToSend.Text = hex;
            //}
            ////string hex = ToHEX(hexString);
            //byte[] bytestosend = ToByteArray(hex);

            //try
            //{
            //    serialPort1.Write(bytestosend, 0, bytestosend.Length);
            //}
            //catch (Exception ex)
            //{
            //    txtReceivedData.Text = ex.Message;
            //}
        }

        private void btnReceive_Click(object sender, EventArgs e)
        {
            //try
            //{
            //    txtReceivedData.Text = serialPort1.ReadLine();
            //}
            //catch (TimeoutException ex)
            //{
            //    txtReceivedData.Text = ex.Message;
            //}
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

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtReceivedData.Text = "";
        }

        //private void SendCommand(string hexCommand)
        //{
        //    txtReceivedData.Text = "";
        //    byte[] bytestosend = ToByteArray(hexCommand);

        //    try
        //    {
        //        serialPort1.Write(bytestosend, 0, bytestosend.Length);
        //    }
        //    catch (Exception ex)
        //    {
        //        txtReceivedData.Text = ex.Message;
        //    }
        //}

        //private void SendASCIICommand(string asciiCommand)
        //{
        //    txtReceivedData.Text = "";
        //    try
        //    {
        //        MessageBox.Show("Starting to send command '" + asciiCommand + "' to SSK LED Status Lighting...");
        //        serialPort1.WriteLine(asciiCommand);
        //    }
        //    catch (Exception ex)
        //    {
        //        txtReceivedData.Text = ex.Message;
        //    }
        //}

        private void btnStartCommunication_Click(object sender, EventArgs e)
        {
            //SendCommand("43520D");
            //btnStartCommunication.Enabled = false;
        }

        private void btnResetPLC_Click(object sender, EventArgs e)
        {
            //SendCommand("4D310D");
            //btnResetPLC.Enabled = false;
        }


        private void btnTurnOffAllLights_Click(object sender, EventArgs e)
        {
            LEDStatusLightingUtil ledStatusLightingUtil = LEDStatusLightingUtil.Instance;
            ledStatusLightingUtil.TurnOffAllLEDs();
            //if (chkSSA.Checked)
            //{
            //    string hexCommand = "";
            //    string asciiCommand = "";
            //    //
            //    // Turn of RED light
            //    //
            //    asciiCommand = "WR 605 0";
            //    hexCommand = ToHEX(asciiCommand);
            //    SendCommand(hexCommand);
            //    //
            //    // Turn of GREEN light
            //    //
            //    asciiCommand = "WR 606 0";
            //    hexCommand = ToHEX(asciiCommand);
            //    SendCommand(hexCommand);
            //    //
            //    // Turn of BLUE light
            //    //
            //    asciiCommand = "WR 604 0";
            //    hexCommand = ToHEX(asciiCommand);
            //    SendCommand(hexCommand);
            //}
        }

        private void radREDLight_CheckedChanged(object sender, EventArgs e)
        {
            LEDStatusLightingUtil ledStatusLightingUtil = LEDStatusLightingUtil.Instance;
            ledStatusLightingUtil.SwitchREDLightOnOff(radREDLight.Checked);
        }

        private void radGREENLight_CheckedChanged(object sender, EventArgs e)
        {
            LEDStatusLightingUtil ledStatusLightingUtil = LEDStatusLightingUtil.Instance;
            ledStatusLightingUtil.SwitchGREENLightOnOff(radGREENLight.Checked);
        }

        private void radBLUELight_CheckedChanged(object sender, EventArgs e)
        {
            LEDStatusLightingUtil ledStatusLightingUtil = LEDStatusLightingUtil.Instance;
            ledStatusLightingUtil.SwitchBLUELightOnOff(radBLUELight.Checked);
        }

        private void radYELLOWLight_CheckedChanged(object sender, EventArgs e)
        {
            LEDStatusLightingUtil ledStatusLightingUtil = LEDStatusLightingUtil.Instance;
            ledStatusLightingUtil.SwitchYELLOWLightOnOff(radYELLOWLight.Checked);
        }
    }
}
