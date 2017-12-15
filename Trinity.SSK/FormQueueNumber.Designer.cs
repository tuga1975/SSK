namespace SSK
{
    partial class FormQueueNumber
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormQueueNumber));
            this.lblQueueNumber = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblQueueNumber
            // 
            this.lblQueueNumber.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblQueueNumber.Font = new System.Drawing.Font("Microsoft Sans Serif", 72F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblQueueNumber.Location = new System.Drawing.Point(0, 0);
            this.lblQueueNumber.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblQueueNumber.Name = "lblQueueNumber";
            this.lblQueueNumber.Size = new System.Drawing.Size(664, 308);
            this.lblQueueNumber.TabIndex = 0;
            this.lblQueueNumber.Text = "WAITING FOR YOUR NUMBER...";
            this.lblQueueNumber.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // FormQueueNumber
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(664, 308);
            this.Controls.Add(this.lblQueueNumber);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "FormQueueNumber";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblQueueNumber;
    }
}