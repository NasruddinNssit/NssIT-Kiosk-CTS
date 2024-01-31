namespace LibShowMessageWindow
{
	partial class WFmMessageWindow
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.txtMsg = new System.Windows.Forms.TextBox();
            this.btnClearMessage = new System.Windows.Forms.Button();
            this.btnCopyToClipboard = new System.Windows.Forms.Button();
            this.chkShowLatestMessageOnTop = new System.Windows.Forms.CheckBox();
            this.chkTopMost = new System.Windows.Forms.CheckBox();
            this.rbtMicrosoftSansSerif = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rbtCourierNew = new System.Windows.Forms.RadioButton();
            this.grpMsgSize = new System.Windows.Forms.GroupBox();
            this.rbtMaxMsgSize8 = new System.Windows.Forms.RadioButton();
            this.radioButton7 = new System.Windows.Forms.RadioButton();
            this.radioButton6 = new System.Windows.Forms.RadioButton();
            this.radioButton5 = new System.Windows.Forms.RadioButton();
            this.radioButton4 = new System.Windows.Forms.RadioButton();
            this.radioButton3 = new System.Windows.Forms.RadioButton();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.rbtMaxMsgSize0 = new System.Windows.Forms.RadioButton();
            this.groupBox1.SuspendLayout();
            this.grpMsgSize.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtMsg
            // 
            this.txtMsg.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtMsg.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtMsg.Location = new System.Drawing.Point(12, 12);
            this.txtMsg.Multiline = true;
            this.txtMsg.Name = "txtMsg";
            this.txtMsg.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtMsg.Size = new System.Drawing.Size(719, 329);
            this.txtMsg.TabIndex = 0;
            this.txtMsg.WordWrap = false;
            // 
            // btnClearMessage
            // 
            this.btnClearMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClearMessage.Location = new System.Drawing.Point(615, 392);
            this.btnClearMessage.Name = "btnClearMessage";
            this.btnClearMessage.Size = new System.Drawing.Size(116, 23);
            this.btnClearMessage.TabIndex = 1;
            this.btnClearMessage.Text = "Clear Message";
            this.btnClearMessage.UseVisualStyleBackColor = true;
            this.btnClearMessage.Click += new System.EventHandler(this.btnClearMessage_Click);
            // 
            // btnCopyToClipboard
            // 
            this.btnCopyToClipboard.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCopyToClipboard.Location = new System.Drawing.Point(615, 363);
            this.btnCopyToClipboard.Name = "btnCopyToClipboard";
            this.btnCopyToClipboard.Size = new System.Drawing.Size(116, 23);
            this.btnCopyToClipboard.TabIndex = 2;
            this.btnCopyToClipboard.Text = "Copy To Clipboard";
            this.btnCopyToClipboard.UseVisualStyleBackColor = true;
            this.btnCopyToClipboard.Click += new System.EventHandler(this.btnCopyToClipboard_Click);
            // 
            // chkShowLatestMessageOnTop
            // 
            this.chkShowLatestMessageOnTop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkShowLatestMessageOnTop.AutoSize = true;
            this.chkShowLatestMessageOnTop.Location = new System.Drawing.Point(12, 347);
            this.chkShowLatestMessageOnTop.Name = "chkShowLatestMessageOnTop";
            this.chkShowLatestMessageOnTop.Size = new System.Drawing.Size(170, 17);
            this.chkShowLatestMessageOnTop.TabIndex = 3;
            this.chkShowLatestMessageOnTop.Text = "Show Latest Message On Top";
            this.chkShowLatestMessageOnTop.UseVisualStyleBackColor = true;
            // 
            // chkTopMost
            // 
            this.chkTopMost.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkTopMost.AutoSize = true;
            this.chkTopMost.Checked = true;
            this.chkTopMost.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkTopMost.Location = new System.Drawing.Point(12, 370);
            this.chkTopMost.Name = "chkTopMost";
            this.chkTopMost.Size = new System.Drawing.Size(127, 17);
            this.chkTopMost.TabIndex = 4;
            this.chkTopMost.Text = "This form is Top Most";
            this.chkTopMost.UseVisualStyleBackColor = true;
            this.chkTopMost.CheckedChanged += new System.EventHandler(this.chkTopMost_CheckedChanged);
            // 
            // rbtMicrosoftSansSerif
            // 
            this.rbtMicrosoftSansSerif.AutoSize = true;
            this.rbtMicrosoftSansSerif.Checked = true;
            this.rbtMicrosoftSansSerif.Location = new System.Drawing.Point(6, 22);
            this.rbtMicrosoftSansSerif.Name = "rbtMicrosoftSansSerif";
            this.rbtMicrosoftSansSerif.Size = new System.Drawing.Size(119, 17);
            this.rbtMicrosoftSansSerif.TabIndex = 5;
            this.rbtMicrosoftSansSerif.TabStop = true;
            this.rbtMicrosoftSansSerif.Text = "Microsoft Sans Serif";
            this.rbtMicrosoftSansSerif.UseVisualStyleBackColor = true;
            this.rbtMicrosoftSansSerif.CheckedChanged += new System.EventHandler(this.FontChanged_Event);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox1.Controls.Add(this.rbtCourierNew);
            this.groupBox1.Controls.Add(this.rbtMicrosoftSansSerif);
            this.groupBox1.Location = new System.Drawing.Point(455, 347);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(154, 76);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Font";
            // 
            // rbtCourierNew
            // 
            this.rbtCourierNew.AutoSize = true;
            this.rbtCourierNew.Location = new System.Drawing.Point(6, 48);
            this.rbtCourierNew.Name = "rbtCourierNew";
            this.rbtCourierNew.Size = new System.Drawing.Size(83, 17);
            this.rbtCourierNew.TabIndex = 6;
            this.rbtCourierNew.TabStop = true;
            this.rbtCourierNew.Text = "Courier New";
            this.rbtCourierNew.UseVisualStyleBackColor = true;
            this.rbtCourierNew.CheckedChanged += new System.EventHandler(this.FontChanged_Event);
            // 
            // grpMsgSize
            // 
            this.grpMsgSize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.grpMsgSize.Controls.Add(this.rbtMaxMsgSize8);
            this.grpMsgSize.Controls.Add(this.radioButton7);
            this.grpMsgSize.Controls.Add(this.radioButton6);
            this.grpMsgSize.Controls.Add(this.radioButton5);
            this.grpMsgSize.Controls.Add(this.radioButton4);
            this.grpMsgSize.Controls.Add(this.radioButton3);
            this.grpMsgSize.Controls.Add(this.radioButton1);
            this.grpMsgSize.Controls.Add(this.rbtMaxMsgSize0);
            this.grpMsgSize.Location = new System.Drawing.Point(188, 347);
            this.grpMsgSize.Name = "grpMsgSize";
            this.grpMsgSize.Size = new System.Drawing.Size(261, 76);
            this.grpMsgSize.TabIndex = 7;
            this.grpMsgSize.TabStop = false;
            this.grpMsgSize.Text = "Max Message Size";
            // 
            // rbtMaxMsgSize8
            // 
            this.rbtMaxMsgSize8.AutoSize = true;
            this.rbtMaxMsgSize8.Checked = true;
            this.rbtMaxMsgSize8.Location = new System.Drawing.Point(191, 48);
            this.rbtMaxMsgSize8.Name = "rbtMaxMsgSize8";
            this.rbtMaxMsgSize8.Size = new System.Drawing.Size(67, 17);
            this.rbtMaxMsgSize8.TabIndex = 12;
            this.rbtMaxMsgSize8.TabStop = true;
            this.rbtMaxMsgSize8.Text = "1000000";
            this.rbtMaxMsgSize8.UseVisualStyleBackColor = true;
            this.rbtMaxMsgSize8.CheckedChanged += new System.EventHandler(this.rbtMaxMsgSize_CheckedChanged);
            // 
            // radioButton7
            // 
            this.radioButton7.AutoSize = true;
            this.radioButton7.Location = new System.Drawing.Point(191, 22);
            this.radioButton7.Name = "radioButton7";
            this.radioButton7.Size = new System.Drawing.Size(61, 17);
            this.radioButton7.TabIndex = 11;
            this.radioButton7.Text = "600000";
            this.radioButton7.UseVisualStyleBackColor = true;
            this.radioButton7.CheckedChanged += new System.EventHandler(this.rbtMaxMsgSize_CheckedChanged);
            // 
            // radioButton6
            // 
            this.radioButton6.AutoSize = true;
            this.radioButton6.Location = new System.Drawing.Point(127, 48);
            this.radioButton6.Name = "radioButton6";
            this.radioButton6.Size = new System.Drawing.Size(61, 17);
            this.radioButton6.TabIndex = 10;
            this.radioButton6.Text = "300000";
            this.radioButton6.UseVisualStyleBackColor = true;
            this.radioButton6.CheckedChanged += new System.EventHandler(this.rbtMaxMsgSize_CheckedChanged);
            // 
            // radioButton5
            // 
            this.radioButton5.AutoSize = true;
            this.radioButton5.Location = new System.Drawing.Point(127, 22);
            this.radioButton5.Name = "radioButton5";
            this.radioButton5.Size = new System.Drawing.Size(61, 17);
            this.radioButton5.TabIndex = 9;
            this.radioButton5.Text = "150000";
            this.radioButton5.UseVisualStyleBackColor = true;
            this.radioButton5.CheckedChanged += new System.EventHandler(this.rbtMaxMsgSize_CheckedChanged);
            // 
            // radioButton4
            // 
            this.radioButton4.AutoSize = true;
            this.radioButton4.Location = new System.Drawing.Point(67, 48);
            this.radioButton4.Name = "radioButton4";
            this.radioButton4.Size = new System.Drawing.Size(55, 17);
            this.radioButton4.TabIndex = 8;
            this.radioButton4.Text = "90000";
            this.radioButton4.UseVisualStyleBackColor = true;
            this.radioButton4.CheckedChanged += new System.EventHandler(this.rbtMaxMsgSize_CheckedChanged);
            // 
            // radioButton3
            // 
            this.radioButton3.AutoSize = true;
            this.radioButton3.Location = new System.Drawing.Point(67, 22);
            this.radioButton3.Name = "radioButton3";
            this.radioButton3.Size = new System.Drawing.Size(55, 17);
            this.radioButton3.TabIndex = 7;
            this.radioButton3.Text = "60000";
            this.radioButton3.UseVisualStyleBackColor = true;
            this.radioButton3.CheckedChanged += new System.EventHandler(this.rbtMaxMsgSize_CheckedChanged);
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Location = new System.Drawing.Point(6, 48);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(55, 17);
            this.radioButton1.TabIndex = 6;
            this.radioButton1.Text = "30000";
            this.radioButton1.UseVisualStyleBackColor = true;
            this.radioButton1.CheckedChanged += new System.EventHandler(this.rbtMaxMsgSize_CheckedChanged);
            // 
            // rbtMaxMsgSize0
            // 
            this.rbtMaxMsgSize0.AutoSize = true;
            this.rbtMaxMsgSize0.Location = new System.Drawing.Point(6, 22);
            this.rbtMaxMsgSize0.Name = "rbtMaxMsgSize0";
            this.rbtMaxMsgSize0.Size = new System.Drawing.Size(55, 17);
            this.rbtMaxMsgSize0.TabIndex = 5;
            this.rbtMaxMsgSize0.Text = "15000";
            this.rbtMaxMsgSize0.UseVisualStyleBackColor = true;
            this.rbtMaxMsgSize0.CheckedChanged += new System.EventHandler(this.rbtMaxMsgSize_CheckedChanged);
            // 
            // WFmMessageWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(743, 427);
            this.Controls.Add(this.grpMsgSize);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.chkTopMost);
            this.Controls.Add(this.chkShowLatestMessageOnTop);
            this.Controls.Add(this.btnCopyToClipboard);
            this.Controls.Add(this.btnClearMessage);
            this.Controls.Add(this.txtMsg);
            this.MinimumSize = new System.Drawing.Size(460, 240);
            this.Name = "WFmMessageWindow";
            this.ShowIcon = false;
            this.Text = "Message Window";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.WFmMessageWindow_FormClosing);
            this.Load += new System.EventHandler(this.WFmMessageWindow_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.grpMsgSize.ResumeLayout(false);
            this.grpMsgSize.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox txtMsg;
		private System.Windows.Forms.Button btnClearMessage;
		private System.Windows.Forms.Button btnCopyToClipboard;
		private System.Windows.Forms.CheckBox chkShowLatestMessageOnTop;
        private System.Windows.Forms.CheckBox chkTopMost;
        private System.Windows.Forms.RadioButton rbtMicrosoftSansSerif;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton rbtCourierNew;
        private System.Windows.Forms.GroupBox grpMsgSize;
        private System.Windows.Forms.RadioButton rbtMaxMsgSize8;
        private System.Windows.Forms.RadioButton radioButton7;
        private System.Windows.Forms.RadioButton radioButton6;
        private System.Windows.Forms.RadioButton radioButton5;
        private System.Windows.Forms.RadioButton radioButton4;
        private System.Windows.Forms.RadioButton radioButton3;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.RadioButton rbtMaxMsgSize0;
    }
}