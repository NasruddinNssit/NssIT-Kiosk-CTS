
namespace CX23MotorSpinningTest
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
            this.btnEndCoinMach = new System.Windows.Forms.Button();
            this.btnReboot = new System.Windows.Forms.Button();
            this.btnStart = new System.Windows.Forms.Button();
            this.txtMsg = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btnEndCoinMach
            // 
            this.btnEndCoinMach.Location = new System.Drawing.Point(284, 12);
            this.btnEndCoinMach.Name = "btnEndCoinMach";
            this.btnEndCoinMach.Size = new System.Drawing.Size(96, 43);
            this.btnEndCoinMach.TabIndex = 48;
            this.btnEndCoinMach.Text = "End Coin Machine";
            this.btnEndCoinMach.UseVisualStyleBackColor = true;
            this.btnEndCoinMach.Click += new System.EventHandler(this.btnEndCoinMach_Click);
            // 
            // btnReboot
            // 
            this.btnReboot.Location = new System.Drawing.Point(122, 12);
            this.btnReboot.Name = "btnReboot";
            this.btnReboot.Size = new System.Drawing.Size(137, 43);
            this.btnReboot.TabIndex = 47;
            this.btnReboot.Text = "Spin Low Coin Bin     (Reboot Coin Machine)";
            this.btnReboot.UseVisualStyleBackColor = true;
            this.btnReboot.Click += new System.EventHandler(this.btnReboot_Click);
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(12, 12);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(87, 43);
            this.btnStart.TabIndex = 46;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // txtMsg
            // 
            this.txtMsg.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtMsg.Location = new System.Drawing.Point(12, 61);
            this.txtMsg.Multiline = true;
            this.txtMsg.Name = "txtMsg";
            this.txtMsg.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtMsg.Size = new System.Drawing.Size(600, 201);
            this.txtMsg.TabIndex = 55;
            this.txtMsg.WordWrap = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(617, 274);
            this.Controls.Add(this.txtMsg);
            this.Controls.Add(this.btnEndCoinMach);
            this.Controls.Add(this.btnReboot);
            this.Controls.Add(this.btnStart);
            this.Name = "Form1";
            this.Text = "CX23 Coin Machine Spinning Test";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnEndCoinMach;
        private System.Windows.Forms.Button btnReboot;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.TextBox txtMsg;
    }
}

