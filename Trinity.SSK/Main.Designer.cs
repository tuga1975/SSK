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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.LayerWeb = new System.Windows.Forms.WebBrowser();
            this.SuspendLayout();
            // 
            // LayerWeb
            // 
            this.LayerWeb.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LayerWeb.Location = new System.Drawing.Point(0, 0);
            this.LayerWeb.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.LayerWeb.MinimumSize = new System.Drawing.Size(27, 25);
            this.LayerWeb.Name = "LayerWeb";
            this.LayerWeb.Size = new System.Drawing.Size(1201, 716);
            this.LayerWeb.TabIndex = 0;
            this.LayerWeb.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.LayerWeb_DocumentCompleted);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1201, 716);
            this.Controls.Add(this.LayerWeb);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Self-Servie Kiosk";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Main_FormClosing);
            this.Load += new System.EventHandler(this.Main_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.WebBrowser LayerWeb;
    }
}

