namespace NssIT.Kiosk.Device.Telequip.CX2_3.OrgApi
{
	partial class Form4
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
			this.btnReleaseCOMObj = new System.Windows.Forms.Button();
			this.btnDispense01 = new System.Windows.Forms.Button();
			this.txtCoinAmount = new System.Windows.Forms.TextBox();
			this.txtMsg = new System.Windows.Forms.TextBox();
			this.btnStart = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// btnReleaseCOMObj
			// 
			this.btnReleaseCOMObj.Location = new System.Drawing.Point(364, 66);
			this.btnReleaseCOMObj.Name = "btnReleaseCOMObj";
			this.btnReleaseCOMObj.Size = new System.Drawing.Size(100, 64);
			this.btnReleaseCOMObj.TabIndex = 64;
			this.btnReleaseCOMObj.Text = "Release COM Obj";
			this.btnReleaseCOMObj.UseVisualStyleBackColor = true;
			this.btnReleaseCOMObj.Click += new System.EventHandler(this.btnReleaseCOMObj_Click);
			// 
			// btnDispense01
			// 
			this.btnDispense01.Location = new System.Drawing.Point(258, 27);
			this.btnDispense01.Name = "btnDispense01";
			this.btnDispense01.Size = new System.Drawing.Size(84, 23);
			this.btnDispense01.TabIndex = 63;
			this.btnDispense01.Text = "Dispense";
			this.btnDispense01.UseVisualStyleBackColor = true;
			this.btnDispense01.Click += new System.EventHandler(this.btnDispense01_Click);
			// 
			// txtCoinAmount
			// 
			this.txtCoinAmount.Location = new System.Drawing.Point(195, 30);
			this.txtCoinAmount.Name = "txtCoinAmount";
			this.txtCoinAmount.Size = new System.Drawing.Size(57, 20);
			this.txtCoinAmount.TabIndex = 62;
			this.txtCoinAmount.Text = "0.80";
			// 
			// txtMsg
			// 
			this.txtMsg.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtMsg.Location = new System.Drawing.Point(12, 136);
			this.txtMsg.Multiline = true;
			this.txtMsg.Name = "txtMsg";
			this.txtMsg.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtMsg.Size = new System.Drawing.Size(449, 451);
			this.txtMsg.TabIndex = 61;
			this.txtMsg.WordWrap = false;
			// 
			// btnStart
			// 
			this.btnStart.Location = new System.Drawing.Point(12, 6);
			this.btnStart.Name = "btnStart";
			this.btnStart.Size = new System.Drawing.Size(177, 66);
			this.btnStart.TabIndex = 60;
			this.btnStart.Text = "Start";
			this.btnStart.UseVisualStyleBackColor = true;
			this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
			// 
			// Form4
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(473, 599);
			this.Controls.Add(this.btnReleaseCOMObj);
			this.Controls.Add(this.btnDispense01);
			this.Controls.Add(this.txtCoinAmount);
			this.Controls.Add(this.txtMsg);
			this.Controls.Add(this.btnStart);
			this.Name = "Form4";
			this.Text = "Form4 - NssIT.Kiosk.Device.Telequip.CX2_3.OrgApi";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form4_FormClosing);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button btnReleaseCOMObj;
		private System.Windows.Forms.Button btnDispense01;
		private System.Windows.Forms.TextBox txtCoinAmount;
		private System.Windows.Forms.TextBox txtMsg;
		private System.Windows.Forms.Button btnStart;
	}
}