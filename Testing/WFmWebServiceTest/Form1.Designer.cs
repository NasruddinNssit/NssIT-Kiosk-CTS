namespace WFmWebServiceTest
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
            this.btnGetTimeStamp = new System.Windows.Forms.Button();
            this.txtTimeStamp = new System.Windows.Forms.TextBox();
            this.btnLogin = new System.Windows.Forms.Button();
            this.txtLogin = new System.Windows.Forms.TextBox();
            this.btnWriteReg = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnGetTimeStamp
            // 
            this.btnGetTimeStamp.Location = new System.Drawing.Point(12, 12);
            this.btnGetTimeStamp.Name = "btnGetTimeStamp";
            this.btnGetTimeStamp.Size = new System.Drawing.Size(169, 23);
            this.btnGetTimeStamp.TabIndex = 0;
            this.btnGetTimeStamp.Text = "Get TimeStamp";
            this.btnGetTimeStamp.UseVisualStyleBackColor = true;
            this.btnGetTimeStamp.Click += new System.EventHandler(this.btnGetTimeStamp_Click);
            // 
            // txtTimeStamp
            // 
            this.txtTimeStamp.Location = new System.Drawing.Point(187, 12);
            this.txtTimeStamp.Name = "txtTimeStamp";
            this.txtTimeStamp.Size = new System.Drawing.Size(290, 20);
            this.txtTimeStamp.TabIndex = 1;
            // 
            // btnLogin
            // 
            this.btnLogin.Location = new System.Drawing.Point(12, 41);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(169, 23);
            this.btnLogin.TabIndex = 2;
            this.btnLogin.Text = "Login In";
            this.btnLogin.UseVisualStyleBackColor = true;
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
            // 
            // txtLogin
            // 
            this.txtLogin.Location = new System.Drawing.Point(187, 44);
            this.txtLogin.Multiline = true;
            this.txtLogin.Name = "txtLogin";
            this.txtLogin.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtLogin.Size = new System.Drawing.Size(416, 115);
            this.txtLogin.TabIndex = 3;
            this.txtLogin.WordWrap = false;
            // 
            // btnWriteReg
            // 
            this.btnWriteReg.Location = new System.Drawing.Point(569, 12);
            this.btnWriteReg.Name = "btnWriteReg";
            this.btnWriteReg.Size = new System.Drawing.Size(169, 23);
            this.btnWriteReg.TabIndex = 4;
            this.btnWriteReg.Text = "Write Registry";
            this.btnWriteReg.UseVisualStyleBackColor = true;
            this.btnWriteReg.Click += new System.EventHandler(this.btnWriteReg_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btnWriteReg);
            this.Controls.Add(this.txtLogin);
            this.Controls.Add(this.btnLogin);
            this.Controls.Add(this.txtTimeStamp);
            this.Controls.Add(this.btnGetTimeStamp);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button btnGetTimeStamp;
		private System.Windows.Forms.TextBox txtTimeStamp;
		private System.Windows.Forms.Button btnLogin;
		private System.Windows.Forms.TextBox txtLogin;
		private System.Windows.Forms.Button btnWriteReg;
	}
}

