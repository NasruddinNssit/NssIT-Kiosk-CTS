namespace B2BWFmAppTest
{
	partial class FmB2BUI
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
			this.lblPrice = new System.Windows.Forms.Label();
			this.lblCountDown = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.lblPleasePay = new System.Windows.Forms.Label();
			this.txtAcceptedBanknote = new System.Windows.Forms.TextBox();
			this.txtCustomerMsg = new System.Windows.Forms.TextBox();
			this.txtErrorMsg = new System.Windows.Forms.TextBox();
			this.btnCancelSales = new System.Windows.Forms.Button();
			this.txtProcessingMsg = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// lblPrice
			// 
			this.lblPrice.AutoSize = true;
			this.lblPrice.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblPrice.Location = new System.Drawing.Point(12, 9);
			this.lblPrice.Name = "lblPrice";
			this.lblPrice.Size = new System.Drawing.Size(101, 26);
			this.lblPrice.TabIndex = 0;
			this.lblPrice.Text = "RM 0.00";
			// 
			// lblCountDown
			// 
			this.lblCountDown.AutoSize = true;
			this.lblCountDown.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblCountDown.Location = new System.Drawing.Point(344, 9);
			this.lblCountDown.Name = "lblCountDown";
			this.lblCountDown.Size = new System.Drawing.Size(135, 26);
			this.lblCountDown.TabIndex = 1;
			this.lblCountDown.Text = "CountDown";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(111, 52);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(69, 13);
			this.label3.TabIndex = 2;
			this.label3.Text = "Please Pay : ";
			// 
			// lblPleasePay
			// 
			this.lblPleasePay.AutoSize = true;
			this.lblPleasePay.Location = new System.Drawing.Point(203, 52);
			this.lblPleasePay.Name = "lblPleasePay";
			this.lblPleasePay.Size = new System.Drawing.Size(66, 13);
			this.lblPleasePay.TabIndex = 3;
			this.lblPleasePay.Text = "MoneyCome";
			// 
			// txtAcceptedBanknote
			// 
			this.txtAcceptedBanknote.AcceptsReturn = true;
			this.txtAcceptedBanknote.AcceptsTab = true;
			this.txtAcceptedBanknote.Location = new System.Drawing.Point(12, 77);
			this.txtAcceptedBanknote.Multiline = true;
			this.txtAcceptedBanknote.Name = "txtAcceptedBanknote";
			this.txtAcceptedBanknote.Size = new System.Drawing.Size(82, 282);
			this.txtAcceptedBanknote.TabIndex = 4;
			this.txtAcceptedBanknote.Text = "$$";
			this.txtAcceptedBanknote.WordWrap = false;
			// 
			// txtCustomerMsg
			// 
			this.txtCustomerMsg.BackColor = System.Drawing.Color.Aqua;
			this.txtCustomerMsg.Location = new System.Drawing.Point(110, 77);
			this.txtCustomerMsg.Multiline = true;
			this.txtCustomerMsg.Name = "txtCustomerMsg";
			this.txtCustomerMsg.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtCustomerMsg.Size = new System.Drawing.Size(369, 138);
			this.txtCustomerMsg.TabIndex = 5;
			this.txtCustomerMsg.Text = "Customer Msg";
			this.txtCustomerMsg.WordWrap = false;
			// 
			// txtErrorMsg
			// 
			this.txtErrorMsg.BackColor = System.Drawing.Color.Yellow;
			this.txtErrorMsg.Location = new System.Drawing.Point(110, 221);
			this.txtErrorMsg.Multiline = true;
			this.txtErrorMsg.Name = "txtErrorMsg";
			this.txtErrorMsg.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtErrorMsg.Size = new System.Drawing.Size(369, 138);
			this.txtErrorMsg.TabIndex = 6;
			this.txtErrorMsg.Text = "Error Msg";
			this.txtErrorMsg.WordWrap = false;
			// 
			// btnCancelSales
			// 
			this.btnCancelSales.Location = new System.Drawing.Point(338, 365);
			this.btnCancelSales.Name = "btnCancelSales";
			this.btnCancelSales.Size = new System.Drawing.Size(141, 23);
			this.btnCancelSales.TabIndex = 7;
			this.btnCancelSales.Text = "Cancel Sales";
			this.btnCancelSales.UseVisualStyleBackColor = true;
			this.btnCancelSales.Click += new System.EventHandler(this.btnCancelSales_Click);
			// 
			// txtProcessingMsg
			// 
			this.txtProcessingMsg.BackColor = System.Drawing.SystemColors.Window;
			this.txtProcessingMsg.Location = new System.Drawing.Point(12, 394);
			this.txtProcessingMsg.Multiline = true;
			this.txtProcessingMsg.Name = "txtProcessingMsg";
			this.txtProcessingMsg.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtProcessingMsg.Size = new System.Drawing.Size(467, 95);
			this.txtProcessingMsg.TabIndex = 8;
			this.txtProcessingMsg.Text = "Processing Message";
			this.txtProcessingMsg.WordWrap = false;
			// 
			// FmB2BUI
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(494, 500);
			this.Controls.Add(this.txtProcessingMsg);
			this.Controls.Add(this.btnCancelSales);
			this.Controls.Add(this.txtErrorMsg);
			this.Controls.Add(this.txtCustomerMsg);
			this.Controls.Add(this.txtAcceptedBanknote);
			this.Controls.Add(this.lblPleasePay);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.lblCountDown);
			this.Controls.Add(this.lblPrice);
			this.Name = "FmB2BUI";
			this.Text = "FmB2B - UI";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FmB2BUI_FormClosing);
			this.Load += new System.EventHandler(this.FmB2BUI_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label lblPrice;
		private System.Windows.Forms.Label lblCountDown;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label lblPleasePay;
		private System.Windows.Forms.TextBox txtAcceptedBanknote;
		private System.Windows.Forms.TextBox txtCustomerMsg;
		private System.Windows.Forms.TextBox txtErrorMsg;
		private System.Windows.Forms.Button btnCancelSales;
		private System.Windows.Forms.TextBox txtProcessingMsg;
	}
}