
namespace NssIT.Kiosk.Security.Live.AdminUI
{
    partial class FrmMain
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
            this.btnLiveExport = new System.Windows.Forms.Button();
            this.btnLiveWrite = new System.Windows.Forms.Button();
            this.btnLiveVerify = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnLiveExport
            // 
            this.btnLiveExport.Enabled = false;
            this.btnLiveExport.Location = new System.Drawing.Point(12, 217);
            this.btnLiveExport.Name = "btnLiveExport";
            this.btnLiveExport.Size = new System.Drawing.Size(447, 23);
            this.btnLiveExport.TabIndex = 5;
            this.btnLiveExport.Text = "Export Windows Setting";
            this.btnLiveExport.UseVisualStyleBackColor = true;
            // 
            // btnLiveWrite
            // 
            this.btnLiveWrite.Location = new System.Drawing.Point(12, 188);
            this.btnLiveWrite.Name = "btnLiveWrite";
            this.btnLiveWrite.Size = new System.Drawing.Size(447, 23);
            this.btnLiveWrite.TabIndex = 4;
            this.btnLiveWrite.Text = "Write Windows Setting";
            this.btnLiveWrite.UseVisualStyleBackColor = true;
            this.btnLiveWrite.Click += new System.EventHandler(this.btnLiveWrite_Click);
            // 
            // btnLiveVerify
            // 
            this.btnLiveVerify.Location = new System.Drawing.Point(12, 159);
            this.btnLiveVerify.Name = "btnLiveVerify";
            this.btnLiveVerify.Size = new System.Drawing.Size(447, 23);
            this.btnLiveVerify.TabIndex = 3;
            this.btnLiveVerify.Text = "Verify Windows Setting";
            this.btnLiveVerify.UseVisualStyleBackColor = true;
            this.btnLiveVerify.Click += new System.EventHandler(this.btnLiveVerify_Click);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 64F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(132, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(217, 97);
            this.label1.TabIndex = 7;
            this.label1.Text = "CTS";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 32F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Red;
            this.label2.Location = new System.Drawing.Point(12, 96);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(447, 51);
            this.label2.TabIndex = 8;
            this.label2.Text = "Melaka Sentral - Live";
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(492, 266);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnLiveExport);
            this.Controls.Add(this.btnLiveWrite);
            this.Controls.Add(this.btnLiveVerify);
            this.Name = "FrmMain";
            this.Text = "CTS - Live Version - Security Admin";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnLiveExport;
        private System.Windows.Forms.Button btnLiveWrite;
        private System.Windows.Forms.Button btnLiveVerify;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
    }
}

