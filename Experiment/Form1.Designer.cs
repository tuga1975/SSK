namespace Experiment
{
    partial class Form1
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
            this.btnPrintAppointmentDetails = new System.Windows.Forms.Button();
            this.btnPrintSuperviseeCard = new System.Windows.Forms.Button();
            this.btnReadSmartCardData = new System.Windows.Forms.Button();
            this.btnIdentifyFingerprint = new System.Windows.Forms.Button();
            this.btnStartHealthChecker = new System.Windows.Forms.Button();
            this.btnTestSerialComm = new System.Windows.Forms.Button();
            this.btnStartFlashing = new System.Windows.Forms.Button();
            this.bnStopFlashing = new System.Windows.Forms.Button();
            this.btnInitFlashing = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnPrintAppointmentDetails
            // 
            this.btnPrintAppointmentDetails.Location = new System.Drawing.Point(12, 12);
            this.btnPrintAppointmentDetails.Name = "btnPrintAppointmentDetails";
            this.btnPrintAppointmentDetails.Size = new System.Drawing.Size(183, 23);
            this.btnPrintAppointmentDetails.TabIndex = 0;
            this.btnPrintAppointmentDetails.Text = "Print Appointment Details";
            this.btnPrintAppointmentDetails.UseVisualStyleBackColor = true;
            this.btnPrintAppointmentDetails.Click += new System.EventHandler(this.btnPrintAppointmentDetails_Click);
            // 
            // btnPrintSuperviseeCard
            // 
            this.btnPrintSuperviseeCard.Location = new System.Drawing.Point(12, 41);
            this.btnPrintSuperviseeCard.Name = "btnPrintSuperviseeCard";
            this.btnPrintSuperviseeCard.Size = new System.Drawing.Size(183, 23);
            this.btnPrintSuperviseeCard.TabIndex = 1;
            this.btnPrintSuperviseeCard.Text = "Print Supervisee Card";
            this.btnPrintSuperviseeCard.UseVisualStyleBackColor = true;
            this.btnPrintSuperviseeCard.Click += new System.EventHandler(this.btnPrintSuperviseeCard_Click);
            // 
            // btnReadSmartCardData
            // 
            this.btnReadSmartCardData.Location = new System.Drawing.Point(12, 70);
            this.btnReadSmartCardData.Name = "btnReadSmartCardData";
            this.btnReadSmartCardData.Size = new System.Drawing.Size(183, 23);
            this.btnReadSmartCardData.TabIndex = 2;
            this.btnReadSmartCardData.Text = "Read Smart Card Data";
            this.btnReadSmartCardData.UseVisualStyleBackColor = true;
            this.btnReadSmartCardData.Click += new System.EventHandler(this.btnReadSmartCardData_Click);
            // 
            // btnIdentifyFingerprint
            // 
            this.btnIdentifyFingerprint.Location = new System.Drawing.Point(12, 99);
            this.btnIdentifyFingerprint.Name = "btnIdentifyFingerprint";
            this.btnIdentifyFingerprint.Size = new System.Drawing.Size(183, 23);
            this.btnIdentifyFingerprint.TabIndex = 3;
            this.btnIdentifyFingerprint.Text = "Identify Fingerprint";
            this.btnIdentifyFingerprint.UseVisualStyleBackColor = true;
            this.btnIdentifyFingerprint.Click += new System.EventHandler(this.btnIdentifyFingerprint_Click);
            // 
            // btnStartHealthChecker
            // 
            this.btnStartHealthChecker.Location = new System.Drawing.Point(267, 12);
            this.btnStartHealthChecker.Name = "btnStartHealthChecker";
            this.btnStartHealthChecker.Size = new System.Drawing.Size(183, 23);
            this.btnStartHealthChecker.TabIndex = 4;
            this.btnStartHealthChecker.Text = "Start Health Checker";
            this.btnStartHealthChecker.UseVisualStyleBackColor = true;
            this.btnStartHealthChecker.Click += new System.EventHandler(this.btnStartHealthChecker_Click);
            // 
            // btnTestSerialComm
            // 
            this.btnTestSerialComm.Location = new System.Drawing.Point(267, 41);
            this.btnTestSerialComm.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnTestSerialComm.Name = "btnTestSerialComm";
            this.btnTestSerialComm.Size = new System.Drawing.Size(183, 19);
            this.btnTestSerialComm.TabIndex = 5;
            this.btnTestSerialComm.Text = "Test Serial Communication";
            this.btnTestSerialComm.UseVisualStyleBackColor = true;
            this.btnTestSerialComm.Click += new System.EventHandler(this.btnTestSerialComm_Click);
            // 
            // btnStartFlashing
            // 
            this.btnStartFlashing.Location = new System.Drawing.Point(12, 169);
            this.btnStartFlashing.Name = "btnStartFlashing";
            this.btnStartFlashing.Size = new System.Drawing.Size(183, 23);
            this.btnStartFlashing.TabIndex = 6;
            this.btnStartFlashing.Text = "Start led flashing";
            this.btnStartFlashing.UseVisualStyleBackColor = true;
            this.btnStartFlashing.Click += new System.EventHandler(this.btnStartFlashing_Click);
            // 
            // bnStopFlashing
            // 
            this.bnStopFlashing.Location = new System.Drawing.Point(12, 198);
            this.bnStopFlashing.Name = "bnStopFlashing";
            this.bnStopFlashing.Size = new System.Drawing.Size(183, 23);
            this.bnStopFlashing.TabIndex = 7;
            this.bnStopFlashing.Text = "Stop led flashing";
            this.bnStopFlashing.UseVisualStyleBackColor = true;
            this.bnStopFlashing.Click += new System.EventHandler(this.bnStopFlashing_Click);
            // 
            // btnInitFlashing
            // 
            this.btnInitFlashing.Location = new System.Drawing.Point(12, 140);
            this.btnInitFlashing.Name = "btnInitFlashing";
            this.btnInitFlashing.Size = new System.Drawing.Size(183, 23);
            this.btnInitFlashing.TabIndex = 8;
            this.btnInitFlashing.Text = "Init Led flashing";
            this.btnInitFlashing.UseVisualStyleBackColor = true;
            this.btnInitFlashing.Click += new System.EventHandler(this.btnInitFlashing_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(971, 581);
            this.Controls.Add(this.btnInitFlashing);
            this.Controls.Add(this.bnStopFlashing);
            this.Controls.Add(this.btnStartFlashing);
            this.Controls.Add(this.btnTestSerialComm);
            this.Controls.Add(this.btnStartHealthChecker);
            this.Controls.Add(this.btnIdentifyFingerprint);
            this.Controls.Add(this.btnReadSmartCardData);
            this.Controls.Add(this.btnPrintSuperviseeCard);
            this.Controls.Add(this.btnPrintAppointmentDetails);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnPrintAppointmentDetails;
        private System.Windows.Forms.Button btnPrintSuperviseeCard;
        private System.Windows.Forms.Button btnReadSmartCardData;
        private System.Windows.Forms.Button btnIdentifyFingerprint;
        private System.Windows.Forms.Button btnStartHealthChecker;
        private System.Windows.Forms.Button btnTestSerialComm;
        private System.Windows.Forms.Button btnStartFlashing;
        private System.Windows.Forms.Button bnStopFlashing;
        private System.Windows.Forms.Button btnInitFlashing;
    }
}

