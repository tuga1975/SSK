namespace SSK
{
    partial class FormAppointmentDetails
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormAppointmentDetails));
            this.btnPrint = new System.Windows.Forms.Button();
            this.webBrowserAppointmentDetails = new System.Windows.Forms.WebBrowser();
            this.btnGenerateTimeslots = new System.Windows.Forms.Button();
            this.btnGenerateAppointments = new System.Windows.Forms.Button();
            this.dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
            this.btnWriteDataToSmartCard = new System.Windows.Forms.Button();
            this.btnReaderData = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnPrint
            // 
            this.btnPrint.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPrint.Location = new System.Drawing.Point(13, 13);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new System.Drawing.Size(152, 46);
            this.btnPrint.TabIndex = 0;
            this.btnPrint.Text = "Print";
            this.btnPrint.UseVisualStyleBackColor = true;
            this.btnPrint.Click += new System.EventHandler(this.btnPrint_Click);
            // 
            // webBrowserAppointmentDetails
            // 
            this.webBrowserAppointmentDetails.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.webBrowserAppointmentDetails.Location = new System.Drawing.Point(13, 287);
            this.webBrowserAppointmentDetails.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowserAppointmentDetails.Name = "webBrowserAppointmentDetails";
            this.webBrowserAppointmentDetails.ScriptErrorsSuppressed = true;
            this.webBrowserAppointmentDetails.Size = new System.Drawing.Size(694, 299);
            this.webBrowserAppointmentDetails.TabIndex = 1;
            // 
            // btnGenerateTimeslots
            // 
            this.btnGenerateTimeslots.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnGenerateTimeslots.Location = new System.Drawing.Point(171, 14);
            this.btnGenerateTimeslots.Name = "btnGenerateTimeslots";
            this.btnGenerateTimeslots.Size = new System.Drawing.Size(160, 46);
            this.btnGenerateTimeslots.TabIndex = 2;
            this.btnGenerateTimeslots.Text = "Generate Timeslots";
            this.btnGenerateTimeslots.UseVisualStyleBackColor = true;
            this.btnGenerateTimeslots.Click += new System.EventHandler(this.btnGenerateTimeslots_Click);
            // 
            // btnGenerateAppointments
            // 
            this.btnGenerateAppointments.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnGenerateAppointments.Location = new System.Drawing.Point(13, 65);
            this.btnGenerateAppointments.Name = "btnGenerateAppointments";
            this.btnGenerateAppointments.Size = new System.Drawing.Size(201, 46);
            this.btnGenerateAppointments.TabIndex = 3;
            this.btnGenerateAppointments.Text = "Create Appointment";
            this.btnGenerateAppointments.UseVisualStyleBackColor = true;
            this.btnGenerateAppointments.Click += new System.EventHandler(this.btnGenerateAppointments_Click);
            // 
            // dateTimePicker1
            // 
            this.dateTimePicker1.CustomFormat = "dd/MM/yyyy";
            this.dateTimePicker1.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePicker1.Location = new System.Drawing.Point(220, 75);
            this.dateTimePicker1.Name = "dateTimePicker1";
            this.dateTimePicker1.Size = new System.Drawing.Size(200, 22);
            this.dateTimePicker1.TabIndex = 4;
            // 
            // btnWriteDataToSmartCard
            // 
            this.btnWriteDataToSmartCard.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnWriteDataToSmartCard.Location = new System.Drawing.Point(13, 158);
            this.btnWriteDataToSmartCard.Name = "btnWriteDataToSmartCard";
            this.btnWriteDataToSmartCard.Size = new System.Drawing.Size(201, 46);
            this.btnWriteDataToSmartCard.TabIndex = 5;
            this.btnWriteDataToSmartCard.Text = "Write data to smart card";
            this.btnWriteDataToSmartCard.UseVisualStyleBackColor = true;
            this.btnWriteDataToSmartCard.Click += new System.EventHandler(this.btnWriteDataToSmartCard_Click);
            // 
            // btnReaderData
            // 
            this.btnReaderData.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnReaderData.Location = new System.Drawing.Point(237, 158);
            this.btnReaderData.Name = "btnReaderData";
            this.btnReaderData.Size = new System.Drawing.Size(201, 46);
            this.btnReaderData.TabIndex = 6;
            this.btnReaderData.Text = "Read data";
            this.btnReaderData.UseVisualStyleBackColor = true;
            this.btnReaderData.Click += new System.EventHandler(this.btnReaderData_Click);
            // 
            // FormAppointmentDetails
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.ClientSize = new System.Drawing.Size(719, 598);
            this.Controls.Add(this.btnReaderData);
            this.Controls.Add(this.btnWriteDataToSmartCard);
            this.Controls.Add(this.dateTimePicker1);
            this.Controls.Add(this.btnGenerateAppointments);
            this.Controls.Add(this.btnGenerateTimeslots);
            this.Controls.Add(this.webBrowserAppointmentDetails);
            this.Controls.Add(this.btnPrint);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormAppointmentDetails";
            this.Text = "Appointment Details";
            this.Load += new System.EventHandler(this.FormLabelPrint_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnPrint;
        private System.Windows.Forms.WebBrowser webBrowserAppointmentDetails;
        private System.Windows.Forms.Button btnGenerateTimeslots;
        private System.Windows.Forms.Button btnGenerateAppointments;
        private System.Windows.Forms.DateTimePicker dateTimePicker1;
        private System.Windows.Forms.Button btnWriteDataToSmartCard;
        private System.Windows.Forms.Button btnReaderData;
    }
}