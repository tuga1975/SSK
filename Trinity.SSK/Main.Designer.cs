namespace SSK
{
    partial class Main
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
            this.LayerWeb = new System.Windows.Forms.WebBrowser();
            this.SuspendLayout();
            // 
            // LayerWeb
            // 
            this.LayerWeb.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LayerWeb.Location = new System.Drawing.Point(0, 0);
            this.LayerWeb.MinimumSize = new System.Drawing.Size(20, 20);
            this.LayerWeb.Name = "LayerWeb";
            this.LayerWeb.Size = new System.Drawing.Size(901, 582);
            this.LayerWeb.TabIndex = 0;
            this.LayerWeb.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.LayerWeb_DocumentCompleted);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(901, 582);
            this.Controls.Add(this.LayerWeb);
            this.Name = "Main";
            this.Text = "SSK-Kiosk";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Main_FormClosing);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.WebBrowser LayerWeb;
    }
}

