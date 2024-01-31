
namespace WinFormAccessSDKCoinTest
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
            this.btnStopTesting = new System.Windows.Forms.Button();
            this.btnStartTestingLoop = new System.Windows.Forms.Button();
            this.btnEndCoinMach = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radioButton9 = new System.Windows.Forms.RadioButton();
            this.radioButton8 = new System.Windows.Forms.RadioButton();
            this.radioButton7 = new System.Windows.Forms.RadioButton();
            this.radioButton6 = new System.Windows.Forms.RadioButton();
            this.radioButton5 = new System.Windows.Forms.RadioButton();
            this.radioButton4 = new System.Windows.Forms.RadioButton();
            this.radioButton3 = new System.Windows.Forms.RadioButton();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.btnDis90Cents = new System.Windows.Forms.Button();
            this.btnDis80Cents = new System.Windows.Forms.Button();
            this.btnDis70Cents = new System.Windows.Forms.Button();
            this.btnDis60Cents = new System.Windows.Forms.Button();
            this.btnDis50Cents = new System.Windows.Forms.Button();
            this.btnDis40Cents = new System.Windows.Forms.Button();
            this.btnDis30Cents = new System.Windows.Forms.Button();
            this.btnDis20Cents = new System.Windows.Forms.Button();
            this.btnDis10Cents = new System.Windows.Forms.Button();
            this.btnStart = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtMsg
            // 
            this.txtMsg.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtMsg.Location = new System.Drawing.Point(12, 307);
            this.txtMsg.Multiline = true;
            this.txtMsg.Name = "txtMsg";
            this.txtMsg.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtMsg.Size = new System.Drawing.Size(528, 162);
            this.txtMsg.TabIndex = 60;
            this.txtMsg.WordWrap = false;
            // 
            // btnStopTesting
            // 
            this.btnStopTesting.Location = new System.Drawing.Point(388, 269);
            this.btnStopTesting.Name = "btnStopTesting";
            this.btnStopTesting.Size = new System.Drawing.Size(155, 30);
            this.btnStopTesting.TabIndex = 59;
            this.btnStopTesting.Text = "Stop Testing";
            this.btnStopTesting.UseVisualStyleBackColor = true;
            this.btnStopTesting.Click += new System.EventHandler(this.btnStopTesting_Click);
            // 
            // btnStartTestingLoop
            // 
            this.btnStartTestingLoop.Location = new System.Drawing.Point(12, 269);
            this.btnStartTestingLoop.Name = "btnStartTestingLoop";
            this.btnStartTestingLoop.Size = new System.Drawing.Size(370, 30);
            this.btnStartTestingLoop.TabIndex = 58;
            this.btnStartTestingLoop.Text = "Start Testing Loop";
            this.btnStartTestingLoop.UseVisualStyleBackColor = true;
            this.btnStartTestingLoop.Click += new System.EventHandler(this.btnStartTestingLoop_Click);
            // 
            // btnEndCoinMach
            // 
            this.btnEndCoinMach.Location = new System.Drawing.Point(447, 12);
            this.btnEndCoinMach.Name = "btnEndCoinMach";
            this.btnEndCoinMach.Size = new System.Drawing.Size(96, 43);
            this.btnEndCoinMach.TabIndex = 57;
            this.btnEndCoinMach.Text = "End Coin Machine";
            this.btnEndCoinMach.UseVisualStyleBackColor = true;
            this.btnEndCoinMach.Click += new System.EventHandler(this.btnEndCoinMach_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radioButton9);
            this.groupBox1.Controls.Add(this.radioButton8);
            this.groupBox1.Controls.Add(this.radioButton7);
            this.groupBox1.Controls.Add(this.radioButton6);
            this.groupBox1.Controls.Add(this.radioButton5);
            this.groupBox1.Controls.Add(this.radioButton4);
            this.groupBox1.Controls.Add(this.radioButton3);
            this.groupBox1.Controls.Add(this.radioButton2);
            this.groupBox1.Controls.Add(this.radioButton1);
            this.groupBox1.Controls.Add(this.btnDis90Cents);
            this.groupBox1.Controls.Add(this.btnDis80Cents);
            this.groupBox1.Controls.Add(this.btnDis70Cents);
            this.groupBox1.Controls.Add(this.btnDis60Cents);
            this.groupBox1.Controls.Add(this.btnDis50Cents);
            this.groupBox1.Controls.Add(this.btnDis40Cents);
            this.groupBox1.Controls.Add(this.btnDis30Cents);
            this.groupBox1.Controls.Add(this.btnDis20Cents);
            this.groupBox1.Controls.Add(this.btnDis10Cents);
            this.groupBox1.Location = new System.Drawing.Point(12, 61);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(531, 202);
            this.groupBox1.TabIndex = 56;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Price Dispense";
            // 
            // radioButton9
            // 
            this.radioButton9.AutoSize = true;
            this.radioButton9.Location = new System.Drawing.Point(10, 170);
            this.radioButton9.Name = "radioButton9";
            this.radioButton9.Size = new System.Drawing.Size(104, 17);
            this.radioButton9.TabIndex = 59;
            this.radioButton9.Tag = "0.90";
            this.radioButton9.Text = "90 Cent TestLop";
            this.radioButton9.UseVisualStyleBackColor = true;
            this.radioButton9.Click += new System.EventHandler(this.TestLoopingPrice_CheckedChanged);
            // 
            // radioButton8
            // 
            this.radioButton8.AutoSize = true;
            this.radioButton8.Checked = true;
            this.radioButton8.Location = new System.Drawing.Point(382, 109);
            this.radioButton8.Name = "radioButton8";
            this.radioButton8.Size = new System.Drawing.Size(104, 17);
            this.radioButton8.TabIndex = 58;
            this.radioButton8.TabStop = true;
            this.radioButton8.Tag = "0.80";
            this.radioButton8.Text = "80 Cent TestLop";
            this.radioButton8.UseVisualStyleBackColor = true;
            this.radioButton8.Click += new System.EventHandler(this.TestLoopingPrice_CheckedChanged);
            // 
            // radioButton7
            // 
            this.radioButton7.AutoSize = true;
            this.radioButton7.Location = new System.Drawing.Point(254, 109);
            this.radioButton7.Name = "radioButton7";
            this.radioButton7.Size = new System.Drawing.Size(104, 17);
            this.radioButton7.TabIndex = 57;
            this.radioButton7.Tag = "0.70";
            this.radioButton7.Text = "70 Cent TestLop";
            this.radioButton7.UseVisualStyleBackColor = true;
            this.radioButton7.Click += new System.EventHandler(this.TestLoopingPrice_CheckedChanged);
            // 
            // radioButton6
            // 
            this.radioButton6.AutoSize = true;
            this.radioButton6.Location = new System.Drawing.Point(132, 109);
            this.radioButton6.Name = "radioButton6";
            this.radioButton6.Size = new System.Drawing.Size(104, 17);
            this.radioButton6.TabIndex = 56;
            this.radioButton6.Tag = "0.60";
            this.radioButton6.Text = "60 Cent TestLop";
            this.radioButton6.UseVisualStyleBackColor = true;
            this.radioButton6.Click += new System.EventHandler(this.TestLoopingPrice_CheckedChanged);
            // 
            // radioButton5
            // 
            this.radioButton5.AutoSize = true;
            this.radioButton5.Location = new System.Drawing.Point(10, 109);
            this.radioButton5.Name = "radioButton5";
            this.radioButton5.Size = new System.Drawing.Size(104, 17);
            this.radioButton5.TabIndex = 55;
            this.radioButton5.Tag = "0.50";
            this.radioButton5.Text = "50 Cent TestLop";
            this.radioButton5.UseVisualStyleBackColor = true;
            this.radioButton5.Click += new System.EventHandler(this.TestLoopingPrice_CheckedChanged);
            // 
            // radioButton4
            // 
            this.radioButton4.AutoSize = true;
            this.radioButton4.Location = new System.Drawing.Point(382, 48);
            this.radioButton4.Name = "radioButton4";
            this.radioButton4.Size = new System.Drawing.Size(104, 17);
            this.radioButton4.TabIndex = 54;
            this.radioButton4.Tag = "0.40";
            this.radioButton4.Text = "40 Cent TestLop";
            this.radioButton4.UseVisualStyleBackColor = true;
            this.radioButton4.Click += new System.EventHandler(this.TestLoopingPrice_CheckedChanged);
            // 
            // radioButton3
            // 
            this.radioButton3.AutoSize = true;
            this.radioButton3.Location = new System.Drawing.Point(254, 48);
            this.radioButton3.Name = "radioButton3";
            this.radioButton3.Size = new System.Drawing.Size(104, 17);
            this.radioButton3.TabIndex = 53;
            this.radioButton3.Tag = "0.30";
            this.radioButton3.Text = "30 Cent TestLop";
            this.radioButton3.UseVisualStyleBackColor = true;
            this.radioButton3.Click += new System.EventHandler(this.TestLoopingPrice_CheckedChanged);
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Location = new System.Drawing.Point(132, 48);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(104, 17);
            this.radioButton2.TabIndex = 52;
            this.radioButton2.Tag = "0.20";
            this.radioButton2.Text = "20 Cent TestLop";
            this.radioButton2.UseVisualStyleBackColor = true;
            this.radioButton2.Click += new System.EventHandler(this.TestLoopingPrice_CheckedChanged);
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Location = new System.Drawing.Point(10, 48);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(104, 17);
            this.radioButton1.TabIndex = 51;
            this.radioButton1.Tag = "0.10";
            this.radioButton1.Text = "10 Cent TestLop";
            this.radioButton1.UseVisualStyleBackColor = true;
            this.radioButton1.Click += new System.EventHandler(this.TestLoopingPrice_CheckedChanged);
            // 
            // btnDis90Cents
            // 
            this.btnDis90Cents.Location = new System.Drawing.Point(10, 141);
            this.btnDis90Cents.Name = "btnDis90Cents";
            this.btnDis90Cents.Size = new System.Drawing.Size(116, 23);
            this.btnDis90Cents.TabIndex = 50;
            this.btnDis90Cents.Tag = "0.90";
            this.btnDis90Cents.Text = "90 Cent";
            this.btnDis90Cents.UseVisualStyleBackColor = true;
            this.btnDis90Cents.Click += new System.EventHandler(this.OnDispenseCents_Click);
            // 
            // btnDis80Cents
            // 
            this.btnDis80Cents.Location = new System.Drawing.Point(382, 80);
            this.btnDis80Cents.Name = "btnDis80Cents";
            this.btnDis80Cents.Size = new System.Drawing.Size(116, 23);
            this.btnDis80Cents.TabIndex = 49;
            this.btnDis80Cents.Tag = "0.80";
            this.btnDis80Cents.Text = "80 Cent";
            this.btnDis80Cents.UseVisualStyleBackColor = true;
            this.btnDis80Cents.Click += new System.EventHandler(this.OnDispenseCents_Click);
            // 
            // btnDis70Cents
            // 
            this.btnDis70Cents.Location = new System.Drawing.Point(254, 80);
            this.btnDis70Cents.Name = "btnDis70Cents";
            this.btnDis70Cents.Size = new System.Drawing.Size(116, 23);
            this.btnDis70Cents.TabIndex = 48;
            this.btnDis70Cents.Tag = "0.70";
            this.btnDis70Cents.Text = "70 Cent";
            this.btnDis70Cents.UseVisualStyleBackColor = true;
            this.btnDis70Cents.Click += new System.EventHandler(this.OnDispenseCents_Click);
            // 
            // btnDis60Cents
            // 
            this.btnDis60Cents.Location = new System.Drawing.Point(132, 80);
            this.btnDis60Cents.Name = "btnDis60Cents";
            this.btnDis60Cents.Size = new System.Drawing.Size(116, 23);
            this.btnDis60Cents.TabIndex = 47;
            this.btnDis60Cents.Tag = "0.60";
            this.btnDis60Cents.Text = "60 Cent";
            this.btnDis60Cents.UseVisualStyleBackColor = true;
            this.btnDis60Cents.Click += new System.EventHandler(this.OnDispenseCents_Click);
            // 
            // btnDis50Cents
            // 
            this.btnDis50Cents.Location = new System.Drawing.Point(10, 80);
            this.btnDis50Cents.Name = "btnDis50Cents";
            this.btnDis50Cents.Size = new System.Drawing.Size(116, 23);
            this.btnDis50Cents.TabIndex = 46;
            this.btnDis50Cents.Tag = "0.50";
            this.btnDis50Cents.Text = "50 Cent";
            this.btnDis50Cents.UseVisualStyleBackColor = true;
            this.btnDis50Cents.Click += new System.EventHandler(this.OnDispenseCents_Click);
            // 
            // btnDis40Cents
            // 
            this.btnDis40Cents.Location = new System.Drawing.Point(382, 19);
            this.btnDis40Cents.Name = "btnDis40Cents";
            this.btnDis40Cents.Size = new System.Drawing.Size(116, 23);
            this.btnDis40Cents.TabIndex = 45;
            this.btnDis40Cents.Tag = "0.40";
            this.btnDis40Cents.Text = "40 Cent";
            this.btnDis40Cents.UseVisualStyleBackColor = true;
            this.btnDis40Cents.Click += new System.EventHandler(this.OnDispenseCents_Click);
            // 
            // btnDis30Cents
            // 
            this.btnDis30Cents.Location = new System.Drawing.Point(254, 19);
            this.btnDis30Cents.Name = "btnDis30Cents";
            this.btnDis30Cents.Size = new System.Drawing.Size(116, 23);
            this.btnDis30Cents.TabIndex = 44;
            this.btnDis30Cents.Tag = "0.30";
            this.btnDis30Cents.Text = "30 Cent";
            this.btnDis30Cents.UseVisualStyleBackColor = true;
            this.btnDis30Cents.Click += new System.EventHandler(this.OnDispenseCents_Click);
            // 
            // btnDis20Cents
            // 
            this.btnDis20Cents.Location = new System.Drawing.Point(132, 19);
            this.btnDis20Cents.Name = "btnDis20Cents";
            this.btnDis20Cents.Size = new System.Drawing.Size(116, 23);
            this.btnDis20Cents.TabIndex = 43;
            this.btnDis20Cents.Tag = "0.20";
            this.btnDis20Cents.Text = "20 Cent";
            this.btnDis20Cents.UseVisualStyleBackColor = true;
            this.btnDis20Cents.Click += new System.EventHandler(this.OnDispenseCents_Click);
            // 
            // btnDis10Cents
            // 
            this.btnDis10Cents.Location = new System.Drawing.Point(10, 19);
            this.btnDis10Cents.Name = "btnDis10Cents";
            this.btnDis10Cents.Size = new System.Drawing.Size(116, 23);
            this.btnDis10Cents.TabIndex = 42;
            this.btnDis10Cents.Tag = "";
            this.btnDis10Cents.Text = "10 Cent";
            this.btnDis10Cents.UseVisualStyleBackColor = true;
            this.btnDis10Cents.Click += new System.EventHandler(this.OnDispenseCents_Click);
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(12, 12);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(87, 43);
            this.btnStart.TabIndex = 55;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(552, 481);
            this.Controls.Add(this.txtMsg);
            this.Controls.Add(this.btnStopTesting);
            this.Controls.Add(this.btnStartTestingLoop);
            this.Controls.Add(this.btnEndCoinMach);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnStart);
            this.Name = "Form1";
            this.Text = "Access SDK Coin Machine Test";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtMsg;
        private System.Windows.Forms.Button btnStopTesting;
        private System.Windows.Forms.Button btnStartTestingLoop;
        private System.Windows.Forms.Button btnEndCoinMach;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radioButton9;
        private System.Windows.Forms.RadioButton radioButton8;
        private System.Windows.Forms.RadioButton radioButton7;
        private System.Windows.Forms.RadioButton radioButton6;
        private System.Windows.Forms.RadioButton radioButton5;
        private System.Windows.Forms.RadioButton radioButton4;
        private System.Windows.Forms.RadioButton radioButton3;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.Button btnDis90Cents;
        private System.Windows.Forms.Button btnDis80Cents;
        private System.Windows.Forms.Button btnDis70Cents;
        private System.Windows.Forms.Button btnDis60Cents;
        private System.Windows.Forms.Button btnDis50Cents;
        private System.Windows.Forms.Button btnDis40Cents;
        private System.Windows.Forms.Button btnDis30Cents;
        private System.Windows.Forms.Button btnDis20Cents;
        private System.Windows.Forms.Button btnDis10Cents;
        private System.Windows.Forms.Button btnStart;
    }
}

