namespace WinFormCoinTest
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
            this.btnStart = new System.Windows.Forms.Button();
            this.txtMsg = new System.Windows.Forms.TextBox();
            this.txtCoinAmount = new System.Windows.Forms.TextBox();
            this.btnDispense01 = new System.Windows.Forms.Button();
            this.btnResetCoinMachine = new System.Windows.Forms.Button();
            this.btnSpinCoinBox1 = new System.Windows.Forms.Button();
            this.btnSpinCoinBox2 = new System.Windows.Forms.Button();
            this.btnSpinCoinBox3 = new System.Windows.Forms.Button();
            this.btnGetLastDispenseHistory = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnGetMachineStatus = new System.Windows.Forms.Button();
            this.lblM6 = new System.Windows.Forms.Label();
            this.lblM5 = new System.Windows.Forms.Label();
            this.lblM4 = new System.Windows.Forms.Label();
            this.lblM3 = new System.Windows.Forms.Label();
            this.lblM2 = new System.Windows.Forms.Label();
            this.lblM1 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
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
            this.btnReleaseCOMObj = new System.Windows.Forms.Button();
            this.btnGetLowCoinColumnStatus = new System.Windows.Forms.Button();
            this.btnGetLowBinStatus = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.rbtBuzzerOff = new System.Windows.Forms.RadioButton();
            this.rbtBuzzerOn = new System.Windows.Forms.RadioButton();
            this.btnSetBuzzer = new System.Windows.Forms.Button();
            this.btnRebootMachine = new System.Windows.Forms.Button();
            this.btnResetMachine = new System.Windows.Forms.Button();
            this.btnClearColumnHis = new System.Windows.Forms.Button();
            this.btnClearSensor = new System.Windows.Forms.Button();
            this.btnClearMachine = new System.Windows.Forms.Button();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.chkRebootMachine = new System.Windows.Forms.CheckBox();
            this.chkResetMachine = new System.Windows.Forms.CheckBox();
            this.chkClearColumnHistory = new System.Windows.Forms.CheckBox();
            this.chkClearSensor = new System.Windows.Forms.CheckBox();
            this.chkClearMachine = new System.Windows.Forms.CheckBox();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtNoOfCoin = new System.Windows.Forms.NumericUpDown();
            this.btnBaseDisp50Cent = new System.Windows.Forms.Button();
            this.btnBaseDisp20Cent = new System.Windows.Forms.Button();
            this.btnBaseDisp10Cent = new System.Windows.Forms.Button();
            this.btnGetMachineErrors = new System.Windows.Forms.Button();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.lblME3 = new System.Windows.Forms.Label();
            this.lblME2 = new System.Windows.Forms.Label();
            this.lblME1 = new System.Windows.Forms.Label();
            this.label28 = new System.Windows.Forms.Label();
            this.label29 = new System.Windows.Forms.Label();
            this.label30 = new System.Windows.Forms.Label();
            this.axTQ01001 = new AxTQ0100Lib.AxTQ0100();
            this.btnRetryDispenseOnFail = new System.Windows.Forms.Button();
            this.btnStartTestingLoop = new System.Windows.Forms.Button();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtNoOfCoin)).BeginInit();
            this.groupBox7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.axTQ01001)).BeginInit();
            this.SuspendLayout();
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(24, 12);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(177, 43);
            this.btnStart.TabIndex = 1;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // txtMsg
            // 
            this.txtMsg.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtMsg.Location = new System.Drawing.Point(12, 424);
            this.txtMsg.Multiline = true;
            this.txtMsg.Name = "txtMsg";
            this.txtMsg.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtMsg.Size = new System.Drawing.Size(776, 181);
            this.txtMsg.TabIndex = 2;
            this.txtMsg.WordWrap = false;
            // 
            // txtCoinAmount
            // 
            this.txtCoinAmount.Location = new System.Drawing.Point(207, 41);
            this.txtCoinAmount.Name = "txtCoinAmount";
            this.txtCoinAmount.Size = new System.Drawing.Size(57, 20);
            this.txtCoinAmount.TabIndex = 3;
            this.txtCoinAmount.Text = "0.80";
            // 
            // btnDispense01
            // 
            this.btnDispense01.Location = new System.Drawing.Point(270, 39);
            this.btnDispense01.Name = "btnDispense01";
            this.btnDispense01.Size = new System.Drawing.Size(84, 23);
            this.btnDispense01.TabIndex = 4;
            this.btnDispense01.Text = "Dispense";
            this.btnDispense01.UseVisualStyleBackColor = true;
            this.btnDispense01.Click += new System.EventHandler(this.btnDispense01_Click);
            // 
            // btnResetCoinMachine
            // 
            this.btnResetCoinMachine.Location = new System.Drawing.Point(5, 73);
            this.btnResetCoinMachine.Name = "btnResetCoinMachine";
            this.btnResetCoinMachine.Size = new System.Drawing.Size(191, 37);
            this.btnResetCoinMachine.TabIndex = 5;
            this.btnResetCoinMachine.Text = "All Reset Coid Machine";
            this.btnResetCoinMachine.UseVisualStyleBackColor = true;
            this.btnResetCoinMachine.Click += new System.EventHandler(this.btnResetCoinMachine_Click);
            // 
            // btnSpinCoinBox1
            // 
            this.btnSpinCoinBox1.Location = new System.Drawing.Point(12, 344);
            this.btnSpinCoinBox1.Name = "btnSpinCoinBox1";
            this.btnSpinCoinBox1.Size = new System.Drawing.Size(110, 39);
            this.btnSpinCoinBox1.TabIndex = 6;
            this.btnSpinCoinBox1.Text = "Spin Coin Box 1 - 10 Cent";
            this.btnSpinCoinBox1.UseVisualStyleBackColor = true;
            this.btnSpinCoinBox1.Click += new System.EventHandler(this.btnSpinCoinBox1_Click);
            // 
            // btnSpinCoinBox2
            // 
            this.btnSpinCoinBox2.Location = new System.Drawing.Point(128, 344);
            this.btnSpinCoinBox2.Name = "btnSpinCoinBox2";
            this.btnSpinCoinBox2.Size = new System.Drawing.Size(110, 39);
            this.btnSpinCoinBox2.TabIndex = 7;
            this.btnSpinCoinBox2.Text = "Spin Coin Box 2 - 20 Cent";
            this.btnSpinCoinBox2.UseVisualStyleBackColor = true;
            this.btnSpinCoinBox2.Click += new System.EventHandler(this.btnSpinCoinBox2_Click);
            // 
            // btnSpinCoinBox3
            // 
            this.btnSpinCoinBox3.Location = new System.Drawing.Point(244, 344);
            this.btnSpinCoinBox3.Name = "btnSpinCoinBox3";
            this.btnSpinCoinBox3.Size = new System.Drawing.Size(110, 39);
            this.btnSpinCoinBox3.TabIndex = 8;
            this.btnSpinCoinBox3.Text = "Spin Coin Box 3 - 50 Cent";
            this.btnSpinCoinBox3.UseVisualStyleBackColor = true;
            this.btnSpinCoinBox3.Click += new System.EventHandler(this.btnSpinCoinBox3_Click);
            // 
            // btnGetLastDispenseHistory
            // 
            this.btnGetLastDispenseHistory.Location = new System.Drawing.Point(12, 389);
            this.btnGetLastDispenseHistory.Name = "btnGetLastDispenseHistory";
            this.btnGetLastDispenseHistory.Size = new System.Drawing.Size(342, 23);
            this.btnGetLastDispenseHistory.TabIndex = 9;
            this.btnGetLastDispenseHistory.Text = "Get Last Dispense History";
            this.btnGetLastDispenseHistory.UseVisualStyleBackColor = true;
            this.btnGetLastDispenseHistory.Click += new System.EventHandler(this.btnGetLastDispenseHistory_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnGetMachineStatus);
            this.groupBox2.Controls.Add(this.lblM6);
            this.groupBox2.Controls.Add(this.lblM5);
            this.groupBox2.Controls.Add(this.lblM4);
            this.groupBox2.Controls.Add(this.lblM3);
            this.groupBox2.Controls.Add(this.lblM2);
            this.groupBox2.Controls.Add(this.lblM1);
            this.groupBox2.Controls.Add(this.label18);
            this.groupBox2.Controls.Add(this.label17);
            this.groupBox2.Controls.Add(this.label16);
            this.groupBox2.Controls.Add(this.label15);
            this.groupBox2.Controls.Add(this.label14);
            this.groupBox2.Controls.Add(this.label13);
            this.groupBox2.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox2.Location = new System.Drawing.Point(410, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(242, 160);
            this.groupBox2.TabIndex = 39;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Machine Status";
            // 
            // btnGetMachineStatus
            // 
            this.btnGetMachineStatus.Location = new System.Drawing.Point(156, 19);
            this.btnGetMachineStatus.Name = "btnGetMachineStatus";
            this.btnGetMachineStatus.Size = new System.Drawing.Size(80, 60);
            this.btnGetMachineStatus.TabIndex = 27;
            this.btnGetMachineStatus.Text = "Get Machine Status";
            this.btnGetMachineStatus.UseVisualStyleBackColor = true;
            this.btnGetMachineStatus.Click += new System.EventHandler(this.btnGetMachineStatus_Click);
            // 
            // lblM6
            // 
            this.lblM6.AutoSize = true;
            this.lblM6.BackColor = System.Drawing.Color.White;
            this.lblM6.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblM6.Location = new System.Drawing.Point(115, 131);
            this.lblM6.Name = "lblM6";
            this.lblM6.Size = new System.Drawing.Size(16, 14);
            this.lblM6.TabIndex = 24;
            this.lblM6.Text = "   ";
            // 
            // lblM5
            // 
            this.lblM5.AutoSize = true;
            this.lblM5.BackColor = System.Drawing.Color.White;
            this.lblM5.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblM5.Location = new System.Drawing.Point(115, 109);
            this.lblM5.Name = "lblM5";
            this.lblM5.Size = new System.Drawing.Size(16, 14);
            this.lblM5.TabIndex = 24;
            this.lblM5.Text = "   ";
            // 
            // lblM4
            // 
            this.lblM4.AutoSize = true;
            this.lblM4.BackColor = System.Drawing.Color.White;
            this.lblM4.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblM4.Location = new System.Drawing.Point(115, 87);
            this.lblM4.Name = "lblM4";
            this.lblM4.Size = new System.Drawing.Size(16, 14);
            this.lblM4.TabIndex = 24;
            this.lblM4.Text = "   ";
            // 
            // lblM3
            // 
            this.lblM3.AutoSize = true;
            this.lblM3.BackColor = System.Drawing.Color.White;
            this.lblM3.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblM3.Location = new System.Drawing.Point(115, 65);
            this.lblM3.Name = "lblM3";
            this.lblM3.Size = new System.Drawing.Size(16, 14);
            this.lblM3.TabIndex = 24;
            this.lblM3.Text = "   ";
            // 
            // lblM2
            // 
            this.lblM2.AutoSize = true;
            this.lblM2.BackColor = System.Drawing.Color.White;
            this.lblM2.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblM2.Location = new System.Drawing.Point(115, 43);
            this.lblM2.Name = "lblM2";
            this.lblM2.Size = new System.Drawing.Size(16, 14);
            this.lblM2.TabIndex = 26;
            this.lblM2.Text = "   ";
            // 
            // lblM1
            // 
            this.lblM1.AutoSize = true;
            this.lblM1.BackColor = System.Drawing.Color.White;
            this.lblM1.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblM1.Location = new System.Drawing.Point(115, 21);
            this.lblM1.Name = "lblM1";
            this.lblM1.Size = new System.Drawing.Size(16, 14);
            this.lblM1.TabIndex = 23;
            this.lblM1.Text = "   ";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label18.Location = new System.Drawing.Point(6, 132);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(61, 14);
            this.label18.TabIndex = 25;
            this.label18.Text = "Parity Error";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label17.Location = new System.Drawing.Point(6, 110);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(75, 14);
            this.label17.TabIndex = 25;
            this.label17.Text = "Function Error";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label16.Location = new System.Drawing.Point(6, 88);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(74, 14);
            this.label16.TabIndex = 25;
            this.label16.Text = "Machine Error";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label15.Location = new System.Drawing.Point(6, 65);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(29, 14);
            this.label15.TabIndex = 25;
            this.label15.Text = "Free";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label14.Location = new System.Drawing.Point(6, 43);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(83, 14);
            this.label14.TabIndex = 24;
            this.label14.Text = "Low Coin Alarm";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.Location = new System.Drawing.Point(6, 21);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(103, 14);
            this.label13.TabIndex = 23;
            this.label13.Text = "Dispense Attempted";
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
            this.groupBox1.Location = new System.Drawing.Point(12, 143);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(342, 73);
            this.groupBox1.TabIndex = 41;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Price Dispense";
            // 
            // btnDis90Cents
            // 
            this.btnDis90Cents.Location = new System.Drawing.Point(195, 44);
            this.btnDis90Cents.Name = "btnDis90Cents";
            this.btnDis90Cents.Size = new System.Drawing.Size(57, 23);
            this.btnDis90Cents.TabIndex = 50;
            this.btnDis90Cents.Tag = "0.90";
            this.btnDis90Cents.Text = "90 Cent";
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
            this.btnDis80Cents.Text = "80 Cent";
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
            this.btnDis70Cents.Text = "70 Cent";
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
            this.btnDis60Cents.Text = "60 Cent";
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
            this.btnDis50Cents.Text = "50 Cent";
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
            this.btnDis40Cents.Text = "40 Cent";
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
            this.btnDis30Cents.Text = "30 Cent";
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
            this.btnDis20Cents.Text = "20 Cent";
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
            this.btnDis10Cents.Text = "10 Cent";
            this.btnDis10Cents.UseVisualStyleBackColor = true;
            this.btnDis10Cents.Click += new System.EventHandler(this.OnDispenseCents_Click);
            // 
            // btnReleaseCOMObj
            // 
            this.btnReleaseCOMObj.Location = new System.Drawing.Point(658, 128);
            this.btnReleaseCOMObj.Name = "btnReleaseCOMObj";
            this.btnReleaseCOMObj.Size = new System.Drawing.Size(130, 43);
            this.btnReleaseCOMObj.TabIndex = 42;
            this.btnReleaseCOMObj.Text = "Release COM Obj";
            this.btnReleaseCOMObj.UseVisualStyleBackColor = true;
            this.btnReleaseCOMObj.Click += new System.EventHandler(this.btnReleaseCOMObj_Click);
            // 
            // btnGetLowCoinColumnStatus
            // 
            this.btnGetLowCoinColumnStatus.Enabled = false;
            this.btnGetLowCoinColumnStatus.Location = new System.Drawing.Point(771, 183);
            this.btnGetLowCoinColumnStatus.Name = "btnGetLowCoinColumnStatus";
            this.btnGetLowCoinColumnStatus.Size = new System.Drawing.Size(17, 20);
            this.btnGetLowCoinColumnStatus.TabIndex = 43;
            this.btnGetLowCoinColumnStatus.Text = "Get Low Coin Column Status";
            this.btnGetLowCoinColumnStatus.UseVisualStyleBackColor = true;
            this.btnGetLowCoinColumnStatus.Click += new System.EventHandler(this.btnGetLowCoinColumnStatus_Click);
            // 
            // btnGetLowBinStatus
            // 
            this.btnGetLowBinStatus.Location = new System.Drawing.Point(194, 11);
            this.btnGetLowBinStatus.Name = "btnGetLowBinStatus";
            this.btnGetLowBinStatus.Size = new System.Drawing.Size(155, 91);
            this.btnGetLowBinStatus.TabIndex = 44;
            this.btnGetLowBinStatus.Text = "Get Low Bin Status";
            this.btnGetLowBinStatus.UseVisualStyleBackColor = true;
            this.btnGetLowBinStatus.Click += new System.EventHandler(this.btnGetLowBinStatus_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.groupBox4);
            this.groupBox3.Controls.Add(this.btnRebootMachine);
            this.groupBox3.Controls.Add(this.btnResetMachine);
            this.groupBox3.Controls.Add(this.btnClearColumnHis);
            this.groupBox3.Controls.Add(this.btnClearSensor);
            this.groupBox3.Controls.Add(this.btnClearMachine);
            this.groupBox3.Controls.Add(this.btnResetCoinMachine);
            this.groupBox3.Location = new System.Drawing.Point(12, 222);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(342, 116);
            this.groupBox3.TabIndex = 45;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Reset Machine";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.rbtBuzzerOff);
            this.groupBox4.Controls.Add(this.rbtBuzzerOn);
            this.groupBox4.Controls.Add(this.btnSetBuzzer);
            this.groupBox4.Location = new System.Drawing.Point(202, 41);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(134, 70);
            this.groupBox4.TabIndex = 11;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Sound";
            // 
            // rbtBuzzerOff
            // 
            this.rbtBuzzerOff.AutoSize = true;
            this.rbtBuzzerOff.Checked = true;
            this.rbtBuzzerOff.Location = new System.Drawing.Point(74, 37);
            this.rbtBuzzerOff.Name = "rbtBuzzerOff";
            this.rbtBuzzerOff.Size = new System.Drawing.Size(39, 17);
            this.rbtBuzzerOff.TabIndex = 13;
            this.rbtBuzzerOff.TabStop = true;
            this.rbtBuzzerOff.Text = "Off";
            this.rbtBuzzerOff.UseVisualStyleBackColor = true;
            // 
            // rbtBuzzerOn
            // 
            this.rbtBuzzerOn.AutoSize = true;
            this.rbtBuzzerOn.Location = new System.Drawing.Point(74, 13);
            this.rbtBuzzerOn.Name = "rbtBuzzerOn";
            this.rbtBuzzerOn.Size = new System.Drawing.Size(39, 17);
            this.rbtBuzzerOn.TabIndex = 12;
            this.rbtBuzzerOn.Text = "On";
            this.rbtBuzzerOn.UseVisualStyleBackColor = true;
            // 
            // btnSetBuzzer
            // 
            this.btnSetBuzzer.Location = new System.Drawing.Point(6, 12);
            this.btnSetBuzzer.Name = "btnSetBuzzer";
            this.btnSetBuzzer.Size = new System.Drawing.Size(56, 51);
            this.btnSetBuzzer.TabIndex = 11;
            this.btnSetBuzzer.Text = "Set Buzzer";
            this.btnSetBuzzer.UseVisualStyleBackColor = true;
            this.btnSetBuzzer.Click += new System.EventHandler(this.btnSetBuzzer_Click);
            // 
            // btnRebootMachine
            // 
            this.btnRebootMachine.Location = new System.Drawing.Point(99, 48);
            this.btnRebootMachine.Name = "btnRebootMachine";
            this.btnRebootMachine.Size = new System.Drawing.Size(97, 23);
            this.btnRebootMachine.TabIndex = 10;
            this.btnRebootMachine.Text = "Reboot Machine";
            this.btnRebootMachine.UseVisualStyleBackColor = true;
            this.btnRebootMachine.Click += new System.EventHandler(this.btnRebootMachine_Click);
            // 
            // btnResetMachine
            // 
            this.btnResetMachine.Location = new System.Drawing.Point(6, 48);
            this.btnResetMachine.Name = "btnResetMachine";
            this.btnResetMachine.Size = new System.Drawing.Size(87, 23);
            this.btnResetMachine.TabIndex = 9;
            this.btnResetMachine.Text = "Reset Machine";
            this.btnResetMachine.UseVisualStyleBackColor = true;
            this.btnResetMachine.Click += new System.EventHandler(this.btnResetMachine_Click);
            // 
            // btnClearColumnHis
            // 
            this.btnClearColumnHis.Location = new System.Drawing.Point(180, 19);
            this.btnClearColumnHis.Name = "btnClearColumnHis";
            this.btnClearColumnHis.Size = new System.Drawing.Size(112, 23);
            this.btnClearColumnHis.TabIndex = 8;
            this.btnClearColumnHis.Text = "Clear Column History";
            this.btnClearColumnHis.UseVisualStyleBackColor = true;
            this.btnClearColumnHis.Click += new System.EventHandler(this.btnClearColumnHis_Click);
            // 
            // btnClearSensor
            // 
            this.btnClearSensor.Location = new System.Drawing.Point(99, 19);
            this.btnClearSensor.Name = "btnClearSensor";
            this.btnClearSensor.Size = new System.Drawing.Size(75, 23);
            this.btnClearSensor.TabIndex = 7;
            this.btnClearSensor.Text = "Clear Sensor";
            this.btnClearSensor.UseVisualStyleBackColor = true;
            this.btnClearSensor.Click += new System.EventHandler(this.btnClearSensor_Click);
            // 
            // btnClearMachine
            // 
            this.btnClearMachine.Location = new System.Drawing.Point(6, 19);
            this.btnClearMachine.Name = "btnClearMachine";
            this.btnClearMachine.Size = new System.Drawing.Size(87, 23);
            this.btnClearMachine.TabIndex = 6;
            this.btnClearMachine.Text = "Clear Machine";
            this.btnClearMachine.UseVisualStyleBackColor = true;
            this.btnClearMachine.Click += new System.EventHandler(this.btnClearMachine_Click);
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.chkRebootMachine);
            this.groupBox5.Controls.Add(this.chkResetMachine);
            this.groupBox5.Controls.Add(this.chkClearColumnHistory);
            this.groupBox5.Controls.Add(this.chkClearSensor);
            this.groupBox5.Controls.Add(this.chkClearMachine);
            this.groupBox5.Controls.Add(this.btnGetLowBinStatus);
            this.groupBox5.Location = new System.Drawing.Point(410, 172);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(355, 131);
            this.groupBox5.TabIndex = 46;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Check Bin Status";
            // 
            // chkRebootMachine
            // 
            this.chkRebootMachine.AutoSize = true;
            this.chkRebootMachine.Checked = true;
            this.chkRebootMachine.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkRebootMachine.Location = new System.Drawing.Point(8, 108);
            this.chkRebootMachine.Name = "chkRebootMachine";
            this.chkRebootMachine.Size = new System.Drawing.Size(230, 17);
            this.chkRebootMachine.TabIndex = 49;
            this.chkRebootMachine.Text = "5 - Reboot Machine - Spin Motor if low coin";
            this.chkRebootMachine.UseVisualStyleBackColor = true;
            // 
            // chkResetMachine
            // 
            this.chkResetMachine.AutoSize = true;
            this.chkResetMachine.Location = new System.Drawing.Point(8, 86);
            this.chkResetMachine.Name = "chkResetMachine";
            this.chkResetMachine.Size = new System.Drawing.Size(113, 17);
            this.chkResetMachine.TabIndex = 48;
            this.chkResetMachine.Text = "4 - Reset Machine";
            this.chkResetMachine.UseVisualStyleBackColor = true;
            // 
            // chkClearColumnHistory
            // 
            this.chkClearColumnHistory.AutoSize = true;
            this.chkClearColumnHistory.Location = new System.Drawing.Point(8, 65);
            this.chkClearColumnHistory.Name = "chkClearColumnHistory";
            this.chkClearColumnHistory.Size = new System.Drawing.Size(138, 17);
            this.chkClearColumnHistory.TabIndex = 47;
            this.chkClearColumnHistory.Text = "3 - Clear Column History";
            this.chkClearColumnHistory.UseVisualStyleBackColor = true;
            // 
            // chkClearSensor
            // 
            this.chkClearSensor.AutoSize = true;
            this.chkClearSensor.Location = new System.Drawing.Point(8, 42);
            this.chkClearSensor.Name = "chkClearSensor";
            this.chkClearSensor.Size = new System.Drawing.Size(101, 17);
            this.chkClearSensor.TabIndex = 46;
            this.chkClearSensor.Text = "2 - Clear Sensor";
            this.chkClearSensor.UseVisualStyleBackColor = true;
            // 
            // chkClearMachine
            // 
            this.chkClearMachine.AutoSize = true;
            this.chkClearMachine.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkClearMachine.ForeColor = System.Drawing.Color.Black;
            this.chkClearMachine.Location = new System.Drawing.Point(8, 19);
            this.chkClearMachine.Name = "chkClearMachine";
            this.chkClearMachine.Size = new System.Drawing.Size(109, 17);
            this.chkClearMachine.TabIndex = 45;
            this.chkClearMachine.Text = "1 - Clear Machine";
            this.chkClearMachine.UseVisualStyleBackColor = true;
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.label1);
            this.groupBox6.Controls.Add(this.txtNoOfCoin);
            this.groupBox6.Controls.Add(this.btnBaseDisp50Cent);
            this.groupBox6.Controls.Add(this.btnBaseDisp20Cent);
            this.groupBox6.Controls.Add(this.btnBaseDisp10Cent);
            this.groupBox6.Location = new System.Drawing.Point(12, 62);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(342, 73);
            this.groupBox6.TabIndex = 47;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Basic Dispense";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(17, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 13);
            this.label1.TabIndex = 51;
            this.label1.Text = "Number of Coin : ";
            // 
            // txtNoOfCoin
            // 
            this.txtNoOfCoin.Location = new System.Drawing.Point(112, 11);
            this.txtNoOfCoin.Maximum = new decimal(new int[] {
            9,
            0,
            0,
            0});
            this.txtNoOfCoin.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.txtNoOfCoin.Name = "txtNoOfCoin";
            this.txtNoOfCoin.Size = new System.Drawing.Size(47, 20);
            this.txtNoOfCoin.TabIndex = 50;
            this.txtNoOfCoin.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // btnBaseDisp50Cent
            // 
            this.btnBaseDisp50Cent.Location = new System.Drawing.Point(258, 37);
            this.btnBaseDisp50Cent.Name = "btnBaseDisp50Cent";
            this.btnBaseDisp50Cent.Size = new System.Drawing.Size(57, 23);
            this.btnBaseDisp50Cent.TabIndex = 49;
            this.btnBaseDisp50Cent.Tag = "0.50";
            this.btnBaseDisp50Cent.Text = "50 Cent";
            this.btnBaseDisp50Cent.UseVisualStyleBackColor = true;
            this.btnBaseDisp50Cent.Click += new System.EventHandler(this.OnBasicDispenseCents_Click);
            // 
            // btnBaseDisp20Cent
            // 
            this.btnBaseDisp20Cent.Location = new System.Drawing.Point(132, 37);
            this.btnBaseDisp20Cent.Name = "btnBaseDisp20Cent";
            this.btnBaseDisp20Cent.Size = new System.Drawing.Size(57, 23);
            this.btnBaseDisp20Cent.TabIndex = 48;
            this.btnBaseDisp20Cent.Tag = "0.20";
            this.btnBaseDisp20Cent.Text = "20 Cent";
            this.btnBaseDisp20Cent.UseVisualStyleBackColor = true;
            this.btnBaseDisp20Cent.Click += new System.EventHandler(this.OnBasicDispenseCents_Click);
            // 
            // btnBaseDisp10Cent
            // 
            this.btnBaseDisp10Cent.Location = new System.Drawing.Point(17, 37);
            this.btnBaseDisp10Cent.Name = "btnBaseDisp10Cent";
            this.btnBaseDisp10Cent.Size = new System.Drawing.Size(57, 23);
            this.btnBaseDisp10Cent.TabIndex = 47;
            this.btnBaseDisp10Cent.Tag = "0.10";
            this.btnBaseDisp10Cent.Text = "10 Cent";
            this.btnBaseDisp10Cent.UseVisualStyleBackColor = true;
            this.btnBaseDisp10Cent.Click += new System.EventHandler(this.OnBasicDispenseCents_Click);
            // 
            // btnGetMachineErrors
            // 
            this.btnGetMachineErrors.Location = new System.Drawing.Point(248, 12);
            this.btnGetMachineErrors.Name = "btnGetMachineErrors";
            this.btnGetMachineErrors.Size = new System.Drawing.Size(101, 62);
            this.btnGetMachineErrors.TabIndex = 48;
            this.btnGetMachineErrors.Text = "Get Machine Errors";
            this.btnGetMachineErrors.UseVisualStyleBackColor = true;
            this.btnGetMachineErrors.Click += new System.EventHandler(this.btnGetMachineErrors_Click);
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this.lblME3);
            this.groupBox7.Controls.Add(this.lblME2);
            this.groupBox7.Controls.Add(this.lblME1);
            this.groupBox7.Controls.Add(this.label28);
            this.groupBox7.Controls.Add(this.label29);
            this.groupBox7.Controls.Add(this.label30);
            this.groupBox7.Controls.Add(this.btnGetMachineErrors);
            this.groupBox7.Location = new System.Drawing.Point(410, 309);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(355, 80);
            this.groupBox7.TabIndex = 49;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "Machine Error";
            // 
            // lblME3
            // 
            this.lblME3.AutoSize = true;
            this.lblME3.BackColor = System.Drawing.Color.White;
            this.lblME3.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblME3.Location = new System.Drawing.Point(114, 60);
            this.lblME3.Name = "lblME3";
            this.lblME3.Size = new System.Drawing.Size(16, 14);
            this.lblME3.TabIndex = 51;
            this.lblME3.Text = "   ";
            // 
            // lblME2
            // 
            this.lblME2.AutoSize = true;
            this.lblME2.BackColor = System.Drawing.Color.White;
            this.lblME2.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblME2.Location = new System.Drawing.Point(114, 38);
            this.lblME2.Name = "lblME2";
            this.lblME2.Size = new System.Drawing.Size(16, 14);
            this.lblME2.TabIndex = 54;
            this.lblME2.Text = "   ";
            // 
            // lblME1
            // 
            this.lblME1.AutoSize = true;
            this.lblME1.BackColor = System.Drawing.Color.White;
            this.lblME1.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblME1.Location = new System.Drawing.Point(114, 16);
            this.lblME1.Name = "lblME1";
            this.lblME1.Size = new System.Drawing.Size(16, 14);
            this.lblME1.TabIndex = 49;
            this.lblME1.Text = "   ";
            // 
            // label28
            // 
            this.label28.AutoSize = true;
            this.label28.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label28.Location = new System.Drawing.Point(5, 60);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(86, 14);
            this.label28.TabIndex = 53;
            this.label28.Text = "Config Mismatch";
            // 
            // label29
            // 
            this.label29.AutoSize = true;
            this.label29.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label29.Location = new System.Drawing.Point(5, 38);
            this.label29.Name = "label29";
            this.label29.Size = new System.Drawing.Size(97, 14);
            this.label29.TabIndex = 52;
            this.label29.Text = "Coin Sensor Failed";
            // 
            // label30
            // 
            this.label30.AutoSize = true;
            this.label30.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label30.Location = new System.Drawing.Point(5, 16);
            this.label30.Name = "label30";
            this.label30.Size = new System.Drawing.Size(88, 14);
            this.label30.TabIndex = 50;
            this.label30.Text = "Coin Eject Timout";
            // 
            // axTQ01001
            // 
            this.axTQ01001.Enabled = true;
            this.axTQ01001.Location = new System.Drawing.Point(688, 12);
            this.axTQ01001.Name = "axTQ01001";
            this.axTQ01001.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axTQ01001.OcxState")));
            this.axTQ01001.Size = new System.Drawing.Size(100, 50);
            this.axTQ01001.TabIndex = 0;
            // 
            // btnRetryDispenseOnFail
            // 
            this.btnRetryDispenseOnFail.Location = new System.Drawing.Point(587, 395);
            this.btnRetryDispenseOnFail.Name = "btnRetryDispenseOnFail";
            this.btnRetryDispenseOnFail.Size = new System.Drawing.Size(165, 23);
            this.btnRetryDispenseOnFail.TabIndex = 50;
            this.btnRetryDispenseOnFail.Text = "Retry Dispense On Fail";
            this.btnRetryDispenseOnFail.UseVisualStyleBackColor = true;
            this.btnRetryDispenseOnFail.Click += new System.EventHandler(this.btnRetryDispenseOnFail_Click);
            // 
            // btnStartTestingLoop
            // 
            this.btnStartTestingLoop.Location = new System.Drawing.Point(410, 395);
            this.btnStartTestingLoop.Name = "btnStartTestingLoop";
            this.btnStartTestingLoop.Size = new System.Drawing.Size(165, 23);
            this.btnStartTestingLoop.TabIndex = 51;
            this.btnStartTestingLoop.Text = "Start Testing Loop";
            this.btnStartTestingLoop.UseVisualStyleBackColor = true;
            this.btnStartTestingLoop.Click += new System.EventHandler(this.btnStartTestingLoop_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 617);
            this.Controls.Add(this.btnStartTestingLoop);
            this.Controls.Add(this.btnRetryDispenseOnFail);
            this.Controls.Add(this.groupBox7);
            this.Controls.Add(this.groupBox6);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.btnGetLowCoinColumnStatus);
            this.Controls.Add(this.btnReleaseCOMObj);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.btnGetLastDispenseHistory);
            this.Controls.Add(this.btnSpinCoinBox3);
            this.Controls.Add(this.btnSpinCoinBox2);
            this.Controls.Add(this.btnSpinCoinBox1);
            this.Controls.Add(this.btnDispense01);
            this.Controls.Add(this.txtCoinAmount);
            this.Controls.Add(this.txtMsg);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.axTQ01001);
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtNoOfCoin)).EndInit();
            this.groupBox7.ResumeLayout(false);
            this.groupBox7.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.axTQ01001)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private AxTQ0100Lib.AxTQ0100 axTQ01001;
		private System.Windows.Forms.Button btnStart;
		private System.Windows.Forms.TextBox txtMsg;
		private System.Windows.Forms.TextBox txtCoinAmount;
		private System.Windows.Forms.Button btnDispense01;
		private System.Windows.Forms.Button btnResetCoinMachine;
		private System.Windows.Forms.Button btnSpinCoinBox1;
		private System.Windows.Forms.Button btnSpinCoinBox2;
		private System.Windows.Forms.Button btnSpinCoinBox3;
		private System.Windows.Forms.Button btnGetLastDispenseHistory;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Label lblM6;
		private System.Windows.Forms.Label lblM5;
		private System.Windows.Forms.Label lblM4;
		private System.Windows.Forms.Label lblM3;
		private System.Windows.Forms.Label lblM2;
		private System.Windows.Forms.Label lblM1;
		private System.Windows.Forms.Label label18;
		private System.Windows.Forms.Label label17;
		private System.Windows.Forms.Label label16;
		private System.Windows.Forms.Label label15;
		private System.Windows.Forms.Label label14;
		private System.Windows.Forms.Label label13;
		private System.Windows.Forms.Button btnGetMachineStatus;
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
		private System.Windows.Forms.Button btnReleaseCOMObj;
		private System.Windows.Forms.Button btnGetLowCoinColumnStatus;
		private System.Windows.Forms.Button btnGetLowBinStatus;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button btnClearColumnHis;
        private System.Windows.Forms.Button btnClearSensor;
        private System.Windows.Forms.Button btnClearMachine;
        private System.Windows.Forms.Button btnResetMachine;
        private System.Windows.Forms.Button btnRebootMachine;
        private System.Windows.Forms.Button btnSetBuzzer;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.RadioButton rbtBuzzerOff;
        private System.Windows.Forms.RadioButton rbtBuzzerOn;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.CheckBox chkRebootMachine;
        private System.Windows.Forms.CheckBox chkResetMachine;
        private System.Windows.Forms.CheckBox chkClearColumnHistory;
        private System.Windows.Forms.CheckBox chkClearSensor;
        private System.Windows.Forms.CheckBox chkClearMachine;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.Button btnBaseDisp50Cent;
        private System.Windows.Forms.Button btnBaseDisp20Cent;
        private System.Windows.Forms.Button btnBaseDisp10Cent;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown txtNoOfCoin;
        private System.Windows.Forms.Button btnGetMachineErrors;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.Label lblME3;
        private System.Windows.Forms.Label lblME2;
        private System.Windows.Forms.Label lblME1;
        private System.Windows.Forms.Label label28;
        private System.Windows.Forms.Label label29;
        private System.Windows.Forms.Label label30;
        private System.Windows.Forms.Button btnRetryDispenseOnFail;
        private System.Windows.Forms.Button btnStartTestingLoop;
    }
}

