namespace NssIT.Kiosk.Device.Telequip.CX2_3.OrgApi
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
			this.axTQ01001 = new AxTQ0100Lib.AxTQ0100();
			this.btnReleaseCOMObj = new System.Windows.Forms.Button();
			this.btnSpinCoinBox3 = new System.Windows.Forms.Button();
			this.btnSpinCoinBox2 = new System.Windows.Forms.Button();
			this.btnSpinCoinBox1 = new System.Windows.Forms.Button();
			this.btnResetCoinMachine = new System.Windows.Forms.Button();
			this.btnDispense01 = new System.Windows.Forms.Button();
			this.txtCoinAmount = new System.Windows.Forms.TextBox();
			this.txtMsg = new System.Windows.Forms.TextBox();
			this.btnStart = new System.Windows.Forms.Button();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.btnDis90Cents = new System.Windows.Forms.Button();
			this.btnDis80Cents = new System.Windows.Forms.Button();
			this.btnDis70Cents = new System.Windows.Forms.Button();
			this.btnDis60Cents = new System.Windows.Forms.Button();
			this.btnDis50Cents = new System.Windows.Forms.Button();
			this.btnDis40Cents = new System.Windows.Forms.Button();
			this.btnDis30Cents = new System.Windows.Forms.Button();
			this.btnDis20Cents = new System.Windows.Forms.Button();
			this.btnDis10Cents = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.axTQ01001)).BeginInit();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// axTQ01001
			// 
			this.axTQ01001.Enabled = true;
			this.axTQ01001.Location = new System.Drawing.Point(360, 12);
			this.axTQ01001.Name = "axTQ01001";
			this.axTQ01001.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axTQ01001.OcxState")));
			this.axTQ01001.Size = new System.Drawing.Size(100, 53);
			this.axTQ01001.TabIndex = 0;
			// 
			// btnReleaseCOMObj
			// 
			this.btnReleaseCOMObj.Location = new System.Drawing.Point(360, 72);
			this.btnReleaseCOMObj.Name = "btnReleaseCOMObj";
			this.btnReleaseCOMObj.Size = new System.Drawing.Size(100, 64);
			this.btnReleaseCOMObj.TabIndex = 54;
			this.btnReleaseCOMObj.Text = "Release COM Obj";
			this.btnReleaseCOMObj.UseVisualStyleBackColor = true;
			this.btnReleaseCOMObj.Click += new System.EventHandler(this.btnReleaseCOMObj_Click);
			// 
			// btnSpinCoinBox3
			// 
			this.btnSpinCoinBox3.Location = new System.Drawing.Point(244, 191);
			this.btnSpinCoinBox3.Name = "btnSpinCoinBox3";
			this.btnSpinCoinBox3.Size = new System.Drawing.Size(110, 23);
			this.btnSpinCoinBox3.TabIndex = 50;
			this.btnSpinCoinBox3.Text = "Spin Coin Box 3";
			this.btnSpinCoinBox3.UseVisualStyleBackColor = true;
			this.btnSpinCoinBox3.Click += new System.EventHandler(this.btnSpinCoinBox3_Click);
			// 
			// btnSpinCoinBox2
			// 
			this.btnSpinCoinBox2.Location = new System.Drawing.Point(128, 191);
			this.btnSpinCoinBox2.Name = "btnSpinCoinBox2";
			this.btnSpinCoinBox2.Size = new System.Drawing.Size(110, 23);
			this.btnSpinCoinBox2.TabIndex = 49;
			this.btnSpinCoinBox2.Text = "Spin Coin Box 2";
			this.btnSpinCoinBox2.UseVisualStyleBackColor = true;
			this.btnSpinCoinBox2.Click += new System.EventHandler(this.btnSpinCoinBox2_Click);
			// 
			// btnSpinCoinBox1
			// 
			this.btnSpinCoinBox1.Location = new System.Drawing.Point(12, 191);
			this.btnSpinCoinBox1.Name = "btnSpinCoinBox1";
			this.btnSpinCoinBox1.Size = new System.Drawing.Size(110, 23);
			this.btnSpinCoinBox1.TabIndex = 48;
			this.btnSpinCoinBox1.Text = "Spin Coin Box 1";
			this.btnSpinCoinBox1.UseVisualStyleBackColor = true;
			this.btnSpinCoinBox1.Click += new System.EventHandler(this.btnSpinCoinBox1_Click);
			// 
			// btnResetCoinMachine
			// 
			this.btnResetCoinMachine.Location = new System.Drawing.Point(12, 162);
			this.btnResetCoinMachine.Name = "btnResetCoinMachine";
			this.btnResetCoinMachine.Size = new System.Drawing.Size(342, 23);
			this.btnResetCoinMachine.TabIndex = 47;
			this.btnResetCoinMachine.Text = "Reset Coid Machine";
			this.btnResetCoinMachine.UseVisualStyleBackColor = true;
			this.btnResetCoinMachine.Click += new System.EventHandler(this.btnResetCoinMachine_Click);
			// 
			// btnDispense01
			// 
			this.btnDispense01.Location = new System.Drawing.Point(270, 55);
			this.btnDispense01.Name = "btnDispense01";
			this.btnDispense01.Size = new System.Drawing.Size(84, 23);
			this.btnDispense01.TabIndex = 46;
			this.btnDispense01.Text = "Dispense";
			this.btnDispense01.UseVisualStyleBackColor = true;
			this.btnDispense01.Click += new System.EventHandler(this.btnDispense01_Click);
			// 
			// txtCoinAmount
			// 
			this.txtCoinAmount.Location = new System.Drawing.Point(207, 57);
			this.txtCoinAmount.Name = "txtCoinAmount";
			this.txtCoinAmount.Size = new System.Drawing.Size(57, 20);
			this.txtCoinAmount.TabIndex = 45;
			this.txtCoinAmount.Text = "0.80";
			// 
			// txtMsg
			// 
			this.txtMsg.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtMsg.Location = new System.Drawing.Point(12, 220);
			this.txtMsg.Multiline = true;
			this.txtMsg.Name = "txtMsg";
			this.txtMsg.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtMsg.Size = new System.Drawing.Size(451, 305);
			this.txtMsg.TabIndex = 44;
			this.txtMsg.WordWrap = false;
			// 
			// btnStart
			// 
			this.btnStart.Location = new System.Drawing.Point(12, 12);
			this.btnStart.Name = "btnStart";
			this.btnStart.Size = new System.Drawing.Size(177, 66);
			this.btnStart.TabIndex = 43;
			this.btnStart.Text = "Start";
			this.btnStart.UseVisualStyleBackColor = true;
			this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.btnDis90Cents);
			this.groupBox1.Controls.Add(this.btnDis80Cents);
			this.groupBox1.Controls.Add(this.btnDis70Cents);
			this.groupBox1.Controls.Add(this.btnDis60Cents);
			this.groupBox1.Controls.Add(this.btnDis50Cents);
			this.groupBox1.Controls.Add(this.btnDis40Cents);
			this.groupBox1.Controls.Add(this.btnDis30Cents);
			this.groupBox1.Controls.Add(this.btnDis20Cents);
			this.groupBox1.Controls.Add(this.btnDis10Cents);
			this.groupBox1.Location = new System.Drawing.Point(12, 83);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(342, 73);
			this.groupBox1.TabIndex = 55;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Dispense";
			// 
			// btnDis90Cents
			// 
			this.btnDis90Cents.Location = new System.Drawing.Point(195, 44);
			this.btnDis90Cents.Name = "btnDis90Cents";
			this.btnDis90Cents.Size = new System.Drawing.Size(57, 23);
			this.btnDis90Cents.TabIndex = 50;
			this.btnDis90Cents.Tag = "0.90";
			this.btnDis90Cents.Text = "90 Cents";
			this.btnDis90Cents.UseVisualStyleBackColor = true;
			this.btnDis90Cents.Click += new System.EventHandler(this.OnDispenseCents_Click);
			// 
			// btnDis80Cents
			// 
			this.btnDis80Cents.Location = new System.Drawing.Point(132, 44);
			this.btnDis80Cents.Name = "btnDis80Cents";
			this.btnDis80Cents.Size = new System.Drawing.Size(57, 23);
			this.btnDis80Cents.TabIndex = 49;
			this.btnDis80Cents.Tag = "0.80";
			this.btnDis80Cents.Text = "80 Cents";
			this.btnDis80Cents.UseVisualStyleBackColor = true;
			this.btnDis80Cents.Click += new System.EventHandler(this.OnDispenseCents_Click);
			// 
			// btnDis70Cents
			// 
			this.btnDis70Cents.Location = new System.Drawing.Point(69, 44);
			this.btnDis70Cents.Name = "btnDis70Cents";
			this.btnDis70Cents.Size = new System.Drawing.Size(57, 23);
			this.btnDis70Cents.TabIndex = 48;
			this.btnDis70Cents.Tag = "0.70";
			this.btnDis70Cents.Text = "70 Cents";
			this.btnDis70Cents.UseVisualStyleBackColor = true;
			this.btnDis70Cents.Click += new System.EventHandler(this.OnDispenseCents_Click);
			// 
			// btnDis60Cents
			// 
			this.btnDis60Cents.Location = new System.Drawing.Point(6, 44);
			this.btnDis60Cents.Name = "btnDis60Cents";
			this.btnDis60Cents.Size = new System.Drawing.Size(57, 23);
			this.btnDis60Cents.TabIndex = 47;
			this.btnDis60Cents.Tag = "0.60";
			this.btnDis60Cents.Text = "60 Cents";
			this.btnDis60Cents.UseVisualStyleBackColor = true;
			this.btnDis60Cents.Click += new System.EventHandler(this.OnDispenseCents_Click);
			// 
			// btnDis50Cents
			// 
			this.btnDis50Cents.Location = new System.Drawing.Point(258, 17);
			this.btnDis50Cents.Name = "btnDis50Cents";
			this.btnDis50Cents.Size = new System.Drawing.Size(57, 23);
			this.btnDis50Cents.TabIndex = 46;
			this.btnDis50Cents.Tag = "0.50";
			this.btnDis50Cents.Text = "50 Cents";
			this.btnDis50Cents.UseVisualStyleBackColor = true;
			this.btnDis50Cents.Click += new System.EventHandler(this.OnDispenseCents_Click);
			// 
			// btnDis40Cents
			// 
			this.btnDis40Cents.Location = new System.Drawing.Point(195, 17);
			this.btnDis40Cents.Name = "btnDis40Cents";
			this.btnDis40Cents.Size = new System.Drawing.Size(57, 23);
			this.btnDis40Cents.TabIndex = 45;
			this.btnDis40Cents.Tag = "0.40";
			this.btnDis40Cents.Text = "40 Cents";
			this.btnDis40Cents.UseVisualStyleBackColor = true;
			this.btnDis40Cents.Click += new System.EventHandler(this.OnDispenseCents_Click);
			// 
			// btnDis30Cents
			// 
			this.btnDis30Cents.Location = new System.Drawing.Point(132, 17);
			this.btnDis30Cents.Name = "btnDis30Cents";
			this.btnDis30Cents.Size = new System.Drawing.Size(57, 23);
			this.btnDis30Cents.TabIndex = 44;
			this.btnDis30Cents.Tag = "0.30";
			this.btnDis30Cents.Text = "30 Cents";
			this.btnDis30Cents.UseVisualStyleBackColor = true;
			this.btnDis30Cents.Click += new System.EventHandler(this.OnDispenseCents_Click);
			// 
			// btnDis20Cents
			// 
			this.btnDis20Cents.Location = new System.Drawing.Point(69, 17);
			this.btnDis20Cents.Name = "btnDis20Cents";
			this.btnDis20Cents.Size = new System.Drawing.Size(57, 23);
			this.btnDis20Cents.TabIndex = 43;
			this.btnDis20Cents.Tag = "0.20";
			this.btnDis20Cents.Text = "20 Cents";
			this.btnDis20Cents.UseVisualStyleBackColor = true;
			this.btnDis20Cents.Click += new System.EventHandler(this.OnDispenseCents_Click);
			// 
			// btnDis10Cents
			// 
			this.btnDis10Cents.Location = new System.Drawing.Point(6, 17);
			this.btnDis10Cents.Name = "btnDis10Cents";
			this.btnDis10Cents.Size = new System.Drawing.Size(57, 23);
			this.btnDis10Cents.TabIndex = 42;
			this.btnDis10Cents.Tag = "0.10";
			this.btnDis10Cents.Text = "10 Cents";
			this.btnDis10Cents.UseVisualStyleBackColor = true;
			this.btnDis10Cents.Click += new System.EventHandler(this.OnDispenseCents_Click);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(475, 537);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.btnReleaseCOMObj);
			this.Controls.Add(this.btnSpinCoinBox3);
			this.Controls.Add(this.btnSpinCoinBox2);
			this.Controls.Add(this.btnSpinCoinBox1);
			this.Controls.Add(this.btnResetCoinMachine);
			this.Controls.Add(this.btnDispense01);
			this.Controls.Add(this.txtCoinAmount);
			this.Controls.Add(this.txtMsg);
			this.Controls.Add(this.btnStart);
			this.Controls.Add(this.axTQ01001);
			this.Name = "Form1";
			this.Text = "Form1 - NssIT.Kiosk.Device.Telequip.CX2_3.OrgApi";
			this.Activated += new System.EventHandler(this.Form1_Activated);
			((System.ComponentModel.ISupportInitialize)(this.axTQ01001)).EndInit();
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private AxTQ0100Lib.AxTQ0100 axTQ01001;
		private System.Windows.Forms.Button btnReleaseCOMObj;
		private System.Windows.Forms.Button btnSpinCoinBox3;
		private System.Windows.Forms.Button btnSpinCoinBox2;
		private System.Windows.Forms.Button btnSpinCoinBox1;
		private System.Windows.Forms.Button btnResetCoinMachine;
		private System.Windows.Forms.Button btnDispense01;
		private System.Windows.Forms.TextBox txtCoinAmount;
		private System.Windows.Forms.TextBox txtMsg;
		private System.Windows.Forms.Button btnStart;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button btnDis90Cents;
		private System.Windows.Forms.Button btnDis80Cents;
		private System.Windows.Forms.Button btnDis70Cents;
		private System.Windows.Forms.Button btnDis60Cents;
		private System.Windows.Forms.Button btnDis50Cents;
		private System.Windows.Forms.Button btnDis40Cents;
		private System.Windows.Forms.Button btnDis30Cents;
		private System.Windows.Forms.Button btnDis20Cents;
		private System.Windows.Forms.Button btnDis10Cents;
	}
}