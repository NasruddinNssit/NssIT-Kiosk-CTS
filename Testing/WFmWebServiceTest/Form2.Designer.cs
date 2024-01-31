namespace WFmWebServiceTest
{
	partial class Form2
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
			this.txtMsg = new System.Windows.Forms.TextBox();
			this.btnLogin = new System.Windows.Forms.Button();
			this.txtToken = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.btnGetDestination = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// txtMsg
			// 
			this.txtMsg.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtMsg.Location = new System.Drawing.Point(12, 121);
			this.txtMsg.Multiline = true;
			this.txtMsg.Name = "txtMsg";
			this.txtMsg.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtMsg.Size = new System.Drawing.Size(613, 249);
			this.txtMsg.TabIndex = 7;
			this.txtMsg.WordWrap = false;
			// 
			// btnLogin
			// 
			this.btnLogin.Location = new System.Drawing.Point(12, 12);
			this.btnLogin.Name = "btnLogin";
			this.btnLogin.Size = new System.Drawing.Size(93, 23);
			this.btnLogin.TabIndex = 6;
			this.btnLogin.Text = "Login In";
			this.btnLogin.UseVisualStyleBackColor = true;
			this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
			// 
			// txtToken
			// 
			this.txtToken.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtToken.Location = new System.Drawing.Point(161, 14);
			this.txtToken.Name = "txtToken";
			this.txtToken.Size = new System.Drawing.Size(464, 20);
			this.txtToken.TabIndex = 8;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(111, 17);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(44, 13);
			this.label1.TabIndex = 9;
			this.label1.Text = "Token :";
			// 
			// btnGetDestination
			// 
			this.btnGetDestination.Location = new System.Drawing.Point(12, 41);
			this.btnGetDestination.Name = "btnGetDestination";
			this.btnGetDestination.Size = new System.Drawing.Size(143, 23);
			this.btnGetDestination.TabIndex = 10;
			this.btnGetDestination.Text = "Get Destination";
			this.btnGetDestination.UseVisualStyleBackColor = true;
			this.btnGetDestination.Click += new System.EventHandler(this.btnGetDestination_Click);
			// 
			// Form2
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(637, 382);
			this.Controls.Add(this.btnGetDestination);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.txtToken);
			this.Controls.Add(this.txtMsg);
			this.Controls.Add(this.btnLogin);
			this.Name = "Form2";
			this.Text = "Form2";
			this.Load += new System.EventHandler(this.Form2_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox txtMsg;
		private System.Windows.Forms.Button btnLogin;
		private System.Windows.Forms.TextBox txtToken;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button btnGetDestination;
	}
}