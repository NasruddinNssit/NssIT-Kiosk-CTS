namespace NssIT.Kiosk.Admin
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.lblLatestQueryDate = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.dtpLogDate = new System.Windows.Forms.DateTimePicker();
            this.btnQueryLog = new System.Windows.Forms.Button();
            this.grdLog = new System.Windows.Forms.DataGridView();
            this.ColDateTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColMsg = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColTransNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColTimeCode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.txtMsg = new System.Windows.Forms.TextBox();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grdLog)).BeginInit();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(776, 426);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.lblLatestQueryDate);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Controls.Add(this.dtpLogDate);
            this.tabPage1.Controls.Add(this.btnQueryLog);
            this.tabPage1.Controls.Add(this.grdLog);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(768, 400);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Transaction Log";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // lblLatestQueryDate
            // 
            this.lblLatestQueryDate.AutoSize = true;
            this.lblLatestQueryDate.Location = new System.Drawing.Point(6, 50);
            this.lblLatestQueryDate.Name = "lblLatestQueryDate";
            this.lblLatestQueryDate.Size = new System.Drawing.Size(10, 13);
            this.lblLatestQueryDate.TabIndex = 4;
            this.lblLatestQueryDate.Text = "-";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(39, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Date : ";
            // 
            // dtpLogDate
            // 
            this.dtpLogDate.Location = new System.Drawing.Point(51, 17);
            this.dtpLogDate.Name = "dtpLogDate";
            this.dtpLogDate.Size = new System.Drawing.Size(205, 20);
            this.dtpLogDate.TabIndex = 2;
            // 
            // btnQueryLog
            // 
            this.btnQueryLog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnQueryLog.Location = new System.Drawing.Point(626, 18);
            this.btnQueryLog.Name = "btnQueryLog";
            this.btnQueryLog.Size = new System.Drawing.Size(133, 23);
            this.btnQueryLog.TabIndex = 1;
            this.btnQueryLog.Text = "Get Log";
            this.btnQueryLog.UseVisualStyleBackColor = true;
            this.btnQueryLog.Click += new System.EventHandler(this.btnQueryLog_Click);
            // 
            // grdLog
            // 
            this.grdLog.AllowUserToAddRows = false;
            this.grdLog.AllowUserToDeleteRows = false;
            this.grdLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grdLog.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grdLog.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColDateTime,
            this.ColMsg,
            this.ColTransNo,
            this.ColTimeCode});
            this.grdLog.Location = new System.Drawing.Point(3, 66);
            this.grdLog.Name = "grdLog";
            this.grdLog.ReadOnly = true;
            this.grdLog.Size = new System.Drawing.Size(756, 328);
            this.grdLog.TabIndex = 0;
            // 
            // ColDateTime
            // 
            this.ColDateTime.DataPropertyName = "TimeStr";
            this.ColDateTime.HeaderText = "Time";
            this.ColDateTime.Name = "ColDateTime";
            this.ColDateTime.ReadOnly = true;
            this.ColDateTime.Width = 200;
            // 
            // ColMsg
            // 
            this.ColMsg.DataPropertyName = "AdminMsg";
            this.ColMsg.HeaderText = "Message";
            this.ColMsg.Name = "ColMsg";
            this.ColMsg.ReadOnly = true;
            this.ColMsg.Width = 300;
            // 
            // ColTransNo
            // 
            this.ColTransNo.DataPropertyName = "ProcId";
            this.ColTransNo.HeaderText = "Transaction No.";
            this.ColTransNo.Name = "ColTransNo";
            this.ColTransNo.ReadOnly = true;
            this.ColTransNo.Width = 150;
            // 
            // ColTimeCode
            // 
            this.ColTimeCode.DataPropertyName = "Time";
            this.ColTimeCode.HeaderText = "Time Code";
            this.ColTimeCode.Name = "ColTimeCode";
            this.ColTimeCode.ReadOnly = true;
            this.ColTimeCode.Width = 150;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.txtMsg);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(768, 400);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Message";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // txtMsg
            // 
            this.txtMsg.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtMsg.Location = new System.Drawing.Point(6, 6);
            this.txtMsg.Multiline = true;
            this.txtMsg.Name = "txtMsg";
            this.txtMsg.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtMsg.Size = new System.Drawing.Size(756, 388);
            this.txtMsg.TabIndex = 0;
            this.txtMsg.WordWrap = false;
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.tabControl1);
            this.Name = "frmMain";
            this.Text = "Kiosk Admin";
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grdLog)).EndInit();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.DataGridView grdLog;
        private System.Windows.Forms.Button btnQueryLog;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColDateTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColMsg;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColTransNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColTimeCode;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DateTimePicker dtpLogDate;
        private System.Windows.Forms.Label lblLatestQueryDate;
        private System.Windows.Forms.TextBox txtMsg;
    }
}

