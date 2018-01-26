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
            this.btnIdentifyFingerprint.Location = new System.Drawing.Point(12, 172);
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
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(971, 581);
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
    }
}

