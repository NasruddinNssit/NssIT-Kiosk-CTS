namespace WFmTCPServiceTest01
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
			this.label1 = new System.Windows.Forms.Label();
			this.txtLocSvcPort = new System.Windows.Forms.TextBox();
			this.btnStart = new System.Windows.Forms.Button();
			this.btnEnd = new System.Windows.Forms.Button();
			this.label3 = new System.Windows.Forms.Label();
			this.txtSendMsg = new System.Windows.Forms.TextBox();
			this.btnSend = new System.Windows.Forms.Button();
			this.txtMsg = new System.Windows.Forms.TextBox();
			this.cboClientPortList = new System.Windows.Forms.ComboBox();
			this.label4 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 18);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(103, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Local Service Port : ";
			// 
			// txtLocSvcPort
			// 
			this.txtLocSvcPort.Location = new System.Drawing.Point(149, 15);
			this.txtLocSvcPort.Name = "txtLocSvcPort";
			this.txtLocSvcPort.Size = new System.Drawing.Size(133, 20);
			this.txtLocSvcPort.TabIndex = 2;
			this.txtLocSvcPort.Text = "9838";
			// 
			// btnStart
			// 
			this.btnStart.Location = new System.Drawing.Point(298, 15);
			this.btnStart.Name = "btnStart";
			this.btnStart.Size = new System.Drawing.Size(121, 49);
			this.btnStart.TabIndex = 4;
			this.btnStart.Text = "Start Local Service";
			this.btnStart.UseVisualStyleBackColor = true;
			this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
			// 
			// btnEnd
			// 
			this.btnEnd.Location = new System.Drawing.Point(434, 15);
			this.btnEnd.Name = "btnEnd";
			this.btnEnd.Size = new System.Drawing.Size(121, 49);
			this.btnEnd.TabIndex = 5;
			this.btnEnd.Text = "End Local Service";
			this.btnEnd.UseVisualStyleBackColor = true;
			this.btnEnd.Click += new System.EventHandler(this.btnEnd_Click);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(12, 79);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(87, 13);
			this.label3.TabIndex = 6;
			this.label3.Text = "Send Message : ";
			// 
			// txtSendMsg
			// 
			this.txtSendMsg.Location = new System.Drawing.Point(149, 76);
			this.txtSendMsg.Name = "txtSendMsg";
			this.txtSendMsg.Size = new System.Drawing.Size(406, 20);
			this.txtSendMsg.TabIndex = 7;
			// 
			// btnSend
			// 
			this.btnSend.Location = new System.Drawing.Point(561, 67);
			this.btnSend.Name = "btnSend";
			this.btnSend.Size = new System.Drawing.Size(94, 37);
			this.btnSend.TabIndex = 8;
			this.btnSend.Text = "Send";
			this.btnSend.UseVisualStyleBackColor = true;
			this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
			// 
			// txtMsg
			// 
			this.txtMsg.AcceptsReturn = true;
			this.txtMsg.AcceptsTab = true;
			this.txtMsg.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtMsg.Location = new System.Drawing.Point(15, 110);
			this.txtMsg.Multiline = true;
			this.txtMsg.Name = "txtMsg";
			this.txtMsg.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtMsg.Size = new System.Drawing.Size(640, 239);
			this.txtMsg.TabIndex = 9;
			// 
			// cboClientPortList
			// 
			this.cboClientPortList.FormattingEnabled = true;
			this.cboClientPortList.Location = new System.Drawing.Point(149, 43);
			this.cboClientPortList.Name = "cboClientPortList";
			this.cboClientPortList.Size = new System.Drawing.Size(133, 21);
			this.cboClientPortList.TabIndex = 10;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(12, 46);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(91, 13);
			this.label4.TabIndex = 11;
			this.label4.Text = "Destination Port : ";
			// 
			// Main
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(660, 361);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.cboClientPortList);
			this.Controls.Add(this.txtMsg);
			this.Controls.Add(this.btnSend);
			this.Controls.Add(this.txtSendMsg);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.btnEnd);
			this.Controls.Add(this.btnStart);
			this.Controls.Add(this.txtLocSvcPort);
			this.Controls.Add(this.label1);
			this.Name = "Main";
			this.Text = "Main - TCP Service Test 01";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Main_FormClosing);
			this.Load += new System.EventHandler(this.Main_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox txtLocSvcPort;
		private System.Windows.Forms.Button btnStart;
		private System.Windows.Forms.Button btnEnd;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox txtSendMsg;
		private System.Windows.Forms.Button btnSend;
		private System.Windows.Forms.TextBox txtMsg;
		private System.Windows.Forms.ComboBox cboClientPortList;
		private System.Windows.Forms.Label label4;
	}
}

