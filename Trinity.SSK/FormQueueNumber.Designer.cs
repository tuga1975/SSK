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
            this.wbQueueNumber = new System.Windows.Forms.WebBrowser();
            this.SuspendLayout();
            // 
            // wbQueueNumber
            // 
            this.wbQueueNumber.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wbQueueNumber.Location = new System.Drawing.Point(0, 0);
            this.wbQueueNumber.MinimumSize = new System.Drawing.Size(20, 20);
            this.wbQueueNumber.Name = "wbQueueNumber";
            this.wbQueueNumber.Size = new System.Drawing.Size(885, 379);
            this.wbQueueNumber.TabIndex = 0;
            this.wbQueueNumber.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.wbQueueNumber_DocumentCompleted);
            // 
            // FormQueueNumber
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(885, 379);
            this.Controls.Add(this.wbQueueNumber);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "FormQueueNumber";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.WebBrowser wbQueueNumber;
    }
}