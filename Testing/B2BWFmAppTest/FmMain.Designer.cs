namespace B2BWFmAppTest
{
	partial class FmMain
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
			this.txtAmount = new System.Windows.Forms.TextBox();
			this.txtDocNo = new System.Windows.Forms.TextBox();
			this.btnCashMachineStatus = new System.Windows.Forms.Button();
			this.btnStartPayment = new System.Windows.Forms.Button();
			this.txtMsg = new System.Windows.Forms.TextBox();
			this.btnClearMsg = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(96, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Payment Amount : ";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 38);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(85, 13);
			this.label2.TabIndex = 1;
			this.label2.Text = "Document No. : ";
			// 
			// txtAmount
			// 
			this.txtAmount.Location = new System.Drawing.Point(114, 6);
			this.txtAmount.Name = "txtAmount";
			this.txtAmount.Size = new System.Drawing.Size(131, 20);
			this.txtAmount.TabIndex = 2;
			this.txtAmount.Text = "0.00";
			// 
			// txtDocNo
			// 
			this.txtDocNo.Location = new System.Drawing.Point(114, 35);
			this.txtDocNo.Name = "txtDocNo";
			this.txtDocNo.Size = new System.Drawing.Size(131, 20);
			this.txtDocNo.TabIndex = 3;
			this.txtDocNo.Text = "TKxTestA0001";
			// 
			// btnCashMachineStatus
			// 
			this.btnCashMachineStatus.Location = new System.Drawing.Point(15, 68);
			this.btnCashMachineStatus.Name = "btnCashMachineStatus";
			this.btnCashMachineStatus.Size = new System.Drawing.Size(198, 23);
			this.btnCashMachineStatus.TabIndex = 4;
			this.btnCashMachineStatus.Text = "Is Cash Machine Ready";
			this.btnCashMachineStatus.UseVisualStyleBackColor = true;
			this.btnCashMachineStatus.Click += new System.EventHandler(this.btnCashMachineStatus_Click);
			// 
			// btnStartPayment
			// 
			this.btnStartPayment.Location = new System.Drawing.Point(228, 68);
			this.btnStartPayment.Name = "btnStartPayment";
			this.btnStartPayment.Size = new System.Drawing.Size(198, 23);
			this.btnStartPayment.TabIndex = 5;
			this.btnStartPayment.Text = "Start Payment";
			this.btnStartPayment.UseVisualStyleBackColor = true;
			this.btnStartPayment.Click += new System.EventHandler(this.btnStartPayment_Click);
			// 
			// txtMsg
			// 
			this.txtMsg.AcceptsReturn = true;
			this.txtMsg.AcceptsTab = true;
			this.txtMsg.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtMsg.Location = new System.Drawing.Point(15, 97);
			this.txtMsg.Multiline = true;
			this.txtMsg.Name = "txtMsg";
			this.txtMsg.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtMsg.Size = new System.Drawing.Size(410, 364);
			this.txtMsg.TabIndex = 6;
			this.txtMsg.WordWrap = false;
			// 
			// btnClearMsg
			// 
			this.btnClearMsg.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnClearMsg.Location = new System.Drawing.Point(227, 467);
			this.btnClearMsg.Name = "btnClearMsg";
			this.btnClearMsg.Size = new System.Drawing.Size(198, 36);
			this.btnClearMsg.TabIndex = 7;
			this.btnClearMsg.Text = "Clear Msg";
			this.btnClearMsg.UseVisualStyleBackColor = true;
			this.btnClearMsg.Click += new System.EventHandler(this.btnClearMsg_Click);
			// 
			// FmMain
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(432, 515);
			this.Controls.Add(this.btnClearMsg);
			this.Controls.Add(this.txtMsg);
			this.Controls.Add(this.btnStartPayment);
			this.Controls.Add(this.btnCashMachineStatus);
			this.Controls.Add(this.txtDocNo);
			this.Controls.Add(this.txtAmount);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Name = "FmMain";
			this.Text = "FmMain";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FmMain_FormClosing);
			this.Load += new System.EventHandler(this.FmMain_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox txtAmount;
		private System.Windows.Forms.TextBox txtDocNo;
		private System.Windows.Forms.Button btnCashMachineStatus;
		private System.Windows.Forms.Button btnStartPayment;
		private System.Windows.Forms.TextBox txtMsg;
		private System.Windows.Forms.Button btnClearMsg;
	}
}

