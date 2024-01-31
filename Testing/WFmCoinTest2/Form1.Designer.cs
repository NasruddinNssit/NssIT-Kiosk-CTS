namespace WFmCoinTest2
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
			this.txtMsg = new System.Windows.Forms.TextBox();
			this.btnStartCoinMach = new System.Windows.Forms.Button();
			this.btnEndCoinMach = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.txtDispenseAmount = new System.Windows.Forms.TextBox();
			this.btnDispenseCoin = new System.Windows.Forms.Button();
			this.txtDocNo = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// txtMsg
			// 
			this.txtMsg.AcceptsReturn = true;
			this.txtMsg.AcceptsTab = true;
			this.txtMsg.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtMsg.Location = new System.Drawing.Point(12, 118);
			this.txtMsg.Multiline = true;
			this.txtMsg.Name = "txtMsg";
			this.txtMsg.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtMsg.Size = new System.Drawing.Size(553, 253);
			this.txtMsg.TabIndex = 0;
			this.txtMsg.WordWrap = false;
			// 
			// btnStartCoinMach
			// 
			this.btnStartCoinMach.Location = new System.Drawing.Point(12, 12);
			this.btnStartCoinMach.Name = "btnStartCoinMach";
			this.btnStartCoinMach.Size = new System.Drawing.Size(161, 23);
			this.btnStartCoinMach.TabIndex = 1;
			this.btnStartCoinMach.Text = "Start Coin Machine";
			this.btnStartCoinMach.UseVisualStyleBackColor = true;
			this.btnStartCoinMach.Click += new System.EventHandler(this.btnStartCoinMach_Click);
			// 
			// btnEndCoinMach
			// 
			this.btnEndCoinMach.Location = new System.Drawing.Point(179, 12);
			this.btnEndCoinMach.Name = "btnEndCoinMach";
			this.btnEndCoinMach.Size = new System.Drawing.Size(161, 23);
			this.btnEndCoinMach.TabIndex = 2;
			this.btnEndCoinMach.Text = "End Coin Machine";
			this.btnEndCoinMach.UseVisualStyleBackColor = true;
			this.btnEndCoinMach.Click += new System.EventHandler(this.btnEndCoinMach_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 71);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(125, 13);
			this.label1.TabIndex = 3;
			this.label1.Text = "Dispense Amount (RM) : ";
			// 
			// txtDispenseAmount
			// 
			this.txtDispenseAmount.Location = new System.Drawing.Point(133, 68);
			this.txtDispenseAmount.Name = "txtDispenseAmount";
			this.txtDispenseAmount.Size = new System.Drawing.Size(163, 20);
			this.txtDispenseAmount.TabIndex = 4;
			this.txtDispenseAmount.Text = "0.80";
			// 
			// btnDispenseCoin
			// 
			this.btnDispenseCoin.Location = new System.Drawing.Point(302, 66);
			this.btnDispenseCoin.Name = "btnDispenseCoin";
			this.btnDispenseCoin.Size = new System.Drawing.Size(161, 23);
			this.btnDispenseCoin.TabIndex = 5;
			this.btnDispenseCoin.Text = "Dispense Coin";
			this.btnDispenseCoin.UseVisualStyleBackColor = true;
			this.btnDispenseCoin.Click += new System.EventHandler(this.btnDispenseCoin_Click);
			// 
			// txtDocNo
			// 
			this.txtDocNo.Location = new System.Drawing.Point(133, 41);
			this.txtDocNo.Name = "txtDocNo";
			this.txtDocNo.Size = new System.Drawing.Size(163, 20);
			this.txtDocNo.TabIndex = 7;
			this.txtDocNo.Text = "TestDoc_X001";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 44);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(65, 13);
			this.label2.TabIndex = 6;
			this.label2.Text = "Sales Doc : ";
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(577, 383);
			this.Controls.Add(this.txtDocNo);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.btnDispenseCoin);
			this.Controls.Add(this.txtDispenseAmount);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.btnEndCoinMach);
			this.Controls.Add(this.btnStartCoinMach);
			this.Controls.Add(this.txtMsg);
			this.Name = "Form1";
			this.Text = "WFm Coin Test 2";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox txtMsg;
		private System.Windows.Forms.Button btnStartCoinMach;
		private System.Windows.Forms.Button btnEndCoinMach;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox txtDispenseAmount;
		private System.Windows.Forms.Button btnDispenseCoin;
		private System.Windows.Forms.TextBox txtDocNo;
		private System.Windows.Forms.Label label2;
	}
}

