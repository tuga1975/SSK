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
            this.btnOpenPort.Location = new System.Drawing.Point(749, 146);
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
            this.btnClosePort.Location = new System.Drawing.Point(749, 233);
            this.btnClosePort.Name = "btnClosePort";
            this.btnClosePort.Size = new System.Drawing.Size(96, 45);
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
            this.btnStartCommunication.Size = new System.Drawing.Size(334, 42);
            this.btnStartCommunication.TabIndex = 9;
            this.btnStartCommunication.Text = "START COMMUNICATION";
            this.btnStartCommunication.UseVisualStyleBackColor = true;
            this.btnStartCommunication.Visible = false;
            this.btnStartCommunication.Click += new System.EventHandler(this.btnStartCommunication_Click);
            // 
            // btnResetPLC
            // 
            this.btnResetPLC.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnResetPLC.Location = new System.Drawing.Point(377, 475);
            this.btnResetPLC.Name = "btnResetPLC";
            this.btnResetPLC.Size = new System.Drawing.Size(166, 42);
            this.btnResetPLC.TabIndex = 10;
            this.btnResetPLC.Text = "RESET PLC";
            this.btnResetPLC.UseVisualStyleBackColor = true;
            this.btnResetPLC.Visible = false;
            this.btnResetPLC.Click += new System.EventHandler(this.btnResetPLC_Click);
            // 
            // btnTurnOffAllLights
            // 
            this.btnTurnOffAllLights.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnTurnOffAllLights.Location = new System.Drawing.Point(575, 475);
            this.btnTurnOffAllLights.Name = "btnTurnOffAllLights";
            this.btnTurnOffAllLights.Size = new System.Drawing.Size(270, 42);
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
            this.radBLUELight.Location = new System.Drawing.Point(412, 547);
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
            this.radYELLOWLight.Location = new System.Drawing.Point(625, 547);
            this.radYELLOWLight.Name = "radYELLOWLight";
            this.radYELLOWLight.Size = new System.Drawing.Size(221, 33);
            this.radYELLOWLight.TabIndex = 24;
            this.radYELLOWLight.TabStop = true;
            this.radYELLOWLight.Text = "YELLOW LIGHT";
            this.radYELLOWLight.UseVisualStyleBackColor = true;
            this.radYELLOWLight.CheckedChanged += new System.EventHandler(this.radYELLOWLight_CheckedChanged);
            // 
            // FormLEDLightControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(857, 609);
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
    }
}