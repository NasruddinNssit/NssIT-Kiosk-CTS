namespace serialCommunicationECR2
{
    partial class frmMain
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
            this.btnReadCard = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.cboComPort = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnEcho = new System.Windows.Forms.Button();
            this.grpNormal = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.btnSalesKtmbTest = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.btnSettlement = new System.Windows.Forms.Button();
            this.btnVoid = new System.Windows.Forms.Button();
            this.btnSale = new System.Windows.Forms.Button();
            this.txtExit = new System.Windows.Forms.Button();
            this.btnQuery = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.grpNormal.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnReadCard
            // 
            this.btnReadCard.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnReadCard.ForeColor = System.Drawing.Color.Maroon;
            this.btnReadCard.Location = new System.Drawing.Point(214, 97);
            this.btnReadCard.Name = "btnReadCard";
            this.btnReadCard.Size = new System.Drawing.Size(114, 23);
            this.btnReadCard.TabIndex = 38;
            this.btnReadCard.Text = "Read Card";
            this.btnReadCard.UseVisualStyleBackColor = true;
            this.btnReadCard.Click += new System.EventHandler(this.btnReadCard_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(288, 12);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(105, 13);
            this.label2.TabIndex = 37;
            this.label2.Text = "Select COM Port:";
            // 
            // cboComPort
            // 
            this.cboComPort.FormattingEnabled = true;
            this.cboComPort.Location = new System.Drawing.Point(397, 9);
            this.cboComPort.Name = "cboComPort";
            this.cboComPort.Size = new System.Drawing.Size(105, 21);
            this.cboComPort.TabIndex = 36;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Verdana", 14.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.DarkGreen;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(187, 23);
            this.label1.TabIndex = 35;
            this.label1.Text = "ECR SIMULATOR";
            // 
            // btnEcho
            // 
            this.btnEcho.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnEcho.ForeColor = System.Drawing.Color.Maroon;
            this.btnEcho.Location = new System.Drawing.Point(214, 147);
            this.btnEcho.Name = "btnEcho";
            this.btnEcho.Size = new System.Drawing.Size(114, 37);
            this.btnEcho.TabIndex = 34;
            this.btnEcho.Text = "ECHO";
            this.btnEcho.UseVisualStyleBackColor = true;
            this.btnEcho.Click += new System.EventHandler(this.btnEcho_Click);
            // 
            // grpNormal
            // 
            this.grpNormal.Controls.Add(this.label7);
            this.grpNormal.Controls.Add(this.btnQuery);
            this.grpNormal.Controls.Add(this.label5);
            this.grpNormal.Controls.Add(this.btnSalesKtmbTest);
            this.grpNormal.Controls.Add(this.label6);
            this.grpNormal.Controls.Add(this.label4);
            this.grpNormal.Controls.Add(this.label3);
            this.grpNormal.Controls.Add(this.btnSettlement);
            this.grpNormal.Controls.Add(this.btnVoid);
            this.grpNormal.Controls.Add(this.btnSale);
            this.grpNormal.Font = new System.Drawing.Font("Verdana", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grpNormal.ForeColor = System.Drawing.SystemColors.Highlight;
            this.grpNormal.Location = new System.Drawing.Point(5, 81);
            this.grpNormal.Name = "grpNormal";
            this.grpNormal.Size = new System.Drawing.Size(176, 331);
            this.grpNormal.TabIndex = 33;
            this.grpNormal.TabStop = false;
            this.grpNormal.Text = "NORMAL ECR";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Verdana", 16F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.label5.Location = new System.Drawing.Point(6, 73);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(28, 26);
            this.label5.TabIndex = 13;
            this.label5.Text = "*";
            // 
            // btnSalesKtmbTest
            // 
            this.btnSalesKtmbTest.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSalesKtmbTest.ForeColor = System.Drawing.Color.Maroon;
            this.btnSalesKtmbTest.Location = new System.Drawing.Point(35, 73);
            this.btnSalesKtmbTest.Name = "btnSalesKtmbTest";
            this.btnSalesKtmbTest.Size = new System.Drawing.Size(100, 45);
            this.btnSalesKtmbTest.TabIndex = 12;
            this.btnSalesKtmbTest.Text = "SALE (NssIT KTMB Test)";
            this.btnSalesKtmbTest.UseVisualStyleBackColor = true;
            this.btnSalesKtmbTest.Click += new System.EventHandler(this.btnSalesKtmbTest_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Verdana", 16F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.label6.Location = new System.Drawing.Point(6, 183);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(28, 26);
            this.label6.TabIndex = 11;
            this.label6.Text = "*";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Verdana", 16F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.label4.Location = new System.Drawing.Point(6, 136);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(28, 26);
            this.label4.TabIndex = 9;
            this.label4.Text = "*";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Verdana", 16F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.label3.Location = new System.Drawing.Point(6, 39);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(28, 26);
            this.label3.TabIndex = 8;
            this.label3.Text = "*";
            // 
            // btnSettlement
            // 
            this.btnSettlement.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSettlement.ForeColor = System.Drawing.Color.Maroon;
            this.btnSettlement.Location = new System.Drawing.Point(35, 183);
            this.btnSettlement.Name = "btnSettlement";
            this.btnSettlement.Size = new System.Drawing.Size(100, 23);
            this.btnSettlement.TabIndex = 6;
            this.btnSettlement.Text = "SETTLEMENT";
            this.btnSettlement.UseVisualStyleBackColor = true;
            this.btnSettlement.Click += new System.EventHandler(this.btnSettlement_Click);
            // 
            // btnVoid
            // 
            this.btnVoid.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnVoid.ForeColor = System.Drawing.Color.Maroon;
            this.btnVoid.Location = new System.Drawing.Point(35, 136);
            this.btnVoid.Name = "btnVoid";
            this.btnVoid.Size = new System.Drawing.Size(100, 23);
            this.btnVoid.TabIndex = 3;
            this.btnVoid.Text = "VOID";
            this.btnVoid.UseVisualStyleBackColor = true;
            this.btnVoid.Click += new System.EventHandler(this.btnVoid_Click);
            // 
            // btnSale
            // 
            this.btnSale.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSale.ForeColor = System.Drawing.Color.Maroon;
            this.btnSale.Location = new System.Drawing.Point(35, 39);
            this.btnSale.Name = "btnSale";
            this.btnSale.Size = new System.Drawing.Size(100, 23);
            this.btnSale.TabIndex = 2;
            this.btnSale.Text = "SALE";
            this.btnSale.UseVisualStyleBackColor = true;
            this.btnSale.Click += new System.EventHandler(this.btnSale_Click);
            // 
            // txtExit
            // 
            this.txtExit.Font = new System.Drawing.Font("Verdana", 11.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtExit.ForeColor = System.Drawing.Color.Maroon;
            this.txtExit.Location = new System.Drawing.Point(364, 217);
            this.txtExit.Name = "txtExit";
            this.txtExit.Size = new System.Drawing.Size(138, 50);
            this.txtExit.TabIndex = 32;
            this.txtExit.Text = "EXIT";
            this.txtExit.UseVisualStyleBackColor = true;
            this.txtExit.Click += new System.EventHandler(this.txtExit_Click);
            // 
            // btnQuery
            // 
            this.btnQuery.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnQuery.ForeColor = System.Drawing.Color.Maroon;
            this.btnQuery.Location = new System.Drawing.Point(35, 227);
            this.btnQuery.Name = "btnQuery";
            this.btnQuery.Size = new System.Drawing.Size(100, 23);
            this.btnQuery.TabIndex = 14;
            this.btnQuery.Text = "Query";
            this.btnQuery.UseVisualStyleBackColor = true;
            this.btnQuery.Click += new System.EventHandler(this.btnQuery_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Verdana", 16F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.label7.Location = new System.Drawing.Point(6, 227);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(28, 26);
            this.label7.TabIndex = 15;
            this.label7.Text = "*";
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(513, 457);
            this.Controls.Add(this.btnReadCard);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cboComPort);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnEcho);
            this.Controls.Add(this.grpNormal);
            this.Controls.Add(this.txtExit);
            this.Name = "frmMain";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.grpNormal.ResumeLayout(false);
            this.grpNormal.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnReadCard;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cboComPort;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnEcho;
        private System.Windows.Forms.GroupBox grpNormal;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnSettlement;
        private System.Windows.Forms.Button btnVoid;
        private System.Windows.Forms.Button btnSale;
        private System.Windows.Forms.Button txtExit;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnSalesKtmbTest;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button btnQuery;
    }
}

