namespace Experiment
{
    partial class FormTestBackendAPI
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
            this.txtNRIC = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnGetUserFingerprint = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.txtURL = new System.Windows.Forms.TextBox();
            this.btnSaveToFile = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtNRIC
            // 
            this.txtNRIC.Location = new System.Drawing.Point(76, 40);
            this.txtNRIC.Name = "txtNRIC";
            this.txtNRIC.Size = new System.Drawing.Size(195, 22);
            this.txtNRIC.TabIndex = 0;
            this.txtNRIC.Text = "S3456789A";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(26, 43);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 17);
            this.label1.TabIndex = 1;
            this.label1.Text = "NRIC:";
            // 
            // btnGetUserFingerprint
            // 
            this.btnGetUserFingerprint.Location = new System.Drawing.Point(76, 69);
            this.btnGetUserFingerprint.Name = "btnGetUserFingerprint";
            this.btnGetUserFingerprint.Size = new System.Drawing.Size(195, 36);
            this.btnGetUserFingerprint.TabIndex = 2;
            this.btnGetUserFingerprint.Text = "Get fingerprint";
            this.btnGetUserFingerprint.UseVisualStyleBackColor = true;
            this.btnGetUserFingerprint.Click += new System.EventHandler(this.btnGetUserFingerprint_ClickAsync);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(26, 15);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(40, 17);
            this.label2.TabIndex = 4;
            this.label2.Text = "URL:";
            // 
            // txtURL
            // 
            this.txtURL.Location = new System.Drawing.Point(76, 12);
            this.txtURL.Name = "txtURL";
            this.txtURL.Size = new System.Drawing.Size(606, 22);
            this.txtURL.TabIndex = 3;
            this.txtURL.Text = "http://192.168.50.132:1122/api/SSP/SSPAuthenticate?NRIC=S3456789A";
            // 
            // btnSaveToFile
            // 
            this.btnSaveToFile.Enabled = false;
            this.btnSaveToFile.Location = new System.Drawing.Point(277, 69);
            this.btnSaveToFile.Name = "btnSaveToFile";
            this.btnSaveToFile.Size = new System.Drawing.Size(195, 36);
            this.btnSaveToFile.TabIndex = 5;
            this.btnSaveToFile.Text = "Save to file";
            this.btnSaveToFile.UseVisualStyleBackColor = true;
            this.btnSaveToFile.Click += new System.EventHandler(this.btnSaveToFile_Click);
            // 
            // FormTestBackendAPI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(694, 367);
            this.Controls.Add(this.btnSaveToFile);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtURL);
            this.Controls.Add(this.btnGetUserFingerprint);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtNRIC);
            this.Name = "FormTestBackendAPI";
            this.Text = "Test fingerprint";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtNRIC;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnGetUserFingerprint;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtURL;
        private System.Windows.Forms.Button btnSaveToFile;
    }
}