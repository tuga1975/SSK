using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
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
            cboBaudRate.SelectedIndex = 0;
            cboParity.SelectedIndex = 0;

            LEDStatusLightingUtil ledStatusLightingUtil = LEDStatusLightingUtil.Instance;
            ledStatusLightingUtil.OnNewEvent += LedStatusLightingUtil_OnNewEvent;
        }

        private void LedStatusLightingUtil_OnNewEvent(object sender, string e)
        {
            txtLogs.Text = e + "\n" + txtLogs.Text;
        }

        public void GetAvailablePorts()
        {
            string[] ports = SerialPort.GetPortNames();
            cboPortNames.Items.AddRange(ports);
            if (ports.Contains("COM5"))
            {
                cboPortNames.Text = "COM5";
            }
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
                btnSend.Enabled = true;
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
        private static object syncRoot = new Object();

        private void LedStatusLightingUtil_DataReceived(object sender, string response)
        {
            lock (syncRoot)
            {
                txtReceivedData.Text = response;
            }
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
            LEDStatusLightingUtil ledStatusLightingUtil = LEDStatusLightingUtil.Instance;
            string ascii = txtASCIIStringToSend.Text;
            string hex = txtHEXStringToSend.Text;
            if (!string.IsNullOrEmpty(ascii))
            {
                ledStatusLightingUtil.SendASCIICommand(ascii);
            }
            else
            {
                ledStatusLightingUtil.SendCommand(txtHEXStringToSend.Text);
            }
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

        private void btnInitializeMUBApplicator_Click(object sender, EventArgs e)
        {
            LEDStatusLightingUtil ledStatusLightingUtil = LEDStatusLightingUtil.Instance;
            ledStatusLightingUtil.InitializeMUBApplicator_Async();
        }

        private void btnStartMUBApplicator_Click(object sender, EventArgs e)
        {
            LEDStatusLightingUtil ledStatusLightingUtil = LEDStatusLightingUtil.Instance;
            ledStatusLightingUtil.StartMUBApplicator_Async();
        }

        private void btnCloseMUBDoor_Click(object sender, EventArgs e)
        {
            LEDStatusLightingUtil ledStatusLightingUtil = LEDStatusLightingUtil.Instance;
            ledStatusLightingUtil.CloseMUBDoor_Async();
        }

        private void btnOpenMUBDoor_Click(object sender, EventArgs e)
        {
            LEDStatusLightingUtil ledStatusLightingUtil = LEDStatusLightingUtil.Instance;
            ledStatusLightingUtil.OpenMUBDoor_Async();
        }

        private void SetMUBButtonsStatus(bool enable)
        {
            btnCheckIfMUBApplicatorIsReady.Enabled = enable;
            btnCheckIfMUBApplicatorIsStarted.Enabled = enable;
            btnCheckIfMUBIsPresent.Enabled = enable;
            btnCheckIfMUBIsRemoved.Enabled = enable;
            btnCheckIfMUBDoorIsFullyOpen.Enabled = enable;
            btnCheckIfMUBDoorIsFullyClosed.Enabled = enable;
        }

        private void SetTTButtonsStatus(bool enable)
        {
            btnCheckIfTTApplicatorIsReady.Enabled = enable;
            btnCheckIfTTApplicatorIsStarted.Enabled = enable;
            btnCheckIfTTIsPresent.Enabled = enable;
            btnCheckIfTTIsRemoved.Enabled = enable;
            btnCheckIfTTDoorIsFullyOpen.Enabled = enable;
            btnCheckIfTTDoorIsFullyClosed.Enabled = enable;
        }

        private void btnCheckIfMUBApplicatorIsReady_Click(object sender, EventArgs e)
        {
            SetMUBButtonsStatus(false);
            LEDStatusLightingUtil ledStatusLightingUtil = LEDStatusLightingUtil.Instance;
            ledStatusLightingUtil.SendCommand_Async(EnumCommands.CheckIfMUBApplicatorIsReady, CheckIfMUBApplicatorIsReady_Callback);
        }

        private void CheckIfMUBApplicatorIsReady_Callback(bool result)
        {
            SetMUBButtonsStatus(true);
            lblCheckIfMUBApplicatorIsReady.Text = result ? "True" : "False";
        }

        private void CheckIfMUBApplicatorIsStarted_Callback(bool result)
        {
            SetMUBButtonsStatus(true);
            lblCheckIfMUBApplicatorIsStarted.Text = result ? "True" : "False";
        }

        private void CheckIfMUBIsPresent_Callback(bool result)
        {
            SetMUBButtonsStatus(true);

            lblCheckIfMUBIsPresent.Text = result ? "True" : "False";
        }

        private void CheckIfMUBIsRemoved_Callback(bool result)
        {
            SetMUBButtonsStatus(true);

            lblCheckIfMUBIsRemoved.Text = result ? "True" : "False";
        }

        private void CheckIfMUBDoorIsFullyClosed_Callback(bool result)
        {
            SetMUBButtonsStatus(true);

            lblCheckIfMUBDoorIsFullyClosed.Text = result ? "True" : "False";
        }

        private void CheckIfMUBDoorIsFullyOpen_Callback(bool result)
        {
            SetMUBButtonsStatus(true);

            lblCheckIfMUBDoorIsFullyOpen.Text = result ? "True" : "False";
        }

        private void btnCheckIfMUBApplicatorIsStarted_Click(object sender, EventArgs e)
        {
            SetMUBButtonsStatus(false);
            LEDStatusLightingUtil ledStatusLightingUtil = LEDStatusLightingUtil.Instance;
            ledStatusLightingUtil.SendCommand_Async(EnumCommands.CheckIfMUBApplicatorIsStarted, CheckIfMUBApplicatorIsStarted_Callback);
        }

        private void btnCheckIfMUBIsPresent_Click(object sender, EventArgs e)
        {
            SetMUBButtonsStatus(false);

            LEDStatusLightingUtil ledStatusLightingUtil = LEDStatusLightingUtil.Instance;
            ledStatusLightingUtil.SendCommand_Async(EnumCommands.CheckIfMUBIsPresent, CheckIfMUBIsPresent_Callback);
        }

        private void btnCheckIfMUBIsRemoved_Click(object sender, EventArgs e)
        {
            SetMUBButtonsStatus(false);

            LEDStatusLightingUtil ledStatusLightingUtil = LEDStatusLightingUtil.Instance;
            ledStatusLightingUtil.SendCommand_Async(EnumCommands.CheckIfMUBIsRemoved, CheckIfMUBIsRemoved_Callback);
        }

        private void btnCheckIfMUBDoorIsFullyClosed_Click(object sender, EventArgs e)
        {
            SetMUBButtonsStatus(false);

            LEDStatusLightingUtil ledStatusLightingUtil = LEDStatusLightingUtil.Instance;
            ledStatusLightingUtil.SendCommand_Async(EnumCommands.CheckIfMUBDoorIsFullyClosed, CheckIfMUBDoorIsFullyClosed_Callback);
        }

        private void btnCheckIfMUBDoorIsFullyOpen_Click(object sender, EventArgs e)
        {
            SetMUBButtonsStatus(false);

            LEDStatusLightingUtil ledStatusLightingUtil = LEDStatusLightingUtil.Instance;
            ledStatusLightingUtil.SendCommand_Async(EnumCommands.CheckIfMUBDoorIsFullyOpen, CheckIfMUBDoorIsFullyOpen_Callback);
        }

        //////////////
        private void CheckIfTTApplicatorIsReady_Callback(bool result)
        {
            SetTTButtonsStatus(true);
            lblCheckIfTTApplicatorIsReady.Text = result ? "True" : "False";
        }

        private void CheckIfTTApplicatorIsStarted_Callback(bool result)
        {
            SetTTButtonsStatus(true);
            lblCheckIfTTApplicatorIsStarted.Text = result ? "True" : "False";
        }

        private void CheckIfTTIsPresent_Callback(bool result)
        {
            SetTTButtonsStatus(true);

            lblCheckIfTTIsPresent.Text = result ? "True" : "False";
        }

        private void CheckIfTTIsRemoved_Callback(bool result)
        {
            SetTTButtonsStatus(true);

            lblCheckIfTTIsRemoved.Text = result ? "True" : "False";
        }

        private void CheckIfTTDoorIsFullyClosed_Callback(bool result)
        {
            SetTTButtonsStatus(true);

            lblCheckIfTTDoorIsFullyClosed.Text = result ? "True" : "False";
        }

        private void CheckIfTTDoorIsFullyOpen_Callback(bool result)
        {
            SetTTButtonsStatus(true);

            lblCheckIfTTDoorIsFullyOpen.Text = result ? "True" : "False";
        }
        //
        private void btnCheckIfTTApplicatorIsReady_Click(object sender, EventArgs e)
        {
            SetTTButtonsStatus(false);
            LEDStatusLightingUtil ledStatusLightingUtil = LEDStatusLightingUtil.Instance;
            ledStatusLightingUtil.SendCommand_Async(EnumCommands.CheckIfTTApplicatorIsReady, CheckIfTTApplicatorIsReady_Callback);
        }

        private void btnCheckIfTTApplicatorIsStarted_Click(object sender, EventArgs e)
        {
            SetTTButtonsStatus(false);
            LEDStatusLightingUtil ledStatusLightingUtil = LEDStatusLightingUtil.Instance;
            ledStatusLightingUtil.SendCommand_Async(EnumCommands.CheckIfTTApplicatorIsStarted, CheckIfTTApplicatorIsStarted_Callback);
        }

        private void btnCheckIfTTIsPresent_Click(object sender, EventArgs e)
        {
            SetTTButtonsStatus(false);

            LEDStatusLightingUtil ledStatusLightingUtil = LEDStatusLightingUtil.Instance;
            ledStatusLightingUtil.SendCommand_Async(EnumCommands.CheckIfTTIsPresent, CheckIfTTIsPresent_Callback);
        }

        private void btnCheckIfTTIsRemoved_Click(object sender, EventArgs e)
        {
            SetTTButtonsStatus(false);

            LEDStatusLightingUtil ledStatusLightingUtil = LEDStatusLightingUtil.Instance;
            ledStatusLightingUtil.SendCommand_Async(EnumCommands.CheckIfTTIsRemoved, CheckIfTTIsRemoved_Callback);
        }

        private void btnCheckIfTTDoorIsFullyClosed_Click(object sender, EventArgs e)
        {
            SetTTButtonsStatus(false);

            LEDStatusLightingUtil ledStatusLightingUtil = LEDStatusLightingUtil.Instance;
            ledStatusLightingUtil.SendCommand_Async(EnumCommands.CheckIfTTDoorIsFullyClosed, CheckIfTTDoorIsFullyClosed_Callback);
        }

        private void btnCheckIfTTDoorIsFullyOpen_Click(object sender, EventArgs e)
        {
            SetTTButtonsStatus(false);

            LEDStatusLightingUtil ledStatusLightingUtil = LEDStatusLightingUtil.Instance;
            ledStatusLightingUtil.SendCommand_Async(EnumCommands.CheckIfTTDoorIsFullyOpen, CheckIfTTDoorIsFullyOpen_Callback);
        }

        private void btnInitializeTTApplicator_Click(object sender, EventArgs e)
        {
            LEDStatusLightingUtil ledStatusLightingUtil = LEDStatusLightingUtil.Instance;
            ledStatusLightingUtil.InitializeTTApplicator_Async();
        }

        private void btnStartTTApplicator_Click(object sender, EventArgs e)
        {
            LEDStatusLightingUtil ledStatusLightingUtil = LEDStatusLightingUtil.Instance;
            ledStatusLightingUtil.StartTTApplicator_Async();
        }

        private void btnCloseTTDoor_Click(object sender, EventArgs e)
        {
            LEDStatusLightingUtil ledStatusLightingUtil = LEDStatusLightingUtil.Instance;
            ledStatusLightingUtil.CloseTTDoor_Async();
        }

        private void btnOpenTTDoor_Click(object sender, EventArgs e)
        {
            LEDStatusLightingUtil ledStatusLightingUtil = LEDStatusLightingUtil.Instance;
            ledStatusLightingUtil.OpenTTDoor_Async();
        }

        private void btnTTRobotUp_Click(object sender, EventArgs e)
        {
            LEDStatusLightingUtil ledStatusLightingUtil = LEDStatusLightingUtil.Instance;
            ledStatusLightingUtil.MoveUpTTRobot_Async();
        }

        private void btnTTRobotDown_Click(object sender, EventArgs e)
        {
            LEDStatusLightingUtil ledStatusLightingUtil = LEDStatusLightingUtil.Instance;
            ledStatusLightingUtil.MoveDownTTRobot_Async();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            LEDStatusLightingUtil ledStatusLightingUtil = LEDStatusLightingUtil.Instance;
            string ascii = txtASCIIStringToSend.Text;
            for (int i = 0; i < 10; i++)
            {
                ledStatusLightingUtil.SendCommand_Async( EnumCommands.CheckIfTTApplicatorIsReady, null);
                Thread.Sleep(1000);
            }
            ledStatusLightingUtil.InitializeTTApplicator_Async();
            Thread.Sleep(1000);
            ledStatusLightingUtil.SendCommand_Async(EnumCommands.CheckIfTTApplicatorIsReady, null);
            //ledStatusLightingUtil.SendASCIICommand(ascii);
            //LEDStatusLightingUtil ledStatusLightingUtil = LEDStatusLightingUtil.Instance;
            //for (int i = 0; i < 100; i++)
            //{
            //    ledStatusLightingUtil.InitializeMUBApplicator_Async();
            //    ledStatusLightingUtil.SendCommand_Async(EnumCommands.CheckIfMUBApplicatorIsReady, CheckIfMUBApplicatorIsReady_Callback);

            //    ledStatusLightingUtil.StartMUBApplicator_Async();
            //    ledStatusLightingUtil.SendCommand_Async(EnumCommands.CheckIfMUBApplicatorIsStarted, CheckIfMUBApplicatorIsStarted_Callback);

            //    ledStatusLightingUtil.InitializeTTApplicator_Async();
            //    ledStatusLightingUtil.SendCommand_Async(EnumCommands.CheckIfTTApplicatorIsReady, CheckIfTTApplicatorIsReady_Callback);

            //    ledStatusLightingUtil.StartTTApplicator_Async();
            //    ledStatusLightingUtil.SendCommand_Async(EnumCommands.CheckIfTTApplicatorIsStarted, CheckIfTTApplicatorIsStarted_Callback);
            //}
        }
    }
}
