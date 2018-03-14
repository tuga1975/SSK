namespace Experiment
{
    partial class FormLEDLightControl
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.cboPortNames = new System.Windows.Forms.ComboBox();
            this.cboBaudRate = new System.Windows.Forms.ComboBox();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtASCIIStringToSend = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.btnSend = new System.Windows.Forms.Button();
            this.txtHEXStringToSend = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnReceive = new System.Windows.Forms.Button();
            this.txtReceivedData = new System.Windows.Forms.TextBox();
            this.btnOpenPort = new System.Windows.Forms.Button();
            this.btnClosePort = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.btnStartCommunication = new System.Windows.Forms.Button();
            this.btnResetPLC = new System.Windows.Forms.Button();
            this.btnTurnOffAllLights = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.cboParity = new System.Windows.Forms.ComboBox();
            this.chkSSA = new System.Windows.Forms.CheckBox();
            this.chkSSK = new System.Windows.Forms.CheckBox();
            this.radREDLight = new System.Windows.Forms.RadioButton();
            this.radGREENLight = new System.Windows.Forms.RadioButton();
            this.radBLUELight = new System.Windows.Forms.RadioButton();
            this.radYELLOWLight = new System.Windows.Forms.RadioButton();
            this.serialPort1 = new System.IO.Ports.SerialPort(this.components);
            this.lblCheckIfMUBApplicatorIsReady = new System.Windows.Forms.Label();
            this.lblCheckIfMUBApplicatorIsStarted = new System.Windows.Forms.Label();
            this.lblCheckIfMUBIsPresent = new System.Windows.Forms.Label();
            this.lblCheckIfMUBIsRemoved = new System.Windows.Forms.Label();
            this.lblCheckIfMUBDoorIsFullyClosed = new System.Windows.Forms.Label();
            this.lblCheckIfMUBDoorIsFullyOpen = new System.Windows.Forms.Label();
            this.btnCheckIfMUBApplicatorIsReady = new System.Windows.Forms.Button();
            this.btnCheckIfMUBApplicatorIsStarted = new System.Windows.Forms.Button();
            this.btnCheckIfMUBIsPresent = new System.Windows.Forms.Button();
            this.btnCheckIfMUBIsRemoved = new System.Windows.Forms.Button();
            this.btnCheckIfMUBDoorIsFullyClosed = new System.Windows.Forms.Button();
            this.btnCheckIfMUBDoorIsFullyOpen = new System.Windows.Forms.Button();
            this.btnCheckIfTTDoorIsFullyOpen = new System.Windows.Forms.Button();
            this.btnCheckIfTTDoorIsFullyClosed = new System.Windows.Forms.Button();
            this.btnCheckIfTTIsRemoved = new System.Windows.Forms.Button();
            this.btnCheckIfTTIsPresent = new System.Windows.Forms.Button();
            this.btnCheckIfTTApplicatorIsStarted = new System.Windows.Forms.Button();
            this.btnCheckIfTTApplicatorIsReady = new System.Windows.Forms.Button();
            this.lblCheckIfTTDoorIsFullyOpen = new System.Windows.Forms.Label();
            this.lblCheckIfTTDoorIsFullyClosed = new System.Windows.Forms.Label();
            this.lblCheckIfTTIsRemoved = new System.Windows.Forms.Label();
            this.lblCheckIfTTIsPresent = new System.Windows.Forms.Label();
            this.lblCheckIfTTApplicatorIsStarted = new System.Windows.Forms.Label();
            this.lblCheckIfTTApplicatorIsReady = new System.Windows.Forms.Label();
            this.btnInitializeMUBApplicator = new System.Windows.Forms.Button();
            this.btnStartMUBApplicator = new System.Windows.Forms.Button();
            this.btnCloseMUBDoor = new System.Windows.Forms.Button();
            this.btnOpenMUBDoor = new System.Windows.Forms.Button();
            this.btnOpenTTDoor = new System.Windows.Forms.Button();
            this.btnCloseTTDoor = new System.Windows.Forms.Button();
            this.btnStartTTApplicator = new System.Windows.Forms.Button();
            this.btnInitializeTTApplicator = new System.Windows.Forms.Button();
            this.btnTTRobotUp = new System.Windows.Forms.Button();
            this.btnTTRobotDown = new System.Windows.Forms.Button();
            this.txtLogs = new System.Windows.Forms.RichTextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // cboPortNames
            // 
            this.cboPortNames.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboPortNames.FormattingEnabled = true;
            this.cboPortNames.Location = new System.Drawing.Point(19, 31);
            this.cboPortNames.Name = "cboPortNames";
            this.cboPortNames.Size = new System.Drawing.Size(121, 24);
            this.cboPortNames.TabIndex = 0;
            // 
            // cboBaudRate
            // 
            this.cboBaudRate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboBaudRate.FormattingEnabled = true;
            this.cboBaudRate.Items.AddRange(new object[] {
            "19200",
            "2400"});
            this.cboBaudRate.Location = new System.Drawing.Point(156, 32);
            this.cboBaudRate.Name = "cboBaudRate";
            this.cboBaudRate.Size = new System.Drawing.Size(121, 24);
            this.cboBaudRate.TabIndex = 1;
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(498, 32);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(100, 23);
            this.progressBar1.TabIndex = 2;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.txtASCIIStringToSend);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.btnSend);
            this.groupBox1.Controls.Add(this.txtHEXStringToSend);
            this.groupBox1.Location = new System.Drawing.Point(12, 130);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(265, 339);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Send Data";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 176);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(45, 17);
            this.label5.TabIndex = 4;
            this.label5.Text = "ASCII:";
            // 
            // txtASCIIStringToSend
            // 
            this.txtASCIIStringToSend.Location = new System.Drawing.Point(10, 200);
            this.txtASCIIStringToSend.Multiline = true;
            this.txtASCIIStringToSend.Name = "txtASCIIStringToSend";
            this.txtASCIIStringToSend.Size = new System.Drawing.Size(249, 80);
            this.txtASCIIStringToSend.TabIndex = 3;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(10, 28);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(81, 17);
            this.label4.TabIndex = 2;
            this.label4.Text = "HEX String:";
            // 
            // btnSend
            // 
            this.btnSend.Location = new System.Drawing.Point(166, 295);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(96, 38);
            this.btnSend.TabIndex = 1;
            this.btnSend.Text = "Send";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // txtHEXStringToSend
            // 
            this.txtHEXStringToSend.Location = new System.Drawing.Point(10, 52);
            this.txtHEXStringToSend.Multiline = true;
            this.txtHEXStringToSend.Name = "txtHEXStringToSend";
            this.txtHEXStringToSend.Size = new System.Drawing.Size(249, 96);
            this.txtHEXStringToSend.TabIndex = 0;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnClear);
            this.groupBox2.Controls.Add(this.btnReceive);
            this.groupBox2.Controls.Add(this.txtReceivedData);
            this.groupBox2.Location = new System.Drawing.Point(323, 130);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(275, 217);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Received Data";
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(7, 173);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(96, 38);
            this.btnClear.TabIndex = 3;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnReceive
            // 
            this.btnReceive.Enabled = false;
            this.btnReceive.Location = new System.Drawing.Point(173, 173);
            this.btnReceive.Name = "btnReceive";
            this.btnReceive.Size = new System.Drawing.Size(96, 38);
            this.btnReceive.TabIndex = 2;
            this.btnReceive.Text = "Read";
            this.btnReceive.UseVisualStyleBackColor = true;
            this.btnReceive.Click += new System.EventHandler(this.btnReceive_Click);
            // 
            // txtReceivedData
            // 
            this.txtReceivedData.Enabled = false;
            this.txtReceivedData.Location = new System.Drawing.Point(7, 34);
            this.txtReceivedData.Multiline = true;
            this.txtReceivedData.Name = "txtReceivedData";
            this.txtReceivedData.ReadOnly = true;
            this.txtReceivedData.Size = new System.Drawing.Size(262, 133);
            this.txtReceivedData.TabIndex = 0;
            // 
            // btnOpenPort
            // 
            this.btnOpenPort.Location = new System.Drawing.Point(270, 73);
            this.btnOpenPort.Name = "btnOpenPort";
            this.btnOpenPort.Size = new System.Drawing.Size(96, 46);
            this.btnOpenPort.TabIndex = 1;
            this.btnOpenPort.Text = "Open Port";
            this.btnOpenPort.UseVisualStyleBackColor = true;
            this.btnOpenPort.Click += new System.EventHandler(this.btnOpenPort_Click);
            // 
            // btnClosePort
            // 
            this.btnClosePort.Enabled = false;
            this.btnClosePort.Location = new System.Drawing.Point(377, 73);
            this.btnClosePort.Name = "btnClosePort";
            this.btnClosePort.Size = new System.Drawing.Size(96, 46);
            this.btnClosePort.TabIndex = 5;
            this.btnClosePort.Text = "Close Port";
            this.btnClosePort.UseVisualStyleBackColor = true;
            this.btnClosePort.Click += new System.EventHandler(this.btnClosePort_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(19, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(75, 17);
            this.label1.TabIndex = 6;
            this.label1.Text = "Port Name";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(156, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(75, 17);
            this.label2.TabIndex = 7;
            this.label2.Text = "Baud Rate";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(495, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(48, 17);
            this.label3.TabIndex = 8;
            this.label3.Text = "Status";
            // 
            // btnStartCommunication
            // 
            this.btnStartCommunication.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStartCommunication.Location = new System.Drawing.Point(12, 475);
            this.btnStartCommunication.Name = "btnStartCommunication";
            this.btnStartCommunication.Size = new System.Drawing.Size(265, 42);
            this.btnStartCommunication.TabIndex = 9;
            this.btnStartCommunication.Text = "START COMMUNICATION";
            this.btnStartCommunication.UseVisualStyleBackColor = true;
            this.btnStartCommunication.Visible = false;
            this.btnStartCommunication.Click += new System.EventHandler(this.btnStartCommunication_Click);
            // 
            // btnResetPLC
            // 
            this.btnResetPLC.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnResetPLC.Location = new System.Drawing.Point(283, 475);
            this.btnResetPLC.Name = "btnResetPLC";
            this.btnResetPLC.Size = new System.Drawing.Size(132, 42);
            this.btnResetPLC.TabIndex = 10;
            this.btnResetPLC.Text = "RESET PLC";
            this.btnResetPLC.UseVisualStyleBackColor = true;
            this.btnResetPLC.Visible = false;
            this.btnResetPLC.Click += new System.EventHandler(this.btnResetPLC_Click);
            // 
            // btnTurnOffAllLights
            // 
            this.btnTurnOffAllLights.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnTurnOffAllLights.Location = new System.Drawing.Point(421, 475);
            this.btnTurnOffAllLights.Name = "btnTurnOffAllLights";
            this.btnTurnOffAllLights.Size = new System.Drawing.Size(242, 42);
            this.btnTurnOffAllLights.TabIndex = 14;
            this.btnTurnOffAllLights.Text = "TURN OFF ALL LIGHTS";
            this.btnTurnOffAllLights.UseVisualStyleBackColor = true;
            this.btnTurnOffAllLights.Click += new System.EventHandler(this.btnTurnOffAllLights_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(619, 8);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(44, 17);
            this.label6.TabIndex = 16;
            this.label6.Text = "Parity";
            // 
            // cboParity
            // 
            this.cboParity.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboParity.FormattingEnabled = true;
            this.cboParity.Items.AddRange(new object[] {
            "Even",
            "None"});
            this.cboParity.Location = new System.Drawing.Point(622, 31);
            this.cboParity.Name = "cboParity";
            this.cboParity.Size = new System.Drawing.Size(121, 24);
            this.cboParity.TabIndex = 17;
            // 
            // chkSSA
            // 
            this.chkSSA.AutoSize = true;
            this.chkSSA.Checked = true;
            this.chkSSA.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkSSA.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkSSA.Location = new System.Drawing.Point(12, 73);
            this.chkSSA.Name = "chkSSA";
            this.chkSSA.Size = new System.Drawing.Size(97, 36);
            this.chkSSA.TabIndex = 18;
            this.chkSSA.Text = "SSA";
            this.chkSSA.UseVisualStyleBackColor = true;
            // 
            // chkSSK
            // 
            this.chkSSK.AutoSize = true;
            this.chkSSK.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkSSK.Location = new System.Drawing.Point(134, 73);
            this.chkSSK.Name = "chkSSK";
            this.chkSSK.Size = new System.Drawing.Size(97, 36);
            this.chkSSK.TabIndex = 19;
            this.chkSSK.Text = "SSK";
            this.chkSSK.UseVisualStyleBackColor = true;
            // 
            // radREDLight
            // 
            this.radREDLight.AutoSize = true;
            this.radREDLight.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radREDLight.ForeColor = System.Drawing.Color.Red;
            this.radREDLight.Location = new System.Drawing.Point(15, 547);
            this.radREDLight.Name = "radREDLight";
            this.radREDLight.Size = new System.Drawing.Size(169, 33);
            this.radREDLight.TabIndex = 21;
            this.radREDLight.TabStop = true;
            this.radREDLight.Text = "RED LIGHT";
            this.radREDLight.UseVisualStyleBackColor = true;
            this.radREDLight.CheckedChanged += new System.EventHandler(this.radREDLight_CheckedChanged);
            // 
            // radGREENLight
            // 
            this.radGREENLight.AutoSize = true;
            this.radGREENLight.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radGREENLight.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.radGREENLight.Location = new System.Drawing.Point(191, 547);
            this.radGREENLight.Name = "radGREENLight";
            this.radGREENLight.Size = new System.Drawing.Size(206, 33);
            this.radGREENLight.TabIndex = 22;
            this.radGREENLight.TabStop = true;
            this.radGREENLight.Text = "GREEN LIGHT";
            this.radGREENLight.UseVisualStyleBackColor = true;
            this.radGREENLight.CheckedChanged += new System.EventHandler(this.radGREENLight_CheckedChanged);
            // 
            // radBLUELight
            // 
            this.radBLUELight.AutoSize = true;
            this.radBLUELight.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radBLUELight.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.radBLUELight.Location = new System.Drawing.Point(12, 612);
            this.radBLUELight.Name = "radBLUELight";
            this.radBLUELight.Size = new System.Drawing.Size(182, 33);
            this.radBLUELight.TabIndex = 23;
            this.radBLUELight.TabStop = true;
            this.radBLUELight.Text = "BLUE LIGHT";
            this.radBLUELight.UseVisualStyleBackColor = true;
            this.radBLUELight.CheckedChanged += new System.EventHandler(this.radBLUELight_CheckedChanged);
            // 
            // radYELLOWLight
            // 
            this.radYELLOWLight.AutoSize = true;
            this.radYELLOWLight.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radYELLOWLight.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.radYELLOWLight.Location = new System.Drawing.Point(194, 612);
            this.radYELLOWLight.Name = "radYELLOWLight";
            this.radYELLOWLight.Size = new System.Drawing.Size(221, 33);
            this.radYELLOWLight.TabIndex = 24;
            this.radYELLOWLight.TabStop = true;
            this.radYELLOWLight.Text = "YELLOW LIGHT";
            this.radYELLOWLight.UseVisualStyleBackColor = true;
            this.radYELLOWLight.CheckedChanged += new System.EventHandler(this.radYELLOWLight_CheckedChanged);
            // 
            // lblCheckIfMUBApplicatorIsReady
            // 
            this.lblCheckIfMUBApplicatorIsReady.AutoSize = true;
            this.lblCheckIfMUBApplicatorIsReady.ForeColor = System.Drawing.Color.Red;
            this.lblCheckIfMUBApplicatorIsReady.Location = new System.Drawing.Point(1100, 34);
            this.lblCheckIfMUBApplicatorIsReady.Name = "lblCheckIfMUBApplicatorIsReady";
            this.lblCheckIfMUBApplicatorIsReady.Size = new System.Drawing.Size(46, 17);
            this.lblCheckIfMUBApplicatorIsReady.TabIndex = 35;
            this.lblCheckIfMUBApplicatorIsReady.Text = "label7";
            // 
            // lblCheckIfMUBApplicatorIsStarted
            // 
            this.lblCheckIfMUBApplicatorIsStarted.AutoSize = true;
            this.lblCheckIfMUBApplicatorIsStarted.ForeColor = System.Drawing.Color.Red;
            this.lblCheckIfMUBApplicatorIsStarted.Location = new System.Drawing.Point(1100, 80);
            this.lblCheckIfMUBApplicatorIsStarted.Name = "lblCheckIfMUBApplicatorIsStarted";
            this.lblCheckIfMUBApplicatorIsStarted.Size = new System.Drawing.Size(46, 17);
            this.lblCheckIfMUBApplicatorIsStarted.TabIndex = 36;
            this.lblCheckIfMUBApplicatorIsStarted.Text = "label7";
            // 
            // lblCheckIfMUBIsPresent
            // 
            this.lblCheckIfMUBIsPresent.AutoSize = true;
            this.lblCheckIfMUBIsPresent.ForeColor = System.Drawing.Color.Red;
            this.lblCheckIfMUBIsPresent.Location = new System.Drawing.Point(1100, 127);
            this.lblCheckIfMUBIsPresent.Name = "lblCheckIfMUBIsPresent";
            this.lblCheckIfMUBIsPresent.Size = new System.Drawing.Size(46, 17);
            this.lblCheckIfMUBIsPresent.TabIndex = 37;
            this.lblCheckIfMUBIsPresent.Text = "label7";
            // 
            // lblCheckIfMUBIsRemoved
            // 
            this.lblCheckIfMUBIsRemoved.AutoSize = true;
            this.lblCheckIfMUBIsRemoved.ForeColor = System.Drawing.Color.Red;
            this.lblCheckIfMUBIsRemoved.Location = new System.Drawing.Point(1100, 172);
            this.lblCheckIfMUBIsRemoved.Name = "lblCheckIfMUBIsRemoved";
            this.lblCheckIfMUBIsRemoved.Size = new System.Drawing.Size(46, 17);
            this.lblCheckIfMUBIsRemoved.TabIndex = 38;
            this.lblCheckIfMUBIsRemoved.Text = "label7";
            // 
            // lblCheckIfMUBDoorIsFullyClosed
            // 
            this.lblCheckIfMUBDoorIsFullyClosed.AutoSize = true;
            this.lblCheckIfMUBDoorIsFullyClosed.ForeColor = System.Drawing.Color.Red;
            this.lblCheckIfMUBDoorIsFullyClosed.Location = new System.Drawing.Point(1100, 217);
            this.lblCheckIfMUBDoorIsFullyClosed.Name = "lblCheckIfMUBDoorIsFullyClosed";
            this.lblCheckIfMUBDoorIsFullyClosed.Size = new System.Drawing.Size(46, 17);
            this.lblCheckIfMUBDoorIsFullyClosed.TabIndex = 39;
            this.lblCheckIfMUBDoorIsFullyClosed.Text = "label7";
            // 
            // lblCheckIfMUBDoorIsFullyOpen
            // 
            this.lblCheckIfMUBDoorIsFullyOpen.AutoSize = true;
            this.lblCheckIfMUBDoorIsFullyOpen.ForeColor = System.Drawing.Color.Red;
            this.lblCheckIfMUBDoorIsFullyOpen.Location = new System.Drawing.Point(1100, 264);
            this.lblCheckIfMUBDoorIsFullyOpen.Name = "lblCheckIfMUBDoorIsFullyOpen";
            this.lblCheckIfMUBDoorIsFullyOpen.Size = new System.Drawing.Size(46, 17);
            this.lblCheckIfMUBDoorIsFullyOpen.TabIndex = 40;
            this.lblCheckIfMUBDoorIsFullyOpen.Text = "label7";
            // 
            // btnCheckIfMUBApplicatorIsReady
            // 
            this.btnCheckIfMUBApplicatorIsReady.Location = new System.Drawing.Point(828, 22);
            this.btnCheckIfMUBApplicatorIsReady.Name = "btnCheckIfMUBApplicatorIsReady";
            this.btnCheckIfMUBApplicatorIsReady.Size = new System.Drawing.Size(266, 40);
            this.btnCheckIfMUBApplicatorIsReady.TabIndex = 41;
            this.btnCheckIfMUBApplicatorIsReady.Text = "Check if MUB Applicator is ready";
            this.btnCheckIfMUBApplicatorIsReady.UseVisualStyleBackColor = true;
            this.btnCheckIfMUBApplicatorIsReady.Click += new System.EventHandler(this.btnCheckIfMUBApplicatorIsReady_Click);
            // 
            // btnCheckIfMUBApplicatorIsStarted
            // 
            this.btnCheckIfMUBApplicatorIsStarted.Location = new System.Drawing.Point(828, 68);
            this.btnCheckIfMUBApplicatorIsStarted.Name = "btnCheckIfMUBApplicatorIsStarted";
            this.btnCheckIfMUBApplicatorIsStarted.Size = new System.Drawing.Size(266, 40);
            this.btnCheckIfMUBApplicatorIsStarted.TabIndex = 42;
            this.btnCheckIfMUBApplicatorIsStarted.Text = "Check if MUB Applicator is started";
            this.btnCheckIfMUBApplicatorIsStarted.UseVisualStyleBackColor = true;
            this.btnCheckIfMUBApplicatorIsStarted.Click += new System.EventHandler(this.btnCheckIfMUBApplicatorIsStarted_Click);
            // 
            // btnCheckIfMUBIsPresent
            // 
            this.btnCheckIfMUBIsPresent.Location = new System.Drawing.Point(828, 114);
            this.btnCheckIfMUBIsPresent.Name = "btnCheckIfMUBIsPresent";
            this.btnCheckIfMUBIsPresent.Size = new System.Drawing.Size(266, 40);
            this.btnCheckIfMUBIsPresent.TabIndex = 43;
            this.btnCheckIfMUBIsPresent.Text = "Check if MUB is present";
            this.btnCheckIfMUBIsPresent.UseVisualStyleBackColor = true;
            this.btnCheckIfMUBIsPresent.Click += new System.EventHandler(this.btnCheckIfMUBIsPresent_Click);
            // 
            // btnCheckIfMUBIsRemoved
            // 
            this.btnCheckIfMUBIsRemoved.Location = new System.Drawing.Point(828, 160);
            this.btnCheckIfMUBIsRemoved.Name = "btnCheckIfMUBIsRemoved";
            this.btnCheckIfMUBIsRemoved.Size = new System.Drawing.Size(266, 40);
            this.btnCheckIfMUBIsRemoved.TabIndex = 44;
            this.btnCheckIfMUBIsRemoved.Text = "Check if MUB is removed";
            this.btnCheckIfMUBIsRemoved.UseVisualStyleBackColor = true;
            this.btnCheckIfMUBIsRemoved.Click += new System.EventHandler(this.btnCheckIfMUBIsRemoved_Click);
            // 
            // btnCheckIfMUBDoorIsFullyClosed
            // 
            this.btnCheckIfMUBDoorIsFullyClosed.Location = new System.Drawing.Point(828, 206);
            this.btnCheckIfMUBDoorIsFullyClosed.Name = "btnCheckIfMUBDoorIsFullyClosed";
            this.btnCheckIfMUBDoorIsFullyClosed.Size = new System.Drawing.Size(266, 40);
            this.btnCheckIfMUBDoorIsFullyClosed.TabIndex = 45;
            this.btnCheckIfMUBDoorIsFullyClosed.Text = "Check if MUB Door is fully closed";
            this.btnCheckIfMUBDoorIsFullyClosed.UseVisualStyleBackColor = true;
            this.btnCheckIfMUBDoorIsFullyClosed.Click += new System.EventHandler(this.btnCheckIfMUBDoorIsFullyClosed_Click);
            // 
            // btnCheckIfMUBDoorIsFullyOpen
            // 
            this.btnCheckIfMUBDoorIsFullyOpen.Location = new System.Drawing.Point(828, 252);
            this.btnCheckIfMUBDoorIsFullyOpen.Name = "btnCheckIfMUBDoorIsFullyOpen";
            this.btnCheckIfMUBDoorIsFullyOpen.Size = new System.Drawing.Size(266, 40);
            this.btnCheckIfMUBDoorIsFullyOpen.TabIndex = 46;
            this.btnCheckIfMUBDoorIsFullyOpen.Text = "Check if MUB Door is fully open";
            this.btnCheckIfMUBDoorIsFullyOpen.UseVisualStyleBackColor = true;
            this.btnCheckIfMUBDoorIsFullyOpen.Click += new System.EventHandler(this.btnCheckIfMUBDoorIsFullyOpen_Click);
            // 
            // btnCheckIfTTDoorIsFullyOpen
            // 
            this.btnCheckIfTTDoorIsFullyOpen.Location = new System.Drawing.Point(828, 551);
            this.btnCheckIfTTDoorIsFullyOpen.Name = "btnCheckIfTTDoorIsFullyOpen";
            this.btnCheckIfTTDoorIsFullyOpen.Size = new System.Drawing.Size(266, 40);
            this.btnCheckIfTTDoorIsFullyOpen.TabIndex = 58;
            this.btnCheckIfTTDoorIsFullyOpen.Text = "Check if TT Door is fully open";
            this.btnCheckIfTTDoorIsFullyOpen.UseVisualStyleBackColor = true;
            this.btnCheckIfTTDoorIsFullyOpen.Click += new System.EventHandler(this.btnCheckIfTTDoorIsFullyOpen_Click);
            // 
            // btnCheckIfTTDoorIsFullyClosed
            // 
            this.btnCheckIfTTDoorIsFullyClosed.Location = new System.Drawing.Point(828, 505);
            this.btnCheckIfTTDoorIsFullyClosed.Name = "btnCheckIfTTDoorIsFullyClosed";
            this.btnCheckIfTTDoorIsFullyClosed.Size = new System.Drawing.Size(266, 40);
            this.btnCheckIfTTDoorIsFullyClosed.TabIndex = 57;
            this.btnCheckIfTTDoorIsFullyClosed.Text = "Check if TT Door is fully closed";
            this.btnCheckIfTTDoorIsFullyClosed.UseVisualStyleBackColor = true;
            this.btnCheckIfTTDoorIsFullyClosed.Click += new System.EventHandler(this.btnCheckIfTTDoorIsFullyClosed_Click);
            // 
            // btnCheckIfTTIsRemoved
            // 
            this.btnCheckIfTTIsRemoved.Location = new System.Drawing.Point(828, 459);
            this.btnCheckIfTTIsRemoved.Name = "btnCheckIfTTIsRemoved";
            this.btnCheckIfTTIsRemoved.Size = new System.Drawing.Size(266, 40);
            this.btnCheckIfTTIsRemoved.TabIndex = 56;
            this.btnCheckIfTTIsRemoved.Text = "Check if TT is removed";
            this.btnCheckIfTTIsRemoved.UseVisualStyleBackColor = true;
            this.btnCheckIfTTIsRemoved.Click += new System.EventHandler(this.btnCheckIfTTIsRemoved_Click);
            // 
            // btnCheckIfTTIsPresent
            // 
            this.btnCheckIfTTIsPresent.Location = new System.Drawing.Point(828, 413);
            this.btnCheckIfTTIsPresent.Name = "btnCheckIfTTIsPresent";
            this.btnCheckIfTTIsPresent.Size = new System.Drawing.Size(266, 40);
            this.btnCheckIfTTIsPresent.TabIndex = 55;
            this.btnCheckIfTTIsPresent.Text = "Check if TT is present";
            this.btnCheckIfTTIsPresent.UseVisualStyleBackColor = true;
            this.btnCheckIfTTIsPresent.Click += new System.EventHandler(this.btnCheckIfTTIsPresent_Click);
            // 
            // btnCheckIfTTApplicatorIsStarted
            // 
            this.btnCheckIfTTApplicatorIsStarted.Location = new System.Drawing.Point(828, 367);
            this.btnCheckIfTTApplicatorIsStarted.Name = "btnCheckIfTTApplicatorIsStarted";
            this.btnCheckIfTTApplicatorIsStarted.Size = new System.Drawing.Size(266, 40);
            this.btnCheckIfTTApplicatorIsStarted.TabIndex = 54;
            this.btnCheckIfTTApplicatorIsStarted.Text = "Check if TT Applicator is started";
            this.btnCheckIfTTApplicatorIsStarted.UseVisualStyleBackColor = true;
            this.btnCheckIfTTApplicatorIsStarted.Click += new System.EventHandler(this.btnCheckIfTTApplicatorIsStarted_Click);
            // 
            // btnCheckIfTTApplicatorIsReady
            // 
            this.btnCheckIfTTApplicatorIsReady.Location = new System.Drawing.Point(828, 321);
            this.btnCheckIfTTApplicatorIsReady.Name = "btnCheckIfTTApplicatorIsReady";
            this.btnCheckIfTTApplicatorIsReady.Size = new System.Drawing.Size(266, 40);
            this.btnCheckIfTTApplicatorIsReady.TabIndex = 53;
            this.btnCheckIfTTApplicatorIsReady.Text = "Check if TT Applicator is ready";
            this.btnCheckIfTTApplicatorIsReady.UseVisualStyleBackColor = true;
            this.btnCheckIfTTApplicatorIsReady.Click += new System.EventHandler(this.btnCheckIfTTApplicatorIsReady_Click);
            // 
            // lblCheckIfTTDoorIsFullyOpen
            // 
            this.lblCheckIfTTDoorIsFullyOpen.AutoSize = true;
            this.lblCheckIfTTDoorIsFullyOpen.ForeColor = System.Drawing.Color.Red;
            this.lblCheckIfTTDoorIsFullyOpen.Location = new System.Drawing.Point(1100, 563);
            this.lblCheckIfTTDoorIsFullyOpen.Name = "lblCheckIfTTDoorIsFullyOpen";
            this.lblCheckIfTTDoorIsFullyOpen.Size = new System.Drawing.Size(46, 17);
            this.lblCheckIfTTDoorIsFullyOpen.TabIndex = 52;
            this.lblCheckIfTTDoorIsFullyOpen.Text = "label7";
            // 
            // lblCheckIfTTDoorIsFullyClosed
            // 
            this.lblCheckIfTTDoorIsFullyClosed.AutoSize = true;
            this.lblCheckIfTTDoorIsFullyClosed.ForeColor = System.Drawing.Color.Red;
            this.lblCheckIfTTDoorIsFullyClosed.Location = new System.Drawing.Point(1100, 516);
            this.lblCheckIfTTDoorIsFullyClosed.Name = "lblCheckIfTTDoorIsFullyClosed";
            this.lblCheckIfTTDoorIsFullyClosed.Size = new System.Drawing.Size(46, 17);
            this.lblCheckIfTTDoorIsFullyClosed.TabIndex = 51;
            this.lblCheckIfTTDoorIsFullyClosed.Text = "label7";
            // 
            // lblCheckIfTTIsRemoved
            // 
            this.lblCheckIfTTIsRemoved.AutoSize = true;
            this.lblCheckIfTTIsRemoved.ForeColor = System.Drawing.Color.Red;
            this.lblCheckIfTTIsRemoved.Location = new System.Drawing.Point(1100, 471);
            this.lblCheckIfTTIsRemoved.Name = "lblCheckIfTTIsRemoved";
            this.lblCheckIfTTIsRemoved.Size = new System.Drawing.Size(46, 17);
            this.lblCheckIfTTIsRemoved.TabIndex = 50;
            this.lblCheckIfTTIsRemoved.Text = "label7";
            // 
            // lblCheckIfTTIsPresent
            // 
            this.lblCheckIfTTIsPresent.AutoSize = true;
            this.lblCheckIfTTIsPresent.ForeColor = System.Drawing.Color.Red;
            this.lblCheckIfTTIsPresent.Location = new System.Drawing.Point(1100, 426);
            this.lblCheckIfTTIsPresent.Name = "lblCheckIfTTIsPresent";
            this.lblCheckIfTTIsPresent.Size = new System.Drawing.Size(46, 17);
            this.lblCheckIfTTIsPresent.TabIndex = 49;
            this.lblCheckIfTTIsPresent.Text = "label7";
            // 
            // lblCheckIfTTApplicatorIsStarted
            // 
            this.lblCheckIfTTApplicatorIsStarted.AutoSize = true;
            this.lblCheckIfTTApplicatorIsStarted.ForeColor = System.Drawing.Color.Red;
            this.lblCheckIfTTApplicatorIsStarted.Location = new System.Drawing.Point(1100, 379);
            this.lblCheckIfTTApplicatorIsStarted.Name = "lblCheckIfTTApplicatorIsStarted";
            this.lblCheckIfTTApplicatorIsStarted.Size = new System.Drawing.Size(46, 17);
            this.lblCheckIfTTApplicatorIsStarted.TabIndex = 48;
            this.lblCheckIfTTApplicatorIsStarted.Text = "label7";
            // 
            // lblCheckIfTTApplicatorIsReady
            // 
            this.lblCheckIfTTApplicatorIsReady.AutoSize = true;
            this.lblCheckIfTTApplicatorIsReady.ForeColor = System.Drawing.Color.Red;
            this.lblCheckIfTTApplicatorIsReady.Location = new System.Drawing.Point(1100, 333);
            this.lblCheckIfTTApplicatorIsReady.Name = "lblCheckIfTTApplicatorIsReady";
            this.lblCheckIfTTApplicatorIsReady.Size = new System.Drawing.Size(46, 17);
            this.lblCheckIfTTApplicatorIsReady.TabIndex = 47;
            this.lblCheckIfTTApplicatorIsReady.Text = "label7";
            // 
            // btnInitializeMUBApplicator
            // 
            this.btnInitializeMUBApplicator.Location = new System.Drawing.Point(622, 73);
            this.btnInitializeMUBApplicator.Name = "btnInitializeMUBApplicator";
            this.btnInitializeMUBApplicator.Size = new System.Drawing.Size(138, 46);
            this.btnInitializeMUBApplicator.TabIndex = 25;
            this.btnInitializeMUBApplicator.Text = "Initialize MUB Applicator";
            this.btnInitializeMUBApplicator.UseVisualStyleBackColor = true;
            this.btnInitializeMUBApplicator.Click += new System.EventHandler(this.btnInitializeMUBApplicator_Click);
            // 
            // btnStartMUBApplicator
            // 
            this.btnStartMUBApplicator.Location = new System.Drawing.Point(622, 126);
            this.btnStartMUBApplicator.Name = "btnStartMUBApplicator";
            this.btnStartMUBApplicator.Size = new System.Drawing.Size(138, 46);
            this.btnStartMUBApplicator.TabIndex = 26;
            this.btnStartMUBApplicator.Text = "Start MUB Applicator";
            this.btnStartMUBApplicator.UseVisualStyleBackColor = true;
            this.btnStartMUBApplicator.Click += new System.EventHandler(this.btnStartMUBApplicator_Click);
            // 
            // btnCloseMUBDoor
            // 
            this.btnCloseMUBDoor.Location = new System.Drawing.Point(622, 178);
            this.btnCloseMUBDoor.Name = "btnCloseMUBDoor";
            this.btnCloseMUBDoor.Size = new System.Drawing.Size(138, 46);
            this.btnCloseMUBDoor.TabIndex = 27;
            this.btnCloseMUBDoor.Text = "Close MUB Door";
            this.btnCloseMUBDoor.UseVisualStyleBackColor = true;
            this.btnCloseMUBDoor.Click += new System.EventHandler(this.btnCloseMUBDoor_Click);
            // 
            // btnOpenMUBDoor
            // 
            this.btnOpenMUBDoor.Location = new System.Drawing.Point(622, 230);
            this.btnOpenMUBDoor.Name = "btnOpenMUBDoor";
            this.btnOpenMUBDoor.Size = new System.Drawing.Size(138, 46);
            this.btnOpenMUBDoor.TabIndex = 28;
            this.btnOpenMUBDoor.Text = "Open MUB Door";
            this.btnOpenMUBDoor.UseVisualStyleBackColor = true;
            this.btnOpenMUBDoor.Click += new System.EventHandler(this.btnOpenMUBDoor_Click);
            // 
            // btnOpenTTDoor
            // 
            this.btnOpenTTDoor.Location = new System.Drawing.Point(671, 478);
            this.btnOpenTTDoor.Name = "btnOpenTTDoor";
            this.btnOpenTTDoor.Size = new System.Drawing.Size(138, 46);
            this.btnOpenTTDoor.TabIndex = 62;
            this.btnOpenTTDoor.Text = "Open TT Door";
            this.btnOpenTTDoor.UseVisualStyleBackColor = true;
            this.btnOpenTTDoor.Click += new System.EventHandler(this.btnOpenTTDoor_Click);
            // 
            // btnCloseTTDoor
            // 
            this.btnCloseTTDoor.Location = new System.Drawing.Point(671, 426);
            this.btnCloseTTDoor.Name = "btnCloseTTDoor";
            this.btnCloseTTDoor.Size = new System.Drawing.Size(138, 46);
            this.btnCloseTTDoor.TabIndex = 61;
            this.btnCloseTTDoor.Text = "Close TT Door";
            this.btnCloseTTDoor.UseVisualStyleBackColor = true;
            this.btnCloseTTDoor.Click += new System.EventHandler(this.btnCloseTTDoor_Click);
            // 
            // btnStartTTApplicator
            // 
            this.btnStartTTApplicator.Location = new System.Drawing.Point(671, 374);
            this.btnStartTTApplicator.Name = "btnStartTTApplicator";
            this.btnStartTTApplicator.Size = new System.Drawing.Size(138, 46);
            this.btnStartTTApplicator.TabIndex = 60;
            this.btnStartTTApplicator.Text = "Start TT Applicator";
            this.btnStartTTApplicator.UseVisualStyleBackColor = true;
            this.btnStartTTApplicator.Click += new System.EventHandler(this.btnStartTTApplicator_Click);
            // 
            // btnInitializeTTApplicator
            // 
            this.btnInitializeTTApplicator.Location = new System.Drawing.Point(671, 321);
            this.btnInitializeTTApplicator.Name = "btnInitializeTTApplicator";
            this.btnInitializeTTApplicator.Size = new System.Drawing.Size(138, 46);
            this.btnInitializeTTApplicator.TabIndex = 59;
            this.btnInitializeTTApplicator.Text = "Initialize TT Applicator";
            this.btnInitializeTTApplicator.UseVisualStyleBackColor = true;
            this.btnInitializeTTApplicator.Click += new System.EventHandler(this.btnInitializeTTApplicator_Click);
            // 
            // btnTTRobotUp
            // 
            this.btnTTRobotUp.Location = new System.Drawing.Point(671, 530);
            this.btnTTRobotUp.Name = "btnTTRobotUp";
            this.btnTTRobotUp.Size = new System.Drawing.Size(138, 46);
            this.btnTTRobotUp.TabIndex = 63;
            this.btnTTRobotUp.Text = "Robot UP";
            this.btnTTRobotUp.UseVisualStyleBackColor = true;
            this.btnTTRobotUp.Click += new System.EventHandler(this.btnTTRobotUp_Click);
            // 
            // btnTTRobotDown
            // 
            this.btnTTRobotDown.Location = new System.Drawing.Point(671, 582);
            this.btnTTRobotDown.Name = "btnTTRobotDown";
            this.btnTTRobotDown.Size = new System.Drawing.Size(138, 46);
            this.btnTTRobotDown.TabIndex = 64;
            this.btnTTRobotDown.Text = "Robot DOWN";
            this.btnTTRobotDown.UseVisualStyleBackColor = true;
            this.btnTTRobotDown.Click += new System.EventHandler(this.btnTTRobotDown_Click);
            // 
            // txtLogs
            // 
            this.txtLogs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLogs.Location = new System.Drawing.Point(671, 634);
            this.txtLogs.Name = "txtLogs";
            this.txtLogs.Size = new System.Drawing.Size(602, 138);
            this.txtLogs.TabIndex = 66;
            this.txtLogs.Text = "";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 728);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 67;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // FormLEDLightControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1299, 784);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.txtLogs);
            this.Controls.Add(this.btnTTRobotDown);
            this.Controls.Add(this.btnTTRobotUp);
            this.Controls.Add(this.btnOpenTTDoor);
            this.Controls.Add(this.btnCloseTTDoor);
            this.Controls.Add(this.btnStartTTApplicator);
            this.Controls.Add(this.btnInitializeTTApplicator);
            this.Controls.Add(this.btnCheckIfTTDoorIsFullyOpen);
            this.Controls.Add(this.btnCheckIfTTDoorIsFullyClosed);
            this.Controls.Add(this.btnCheckIfTTIsRemoved);
            this.Controls.Add(this.btnCheckIfTTIsPresent);
            this.Controls.Add(this.btnCheckIfTTApplicatorIsStarted);
            this.Controls.Add(this.btnCheckIfTTApplicatorIsReady);
            this.Controls.Add(this.lblCheckIfTTDoorIsFullyOpen);
            this.Controls.Add(this.lblCheckIfTTDoorIsFullyClosed);
            this.Controls.Add(this.lblCheckIfTTIsRemoved);
            this.Controls.Add(this.lblCheckIfTTIsPresent);
            this.Controls.Add(this.lblCheckIfTTApplicatorIsStarted);
            this.Controls.Add(this.lblCheckIfTTApplicatorIsReady);
            this.Controls.Add(this.btnCheckIfMUBDoorIsFullyOpen);
            this.Controls.Add(this.btnCheckIfMUBDoorIsFullyClosed);
            this.Controls.Add(this.btnCheckIfMUBIsRemoved);
            this.Controls.Add(this.btnCheckIfMUBIsPresent);
            this.Controls.Add(this.btnCheckIfMUBApplicatorIsStarted);
            this.Controls.Add(this.btnCheckIfMUBApplicatorIsReady);
            this.Controls.Add(this.lblCheckIfMUBDoorIsFullyOpen);
            this.Controls.Add(this.lblCheckIfMUBDoorIsFullyClosed);
            this.Controls.Add(this.lblCheckIfMUBIsRemoved);
            this.Controls.Add(this.lblCheckIfMUBIsPresent);
            this.Controls.Add(this.lblCheckIfMUBApplicatorIsStarted);
            this.Controls.Add(this.lblCheckIfMUBApplicatorIsReady);
            this.Controls.Add(this.btnOpenMUBDoor);
            this.Controls.Add(this.btnCloseMUBDoor);
            this.Controls.Add(this.btnStartMUBApplicator);
            this.Controls.Add(this.btnInitializeMUBApplicator);
            this.Controls.Add(this.radYELLOWLight);
            this.Controls.Add(this.radBLUELight);
            this.Controls.Add(this.radGREENLight);
            this.Controls.Add(this.radREDLight);
            this.Controls.Add(this.chkSSK);
            this.Controls.Add(this.chkSSA);
            this.Controls.Add(this.cboParity);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.btnTurnOffAllLights);
            this.Controls.Add(this.btnResetPLC);
            this.Controls.Add(this.btnStartCommunication);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnClosePort);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.btnOpenPort);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.cboBaudRate);
            this.Controls.Add(this.cboPortNames);
            this.Name = "FormLEDLightControl";
            this.Text = "LED Light Control";
            this.Load += new System.EventHandler(this.FormLEDLightControl_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cboPortNames;
        private System.Windows.Forms.ComboBox cboBaudRate;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.TextBox txtHEXStringToSend;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnReceive;
        private System.Windows.Forms.TextBox txtReceivedData;
        private System.Windows.Forms.Button btnOpenPort;
        private System.Windows.Forms.Button btnClosePort;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtASCIIStringToSend;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnStartCommunication;
        private System.Windows.Forms.Button btnResetPLC;
        private System.Windows.Forms.Button btnTurnOffAllLights;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox cboParity;
        private System.Windows.Forms.CheckBox chkSSA;
        private System.Windows.Forms.CheckBox chkSSK;
        private System.Windows.Forms.RadioButton radREDLight;
        private System.Windows.Forms.RadioButton radGREENLight;
        private System.Windows.Forms.RadioButton radBLUELight;
        private System.Windows.Forms.RadioButton radYELLOWLight;
        private System.IO.Ports.SerialPort serialPort1;
        private System.Windows.Forms.Label lblCheckIfMUBApplicatorIsReady;
        private System.Windows.Forms.Label lblCheckIfMUBApplicatorIsStarted;
        private System.Windows.Forms.Label lblCheckIfMUBIsPresent;
        private System.Windows.Forms.Label lblCheckIfMUBIsRemoved;
        private System.Windows.Forms.Label lblCheckIfMUBDoorIsFullyClosed;
        private System.Windows.Forms.Label lblCheckIfMUBDoorIsFullyOpen;
        private System.Windows.Forms.Button btnCheckIfMUBApplicatorIsReady;
        private System.Windows.Forms.Button btnCheckIfMUBApplicatorIsStarted;
        private System.Windows.Forms.Button btnCheckIfMUBIsPresent;
        private System.Windows.Forms.Button btnCheckIfMUBIsRemoved;
        private System.Windows.Forms.Button btnCheckIfMUBDoorIsFullyClosed;
        private System.Windows.Forms.Button btnCheckIfMUBDoorIsFullyOpen;
        private System.Windows.Forms.Button btnCheckIfTTDoorIsFullyOpen;
        private System.Windows.Forms.Button btnCheckIfTTDoorIsFullyClosed;
        private System.Windows.Forms.Button btnCheckIfTTIsRemoved;
        private System.Windows.Forms.Button btnCheckIfTTIsPresent;
        private System.Windows.Forms.Button btnCheckIfTTApplicatorIsStarted;
        private System.Windows.Forms.Button btnCheckIfTTApplicatorIsReady;
        private System.Windows.Forms.Label lblCheckIfTTDoorIsFullyOpen;
        private System.Windows.Forms.Label lblCheckIfTTDoorIsFullyClosed;
        private System.Windows.Forms.Label lblCheckIfTTIsRemoved;
        private System.Windows.Forms.Label lblCheckIfTTIsPresent;
        private System.Windows.Forms.Label lblCheckIfTTApplicatorIsStarted;
        private System.Windows.Forms.Label lblCheckIfTTApplicatorIsReady;
        private System.Windows.Forms.Button btnInitializeMUBApplicator;
        private System.Windows.Forms.Button btnStartMUBApplicator;
        private System.Windows.Forms.Button btnCloseMUBDoor;
        private System.Windows.Forms.Button btnOpenMUBDoor;
        private System.Windows.Forms.Button btnOpenTTDoor;
        private System.Windows.Forms.Button btnCloseTTDoor;
        private System.Windows.Forms.Button btnStartTTApplicator;
        private System.Windows.Forms.Button btnInitializeTTApplicator;
        private System.Windows.Forms.Button btnTTRobotUp;
        private System.Windows.Forms.Button btnTTRobotDown;
        private System.Windows.Forms.RichTextBox txtLogs;
        private System.Windows.Forms.Button button1;
    }
}