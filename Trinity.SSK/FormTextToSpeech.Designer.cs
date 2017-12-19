namespace SSK
{
    partial class FormTextToSpeech
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
            this.label1 = new System.Windows.Forms.Label();
            this.btnSpeak = new System.Windows.Forms.Button();
            this.txtTextToSpeech = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(107, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Text to Speech:";
            // 
            // btnSpeak
            // 
            this.btnSpeak.Location = new System.Drawing.Point(127, 101);
            this.btnSpeak.Name = "btnSpeak";
            this.btnSpeak.Size = new System.Drawing.Size(128, 36);
            this.btnSpeak.TabIndex = 2;
            this.btnSpeak.Text = "Speak";
            this.btnSpeak.UseVisualStyleBackColor = true;
            this.btnSpeak.Click += new System.EventHandler(this.btnSpeak_Click);
            // 
            // txtTextToSpeech
            // 
            this.txtTextToSpeech.Location = new System.Drawing.Point(127, 13);
            this.txtTextToSpeech.Name = "txtTextToSpeech";
            this.txtTextToSpeech.Size = new System.Drawing.Size(458, 82);
            this.txtTextToSpeech.TabIndex = 3;
            this.txtTextToSpeech.Text = "";
            // 
            // FormTextToSpeech
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(597, 326);
            this.Controls.Add(this.txtTextToSpeech);
            this.Controls.Add(this.btnSpeak);
            this.Controls.Add(this.label1);
            this.Name = "FormTextToSpeech";
            this.Text = "FormTextToSpeech";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnSpeak;
        private System.Windows.Forms.RichTextBox txtTextToSpeech;
    }
}