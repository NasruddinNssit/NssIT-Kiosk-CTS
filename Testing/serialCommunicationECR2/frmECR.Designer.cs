namespace serialCommunicationECR2
{
    partial class frmECR
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
            this.btnCancelTrans = new System.Windows.Forms.Button();
            this.lblCheck = new System.Windows.Forms.Label();
            this.btnAbort = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblProgressMsg = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.txtDocs = new System.Windows.Forms.TextBox();
            this.lblDocs = new System.Windows.Forms.Label();
            this.btnClsMsg = new System.Windows.Forms.Button();
            this.txtQRNo = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.lbl_status = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.txtTipAmount = new System.Windows.Forms.TextBox();
            this.txtQRId = new System.Windows.Forms.TextBox();
            this.txtAmount = new System.Windows.Forms.TextBox();
            this.txtHostNo = new System.Windows.Forms.TextBox();
            this.txtSendMsg = new System.Windows.Forms.TextBox();
            this.rtxtReceiveMsg = new System.Windows.Forms.RichTextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lbl_portname = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.btnDisconnect = new System.Windows.Forms.Button();
            this.btnSendData = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCancelTrans
            // 
            this.btnCancelTrans.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancelTrans.ForeColor = System.Drawing.Color.DarkRed;
            this.btnCancelTrans.Location = new System.Drawing.Point(294, 496);
            this.btnCancelTrans.Name = "btnCancelTrans";
            this.btnCancelTrans.Size = new System.Drawing.Size(150, 23);
            this.btnCancelTrans.TabIndex = 86;
            this.btnCancelTrans.Text = "Cancel Transaction";
            this.btnCancelTrans.Click += new System.EventHandler(this.btnCancelTrans_Click);
            // 
            // lblCheck
            // 
            this.lblCheck.AutoSize = true;
            this.lblCheck.Location = new System.Drawing.Point(316, 466);
            this.lblCheck.Name = "lblCheck";
            this.lblCheck.Size = new System.Drawing.Size(13, 13);
            this.lblCheck.TabIndex = 85;
            this.lblCheck.Text = "..";
            // 
            // btnAbort
            // 
            this.btnAbort.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAbort.ForeColor = System.Drawing.Color.Maroon;
            this.btnAbort.Location = new System.Drawing.Point(621, 496);
            this.btnAbort.Name = "btnAbort";
            this.btnAbort.Size = new System.Drawing.Size(119, 23);
            this.btnAbort.TabIndex = 84;
            this.btnAbort.Text = "ABORT";
            this.btnAbort.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lblProgressMsg);
            this.groupBox1.Controls.Add(this.label11);
            this.groupBox1.Controls.Add(this.txtDocs);
            this.groupBox1.Controls.Add(this.lblDocs);
            this.groupBox1.Controls.Add(this.btnClsMsg);
            this.groupBox1.Controls.Add(this.txtQRNo);
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.lbl_status);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.txtTipAmount);
            this.groupBox1.Controls.Add(this.txtQRId);
            this.groupBox1.Controls.Add(this.txtAmount);
            this.groupBox1.Controls.Add(this.txtHostNo);
            this.groupBox1.Controls.Add(this.txtSendMsg);
            this.groupBox1.Controls.Add(this.rtxtReceiveMsg);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.lbl_portname);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.ForeColor = System.Drawing.SystemColors.Highlight;
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(738, 451);
            this.groupBox1.TabIndex = 83;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "INPUT BOX";
            // 
            // lblProgressMsg
            // 
            this.lblProgressMsg.AutoSize = true;
            this.lblProgressMsg.Location = new System.Drawing.Point(175, 421);
            this.lblProgressMsg.Name = "lblProgressMsg";
            this.lblProgressMsg.Size = new System.Drawing.Size(92, 13);
            this.lblProgressMsg.TabIndex = 79;
            this.lblProgressMsg.Text = "lblProgressMsg";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.ForeColor = System.Drawing.Color.DarkRed;
            this.label11.Location = new System.Drawing.Point(38, 421);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(126, 13);
            this.label11.TabIndex = 78;
            this.label11.Text = "Progressing Msg : ";
            // 
            // txtDocs
            // 
            this.txtDocs.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtDocs.Location = new System.Drawing.Point(181, 164);
            this.txtDocs.Name = "txtDocs";
            this.txtDocs.Size = new System.Drawing.Size(508, 21);
            this.txtDocs.TabIndex = 77;
            // 
            // lblDocs
            // 
            this.lblDocs.AutoSize = true;
            this.lblDocs.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDocs.ForeColor = System.Drawing.Color.DarkRed;
            this.lblDocs.Location = new System.Drawing.Point(29, 167);
            this.lblDocs.Name = "lblDocs";
            this.lblDocs.Size = new System.Drawing.Size(146, 13);
            this.lblDocs.TabIndex = 76;
            this.lblDocs.Text = "Document Numbers : ";
            // 
            // btnClsMsg
            // 
            this.btnClsMsg.Location = new System.Drawing.Point(689, 237);
            this.btnClsMsg.Name = "btnClsMsg";
            this.btnClsMsg.Size = new System.Drawing.Size(42, 35);
            this.btnClsMsg.TabIndex = 75;
            this.btnClsMsg.Text = "CLS";
            this.btnClsMsg.UseVisualStyleBackColor = true;
            this.btnClsMsg.Click += new System.EventHandler(this.btnClsMsg_Click);
            // 
            // txtQRNo
            // 
            this.txtQRNo.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtQRNo.Location = new System.Drawing.Point(475, 127);
            this.txtQRNo.Name = "txtQRNo";
            this.txtQRNo.Size = new System.Drawing.Size(194, 21);
            this.txtQRNo.TabIndex = 74;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.ForeColor = System.Drawing.Color.DarkRed;
            this.label10.Location = new System.Drawing.Point(341, 129);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(45, 13);
            this.label10.TabIndex = 73;
            this.label10.Text = "QR No";
            // 
            // lbl_status
            // 
            this.lbl_status.AutoSize = true;
            this.lbl_status.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_status.ForeColor = System.Drawing.Color.DarkRed;
            this.lbl_status.Location = new System.Drawing.Point(172, 388);
            this.lbl_status.Name = "lbl_status";
            this.lbl_status.Size = new System.Drawing.Size(0, 13);
            this.lbl_status.TabIndex = 72;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.ForeColor = System.Drawing.Color.DarkRed;
            this.label8.Location = new System.Drawing.Point(38, 388);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(52, 13);
            this.label8.TabIndex = 71;
            this.label8.Text = "Status:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.ForeColor = System.Drawing.Color.DarkRed;
            this.label7.Location = new System.Drawing.Point(341, 86);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(121, 13);
            this.label7.TabIndex = 16;
            this.label7.Text = "TIP AMOUNT (RM)";
            // 
            // txtTipAmount
            // 
            this.txtTipAmount.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTipAmount.Location = new System.Drawing.Point(475, 84);
            this.txtTipAmount.Name = "txtTipAmount";
            this.txtTipAmount.Size = new System.Drawing.Size(154, 21);
            this.txtTipAmount.TabIndex = 4;
            // 
            // txtQRId
            // 
            this.txtQRId.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtQRId.Location = new System.Drawing.Point(166, 124);
            this.txtQRId.Name = "txtQRId";
            this.txtQRId.Size = new System.Drawing.Size(135, 21);
            this.txtQRId.TabIndex = 3;
            // 
            // txtAmount
            // 
            this.txtAmount.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtAmount.Location = new System.Drawing.Point(166, 86);
            this.txtAmount.Name = "txtAmount";
            this.txtAmount.Size = new System.Drawing.Size(135, 21);
            this.txtAmount.TabIndex = 2;
            // 
            // txtHostNo
            // 
            this.txtHostNo.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtHostNo.Location = new System.Drawing.Point(166, 32);
            this.txtHostNo.Name = "txtHostNo";
            this.txtHostNo.Size = new System.Drawing.Size(112, 21);
            this.txtHostNo.TabIndex = 1;
            // 
            // txtSendMsg
            // 
            this.txtSendMsg.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.txtSendMsg.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSendMsg.ForeColor = System.Drawing.Color.Crimson;
            this.txtSendMsg.Location = new System.Drawing.Point(166, 191);
            this.txtSendMsg.Name = "txtSendMsg";
            this.txtSendMsg.ReadOnly = true;
            this.txtSendMsg.Size = new System.Drawing.Size(523, 22);
            this.txtSendMsg.TabIndex = 6;
            // 
            // rtxtReceiveMsg
            // 
            this.rtxtReceiveMsg.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.rtxtReceiveMsg.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rtxtReceiveMsg.ForeColor = System.Drawing.Color.DarkCyan;
            this.rtxtReceiveMsg.Location = new System.Drawing.Point(166, 235);
            this.rtxtReceiveMsg.Name = "rtxtReceiveMsg";
            this.rtxtReceiveMsg.ReadOnly = true;
            this.rtxtReceiveMsg.Size = new System.Drawing.Size(523, 147);
            this.rtxtReceiveMsg.TabIndex = 70;
            this.rtxtReceiveMsg.Text = "";
            this.rtxtReceiveMsg.WordWrap = false;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.DarkRed;
            this.label5.Location = new System.Drawing.Point(29, 248);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(61, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = "RECIEVE";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.DarkRed;
            this.label4.Location = new System.Drawing.Point(29, 194);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(41, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "SEND";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.DarkRed;
            this.label3.Location = new System.Drawing.Point(29, 127);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(43, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "QR ID";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.DarkRed;
            this.label2.Location = new System.Drawing.Point(29, 89);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(95, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "AMOUNT (RM)";
            // 
            // lbl_portname
            // 
            this.lbl_portname.AutoSize = true;
            this.lbl_portname.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_portname.ForeColor = System.Drawing.Color.DarkRed;
            this.lbl_portname.Location = new System.Drawing.Point(510, 32);
            this.lbl_portname.Name = "lbl_portname";
            this.lbl_portname.Size = new System.Drawing.Size(0, 13);
            this.lbl_portname.TabIndex = 0;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.ForeColor = System.Drawing.Color.DarkRed;
            this.label9.Location = new System.Drawing.Point(435, 32);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(69, 13);
            this.label9.TabIndex = 0;
            this.label9.Text = "COM Port:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.DarkRed;
            this.label1.Location = new System.Drawing.Point(29, 32);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(97, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "HOST NUMBER";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Verdana", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.Color.DarkGreen;
            this.label6.Location = new System.Drawing.Point(306, -30);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(0, 29);
            this.label6.TabIndex = 82;
            this.label6.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // btnDisconnect
            // 
            this.btnDisconnect.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDisconnect.ForeColor = System.Drawing.Color.Maroon;
            this.btnDisconnect.Location = new System.Drawing.Point(467, 496);
            this.btnDisconnect.Name = "btnDisconnect";
            this.btnDisconnect.Size = new System.Drawing.Size(119, 23);
            this.btnDisconnect.TabIndex = 81;
            this.btnDisconnect.Text = "Disconnect";
            this.btnDisconnect.UseVisualStyleBackColor = true;
            this.btnDisconnect.Click += new System.EventHandler(this.btnDisconnect_Click);
            // 
            // btnSendData
            // 
            this.btnSendData.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSendData.ForeColor = System.Drawing.Color.DarkRed;
            this.btnSendData.Location = new System.Drawing.Point(178, 496);
            this.btnSendData.Name = "btnSendData";
            this.btnSendData.Size = new System.Drawing.Size(100, 23);
            this.btnSendData.TabIndex = 80;
            this.btnSendData.Text = "Send Data";
            this.btnSendData.Click += new System.EventHandler(this.btnSendData_Click);
            // 
            // frmECR
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(764, 531);
            this.Controls.Add(this.btnCancelTrans);
            this.Controls.Add(this.lblCheck);
            this.Controls.Add(this.btnAbort);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.btnDisconnect);
            this.Controls.Add(this.btnSendData);
            this.Name = "frmECR";
            this.Text = "frmECR";
            this.Load += new System.EventHandler(this.frmECR_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCancelTrans;
        private System.Windows.Forms.Label lblCheck;
        private System.Windows.Forms.Button btnAbort;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lblProgressMsg;
        public System.Windows.Forms.Label label11;
        public System.Windows.Forms.TextBox txtDocs;
        public System.Windows.Forms.Label lblDocs;
        private System.Windows.Forms.Button btnClsMsg;
        public System.Windows.Forms.TextBox txtQRNo;
        public System.Windows.Forms.Label label10;
        public System.Windows.Forms.Label lbl_status;
        public System.Windows.Forms.Label label8;
        public System.Windows.Forms.Label label7;
        public System.Windows.Forms.TextBox txtTipAmount;
        public System.Windows.Forms.TextBox txtQRId;
        public System.Windows.Forms.TextBox txtAmount;
        public System.Windows.Forms.TextBox txtHostNo;
        public System.Windows.Forms.TextBox txtSendMsg;
        public System.Windows.Forms.RichTextBox rtxtReceiveMsg;
        public System.Windows.Forms.Label label5;
        public System.Windows.Forms.Label label4;
        public System.Windows.Forms.Label label3;
        public System.Windows.Forms.Label label2;
        public System.Windows.Forms.Label lbl_portname;
        public System.Windows.Forms.Label label9;
        public System.Windows.Forms.Label label1;
        public System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnDisconnect;
        private System.Windows.Forms.Button btnSendData;
    }
}