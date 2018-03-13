namespace Experiment
{
    partial class FormTestSignalR
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
            this.label2 = new System.Windows.Forms.Label();
            this.txtFromUser = new System.Windows.Forms.TextBox();
            this.txtToUser = new System.Windows.Forms.TextBox();
            this.btnSend = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.txtSubject = new System.Windows.Forms.TextBox();
            this.txtContent = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(78, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "From User:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 54);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 17);
            this.label2.TabIndex = 1;
            this.label2.Text = "To User:";
            // 
            // txtFromUser
            // 
            this.txtFromUser.Location = new System.Drawing.Point(115, 13);
            this.txtFromUser.Name = "txtFromUser";
            this.txtFromUser.Size = new System.Drawing.Size(474, 22);
            this.txtFromUser.TabIndex = 0;
            this.txtFromUser.Text = "f1748cb4-3bb5-4129-852d-2aba28bb8cec";
            // 
            // txtToUser
            // 
            this.txtToUser.Location = new System.Drawing.Point(115, 54);
            this.txtToUser.Name = "txtToUser";
            this.txtToUser.Size = new System.Drawing.Size(474, 22);
            this.txtToUser.TabIndex = 1;
            this.txtToUser.Text = "06a91b1b-99c3-428d-8a55-83892c2adf4c";
            // 
            // btnSend
            // 
            this.btnSend.Location = new System.Drawing.Point(115, 251);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(115, 41);
            this.btnSend.TabIndex = 4;
            this.btnSend.Text = "Send";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 107);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(59, 17);
            this.label3.TabIndex = 5;
            this.label3.Text = "Subject:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 144);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(61, 17);
            this.label4.TabIndex = 6;
            this.label4.Text = "Content:";
            // 
            // txtSubject
            // 
            this.txtSubject.Location = new System.Drawing.Point(115, 104);
            this.txtSubject.Name = "txtSubject";
            this.txtSubject.Size = new System.Drawing.Size(474, 22);
            this.txtSubject.TabIndex = 2;
            // 
            // txtContent
            // 
            this.txtContent.Location = new System.Drawing.Point(115, 144);
            this.txtContent.Name = "txtContent";
            this.txtContent.Size = new System.Drawing.Size(474, 89);
            this.txtContent.TabIndex = 3;
            this.txtContent.Text = "";
            // 
            // FormTestSignalR
            // 
            this.AcceptButton = this.btnSend;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(601, 346);
            this.Controls.Add(this.txtContent);
            this.Controls.Add(this.txtSubject);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnSend);
            this.Controls.Add(this.txtToUser);
            this.Controls.Add(this.txtFromUser);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "FormTestSignalR";
            this.Text = "FormTestSignalR";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtFromUser;
        private System.Windows.Forms.TextBox txtToUser;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtSubject;
        private System.Windows.Forms.RichTextBox txtContent;
    }
}