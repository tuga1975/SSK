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
            this.btnRotateImage = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnPrint
            // 
            this.btnPrint.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPrint.Location = new System.Drawing.Point(10, 11);
            this.btnPrint.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new System.Drawing.Size(114, 37);
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
            this.webBrowserAppointmentDetails.Location = new System.Drawing.Point(10, 233);
            this.webBrowserAppointmentDetails.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.webBrowserAppointmentDetails.MinimumSize = new System.Drawing.Size(15, 16);
            this.webBrowserAppointmentDetails.Name = "webBrowserAppointmentDetails";
            this.webBrowserAppointmentDetails.ScriptErrorsSuppressed = true;
            this.webBrowserAppointmentDetails.Size = new System.Drawing.Size(520, 243);
            this.webBrowserAppointmentDetails.TabIndex = 1;
            // 
            // btnGenerateTimeslots
            // 
            this.btnGenerateTimeslots.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnGenerateTimeslots.Location = new System.Drawing.Point(128, 11);
            this.btnGenerateTimeslots.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnGenerateTimeslots.Name = "btnGenerateTimeslots";
            this.btnGenerateTimeslots.Size = new System.Drawing.Size(120, 37);
            this.btnGenerateTimeslots.TabIndex = 2;
            this.btnGenerateTimeslots.Text = "Generate Timeslots";
            this.btnGenerateTimeslots.UseVisualStyleBackColor = true;
            this.btnGenerateTimeslots.Click += new System.EventHandler(this.btnGenerateTimeslots_Click);
            // 
            // btnGenerateAppointments
            // 
            this.btnGenerateAppointments.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnGenerateAppointments.Location = new System.Drawing.Point(10, 53);
            this.btnGenerateAppointments.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnGenerateAppointments.Name = "btnGenerateAppointments";
            this.btnGenerateAppointments.Size = new System.Drawing.Size(151, 37);
            this.btnGenerateAppointments.TabIndex = 3;
            this.btnGenerateAppointments.Text = "Create Appointment";
            this.btnGenerateAppointments.UseVisualStyleBackColor = true;
            this.btnGenerateAppointments.Click += new System.EventHandler(this.btnGenerateAppointments_Click);
            // 
            // dateTimePicker1
            // 
            this.dateTimePicker1.CustomFormat = "dd/MM/yyyy";
            this.dateTimePicker1.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePicker1.Location = new System.Drawing.Point(165, 61);
            this.dateTimePicker1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.dateTimePicker1.Name = "dateTimePicker1";
            this.dateTimePicker1.Size = new System.Drawing.Size(151, 20);
            this.dateTimePicker1.TabIndex = 4;
            // 
            // btnWriteDataToSmartCard
            // 
            this.btnWriteDataToSmartCard.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnWriteDataToSmartCard.Location = new System.Drawing.Point(10, 128);
            this.btnWriteDataToSmartCard.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnWriteDataToSmartCard.Name = "btnWriteDataToSmartCard";
            this.btnWriteDataToSmartCard.Size = new System.Drawing.Size(151, 37);
            this.btnWriteDataToSmartCard.TabIndex = 5;
            this.btnWriteDataToSmartCard.Text = "Write data to smart card";
            this.btnWriteDataToSmartCard.UseVisualStyleBackColor = true;
            this.btnWriteDataToSmartCard.Click += new System.EventHandler(this.btnWriteDataToSmartCard_Click);
            // 
            // btnReaderData
            // 
            this.btnReaderData.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnReaderData.Location = new System.Drawing.Point(178, 128);
            this.btnReaderData.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnReaderData.Name = "btnReaderData";
            this.btnReaderData.Size = new System.Drawing.Size(151, 37);
            this.btnReaderData.TabIndex = 6;
            this.btnReaderData.Text = "Read data";
            this.btnReaderData.UseVisualStyleBackColor = true;
            this.btnReaderData.Click += new System.EventHandler(this.btnReaderData_Click);
            // 
            // btnRotateImage
            // 
            this.btnRotateImage.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRotateImage.Location = new System.Drawing.Point(10, 169);
            this.btnRotateImage.Margin = new System.Windows.Forms.Padding(2);
            this.btnRotateImage.Name = "btnRotateImage";
            this.btnRotateImage.Size = new System.Drawing.Size(151, 37);
            this.btnRotateImage.TabIndex = 7;
            this.btnRotateImage.Text = "Rotate Image 90";
            this.btnRotateImage.UseVisualStyleBackColor = true;
            this.btnRotateImage.Click += new System.EventHandler(this.btnRotateImage_Click);
            // 
            // FormAppointmentDetails
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.ClientSize = new System.Drawing.Size(539, 486);
            this.Controls.Add(this.btnRotateImage);
            this.Controls.Add(this.btnReaderData);
            this.Controls.Add(this.btnWriteDataToSmartCard);
            this.Controls.Add(this.dateTimePicker1);
            this.Controls.Add(this.btnGenerateAppointments);
            this.Controls.Add(this.btnGenerateTimeslots);
            this.Controls.Add(this.webBrowserAppointmentDetails);
            this.Controls.Add(this.btnPrint);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
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
        private System.Windows.Forms.Button btnRotateImage;
    }
}