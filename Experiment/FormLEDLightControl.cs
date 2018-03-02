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

namespace Experiment
{
    public partial class FormLEDLightControl : Form
    {
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
                if (cboPortNames.Text != "" || cboBaudRate.Text != "")
                {
                    txtReceivedData.Text = "Please select port settings";
                }
                else
                {
                    serialPort1.PortName = cboPortNames.Text;
                    serialPort1.BaudRate = int.Parse(cboBaudRate.Text);
                    progressBar1.Value = 100;
                    btnSend.Enabled = true;
                    btnReceive.Enabled = true;
                    txtDataToSend.Enabled = true;
                    btnOpenPort.Enabled = false;
                    btnClosePort.Enabled = true;
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                txtReceivedData.Text = ex.Message;
            }
        }

        private void btnClosePort_Click(object sender, EventArgs e)
        {
            serialPort1.Close();
            serialPort1.PortName = cboPortNames.Text;
            serialPort1.BaudRate = int.Parse(cboBaudRate.Text);
            progressBar1.Value = 0;
            btnSend.Enabled = false;
            btnReceive.Enabled = false;
            txtDataToSend.Enabled = false;
            btnOpenPort.Enabled = true;
            btnClosePort.Enabled = false;
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            serialPort1.WriteLine(txtDataToSend.Text);
            txtDataToSend.Text = "";
        }

        private void btnReceive_Click(object sender, EventArgs e)
        {
            try
            {
                txtReceivedData.Text = serialPort1.ReadLine();
            }
            catch (TimeoutException ex)
            {
                txtReceivedData.Text = ex.Message; 
            }
        }
    }
}
